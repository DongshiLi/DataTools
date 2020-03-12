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
   public class DynamicMomentumIndex
    {
        public List<KeyValuePair<DateTime, Quote>> StockDetails;
        public List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> StockOHLCList;
        public int ClosePeriodCount;
        public int sdPeriodCount;
        public int smoothedSdPeriodCount;
        public int rsiPeriodCount;
        public int upLimit;
        public int lowLimit;

        public DynamicMomentumIndex(List<KeyValuePair<DateTime, Quote>> stock, List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> ohlc, int close, int sd, int smoothedsd, int rsi, int up, int low)
        {
            this.StockDetails = stock;
            this.StockOHLCList = ohlc;
            this.ClosePeriodCount = close;
            this.sdPeriodCount = sd;
            this.smoothedSdPeriodCount = smoothedsd;
            this.rsiPeriodCount = rsi;
            this.upLimit = up;
            this.lowLimit = low;
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

        public int dymoiTradingRule(int index, decimal OpenDiff, decimal Price, int Position, string newFileName, DateTime StopTime)
        {
            int Side = 0;
            Logger logger = new Logger("start");
            var DYMOI = QH_IndicatorUtils.DYMOI(StockOHLCList, sdPeriodCount, smoothedSdPeriodCount, rsiPeriodCount, upLimit, lowLimit);
            if ((CompareList(index) >= 1) && (CompareList(index) != CompareList(index - 1)))
            {
                //Console.WriteLine((decimal)(shortSMA[CompareList(index)] - longSMA[CompareList(index)]));
                //Console.WriteLine((decimal)(shortSMA[CompareList(index) - 1] - longSMA[CompareList(index) - 1]));
                if (DYMOI[CompareList(index)] > 70)
                {
                    OpenDiff = (decimal)(DYMOI[CompareList(index)]);
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
                else if (DYMOI[CompareList(index)] < 30)
                {
                    OpenDiff = (decimal)(DYMOI[CompareList(index)]);
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
                    OpenDiff = (decimal)(DYMOI[CompareList(index)]);
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

        //public decimal GetAverageClosePrice(int index)
        //{
        //    decimal AverageClosePrice = 0;
        //    decimal ClosePrice = 0;
        //    int Interval = index / ClosePeriodCount;

        //    if (Interval < RollingPeriodCount) AverageClosePrice = 0;
        //    else
        //    {
        //        for (int i = Interval - RollingPeriodCount; i < Interval; i++)
        //        {
        //            OHLC ohlc = new OHLC(StockDetails, ClosePeriodCount, i * ClosePeriodCount);
        //            ClosePrice = ClosePrice + ohlc.GetClosePrice();
        //        }
        //        AverageClosePrice = ClosePrice / RollingPeriodCount;
        //    }
        //    return AverageClosePrice;
        //}

    }
}
