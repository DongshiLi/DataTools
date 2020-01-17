import os
import yaml
import pandas as pd
from sqlalchemy import create_engine
from collections import OrderedDict
from data_tools.env import BCOLZ_ROOT, MYSQL_HOST, MYSQL_USER, MYSQL_PASSWORD, MYSQL_CHARSET
from data_tools.BcolzBase.base import BcolzBase


def load_yaml():
    with open('table_config.yml', 'r') as file:
        config = yaml.load(file, Loader=yaml.FullLoader)
    return config


def query_calendar():
    conn = create_engine(
        f'mysql+mysqlconnector://{MYSQL_USER}:{MYSQL_PASSWORD}@{MYSQL_HOST}:3306/jsy?charset={MYSQL_CHARSET}')
    sql = "SELECT * FROM ashare_calendar"
    calendar_df = pd.read_sql_query(sql, conn)
    calendar_df = calendar_df.rename(columns={'TRADE_DAYS': 'DataDate'}, inplace=False)
    return calendar_df
    

if not os.path.exists(f"{BCOLZ_ROOT}/ASHARE_CALENDAR.bcolz"):
    df = query_calendar()
    trade_days = [str(i) for i in sorted(df.DataDate.unique())]
else:
    db = BcolzBase(f"{BCOLZ_ROOT}/ASHARE_CALENDAR.bcolz", mode='r')
    trade_days = list(db.index.keys())


def get_next_trade_date(date):
    date = str(date)
    return int(next(x for x in trade_days if x > date))


def get_previous_trade_date(date):
    date = str(date)
    return int(next(x for x in trade_days[::-1] if x < date))


def get_trade_dates(start, end=0):
    if end > 0:
        return [int(x) for x in trade_days if str(start) <= x <= str(end)]
    else:
        return [int(x) for x in trade_days if str(start) <= x]


def is_trade_date(date):
    return str(date) in trade_days


def query_table(table_name, start_date, end_date=None, fields=None, subtable=None):
    if not os.path.exists(f"{BCOLZ_ROOT}/{table_name}.bcolz"):
        raise FileExistsError(f"{BCOLZ_ROOT}/{table_name}.bcolz dose not exists, please double check it!")
    temp_db = BcolzBase(f"{BCOLZ_ROOT}/{table_name}.bcolz", mode='r')
    if subtable is not None:
        config = load_yaml()
        subtable = [subtable] if isinstance(subtable, list) else subtable
        _field = []
        for title in subtable:
            _field.extend(list(config[title]['fields'].values()))
        fields = list(fields) if not isinstance(fields, list) else fields
        fields.extend(list(OrderedDict.fromkeys(_field)))
    return temp_db.query_table(start_date, end_date, fields)


if __name__ == "__main__":
    print(type(get_next_trade_date(20190614)))
    print(get_previous_trade_date(20190614))
    print(get_trade_dates(20190611, 20190614))
