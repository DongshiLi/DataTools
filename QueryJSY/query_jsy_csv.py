import os
import pandas as pd
from datetime import datetime
from data_tools.api import load_yaml
from data_tools.QueryJSY.jsy_base import query_jsy_csv, reverse_copy_industry_class, refresh_industry_class, \
    check_dir, set_logger, split_date
from data_tools.BcolzBase.base import BcolzBase
from data_tools.api import query_calendar
from data_tools.env import BCOLZ_ROOT, LANDING_BCOLZ


check_dir(BCOLZ_ROOT)


def unit_query(key, config, logger):
    from data_tools.api import get_next_trade_date
    logger.info("------ {:^30} ------".format(key))
    logger.info(f'Start updating table {key}...')
    dir_path_bcolz = f'{BCOLZ_ROOT}/{key}.bcolz'
    if config['mode'] == 'r' and os.path.exists(dir_path_bcolz):
        logger.info(f'Remove table {key}...')
        for root, dirs, files in os.walk(dir_path_bcolz, topdown=False):
            for name in files:
                os.remove(os.path.join(root, name))
            for name in dirs:
                os.rmdir(os.path.join(root, name))
        os.rmdir(dir_path_bcolz)
    db_bcolz = BcolzBase(root_dir=os.path.join(BCOLZ_ROOT, f'{key}.bcolz'))
    if config['mode'] == 'r':
        logger.info(f"Start querying table {key}...")
        if key == "ASHARE_CALENDAR":
            df_temp = query_calendar()
            df_temp.rename(columns=config["fields"], inplace=True)
            df_list = [df_temp]
        else:
            df_list = query_jsy_csv(
                key,
                start_date=config['start_date'],
                end_date=int(datetime.today().strftime("%Y%m%d")),
                ref_date_field=config['ref_date_field'],
                fields=config['fields'], db=config["db"], logger=logger)
        if LANDING_BCOLZ:
            logger.info(f"Start writing {key}.bcolz")
            if df_list is not None and len(df_list) != 0:
                for df in df_list:
                    if len(df) > 0:
                        db_bcolz.write(df)
                    del df
            logger.info(f"{key} Done!")
            del df_list, db_bcolz
        return

    last_update_date = config['start_date'] if not LANDING_BCOLZ or db_bcolz.last_update_date is None \
        else db_bcolz.last_update_date
    if str(last_update_date) == datetime.today().strftime('%Y%m%d'):
        logger.info(f"{key} is already up-to-date!")
        return

    start_date = get_next_trade_date(last_update_date)
    if key == "ASHARE_EOD_PRICES" or key == "ASHARE_DESCRIPTION" or key == "ASHARE_INDUSTRIES_CLASS" \
            or key == "AINDEX_HS300_FREE_WEIGHT" or key == "ASHARE_EOD_DERIVATIVE_INDICATOR"\
            or key == "ASHARE_MONEY_FLOW":
        periods = [(start_date, int(datetime.today().strftime("%Y%m%d")))]
    else:
        periods = split_date(start_date)
    for s, e in periods:
        logger.info(f"Start querying table {key} from {s} to {e}")
        df_list = query_jsy_csv(key, start_date=s, end_date=e, ref_date_field=config['ref_date_field'],
                                fields=config['fields'], intra_day=False, db=config['db'], logger=logger)
        if LANDING_BCOLZ:
            if df_list is not None and len(df_list) != 0:
                logger.info(f"Start writing table {key}.bcolz from {s} to {e}")
                for df in df_list:
                    if type(df) is pd.DataFrame and len(df) > 0:
                        db_bcolz.write(df)
                    del df
            else:
                logger.info(f"No data for table {key} from {s} to {e}")
    logger.info(f"{key} Done!")
    del db_bcolz
    return


def query_jsy_tables():
    configs = load_yaml()
    # set log file
    today = datetime.today().strftime('%Y%m%d')
    file = f"{BCOLZ_ROOT}/log/{today}"
    check_dir(file)
    file_id = len(os.listdir(file))
    file += f"/{file_id}.txt"
    logger = set_logger(file)
    # update ASHARE_CALENDAR FIRST:
    config = configs.pop('ASHARE_CALENDAR')
    unit_query('ASHARE_CALENDAR', config, logger)
    for key, config in configs.items():
        unit_query(key, config, logger)
    return logger


if __name__ == "__main__":
    logger_temp = query_jsy_tables()
    reverse_copy_industry_class(logger_temp)
    refresh_industry_class(logger_temp)
