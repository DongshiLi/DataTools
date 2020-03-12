using System;
using System.Collections.Generic;
using System.Linq;
using EMSMessageType;

namespace Strategy
{
   public class OHLC
    {
        public List<KeyValuePair<DateTime, Quote>> StockDetails;
        public int PeriodCount;

        public OHLC(List<KeyValuePair<DateTime, Quote>> stock, int periodcount)
        {
            this.StockDetails = stock;
            this.PeriodCount = periodcount;
        }

        public List<KeyValuePair<DateTime, decimal>> GetOpenPriceList()
        {
            List<KeyValuePair<DateTime, decimal>> OpenPriceList = new List<KeyValuePair<DateTime, decimal>>();

            DateTime OpenTime1 = new DateTime(StockDetails[0].Key.Year, StockDetails[1].Key.Month, StockDetails[1].Key.Day, 9, 30, 0);
            DateTime OpenTime2 = new DateTime(StockDetails[0].Key.Year, StockDetails[1].Key.Month, StockDetails[1].Key.Day, 13, 0, 0);

            for(int i = 0; i < 7200 / PeriodCount; i++)
            {
                foreach (var kvp in StockDetails)
                {
                    if (kvp.Key > OpenTime1.AddSeconds(i * PeriodCount) && kvp.Key <= OpenTime1.AddSeconds((i + 1) * PeriodCount))
                    {
                        if (OpenPriceList.Count() < i + 1)
                        {
                            KeyValuePair<DateTime, decimal> OpenPrice = new KeyValuePair<DateTime, decimal>(OpenTime1.AddSeconds((i + 1) * PeriodCount), (kvp.Value.BestBid + kvp.Value.BestAsk) / 2);
                            OpenPriceList.Add(OpenPrice);
                        }
                    }
                }
            }

            for (int i = 0; i < 7200 / PeriodCount; i++)
            {
                foreach (var kvp in StockDetails)
                {
                    if (kvp.Key > OpenTime2.AddSeconds(i * PeriodCount) && kvp.Key <= OpenTime2.AddSeconds((i + 1) * PeriodCount))
                    {
                        if (OpenPriceList.Count() < i + 1 + 7200 / PeriodCount)
                        {
                            KeyValuePair<DateTime, decimal> OpenPrice = new KeyValuePair<DateTime, decimal>(OpenTime2.AddSeconds((i + 1) * PeriodCount), (kvp.Value.BestBid + kvp.Value.BestAsk) / 2);
                            OpenPriceList.Add(OpenPrice);
                        }
                    }
                }
            }
            return OpenPriceList;
        }

        public List<KeyValuePair<DateTime, decimal>> GetHighPriceList()
        {
            List<KeyValuePair<DateTime, decimal>> HighPriceList = new List<KeyValuePair<DateTime, decimal>>();

            DateTime OpenTime1 = new DateTime(StockDetails[0].Key.Year, StockDetails[1].Key.Month, StockDetails[1].Key.Day, 9, 30, 0);
            DateTime OpenTime2 = new DateTime(StockDetails[0].Key.Year, StockDetails[1].Key.Month, StockDetails[1].Key.Day, 13, 0, 0);

            for (int i = 0; i < 7200 / PeriodCount; i++)
            {
                decimal HighPrice = 0;
                decimal Price = 0;
                foreach (var kvp in StockDetails)
                {
                    if (kvp.Key > OpenTime1.AddSeconds(i * PeriodCount) && kvp.Key <= OpenTime1.AddSeconds((i + 1) * PeriodCount))
                    {
                        Price = (kvp.Value.BestBid + kvp.Value.BestAsk) / 2;
                        if (HighPrice < Price) HighPrice = Price;
                    }
                }
                KeyValuePair<DateTime, decimal> HighPricePair = new KeyValuePair<DateTime, decimal>(OpenTime1.AddSeconds((i + 1) * PeriodCount), HighPrice);
                HighPriceList.Add(HighPricePair);
            }

            for (int i = 0; i < 7200 / PeriodCount; i++)
            {
                decimal HighPrice = 0;
                decimal Price = 0;
                foreach (var kvp in StockDetails)
                {
                    if (kvp.Key > OpenTime2.AddSeconds(i * PeriodCount) && kvp.Key <= OpenTime2.AddSeconds((i + 1) * PeriodCount))
                    {
                        Price = (kvp.Value.BestBid + kvp.Value.BestAsk) / 2;
                        if (HighPrice < Price) HighPrice = Price;
                    }
                }
                KeyValuePair<DateTime, decimal> HighPricePair = new KeyValuePair<DateTime, decimal>(OpenTime2.AddSeconds((i + 1) * PeriodCount), HighPrice);
                HighPriceList.Add(HighPricePair);
            }
            return HighPriceList;
        }

        public List<KeyValuePair<DateTime, decimal>> GetLowPriceList()
        {
            List<KeyValuePair<DateTime, decimal>> LowPriceList = new List<KeyValuePair<DateTime, decimal>>();

            DateTime OpenTime1 = new DateTime(StockDetails[0].Key.Year, StockDetails[1].Key.Month, StockDetails[1].Key.Day, 9, 30, 0);
            DateTime OpenTime2 = new DateTime(StockDetails[0].Key.Year, StockDetails[1].Key.Month, StockDetails[1].Key.Day, 13, 0, 0);

            for (int i = 0; i < 7200 / PeriodCount; i++)
            {
                decimal LowPrice = 9999;
                decimal Price = 0;
                foreach (var kvp in StockDetails)
                {
                    if (kvp.Key > OpenTime1.AddSeconds(i * PeriodCount) && kvp.Key <= OpenTime1.AddSeconds((i + 1) * PeriodCount))
                    {
                        Price = (kvp.Value.BestBid + kvp.Value.BestAsk) / 2;
                        if (LowPrice > Price) LowPrice = Price;
                    }
                }
                KeyValuePair<DateTime, decimal> LowPricePair = new KeyValuePair<DateTime, decimal>(OpenTime1.AddSeconds((i + 1) * PeriodCount), LowPrice);
                LowPriceList.Add(LowPricePair);
            }

            for (int i = 0; i < 7200 / PeriodCount; i++)
            {
                decimal LowPrice = 9999;
                decimal Price = 0;
                foreach (var kvp in StockDetails)
                {
                    if (kvp.Key > OpenTime2.AddSeconds(i * PeriodCount) && kvp.Key <= OpenTime2.AddSeconds((i + 1) * PeriodCount))
                    {
                        Price = (kvp.Value.BestBid + kvp.Value.BestAsk) / 2;
                        if (LowPrice > Price) LowPrice = Price;
                    }
                }
                KeyValuePair<DateTime, decimal> LowPricePair = new KeyValuePair<DateTime, decimal>(OpenTime2.AddSeconds((i + 1) * PeriodCount), LowPrice);
                LowPriceList.Add(LowPricePair);
            }
            return LowPriceList; ;
        }

        public List<KeyValuePair<DateTime, decimal>> GetClosePriceList()
        {
            List<KeyValuePair<DateTime, decimal>> ClosePriceList = new List<KeyValuePair<DateTime, decimal>>();

            DateTime OpenTime1 = new DateTime(StockDetails[0].Key.Year, StockDetails[1].Key.Month, StockDetails[1].Key.Day, 9, 30, 0);
            DateTime OpenTime2 = new DateTime(StockDetails[0].Key.Year, StockDetails[1].Key.Month, StockDetails[1].Key.Day, 13, 0, 0);

            for (int i = 0; i < 7200 / PeriodCount; i++)
            {
                decimal ClosePrice = 0;
                foreach (var kvp in StockDetails)
                {
                    if (kvp.Key > OpenTime1.AddSeconds(i * PeriodCount) && kvp.Key <= OpenTime1.AddSeconds((i + 1) * PeriodCount)) ClosePrice = (kvp.Value.BestBid + kvp.Value.BestAsk) / 2;
                }
                KeyValuePair<DateTime, decimal> ClosePricePair = new KeyValuePair<DateTime, decimal>(OpenTime1.AddSeconds((i + 1) * PeriodCount), ClosePrice);
                ClosePriceList.Add(ClosePricePair);
            }

            for (int i = 0; i < 7200 / PeriodCount; i++)
            {
                decimal ClosePrice = 0;
                foreach (var kvp in StockDetails)
                {
                    if (kvp.Key > OpenTime2.AddSeconds(i * PeriodCount) && kvp.Key <= OpenTime2.AddSeconds((i + 1) * PeriodCount)) ClosePrice = (kvp.Value.BestBid + kvp.Value.BestAsk) / 2;
                }
                KeyValuePair<DateTime, decimal> ClosePricePair = new KeyValuePair<DateTime, decimal>(OpenTime2.AddSeconds((i + 1) * PeriodCount), ClosePrice);
                ClosePriceList.Add(ClosePricePair);
            }
            return ClosePriceList;
        }
    }
}
