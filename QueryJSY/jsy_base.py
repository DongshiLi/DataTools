from datetime import datetime
from dateutil.relativedelta import relativedelta
from sqlalchemy import create_engine
from data_tools.env import JSY_CSV_ROOT, DEBUG_MODE, FINANCIAL_MODE,\
    MYSQL_HOST, MYSQL_USER, MYSQL_PASSWORD, MYSQL_CHARSET
from data_tools.api import get_trade_dates
import pandas as pd
import numpy as np
import os
import logging
import yaml
import rqdatac as rq
from rqdatac import *
#import tushare as ts
# from WindPy import *


DATA_DIR = JSY_CSV_ROOT if JSY_CSV_ROOT.endswith(os.path.sep) else JSY_CSV_ROOT + os.path.sep
jsy_conn = create_engine(
    f'mysql+mysqlconnector://{MYSQL_USER}:{MYSQL_PASSWORD}@{MYSQL_HOST}:3306/jsy?charset={MYSQL_CHARSET}')
jsy_bar_conn = create_engine(
    f'mysql+mysqlconnector://{MYSQL_USER}:{MYSQL_PASSWORD}@{MYSQL_HOST}:3306/jsy_bar?charset={MYSQL_CHARSET}')
jsy_ticker_conn = create_engine(
    f'mysql+mysqlconnector://{MYSQL_USER}:{MYSQL_PASSWORD}@{MYSQL_HOST}:3306/jsy_ticker?charset={MYSQL_CHARSET}')


def check_dir(dir_path):
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)
        return False
    return True


def __process_eod_prices(df, ref_date_field='TRADE_DT'):
    df[ref_date_field] = df['时间'].replace("[-]", "", regex=True).apply(int)
    con = df[ref_date_field].apply(lambda x: x is not None)
    df = df.loc[con]
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["代码"].replace("[SH|SZ]", "", regex=True)
    new_df["S_INFO_WINDCODE"] += "." + df["代码"].replace("[0123456789]", "", regex=True)
    new_df[ref_date_field] = df[ref_date_field]
    new_df["S_DQ_OPEN"] = (df["开盘价"] / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_HIGH"] = (df["最高价"] / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_LOW"] = (df["最低价"] / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_CLOSE"] = (df["收盘价"] / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_VOLUME"] = (df["成交量(股)"] / df["复权系数"]).apply(lambda x: round(x))
    new_df["S_DQ_AMOUNT"] = df["成交额(元)"]
    new_df["S_DQ_ADJFACTOR"] = df["复权系数"]
    pre_close = list(df["收盘价"].apply(lambda x: round(x, 2)))
    pre_close.insert(0, 0)
    pre_close.pop()
    new_df['S_DQ_PRECLOSE'] = (pd.Series(pre_close) / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_ADJPRECLOSE"] = pre_close
    new_df["S_DQ_ADJPRECLOSE"] = new_df["S_DQ_ADJPRECLOSE"].apply(lambda x: round(x, 2))
    new_df["S_DQ_ADJOPEN"] = df["开盘价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_ADJHIGH"] = df["最高价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_ADJLOW"] = df["最低价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_ADJCLOSE"] = df["收盘价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_AVGPRICE"] = (df["成交额(元)"] / df["成交量(股)"] / df["复权系数"]).apply(
        lambda x: 0 if x is None or x == float("inf") or x == float("-inf") else round(x, 2))
    new_df["S_DQ_CHANGE"] = (new_df["S_DQ_CLOSE"] - new_df["S_DQ_PRECLOSE"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_PCTCHANGE"] = (new_df["S_DQ_CHANGE"] / pre_close).apply(
        lambda x: 0 if x is None or x == float("inf") or x == float("-inf") else round(x, 4))
    new_df["S_DQ_TURNOVER_RATE"] = df["流通盘换手率"].apply(lambda x: round(x, 4))
    new_df["S_DQ_FULL_TURNOVER_RATE"] = df["全流通换手率"].apply(lambda x: round(x, 4))
    new_df["S_DQ_TRADESTATUS"] = 1
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_index_prices(df, ref_date_field='TRADE_DT'):
    df[ref_date_field] = df['时间'].replace("[-]", "", regex=True).apply(int)
    con = df[ref_date_field].apply(lambda x: x is not None)
    df = df.loc[con]
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["代码"].replace("[SH|SZ]", "", regex=True)
    new_df["S_INFO_WINDCODE"] += "." + df["代码"].replace("[0123456789]", "", regex=True)
    new_df[ref_date_field] = df[ref_date_field]
    new_df["S_DQ_OPEN"] = df["开盘价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_HIGH"] = df["最高价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_LOW"] = df["最低价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_CLOSE"] = df["收盘价"].apply(lambda x: round(x, 2))
    pre_close = list(df["收盘价"].apply(lambda x: round(x, 2)))
    pre_close.insert(0, 0)
    pre_close.pop()
    new_df['S_DQ_PRECLOSE'] = pd.Series(pre_close).apply(lambda x: round(x, 2))
    new_df["S_DQ_VOLUME"] = df["成交量(股)"].apply(lambda x: round(x))
    new_df["S_DQ_AMOUNT"] = df["成交额(元)"]
    new_df["S_DQ_CHANGE"] = (new_df["S_DQ_CLOSE"] - new_df["S_DQ_PRECLOSE"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_PCTCHANGE"] = (new_df["S_DQ_CHANGE"] / pre_close).apply(
        lambda x: 0 if x is None or x == float("inf") or x == float("-inf") else round(x, 4))
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_financial_indicator_wind(df):
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["S_INFO_WINDCODE"]
    new_df["TRADE_DT"] = df["TRADE_DT"]
    new_df["S_VAL_TOTAL_MV"] = df["MKT_CAP_FLOAT"]
    new_df["S_VAL_CIRCLE_MV"] = df["MKT_CAP_CSRC"]
    new_df["S_VAL_PE"] = df["PE"]
    new_df["S_VAL_PE_TTM"] = df["PE_TTM"]
    new_df["S_VAL_PB_NEW"] = df["PB"]
    new_df["S_VAL_PCF_OCF"] = df["PCF_OCF"]
    new_df["S_VAL_PCF_OCFTTM"] = df["PCF_OCF_TTM"]
    new_df["S_VAL_PCF_NCF"] = df["PCF_NCF"]
    new_df["S_VAL_PCF_NCFTTM"] = df["PCF_NCF_TTM"]
    new_df["S_VAL_PS"] = df["PS"]
    new_df["S_VAL_PS_TTM"] = df["PS_TTM"]
    new_df["S_VAL_TURNOVER_RATE"] = df["FREE_TURN"]
    new_df["S_VAL_PRICE_DIV_DPS"] = df["DIVIDENDYIELD"]
    new_df["NET_PROFIT_PARENT_COMP_TTM"] = df["NETPROFIT_TTM"]
    new_df["NET_PROFIT_PARENT_COMP_LYR"] = [0] * len(df)
    new_df["NET_CASH_FLOWS_OPER_ACT_TTM"] = df["OPERATECASHFLOW_TTM"]
    new_df["NET_CASH_FLOWS_OPER_ACT_LYR"] = [0] * len(df)
    new_df["OPER_REV"] = df["OPER_REV"]
    new_df["OPER_REV_LYR"] = [0] * len(df)
    new_df["NET_INCR_CASH_CASH_EQU_TTM"] = df["NET_INCR_CASH_CASH_EQU_DM"]
    new_df["NET_INCR_CASH_CASH_EQU_LYR"] = [0] * len(df)
    new_df["S_VAL_TOTAL_SHARES"] = df["MATOTALSHARES"]
    new_df["S_VAL_FLOAT_SHARES"] = df["FLOAT_A_SHARES"]
    new_df["S_VAL_FREE_FLOAT_SHARES"] = df["FREE_FLOAT_SHARES"]
    new_df["S_VAL_NET_ASSETS"] = [0] * len(df)
    new_df["UP_DOWN_LIMIT_STATUS"] = [0] * len(df)
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_financial_indicator_ts(df):
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["ts_code"]
    new_df["TRADE_DT"] = df["trade_date"]
    new_df["S_VAL_TOTAL_MV"] = df["total_mv"] * ([10000] * len(df))
    new_df["S_VAL_CIRCLE_MV"] = df["circ_mv"] * ([10000] * len(df))
    new_df["S_VAL_PE"] = df["pe"]
    new_df["S_VAL_PE_TTM"] = df["pe_ttm"]
    new_df["S_VAL_PB_NEW"] = df["pb"]
    new_df["S_VAL_PCF_OCF"] = [0] * len(df)
    new_df["S_VAL_PCF_OCFTTM"] = [0] * len(df)
    new_df["S_VAL_PCF_NCF"] = [0] * len(df)
    new_df["S_VAL_PCF_NCFTTM"] = [0] * len(df)
    new_df["S_VAL_PS"] = df["ps"]
    new_df["S_VAL_PS_TTM"] = df["ps_ttm"]
    new_df["S_VAL_TURNOVER_RATE"] = df["turnover_rate"]
    new_df["S_VAL_VOLUME_RATIO"] = df["volume_ratio"]
    new_df["S_VAL_PRICE_DIV_DPS"] = [0] * len(df)
    new_df["NET_PROFIT_PARENT_COMP_TTM"] = [0] * len(df)
    new_df["NET_PROFIT_PARENT_COMP_LYR"] = [0] * len(df)
    new_df["NET_CASH_FLOWS_OPER_ACT_TTM"] = [0] * len(df)
    new_df["NET_CASH_FLOWS_OPER_ACT_LYR"] = [0] * len(df)
    new_df["OPER_REV"] = [0] * len(df)
    new_df["OPER_REV_LYR"] = [0] * len(df)
    new_df["NET_INCR_CASH_CASH_EQU_TTM"] = [0] * len(df)
    new_df["NET_INCR_CASH_CASH_EQU_LYR"] = [0] * len(df)
    new_df["S_VAL_TOTAL_SHARES"] = df["total_share"] * ([10000] * len(df))
    new_df["S_VAL_FLOAT_SHARES"] = df["float_share"] * ([10000] * len(df))
    new_df["S_VAL_FREE_FLOAT_SHARES"] = df["free_share"] * ([10000] * len(df))
    new_df["S_VAL_NET_ASSETS"] = df["pb"] * df["total_mv"] * ([10000] * len(df))
    new_df["UP_DOWN_LIMIT_STATUS"] = [0] * len(df)
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_zjlx(df, ref_date_field="TRADE_DT"):
    df[ref_date_field] = df['时间'].replace("[-]", "", regex=True).apply(int)
    con = df[ref_date_field].apply(lambda x: x is not None)
    df = df.loc[con]
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["代码"].replace("[SH|sh|SZ|sz]", "", regex=True)
    new_df["S_INFO_WINDCODE"] += ["."] * len(df) + df["代码"].replace("[0123456789]", "", regex=True).apply(
        lambda x: x.upper())
    new_df[ref_date_field] = df[ref_date_field]
    new_df["MFW_NET_INFLOW_AMOUNT"] = (df["主力净流入(万元)"] * [10000] * len(df))
    new_df["MFW_NET_INFLOW_RATE"] = df["主力净流入占比"].apply(lambda x: round(x, 4))
    new_df["SUPER_LARGE_NET_INFLOW_AMOUNT"] = (df["超大单净流入(万元)"] * [10000] * len(df))
    new_df["SUPER_LARGE_NET_INFLOW_RATE"] = df["超大单净流入占比"].apply(lambda x: round(x, 4))
    new_df["LARGE_NET_INFLOW_AMOUNT"] = (df["大单净流入(万元)"] * [10000] * len(df))
    new_df["LARGE_NET_INFLOW_RATE"] = df["大单净流入占比"].apply(lambda x: round(x, 4))
    new_df["MIDDLE_NET_INFLOW_AMOUNT"] = (df["中单净流入(万元)"] * [10000] * len(df))
    new_df['MIDDLE_NET_INFLOW_RATE'] = df["中单净流入占比"].apply(lambda x: round(x, 4))
    new_df["SMALL_NET_INFLOW_AMOUNT"] = (df["小单净流入(万元)"] * [10000] * len(df))
    new_df["SMALL_NET_INFLOW_RATE"] = df["小单净流入占比"].apply(lambda x: round(x, 4))
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_1min(df, ref_date_field='TRADE_DT'):
    df["时间"] = df['时间'].replace("[- :]", "", regex=True).apply(int)
    con = df["时间"].apply(lambda x: x is not None)
    df = df.loc[con]
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["代码"].replace("[SH|SZ]", "", regex=True)
    new_df["S_INFO_WINDCODE"] += "." + df["代码"].replace("[0123456789]", "", regex=True)
    new_df[ref_date_field] = (df["时间"] / 1000000).apply(lambda x: int(x))
    new_df["TRADE_BAR_TIME"] = (df["时间"] % 1000000).apply(lambda x: int(x))
    new_df["S_DQ_OPEN"] = (df["开盘价"] / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_HIGH"] = (df["最高价"] / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_LOW"] = (df["最低价"] / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_CLOSE"] = (df["收盘价"] / df["复权系数"]).apply(lambda x: round(x, 2))
    new_df["S_DQ_VOLUME"] = (df["成交量(手)"] * 100 / df["复权系数"]).apply(lambda x: round(x))
    new_df["S_DQ_AMOUNT"] = df["成交额(元)"]
    new_df["S_DQ_ADJFACTOR"] = df["复权系数"]
    pre_close = list(new_df["S_DQ_CLOSE"])
    pre_close.insert(0, pre_close[0])
    pre_close[2] = pre_close[0]
    pre_close = pre_close[0:len(pre_close)-1]
    new_df['S_DQ_PRECLOSE'] = pre_close
    new_df["S_DQ_ADJOPEN"] = df["开盘价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_ADJHIGH"] = df["最高价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_ADJLOW"] = df["最低价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_ADJCLOSE"] = df["收盘价"].apply(lambda x: round(x, 2))
    new_df["S_DQ_AVGPRICE"] = (df["成交额(元)"] / df["成交量(手)"] / 100).apply(
        lambda x: 0 if x is None or x == float("inf") or x == float("-inf") else round(x, 2))
    new_df["S_DQ_CHANGE"] = (new_df["S_DQ_CLOSE"] - new_df["S_DQ_PRECLOSE"]).apply(
        lambda x: round(x, 2))
    new_df["S_DQ_PCTCHANGE"] = (new_df["S_DQ_CHANGE"] / new_df['S_DQ_PRECLOSE']).apply(
        lambda x: 0 if x is None or x == float("inf") or x == float("-inf") else round(x, 4))
    new_df["S_DQ_TRADESTATUS"] = 1
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_description(df):
    df["开始时间"] = df['开始时间'].replace("[-]", "", regex=True).apply(int)
    df["结束时间"] = df['结束时间'].replace("[-]", "", regex=True).apply(
        lambda x: 0 if x is None or x is np.nan else int(x))
    con = df["开始时间"].apply(lambda x: x is not None)
    df = df.loc[con]
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["股票代码"].replace("[SH|sh|SZ|sz]", "", regex=True)
    new_df["S_INFO_WINDCODE"] += "." + df["股票代码"].replace("[0123456789]", "", regex=True)
    new_df["S_INFO_WINDCODE"] = new_df["S_INFO_WINDCODE"].apply(lambda x: x.upper())
    new_df["S_INFO_LISTDATE"] = df["开始时间"]
    new_df["S_INFO_DELISTDATE"] = df["结束时间"]
    new_df["S_INFO_STOCK_NAME"] = df["期间名称"]
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_st(df):
    df["开始时间"] = df['开始时间'].replace("[-]", "", regex=True).apply(int)
    df["结束时间"] = df['结束时间'].replace("[-]", "", regex=True).apply(
        lambda x: 0 if x is None or x is np.nan else int(x))
    con = df["开始时间"].apply(lambda x: x is not None)
    df = df.loc[con]
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["股票代码"].replace("[SH|sh|SZ|sz]", "", regex=True)
    new_df["S_INFO_WINDCODE"] += "." + df["股票代码"].replace("[0123456789]", "", regex=True)
    new_df["S_INFO_WINDCODE"] = new_df["S_INFO_WINDCODE"].apply(lambda x: x.upper())
    new_df["ANN_DT"] = df["开始时间"]
    new_df["ENTRY_DT"] = df["开始时间"]
    new_df["REMOVE_DT"] = df["结束时间"]
    new_df["S_TYPE_ST"] = df["期间名称"].apply(lambda x: 1 if "ST" in x else 0)
    con = new_df["S_TYPE_ST"].apply(lambda x: x == 1)
    new_df = new_df.loc[con]
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_suspension(df):
    df = df.dropna()
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["代码"].apply(
        lambda x: str(x).rjust(6, "0") + ".SZ" if x < 600000 else str(x) + ".SH")
    new_df["S_DQ_SUSPEND_DATE"] = df["停牌日期"].apply(lambda x: x.year * 10000 + x.month * 100 + x.day)
    new_df["S_DQ_SUSPEND_TIME"] = df["停牌时间"].apply(lambda x: x.hour * 10000 + x.minute * 100 + x.second)
    new_df["S_DQ_RESUME_DATE"] = df["复牌日期"].apply(
        lambda x: x.year * 10000 + x.month * 100 + x.day if x is not None and type(x) is not float else 0)
    new_df["S_DQ_RESUME_TIME"] = df["复牌时间"].apply(
        lambda x: x.hour * 10000 + x.minute * 100 + x.second if x is not None and type(x) is not float else 0)
    new_df["S_DQ_SUSPEND_TYPE"] = df["类型"].apply(int)
    new_df["S_INFO_REASON"] = df["Reason"]
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_ticker(df, ref_date_field='TRADE_DT'):
    df["时间"] = df['时间'].replace("[- :]", "", regex=True).apply(int)
    con = df["时间"].apply(lambda x: x is not None)
    df = df.loc[con]
    new_df = pd.DataFrame()
    new_df["S_INFO_MARKET"] = df["市场代码"].apply(str)
    new_df["S_INFO_CODE"] = df["证券代码"].apply(str)
    new_df["S_INFO_WINDCODE"] = new_df["S_INFO_CODE"] + pd.Series(["."] * len(df)) + new_df["S_INFO_MARKET"]
    new_df[ref_date_field] = (df["时间"] / 1000000).apply(lambda x: int(x))
    new_df["TRADE_TIME"] = (df["时间"] % 1000000).apply(lambda x: int(x))
    new_df["S_DQ_TRADE_TIMES"] = df["成交笔数"]
    new_df["S_DQ_SIDE"] = df["方向"]
    new_df["S_DQ_VOLUME"] = df["成交量"]
    new_df["S_DQ_AMOUNT"] = df["成交额"]
    new_df["S_DQ_LAST_PRICE"] = df["最新价"]
    new_df["S_DQ_BID_PRICE1"] = df["买一价"]
    new_df["S_DQ_BID_PRICE2"] = df["买二价"]
    new_df["S_DQ_BID_PRICE3"] = df["买三价"]
    new_df["S_DQ_BID_PRICE4"] = df["买四价"]
    new_df["S_DQ_BID_PRICE5"] = df["买五价"]
    new_df["S_DQ_BID_QUANTITY1"] = df["买一量"]
    new_df["S_DQ_BID_QUANTITY2"] = df["买二量"]
    new_df["S_DQ_BID_QUANTITY3"] = df["买三量"]
    new_df["S_DQ_BID_QUANTITY4"] = df["买四量"]
    new_df["S_DQ_BID_QUANTITY5"] = df["买五量"]
    new_df["S_DQ_ASK_PRICE1"] = df["卖一价"]
    new_df["S_DQ_ASK_PRICE2"] = df["卖二价"]
    new_df["S_DQ_ASK_PRICE3"] = df["卖三价"]
    new_df["S_DQ_ASK_PRICE4"] = df["卖四价"]
    new_df["S_DQ_ASK_PRICE5"] = df["卖五价"]
    new_df["S_DQ_ASK_QUANTITY1"] = df["卖一量"]
    new_df["S_DQ_ASK_QUANTITY2"] = df["卖二量"]
    new_df["S_DQ_ASK_QUANTITY3"] = df["卖三量"]
    new_df["S_DQ_ASK_QUANTITY4"] = df["卖四量"]
    new_df["S_DQ_ASK_QUANTITY5"] = df["卖五量"]
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_transaction(df, code, trade_date):
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = [code] * len(df)
    new_df['TRADE_DT'] = [trade_date] * len(df)
    new_df['TRADE_TIME'] = df["t"].apply(int)
    new_df['S_DQ_PRICE'] = df["p"].apply(lambda x: round(float(x), 2))
    new_df['S_DQ_QUANTITY'] = df["q"].apply(lambda x: round(float(x), 2))
    new_df['S_DQ_SIDE'] = df["s"]
    new_df['S_RESERVED_ID'] = range(len(df))
    new_df.replace([None], "None", inplace=True)
    return new_df


def __process_industry_class(df):
    con = df["l1"].apply(lambda x: x is not None)
    df = df.loc[con].dropna()
    new_df = pd.DataFrame()
    new_df["S_INFO_WINDCODE"] = df["s"]
    new_df['TRADE_DT'] = [20191018] * len(df)
    new_df['INDUSTRY_NAME'] = df["l1"]
    new_df['DATA_SOURCE'] = ["申万"] * len(df)
    new_df['STOCK_CODE'] = df["s"].replace("[.SH|.sh|.SZ|.sz]", "", regex=True)
    new_df['STOCK_NAME'] = df["n"]
    new_df['LEVEL_NUM'] = [1] * len(df)
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_industry_class_inout(df, industry_class):
    con = df["s"].apply(lambda x: x is not None and x != '数据来源：Wind' and x is not np.nan)
    df = df[con].dropna()
    new_df = pd.DataFrame()
    new_df["SEQUENCE"] = df["s"].apply(lambda x: 0 if x is np.nan else int(x))
    new_df['ACTION_DT'] = df["ad"].apply(lambda x: x.year * 10000 + x.month * 100 + x.day)
    new_df['INDUSTRY_NAME'] = [industry_class] * len(df)
    new_df['DATA_SOURCE'] = ["申万"] * len(df)
    new_df['STOCK_CODE'] = df["sc"].apply(lambda x: round(float(str(x).replace("T", "6")), 0)).apply(int).apply(
        lambda x: str(x).rjust(6, "0"))
    new_df['STOCK_NAME'] = df["sn"]
    new_df['ACTION'] = df["a"]
    con_result = new_df["ACTION"].apply(lambda x: x is not None)
    new_df = new_df[con_result]
    new_df.replace([None], 0, inplace=True)
    return new_df


def __process_idx_weight(df):
    new_df = pd.DataFrame()
    new_df['S_INDEX_WINDCODE'] = df["指数代码"].apply(lambda x: str(x).zfill(6))
    new_df['S_INFO_WINDCODE'] = df["成份证券代码"].apply(lambda x: str(x).zfill(6))
    new_df['S_STOCK_NAME'] = df["成份证券简称"]
    new_df['TRADE_DT'] = df["截止日期"].apply(lambda x: (x.replace("-", ""))).apply(int)
    new_df['I_WEIGHT'] = df['权重(%)']
    new_df.replace([None], "", inplace=True)
    return new_df


def to_sql_replace(pd_table, connection, keys, data_iter):
    header_sql = "REPLACE INTO " + pd_table.name + "(" + ", ".join(keys) + ") VALUES "
    mid_sql = ""
    cnt = 0
    for yy in data_iter:
        mid_sql += str(yy) + ","
        cnt += 1
        if cnt >= 4000:
            sql = header_sql + mid_sql[:-1]
            # print(sql)
            connection.execute(sql)
            cnt = 0
            mid_sql = ""
    if mid_sql != "":
        sql = header_sql + mid_sql[:-1]
        # print(sql)
        connection.execute(sql)
        
        
def __write_log(logger, content):
    if logger is not None:
        logger.info(content)
    else:
        print(content)


def query_jsy_csv(table_name: str, start_date=None, end_date=None, ref_date_field='TRADE_DT', fields=None,
                  intra_day=False, db="BASE", logger=None):
    if start_date is not None:
        today = datetime.today().strftime('%Y%m%d')
        if today == str(start_date):
            if not intra_day:
                now_time = int(datetime.now().time().strftime('%H%M'))
                if now_time < 1500:
                    return pd.DataFrame()
    year = 1970 if start_date is None else int(start_date / 10000)
    if table_name == "ASHARE_EOD_PRICES":
        dir_path = "Stk_DAY_FQ_WithHS_20191001" + os.path.sep
    elif table_name == "ASHARE_1MIN":
        dir_path = "Stk_Min1_FQ_" + str(year) + os.path.sep
    elif table_name == "ASHARE_DESCRIPTION":
        dir_path = "Stk_Nmchg" + os.path.sep
    elif table_name == "ASHARE_TICKER":
        dir_path = "Stk_Tick" + os.path.sep
    elif table_name == "ASHARE_TRANSACTION":
        dir_path = "Stk_TradeByTrade" + os.path.sep
    elif table_name == "ASHARE_INDUSTRIES_CLASS" or table_name == "ASHARE_INDUSTRIES_CLASS_INOUT":
        dir_path = "申万一级行业数据" + os.path.sep
    elif table_name == "ASHARE_MONEY_FLOW":
        dir_path = "Stk_ZJLX" + os.path.sep
    elif table_name == "AINDEX_EOD_PRICES":
        dir_path = "Idx_DAY_20191001" + os.path.sep
    elif table_name == "ASHARE_ST":
        dir_path = "Stk_Nmchg" + os.path.sep
    elif table_name == "ASHARE_TRADING_SUSPENSION":
        dir_path = "Stk_Suspension" + os.path.sep
    elif table_name == "AINDEX_HS300_FREE_WEIGHT":
        dir_path = "Idx_Weight_2005-201910" + os.path.sep
    elif table_name == "ASHARE_EOD_DERIVATIVE_INDICATOR":
        dir_path = "Stk_A" + os.path.sep
    else:
        dir_path = "?"
    file_path = DATA_DIR + dir_path
    result_df = None
    if os.path.exists(file_path):
        __write_log(logger, "processing path " + file_path)
        db_table_name = table_name
        if db == "TICK":
            conn = jsy_ticker_conn
            db_table_name = table_name + "_" + str(year)
        elif db == "BAR":
            conn = jsy_bar_conn
            db_table_name = table_name + "_" + str(year)
        else:
            conn = jsy_conn
        for csv_file in os.listdir(file_path):
            if table_name == "ASHARE_TRANSACTION":
                sub_file_path = file_path + csv_file
                file_prefix = "Stk_TradeByTrade_" + str(int(start_date / 10000))
                if os.path.isdir(sub_file_path) and csv_file.startswith(file_prefix):
                    __write_log(logger, f"processing path  {sub_file_path}")
                    for date_dir in os.listdir(sub_file_path):
                        sub_sub_file_path = sub_file_path + os.path.sep + date_dir
                        if os.path.isdir(sub_sub_file_path):
                            # __write_log(logger, f"processing path  {sub_sub_file_path}")
                            for real_csv_file in os.listdir(sub_sub_file_path):
                                if DEBUG_MODE and not real_csv_file.upper().startswith("SH600570"):
                                    continue
                                final_path = sub_sub_file_path + os.path.sep + real_csv_file
                                __write_log(logger, f"processing file {final_path}")
                                if year <= 2016:
                                    df = pd.read_csv(final_path, encoding="gbk", names=['t', 'p', 'q', 's'])
                                else:
                                    df = pd.read_csv(final_path, encoding="gbk", names=['t', 'p', 's', 'q'])
                                f_name = real_csv_file[:real_csv_file.index(".")]
                                stock_name = f_name[2:8] + "." + f_name[0:2]
                                df = __process_transaction(df, stock_name, int(date_dir))
                                if len(df) <= 0:
                                    continue
                                df = df[(df[ref_date_field] > start_date) & (df[ref_date_field] < end_date)]
                                if len(df) <= 0:
                                    continue
                                df.sort_values(by=[ref_date_field], ascending=True, inplace=True)
                                df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False,
                                          method=to_sql_replace)
                                if type(fields) is dict:
                                    df = df.rename(columns=fields, inplace=False)
                                    if result_df is None:
                                        result_df = [df]
                                    else:
                                        result_df = [result_df, df]
            elif table_name == "ASHARE_TICKER":
                sub_file_path = file_path + csv_file
                file_prefix = "Stk_Tick_" + str(int(start_date / 10000))
                if os.path.isdir(sub_file_path) and csv_file.startswith(file_prefix):
                    # __write_log(logger, f"processing path  {sub_file_path}")
                    for date_dir in os.listdir(sub_file_path):
                        sub_sub_file_path = sub_file_path + os.path.sep + date_dir
                        if os.path.isdir(sub_sub_file_path):
                            # __write_log(logger, f"processing path  {sub_sub_file_path}")
                            for real_csv_file in os.listdir(sub_sub_file_path):
                                if DEBUG_MODE and not real_csv_file.upper().startswith("SH600570"):
                                    continue
                                final_path = sub_sub_file_path + os.path.sep + real_csv_file
                                __write_log(logger, f"processing file {final_path}")
                                df = pd.read_csv(final_path, encoding="gbk")
                                df = __process_ticker(df)
                                if len(df) <= 0:
                                    continue
                                df = df[(df[ref_date_field] > start_date) & (df[ref_date_field] < end_date)]
                                if len(df) <= 0:
                                    continue
                                df.sort_values(by=[ref_date_field], ascending=True, inplace=True)
                                df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False,
                                          method=to_sql_replace)
                                if type(fields) is dict:
                                    df = df.rename(columns=fields, inplace=False)
                                    if result_df is None:
                                        result_df = [df]
                                    else:
                                        result_df = [result_df, df]
            elif table_name == "ASHARE_INDUSTRIES_CLASS" and csv_file.startswith("沪深股票申万行业分类数据"):
                final_path = file_path + "沪深股票申万行业分类数据.xlsx"
                __write_log(logger, f"processing file {final_path}")
                df = pd.read_excel(final_path, encoding="gbk", names=["s", "n", "l1", "l2", "l3"])
                df = __process_industry_class(df)
                df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False,
                          method=to_sql_replace)
                if type(fields) is dict:
                    df = df.rename(columns=fields, inplace=False)
                    if result_df is None:
                        result_df = [df]
                    else:
                        result_df = [result_df, df]
            elif table_name == "ASHARE_INDUSTRIES_CLASS_INOUT" and \
                    csv_file.startswith("进出记录") and csv_file.endswith(".xlsx"):
                final_path = file_path + csv_file
                __write_log(logger, f"processing file {final_path}")
                df = pd.read_excel(final_path, encoding="gbk", names=["s", "sc", "sn", "ad", "a"])
                ic = csv_file[csv_file.index("(")+3: csv_file.index(")")]
                df = __process_industry_class_inout(df, ic)
                df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False,
                          method=to_sql_replace)
                if type(fields) is dict:
                    df = df.rename(columns=fields, inplace=False)
                    if result_df is None:
                        result_df = [df]
                    else:
                        result_df = [result_df, df]
            elif table_name == "ASHARE_TRADING_SUSPENSION":
                final_path = file_path + csv_file
                __write_log(logger, f"processing file {final_path}")
                df = pd.read_excel(final_path, encoding="gbk")
                df = __process_suspension(df)
                df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False,
                          method=to_sql_replace)
                if type(fields) is dict:
                    df = df.rename(columns=fields, inplace=False)
                    if result_df is None:
                        result_df = [df]
                    else:
                        result_df = [result_df, df]
            elif table_name == "ASHARE_EOD_DERIVATIVE_INDICATOR" and FINANCIAL_MODE == "WIND":
                final_path = file_path + csv_file
                __write_log(logger, f"processing file {final_path}")
                stock_df = pd.read_excel(final_path, encoding="gbk")
                stock_df = stock_df.dropna()
                w = w if w is not None else None
                w.start()
                for row in stock_df.values:
                    d = pd.DataFrame()
                    temp_1 = w.wsd("000001.SZ",
                                   "mkt_cap_CSRC, mkt_cap_float, pe, pe_ttm", "2016-01-01", "2019-10-20",
                                   "unit=1;currencyType=;ruleType=10")
                    d["S_INFO_WINDCODE"] = [row[0]] * len(temp_1.Times)
                    d["TRADE_DT"] = pd.Series(temp_1.Times).apply(lambda x: x.year * 10000 + x.month * 100 + x.day)
                    if temp_1.ErrorCode != 0:
                        __write_log(logger, f"query error: ({temp_1.ErrorCode}){temp_1.Data[0]}")
                        break
                    temp_idx_1 = 0
                    for field_1 in temp_1.Fields:
                        d[field_1] = temp_1.Data[temp_idx_1]
                        temp_idx_1 += 1
                    temp_2 = w.wsd("000001.SZ",
                                   "pb",
                                   "2016-01-01", "2019-10-20", "ruleType=3")
                    if temp_2.ErrorCode != 0:
                        __write_log(logger, f"query error: ({temp_2.ErrorCode}){temp_2.Data[0]}")
                        break
                    temp_idx_2 = 0
                    for field_2 in temp_2.Fields:
                        d[field_2] = temp_2.Data[temp_idx_2]
                        temp_idx_2 += 1
                    temp_3 = w.wsd("000001.SZ",
                                   "pcf_ocf, pcf_ocf_ttm, pcf_ncf, pcf_ncf_ttm, ps, ps_ttm, free_turn," +
                                   "dividendyield, netprofit_ttm, operatecashflow_ttm, oper_rev, " +
                                   "net_incr_cash_cash_equ_dm, matotalshares, float_a_shares,free_float_shares",
                                   "2016-01-01", "2019-10-20", "ruleType=2; rptYear=2018; unit=1; rptType=1")
                    if temp_3.ErrorCode != 0:
                        __write_log(logger, f"query error: ({temp_3.ErrorCode}){temp_3.Data[0]}")
                        break
                    temp_idx_3 = 0
                    for field_3 in temp_3.Fields:
                        d[field_3] = temp_3.Data[temp_idx_3]
                        temp_idx_3 += 1
                    print(d)
                    df = __process_financial_indicator_wind(d)
                    if len(df) <= 0:
                        continue
                    print(df)
                    df.sort_values(by=[ref_date_field], ascending=True, inplace=True)
                    df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False, method=to_sql_replace)
                    if type(fields) is dict:
                        df = df.rename(columns=fields, inplace=False)
                        if result_df is None:
                            result_df = [df]
                        else:
                            result_df = [result_df, df]
            elif table_name == "ASHARE_EOD_DERIVATIVE_INDICATOR" and FINANCIAL_MODE == "TU_SHARE":
                ts.set_token("4e63b561394281588f62beceb8e08ef3c2f327b512425777974098e5")
                pro = ts.pro_api()
                for dt in get_trade_dates(start_date):
                    __write_log(logger, f"processing date {dt}")
                    fields_str = 'ts_code,trade_date,turnover_rate,volume_ratio, pe, pe_ttm, pb,' + \
                                 'ps, ps_ttm, total_share, float_share, free_share, total_mv, circ_mv'
                    try:
                        df = pro.daily_basic(ts_code='', trade_date=f'{dt}', fields=fields_str)
                    except Exception as ex:
                        __write_log(logger, f"daily_basic exception: {ex}")
                        df = pro.daily_basic(ts_code='', trade_date=f'{dt}', fields=fields_str)
                    df = __process_financial_indicator_ts(df)
                    if len(df) <= 0:
                        continue
                    df.sort_values(by=[ref_date_field], ascending=True, inplace=True)
                    df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False, method=to_sql_replace)
                    if type(fields) is dict:
                        df = df.rename(columns=fields, inplace=False)
                        if result_df is None:
                            result_df = [df]
                        else:
                            result_df = [result_df, df]
            elif table_name == "ASHARE_EOD_DERIVATIVE_INDICATOR" and FINANCIAL_MODE == "RiceQuant":
                final_path = file_path + csv_file
                __write_log(logger, f"processing file {final_path}")
                stock_df = pd.read_excel(final_path, encoding="gbk")
                stock_df = stock_df.dropna()
                rq = rq if rq is not None else None
                rq.init()
                for row in stock_df.values:
					for dt in get_trade_dates(start_date):
						__write_log(logger, f"processing date {dt}")
						d = pd.DataFrame()
						temp_1 = get_factor(id_convert('000001.SZ'),
											factor=['market_cap', 'market_cap_2', 'pe_ratio_lyr', 'pe_ratio_ttm', 'pb_ratio_lyr', 'pcf_ratio_lyr', 'pcf_ratio_ttm' + \
													'pcf_ratio_total_lyr', 'pcf_ratio_total_ttm' , 'ps_ratio_lyr', 'ps_ratio_ttm']
											date = f'{dt}' 
											)
                    d["S_INFO_WINDCODE"] = [row[0]] * len(temp_1.Times)
                    d["TRADE_DT"] = pd.Series(temp_1.Times).apply(lambda x: x.year * 10000 + x.month * 100 + x.day)
                    if temp_1.ErrorCode != 0:
                        __write_log(logger, f"query error: ({temp_1.ErrorCode}){temp_1.Data[0]}")
                        break
                    temp_idx_1 = 0
                    for field_1 in temp_1.Fields:
                        d[field_1] = temp_1.Data[temp_idx_1]
                        temp_idx_1 += 1
					temp_2 = get_turnover_rate(id_convert('000001.SZ'), start_date)
                    if temp_2.ErrorCode != 0:
                        __write_log(logger, f"query error: ({temp_2.ErrorCode}){temp_2.Data[0]}")
                        break
                    temp_idx_2 = 0
                    for field_2 in temp_2.Fields:
                        d[field_2] = temp_2.Data[temp_idx_2]
                        temp_idx_2 += 1
                    temp_3 = w.wsd("000001.SZ",
                                   "pcf_ocf, pcf_ocf_ttm, pcf_ncf, pcf_ncf_ttm, ps, ps_ttm, free_turn," +
                                   "dividendyield, netprofit_ttm, operatecashflow_ttm, oper_rev, " +
                                   "net_incr_cash_cash_equ_dm, matotalshares, float_a_shares,free_float_shares",
                                   "2016-01-01", "2019-10-20", "ruleType=2; rptYear=2018; unit=1; rptType=1")
                    if temp_3.ErrorCode != 0:
                        __write_log(logger, f"query error: ({temp_3.ErrorCode}){temp_3.Data[0]}")
                        break
                    temp_idx_3 = 0
                    for field_3 in temp_3.Fields:
                        d[field_3] = temp_3.Data[temp_idx_3]
                        temp_idx_3 += 1
                    print(d)
                    df = __process_financial_indicator_wind(d)
                    if len(df) <= 0:
                        continue
                    print(df)
                    df.sort_values(by=[ref_date_field], ascending=True, inplace=True)
                    df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False, method=to_sql_replace)
                    if type(fields) is dict:
                        df = df.rename(columns=fields, inplace=False)
                        if result_df is None:
                            result_df = [df]
                        else:
                            result_df = [result_df, df]
            else:
                if DEBUG_MODE and not (csv_file.upper().startswith("SH600570") or
                                       csv_file.upper().startswith("SH000001") or
                                       csv_file.lower().startswith("stk_nmchg") or
                                       csv_file.lower().startswith('idx_weight')) \
                        and csv_file.endswith(".csv"):
                    continue
                elif not csv_file.endswith(".csv"):
                    continue
                __write_log(logger, f"processing file {table_name} {file_path}{csv_file}")
                if table_name == "AINDEX_HS300_FREE_WEIGHT":
                    df = pd.read_csv(file_path + csv_file, encoding="gbk", low_memory=False)
                else:
                    df = pd.read_csv(file_path + csv_file, encoding="gbk")
                if table_name == "ASHARE_EOD_PRICES":
                    df = __process_eod_prices(df)
                elif table_name == "ASHARE_1MIN":
                    df = __process_1min(df)
                elif table_name == "ASHARE_DESCRIPTION":
                    df = __process_description(df)
                elif table_name == "ASHARE_MONEY_FLOW":
                    df = __process_zjlx(df)
                elif table_name == "AINDEX_EOD_PRICES":
                    df = __process_index_prices(df)
                elif table_name == "ASHARE_ST":
                    df = __process_st(df)
                elif table_name == "AINDEX_HS300_FREE_WEIGHT":
                    df = __process_idx_weight(df)
                else:
                    continue
                if len(df) <= 0:
                    continue
                df = df[(df[ref_date_field] > start_date) & (df[ref_date_field] < end_date)]
                if len(df) <= 0:
                    continue
                df.sort_values(by=[ref_date_field], ascending=True, inplace=True)
                df.to_sql(db_table_name.lower(), conn, if_exists='append', index=False, method=to_sql_replace)
                if type(fields) is dict:
                    df = df.rename(columns=fields, inplace=False)
                    if result_df is None:
                        result_df = [df]
                    else:
                        result_df = [result_df, df]
    else:
        __write_log(logger, "processing path " + file_path + " not found")
    return result_df


def set_logger(log_file=None, level=logging.DEBUG, mode='a', name=""):
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
    if log_file is not None:
        handler_file = logging.FileHandler(filename=log_file, encoding='utf-8', mode=mode)
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
    start_dt = datetime.strptime(str(start_date), '%Y%m%d')
    today = datetime.today()
    periods = []
    _start = start_dt
    _end = datetime(start_dt.year, 12, 31)
    for i in range(100):
        if _end.date() < today.date():
            periods.append((int(_start.strftime('%Y%m%d')), int(_end.strftime('%Y%m%d'))))
        else:
            periods.append((int(_start.strftime('%Y%m%d')), int(today.strftime("%Y%m%d"))))
            break
        _temp = _end + relativedelta(days=1)
        _end = _end + relativedelta(years=1)
        _start = _temp
    return periods


def reverse_copy_industry_class(logger=None):
    sql = "SELECT " + \
          " t.S_INFO_WINDCODE, t.TRADE_DT, t.INDUSTRY_NAME, t.DATA_SOURCE, t.STOCK_CODE, t.STOCK_NAME, t.LEVEL_NUM" + \
          " FROM ashare_industries_class t WHERE t.TRADE_DT in (SELECT max(a.TRADE_DT) from ashare_industries_class a)"
    df = pd.read_sql_query(sql, jsy_conn)
    sql_trade_df = pd.read_sql_query(
        "SELECT DISTINCT a.TRADE_DT from ashare_eod_prices a WHERE a.TRADE_DT > 20060101 ORDER BY a.TRADE_DT DESC",
        jsy_conn)
    sql_min_trade_dt = pd.read_sql_query("SELECT min(a.TRADE_DT) FROM ashare_industries_class a", jsy_conn)
    begin = datetime.now()
    for row in sql_trade_df.values:
        if row[0] > sql_min_trade_dt.iloc[0, 0]:
            __write_log(logger, f"skip processing industry class {row[0]}")
            continue
        __write_log(logger, f"processing copy {row[0]} {len(df)} rows")
        df["TRADE_DT"] = [row[0]] * len(df)
        df.to_sql("ashare_industries_class", jsy_conn, if_exists='append', index=False,
                  method=to_sql_replace)
    end = datetime.now()
    __write_log(logger, f"cost time {(end - begin).seconds}s")
    
    
def refresh_industry_class(logger=None):
    sql_stock_codes = pd.read_sql_query(
        "SELECT DISTINCT a.STOCK_CODE FROM ashare_industries_class_inout a",
        jsy_conn)
    begin = datetime.now()
    cnt = 0
    for row in sql_stock_codes.values:
        cnt += 1
        __write_log(logger, f"processing {row[0]} {cnt}/{len(sql_stock_codes)}")
        prev_date, prev_industry_name = None, ""
        inout = pd.read_sql_query(
            " SELECT a.ACTION_DT, a.INDUSTRY_NAME, a.DATA_SOURCE, a.STOCK_CODE, a.ACTION " +
            " FROM ashare_industries_class_inout a" +
            f" WHERE a.STOCK_CODE = '{row[0]}' ORDER BY a.ACTION_DT ASC;",
            jsy_conn)
        for sub_row in inout.values:
            sql = ""
            if sub_row[4] == "纳入":
                if prev_date is None:
                    sql = " UPDATE ashare_industries_class " + \
                          " SET INDUSTRY_NAME = '' " + \
                          f" WHERE STOCK_CODE = '{sub_row[3]}' " + \
                          f" AND DATA_SOURCE = '{sub_row[2]}' " + \
                          f" AND TRADE_DT < {sub_row[0]}"
                    prev_date = sub_row[0]
                else:
                    sql = " UPDATE ashare_industries_class " + \
                          " SET INDUSTRY_NAME = '{prev_industry_name}' " + \
                          f" WHERE STOCK_CODE = '{sub_row[3]}' " + \
                          f" AND DATA_SOURCE = '{sub_row[2]}' " + \
                          f" AND TRADE_DT < {sub_row[0]} " + \
                          f" AND TRADE_DT > {prev_date}"
                    prev_date = row[0]
            elif sub_row[4] == "剔除":
                if prev_date is None:
                    sql = f"UPDATE ashare_industries_class " + \
                          f" SET INDUSTRY_NAME = REPLACE(INDUSTRY_NAME, '{sub_row[1]}', '') " + \
                          f" WHERE STOCK_CODE = '{sub_row[3]}' " + \
                          f" AND DATA_SOURCE = '{sub_row[2]}' " + \
                          f" AND TRADE_DT < {sub_row[0]}"
                else:
                    sql = " UPDATE ashare_industries_class " + \
                          f" SET INDUSTRY_NAME = REPLACE(INDUSTRY_NAME, '{sub_row[1]}', '') " +\
                          f" WHERE STOCK_CODE = '{sub_row[3]}' " + \
                          f" AND DATA_SOURCE = '{sub_row[2]}' " + \
                          f" AND TRADE_DT < {sub_row[0]} " + \
                          f" AND TRADE_DT > {prev_date}"
            else:
                __write_log(logger, "why ???? " + row[4])
            # print(sql)
            jsy_conn.execute(sql)
    end = datetime.now()
    __write_log(logger, f"cost time {(end - begin).seconds}s")


if __name__ == '__main__':
    print([i for i in split_date(20061101)])
    print([i for i in split_date(20181228)])
    jsy_logger = set_logger()
    jsy_logger.info("This is a test")
    jsy_logger.debug("This is a test")
    with open('table_config.yml', 'r') as file:
        config = yaml.load(file, Loader=yaml.FullLoader)
    result = query_jsy_csv(
        "ASHARE_EOD_PRICES", 19000101, 19931231, fields=config["ASHARE_EOD_PRICES"]["fields"], logger=jsy_logger)
