using Common.Logger;
using QH_Indicator;
using EMSMessageType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Strategy
{
    public class MovingAverageConvergenceDivergence
    {
        public List<KeyValuePair<DateTime, Quote>> StockDetails;
        public List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> StockOHLCList;
        public int closePeriodCount;
        public int emaPeriodCount1;
        public int emaPeriodCount2;
        public int demPeriodCount;

        public MovingAverageConvergenceDivergence(List<KeyValuePair<DateTime, Quote>> stock, List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> ohlc, int close, int ema1, int ema2, int dem)
        {
            this.StockDetails = stock;
            this.StockOHLCList = ohlc;
            this.closePeriodCount = close;
            this.emaPeriodCount1 = ema1;
            this.emaPeriodCount2 = ema2;
            this.demPeriodCount = dem;

        }

        public int CompareList(int index)
        {
            int CloseIndex = -1;
            for(int i = 0; i < StockOHLCList.Count(); i++)
            {
                if (StockOHLCList[i].Key > StockDetails[index - 1].Key && StockOHLCList[i].Key <= StockDetails[index].Key) CloseIndex = i;
            }
            return CloseIndex;
        }

        public int macdTradingRule(int index, decimal OpenDiff, decimal Price, int Position, string newFileName, DateTime StopTime)
        {
            int Side = 0;
            Logger logger = new Logger("start");
            var macd = QH_IndicatorUtils.MACD(StockOHLCList, emaPeriodCount1, emaPeriodCount2, demPeriodCount);
            if((CompareList(index) >= 1) && (CompareList(index) != CompareList(index - 1)))
            {
                //Console.WriteLine((decimal)(macd[CompareList(index)].MacdLine - macd[CompareList(index)].SignalLine));
                //Console.WriteLine((decimal)(macd[CompareList(index) - 1].MacdLine - macd[CompareList(index) - 1].SignalLine));
                if ((macd[CompareList(index)].MacdLine - macd[CompareList(index)].SignalLine > 0) && (macd[CompareList(index) - 1].MacdLine - macd[CompareList(index) - 1].SignalLine <= 0))
                {
                    OpenDiff = (decimal)(macd[CompareList(index)].MacdLine - macd[CompareList(index)].SignalLine);
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
                else if ((macd[CompareList(index)].MacdLine - macd[CompareList(index)].SignalLine < 0) & (macd[CompareList(index) - 1].MacdLine - macd[CompareList(index) - 1].SignalLine >= 0))
                {
                    OpenDiff = (decimal) (macd[CompareList(index)].MacdLine - macd[CompareList(index)].SignalLine);
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
                    OpenDiff = (decimal)(macd[CompareList(index)].MacdLine - macd[CompareList(index)].SignalLine);
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

        //public List<Quote> StockDetails;
        //public int ClosePeriodCount;
        //public int emaPeriodCount1;
        //public int emaPeriodCount2;
        //public int demPeriodCount;

        //public MovingAverageConvergenceDivergence(List<Quote> stock, int close, int ema1, int ema2, int dem)
        //{
        //    this.StockDetails = stock;
        //    this.ClosePeriodCount = close;
        //    this.emaPeriodCount1 = ema1;
        //    this.emaPeriodCount2 = ema2;
        //    this.demPeriodCount = dem;
        //}

        //public decimal GetAveragePrice(int index, int count)
        //{
        //    decimal AveragePrice = 0;
        //    decimal ClosePrice = 0;
        //    int Interval = index / ClosePeriodCount;
        //    if (Interval < count) AveragePrice = 0;
        //    else
        //    {
        //        for (int i = Interval - count; i < Interval; i++)
        //        {
        //            OHLC ohlc = new OHLC(StockDetails, ClosePeriodCount, i * ClosePeriodCount);
        //            ClosePrice = ClosePrice + ohlc.GetClosePrice();
        //        }
        //        AveragePrice = ClosePrice / count;
        //    }
        //    return AveragePrice;
        //}

        //public decimal MACD(int index)
        //{
        //    decimal MACD;
        //    int Interval = index / ClosePeriodCount;
        //    if(Interval < emaPeriodCount2) MACD = 0;
        //    else
        //    {
        //        decimal Short = GetAveragePrice(Interval - emaPeriodCount1, emaPeriodCount1);
        //        decimal Long = GetAveragePrice(Interval - emaPeriodCount2, emaPeriodCount2);
        //        MACD = Short - Long;
        //    }
        //    return MACD;
        //}

        //public decimal Signal(int index)
        //{
        //    decimal signal;
        //    int Interval = index / ClosePeriodCount;
        //    if (Interval < emaPeriodCount2) signal = 0;
        //    else signal = GetAveragePrice(Interval - demPeriodCount, demPeriodCount);
        //    return signal;
        //}

        //public decimal Diff(int index)
        //{
        //    decimal Diff;
        //    int Interval = index / ClosePeriodCount;
        //    if (Interval < emaPeriodCount2) Diff = 0;
        //    else Diff = MACD(Interval) - Signal(Interval);
        //    return Diff;
        //}


    }
}
