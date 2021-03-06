﻿
namespace MerageTable.asharetable
{
    class ASHAREMONEYFLOW
    {
        public string S_INFO_WINDCODE{ get; set; } // Wind代码  
        public string TRADE_DT{ get; set; }        // 日期  
        public decimal BUY_VALUE_EXLARGE_ORDER { get; set; } //机构买入金额(万元) 
        public decimal SELL_VALUE_EXLARGE_ORDER { get; set; }              //机构卖出金额(万元)                 
        public decimal BUY_VALUE_LARGE_ORDER { get; set; }       //大户买入金额(万元)                 
        public decimal SELL_VALUE_LARGE_ORDER { get; set; }       //大户卖出金额(万元)       
        public decimal BUY_VALUE_MED_ORDER { get; set; }       //中户买入金额(万元)                 
        public decimal SELL_VALUE_MED_ORDER { get; set; }       //中户卖出金额(万元)                 
        public decimal BUY_VALUE_SMALL_ORDER { get; set; }       //散户买入金额(万元)                 
        public decimal SELL_VALUE_SMALL_ORDER { get; set; }       //散户卖出金额(万元)                 
        public decimal BUY_VOLUME_EXLARGE_ORDER { get; set; }       //机构买入总量(手)                  
        public decimal SELL_VOLUME_EXLARGE_ORDER { get; set; }       //机构卖出总量(手)                  
        public decimal BUY_VOLUME_LARGE_ORDER{ get; set; }        //大户买入总量(手)                  
        public decimal SELL_VOLUME_LARGE_ORDER{ get; set; }        //大户卖出总量(手)                  
        public decimal BUY_VOLUME_MED_ORDER{ get; set; }        //中户买入总量(手)                  
        public decimal SELL_VOLUME_MED_ORDER{ get; set; }        //中户卖出总量(手)                  
        public decimal BUY_VOLUME_SMALL_ORDER{ get; set; }        //散户买入总量(手)                  
        public decimal SELL_VOLUME_SMALL_ORDER{ get; set; }        //散户卖出总量(手)                  
        public decimal TRADES_COUNT{ get; set; }        //成交笔数(笔)                
        public decimal BUY_TRADES_EXLARGE_ORDER{ get; set; }        //机构买入单数(单)                  
        public decimal SELL_TRADES_EXLARGE_ORDER{ get; set; }        //机构卖出单数(单)                  
        public decimal BUY_TRADES_LARGE_ORDER{ get; set; }        //大户买入单数(单)                  
        public decimal SELL_TRADES_LARGE_ORDER{ get; set; }        //大户卖出单数(单)                  
        public decimal BUY_TRADES_MED_ORDER{ get; set; }        //中户买入单数(单)                  
        public decimal SELL_TRADES_MED_ORDER{ get; set; }        //中户卖出单数(单)                  
        public decimal BUY_TRADES_SMALL_ORDER{ get; set; }        //散户买入单数(单)                  
        public decimal SELL_TRADES_SMALL_ORDER{ get; set; }        //散户卖出单数(单)                  
        public decimal VOLUME_DIFF_SMALL_TRADER{ get; set; }        //散户量差(含主动被动)(手)				 
        public decimal VOLUME_DIFF_SMALL_TRADER_ACT{ get; set; }        //散户量差(仅主动)(手)				 
        public decimal VOLUME_DIFF_MED_TRADER{ get; set; }        //中户量差(含主动被动)(手)				 
        public decimal VOLUME_DIFF_MED_TRADER_ACT{ get; set; }        //中户量差(仅主动)(手)				 
        public decimal VOLUME_DIFF_LARGE_TRADER{ get; set; }        //大户量差(含主动被动)(手)				 
        public decimal VOLUME_DIFF_LARGE_TRADER_ACT{ get; set; }        //大户量差(仅主动)(手)				 
        public decimal VOLUME_DIFF_INSTITUTE{ get; set; }        //机构量差(含主动被动)(手)				 
        public decimal VOLUME_DIFF_INSTITUTE_ACT{ get; set; }        //机构量差(仅主动)(手)				 
        public decimal VALUE_DIFF_SMALL_TRADER{ get; set; }        //散户金额差(含主动被动)(万元)				 
        public decimal VALUE_DIFF_SMALL_TRADER_ACT{ get; set; }        //散户金额差(仅主动)(万元)				 
        public decimal VALUE_DIFF_MED_TRADER{ get; set; }        //中户金额差(含主动被动)(万元)				 
        public decimal VALUE_DIFF_MED_TRADER_ACT{ get; set; }        //中户金额差(仅主动)(万元)				 
        public decimal VALUE_DIFF_LARGE_TRADER{ get; set; }        //大户金额差(含主动被动)(万元)				 
        public decimal VALUE_DIFF_LARGE_TRADER_ACT{ get; set; }        //大户金额差(仅主动)(万元)				 
        public decimal VALUE_DIFF_INSTITUTE{ get; set; }        //机构金额差(含主动被动)(万元)				 
        public decimal VALUE_DIFF_INSTITUTE_ACT{ get; set; }        //机构金额差(仅主动)(万元)				 
        public decimal S_MFD_INFLOWVOLUME{ get; set; }        //净流入量(手)                
        public decimal NET_INFLOW_RATE_VOLUME{ get; set; }        //流入率(量)(%)			
    	public decimal S_MFD_INFLOW_OPENVOLUME{ get; set; }        //开盘资金流入量(手)                 
        public decimal OPEN_NET_INFLOW_RATE_VOLUME{ get; set; }        //开盘资金流入率(量)(%)				 
        public decimal S_MFD_INFLOW_CLOSEVOLUME{ get; set; }        //尾盘资金流入量(手)                 
        public decimal CLOSE_NET_INFLOW_RATE_VOLUME{ get; set; }        //尾盘资金流入率(量)(%)				 
        public decimal S_MFD_INFLOW{ get; set; }        //净流入金额(万元)                  
        public decimal NET_INFLOW_RATE_VALUE{ get; set; }        //流入率(金额)                
        public decimal S_MFD_INFLOW_OPEN{ get; set; }        //开盘资金流入金额(万元)                   
        public decimal OPEN_NET_INFLOW_RATE_VALUE{ get; set; }        //开盘资金流入率(金额)                
        public decimal S_MFD_INFLOW_CLOSE{ get; set; }        //尾盘资金流入金额(万元)                   
        public decimal CLOSE_NET_INFLOW_RATE_VALUE{ get; set; }        //尾盘资金流入率(金额)                
        public decimal TOT_VOLUME_BID{ get; set; }        //委买总量(手)                
        public decimal TOT_VOLUME_ASK{ get; set; }        //委卖总量(手)                
        public decimal MONEYFLOW_PCT_VOLUME{ get; set; }        //资金流向占比(量)(%)				 
        public decimal OPEN_MONEYFLOW_PCT_VOLUME{ get; set; }        //开盘资金流向占比(量)(%)				 
        public decimal CLOSE_MONEYFLOW_PCT_VOLUME{ get; set; }        //尾盘资金流向占比(量)(%)				 
        public decimal MONEYFLOW_PCT_VALUE{ get; set; }        //资金流向占比(金额)                 
        public decimal OPEN_MONEYFLOW_PCT_VALUE{ get; set; }        //开盘资金流向占比(金额)                   
        public decimal CLOSE_MONEYFLOW_PCT_VALUE{ get; set; }        //尾盘资金流向占比(金额)                   
        public decimal S_MFD_INFLOWVOLUME_LARGE_ORDER{ get; set; }        //大单净流入量(手)                  
        public decimal NET_INFLOW_RATE_VOLUME_L{ get; set; } 		//大单流入率(量)(%)				 
        public decimal S_MFD_INFLOW_LARGE_ORDER{ get; set; }        //大单净流入金额(万元)                
        public decimal NET_INFLOW_RATE_VALUE_L{ get; set; }     //[内部]大单流入率(金额)(%)				 
        public decimal MONEYFLOW_PCT_VOLUME_L{ get; set; }         // 大单资金流向占比(量)(%)				 
        public decimal MONEYFLOW_PCT_VALUE_L{ get; set; }         // [内部]大单资金流向占比(金额)(%)				 
        public decimal S_MFD_INFLOW_OPENVOLUME_L{ get; set; }        //大单开盘资金流入量(手)                   
        public decimal OPEN_NET_INFLOW_RATE_VOLUME_L{ get; set; }         // [内部]大单开盘资金流入率(量)(%)				 
        public decimal S_MFD_INFLOW_OPEN_LARGE_ORDER{ get; set; }        //大单开盘资金流入金额(万元)                 
        public decimal OPEN_NET_INFLOW_RATE_VALUE_L{ get; set; }         // [内部]大单开盘资金流入率(金额)(%)				 
        public decimal OPEN_MONEYFLOW_PCT_VOLUME_L{ get; set; }         // [内部]大单开盘资金流向占比(量)(%)				 
        public decimal OPEN_MONEYFLOW_PCT_VALUE_L{ get; set; }         // 大单开盘资金流向占比(金额)(%)				 
        public decimal S_MFD_INFLOW_CLOSEVOLUME_L{ get; set; }        //大单尾盘资金流入量(手)                   
        public decimal CLOSE_NET_INFLOW_RATE_VOLUME_L{ get; set; }         // [内部]大单尾盘资金流入率(量)(%)				 
        public decimal S_MFD_INFLOW_CLOSE_LARGE_ORDER{ get; set; }        //大单尾盘资金流入金额(万元)                 
        public decimal CLOSE_NET_INFLOW_RATE_VALU_L{ get; set; }         // [内部]大单尾盘资金流入率(金额)(%)				 
        public decimal CLOSE_MONEYFLOW_PCT_VOLUME_L{ get; set; }         // 大单尾盘资金流向占比(量)(%)				 
        public decimal CLOSE_MONEYFLOW_PCT_VALUE_L{ get; set; }         // [内部]大单尾盘资金流向占比(金额)(%)				 
        public decimal BUY_VALUE_EXLARGE_ORDER_ACT{ get; set; }        //机构买入金额(仅主动)(万元)				 
        public decimal SELL_VALUE_EXLARGE_ORDER_ACT{ get; set; }        //机构卖出金额(仅主动)(万元)				 
        public decimal BUY_VALUE_LARGE_ORDER_ACT{ get; set; }        //大户买入金额(仅主动)(万元)				 
        public decimal SELL_VALUE_LARGE_ORDER_ACT{ get; set; }        //大户卖出金额(仅主动)(万元)				 
        public decimal BUY_VALUE_MED_ORDER_ACT{ get; set; }        //中户买入金额(仅主动)(万元)				 
        public decimal SELL_VALUE_MED_ORDER_ACT{ get; set; }        //中户卖出金额(仅主动)(万元)				 
        public decimal BUY_VALUE_SMALL_ORDER_ACT{ get; set; }        //散户买入金额(仅主动)(万元)				 
        public decimal SELL_VALUE_SMALL_ORDER_ACT{ get; set; }        //散户卖出金额(仅主动)(万元)				 
        public decimal BUY_VOLUME_EXLARGE_ORDER_ACT{ get; set; }        //机构买入总量(仅主动)(万股)				 
        public decimal SELL_VOLUME_EXLARGE_ORDER_ACT{ get; set; }        //机构卖出总量(仅主动)(万股)				 
        public decimal BUY_VOLUME_LARGE_ORDER_ACT{ get; set; }        //大户买入总量(仅主动)(万股)				 
        public decimal SELL_VOLUME_LARGE_ORDER_ACT{ get; set; }        //大户卖出总量(仅主动)(万股)				 
        public decimal BUY_VOLUME_MED_ORDER_ACT{ get; set; }        //中户买入总量(仅主动)(万股)				 
        public decimal SELL_VOLUME_MED_ORDER_ACT{ get; set; }        //中户卖出总量(仅主动)(万股)				 
        public decimal BUY_VOLUME_SMALL_ORDER_ACT{ get; set; }        //散户买入总量(仅主动)(万股)				 
        public decimal SELL_VOLUME_SMALL_ORDER_ACT{ get; set; }        //散户卖出总量(仅主动)(万股)						 

    }
}
