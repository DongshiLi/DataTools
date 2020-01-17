import tushare as ts

ts.set_token("4e63b561394281588f62beceb8e08ef3c2f327b512425777974098e5")
pro = ts.pro_api()
df = pro.daily_basic(ts_code='', trade_date='20180726', fields='ts_code,trade_date,turnover_rate,volume_ratio,pe,pb')
print(df)
