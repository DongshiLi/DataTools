﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common.Logger;
using EMSMessageType;
using QH_Indicator;

namespace Strategy
{
   public class CommodityChannelIndex
    {
        public List<KeyValuePair<DateTime, Quote>> StockDetails;
        public List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> StockOHLCList;
        public int ClosePeriodCount;
        public int RollingPeriodCount;

        public CommodityChannelIndex(List<KeyValuePair<DateTime, Quote>> stock, List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> ohlc, int close, int rolling)
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

        public int cciTradingRule(int index, decimal OpenDiff, decimal Price, int Position, string newFileName, DateTime StopTime)
        {
            int Side = 0;
            Logger logger = new Logger("start");
            var CCI = QH_IndicatorUtils.CCI(StockOHLCList, RollingPeriodCount);
            if ((CompareList(index) >= 1) && (CompareList(index) != CompareList(index - 1)))
            {
                //Console.WriteLine((decimal)(shortEMA[CompareList(index)] - longEMA[CompareList(index)]));
                //Console.WriteLine((decimal)(shortEMA[CompareList(index) - 1] - longEMA[CompareList(index) - 1]));
                if (CCI[CompareList(index)] > 100)
                {
                    OpenDiff = (decimal)(CCI[CompareList(index)]);
                    Price = StockDetails[index].Value.BestAsk;
                    Side = 1;
                    Position = Position + 1;
                    Console.WriteLine("Open Position(" + " bid: " + StockDetails[index].Value.BestBid + " ask: " + StockDetails[index].Value.BestAsk + " Price: " + Price + " Postion: " + Position + " OpenDiff: " + OpenDiff + ")");
                    logger.Info("Open Position(" + " bid: " + StockDetails[index].Value.BestBid + " ask: " + StockDetails[index].Value.BestAsk + " Price: " + Price + " Postion: " + Position + " OpenDiff: " + OpenDiff + ")");
                    string tradeRecords = StockDetails[index].Key + "," + StockDetails[index].Value.Symbol + "," + StockDetails[index].Value.BestBid + "," + StockDetails[index].Value.BestBidQuantity +
                                            "," + StockDetails[index].Value.BestAsk + "," + StockDetails[index].Value.BestAskQuantity + "," + StockDetails[index].Value.BidPriceQueue + "," + StockDetails[index].Value.BidQuantityQueue +
                                            "," + StockDetails[index].Value.AskPriceQueue + "," + StockDetails[index].Value.AskQuantityQueue + "," + Price + "," + Side + "," + Position + "," + OpenDiff;
                    StringBuilder csvcontent = new StringBuilder();
                    csvcontent.AppendLine(tradeRecords);
                    File.AppendAllText(newFileName, csvcontent.ToString());
                }
                else if (CCI[CompareList(index)] < -100)
                {
                    OpenDiff = (decimal)(CCI[CompareList(index)]);
                    Price = StockDetails[index].Value.BestBid;
                    Side = -1;
                    Position = Position - 1;
                    Console.WriteLine("Open Position(" + " bid: " + StockDetails[index].Value.BestBid + " ask: " + StockDetails[index].Value.BestAsk + " Price: " + Price + " Postion: " + Position + " OpenDiff: " + OpenDiff + ")");
                    logger.Info("Open Position(" + " bid: " + StockDetails[index].Value.BestBid + " ask: " + StockDetails[index].Value.BestAsk + " Price: " + Price + " Postion: " + Position + " OpenDiff: " + OpenDiff + ")");
                    string tradeRecords = StockDetails[index].Key + "," + StockDetails[index].Value.Symbol + "," + StockDetails[index].Value.BestBid + "," + StockDetails[index].Value.BestBidQuantity +
                                            "," + StockDetails[index].Value.BestAsk + "," + StockDetails[index].Value.BestAskQuantity + "," + StockDetails[index].Value.BidPriceQueue + "," + StockDetails[index].Value.BidQuantityQueue +
                                            "," + StockDetails[index].Value.AskPriceQueue + "," + StockDetails[index].Value.AskQuantityQueue + "," + Price + "," + Side + "," + Position + "," + OpenDiff;
                    StringBuilder csvcontent = new StringBuilder();
                    csvcontent.AppendLine(tradeRecords);
                    File.AppendAllText(newFileName, csvcontent.ToString());
                }
                else if ((StockDetails[index].Key >= StopTime) & (Position != 0))
                {
                    OpenDiff = (decimal)(CCI[CompareList(index)]);
                    if (Position == 1)
                    {
                        Price = StockDetails[index].Value.BestBid;
                        Side = -1;
                        Position = Position - 1;
                    }
                    else if (Position == -1)
                    {
                        Price = StockDetails[index].Value.BestAsk;
                        Side = 1;
                        Position = Position + 1;
                    }
                    Console.WriteLine("Open Position(" + " bid: " + StockDetails[index].Value.BestBid + " ask: " + StockDetails[index].Value.BestAsk + " Price: " + Price + " Postion: " + Position + " OpenDiff: " + OpenDiff + ")");
                    logger.Info("Open Position(" + " bid: " + StockDetails[index].Value.BestBid + " ask: " + StockDetails[index].Value.BestAsk + " Price: " + Price + " Postion: " + Position + " OpenDiff: " + OpenDiff + ")");
                    string tradeRecords = StockDetails[index].Key + "," + StockDetails[index].Value.Symbol + "," + StockDetails[index].Value.BestBid + "," + StockDetails[index].Value.BestBidQuantity +
                                            "," + StockDetails[index].Value.BestAsk + "," + StockDetails[index].Value.BestAskQuantity + "," + StockDetails[index].Value.BidPriceQueue + "," + StockDetails[index].Value.BidQuantityQueue +
                                            "," + StockDetails[index].Value.AskPriceQueue + "," + StockDetails[index].Value.AskQuantityQueue + "," + Price + "," + Side + "," + Position + "," + OpenDiff;
                    StringBuilder csvcontent = new StringBuilder();
                    csvcontent.AppendLine(tradeRecords);
                    File.AppendAllText(newFileName, csvcontent.ToString());
                }
            }

            return Position;
        }

        //public List<decimal> GetClosePriceList(int index)
        //{
        //    int Interval = index / ClosePeriodCount;
        //    List<decimal> CloseList = new List<decimal>();

        //    if (Interval < RollingPeriodCount) CloseList.Clear();
        //    else
        //    {
        //        for (int i = Interval - RollingPeriodCount; i < Interval; i++)
        //        {
        //            OHLC ohlc = new OHLC(StockDetails, ClosePeriodCount, i * ClosePeriodCount);
        //            CloseList.Add(ohlc.GetClosePrice());
        //        }
        //    }
        //    return CloseList;
        //}

        //public decimal GetStandardDeviation(int index)
        //{
        //    decimal std = 0;
        //    List<decimal> sList = GetClosePriceList(index);
        //    if(sList.Count() > 0)
        //    {
        //        decimal avg = sList.Average();
        //        decimal sum = sList.Sum(d => (decimal)Math.Pow((double)d - (double)avg, 2));
        //        std = (decimal)Math.Sqrt(((double)sum) / ((double)sList.Count() - 1));
        //    }
        //    return std;
        //}
    }
}
