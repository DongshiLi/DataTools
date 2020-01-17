import pymysql
import pandas as pd
from datetime import datetime
import os
import logging
from dateutil.relativedelta import relativedelta


DB_INFO = dict(host='192.168.3.180',
               user='root',
               password='518052',
               db='wind')

conn = pymysql.connect(** DB_INFO, charset='utf8mb4', cursorclass=pymysql.cursors.DictCursor)


def check_dir(dir_path):
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)
        return False
    return True


def query_wind_table(table_name: str, start_date=None, end_date=None, ref_date_field='TRADE_DT', fields=None,
                     intra_day=False):
    date_check: str = ""
    if start_date is not None:
        today = datetime.today().strftime('%Y%m%d')
        if today == str(start_date):
            if not intra_day:
                time = int(datetime.now().time().strftime('%H%M'))
                if time < 1500:
                    return pd.DataFrame()
            else:
                date_check = f"""WHERE {ref_date_field} = {start_date} """

        else:
            date_check = f"""WHERE {ref_date_field} >= {start_date} """
            end_date = int(today) if end_date is None else int(end_date)
            if not intra_day:
                time = int(datetime.now().time().strftime('%H%M'))
                if time < 1500 and end_date == int(today):
                    date_check += f"""AND {ref_date_field} < {end_date}"""
    else:
        date_check: str = ""
    if type(fields) in [dict, list]:
        keys = fields if type(fields) is list else list(fields.keys())
    elif fields is None:
        keys = "*"
    else:
        raise Exception("rename_dict type is not supported")
    # if there are some special sql for certain table, add more scenario
    if table_name == 'ASHAREINDUSTRIESCLASSCITICS':
        sql = \
            "SELECT a.S_INFO_WINDCODE,a.ENTRY_DT, b.INDUSTRIESNAME, b.LEVELNUM " + \
            " FROM ASHAREINDUSTRIESCLASSCITICS a, ASHAREINDUSTRIESCODE b " + \
            " WHERE (substr(a.CITICS_IND_CODE, 1, 4) = substr(b.INDUSTRIESCODE, 1, 4) " + \
            " AND (b.LEVELNUM = '2')) OR (substr(a.CITICS_IND_CODE, 1, 6) = substr(b.INDUSTRIESCODE, 1, 6) " + \
            " AND (b.LEVELNUM = '3'))"
    else:
        sql = "SELECT " + ', '.join(keys) + " FROM  " + table_name + " " + date_check
    df = pd.read_sql_query(sql, conn)
    con = df[ref_date_field].apply(lambda x: x is not None)
    df = df.loc[con]
    df.sort_values(by=[ref_date_field], ascending=True, inplace=True)
    if type(fields) is dict:
        df.rename(columns=fields, inplace=True)
        if 'sid' in fields.values():
            df = df.loc[df.sid.apply(lambda x: (x[::8] in ['6H', '0Z', '3Z']) and (len(x) == 9))]
    df.DataDate = df.DataDate.apply(int)
    return df


def set_logger(file=None, level=logging.DEBUG, mode='a', name=""):
    """
    written by Sha
    :return:
    """
    formatter = logging.Formatter(fmt='[%(asctime)s] %(levelname)s: %(module)s: %(message)s')

    handler = logging.StreamHandler()
    handler.setFormatter(formatter)

    logger = logging.getLogger(name)
    logger.setLevel(level)
    logger.addHandler(handler)

    if file is not None:
        handler_file = logging.FileHandler(filename=file, encoding='utf-8', mode=mode)
        handler_file.setFormatter(formatter)
        logger.addHandler(handler_file)

    return logger


def split_date(start_date):
    """
    :param start_date: could be none
    :return: [(s, e), (s, e), (s, None)]
    """
    if start_date is None:
        return [(None, None)]
    date = datetime.strptime(str(start_date), '%Y%m%d')
    today = datetime.today()
    periods = []
    _d = date
    for i in range(100):
        _d = date + relativedelta(years=i)
        _d_1 = date + relativedelta(years=i+1)
        if _d_1.date() < today.date():
            periods.append((int(_d.strftime('%Y%m%d')), int(_d_1.strftime('%Y%m%d'))))
        else:
            periods.append((int(_d.strftime('%Y%m%d')), None))
            break
    return periods


if __name__ == '__main__':
    # print([i for i in split_date(20060101)])
    # print([i for i in split_date(20181228)])
    wind_logger = set_logger()
    wind_logger.info("This is a test")
    wind_logger.debug("This is a test")
