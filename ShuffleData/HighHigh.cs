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
   public class HighHigh
    {
        public List<KeyValuePair<DateTime, Quote>> StockDetails;
        public List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> StockOHLCList;
        public int ClosePeriodCount;
        public int RollingPeriodCount;

        public HighHigh(List<KeyValuePair<DateTime, Quote>> stock, List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> ohlc, int close, int rolling)
        {
            this.StockDetails = stock;
            this.StockOHLCList = ohlc;
            this.ClosePeriodCount = close;
            this.RollingPeriodCount = rolling;
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
        
        public void highhigh()
        {
            var HIGHHIGH = QH_IndicatorUtils.HIGHHIGH(StockOHLCList, RollingPeriodCount);
        }
    }
}
