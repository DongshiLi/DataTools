using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common.Logger;
using EMSMessageType;
using QH_Indicator;

namespace Strategy
{
   public class ICHIMOKU
    {
        public List<KeyValuePair<DateTime, Quote>> StockDetails;
        public List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> StockOHLCList;
        public int ClosePeriodCount;
        public int M1;
        public int M2;
        public int M3;

        public ICHIMOKU(List<KeyValuePair<DateTime, Quote>> stock, List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> ohlc, int close, int m1, int m2, int m3)
        {
            this.StockDetails = stock;
            this.StockOHLCList = ohlc;
            this.ClosePeriodCount = close;
            this.M1 = m1;
            this.M2 = m2;
            this.M3 = m3;
        }

        public int CompareList(int index)
        {
            int CloseIndex = -1;
            for (int i = 0; i < StockOHLCList.Count(); i++)
            {
                if (StockOHLCList[i].Key > StockDetails[index - 1].Key && StockOHLCList[i].Key <= StockDetails[index].Key) CloseIndex = i;
            }
            return CloseIndex;
        }
        
        public void ichimoku()
        {
            var ICHIMOKU = QH_IndicatorUtils.ICHIMOKU(StockOHLCList, M1, M2, M3);
        }
    }
}
