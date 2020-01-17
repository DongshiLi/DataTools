import yaml
import os
from datetime import datetime
from data_tools.QueryWind.base import query_wind_table, check_dir, set_logger, split_date
from data_tools.BcolzBase.base import BcolzBase
from data_tools.env import BCOLZ_ROOT

check_dir(BCOLZ_ROOT)


def unit_query(key, config, logger):
    from data_tools.api import get_next_trade_date
    logger.info("------ {:^30} ------".format(key))
    logger.info(f'Start updating table...')

    if config['mode'] == 'r' and os.path.exists(f"{BCOLZ_ROOT}/{key}.bcolz"):
        logger.info(f'Remove table {key}...')
        if not os.system(f'rm -r {BCOLZ_ROOT}/{key}.bcolz'):
            logger.info(f"Remove table {key} successfully!")
        else:
            raise Exception(f"The mode of {key} is rewrite, but the old table didn't "
                            f"be removed successfully!")
    db_bcolz = BcolzBase(root_dir=os.path.join(BCOLZ_ROOT, f'{key}.bcolz'))

    if config['mode'] == 'r':
        logger.info("Start querying")
        df = query_wind_table(key, ref_date_field=config['ref_date_field'], fields=config['fields'])
        logger.info("Start writing bcolz")
        db_bcolz.write(df)
        logger.info(f"Done!")
        del df, db_bcolz
        return

    last_update_date = config['start_date'] if db_bcolz.last_update_date is None \
        else db_bcolz.last_update_date
    if str(last_update_date) == datetime.today().strftime('%Y%m%d'):
        logger.info(f"{key} is already up-to-date!")
        return

    start_date = get_next_trade_date(last_update_date)
    periods = split_date(start_date)
    for s, e in periods:
        logger.info(f"Start querying from {s} to {e}")
        df = query_wind_table(key, start_date=s, end_date=e,
                              ref_date_field=config['ref_date_field'],
                              fields=config['fields'], intra_day=False)
        if len(df) != 0:
            logger.info(f"Start writing bcolz")
            db_bcolz.write(df)
            logger.info(f"Done!")
            del df
        else:
            logger.info(f"No data for {key} from {s} to {e}")
    del db_bcolz
    return


def query_wind_tables():
    configs = load_yaml()
    # set log file
    today = datetime.today().strftime('%Y%m%d')
    file = f"{BCOLZ_ROOT}/log/{today}"
    check_dir(file)
    file_id = len(os.listdir(file))
    file += f"/{file_id}.txt"
    logger = set_logger(file)
    # update ASHARECALENDAR FIRST:
    config = configs.pop('ASHARECALENDAR')
    unit_query('ASHARECALENDAR', config, logger)
    for key, config in configs.items():
        unit_query(key, config, logger)
    return


if __name__ == "__main__":
    query_wind_tables()
