using Common.Logger;
using EMSLib.Connector;
using EMSLib.MarketData;
using EMSMessageType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using QH_Indicator;

namespace Strategy
{
    class MainFunction
    {
        static void Main(string[] args)
        {
            StrategyManager manager = new StrategyManager();
            string side = Console.ReadLine();

            
        }
    }

    public class StrategyManager : IQuoteSubscriber
    {
        private EMSConnector emsConnector;
        private Logger logger;

        private int emsId;
        private string username;
        private string password;

        private int emaPeriodCount1 = 0;
        private int emaPeriodCount2 = 0;
        private int demPeriodCount = 0;
        private decimal macd = 0;
        private decimal price = 0;
        private int position = 0;

        private bool isFinished = false;
        private string newFileName = "I:\\TradeRecord.csv";


        private DateTime StopTime = new DateTime(2019, 4, 1, 15, 0, 0);

        //private List<Quote> test = new List<Quote>();
        private List<KeyValuePair<DateTime, Quote>> test = new List<KeyValuePair<DateTime, Quote>>();
        private List<KeyValuePair<DateTime, decimal>> testOpenList = new List<KeyValuePair<DateTime, decimal>>();
        private List<KeyValuePair<DateTime, decimal>> testHighList = new List<KeyValuePair<DateTime, decimal>>();
        private List<KeyValuePair<DateTime, decimal>> testLowList = new List<KeyValuePair<DateTime, decimal>>();
        private List<KeyValuePair<DateTime, decimal>> testCloseList = new List<KeyValuePair<DateTime, decimal>>();

        private List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>> testOHLCList = new List<KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>>();


        /*-----PARAM----*/

        /*-----PARAM----*/
        private List<Order> pendingOrderList = new List<Order>();

        public StrategyManager()
        {
            logger = new Logger("StrategyManager");
            
            emsId = Properties.Settings.Default.EMSId;
            username = Properties.Settings.Default.UseName;
            password = Properties.Settings.Default.Password;

            emaPeriodCount1 = Properties.Settings.Default.ema1;
            emaPeriodCount2 = Properties.Settings.Default.ema2;
            demPeriodCount = Properties.Settings.Default.dem;
            macd = Properties.Settings.Default.macd;
            price = Properties.Settings.Default.price;
            position = Properties.Settings.Default.position;

            /*-----Init pendOrderList----*/
            //for (int i = 0; i < 100; i++) pendingOrderList.Add(null);
            /*-----Init pendOrderList----*/
            
            Console.WriteLine("Init EMS");
            logger.Info("Init EMS");
            emsConnector = new EMSConnector();
            emsConnector.Init(emsId);
            Dictionary<string, bool> tempDict;
            
            Console.WriteLine("EMS login successfully");     // Subscribe Order Update
            logger.Info("EMS login successfully");
            emsConnector.Login(username, password, new List<string>(), out tempDict);     // EMS Login

            //using (var entity = new DEMOEntities())
            //{
            //    var sec = entity.INSTRUMENT.ToList();
            //    foreach (var Inst in sec)
            //    {
            //        if (Inst.ExpiryDate != null)
            //        {
            //            DateTime ExpiryDay = DateTime.Parse(Inst.ExpiryDate);
            //            DateTime Expiry1 = new DateTime(2019, 01, 23);
            //            //DateTime Expiry2 = new DateTime(2019, 02, 27);
            //            //DateTime Expiry3 = new DateTime(2019, 03, 27);
            //            if ((ExpiryDay == Expiry1)) instList.Add(Inst);
            //        }

            //    }
            //    //get instruments;   filter expiryday;
            //}

            //if (emsConnector.SubscribeQuoteMarketData("BACKTEST", 126, this, "2019/01/02-2019/01/04"))        // Subscribe Market Data
            //{
            //    Console.WriteLine("EMS Subscribe Market Data Successfully!");
            //    logger.Info("EMS Subscribe Market Data Successfully!");
            //}

            if (emsConnector.SubscribeQuoteMarketData("BACKTEST", 222, this, "2019/04/01"))        // Subscribe Market Data
            {
                Console.WriteLine("EMS Subscribe Market Data Successfully!");
                logger.Info("EMS Subscribe Market Data Successfully!");
            }

            //foreach (var Inst in instList)
            //{
            //    if (emsConnector.SubscribeQuoteMarketData("BACKTEST", Inst.InstrumentId, this, "2019/01/02-2019/01/03"))        // subscribe market data
            //    {
            //        Console.WriteLine("EMS Subscribe Market Data Successfully!");
            //        logger.Info("EMS Subscribe Market Data Successfully!");
            //    }
            //}

            //foreach(KeyValuePair<DateTime, List<Quote>> tm in TimeLine)
            //{

            //    Quote k = tm.Value.Single(x => x.InstrumentId == 1);
            //    ETFLastPrice = (double) k.Last;
            //    foreach(var inst in instList)
            //    {
            //        foreach (var it in tm.Value)
            //        {
            //            if(it.InstrumentId == inst.InstrumentId)
            //            {
            //                if (OptDict.ContainsKey(inst.InstrumentId))
            //                {
            //                    OptDict[inst.InstrumentId] = it;
            //                    CalculateTradeDate day = new CalculateTradeDate();
            //                    DateTime End = DateTime.Parse(inst.ExpiryDate);
            //                    DateTime Start = tm.Key;
            //                    double Remain = day.GetRemainingDays(Start, End);
            //                    BlackSholes bs = new BlackSholes(inst.CallPut, ETFLastPrice, (double)inst.Strike, (double)Remain / TradingDays, r, div, 0.175 + VolSmile(ETFLastPrice, (double)inst.Strike) / 100);
            //                    QHDict[inst.InstrumentId] = bs;
            //                }
            //                else
            //                {
            //                    OptDict.Add(inst.InstrumentId, it);
            //                    CalculateTradeDate day = new CalculateTradeDate();
            //                    DateTime End = DateTime.Parse(inst.ExpiryDate);
            //                    DateTime Start = tm.Key;
            //                    double Remain = day.GetRemainingDays(Start, End);
            //                    BlackSholes bs = new BlackSholes(inst.CallPut, ETFLastPrice, (double)inst.Strike, (double)Remain / TradingDays, r, div, 0.175 + VolSmile(ETFLastPrice, (double)inst.Strike) / 100);
            //                    QHDict.Add(inst.InstrumentId, bs);
            //                }
            //            }
            //        }
            //    }

            //}

            //INSTRUMENT inst = null;
            //using (var entity = new DEMOEntities())
            //{
            //    inst = entity.INSTRUMENT.Single(x => x.InstrumentId == 126);
            //}
            //Console.WriteLine(inst.CallPut + " " + inst.ExpiryDate + " " + inst.Strike);
            while(!isFinished)
            {
                //Console.WriteLine("Insert Data .....");
            }

            OHLC ohlc = new OHLC(test, 20);
            testOpenList = ohlc.GetClosePriceList();
            testHighList = ohlc.GetHighPriceList();
            testLowList = ohlc.GetLowPriceList();
            testCloseList = ohlc.GetClosePriceList();

            for(int i = 0; i < testOpenList.Count(); i++)
            {
                KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)> pair = new KeyValuePair<DateTime, (decimal, decimal, decimal, decimal)>(testOpenList[i].Key, (testOpenList[i].Value, testHighList[i].Value, testLowList[i].Value, testCloseList[i].Value));
                testOHLCList.Add(pair);
            }
            

            for (int i = 20 * emaPeriodCount2; i < test.Count() - 1; i++)
            {
                MovingAverageConvergenceDivergence MACD = new MovingAverageConvergenceDivergence(test, testOHLCList, 20, emaPeriodCount1, emaPeriodCount2, demPeriodCount);
                position = MACD.macdTradingRule(i, macd, price, position, newFileName, StopTime);
            }

            decimal k = 0;
        }

        public void QuoteUpdated(Quote quote)
        {

            Console.WriteLine(quote.UpdateTime + " " + quote.Exchange + ": " + quote.Symbol + ", ask: " + quote.BestAsk + ", bid: " + quote.BestBid);
            logger.Info(quote.UpdateTime + " " + quote.Exchange + ": " + quote.Symbol + ", ask: " + quote.BestAsk + ", bid: " + quote.BestBid);

            if ((quote.InstrumentId == 222) & (quote.UpdateTime < StopTime) & (isFinished == false))
            {
                KeyValuePair<DateTime, Quote> pair = new KeyValuePair<DateTime, Quote>(quote.UpdateTime, quote);
                test.Add(pair);
            }
            else
            {
                Console.WriteLine("Insert Data Finished");
                isFinished = true;
            }


            //var macd = QH_IndicatorUtils.MACD(null, 12, 24, 9);
            //decimal macdline = (decimal) macd[1].MacdLine;



            if (!File.Exists(newFileName))
            {
                string Header = "UpdateDate" + "," + "Symbol" + "," + "BestBid" + "," + "BestBidQuantity" + "," + "BestAsk" + "," + "BestAskQuantity" + "," + "BidPriceQueue" + "," + "BidQuantityQueue" + ","
                                    + "AskPriceQueue" + "," + "AskQuantityQueue" + "," + "Price" + "," + "Side" + "," + "Position" + "," + "Indicator";
                StringBuilder csvHeader = new StringBuilder();
                csvHeader.AppendLine(Header);
                File.WriteAllText(newFileName, csvHeader.ToString());
            }

        }

        public void BarUpdated(Bar bar)
        {

            Console.WriteLine("InstrumentId : " + bar.InstrumentId + " Symbol : " + bar.Symbol + ", Amount: " + bar.Amount + ", Volume: " + bar.Volume);
            logger.Info("InstrumentId : " + bar.InstrumentId + " Symbol : " + bar.Symbol + ", Amount: " + bar.Amount + ", Volume: " + bar.Volume);

        }



    }
}
