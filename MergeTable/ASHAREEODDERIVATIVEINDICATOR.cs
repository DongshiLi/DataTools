
namespace MerageTable.asharetable
{
    class ASHAREEODDERIVATIVEINDICATOR
    {
        public string S_INFO_WINDCODE{ get; set; }
        public string TRADE_DT{ get; set; }
        public decimal S_VAL_MV{ get; set; }       // 当日总市值
        public decimal S_DQ_MV{ get; set; }        // 当日流通市值
        public decimal S_PQ_HIGH_52W_{ get; set; } // 周最高价				
        public decimal S_PQ_LOW_52W_{ get; set; }  // 周最低价		
        public decimal S_VAL_PE{ get; set; }       // 市盈率(PE)   
        public decimal S_VAL_PB_NEW{ get; set; }   // 市净率(PB)
        public decimal S_VAL_PE_TTM{ get; set; }   // 市盈率(PE, TTM)
        public decimal S_VAL_PCF_OCF{ get; set; }  // 市现率(PCF, 经营现金流) 
        public decimal S_VAL_PCF_OCFTTM{ get; set; } // 市现率(PCF, 经营现金流TTM)
        public decimal S_VAL_PCF_NCF{ get; set; }   // 市现率(PCF, 现金净流量)  
        public decimal S_VAL_PCF_NCFTTM{ get; set; } // 市现率(PCF, 现金净流量TTM)
        public decimal S_VAL_PS{ get; set; }         // 市销率(PS)
        public decimal S_VAL_PS_TTM{ get; set; }     // 市销率(PS, TTM) 
        public decimal S_DQ_TURN{ get; set; }        // 换手率	
        public decimal S_DQ_FREETURNOVER{ get; set; } // 换手率(基准.自由流通股本) 
        public decimal TOT_SHR_TODAY{ get; set; }    // 当日总股本
        public decimal FLOAT_A_SHR_TODAY{ get; set; } // 当日流通股本
        public decimal S_DQ_CLOSE_TODAY{ get; set; }  // 当日收盘价
        public decimal S_PRICE_DIV_DPS{ get; set; }   // 股价/每股派息	
        public decimal S_PQ_ADJHIGH_52W{ get; set; }  // 周最高价(复权) 
        public decimal S_PQ_ADJLOW_52W{ get; set; }   // 周最低价(复权)
        public decimal FREE_SHARES_TODAY{ get; set; } // 当日自由流通股本 
        public decimal NET_PROFIT_PARENT_COMP_TTM{ get; set; } // 归属母公司净利润(TTM) 
        public decimal NET_PROFIT_PARENT_COMP_LYR{ get; set; } // 归属母公司净利润(LYR) 
        public decimal NET_ASSETS_TODAY{ get; set; }  // 当日净资产
        public decimal NET_CASH_FLOWS_OPER_ACT_TTM{ get; set; } // 经营活动产生的现金流量净额(TTM)
        public decimal NET_CASH_FLOWS_OPER_ACT_LYR{ get; set; } // 经营活动产生的现金流量净额(LYR) 
        public decimal OPER_REV_TTM{ get; set; }      // 营业收入(TTM)
        public decimal OPER_REV_LYR{ get; set; }      // 营业收入(LYR) 
        public decimal NET_INCR_CASH_CASH_EQU_TTM{ get; set; } // 现金及现金等价物净增加额(TTM) 
        public decimal NET_INCR_CASH_CASH_EQU_LYR{ get; set; } // 现金及现金等价物净增加额(LYR) 
        public decimal UP_DOWN_LIMIT_STATUS{ get; set; } // 涨跌停状态
        public decimal LOWEST_HIGHEST_STATUS{ get; set; } // 最高最低价状态

    }
}
