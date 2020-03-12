using MerageTable.asharetable;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MerageTable
{
    class SuperTableService
    {
        bool istoday = false;
        string date;
        int nCount = 0;
        private static object countobj = new object();
        String connetStrRead;
        String connetStrWrite;
        private Dictionary<string,int> m_CompleteList;
        private Dictionary<string, int> m_ErrorList;
        private AutoResetEvent m_pASHAREDESCRIPTIONEvent; 
        private AutoResetEvent m_pASHARECALENDAREvent;
        private AutoResetEvent m_pAshareIndustriesCodeMapEvent;
        private AutoResetEvent m_pAINDEXEODPRICESEvent;
        private AutoResetEvent m_pInsertEvent;
        private Dictionary<string, CHANGEWINDCODE> m_ChangeCodeDict;
        private Dictionary<string ,int> m_OldChangeCodeDict;
        private Dictionary<string ,ASHAREDESCRIPTION>      m_listWINCODE;
        private Dictionary<string, int> m_dictAshareIndustriesCodeMap;
        private Dictionary<string, ASHARECALENDAR> m_dictASHARECALENDAR;

         List<ASHARESUPERTABLE> m_Dictstsupertable;
        private Dictionary<string, AINDEXEODPRICES> m_dictAINDEXEODPRICESSH50;
        private Dictionary<string, AINDEXEODPRICES> m_dictAINDEXEODPRICESHS300;
        private Dictionary<string, AINDEXEODPRICES> m_dictAINDEXEODPRICESCS500;
        private Dictionary<string, AINDEXEODPRICES> m_dictAINDEXEODPRICESCS1000;
        private Dictionary<string, AINDEXEODPRICES> m_dictAINDEXEODPRICESMKT;
        private static object write = new object();
        List<string> Begintime;
        List<string> m_sqlList;
        public SuperTableService()
        {
            m_sqlList = new List<string>();
            m_CompleteList = new Dictionary<string, int>();
            m_ErrorList = new Dictionary<string, int>();
            m_OldChangeCodeDict = new Dictionary<string, int>();
            m_ChangeCodeDict = new Dictionary<string, CHANGEWINDCODE>();
            m_Dictstsupertable = new List<ASHARESUPERTABLE>();
            m_listWINCODE = new Dictionary<string, ASHAREDESCRIPTION>();
            m_dictAshareIndustriesCodeMap = new Dictionary<string, int>();          
            m_dictASHARECALENDAR = new Dictionary<string, ASHARECALENDAR>();
           
            m_dictAINDEXEODPRICESSH50 = new Dictionary<string, AINDEXEODPRICES>();
            m_dictAINDEXEODPRICESHS300 = new Dictionary<string, AINDEXEODPRICES>();
            m_dictAINDEXEODPRICESCS500 = new Dictionary<string, AINDEXEODPRICES>();
            m_dictAINDEXEODPRICESCS1000 = new Dictionary<string, AINDEXEODPRICES>();
            m_dictAINDEXEODPRICESMKT = new Dictionary<string, AINDEXEODPRICES>();

          
            this.m_pInsertEvent = new AutoResetEvent(false);
            Begintime = new List<string>();

            Begintime.Add("200512");
            Begintime.Add("2006");
            Begintime.Add("2007");
            Begintime.Add("2008");
            Begintime.Add("2009");
            Begintime.Add("2010");
            Begintime.Add("2011");
            Begintime.Add("2012");
            Begintime.Add("2013");
            Begintime.Add("2014");
            Begintime.Add("2015");
            Begintime.Add("2016");
            Begintime.Add("2017");
            Begintime.Add("2018");
            Begintime.Add("2019");

            GetCompeleteList();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(SetCompeleteList);
            timer.Interval = 60000;
            timer.Enabled = true;
            timer.Start();

            System.Timers.Timer Errortimer = new System.Timers.Timer();
            Errortimer.Elapsed += new ElapsedEventHandler(SetErrorList);
            Errortimer.Interval = 30000;
            Errortimer.Enabled = true;
            Errortimer.Start();


            //System.Timers.Timer sql = new System.Timers.Timer();
            //sql.Elapsed += new ElapsedEventHandler(SetSqlList);
            //sql.Interval = 30000;
            //sql.Enabled = true;
            //sql.Start();
            //Thread Insertthread = new Thread(InsertSuperTable);
            //Insertthread.Start();
            //m_pInsertEvent.Set();
        }

        private void SetCompeleteList(object sender, ElapsedEventArgs e)
        {
           
            using (FileStream fs = new FileStream("Complist.txt", FileMode.Create, FileAccess.Write))
            {
                StringBuilder strBuilder = new StringBuilder();
                lock (m_CompleteList)
                {
                   
                    foreach (var windcode in m_CompleteList)
                    {
                        strBuilder.Append(windcode.Key).Append("|");
                    }

                }
                byte[] writeMesaage = Encoding.Default.GetBytes(strBuilder.ToString());
                fs.Write(writeMesaage, 0, writeMesaage.Length);
                fs.Close();
            }

        }

        private void SetSqlList(object sender, ElapsedEventArgs e)
        {

            using (FileStream fs = new FileStream("Sql.txt", FileMode.Append, FileAccess.Write))
            {
                StringBuilder strBuilder = new StringBuilder();
                lock (m_ErrorList)
                {

                    foreach (var windcode in m_sqlList)
                    {
                        strBuilder.Append(windcode).Append(";/r/n");
                    }

                }
              
                byte[] writeMesaage = Encoding.Default.GetBytes(strBuilder.ToString());
                fs.Write(writeMesaage, 0, writeMesaage.Length);
                fs.Close();
            }

        }
        private void SetErrorList(object sender, ElapsedEventArgs e)
        {

            using (FileStream fs = new FileStream("Error.txt", FileMode.Create, FileAccess.Write))
            {
                StringBuilder strBuilder = new StringBuilder();
                lock (m_ErrorList)
                {

                    foreach (var windcode in m_ErrorList)
                    {
                        strBuilder.Append(windcode.Key).Append("|");
                    }

                }
                byte[] writeMesaage = Encoding.Default.GetBytes(strBuilder.ToString());
                fs.Write(writeMesaage, 0, writeMesaage.Length);
                fs.Close();
            }

        }

        private void SetChangecodeList()
        {
            using (FileStream fs = new FileStream("Changecode.txt", FileMode.Truncate, FileAccess.Write))
            {
                StringBuilder strBuilder = new StringBuilder();

                foreach (var windcode in m_ChangeCodeDict)
                {
                    strBuilder.Append(windcode.Key).Append('|');
                }

                byte[] writeMesaage = Encoding.Default.GetBytes(strBuilder.ToString());
                fs.Write(writeMesaage, 0, writeMesaage.Length);
                fs.Close();
            }

        }
        private void GetChangecodeList()
        {
            using (FileStream fs = new FileStream("Changecode.txt", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[fs.Length];
                fs.Position = 0;
                fs.Read(buffer, 0, buffer.Length);

                string strContent = Encoding.Default.GetString(buffer);
                if (!string.IsNullOrEmpty(strContent))
                {
                    List<string> temStr = strContent.Split('|').ToList();
                    foreach (var str in temStr)
                    {
                        if (!m_OldChangeCodeDict.ContainsKey(str))
                            m_OldChangeCodeDict.Add(str, 1);
                    }
                }
            }

        }

        private void SetDiffChangecodeList()
        {
            List<string> windcodelist = new List<string>();
            foreach (var windcode in m_ChangeCodeDict)
            {
                if (!m_OldChangeCodeDict.ContainsKey(windcode.Key))
                {
                    windcodelist.Add(windcode.Key);
                }
            }
            if (windcodelist.Count == 0)
                return;
            using (FileStream fs = new FileStream("Differchangecode"+DateTime.Now.ToString("yyyyMMdd")+".txt", FileMode.Open, FileAccess.Write))
            {
                StringBuilder strBuilder = new StringBuilder();

                foreach (var code in windcodelist)
                {
                   strBuilder.Append(code).Append('|');
                }

                byte[] writeMesaage = Encoding.Default.GetBytes(strBuilder.ToString());
                fs.Write(writeMesaage, 0, writeMesaage.Length);
                fs.Close();
            }

        }
        private void GetCompeleteList()
        {
            using (FileStream fs = new FileStream("Complist.txt", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[fs.Length];
                fs.Position = 0;
                fs.Read(buffer, 0, buffer.Length);

                string strContent = Encoding.Default.GetString(buffer);
                if (!string.IsNullOrEmpty(strContent))
                {
                    List<string> temStr = strContent.Split('|').ToList();
                    foreach(var str in temStr)
                    {
                        if (!m_CompleteList.ContainsKey(str))
                            m_CompleteList.Add(str,1);
                    }          
                }
            }

        }
        public bool Init( )
        {
            bool bsuccess = false;


            connetStrRead = "";

            connetStrWrite = "";
            MySqlConnection conn = new MySqlConnection(connetStrRead);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(0) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='WIND' AND TABLE_NAME= 'ASHARESUPERTABLE'", conn);
                cmd.ExecuteNonQuery();
                bsuccess = true;
                Console.WriteLine("当前连接参数为 {0}", connetStrRead);
                conn.Close();

                GetChildTableDict();
                GetChangecodeList();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1146:
                        Console.WriteLine("supertable表不存在，请手动创建！");
                        break;
                    default:
                        break;
                }
               
            }
            finally
            {
                conn.Close();
            }

            istoday = true;
          
            date = DateTime.Now.ToString("yyyyMMdd");
            return bsuccess;
        }

      

        public void Start(string begindate = "")
        {
            m_pASHAREDESCRIPTIONEvent.WaitOne();
            m_pASHARECALENDAREvent.WaitOne();
            m_pAshareIndustriesCodeMapEvent.WaitOne();
            m_pAINDEXEODPRICESEvent.WaitOne();
          

            Console.WriteLine("开始处理supertable表 ");

           
            if(!m_dictASHARECALENDAR.ContainsKey(date))
            {
                Console.WriteLine("{0} 不是交易日", date);
                return;
            }

            foreach (var wincode in m_listWINCODE)
            {

            
               // if (compair.Contains(wincode.Value.S_INFO_WINDCODE))
                {

                    //处理所有数据
                    // DealSuperTable(wincode.Value);


                    //if (m_listWINCODE.ContainsKey(wincode.Key.Trim()))
                    //{
                    //    DealTDSuperTable(m_listWINCODE[wincode.Key.Trim()], wincode.Value.Trim());
                    //}

                  //处理当天数据
                    DealTDSuperTable(wincode.Value, date);
             


               }
           // }
            }
            foreach( var changecode in m_ChangeCodeDict)
            {
                Console.WriteLine("CHANGEWINDCODE:{0} {1} {2}", changecode.Value.S_INFO_OLDWINDCODE, changecode.Value.S_INFO_NEWWINDCODE,changecode.Value.CHANGE_DATE);
            }
            SetChangecodeList();
            SetDiffChangecodeList();
            Console.ReadLine();
        }

     
        //处理今天数据
        private void DealTDSuperTable(ASHAREDESCRIPTION wincode,string trade_dt)
        {
            int count = Interlocked.Increment(ref nCount); ;

            List<ASHARESUPERTABLE> lstsupertable = new List<ASHARESUPERTABLE>();
            Dictionary<string, List<ASHAREST>> m_dictASHAREST = new Dictionary<string, List<ASHAREST>>();
            Dictionary<string, List<ASHAREINDUSTRIESCLASSCITICS>> m_dictASHAREINDUSTRIESCLASSCITICS = new Dictionary<string, List<ASHAREINDUSTRIESCLASSCITICS>>();
            Dictionary<string, Dictionary<string, ASHARETRADINGSUSPENSION>> m_dictASHARETRADINGSUSPENSION = new Dictionary<string, Dictionary<string, ASHARETRADINGSUSPENSION>>();
            Dictionary<string, ASHAREL2INDICATORS> m_dictASHAREL2INDICATORS = new Dictionary<string, ASHAREL2INDICATORS>();
            Dictionary<string, ASHAREEODPRICES> m_dictASHAREEODPRICES = new Dictionary<string, ASHAREEODPRICES>();
            Dictionary<string, ASHAREMONEYFLOW> m_dictASHAREMONEYFLOW = new Dictionary<string, ASHAREMONEYFLOW>();
            Dictionary<string, ASHAREEODDERIVATIVEINDICATOR> m_dictASHAREEODDERIVATIVEINDICATOR = new Dictionary<string, ASHAREEODDERIVATIVEINDICATOR>();
            Dictionary<string, ASHARESUPERTABLE> m_lstsupertable = new Dictionary<string, ASHARESUPERTABLE>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXSH50 = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXHS300 = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXCS500 = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXCS1000 = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXMKT = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            MySqlConnection conn = new MySqlConnection(connetStrRead);

            DealSTDict(conn, wincode.S_INFO_WINDCODE, ref m_dictASHAREST);
            DealSuspendDict(conn, wincode.S_INFO_WINDCODE, ref m_dictASHARETRADINGSUSPENSION);
            DealClassciticsDict(conn, wincode.S_INFO_WINDCODE, ref m_dictASHAREINDUSTRIESCLASSCITICS);
           
            if (!DealASHAREEODPRICES(wincode.S_INFO_WINDCODE, ref m_dictASHAREEODPRICES))
                return;
            if (!DealASHAREMONEYFLOW(wincode.S_INFO_WINDCODE, ref m_dictASHAREMONEYFLOW))
                return;
            if (!DealASHAREL2INDICATORS(wincode.S_INFO_WINDCODE, ref m_dictASHAREL2INDICATORS))
                return;

            if (!DealASHAREEODDERIVATIVEINDICATOR(wincode.S_INFO_WINDCODE, ref m_dictASHAREEODDERIVATIVEINDICATOR))
                return;

            if (!DealAINDEX(wincode.S_INFO_WINDCODE, ref m_dictAINDEXSH50, ref m_dictAINDEXHS300, ref m_dictAINDEXCS500, ref m_dictAINDEXCS1000, ref m_dictAINDEXMKT))
                return;
            string predate = GetLastPreviousTradingDay(trade_dt);


            ASHARESUPERTABLE supertable = new ASHARESUPERTABLE();
            ASHARESUPERTABLE presupertable;
            supertable.S_INFO_WINDCODE = wincode.S_INFO_WINDCODE;
            supertable.TRADE_DT = trade_dt;

            int td = Convert.ToInt32(trade_dt);
            //判断时间
            int listdate = Convert.ToInt32(wincode.S_INFO_LISTDATE);
            int delistdate = Convert.ToInt32(wincode.S_INFO_DELISTDATE);
            if (listdate == 0)//未上市
               return;

            if (delistdate == 0)
            {

                if (listdate > td)
                {

                    return;
                }
            }
            else
            {
                if (listdate > td || delistdate <= td)
                {
                    return;
                }
            }
            //取前一天的supertable
            presupertable = new ASHARESUPERTABLE();
            if (predate == "")
            {

            }
            else
            {
                if (!m_lstsupertable.ContainsKey(predate))
                {
                    DealPreSuperTableForASHAREEODPRICES(conn, wincode.S_INFO_WINDCODE, predate, ref presupertable);
                }
                else
                {
                    presupertable = m_lstsupertable[predate];
                }
            }
            //数据处理
            if (m_dictASHAREEODPRICES.ContainsKey(trade_dt))
            {
                supertable.objASHAREEODPRICES = m_dictASHAREEODPRICES[trade_dt];
                
                //supertable.isDeal = false;
                Dictionary<string, ASHARETRADINGSUSPENSION> dictRest_Trading_Suspension;
                if (m_dictASHARETRADINGSUSPENSION.TryGetValue(wincode.S_INFO_WINDCODE, out dictRest_Trading_Suspension))
                {
                    if (dictRest_Trading_Suspension.ContainsKey(trade_dt))
                    {
                        if (m_dictASHAREEODDERIVATIVEINDICATOR.ContainsKey(trade_dt)) //停牌了
                        {
                            supertable.objASHAREEODDERIVATIVEINDICATOR = m_dictASHAREEODDERIVATIVEINDICATOR[trade_dt];
                            if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0)
                                supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV;
                            if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY == 0)
                                supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY;
                        }
                        else
                        {
                            
                            supertable.objASHAREEODDERIVATIVEINDICATOR = presupertable.objASHAREEODDERIVATIVEINDICATOR;
                        }
                        supertable.SUSPEND_FLAG = 1;
                        supertable.S_DQ_SUSPENDTYPE = dictRest_Trading_Suspension[trade_dt].S_DQ_SUSPENDTYPE;
                        
                        if (m_dictASHAREL2INDICATORS.ContainsKey(trade_dt))
                        {
                            supertable.objASHAREL2INDICATORS = m_dictASHAREL2INDICATORS[trade_dt];
                        }

                        if (m_dictASHAREMONEYFLOW.ContainsKey(trade_dt))
                        {
                            supertable.objASHAREMONEYFLOW = m_dictASHAREMONEYFLOW[trade_dt];
                        }


                    }
                    else
                    {
                        //不在停牌时间内
                        if (wincode.S_INFO_WINDCODE == "000863.SZ" && td == 20070515)  //停牌表里没有，eodprice有记录但是停牌了
                        {
                            supertable.SUSPEND_FLAG = 1;
                        }
                        if (wincode.S_INFO_WINDCODE == "000403.SZ" && (td >= 20070427 && td <= 20070518))  //这些eodprice有记录但是停牌了
                        {
                            supertable.SUSPEND_FLAG = 1;
                        }
                        else
                            supertable.SUSPEND_FLAG = 0;
                        supertable.S_DQ_SUSPENDTYPE = 1;

                        if (m_dictASHAREL2INDICATORS.ContainsKey(trade_dt))
                        {
                            supertable.objASHAREL2INDICATORS = m_dictASHAREL2INDICATORS[trade_dt];
                        }
                       
                        if (m_dictASHAREEODDERIVATIVEINDICATOR.ContainsKey(trade_dt))
                        {
                            supertable.objASHAREEODDERIVATIVEINDICATOR = m_dictASHAREEODDERIVATIVEINDICATOR[trade_dt];

                            if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0)
                                supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV;
                            if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY == 0)
                                supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY;
                        }
                        else
                        {
                            supertable.objASHAREEODDERIVATIVEINDICATOR = presupertable.objASHAREEODDERIVATIVEINDICATOR;
                        }

                        if (m_dictASHAREMONEYFLOW.ContainsKey(trade_dt))
                        {
                            supertable.objASHAREMONEYFLOW = m_dictASHAREMONEYFLOW[trade_dt];
                        }
                      
                    }
                }
                else
                {
                    //不在停牌时间内
                    supertable.SUSPEND_FLAG = 0;
                    supertable.S_DQ_SUSPENDTYPE = 1;
                    if (m_dictASHAREL2INDICATORS.ContainsKey(trade_dt))
                    {
                        supertable.objASHAREL2INDICATORS = m_dictASHAREL2INDICATORS[trade_dt];
                    }
                   

                    if (m_dictASHAREEODDERIVATIVEINDICATOR.ContainsKey(trade_dt))
                    {
                        supertable.objASHAREEODDERIVATIVEINDICATOR = m_dictASHAREEODDERIVATIVEINDICATOR[trade_dt];

                        if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0)
                            supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV;
                        if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY == 0)
                            supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY;
                    }
                    else
                    {
                        supertable.objASHAREEODDERIVATIVEINDICATOR = presupertable.objASHAREEODDERIVATIVEINDICATOR;
                    }

                    if (m_dictASHAREMONEYFLOW.ContainsKey(trade_dt))
                    {
                        supertable.objASHAREMONEYFLOW = m_dictASHAREMONEYFLOW[trade_dt];
                    }
                   
                }
            }
            else
            { 
                //没有eodprice
                //先认定为停牌
                supertable.objASHAREEODPRICES.S_DQ_PRECLOSE = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;
                supertable.objASHAREEODPRICES.S_DQ_OPEN = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;
                supertable.objASHAREEODPRICES.S_DQ_HIGH = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;
                supertable.objASHAREEODPRICES.S_DQ_LOW = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;
                supertable.objASHAREEODPRICES.S_DQ_CLOSE = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;

                supertable.objASHAREEODPRICES.S_DQ_ADJPRECLOSE = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                supertable.objASHAREEODPRICES.S_DQ_ADJOPEN = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                supertable.objASHAREEODPRICES.S_DQ_ADJHIGH = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                supertable.objASHAREEODPRICES.S_DQ_ADJLOW = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                supertable.objASHAREEODPRICES.S_DQ_ADJCLOSE = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                supertable.objASHAREEODPRICES.S_DQ_ADJFACTOR = presupertable.objASHAREEODPRICES.S_DQ_ADJFACTOR;
                supertable.objASHAREEODPRICES.S_DQ_AVGPRICE = presupertable.objASHAREEODPRICES.S_DQ_AVGPRICE;

                if (m_dictASHAREEODDERIVATIVEINDICATOR.ContainsKey(trade_dt))
                {
                    supertable.objASHAREEODDERIVATIVEINDICATOR = m_dictASHAREEODDERIVATIVEINDICATOR[trade_dt];

                    if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0)
                        supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV;
                    if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY == 0)
                        supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY;
                }
                else
                {
                    supertable.objASHAREEODDERIVATIVEINDICATOR = presupertable.objASHAREEODDERIVATIVEINDICATOR;
                }

                supertable.SUSPEND_FLAG = 1;
                supertable.S_DQ_SUSPENDTYPE = presupertable.S_DQ_SUSPENDTYPE;

            }

            //ST
            List<ASHAREST> lstRest;
            if (m_dictASHAREST.TryGetValue(wincode.S_INFO_WINDCODE, out lstRest))
            {
                foreach (var rest in lstRest)
                {
                    int entrydt = Convert.ToInt32(rest.ENTRY_DT);
                    int removedt = Convert.ToInt32(rest.REMOVE_DT);
                    if (removedt == 0)
                    {
                        if (entrydt <= td)
                        {
                            supertable.ST_FLAG = 1;
                            break;
                        }
                        else
                        {
                            supertable.ST_FLAG = 0;
                        }
                    }
                    else
                    {
                        if (entrydt <= td && td < removedt)
                        {
                            supertable.ST_FLAG = 1;
                            break;
                        }
                        else
                        {
                            supertable.ST_FLAG = 0;
                        }
                    }
                }

            }
            else
            {
                supertable.ST_FLAG = 0;

            }
            //行业
            List<ASHAREINDUSTRIESCLASSCITICS> lstIndustries;
            if (m_dictASHAREINDUSTRIESCLASSCITICS.TryGetValue(wincode.S_INFO_WINDCODE, out lstIndustries))
            {
                foreach (var Industrie in lstIndustries)
                {
                    int entrydt = Convert.ToInt32(Industrie.ENTRY_DT);
                    int removedt = Convert.ToInt32(Industrie.REMOVE_DT);
                    if (removedt == 0 || (entrydt <= td && td < removedt))
                    {
                        int code = 0;
                        string CITICS_IND_CODE = Industrie.CITICS_IND_CODE.Substring(0, 4);
                        if (m_dictAshareIndustriesCodeMap.TryGetValue(CITICS_IND_CODE, out code))
                            supertable.CITICS_IND_CODE = code;
                        else
                            supertable.CITICS_IND_CODE = 0;
                        break;
                    }

                }
            }
            //指数权重
            if (!m_dictAINDEXSH50.ContainsKey(trade_dt))
                supertable.objAINDEXSH50.I_WEIGHT = presupertable.objAINDEXSH50.I_WEIGHT;
            else
                supertable.objAINDEXSH50.I_WEIGHT = m_dictAINDEXSH50[trade_dt].I_WEIGHT;

            if (!m_dictAINDEXHS300.ContainsKey(trade_dt))
                supertable.objAINDEXHS300.I_WEIGHT = presupertable.objAINDEXHS300.I_WEIGHT;
            else
                supertable.objAINDEXHS300.I_WEIGHT = m_dictAINDEXHS300[trade_dt].I_WEIGHT;

            if (!m_dictAINDEXCS500.ContainsKey(trade_dt))
                supertable.objAINDEXCS500.I_WEIGHT = presupertable.objAINDEXCS500.I_WEIGHT;
            else
                supertable.objAINDEXCS500.I_WEIGHT = m_dictAINDEXCS500[trade_dt].I_WEIGHT;

            if (!m_dictAINDEXCS1000.ContainsKey(trade_dt))
                supertable.objAINDEXCS1000.I_WEIGHT = presupertable.objAINDEXCS1000.I_WEIGHT;
            else
                supertable.objAINDEXCS1000.I_WEIGHT = m_dictAINDEXCS1000[trade_dt].I_WEIGHT;

            if (!m_dictAINDEXMKT.ContainsKey(trade_dt))
                supertable.objAINDEXMKT.I_WEIGHT = presupertable.objAINDEXMKT.I_WEIGHT;
            else
                supertable.objAINDEXMKT.I_WEIGHT = m_dictAINDEXMKT[trade_dt].I_WEIGHT;

            if (m_dictAINDEXEODPRICESSH50.ContainsKey(trade_dt))
                supertable.objAINDEXEODPRICESSH50 = m_dictAINDEXEODPRICESSH50[trade_dt];

            if (m_dictAINDEXEODPRICESHS300.ContainsKey(trade_dt))
                supertable.objAINDEXEODPRICESHS300 = m_dictAINDEXEODPRICESHS300[trade_dt];

            if (m_dictAINDEXEODPRICESCS500.ContainsKey(trade_dt))
                supertable.objAINDEXEODPRICESCS500 = m_dictAINDEXEODPRICESCS500[trade_dt];

            if (m_dictAINDEXEODPRICESCS1000.ContainsKey(trade_dt))
                supertable.objAINDEXEODPRICESCS1000 = m_dictAINDEXEODPRICESCS1000[trade_dt];

            if (m_dictAINDEXEODPRICESMKT.ContainsKey(trade_dt))
                supertable.objAINDEXEODPRICESMKT = m_dictAINDEXEODPRICESMKT[trade_dt];

            m_lstsupertable.Add(trade_dt, supertable);
            lstsupertable.Add(supertable);

            if (m_lstsupertable.Count == 0)
            {
                Console.WriteLine("不用处理 {0} {1}.", wincode.S_INFO_WINDCODE, count);

                lock (m_CompleteList)
                {
                    if (!m_CompleteList.ContainsKey(wincode.S_INFO_WINDCODE))
                        m_CompleteList.Add(wincode.S_INFO_WINDCODE, 1);
                }
            }
            //写表
            if (m_lstsupertable.Count > 0)
            {
                Console.WriteLine("{0}写表开始{1}条.", wincode.S_INFO_WINDCODE, m_lstsupertable.Count);
                InsertSuperTable(m_lstsupertable);
                Console.WriteLine("{0}写表成功{1}. ", wincode.S_INFO_WINDCODE, count);

                lock (m_CompleteList)
                {
                    if (!m_CompleteList.ContainsKey(wincode.S_INFO_WINDCODE))
                        m_CompleteList.Add(wincode.S_INFO_WINDCODE, 1);
                }
                Console.WriteLine("处理完成{0} {1}", wincode.S_INFO_WINDCODE, count);

                m_dictASHAREEODPRICES.Clear();
                m_dictASHAREMONEYFLOW.Clear();
                m_dictASHAREEODDERIVATIVEINDICATOR.Clear();
                m_dictASHAREL2INDICATORS.Clear();

                m_dictAINDEXSH50.Clear();
                m_dictAINDEXHS300.Clear();
                m_dictAINDEXCS500.Clear();
                m_dictAINDEXCS1000.Clear();
                m_dictAINDEXMKT.Clear();
            }
        }
        //处理所有数据，逻辑和上面相同，因为之前没做读写分离读线程太少，有时候会崩溃。按年查，2015年12月数据是底子，不保证正确性
        private void DealSuperTable(ASHAREDESCRIPTION wincode)
        {
            int count = Interlocked.Increment(ref nCount); ;

            List<ASHARESUPERTABLE> lstsupertable = new List<ASHARESUPERTABLE>();
            Dictionary <string, List<ASHAREST>> m_dictASHAREST = new Dictionary<string, List<ASHAREST>>();
            Dictionary<string, List<ASHAREINDUSTRIESCLASSCITICS>> m_dictASHAREINDUSTRIESCLASSCITICS = new Dictionary<string, List<ASHAREINDUSTRIESCLASSCITICS>>();
            Dictionary<string, Dictionary<string, ASHARETRADINGSUSPENSION>> m_dictASHARETRADINGSUSPENSION = new Dictionary<string, Dictionary<string, ASHARETRADINGSUSPENSION>>();
            Dictionary<string, ASHAREL2INDICATORS> m_dictASHAREL2INDICATORS = new Dictionary<string, ASHAREL2INDICATORS>();
            Dictionary<string, ASHAREEODPRICES> m_dictASHAREEODPRICES = new Dictionary<string, ASHAREEODPRICES>();
            Dictionary<string, ASHAREMONEYFLOW> m_dictASHAREMONEYFLOW = new Dictionary<string, ASHAREMONEYFLOW>();
            Dictionary<string, ASHAREEODDERIVATIVEINDICATOR> m_dictASHAREEODDERIVATIVEINDICATOR = new Dictionary<string, ASHAREEODDERIVATIVEINDICATOR>();
            Dictionary<string, ASHARESUPERTABLE>  m_lstsupertable = new Dictionary<string, ASHARESUPERTABLE>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXSH50 = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXHS300 = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXCS500 = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXCS1000 = new Dictionary<string, AINDEXHS300FREEWEIGHT>();
            Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXMKT = new Dictionary<string, AINDEXHS300FREEWEIGHT>();

            Console.WriteLine("开始处理 {0} {1}",wincode.S_INFO_WINDCODE, count);
            MySqlConnection conn = new MySqlConnection(connetStrRead);

            DealSTDict(conn, wincode.S_INFO_WINDCODE, ref m_dictASHAREST);
            DealSuspendDict(conn, wincode.S_INFO_WINDCODE, ref m_dictASHARETRADINGSUSPENSION);
            DealClassciticsDict(conn, wincode.S_INFO_WINDCODE, ref m_dictASHAREINDUSTRIESCLASSCITICS);

            if (!DealASHAREEODPRICES( wincode.S_INFO_WINDCODE, ref m_dictASHAREEODPRICES))
                return;
            if (!DealASHAREMONEYFLOW(wincode.S_INFO_WINDCODE, ref m_dictASHAREMONEYFLOW))
                return;
            if (!DealASHAREL2INDICATORS(wincode.S_INFO_WINDCODE, ref m_dictASHAREL2INDICATORS))
                return;

            if (!DealASHAREEODDERIVATIVEINDICATOR(wincode.S_INFO_WINDCODE, ref m_dictASHAREEODDERIVATIVEINDICATOR))
                return;

            if (!DealAINDEX( wincode.S_INFO_WINDCODE, ref m_dictAINDEXSH50, ref m_dictAINDEXHS300, ref m_dictAINDEXCS500, ref m_dictAINDEXCS1000, ref m_dictAINDEXMKT))
                return;
          
            foreach (var trade_dt in m_dictASHARECALENDAR.Keys)
            {
                string predate = GetLastPreviousTradingDay(trade_dt);

                
                ASHARESUPERTABLE supertable = new ASHARESUPERTABLE();
                ASHARESUPERTABLE presupertable;
                supertable.S_INFO_WINDCODE = wincode.S_INFO_WINDCODE;
                supertable.TRADE_DT = trade_dt;

                int td = Convert.ToInt32(trade_dt);
 
                int listdate = Convert.ToInt32(wincode.S_INFO_LISTDATE);
                int delistdate = Convert.ToInt32(wincode.S_INFO_DELISTDATE);
                if (listdate == 0)//未上市
                    break;

                if (delistdate == 0)
                {
                  
                    if (listdate > td)
                    {
                      
                        continue;
                    }
                }
                else
                {
                    if (listdate > td || delistdate <= td)
                    {
                        
                        continue;
                    }
                }

                presupertable = new ASHARESUPERTABLE();
                if (predate == "")
                {

                }
                else
                {
                    if (!m_lstsupertable.ContainsKey(predate))
                    {
                        DealPreSuperTableForASHAREEODPRICES(conn, wincode.S_INFO_WINDCODE, predate, ref presupertable);

                        if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0)
                            supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV;
                        if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY == 0)
                            supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY;
                    }
                    else
                    {
                        presupertable = m_lstsupertable[predate];
                    }
                } 

                if (m_dictASHAREEODPRICES.ContainsKey(trade_dt))
                {
                    supertable.objASHAREEODPRICES = m_dictASHAREEODPRICES[trade_dt];
                    //supertable.isDeal = false;
                    Dictionary<string, ASHARETRADINGSUSPENSION> dictRest_Trading_Suspension;
                    if (m_dictASHARETRADINGSUSPENSION.TryGetValue(wincode.S_INFO_WINDCODE, out dictRest_Trading_Suspension))
                    {
                        if (dictRest_Trading_Suspension.ContainsKey(trade_dt))
                        {
                            
                            if (m_dictASHAREEODDERIVATIVEINDICATOR.ContainsKey(trade_dt))
                            {
                               
                                supertable.objASHAREEODDERIVATIVEINDICATOR = m_dictASHAREEODDERIVATIVEINDICATOR[trade_dt];

                                if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0)
                                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV;
                                if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY == 0)
                                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY;
                            }
                            else
                            {
                                supertable.objASHAREEODDERIVATIVEINDICATOR = presupertable.objASHAREEODDERIVATIVEINDICATOR;
                            }
                            supertable.SUSPEND_FLAG = 1;
                            supertable.S_DQ_SUSPENDTYPE = dictRest_Trading_Suspension[trade_dt].S_DQ_SUSPENDTYPE;

                            if (m_dictASHAREL2INDICATORS.ContainsKey(trade_dt))
                            {
                                supertable.objASHAREL2INDICATORS = m_dictASHAREL2INDICATORS[trade_dt];
                            }

                            if (m_dictASHAREMONEYFLOW.ContainsKey(trade_dt))
                            {
                                supertable.objASHAREMONEYFLOW = m_dictASHAREMONEYFLOW[trade_dt];
                            }
                           

                        }
                        else
                        {
                            if (wincode.S_INFO_WINDCODE == "000863.SZ" && td == 20070515)  //停牌表里没有，eodprice有记录但是停牌了
                            {
                                supertable.SUSPEND_FLAG = 1;
                                supertable.S_DQ_SUSPENDTYPE = presupertable.S_DQ_SUSPENDTYPE;
                            }
                            else if (wincode.S_INFO_WINDCODE == "000403.SZ" && (td >= 20070427 && td <= 20070518))  //这些eodprice有记录但是停牌了
                            {
                                supertable.SUSPEND_FLAG = 1;
                                supertable.S_DQ_SUSPENDTYPE = presupertable.S_DQ_SUSPENDTYPE;
                            }
                            else if (wincode.S_INFO_WINDCODE == "002099.SZ" && td == 20071226)
                            {
                                supertable.SUSPEND_FLAG = 1;
                                supertable.S_DQ_SUSPENDTYPE = presupertable.S_DQ_SUSPENDTYPE;
                            }
                            else
                            {
                                supertable.SUSPEND_FLAG = 0;
                                supertable.S_DQ_SUSPENDTYPE = 1;
                            }
                            if (m_dictASHAREL2INDICATORS.ContainsKey(trade_dt))
                            {
                                supertable.objASHAREL2INDICATORS = m_dictASHAREL2INDICATORS[trade_dt];
                            }

                            if (m_dictASHAREEODDERIVATIVEINDICATOR.ContainsKey(trade_dt))
                            {
                                supertable.objASHAREEODDERIVATIVEINDICATOR = m_dictASHAREEODDERIVATIVEINDICATOR[trade_dt];

                                if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0)
                                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV;
                                if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY == 0)
                                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY;
                            }
                            else
                            {
                                supertable.objASHAREEODDERIVATIVEINDICATOR = presupertable.objASHAREEODDERIVATIVEINDICATOR;
                            }

                            if (m_dictASHAREMONEYFLOW.ContainsKey(trade_dt))
                            {
                                supertable.objASHAREMONEYFLOW = m_dictASHAREMONEYFLOW[trade_dt];
                            }
                        }
                    }
                    else
                    {
                        supertable.SUSPEND_FLAG = 0;
                        supertable.S_DQ_SUSPENDTYPE = 1;
                        if (m_dictASHAREL2INDICATORS.ContainsKey(trade_dt))
                        {
                            supertable.objASHAREL2INDICATORS = m_dictASHAREL2INDICATORS[trade_dt];
                        }

                        if (m_dictASHAREEODDERIVATIVEINDICATOR.ContainsKey(trade_dt))
                        {
                            supertable.objASHAREEODDERIVATIVEINDICATOR = m_dictASHAREEODDERIVATIVEINDICATOR[trade_dt];

                            if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0)
                                supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV;
                            if (supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY == 0)
                                supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = presupertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY;
                        }

                        if (m_dictASHAREMONEYFLOW.ContainsKey(trade_dt))
                        {
                            supertable.objASHAREMONEYFLOW = m_dictASHAREMONEYFLOW[trade_dt];
                        }
                    }
                }
                else
                {
                    //先认定为停牌
                    supertable.objASHAREEODPRICES.S_DQ_PRECLOSE = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_OPEN = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_HIGH = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_LOW = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_CLOSE = presupertable.objASHAREEODPRICES.S_DQ_CLOSE;

                    supertable.objASHAREEODPRICES.S_DQ_ADJPRECLOSE = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_ADJOPEN = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_ADJHIGH = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_ADJLOW = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_ADJCLOSE = presupertable.objASHAREEODPRICES.S_DQ_ADJCLOSE;
                    supertable.objASHAREEODPRICES.S_DQ_ADJFACTOR = presupertable.objASHAREEODPRICES.S_DQ_ADJFACTOR;
                    supertable.objASHAREEODPRICES.S_DQ_AVGPRICE = presupertable.objASHAREEODPRICES.S_DQ_AVGPRICE;

                    if (m_dictASHAREEODDERIVATIVEINDICATOR.ContainsKey(trade_dt))
                    {
                        supertable.objASHAREEODDERIVATIVEINDICATOR = m_dictASHAREEODDERIVATIVEINDICATOR[trade_dt];
                        if(supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV == 0|| supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY ==0)
                            supertable.objASHAREEODDERIVATIVEINDICATOR = presupertable.objASHAREEODDERIVATIVEINDICATOR;
                    }
                    else
                    {
                        supertable.objASHAREEODDERIVATIVEINDICATOR = presupertable.objASHAREEODDERIVATIVEINDICATOR;
                    }

                    supertable.SUSPEND_FLAG = 1;
                    supertable.S_DQ_SUSPENDTYPE = presupertable.S_DQ_SUSPENDTYPE;

                }


                List<ASHAREST> lstRest;
                if (m_dictASHAREST.TryGetValue(wincode.S_INFO_WINDCODE, out lstRest))
                {
                    foreach (var rest in lstRest)
                    {
                        int entrydt = Convert.ToInt32(rest.ENTRY_DT);
                        int removedt = Convert.ToInt32(rest.REMOVE_DT);
                        if (removedt == 0)
                        {
                            if (entrydt <= td)
                            {
                                supertable.ST_FLAG = 1;
                                break;
                            }
                            else
                            {
                                supertable.ST_FLAG = 0;
                            }
                        }
                        else
                        {
                            if (entrydt <= td && td < removedt)
                            {
                                supertable.ST_FLAG = 1;
                                break;
                            }
                            else
                            {
                                supertable.ST_FLAG = 0;
                            }
                        }
                    }

                }
                else
                {
                    supertable.ST_FLAG = 0;
                }

                List<ASHAREINDUSTRIESCLASSCITICS> lstIndustries;
                if (m_dictASHAREINDUSTRIESCLASSCITICS.TryGetValue(wincode.S_INFO_WINDCODE, out lstIndustries))
                {
                    foreach (var Industrie in lstIndustries)
                    {
                        int entrydt = Convert.ToInt32(Industrie.ENTRY_DT);
                        int removedt = Convert.ToInt32(Industrie.REMOVE_DT);
                        if (removedt == 0 || (entrydt <= td && td < removedt))
                        {
                            int code = 0;
                            string CITICS_IND_CODE = Industrie.CITICS_IND_CODE.Substring(0, 4);
                            if (m_dictAshareIndustriesCodeMap.TryGetValue(CITICS_IND_CODE, out code))
                                supertable.CITICS_IND_CODE = code;
                            else
                                supertable.CITICS_IND_CODE = 0;
                            break;
                        }

                    }
                }

                if (!m_dictAINDEXSH50.ContainsKey(trade_dt))
                    supertable.objAINDEXSH50.I_WEIGHT = presupertable.objAINDEXSH50.I_WEIGHT;
                else
                    supertable.objAINDEXSH50.I_WEIGHT = m_dictAINDEXSH50[trade_dt].I_WEIGHT;

                if (!m_dictAINDEXHS300.ContainsKey(trade_dt))
                    supertable.objAINDEXHS300.I_WEIGHT = presupertable.objAINDEXHS300.I_WEIGHT;
                else
                    supertable.objAINDEXHS300.I_WEIGHT = m_dictAINDEXHS300[trade_dt].I_WEIGHT;

                if (!m_dictAINDEXCS500.ContainsKey(trade_dt))
                    supertable.objAINDEXCS500.I_WEIGHT = presupertable.objAINDEXCS500.I_WEIGHT;
                else
                    supertable.objAINDEXCS500.I_WEIGHT = m_dictAINDEXCS500[trade_dt].I_WEIGHT;

                if (!m_dictAINDEXCS1000.ContainsKey(trade_dt))
                    supertable.objAINDEXCS1000.I_WEIGHT = presupertable.objAINDEXCS1000.I_WEIGHT;
                else
                    supertable.objAINDEXCS1000.I_WEIGHT = m_dictAINDEXCS1000[trade_dt].I_WEIGHT;

                if (!m_dictAINDEXMKT.ContainsKey(trade_dt))
                    supertable.objAINDEXMKT.I_WEIGHT = presupertable.objAINDEXMKT.I_WEIGHT;
                else
                    supertable.objAINDEXMKT.I_WEIGHT = m_dictAINDEXMKT[trade_dt].I_WEIGHT;

                if (m_dictAINDEXEODPRICESSH50.ContainsKey(trade_dt))
                    supertable.objAINDEXEODPRICESSH50 = m_dictAINDEXEODPRICESSH50[trade_dt];

                if (m_dictAINDEXEODPRICESHS300.ContainsKey(trade_dt))
                    supertable.objAINDEXEODPRICESHS300 = m_dictAINDEXEODPRICESHS300[trade_dt];

                if (m_dictAINDEXEODPRICESCS500.ContainsKey(trade_dt))
                    supertable.objAINDEXEODPRICESCS500 = m_dictAINDEXEODPRICESCS500[trade_dt];

                if (m_dictAINDEXEODPRICESCS1000.ContainsKey(trade_dt))
                    supertable.objAINDEXEODPRICESCS1000 = m_dictAINDEXEODPRICESCS1000[trade_dt];

                if (m_dictAINDEXEODPRICESMKT.ContainsKey(trade_dt))
                    supertable.objAINDEXEODPRICESMKT = m_dictAINDEXEODPRICESMKT[trade_dt];

                m_lstsupertable.Add(trade_dt, supertable);
                lstsupertable.Add(supertable);
            }
            if (m_lstsupertable.Count  ==  0)
            {
                Console.WriteLine("不用处理 {0} {1}.", wincode.S_INFO_WINDCODE, count);

                lock (m_CompleteList)
                {
                    if (!m_CompleteList.ContainsKey(wincode.S_INFO_WINDCODE))
                        m_CompleteList.Add(wincode.S_INFO_WINDCODE, 1);
                }
            }
            if (m_lstsupertable.Count > 0)
            {
                Console.WriteLine( "{0}写表开始{1}条.", wincode.S_INFO_WINDCODE, m_lstsupertable.Count);
                InsertSuperTable(m_lstsupertable);
                Console.WriteLine( "{0}写表成功{1}. ", wincode.S_INFO_WINDCODE , count );

                lock (m_CompleteList)
                {
                    if (!m_CompleteList.ContainsKey(wincode.S_INFO_WINDCODE))
                        m_CompleteList.Add(wincode.S_INFO_WINDCODE, 1);
                }
                Console.WriteLine("处理完成{0} {1}",wincode.S_INFO_WINDCODE , count);

                m_dictASHAREEODPRICES.Clear();
                m_dictASHAREMONEYFLOW.Clear();
                m_dictASHAREEODDERIVATIVEINDICATOR.Clear();
                m_dictASHAREL2INDICATORS.Clear();

                m_dictAINDEXSH50.Clear();
                m_dictAINDEXHS300.Clear();
                m_dictAINDEXCS500.Clear();
                m_dictAINDEXCS1000.Clear();
                m_dictAINDEXMKT.Clear();
            }



        }
        //批量写表
        private void InsertSuperTable(Dictionary<string, ASHARESUPERTABLE> lstsupertable)
        {

            string wincode = "";
            MySqlConnection conn = new MySqlConnection(connetStrWrite);
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                //MySqlTransaction tx = conn.BeginTransaction();
                //cmd.Transaction = tx;


                StringBuilder basesql = new StringBuilder();
                basesql.Append("REPLACE INTO `ASHARESUPERTABLE`(`S_INFO_WINDCODE`, `TRADE_DT`, `S_DQ_PRECLOSE`, `S_DQ_OPEN`, `S_DQ_HIGH`, `S_DQ_LOW`, `S_DQ_CLOSE`,");
                basesql.Append(" `S_CHANGE`, `S_PCTCHANGE`, `S_VOLUME`, `S_AMOUNT`, `S_DQ_ADJPRECLOSE`, `S_DQ_ADJOPEN`, `S_DQ_ADJHIGH`, `S_DQ_ADJLOW`, `S_DQ_ADJCLOSE`, `S_DQ_ADJFACTOR`,");
                basesql.Append(" `S_DQ_AVGPRICE`, `S_TRADESTATUSCODE`, `S_VAL_MV`, `S_DQ_MV`, `S_PQ_HIGH_52W_`, `S_PQ_LOW_52W_`, `S_VAL_PE`, `S_VAL_PB_NEW`, `S_VAL_PE_TTM`,");
                basesql.Append(" `S_VAL_PCF_OCF`, `S_VAL_PCF_OCFTTM`, `S_VAL_PCF_NCF`, `S_VAL_PCF_NCFTTM`, `S_VAL_PS`, `S_VAL_PS_TTM`, `S_TURN`, `S_FREETURNOVER`, `TOT_SHR_TODAY`,");
                basesql.Append(" `FLOAT_A_SHR_TODAY`, `S_DQ_CLOSE_TODAY`, `S_PRICE_DIV_DPS`, `S_PQ_ADJHIGH_52W`, `S_PQ_ADJLOW_52W`, `FREE_SHARES_TODAY`, `NET_PROFIT_PARENT_COMP_TTM`,");
                basesql.Append(" `NET_PROFIT_PARENT_COMP_LYR`, `NET_ASSETS_TODAY`, `NET_CASH_FLOWS_OPER_ACT_TTM`, `NET_CASH_FLOWS_OPER_ACT_LYR`, `OPER_REV_TTM`, `OPER_REV_LYR`, ");
                basesql.Append("`NET_INCR_CASH_CASH_EQU_TTM`, `NET_INCR_CASH_CASH_EQU_LYR`, `UP_DOWN_LIMIT_STATUS`, `LOWEST_HIGHEST_STATUS`, `S_LI_INITIATIVEBUYRATE`, `S_LI_INITIATIVEBUYMONEY`,");
                basesql.Append(" `S_LI_INITIATIVEBUYAMOUNT`, `S_LI_INITIATIVESELLRATE`, `S_LI_INITIATIVESELLMONEY`, `S_LI_INITIATIVESELLAMOUNT`, `S_LI_LARGEBUYRATE`, `S_LI_LARGEBUYMONEY`,");
                basesql.Append(" `S_LI_LARGEBUYAMOUNT`, `S_LI_LARGESELLRATE`, `S_LI_LARGESELLMONEY`, `S_LI_LARGESELLAMOUNT`, `S_LI_ENTRUSTRATE`, `S_LI_ENTRUDIFFERAMOUNT`, `S_LI_ENTRUDIFFERAMONEY`,");
                basesql.Append(" `S_LI_ENTRUSTBUYMONEY`, `S_LI_ENTRUSTSELLMONEY`, `S_LI_ENTRUSTBUYAMOUNT`, `S_LI_ENTRUSTSELLAMOUNT`, `BUY_VALUE_EXLARGE_ORDER`, `SELL_VALUE_EXLARGE_ORDER`,");
                basesql.Append(" `BUY_VALUE_LARGE_ORDER`, `SELL_VALUE_LARGE_ORDER`, `BUY_VALUE_MED_ORDER`, `SELL_VALUE_MED_ORDER`, `BUY_VALUE_SMALL_ORDER`, `SELL_VALUE_SMALL_ORDER`,");
                basesql.Append(" `BUY_VOLUME_EXLARGE_ORDER`, `SELL_VOLUME_EXLARGE_ORDER`, `BUY_VOLUME_LARGE_ORDER`, `SELL_VOLUME_LARGE_ORDER`, `BUY_VOLUME_MED_ORDER`, `SELL_VOLUME_MED_ORDER`,");
                basesql.Append(" `BUY_VOLUME_SMALL_ORDER`, `SELL_VOLUME_SMALL_ORDER`, `TRADES_COUNT`, `BUY_TRADES_EXLARGE_ORDER`, `SELL_TRADES_EXLARGE_ORDER`, `BUY_TRADES_LARGE_ORDER`,");
                basesql.Append(" `SELL_TRADES_LARGE_ORDER`, `BUY_TRADES_MED_ORDER`, `SELL_TRADES_MED_ORDER`, `BUY_TRADES_SMALL_ORDER`, `SELL_TRADES_SMALL_ORDER`, `VOLUME_DIFF_SMALL_TRADER`,");
                basesql.Append(" `VOLUME_DIFF_SMALL_TRADER_ACT`, `VOLUME_DIFF_MED_TRADER`, `VOLUME_DIFF_MED_TRADER_ACT`, `VOLUME_DIFF_LARGE_TRADER`, `VOLUME_DIFF_LARGE_TRADER_ACT`,");
                basesql.Append(" `VOLUME_DIFF_INSTITUTE`, `VOLUME_DIFF_INSTITUTE_ACT`, `VALUE_DIFF_SMALL_TRADER`, `VALUE_DIFF_SMALL_TRADER_ACT`, `VALUE_DIFF_MED_TRADER`, `VALUE_DIFF_MED_TRADER_ACT`,");
                basesql.Append(" `VALUE_DIFF_LARGE_TRADER`, `VALUE_DIFF_LARGE_TRADER_ACT`, `VALUE_DIFF_INSTITUTE`, `VALUE_DIFF_INSTITUTE_ACT`, `S_MFD_INFLOWVOLUME`, `NET_INFLOW_RATE_VOLUME`,");
                basesql.Append(" `S_MFD_INFLOW_OPENVOLUME`, `OPEN_NET_INFLOW_RATE_VOLUME`, `S_MFD_INFLOW_CLOSEVOLUME`, `CLOSE_NET_INFLOW_RATE_VOLUME`, `S_MFD_INFLOW`, `NET_INFLOW_RATE_VALUE`,");
                basesql.Append(" `S_MFD_INFLOW_OPEN`, `OPEN_NET_INFLOW_RATE_VALUE`, `S_MFD_INFLOW_CLOSE`, `CLOSE_NET_INFLOW_RATE_VALUE`, `TOT_VOLUME_BID`, `TOT_VOLUME_ASK`, `MONEYFLOW_PCT_VOLUME`,");
                basesql.Append(" `OPEN_MONEYFLOW_PCT_VOLUME`, `CLOSE_MONEYFLOW_PCT_VOLUME`, `MONEYFLOW_PCT_VALUE`, `OPEN_MONEYFLOW_PCT_VALUE`, `CLOSE_MONEYFLOW_PCT_VALUE`,");
                basesql.Append(" `S_MFD_INFLOWVOLUME_LARGE_ORDER`, `NET_INFLOW_RATE_VOLUME_L`, `S_MFD_INFLOW_LARGE_ORDER`, `NET_INFLOW_RATE_VALUE_L`, `MONEYFLOW_PCT_VOLUME_L`, `MONEYFLOW_PCT_VALUE_L`,");
                basesql.Append(" `S_MFD_INFLOW_OPENVOLUME_L`, `OPEN_NET_INFLOW_RATE_VOLUME_L`, `S_MFD_INFLOW_OPEN_LARGE_ORDER`, `OPEN_NET_INFLOW_RATE_VALUE_L`, `OPEN_MONEYFLOW_PCT_VOLUME_L`,");
                basesql.Append(" `OPEN_MONEYFLOW_PCT_VALUE_L`, `S_MFD_INFLOW_CLOSEVOLUME_L`, `CLOSE_NET_INFLOW_RATE_VOLUME_L`, `S_MFD_INFLOW_CLOSE_LARGE_ORDER`, `CLOSE_NET_INFLOW_RATE_VALU_L`,");
                basesql.Append(" `CLOSE_MONEYFLOW_PCT_VOLUME_L`, `CLOSE_MONEYFLOW_PCT_VALUE_L`, `BUY_VALUE_EXLARGE_ORDER_ACT`, `SELL_VALUE_EXLARGE_ORDER_ACT`, `BUY_VALUE_LARGE_ORDER_ACT`,");
                basesql.Append(" `SELL_VALUE_LARGE_ORDER_ACT`, `BUY_VALUE_MED_ORDER_ACT`, `SELL_VALUE_MED_ORDER_ACT`, `BUY_VALUE_SMALL_ORDER_ACT`, `SELL_VALUE_SMALL_ORDER_ACT`,");
                basesql.Append(" `BUY_VOLUME_EXLARGE_ORDER_ACT`, `SELL_VOLUME_EXLARGE_ORDER_ACT`, `BUY_VOLUME_LARGE_ORDER_ACT`, `SELL_VOLUME_LARGE_ORDER_ACT`, `BUY_VOLUME_MED_ORDER_ACT`,");
                basesql.Append(" `SELL_VOLUME_MED_ORDER_ACT`, `BUY_VOLUME_SMALL_ORDER_ACT`, `SELL_VOLUME_SMALL_ORDER_ACT`, `CITICS_IND_CODE`, `SUSPEND_FLAG`, `S_SUSPENDTYPE`, `ST_FLAG`, `SH50_I_WEIGHT`,");
                basesql.Append(" `SH50_S_DQ_PRECLOSE`, `SH50_S_DQ_OPEN`, `SH50_S_DQ_HIGH`, `SH50_S_DQ_LOW`, `SH50_S_DQ_CLOSE`, `SH50_S_CHANGE`, `SH50_S_PCTCHANGE`, `SH50_S_VOLUME`, ");
                basesql.Append("`SH50_S_AMOUNT`, `HS300_I_WEIGHT`, `HS300_S_DQ_PRECLOSE`, `HS300_S_DQ_OPEN`, `HS300_S_DQ_HIGH`, `HS300_S_DQ_LOW`, `HS300_S_DQ_CLOSE`, `HS300_S_CHANGE`,");
                basesql.Append(" `HS300_S_PCTCHANGE`, `HS300_S_VOLUME`, `HS300_S_AMOUNT`, `CS500_I_WEIGHT`, `CS500_S_DQ_PRECLOSE`, `CS500_S_DQ_OPEN`, `CS500_S_DQ_HIGH`,");
                basesql.Append(" `CS500_S_DQ_LOW`, `CS500_S_DQ_CLOSE`, `CS500_S_CHANGE`, `CS500_S_PCTCHANGE`, `CS500_S_VOLUME`, `CS500_S_AMOUNT`, `CS1000_I_WEIGHT`, ");
                basesql.Append("`CS1000_S_DQ_PRECLOSE`, `CS1000_S_DQ_OPEN`, `CS1000_S_DQ_HIGH`, `CS1000_S_DQ_LOW`, `CS1000_S_DQ_CLOSE`, `CS1000_S_CHANGE`, `CS1000_S_PCTCHANGE`,");
                basesql.Append(" `CS1000_S_VOLUME`, `CS1000_S_AMOUNT`, `MKT_I_WEIGHT`, `MKT_S_DQ_PRECLOSE`, `MKT_S_DQ_OPEN`, `MKT_S_DQ_HIGH`, `MKT_S_DQ_LOW`, `MKT_S_DQ_CLOSE`,");
                basesql.Append(" `MKT_S_CHANGE`, `MKT_S_PCTCHANGE`, `MKT_S_VOLUME`, `MKT_S_AMOUNT`) VALUES ");


                try
                {
                    StringBuilder insertsql = new StringBuilder();
                    insertsql.Append(basesql.ToString());
                    int count = 0;
                    int succcount = 0;
                    foreach (var supertable in lstsupertable)
                    {

                        DealInsertSQL(supertable.Value, ref insertsql);

                        count++;
                        if (lstsupertable.Count == 1)
                        {
                            //Console.WriteLine("写表成功 {0}。", insertsql);
                            cmd.CommandText = insertsql.ToString();
                            cmd.ExecuteNonQuery();

                            //lock (m_sqlList)
                            //{ m_sqlList.Add(supertable.Value.S_INFO_WINDCODE); }
                            // tx.Commit();
                            conn.Close();
                            succcount++;
                            break;
                        }

                        if (count % 1000 == 0 || succcount + count == (lstsupertable.Count))
                        {
                            cmd.CommandText = insertsql.ToString();
                            cmd.ExecuteNonQuery();
                           // tx.Commit();
                            succcount += count;
                            Console.WriteLine("写表成功 {0} {1}  条 。", supertable.Value.S_INFO_WINDCODE, succcount);
                            wincode = supertable.Value.S_INFO_WINDCODE;
                            count = 0;

                           // tx = conn.BeginTransaction();
                            insertsql.Clear();
                            insertsql.Append(basesql.ToString());
                        }
                        else
                        {
                            insertsql.Append(",");
                        }
                    }

                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("写表异常：" + ex);
                    lock (m_ErrorList)
                    {
                        if (wincode != "")
                            m_ErrorList.Add(wincode, 1);
                    }
                }
                finally
                {

                }


            }

        }

      
        private void DealInsertSQL(ASHARESUPERTABLE supertable, ref StringBuilder insertsql)
        {
            insertsql.Append("('").Append(supertable.S_INFO_WINDCODE).Append("', '").Append(supertable.TRADE_DT).Append("', ")
            .Append(supertable.objASHAREEODPRICES.S_DQ_PRECLOSE).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_OPEN).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_HIGH).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_LOW).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_CLOSE).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_CHANGE).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_PCTCHANGE).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_VOLUME).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_AMOUNT).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_ADJPRECLOSE).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_ADJOPEN).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_ADJHIGH).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_ADJLOW).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_ADJCLOSE).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_ADJFACTOR).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_AVGPRICE).Append("," )
            .Append(supertable.objASHAREEODPRICES.S_DQ_TRADESTATUSCODE).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_MV).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_HIGH_52W_).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_LOW_52W_).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PB_NEW).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE_TTM).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCF).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCFTTM).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCF).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCFTTM).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS_TTM).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_TURN).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_FREETURNOVER).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.TOT_SHR_TODAY).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.FLOAT_A_SHR_TODAY).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_PRICE_DIV_DPS).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJHIGH_52W).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJLOW_52W).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.FREE_SHARES_TODAY).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_TTM).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_LYR).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.NET_ASSETS_TODAY).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_TTM).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_LYR).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.OPER_REV_TTM).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.OPER_REV_LYR).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_TTM).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_LYR).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.UP_DOWN_LIMIT_STATUS).Append("," )
            .Append(supertable.objASHAREEODDERIVATIVEINDICATOR.LOWEST_HIGHEST_STATUS).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_INITIATIVEBUYRATE).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_INITIATIVEBUYMONEY).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_INITIATIVEBUYAMOUNT).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_INITIATIVESELLRATE).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_INITIATIVESELLMONEY).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_INITIATIVESELLAMOUNT).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_LARGEBUYRATE).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_LARGEBUYMONEY).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_LARGEBUYAMOUNT).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_LARGESELLRATE).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_LARGESELLMONEY).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_LARGESELLAMOUNT).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTRATE).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_ENTRUDIFFERAMOUNT).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_ENTRUDIFFERAMONEY).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTBUYMONEY).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTSELLMONEY).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTBUYAMOUNT).Append("," )
            .Append(supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTSELLAMOUNT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VALUE_EXLARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VALUE_EXLARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VALUE_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VALUE_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VALUE_MED_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VALUE_MED_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VALUE_SMALL_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VALUE_SMALL_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VOLUME_EXLARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VOLUME_EXLARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VOLUME_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VOLUME_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VOLUME_MED_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VOLUME_MED_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VOLUME_SMALL_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VOLUME_SMALL_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.TRADES_COUNT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_TRADES_EXLARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_TRADES_EXLARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_TRADES_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_TRADES_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_TRADES_MED_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_TRADES_MED_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_TRADES_SMALL_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_TRADES_SMALL_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VOLUME_DIFF_SMALL_TRADER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VOLUME_DIFF_SMALL_TRADER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VOLUME_DIFF_MED_TRADER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VOLUME_DIFF_MED_TRADER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VOLUME_DIFF_LARGE_TRADER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VOLUME_DIFF_LARGE_TRADER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VOLUME_DIFF_INSTITUTE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VOLUME_DIFF_INSTITUTE_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VALUE_DIFF_SMALL_TRADER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VALUE_DIFF_SMALL_TRADER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VALUE_DIFF_MED_TRADER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VALUE_DIFF_MED_TRADER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VALUE_DIFF_LARGE_TRADER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VALUE_DIFF_LARGE_TRADER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VALUE_DIFF_INSTITUTE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.VALUE_DIFF_INSTITUTE_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOWVOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.NET_INFLOW_RATE_VOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_OPENVOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSEVOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.NET_INFLOW_RATE_VALUE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_OPEN).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VALUE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VALUE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.TOT_VOLUME_BID).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.TOT_VOLUME_ASK).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.MONEYFLOW_PCT_VOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VOLUME).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.MONEYFLOW_PCT_VALUE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VALUE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VALUE).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOWVOLUME_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.NET_INFLOW_RATE_VOLUME_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.NET_INFLOW_RATE_VALUE_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.MONEYFLOW_PCT_VOLUME_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.MONEYFLOW_PCT_VALUE_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_OPENVOLUME_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VOLUME_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_OPEN_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VALUE_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VOLUME_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VALUE_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSEVOLUME_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VOLUME_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSE_LARGE_ORDER).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VALU_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VOLUME_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VALUE_L).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VALUE_EXLARGE_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VALUE_EXLARGE_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VALUE_LARGE_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VALUE_LARGE_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VALUE_MED_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VALUE_MED_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VALUE_SMALL_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VALUE_SMALL_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VOLUME_EXLARGE_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VOLUME_EXLARGE_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VOLUME_LARGE_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VOLUME_LARGE_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VOLUME_MED_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VOLUME_MED_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.BUY_VOLUME_SMALL_ORDER_ACT).Append("," )
            .Append(supertable.objASHAREMONEYFLOW.SELL_VOLUME_SMALL_ORDER_ACT).Append("," )
            .Append(supertable.CITICS_IND_CODE).Append("," )
            .Append(supertable.SUSPEND_FLAG).Append("," )
            .Append(supertable.S_DQ_SUSPENDTYPE).Append("," )
            .Append(supertable.ST_FLAG).Append("," )
            .Append(supertable.objAINDEXSH50.I_WEIGHT).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_PRECLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_OPEN).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_HIGH).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_LOW).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_CLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_CHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_PCTCHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_VOLUME).Append("," )
            .Append(supertable.objAINDEXEODPRICESSH50.S_DQ_AMOUNT).Append("," )
            .Append(supertable.objAINDEXHS300.I_WEIGHT).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_PRECLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_OPEN).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_HIGH).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_LOW).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_CLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_CHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_PCTCHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_VOLUME).Append("," )
            .Append(supertable.objAINDEXEODPRICESHS300.S_DQ_AMOUNT).Append("," )
            .Append(supertable.objAINDEXCS500.I_WEIGHT).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_PRECLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_OPEN).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_HIGH).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_LOW).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_CLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_CHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_PCTCHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_VOLUME).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS500.S_DQ_AMOUNT).Append("," )
            .Append(supertable.objAINDEXCS1000.I_WEIGHT).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_PRECLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_OPEN).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_HIGH).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_LOW).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_CLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_CHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_PCTCHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_VOLUME).Append("," )
            .Append(supertable.objAINDEXEODPRICESCS1000.S_DQ_AMOUNT).Append("," )
            .Append(supertable.objAINDEXMKT.I_WEIGHT).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_PRECLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_OPEN).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_HIGH).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_LOW).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_CLOSE).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_CHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_PCTCHANGE).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_VOLUME).Append("," )
            .Append(supertable.objAINDEXEODPRICESMKT.S_DQ_AMOUNT).Append(")");
        }

        //ST表字典获取
        private void DealSTDict(MySqlConnection conn, string wincode,ref Dictionary<string, List<ASHAREST>> m_dictASHAREST)
        {
            //MySqlConnection conn = new MySqlConnection(connetStrRead);
            
            try
            {
                conn.Open();
                string sql = "select * from ASHAREST  where S_INFO_WINDCODE = '" + wincode + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
               
                while (reader.Read())
                {
                    ASHAREST objASHAREST = new ASHAREST();

                    objASHAREST.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    objASHAREST.ENTRY_DT = reader.IsDBNull(3) ? "0" : reader.GetString(3);
                    objASHAREST.REMOVE_DT = reader.IsDBNull(4) ? "0" : reader.GetString(4);
                    objASHAREST.ANN_DT = reader.IsDBNull(5) ? "0" : reader.GetString(5);
                    if (m_dictASHAREST.ContainsKey(objASHAREST.S_INFO_WINDCODE))
                    {
                        m_dictASHAREST[objASHAREST.S_INFO_WINDCODE].Add(objASHAREST);
                    }
                    else
                    {
                        List<ASHAREST> lstASHAREST = new List<ASHAREST>();
                        lstASHAREST.Add(objASHAREST);

                        m_dictASHAREST.Add(objASHAREST.S_INFO_WINDCODE, lstASHAREST);
                    }

                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("查询ASHAREST异常：" + ex);
                lock (m_ErrorList)
                {
                    if (wincode != "")
                        m_ErrorList.Add(wincode, 1);
                }
            }
            finally
            {
                conn.Close();
            }
        }
        //行业字典获取
        private void DealClassciticsDict(MySqlConnection conn,string wincode,ref  Dictionary<string, List<ASHAREINDUSTRIESCLASSCITICS>> m_dictASHAREINDUSTRIESCLASSCITICS)
        {
            //MySqlConnection conn = new MySqlConnection(connetStrRead);
            try
            {
                conn.Open();
                string sql = "select * from ASHAREINDUSTRIESCLASSCITICS where S_INFO_WINDCODE = '" + wincode + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    ASHAREINDUSTRIESCLASSCITICS objASHAREINDUSTRIESCLASSCITICS = new ASHAREINDUSTRIESCLASSCITICS();
                    objASHAREINDUSTRIESCLASSCITICS.WIND_CODE = reader.IsDBNull(1) ? "" : reader.GetString("WIND_CODE");
                    objASHAREINDUSTRIESCLASSCITICS.S_INFO_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_INFO_WINDCODE");
                    objASHAREINDUSTRIESCLASSCITICS.CITICS_IND_CODE = reader.IsDBNull(3) ? "" : reader.GetString("CITICS_IND_CODE"); // 中信行业代码 
                    objASHAREINDUSTRIESCLASSCITICS.ENTRY_DT = reader.IsDBNull(4) ? "0" : reader.GetString("ENTRY_DT");        // 纳入日期 
                    objASHAREINDUSTRIESCLASSCITICS.REMOVE_DT = reader.IsDBNull(5) ? "0" : reader.GetString("REMOVE_DT");    // 剔除日期 
                    if (m_dictASHAREINDUSTRIESCLASSCITICS.ContainsKey(objASHAREINDUSTRIESCLASSCITICS.S_INFO_WINDCODE))
                    {
                        m_dictASHAREINDUSTRIESCLASSCITICS[objASHAREINDUSTRIESCLASSCITICS.S_INFO_WINDCODE].Add(objASHAREINDUSTRIESCLASSCITICS);
                    }
                    else
                    {
                        List<ASHAREINDUSTRIESCLASSCITICS> lstASHAREINDUSTRIESCLASSCITICS = new List<ASHAREINDUSTRIESCLASSCITICS>();
                        lstASHAREINDUSTRIESCLASSCITICS.Add(objASHAREINDUSTRIESCLASSCITICS);

                        m_dictASHAREINDUSTRIESCLASSCITICS.Add(objASHAREINDUSTRIESCLASSCITICS.S_INFO_WINDCODE, lstASHAREINDUSTRIESCLASSCITICS);
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("查询ASHARETRADINGSUSPENSION异常：" + ex);
                lock (m_ErrorList)
                {
                    if (wincode != "")
                        m_ErrorList.Add(wincode, 1);
                }
            }
            finally
            {
                conn.Close();
            }
        }
        //停牌字典获取
        private void DealSuspendDict(MySqlConnection conn,string wincode,ref Dictionary<string, Dictionary<string, ASHARETRADINGSUSPENSION>> m_dictASHARETRADINGSUSPENSION)
        {
            //MySqlConnection conn = new MySqlConnection(connetStrRead);
            try
            {
                conn.Open();
                string sql = "select * from ASHARETRADINGSUSPENSION where S_INFO_WINDCODE = '" + wincode+ "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ASHARETRADINGSUSPENSION objASHARETRADINGSUSPENSION = new ASHARETRADINGSUSPENSION();

                    objASHARETRADINGSUSPENSION.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    objASHARETRADINGSUSPENSION.S_DQ_SUSPENDDATE = reader.IsDBNull(2) ? "0" : reader.GetString(2);
                    objASHARETRADINGSUSPENSION.S_DQ_SUSPENDTYPE = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                    objASHARETRADINGSUSPENSION.S_DQ_RESUMPDATE = reader.IsDBNull(4) ? "0" : reader.GetString(4);
                    objASHARETRADINGSUSPENSION.S_DQ_TIME = reader.IsDBNull(6) ? "0" : reader.GetString(6);

                    Dictionary<string, ASHARETRADINGSUSPENSION> temp;
                    if (m_dictASHARETRADINGSUSPENSION.TryGetValue(objASHARETRADINGSUSPENSION.S_INFO_WINDCODE,out temp))
                    {
                        temp.Add(objASHARETRADINGSUSPENSION.S_DQ_SUSPENDDATE,objASHARETRADINGSUSPENSION);
                    }
                    else
                    {
                        Dictionary<string, ASHARETRADINGSUSPENSION> dictASHAREST = new Dictionary<string, ASHARETRADINGSUSPENSION>();
                        dictASHAREST.Add(objASHARETRADINGSUSPENSION.S_DQ_SUSPENDDATE,objASHARETRADINGSUSPENSION);

                        m_dictASHARETRADINGSUSPENSION.Add(objASHARETRADINGSUSPENSION.S_INFO_WINDCODE, dictASHAREST);
                    }

                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("查询ASHARETRADINGSUSPENSION异常：" + ex);
                lock (m_ErrorList)
                {
                    if (wincode != "")
                        m_ErrorList.Add(wincode, 1);
                }
            }
            finally
            {
                conn.Close();
            }
        }

        private string GetLastPreviousTradingDay(string date)
        {
            ASHARECALENDAR temp;
            if (date == "20060104")
                return "20051230";
            if (m_dictASHARECALENDAR.TryGetValue(date, out temp))
            {
                return temp.PRETRADE_DAYS;
            }
            return "";
        }

        private bool DealASHAREEODPRICES(string wincode,ref Dictionary<string, ASHAREEODPRICES> m_dictASHAREEODPRICES)
        {
            MySqlConnection conn = new MySqlConnection(connetStrRead);

            Console.WriteLine("查询ASHAREEODPRICES查到了：" + wincode + date);
            if (istoday)
            {
                conn.Open();
                string sql;
                try
                {
                    sql = "select * from ASHAREEODPRICES where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT = '" + date + "' order by TRADE_DT;";
                    //sql = "select * from ASHAREEODPRICES where S_INFO_WINDCODE = '" + wincode + "' order by TRADE_DT;";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ASHAREEODPRICES objASHAREEODPRICES = new ASHAREEODPRICES();

                        objASHAREEODPRICES.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        objASHAREEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        objASHAREEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                        objASHAREEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                        objASHAREEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                        objASHAREEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                        objASHAREEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                        objASHAREEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                        objASHAREEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                        objASHAREEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                        objASHAREEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);
                        objASHAREEODPRICES.S_DQ_ADJPRECLOSE = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);
                        objASHAREEODPRICES.S_DQ_ADJOPEN = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14);
                        objASHAREEODPRICES.S_DQ_ADJHIGH = reader.IsDBNull(15) ? 0 : reader.GetDecimal(15);
                        objASHAREEODPRICES.S_DQ_ADJLOW = reader.IsDBNull(16) ? 0 : reader.GetDecimal(16);
                        objASHAREEODPRICES.S_DQ_ADJCLOSE = reader.IsDBNull(17) ? 0 : reader.GetDecimal(17);
                        objASHAREEODPRICES.S_DQ_ADJFACTOR = reader.IsDBNull(18) ? 0 : reader.GetDecimal(18);
                        objASHAREEODPRICES.S_DQ_AVGPRICE = reader.IsDBNull(19) ? 0 : reader.GetDecimal(19);
                        objASHAREEODPRICES.S_DQ_TRADESTATUS = reader.IsDBNull(20) ? "" : reader.GetString(20);
                        objASHAREEODPRICES.S_DQ_TRADESTATUSCODE = reader.IsDBNull(21) ? 0 : reader.GetDecimal(21);

                        Console.WriteLine("查询ASHAREEODPRICES查到了：" + wincode + objASHAREEODPRICES.S_DQ_CLOSE);
                        m_dictASHAREEODPRICES.Add(objASHAREEODPRICES.TRADE_DT, objASHAREEODPRICES);
                    }

                   
                }
                catch (MySqlException ex)
                {

                    Console.WriteLine("查询ASHAREEODPRICES异常：" + ex);
                    lock (m_ErrorList)
                    {
                        if (wincode != "")
                            m_ErrorList.Add(wincode, 1);
                    }
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
               

                foreach (var begindate in Begintime)
                {
                    conn.Open();
                    string sql;
                    try
                    {
                        sql = "select * from ASHAREEODPRICES where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT like '" + begindate + "%' order by TRADE_DT;";
                        //sql = "select * from ASHAREEODPRICES where S_INFO_WINDCODE = '" + wincode + "' order by TRADE_DT;";

                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            ASHAREEODPRICES objASHAREEODPRICES = new ASHAREEODPRICES();

                            objASHAREEODPRICES.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            objASHAREEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            objASHAREEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                            objASHAREEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                            objASHAREEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                            objASHAREEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                            objASHAREEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                            objASHAREEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                            objASHAREEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                            objASHAREEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                            objASHAREEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);
                            objASHAREEODPRICES.S_DQ_ADJPRECLOSE = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);
                            objASHAREEODPRICES.S_DQ_ADJOPEN = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14);
                            objASHAREEODPRICES.S_DQ_ADJHIGH = reader.IsDBNull(15) ? 0 : reader.GetDecimal(15);
                            objASHAREEODPRICES.S_DQ_ADJLOW = reader.IsDBNull(16) ? 0 : reader.GetDecimal(16);
                            objASHAREEODPRICES.S_DQ_ADJCLOSE = reader.IsDBNull(17) ? 0 : reader.GetDecimal(17);
                            objASHAREEODPRICES.S_DQ_ADJFACTOR = reader.IsDBNull(18) ? 0 : reader.GetDecimal(18);
                            objASHAREEODPRICES.S_DQ_AVGPRICE = reader.IsDBNull(19) ? 0 : reader.GetDecimal(19);
                            objASHAREEODPRICES.S_DQ_TRADESTATUS = reader.IsDBNull(20) ? "" : reader.GetString(20);
                            objASHAREEODPRICES.S_DQ_TRADESTATUSCODE = reader.IsDBNull(21) ? 0 : reader.GetDecimal(21);

                            m_dictASHAREEODPRICES.Add(objASHAREEODPRICES.TRADE_DT, objASHAREEODPRICES);
                        }
                        
                    }
                    catch (MySqlException ex)
                    {

                        Console.WriteLine("查询ASHAREEODPRICES异常：" + ex);
                        lock (m_ErrorList)
                        {
                            if (wincode != "")
                                m_ErrorList.Add(wincode, 1);
                        }
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }


                }
            }
           
            return true;
        }


        private void DealPreSuperTable(MySqlConnection conn,string wincode, string pertardedate, ref ASHARESUPERTABLE supertable)
        {
            //MySqlConnection conn = new MySqlConnection(connetStrRead);
            try
            {
                conn.Close();
                conn.Open();
                string sql = "select * from ASHARESUPERTABLE where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT = '" + pertardedate + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    supertable.objASHAREEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                    supertable.objASHAREEODPRICES.S_DQ_OPEN = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                    supertable.objASHAREEODPRICES.S_DQ_HIGH = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                    supertable.objASHAREEODPRICES.S_DQ_LOW = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                    supertable.objASHAREEODPRICES.S_DQ_CLOSE = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                    supertable.objASHAREEODPRICES.S_DQ_CHANGE = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                    supertable.objASHAREEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                    supertable.objASHAREEODPRICES.S_DQ_VOLUME = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                    supertable.objASHAREEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                    supertable.objASHAREEODPRICES.S_DQ_ADJPRECLOSE = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                    supertable.objASHAREEODPRICES.S_DQ_ADJOPEN = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);
                    supertable.objASHAREEODPRICES.S_DQ_ADJHIGH = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);
                    supertable.objASHAREEODPRICES.S_DQ_ADJLOW = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14);
                    supertable.objASHAREEODPRICES.S_DQ_ADJCLOSE = reader.IsDBNull(15) ? 0 : reader.GetDecimal(15);
                    supertable.objASHAREEODPRICES.S_DQ_ADJFACTOR = reader.IsDBNull(16) ? 0 : reader.GetDecimal(16);
                    supertable.objASHAREEODPRICES.S_DQ_AVGPRICE = reader.IsDBNull(17) ? 0 : reader.GetDecimal(17);
                    supertable.objASHAREEODPRICES.S_DQ_TRADESTATUSCODE = reader.IsDBNull(18) ? 0 : reader.GetDecimal(18);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_MV = reader.IsDBNull(19) ? 0 : reader.GetDecimal(19);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = reader.IsDBNull(20) ? 0 : reader.GetDecimal(20);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_HIGH_52W_ = reader.IsDBNull(21) ? 0 : reader.GetDecimal(21);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_LOW_52W_ = reader.IsDBNull(22) ? 0 : reader.GetDecimal(22);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PB_NEW = reader.IsDBNull(24) ? 0 : reader.GetDecimal(24);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE_TTM = reader.IsDBNull(25) ? 0 : reader.GetDecimal(25);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCF = reader.IsDBNull(26) ? 0 : reader.GetDecimal(26);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCFTTM = reader.IsDBNull(27) ? 0 : reader.GetDecimal(27);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCF = reader.IsDBNull(28) ? 0 : reader.GetDecimal(28);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCFTTM = reader.IsDBNull(29) ? 0 : reader.GetDecimal(29);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS = reader.IsDBNull(30) ? 0 : reader.GetDecimal(30);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS_TTM = reader.IsDBNull(31) ? 0 : reader.GetDecimal(31);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_TURN = reader.IsDBNull(32) ? 0 : reader.GetDecimal(32);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_FREETURNOVER = reader.IsDBNull(33) ? 0 : reader.GetDecimal(33);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.TOT_SHR_TODAY = reader.IsDBNull(34) ? 0 : reader.GetDecimal(34);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.FLOAT_A_SHR_TODAY = reader.IsDBNull(35) ? 0 : reader.GetDecimal(35);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = reader.IsDBNull(36) ? 0 : reader.GetDecimal(36);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PRICE_DIV_DPS = reader.IsDBNull(37) ? 0 : reader.GetDecimal(37);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJHIGH_52W = reader.IsDBNull(38) ? 0 : reader.GetDecimal(38);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJLOW_52W = reader.IsDBNull(39) ? 0 : reader.GetDecimal(39);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.FREE_SHARES_TODAY = reader.IsDBNull(40) ? 0 : reader.GetDecimal(40);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_TTM = reader.IsDBNull(41) ? 0 : reader.GetDecimal(41);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_LYR = reader.IsDBNull(42) ? 0 : reader.GetDecimal(42);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_ASSETS_TODAY = reader.IsDBNull(43) ? 0 : reader.GetDecimal(43);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_TTM = reader.IsDBNull(44) ? 0 : reader.GetDecimal(44);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_LYR = reader.IsDBNull(45) ? 0 : reader.GetDecimal(45);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.OPER_REV_TTM = reader.IsDBNull(46) ? 0 : reader.GetDecimal(46);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.OPER_REV_LYR = reader.IsDBNull(47) ? 0 : reader.GetDecimal(47);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_TTM = reader.IsDBNull(48) ? 0 : reader.GetDecimal(48);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_LYR = reader.IsDBNull(49) ? 0 : reader.GetDecimal(49);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.UP_DOWN_LIMIT_STATUS = reader.IsDBNull(50) ? 0 : reader.GetDecimal(50);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.LOWEST_HIGHEST_STATUS = reader.IsDBNull(51) ? 0 : reader.GetDecimal(51);
                    //supertable.objASHAREL2INDICATORS.S_LI_INITIATIVEBUYRATE = reader.IsDBNull(52) ? 0 : reader.GetDecimal(52);
                    //supertable.objASHAREL2INDICATORS.S_LI_INITIATIVEBUYMONEY = reader.IsDBNull(53) ? 0 : reader.GetDecimal(53);
                    //supertable.objASHAREL2INDICATORS.S_LI_INITIATIVEBUYAMOUNT = reader.IsDBNull(54) ? 0 : reader.GetDecimal(54);
                    //supertable.objASHAREL2INDICATORS.S_LI_INITIATIVESELLRATE = reader.IsDBNull(55) ? 0 : reader.GetDecimal(55);
                    //supertable.objASHAREL2INDICATORS.S_LI_INITIATIVESELLMONEY = reader.IsDBNull(56) ? 0 : reader.GetDecimal(56);
                    //supertable.objASHAREL2INDICATORS.S_LI_INITIATIVESELLAMOUNT = reader.IsDBNull(57) ? 0 : reader.GetDecimal(57);
                    //supertable.objASHAREL2INDICATORS.S_LI_LARGEBUYRATE = reader.IsDBNull(58) ? 0 : reader.GetDecimal(58);
                    //supertable.objASHAREL2INDICATORS.S_LI_LARGEBUYMONEY = reader.IsDBNull(59) ? 0 : reader.GetDecimal(59);
                    //supertable.objASHAREL2INDICATORS.S_LI_LARGEBUYAMOUNT = reader.IsDBNull(60) ? 0 : reader.GetDecimal(60);
                    //supertable.objASHAREL2INDICATORS.S_LI_LARGESELLRATE = reader.IsDBNull(61) ? 0 : reader.GetDecimal(61);
                    //supertable.objASHAREL2INDICATORS.S_LI_LARGESELLMONEY = reader.IsDBNull(62) ? 0 : reader.GetDecimal(62);
                    //supertable.objASHAREL2INDICATORS.S_LI_LARGESELLAMOUNT = reader.IsDBNull(63) ? 0 : reader.GetDecimal(63);
                    //supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTRATE = reader.IsDBNull(64) ? 0 : reader.GetDecimal(64);
                    //supertable.objASHAREL2INDICATORS.S_LI_ENTRUDIFFERAMOUNT = reader.IsDBNull(65) ? 0 : reader.GetDecimal(65);
                    //supertable.objASHAREL2INDICATORS.S_LI_ENTRUDIFFERAMONEY = reader.IsDBNull(66) ? 0 : reader.GetDecimal(66);
                    //supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTBUYMONEY = reader.IsDBNull(67) ? 0 : reader.GetDecimal(67);
                    //supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTSELLMONEY = reader.IsDBNull(68) ? 0 : reader.GetDecimal(68);
                    //supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTBUYAMOUNT = reader.IsDBNull(69) ? 0 : reader.GetDecimal(69);
                    //supertable.objASHAREL2INDICATORS.S_LI_ENTRUSTSELLAMOUNT = reader.IsDBNull(70) ? 0 : reader.GetDecimal(70);
                    //supertable.objASHAREMONEYFLOW.BUY_VALUE_EXLARGE_ORDER = reader.IsDBNull(71) ? 0 : reader.GetDecimal(71);
                    //supertable.objASHAREMONEYFLOW.SELL_VALUE_EXLARGE_ORDER = reader.IsDBNull(72) ? 0 : reader.GetDecimal(72);
                    //supertable.objASHAREMONEYFLOW.BUY_VALUE_LARGE_ORDER = reader.IsDBNull(73) ? 0 : reader.GetDecimal(73);
                    //supertable.objASHAREMONEYFLOW.SELL_VALUE_LARGE_ORDER = reader.IsDBNull(74) ? 0 : reader.GetDecimal(74);
                    //supertable.objASHAREMONEYFLOW.BUY_VALUE_MED_ORDER = reader.IsDBNull(75) ? 0 : reader.GetDecimal(75);
                    //supertable.objASHAREMONEYFLOW.SELL_VALUE_MED_ORDER = reader.IsDBNull(76) ? 0 : reader.GetDecimal(76);
                    //supertable.objASHAREMONEYFLOW.BUY_VALUE_SMALL_ORDER = reader.IsDBNull(77) ? 0 : reader.GetDecimal(77);
                    //supertable.objASHAREMONEYFLOW.SELL_VALUE_SMALL_ORDER = reader.IsDBNull(78) ? 0 : reader.GetDecimal(78);
                    //supertable.objASHAREMONEYFLOW.BUY_VOLUME_EXLARGE_ORDER = reader.IsDBNull(79) ? 0 : reader.GetDecimal(79);
                    //supertable.objASHAREMONEYFLOW.SELL_VOLUME_EXLARGE_ORDER = reader.IsDBNull(80) ? 0 : reader.GetDecimal(80);
                    //supertable.objASHAREMONEYFLOW.BUY_VOLUME_LARGE_ORDER = reader.IsDBNull(81) ? 0 : reader.GetDecimal(81);
                    //supertable.objASHAREMONEYFLOW.SELL_VOLUME_LARGE_ORDER = reader.IsDBNull(82) ? 0 : reader.GetDecimal(81);
                    //supertable.objASHAREMONEYFLOW.BUY_VOLUME_MED_ORDER = reader.IsDBNull(83) ? 0 : reader.GetDecimal(83);
                    //supertable.objASHAREMONEYFLOW.SELL_VOLUME_MED_ORDER = reader.IsDBNull(84) ? 0 : reader.GetDecimal(84);
                    //supertable.objASHAREMONEYFLOW.BUY_VOLUME_SMALL_ORDER = reader.IsDBNull(85) ? 0 : reader.GetDecimal(85);
                    //supertable.objASHAREMONEYFLOW.SELL_VOLUME_SMALL_ORDER = reader.IsDBNull(86) ? 0 : reader.GetDecimal(86);
                    //supertable.objASHAREMONEYFLOW.TRADES_COUNT = reader.IsDBNull(87) ? 0 : reader.GetDecimal(87);
                    //supertable.objASHAREMONEYFLOW.BUY_TRADES_EXLARGE_ORDER = reader.IsDBNull(88) ? 0 : reader.GetDecimal(88);
                    //supertable.objASHAREMONEYFLOW.SELL_TRADES_EXLARGE_ORDER = reader.IsDBNull(89) ? 0 : reader.GetDecimal(89);
                    //supertable.objASHAREMONEYFLOW.BUY_TRADES_LARGE_ORDER = reader.IsDBNull(90) ? 0 : reader.GetDecimal(90);
                    //supertable.objASHAREMONEYFLOW.SELL_TRADES_LARGE_ORDER = reader.IsDBNull(91) ? 0 : reader.GetDecimal(91);
                    //supertable.objASHAREMONEYFLOW.BUY_TRADES_MED_ORDER = reader.IsDBNull(92) ? 0 : reader.GetDecimal(92);
                    //supertable.objASHAREMONEYFLOW.SELL_TRADES_MED_ORDER = reader.IsDBNull(93) ? 0 : reader.GetDecimal(93);
                    //supertable.objASHAREMONEYFLOW.BUY_TRADES_SMALL_ORDER = reader.IsDBNull(94) ? 0 : reader.GetDecimal(94);
                    //supertable.objASHAREMONEYFLOW.SELL_TRADES_SMALL_ORDER = reader.IsDBNull(95) ? 0 : reader.GetDecimal(95);
                    //supertable.objASHAREMONEYFLOW.VOLUME_DIFF_SMALL_TRADER = reader.IsDBNull(96) ? 0 : reader.GetDecimal(96);
                    //supertable.objASHAREMONEYFLOW.VOLUME_DIFF_SMALL_TRADER_ACT = reader.IsDBNull(97) ? 0 : reader.GetDecimal(97);
                    //supertable.objASHAREMONEYFLOW.VOLUME_DIFF_MED_TRADER = reader.IsDBNull(98) ? 0 : reader.GetDecimal(98);
                    //supertable.objASHAREMONEYFLOW.VOLUME_DIFF_MED_TRADER_ACT = reader.IsDBNull(99) ? 0 : reader.GetDecimal(99);
                    //supertable.objASHAREMONEYFLOW.VOLUME_DIFF_LARGE_TRADER = reader.IsDBNull(100) ? 0 : reader.GetDecimal(100);
                    //supertable.objASHAREMONEYFLOW.VOLUME_DIFF_LARGE_TRADER_ACT = reader.IsDBNull(101) ? 0 : reader.GetDecimal(101);
                    //supertable.objASHAREMONEYFLOW.VOLUME_DIFF_INSTITUTE = reader.IsDBNull(102) ? 0 : reader.GetDecimal(102);
                    //supertable.objASHAREMONEYFLOW.VOLUME_DIFF_INSTITUTE_ACT = reader.IsDBNull(103) ? 0 : reader.GetDecimal(103);
                    //supertable.objASHAREMONEYFLOW.VALUE_DIFF_SMALL_TRADER = reader.IsDBNull(104) ? 0 : reader.GetDecimal(104);
                    //supertable.objASHAREMONEYFLOW.VALUE_DIFF_SMALL_TRADER_ACT = reader.IsDBNull(105) ? 0 : reader.GetDecimal(105);
                    //supertable.objASHAREMONEYFLOW.VALUE_DIFF_MED_TRADER = reader.IsDBNull(106) ? 0 : reader.GetDecimal(106);
                    //supertable.objASHAREMONEYFLOW.VALUE_DIFF_MED_TRADER_ACT = reader.IsDBNull(107) ? 0 : reader.GetDecimal(107);
                    //supertable.objASHAREMONEYFLOW.VALUE_DIFF_LARGE_TRADER = reader.IsDBNull(108) ? 0 : reader.GetDecimal(108);
                    //supertable.objASHAREMONEYFLOW.VALUE_DIFF_LARGE_TRADER_ACT = reader.IsDBNull(109) ? 0 : reader.GetDecimal(109);
                    //supertable.objASHAREMONEYFLOW.VALUE_DIFF_INSTITUTE = reader.IsDBNull(110) ? 0 : reader.GetDecimal(110);
                    //supertable.objASHAREMONEYFLOW.VALUE_DIFF_INSTITUTE_ACT = reader.IsDBNull(111) ? 0 : reader.GetDecimal(111);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOWVOLUME = reader.IsDBNull(112) ? 0 : reader.GetDecimal(112);
                    //supertable.objASHAREMONEYFLOW.NET_INFLOW_RATE_VOLUME = reader.IsDBNull(113) ? 0 : reader.GetDecimal(113);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_OPENVOLUME = reader.IsDBNull(114) ? 0 : reader.GetDecimal(114);
                    //supertable.objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VOLUME = reader.IsDBNull(115) ? 0 : reader.GetDecimal(115);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSEVOLUME = reader.IsDBNull(116) ? 0 : reader.GetDecimal(116);
                    //supertable.objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VOLUME = reader.IsDBNull(117) ? 0 : reader.GetDecimal(117);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW = reader.IsDBNull(118) ? 0 : reader.GetDecimal(118);
                    //supertable.objASHAREMONEYFLOW.NET_INFLOW_RATE_VALUE = reader.IsDBNull(119) ? 0 : reader.GetDecimal(119);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_OPEN = reader.IsDBNull(121) ? 0 : reader.GetDecimal(120);
                    //supertable.objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VALUE = reader.IsDBNull(122) ? 0 : reader.GetDecimal(121);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSE = reader.IsDBNull(123) ? 0 : reader.GetDecimal(122);
                    //supertable.objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VALUE = reader.IsDBNull(124) ? 0 : reader.GetDecimal(123);
                    //supertable.objASHAREMONEYFLOW.TOT_VOLUME_BID = reader.IsDBNull(125) ? 0 : reader.GetDecimal(124);
                    //supertable.objASHAREMONEYFLOW.TOT_VOLUME_ASK = reader.IsDBNull(126) ? 0 : reader.GetDecimal(125);
                    //supertable.objASHAREMONEYFLOW.MONEYFLOW_PCT_VOLUME = reader.IsDBNull(127) ? 0 : reader.GetDecimal(126);
                    //supertable.objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VOLUME = reader.IsDBNull(128) ? 0 : reader.GetDecimal(127);
                    //supertable.objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VOLUME = reader.IsDBNull(129) ? 0 : reader.GetDecimal(128);
                    //supertable.objASHAREMONEYFLOW.MONEYFLOW_PCT_VALUE = reader.IsDBNull(129) ? 0 : reader.GetDecimal(129);
                    //supertable.objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VALUE = reader.IsDBNull(130) ? 0 : reader.GetDecimal(130);
                    //supertable.objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VALUE = reader.IsDBNull(131) ? 0 : reader.GetDecimal(131);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOWVOLUME_LARGE_ORDER = reader.IsDBNull(132) ? 0 : reader.GetDecimal(132);
                    //supertable.objASHAREMONEYFLOW.NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(133) ? 0 : reader.GetDecimal(133);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_LARGE_ORDER = reader.IsDBNull(134) ? 0 : reader.GetDecimal(134);
                    //supertable.objASHAREMONEYFLOW.NET_INFLOW_RATE_VALUE_L = reader.IsDBNull(135) ? 0 : reader.GetDecimal(135);
                    //supertable.objASHAREMONEYFLOW.MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(136) ? 0 : reader.GetDecimal(136);
                    //supertable.objASHAREMONEYFLOW.MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(137) ? 0 : reader.GetDecimal(137);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_OPENVOLUME_L = reader.IsDBNull(138) ? 0 : reader.GetDecimal(138);
                    //supertable.objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(139) ? 0 : reader.GetDecimal(139);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_OPEN_LARGE_ORDER = reader.IsDBNull(140) ? 0 : reader.GetDecimal(140);
                    //supertable.objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VALUE_L = reader.IsDBNull(141) ? 0 : reader.GetDecimal(141);
                    //supertable.objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(142) ? 0 : reader.GetDecimal(142);
                    //supertable.objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(143) ? 0 : reader.GetDecimal(143);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSEVOLUME_L = reader.IsDBNull(144) ? 0 : reader.GetDecimal(144);
                    //supertable.objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(145) ? 0 : reader.GetDecimal(145);
                    //supertable.objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSE_LARGE_ORDER = reader.IsDBNull(146) ? 0 : reader.GetDecimal(146);
                    //supertable.objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VALU_L = reader.IsDBNull(147) ? 0 : reader.GetDecimal(147);
                    //supertable.objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(148) ? 0 : reader.GetDecimal(148);
                    //supertable.objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(149) ? 0 : reader.GetDecimal(149);
                    //supertable.objASHAREMONEYFLOW.BUY_VALUE_EXLARGE_ORDER_ACT = reader.IsDBNull(150) ? 0 : reader.GetDecimal(150);
                    //supertable.objASHAREMONEYFLOW.SELL_VALUE_EXLARGE_ORDER_ACT = reader.IsDBNull(151) ? 0 : reader.GetDecimal(151);
                    //supertable.objASHAREMONEYFLOW.BUY_VALUE_LARGE_ORDER_ACT = reader.IsDBNull(152) ? 0 : reader.GetDecimal(152);
                    //supertable.objASHAREMONEYFLOW.SELL_VALUE_LARGE_ORDER_ACT = reader.IsDBNull(153) ? 0 : reader.GetDecimal(153);
                    //supertable.objASHAREMONEYFLOW.BUY_VALUE_MED_ORDER_ACT = reader.IsDBNull(154) ? 0 : reader.GetDecimal(154);
                    //supertable.objASHAREMONEYFLOW.SELL_VALUE_MED_ORDER_ACT = reader.IsDBNull(155) ? 0 : reader.GetDecimal(155);
                    //supertable.objASHAREMONEYFLOW.BUY_VALUE_SMALL_ORDER_ACT = reader.IsDBNull(156) ? 0 : reader.GetDecimal(156);
                    //supertable.objASHAREMONEYFLOW.SELL_VALUE_SMALL_ORDER_ACT = reader.IsDBNull(157) ? 0 : reader.GetDecimal(157);
                    //supertable.objASHAREMONEYFLOW.BUY_VOLUME_EXLARGE_ORDER_ACT = reader.IsDBNull(158) ? 0 : reader.GetDecimal(158);
                    //supertable.objASHAREMONEYFLOW.SELL_VOLUME_EXLARGE_ORDER_ACT = reader.IsDBNull(159) ? 0 : reader.GetDecimal(159);
                    //supertable.objASHAREMONEYFLOW.BUY_VOLUME_LARGE_ORDER_ACT = reader.IsDBNull(160) ? 0 : reader.GetDecimal(160);
                    //supertable.objASHAREMONEYFLOW.SELL_VOLUME_LARGE_ORDER_ACT = reader.IsDBNull(161) ? 0 : reader.GetDecimal(161);
                    //supertable.objASHAREMONEYFLOW.BUY_VOLUME_MED_ORDER_ACT = reader.IsDBNull(162) ? 0 : reader.GetDecimal(162);
                    //supertable.objASHAREMONEYFLOW.SELL_VOLUME_MED_ORDER_ACT = reader.IsDBNull(163) ? 0 : reader.GetDecimal(163);
                    //supertable.objASHAREMONEYFLOW.BUY_VOLUME_SMALL_ORDER_ACT = reader.IsDBNull(164) ? 0 : reader.GetDecimal(164);
                    //supertable.objASHAREMONEYFLOW.SELL_VOLUME_SMALL_ORDER_ACT = reader.IsDBNull(165) ? 0 : reader.GetDecimal(165);
                    //supertable.objAINDEXSH50.I_WEIGHT = reader.IsDBNull(170) ? 0 : reader.GetDecimal(170);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_PRECLOSE = reader.IsDBNull(171) ? 0 : reader.GetDecimal(171);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_OPEN = reader.IsDBNull(172) ? 0 : reader.GetDecimal(172);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_HIGH = reader.IsDBNull(173) ? 0 : reader.GetDecimal(173);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_LOW = reader.IsDBNull(174) ? 0 : reader.GetDecimal(174);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_CLOSE = reader.IsDBNull(175) ? 0 : reader.GetDecimal(175);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_CHANGE = reader.IsDBNull(176) ? 0 : reader.GetDecimal(176);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_PCTCHANGE = reader.IsDBNull(177) ? 0 : reader.GetDecimal(177);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_VOLUME = reader.IsDBNull(178) ? 0 : reader.GetDecimal(178);
                    //supertable.objAINDEXEODPRICESSH50.S_DQ_AMOUNT = reader.IsDBNull(179) ? 0 : reader.GetDecimal(179);
                    //supertable.objAINDEXHS300.I_WEIGHT = reader.IsDBNull(180) ? 0 : reader.GetDecimal(180);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_PRECLOSE = reader.IsDBNull(181) ? 0 : reader.GetDecimal(181);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_OPEN = reader.IsDBNull(182) ? 0 : reader.GetDecimal(182);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_HIGH = reader.IsDBNull(183) ? 0 : reader.GetDecimal(183);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_LOW = reader.IsDBNull(184) ? 0 : reader.GetDecimal(184);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_CLOSE = reader.IsDBNull(185) ? 0 : reader.GetDecimal(185);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_CHANGE = reader.IsDBNull(186) ? 0 : reader.GetDecimal(186);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_PCTCHANGE = reader.IsDBNull(187) ? 0 : reader.GetDecimal(187);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_VOLUME = reader.IsDBNull(188) ? 0 : reader.GetDecimal(188);
                    //supertable.objAINDEXEODPRICESHS300.S_DQ_AMOUNT = reader.IsDBNull(189) ? 0 : reader.GetDecimal(189);
                    //supertable.objAINDEXCS500.I_WEIGHT = reader.IsDBNull(190) ? 0 : reader.GetDecimal(190);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_PRECLOSE = reader.IsDBNull(191) ? 0 : reader.GetDecimal(191);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_OPEN = reader.IsDBNull(192) ? 0 : reader.GetDecimal(192);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_HIGH = reader.IsDBNull(193) ? 0 : reader.GetDecimal(193);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_LOW = reader.IsDBNull(194) ? 0 : reader.GetDecimal(194);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_CLOSE = reader.IsDBNull(195) ? 0 : reader.GetDecimal(195);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_CHANGE = reader.IsDBNull(196) ? 0 : reader.GetDecimal(196);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_PCTCHANGE = reader.IsDBNull(197) ? 0 : reader.GetDecimal(197);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_VOLUME = reader.IsDBNull(198) ? 0 : reader.GetDecimal(198);
                    //supertable.objAINDEXEODPRICESCS500.S_DQ_AMOUNT = reader.IsDBNull(199) ? 0 : reader.GetDecimal(199);
                    //supertable.objAINDEXCS1000.I_WEIGHT = reader.IsDBNull(200) ? 0 : reader.GetDecimal(200);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_PRECLOSE = reader.IsDBNull(201) ? 0 : reader.GetDecimal(201);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_OPEN = reader.IsDBNull(202) ? 0 : reader.GetDecimal(202);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_HIGH = reader.IsDBNull(203) ? 0 : reader.GetDecimal(203);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_LOW = reader.IsDBNull(204) ? 0 : reader.GetDecimal(204);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_CLOSE = reader.IsDBNull(205) ? 0 : reader.GetDecimal(205);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_CHANGE = reader.IsDBNull(206) ? 0 : reader.GetDecimal(206);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_PCTCHANGE = reader.IsDBNull(207) ? 0 : reader.GetDecimal(207);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_VOLUME = reader.IsDBNull(208) ? 0 : reader.GetDecimal(208);
                    //supertable.objAINDEXEODPRICESCS1000.S_DQ_AMOUNT = reader.IsDBNull(219) ? 0 : reader.GetDecimal(209);
                    //supertable.objAINDEXMKT.I_WEIGHT = reader.IsDBNull(210) ? 0 : reader.GetDecimal(210);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_PRECLOSE = reader.IsDBNull(211) ? 0 : reader.GetDecimal(211);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_OPEN = reader.IsDBNull(212) ? 0 : reader.GetDecimal(212);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_HIGH = reader.IsDBNull(213) ? 0 : reader.GetDecimal(213);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_LOW = reader.IsDBNull(214) ? 0 : reader.GetDecimal(214);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_CLOSE = reader.IsDBNull(215) ? 0 : reader.GetDecimal(215);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_CHANGE = reader.IsDBNull(216) ? 0 : reader.GetDecimal(216);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_PCTCHANGE = reader.IsDBNull(217) ? 0 : reader.GetDecimal(217);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_VOLUME = reader.IsDBNull(218) ? 0 : reader.GetDecimal(218);
                    //supertable.objAINDEXEODPRICESMKT.S_DQ_AMOUNT = reader.IsDBNull(219) ? 0 : reader.GetDecimal(219);

                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("查询ASHAREEODDERIVATIVEINDICATOR 异常：" + ex);
                lock (m_ErrorList)
                {
                    if (wincode != "")
                        m_ErrorList.Add(wincode, 1);
                }
            }
            finally
            {
                conn.Close();
            }
            
        }
        private void DealPreSuperTableForASHAREEODPRICES(MySqlConnection conn, string wincode, string pertardedate, ref ASHARESUPERTABLE supertable)
        {
            //MySqlConnection conn = new MySqlConnection(connetStrRead);
            try
            {
                conn.Close();
                conn.Open();
                string sql = "select * from ASHARESUPERTABLE where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT = '" + pertardedate + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    supertable.objASHAREEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                    supertable.objASHAREEODPRICES.S_DQ_OPEN = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                    supertable.objASHAREEODPRICES.S_DQ_HIGH = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                    supertable.objASHAREEODPRICES.S_DQ_LOW = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                    supertable.objASHAREEODPRICES.S_DQ_CLOSE = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                    supertable.objASHAREEODPRICES.S_DQ_CHANGE = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                    supertable.objASHAREEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                    supertable.objASHAREEODPRICES.S_DQ_VOLUME = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                    supertable.objASHAREEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                    supertable.objASHAREEODPRICES.S_DQ_ADJPRECLOSE = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                    supertable.objASHAREEODPRICES.S_DQ_ADJOPEN = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);
                    supertable.objASHAREEODPRICES.S_DQ_ADJHIGH = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);
                    supertable.objASHAREEODPRICES.S_DQ_ADJLOW = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14);
                    supertable.objASHAREEODPRICES.S_DQ_ADJCLOSE = reader.IsDBNull(15) ? 0 : reader.GetDecimal(15);
                    supertable.objASHAREEODPRICES.S_DQ_ADJFACTOR = reader.IsDBNull(16) ? 0 : reader.GetDecimal(16);
                    supertable.objASHAREEODPRICES.S_DQ_AVGPRICE = reader.IsDBNull(17) ? 0 : reader.GetDecimal(17);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_MV = reader.IsDBNull(19) ? 0 : reader.GetDecimal(19);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = reader.IsDBNull(20) ? 0 : reader.GetDecimal(20);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_HIGH_52W_ = reader.IsDBNull(21) ? 0 : reader.GetDecimal(21);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_LOW_52W_ = reader.IsDBNull(22) ? 0 : reader.GetDecimal(22);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PB_NEW = reader.IsDBNull(24) ? 0 : reader.GetDecimal(24);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE_TTM = reader.IsDBNull(25) ? 0 : reader.GetDecimal(25);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCF = reader.IsDBNull(26) ? 0 : reader.GetDecimal(26);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCFTTM = reader.IsDBNull(27) ? 0 : reader.GetDecimal(27);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCF = reader.IsDBNull(28) ? 0 : reader.GetDecimal(28);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCFTTM = reader.IsDBNull(29) ? 0 : reader.GetDecimal(29);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS = reader.IsDBNull(30) ? 0 : reader.GetDecimal(30);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS_TTM = reader.IsDBNull(31) ? 0 : reader.GetDecimal(31);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_TURN = reader.IsDBNull(32) ? 0 : reader.GetDecimal(32);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_FREETURNOVER = reader.IsDBNull(33) ? 0 : reader.GetDecimal(33);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.TOT_SHR_TODAY = reader.IsDBNull(34) ? 0 : reader.GetDecimal(34);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.FLOAT_A_SHR_TODAY = reader.IsDBNull(35) ? 0 : reader.GetDecimal(35);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = reader.IsDBNull(36) ? 0 : reader.GetDecimal(36);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PRICE_DIV_DPS = reader.IsDBNull(37) ? 0 : reader.GetDecimal(37);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJHIGH_52W = reader.IsDBNull(38) ? 0 : reader.GetDecimal(38);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJLOW_52W = reader.IsDBNull(39) ? 0 : reader.GetDecimal(39);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.FREE_SHARES_TODAY = reader.IsDBNull(40) ? 0 : reader.GetDecimal(40);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_TTM = reader.IsDBNull(41) ? 0 : reader.GetDecimal(41);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_LYR = reader.IsDBNull(42) ? 0 : reader.GetDecimal(42);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_ASSETS_TODAY = reader.IsDBNull(43) ? 0 : reader.GetDecimal(43);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_TTM = reader.IsDBNull(44) ? 0 : reader.GetDecimal(44);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_LYR = reader.IsDBNull(45) ? 0 : reader.GetDecimal(45);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.OPER_REV_TTM = reader.IsDBNull(46) ? 0 : reader.GetDecimal(46);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.OPER_REV_LYR = reader.IsDBNull(47) ? 0 : reader.GetDecimal(47);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_TTM = reader.IsDBNull(48) ? 0 : reader.GetDecimal(48);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_LYR = reader.IsDBNull(49) ? 0 : reader.GetDecimal(49);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.UP_DOWN_LIMIT_STATUS = reader.IsDBNull(50) ? 0 : reader.GetDecimal(50);
                    supertable.objASHAREEODDERIVATIVEINDICATOR.LOWEST_HIGHEST_STATUS = reader.IsDBNull(51) ? 0 : reader.GetDecimal(51);
                    supertable.S_DQ_SUSPENDTYPE = reader.IsDBNull(178) ? 0 : reader.GetDecimal(178);
                    supertable.objAINDEXSH50.I_WEIGHT = reader.IsDBNull(170) ? 0 : reader.GetDecimal("SH50_I_WEIGHT");
                    supertable.objAINDEXHS300.I_WEIGHT = reader.IsDBNull(180) ? 0 : reader.GetDecimal("HS300_I_WEIGHT");
                    supertable.objAINDEXCS500.I_WEIGHT = reader.IsDBNull(190) ? 0 : reader.GetDecimal("CS500_I_WEIGHT");
                    supertable.objAINDEXCS1000.I_WEIGHT = reader.IsDBNull(200) ? 0 : reader.GetDecimal("CS1000_I_WEIGHT");
                    supertable.objAINDEXMKT.I_WEIGHT = reader.IsDBNull(210) ? 0 : reader.GetDecimal("MKT_I_WEIGHT");
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("查询ASHAREEODDERIVATIVEINDICATOR 异常：" + ex);
                lock (m_ErrorList)
                {
                    if (wincode != "")
                        m_ErrorList.Add(wincode, 1);
                }
            }
            finally
            {
                conn.Close();
            }

        }

        private void DealPreSuperTableIndex(MySqlConnection conn, string wincode, string pertardedate, ref ASHARESUPERTABLE supertable)
        {
            //MySqlConnection conn = new MySqlConnection(connetStrRead);
            try
            {
                conn.Close();
                conn.Open();
                string sql = "select SH50_I_WEIGHT,HS300_I_WEIGHT,CS500_I_WEIGHT,CS1000_I_WEIGHT,MKT_I_WEIGHT from ASHARESUPERTABLE where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT = '" + pertardedate + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    supertable.objAINDEXSH50.I_WEIGHT = reader.IsDBNull(0) ? 0 : reader.GetDecimal("SH50_I_WEIGHT");                
                    supertable.objAINDEXHS300.I_WEIGHT = reader.IsDBNull(1) ? 0 : reader.GetDecimal("HS300_I_WEIGHT");              
                    supertable.objAINDEXCS500.I_WEIGHT = reader.IsDBNull(2) ? 0 : reader.GetDecimal("CS500_I_WEIGHT");              
                    supertable.objAINDEXCS1000.I_WEIGHT = reader.IsDBNull(3) ? 0 : reader.GetDecimal("CS1000_I_WEIGHT");
                    supertable.objAINDEXMKT.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("MKT_I_WEIGHT");

                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("查询ASHARESUPERTABLE 异常：" + ex);
                lock (m_ErrorList)
                {
                    if (wincode != "")
                        m_ErrorList.Add(wincode, 1);
                }
            }
            finally
            {
                conn.Close();
            }

        }


        private void DealAINDEXPrice(MySqlConnection conn )
        {
            if(istoday)
            {
                conn.Open();
                string sql = "select * from AINDEXEODPRICES where  S_INFO_WINDCODE in ('000016.SH','399300.SZ','000905.SH','000852.SH','399317.SZ') and TRADE_DT = '" + date + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                try
                {

                    while (reader.Read())
                    {
                        AINDEXEODPRICES objAINDEXEODPRICES = new AINDEXEODPRICES();
                        string indexcode = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                        if (indexcode == "000016.SH")
                        {
                            objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                            objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                            objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                            objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                            objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                            objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                            objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                            objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                            objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                            objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                            objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                            m_dictAINDEXEODPRICESSH50.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                        }
                        else if (indexcode == "399300.SZ")
                        {
                            objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                            objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                            objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                            objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                            objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                            objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                            objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                            objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                            objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                            objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                            objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                            m_dictAINDEXEODPRICESHS300.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                        }
                        else if (indexcode == "000905.SH")
                        {
                            objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                            objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                            objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                            objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                            objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                            objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                            objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                            objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                            objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                            objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                            objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                            m_dictAINDEXEODPRICESCS500.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                        }
                        else if (indexcode == "000852.SH")
                        {
                            objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                            objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                            objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                            objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                            objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                            objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                            objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                            objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                            objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                            objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                            objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                            m_dictAINDEXEODPRICESCS1000.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                        }
                        else if (indexcode == "399317.SZ")
                        {
                            objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                            objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                            objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                            objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                            objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                            objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                            objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                            objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                            objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                            objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                            objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                            m_dictAINDEXEODPRICESMKT.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                        }

                    }

                    conn.Close();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询AINDEX异常：" + ex);

                }
                finally
                {
                    conn.Close();
                }
            }
        else
        {
                foreach (var begindate in Begintime)
                {
                    try
                    {

                        conn.Open();
                        string sql = "select * from AINDEXEODPRICES where  S_INFO_WINDCODE in ('000016.SH','399300.SZ','000905.SH','000852.SH','399317.SZ') and TRADE_DT like '" + begindate + "%';";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            AINDEXEODPRICES objAINDEXEODPRICES = new AINDEXEODPRICES();
                            string indexcode = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                            if (indexcode == "000016.SH")
                            {
                                objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                                objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                                objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                                objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                                objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                                objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                                objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                                objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                                objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                                objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                                objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                                m_dictAINDEXEODPRICESSH50.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                            }
                            else if (indexcode == "399300.SZ")
                            {
                                objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                                objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                                objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                                objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                                objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                                objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                                objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                                objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                                objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                                objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                                objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                                m_dictAINDEXEODPRICESHS300.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                            }
                            else if (indexcode == "000905.SH")
                            {
                                objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                                objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                                objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                                objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                                objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                                objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                                objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                                objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                                objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                                objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                                objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                                m_dictAINDEXEODPRICESCS500.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                            }
                            else if (indexcode == "000852.SH")
                            {
                                objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                                objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                                objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                                objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                                objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                                objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                                objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                                objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                                objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                                objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                                objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                                m_dictAINDEXEODPRICESCS1000.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                            }
                            else if (indexcode == "399317.SZ")
                            {
                                objAINDEXEODPRICES.S_INFO_WINDCODE = indexcode;
                                objAINDEXEODPRICES.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                                objAINDEXEODPRICES.S_DQ_PRECLOSE = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_DQ_PRECLOSE");
                                objAINDEXEODPRICES.S_DQ_OPEN = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_OPEN");
                                objAINDEXEODPRICES.S_DQ_HIGH = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_DQ_HIGH");
                                objAINDEXEODPRICES.S_DQ_LOW = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_DQ_LOW");
                                objAINDEXEODPRICES.S_DQ_CLOSE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_DQ_CLOSE");
                                objAINDEXEODPRICES.S_DQ_CHANGE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_DQ_CHANGE");
                                objAINDEXEODPRICES.S_DQ_PCTCHANGE = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_DQ_PCTCHANGE");
                                objAINDEXEODPRICES.S_DQ_VOLUME = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_DQ_VOLUME");
                                objAINDEXEODPRICES.S_DQ_AMOUNT = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_DQ_AMOUNT");
                                m_dictAINDEXEODPRICESMKT.Add(objAINDEXEODPRICES.TRADE_DT, objAINDEXEODPRICES);
                            }

                        }
                        conn.Close();
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("查询AINDEX异常：" + ex);

                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }       
          
        }
        private bool DealAINDEX(string wincode,
            ref Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXSH50,ref Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXHS300,ref Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXCS500,
            ref Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXCS1000,ref Dictionary<string, AINDEXHS300FREEWEIGHT> m_dictAINDEXMKT)
        {
             MySqlConnection conn = new MySqlConnection(connetStrRead);
            if (istoday)
            {
                try
                {

                    conn.Open();

                    string sql = "select * from AINDEXHS300FREEWEIGHT where S_INFO_WINDCODE in ('000016.SH','399300.SZ','000905.SH','000852.SH','399317.SZ') and TRADE_DT = '" + date + "' and S_CON_WINDCODE = '" + wincode + "';";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        AINDEXHS300FREEWEIGHT objAINDEX = new AINDEXHS300FREEWEIGHT();
                        string indexcode = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                        if (indexcode == "000016.SH")
                        {
                            objAINDEX.S_INFO_WINDCODE = indexcode;
                            objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                            objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                            objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                            m_dictAINDEXSH50.Add(objAINDEX.TRADE_DT, objAINDEX);
                        }
                        else if (indexcode == "399300.SZ")
                        {
                            objAINDEX.S_INFO_WINDCODE = indexcode;
                            objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                            objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                            objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                            m_dictAINDEXHS300.Add(objAINDEX.TRADE_DT, objAINDEX);
                        }
                        else if (indexcode == "000905.SH")
                        {
                            objAINDEX.S_INFO_WINDCODE = indexcode;
                            objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                            objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                            objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                            m_dictAINDEXCS500.Add(objAINDEX.TRADE_DT, objAINDEX);
                        }
                        else if (indexcode == "000852.SH")
                        {
                            objAINDEX.S_INFO_WINDCODE = indexcode;
                            objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                            objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                            objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                            m_dictAINDEXCS1000.Add(objAINDEX.TRADE_DT, objAINDEX);
                        }
                        else if (indexcode == "399317.SZ")
                        {
                            objAINDEX.S_INFO_WINDCODE = indexcode;
                            objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                            objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                            objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                            m_dictAINDEXMKT.Add(objAINDEX.TRADE_DT, objAINDEX);
                        }
                    }

                    conn.Close();

                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询AINDEX异常：" + ex);
                    lock (m_ErrorList)
                    {
                        if (wincode != "")
                            m_ErrorList.Add(wincode, 1);
                    }
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            
             }
            else
            {
                foreach (var begindate in Begintime)
                {
                    try
                    {

                        conn.Open();

                        string sql = "select * from AINDEXHS300FREEWEIGHT where S_INFO_WINDCODE in ('000016.SH','399300.SZ','000905.SH','000852.SH','399317.SZ') and TRADE_DT like '" + begindate + "%' and S_CON_WINDCODE = '" + wincode + "';";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {

                            AINDEXHS300FREEWEIGHT objAINDEX = new AINDEXHS300FREEWEIGHT();
                            string indexcode = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                            if (indexcode == "000016.SH")
                            {
                                objAINDEX.S_INFO_WINDCODE = indexcode;
                                objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                                objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                                objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                                m_dictAINDEXSH50.Add(objAINDEX.TRADE_DT, objAINDEX);
                            }
                            else if (indexcode == "399300.SZ")
                            {
                                objAINDEX.S_INFO_WINDCODE = indexcode;
                                objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                                objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                                objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                                m_dictAINDEXHS300.Add(objAINDEX.TRADE_DT, objAINDEX);
                            }
                            else if (indexcode == "000905.SH")
                            {
                                objAINDEX.S_INFO_WINDCODE = indexcode;
                                objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                                objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                                objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                                m_dictAINDEXCS500.Add(objAINDEX.TRADE_DT, objAINDEX);
                            }
                            else if (indexcode == "000852.SH")
                            {
                                objAINDEX.S_INFO_WINDCODE = indexcode;
                                objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                                objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                                objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                                m_dictAINDEXCS1000.Add(objAINDEX.TRADE_DT, objAINDEX);
                            }
                            else if (indexcode == "399317.SZ")
                            {
                                objAINDEX.S_INFO_WINDCODE = indexcode;
                                objAINDEX.S_CON_WINDCODE = reader.IsDBNull(2) ? "" : reader.GetString("S_CON_WINDCODE");
                                objAINDEX.TRADE_DT = reader.IsDBNull(3) ? "" : reader.GetString("TRADE_DT");
                                objAINDEX.I_WEIGHT = reader.IsDBNull(4) ? 0 : reader.GetDecimal("I_WEIGHT"); // 权重
                                m_dictAINDEXMKT.Add(objAINDEX.TRADE_DT, objAINDEX);
                            }
                        }

                        conn.Close();

                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("查询AINDEX异常：" + ex);
                        lock (m_ErrorList)
                        {
                            if (wincode != "")
                                m_ErrorList.Add(wincode, 1);
                        }
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
                  
            return true;
        }

        private bool DealASHAREEODDERIVATIVEINDICATOR(string wincode,ref Dictionary<string, ASHAREEODDERIVATIVEINDICATOR> m_dictASHAREEODDERIVATIVEINDICATOR)
        {
            MySqlConnection conn = new MySqlConnection(connetStrRead);
            if(istoday)
            {
                try
                {
                    conn.Open();
                    string sql;
                    if (wincode == "001914.SZ")
                    {
                        if(Convert.ToInt32(date) < 20181214)
                            sql = "select * from ASHAREEODDERIVATIVEINDICATOR where S_INFO_WINDCODE = '000043.SZ' and TRADE_DT = '" + date + "' ;";
                        else
                            sql = "select * from ASHAREEODDERIVATIVEINDICATOR where S_INFO_WINDCODE = '001914.SZ' and TRADE_DT = '" + date + "' ;";
                    }
                    else
                    {
                        sql = "select * from ASHAREEODDERIVATIVEINDICATOR where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT = '" + date + "' ;";
                    }
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ASHAREEODDERIVATIVEINDICATOR objASHAREEODDERIVATIVEINDICATOR = new ASHAREEODDERIVATIVEINDICATOR();
                        objASHAREEODDERIVATIVEINDICATOR.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                        objASHAREEODDERIVATIVEINDICATOR.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_MV = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_VAL_MV");       // 当日总市值
                        objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_MV");        // 当日流通市值
                        objASHAREEODDERIVATIVEINDICATOR.S_PQ_HIGH_52W_ = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_PQ_HIGH_52W_"); // 周最高价				
                        objASHAREEODDERIVATIVEINDICATOR.S_PQ_LOW_52W_ = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_PQ_LOW_52W_");  // 周最低价		
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_VAL_PE");       // 市盈率(PE)   
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PB_NEW = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_VAL_PB_NEW");   // 市净率(PB)
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE_TTM = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_VAL_PE_TTM");   // 市盈率(PE, TTM)
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCF = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_VAL_PCF_OCF");  // 市现率(PCF, 经营现金流) 
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCFTTM = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_VAL_PCF_OCFTTM"); // 市现率(PCF, 经营现金流TTM)
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCF = reader.IsDBNull(13) ? 0 : reader.GetDecimal("S_VAL_PCF_NCF");   // 市现率(PCF, 现金净流量)  
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCFTTM = reader.IsDBNull(14) ? 0 : reader.GetDecimal("S_VAL_PCF_NCFTTM"); // 市现率(PCF, 现金净流量TTM)
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS = reader.IsDBNull(15) ? 0 : reader.GetDecimal("S_VAL_PS");         // 市销率(PS)
                        objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS_TTM = reader.IsDBNull(16) ? 0 : reader.GetDecimal("S_VAL_PS_TTM");     // 市销率(PS, TTM) 
                        objASHAREEODDERIVATIVEINDICATOR.S_DQ_TURN = reader.IsDBNull(17) ? 0 : reader.GetDecimal("S_DQ_TURN");        // 换手率	
                        objASHAREEODDERIVATIVEINDICATOR.S_DQ_FREETURNOVER = reader.IsDBNull(18) ? 0 : reader.GetDecimal("S_DQ_FREETURNOVER"); // 换手率(基准.自由流通股本) 
                        objASHAREEODDERIVATIVEINDICATOR.TOT_SHR_TODAY = reader.IsDBNull(19) ? 0 : reader.GetDecimal("TOT_SHR_TODAY");    // 当日总股本
                        objASHAREEODDERIVATIVEINDICATOR.FLOAT_A_SHR_TODAY = reader.IsDBNull(20) ? 0 : reader.GetDecimal("FLOAT_A_SHR_TODAY"); // 当日流通股本
                        objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = reader.IsDBNull(21) ? 0 : reader.GetDecimal("S_DQ_CLOSE_TODAY");  // 当日收盘价
                        objASHAREEODDERIVATIVEINDICATOR.S_PRICE_DIV_DPS = reader.IsDBNull(22) ? 0 : reader.GetDecimal("S_PRICE_DIV_DPS");   // 股价/每股派息	
                        objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJHIGH_52W = reader.IsDBNull(23) ? 0 : reader.GetDecimal("S_PQ_ADJHIGH_52W");  // 周最高价(复权) 
                        objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJLOW_52W = reader.IsDBNull(24) ? 0 : reader.GetDecimal("S_PQ_ADJLOW_52W");   // 周最低价(复权)
                        objASHAREEODDERIVATIVEINDICATOR.FREE_SHARES_TODAY = reader.IsDBNull(25) ? 0 : reader.GetDecimal("FREE_SHARES_TODAY"); // 当日自由流通股本 
                        objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_TTM = reader.IsDBNull(26) ? 0 : reader.GetDecimal("NET_PROFIT_PARENT_COMP_TTM"); // 归属母公司净利润(TTM) 
                        objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_LYR = reader.IsDBNull(27) ? 0 : reader.GetDecimal("NET_PROFIT_PARENT_COMP_LYR"); // 归属母公司净利润(LYR) 
                        objASHAREEODDERIVATIVEINDICATOR.NET_ASSETS_TODAY = reader.IsDBNull(28) ? 0 : reader.GetDecimal("NET_ASSETS_TODAY");  // 当日净资产
                        objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_TTM = reader.IsDBNull(29) ? 0 : reader.GetDecimal("NET_CASH_FLOWS_OPER_ACT_TTM"); // 经营活动产生的现金流量净额(TTM)
                        objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_LYR = reader.IsDBNull(30) ? 0 : reader.GetDecimal("NET_CASH_FLOWS_OPER_ACT_LYR"); // 经营活动产生的现金流量净额(LYR) 
                        objASHAREEODDERIVATIVEINDICATOR.OPER_REV_TTM = reader.IsDBNull(31) ? 0 : reader.GetDecimal("OPER_REV_TTM");      // 营业收入(TTM)
                        objASHAREEODDERIVATIVEINDICATOR.OPER_REV_LYR = reader.IsDBNull(32) ? 0 : reader.GetDecimal("OPER_REV_LYR");      // 营业收入(LYR) 
                        objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_TTM = reader.IsDBNull(33) ? 0 : reader.GetDecimal("NET_INCR_CASH_CASH_EQU_TTM"); // 现金及现金等价物净增加额(TTM) 
                        objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_LYR = reader.IsDBNull(34) ? 0 : reader.GetDecimal("NET_INCR_CASH_CASH_EQU_LYR"); // 现金及现金等价物净增加额(LYR) 
                        objASHAREEODDERIVATIVEINDICATOR.UP_DOWN_LIMIT_STATUS = reader.IsDBNull(35) ? 0 : reader.GetDecimal("UP_DOWN_LIMIT_STATUS"); // 涨跌停状态
                        objASHAREEODDERIVATIVEINDICATOR.LOWEST_HIGHEST_STATUS = reader.IsDBNull(36) ? 0 : reader.GetDecimal("LOWEST_HIGHEST_STATUS"); // 最高最低价状态
                        m_dictASHAREEODDERIVATIVEINDICATOR.Add(objASHAREEODDERIVATIVEINDICATOR.TRADE_DT, objASHAREEODDERIVATIVEINDICATOR);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询ASHAREEODDERIVATIVEINDICATOR 异常：" + ex);
                    lock (m_ErrorList)
                    {
                        if (wincode != "")
                            m_ErrorList.Add(wincode, 1);
                    }
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                foreach (var begindate in Begintime)
                {
                    try
                    {
                        conn.Open();
                        string sql;
                        if (wincode == "001914.SZ")
                        {
                            sql = "select * from ASHAREEODDERIVATIVEINDICATOR where (S_INFO_WINDCODE = '000043.SZ' or S_INFO_WINDCODE = '001914.SZ')and TRADE_DT like '" + begindate + "%' ;";
                            
                        }
                        else
                        {
                            sql = "select * from ASHAREEODDERIVATIVEINDICATOR where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT like '" + begindate + "%' ;";
                        }
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            ASHAREEODDERIVATIVEINDICATOR objASHAREEODDERIVATIVEINDICATOR = new ASHAREEODDERIVATIVEINDICATOR();
                            objASHAREEODDERIVATIVEINDICATOR.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                            objASHAREEODDERIVATIVEINDICATOR.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_MV = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_VAL_MV");       // 当日总市值
                            objASHAREEODDERIVATIVEINDICATOR.S_DQ_MV = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_DQ_MV");        // 当日流通市值
                            objASHAREEODDERIVATIVEINDICATOR.S_PQ_HIGH_52W_ = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_PQ_HIGH_52W_"); // 周最高价				
                            objASHAREEODDERIVATIVEINDICATOR.S_PQ_LOW_52W_ = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_PQ_LOW_52W_");  // 周最低价		
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_VAL_PE");       // 市盈率(PE)   
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PB_NEW = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_VAL_PB_NEW");   // 市净率(PB)
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PE_TTM = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_VAL_PE_TTM");   // 市盈率(PE, TTM)
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCF = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_VAL_PCF_OCF");  // 市现率(PCF, 经营现金流) 
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_OCFTTM = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_VAL_PCF_OCFTTM"); // 市现率(PCF, 经营现金流TTM)
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCF = reader.IsDBNull(13) ? 0 : reader.GetDecimal("S_VAL_PCF_NCF");   // 市现率(PCF, 现金净流量)  
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PCF_NCFTTM = reader.IsDBNull(14) ? 0 : reader.GetDecimal("S_VAL_PCF_NCFTTM"); // 市现率(PCF, 现金净流量TTM)
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS = reader.IsDBNull(15) ? 0 : reader.GetDecimal("S_VAL_PS");         // 市销率(PS)
                            objASHAREEODDERIVATIVEINDICATOR.S_VAL_PS_TTM = reader.IsDBNull(16) ? 0 : reader.GetDecimal("S_VAL_PS_TTM");     // 市销率(PS, TTM) 
                            objASHAREEODDERIVATIVEINDICATOR.S_DQ_TURN = reader.IsDBNull(17) ? 0 : reader.GetDecimal("S_DQ_TURN");        // 换手率	
                            objASHAREEODDERIVATIVEINDICATOR.S_DQ_FREETURNOVER = reader.IsDBNull(18) ? 0 : reader.GetDecimal("S_DQ_FREETURNOVER"); // 换手率(基准.自由流通股本) 
                            objASHAREEODDERIVATIVEINDICATOR.TOT_SHR_TODAY = reader.IsDBNull(19) ? 0 : reader.GetDecimal("TOT_SHR_TODAY");    // 当日总股本
                            objASHAREEODDERIVATIVEINDICATOR.FLOAT_A_SHR_TODAY = reader.IsDBNull(20) ? 0 : reader.GetDecimal("FLOAT_A_SHR_TODAY"); // 当日流通股本
                            objASHAREEODDERIVATIVEINDICATOR.S_DQ_CLOSE_TODAY = reader.IsDBNull(21) ? 0 : reader.GetDecimal("S_DQ_CLOSE_TODAY");  // 当日收盘价
                            objASHAREEODDERIVATIVEINDICATOR.S_PRICE_DIV_DPS = reader.IsDBNull(22) ? 0 : reader.GetDecimal("S_PRICE_DIV_DPS");   // 股价/每股派息	
                            objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJHIGH_52W = reader.IsDBNull(23) ? 0 : reader.GetDecimal("S_PQ_ADJHIGH_52W");  // 周最高价(复权) 
                            objASHAREEODDERIVATIVEINDICATOR.S_PQ_ADJLOW_52W = reader.IsDBNull(24) ? 0 : reader.GetDecimal("S_PQ_ADJLOW_52W");   // 周最低价(复权)
                            objASHAREEODDERIVATIVEINDICATOR.FREE_SHARES_TODAY = reader.IsDBNull(25) ? 0 : reader.GetDecimal("FREE_SHARES_TODAY"); // 当日自由流通股本 
                            objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_TTM = reader.IsDBNull(26) ? 0 : reader.GetDecimal("NET_PROFIT_PARENT_COMP_TTM"); // 归属母公司净利润(TTM) 
                            objASHAREEODDERIVATIVEINDICATOR.NET_PROFIT_PARENT_COMP_LYR = reader.IsDBNull(27) ? 0 : reader.GetDecimal("NET_PROFIT_PARENT_COMP_LYR"); // 归属母公司净利润(LYR) 
                            objASHAREEODDERIVATIVEINDICATOR.NET_ASSETS_TODAY = reader.IsDBNull(28) ? 0 : reader.GetDecimal("NET_ASSETS_TODAY");  // 当日净资产
                            objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_TTM = reader.IsDBNull(29) ? 0 : reader.GetDecimal("NET_CASH_FLOWS_OPER_ACT_TTM"); // 经营活动产生的现金流量净额(TTM)
                            objASHAREEODDERIVATIVEINDICATOR.NET_CASH_FLOWS_OPER_ACT_LYR = reader.IsDBNull(30) ? 0 : reader.GetDecimal("NET_CASH_FLOWS_OPER_ACT_LYR"); // 经营活动产生的现金流量净额(LYR) 
                            objASHAREEODDERIVATIVEINDICATOR.OPER_REV_TTM = reader.IsDBNull(31) ? 0 : reader.GetDecimal("OPER_REV_TTM");      // 营业收入(TTM)
                            objASHAREEODDERIVATIVEINDICATOR.OPER_REV_LYR = reader.IsDBNull(32) ? 0 : reader.GetDecimal("OPER_REV_LYR");      // 营业收入(LYR) 
                            objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_TTM = reader.IsDBNull(33) ? 0 : reader.GetDecimal("NET_INCR_CASH_CASH_EQU_TTM"); // 现金及现金等价物净增加额(TTM) 
                            objASHAREEODDERIVATIVEINDICATOR.NET_INCR_CASH_CASH_EQU_LYR = reader.IsDBNull(34) ? 0 : reader.GetDecimal("NET_INCR_CASH_CASH_EQU_LYR"); // 现金及现金等价物净增加额(LYR) 
                            objASHAREEODDERIVATIVEINDICATOR.UP_DOWN_LIMIT_STATUS = reader.IsDBNull(35) ? 0 : reader.GetDecimal("UP_DOWN_LIMIT_STATUS"); // 涨跌停状态
                            objASHAREEODDERIVATIVEINDICATOR.LOWEST_HIGHEST_STATUS = reader.IsDBNull(36) ? 0 : reader.GetDecimal("LOWEST_HIGHEST_STATUS"); // 最高最低价状态
                            m_dictASHAREEODDERIVATIVEINDICATOR.Add(objASHAREEODDERIVATIVEINDICATOR.TRADE_DT, objASHAREEODDERIVATIVEINDICATOR);
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("查询ASHAREEODDERIVATIVEINDICATOR 异常：" + ex);
                        lock (m_ErrorList)
                        {
                            if (wincode != "")
                                m_ErrorList.Add(wincode, 1);
                        }
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
           
            }
            return true;
        }

       
        private bool DealASHAREL2INDICATORS(string wincode,ref Dictionary<string, ASHAREL2INDICATORS> m_dictASHAREL2INDICATORS)
        {
            MySqlConnection conn = new MySqlConnection(connetStrRead);
            if(istoday)
            {
                try
                {
                    conn.Open();
                    string sql = "select * from ASHAREL2INDICATORS  where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT = '" + date + "' ;";
                    //string sql = "select * from ASHAREL2INDICATORS  where S_INFO_WINDCODE = '" + wincode + "'  ;";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ASHAREL2INDICATORS objASHAREL2INDICATORS = new ASHAREL2INDICATORS();
                        objASHAREL2INDICATORS.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                        objASHAREL2INDICATORS.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                        objASHAREL2INDICATORS.S_LI_INITIATIVEBUYRATE = reader.IsDBNull(3) ? 0 : reader.GetDecimal("S_LI_INITIATIVEBUYRATE");            // 主买比率(%)  
                        objASHAREL2INDICATORS.S_LI_INITIATIVEBUYMONEY = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_LI_INITIATIVEBUYMONEY");            // 主买总额(万元)     
                        objASHAREL2INDICATORS.S_LI_INITIATIVEBUYAMOUNT = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_LI_INITIATIVEBUYAMOUNT");            // 主买总量(手)  
                        objASHAREL2INDICATORS.S_LI_INITIATIVESELLRATE = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_LI_INITIATIVESELLRATE");            // 主卖比率(%)  
                        objASHAREL2INDICATORS.S_LI_INITIATIVESELLMONEY = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_LI_INITIATIVESELLMONEY");            // 主卖总额(万元)     
                        objASHAREL2INDICATORS.S_LI_INITIATIVESELLAMOUNT = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_LI_INITIATIVESELLAMOUNT");            // 主卖总量(手)  
                        objASHAREL2INDICATORS.S_LI_LARGEBUYRATE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_LI_LARGEBUYRATE");            // 大买比率(%)  
                        objASHAREL2INDICATORS.S_LI_LARGEBUYMONEY = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_LI_LARGEBUYMONEY");            // 大买总额(万元)     
                        objASHAREL2INDICATORS.S_LI_LARGEBUYAMOUNT = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_LI_LARGEBUYAMOUNT");            // 大买总量(手)  
                        objASHAREL2INDICATORS.S_LI_LARGESELLRATE = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_LI_LARGESELLRATE");            // 大卖比率(%)  
                        objASHAREL2INDICATORS.S_LI_LARGESELLMONEY = reader.IsDBNull(13) ? 0 : reader.GetDecimal("S_LI_LARGESELLMONEY");            // 大卖总额(万元)     
                        objASHAREL2INDICATORS.S_LI_LARGESELLAMOUNT = reader.IsDBNull(14) ? 0 : reader.GetDecimal("S_LI_LARGESELLAMOUNT");            // 大卖总量(手)  
                        objASHAREL2INDICATORS.S_LI_ENTRUSTRATE = reader.IsDBNull(15) ? 0 : reader.GetDecimal("S_LI_ENTRUSTRATE");            // 总委比(%)   
                        objASHAREL2INDICATORS.S_LI_ENTRUDIFFERAMOUNT = reader.IsDBNull(16) ? 0 : reader.GetDecimal("S_LI_ENTRUDIFFERAMOUNT");            // 总委差量(手)  
                        objASHAREL2INDICATORS.S_LI_ENTRUDIFFERAMONEY = reader.IsDBNull(17) ? 0 : reader.GetDecimal("S_LI_ENTRUDIFFERAMONEY");            // 总委差额(万元)     
                        objASHAREL2INDICATORS.S_LI_ENTRUSTBUYMONEY = reader.IsDBNull(18) ? 0 : reader.GetDecimal("S_LI_ENTRUSTBUYMONEY");            // 总委买额(万元)     
                        objASHAREL2INDICATORS.S_LI_ENTRUSTSELLMONEY = reader.IsDBNull(19) ? 0 : reader.GetDecimal("S_LI_ENTRUSTSELLMONEY");            // 总委卖额(万元)     
                        objASHAREL2INDICATORS.S_LI_ENTRUSTBUYAMOUNT = reader.IsDBNull(20) ? 0 : reader.GetDecimal("S_LI_ENTRUSTBUYAMOUNT");            // 总委买量(手)  
                        objASHAREL2INDICATORS.S_LI_ENTRUSTSELLAMOUNT = reader.IsDBNull(21) ? 0 : reader.GetDecimal("S_LI_ENTRUSTSELLAMOUNT");            // 总委卖量(手)  
                        m_dictASHAREL2INDICATORS.Add(objASHAREL2INDICATORS.TRADE_DT, objASHAREL2INDICATORS);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("ASHAREL2INDICATORS异常：" + ex);
                    lock (m_ErrorList)
                    {
                        if (wincode != "")
                            m_ErrorList.Add(wincode, 1);
                    }
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                foreach (var begindate in Begintime)
                {
                    try
                    {
                        conn.Open();
                        string sql = "select * from ASHAREL2INDICATORS  where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT like '" + begindate + "%' ;";
                        //string sql = "select * from ASHAREL2INDICATORS  where S_INFO_WINDCODE = '" + wincode + "'  ;";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            ASHAREL2INDICATORS objASHAREL2INDICATORS = new ASHAREL2INDICATORS();
                            objASHAREL2INDICATORS.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                            objASHAREL2INDICATORS.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString("TRADE_DT");
                            objASHAREL2INDICATORS.S_LI_INITIATIVEBUYRATE = reader.IsDBNull(3) ? 0 : reader.GetDecimal("S_LI_INITIATIVEBUYRATE");            // 主买比率(%)  
                            objASHAREL2INDICATORS.S_LI_INITIATIVEBUYMONEY = reader.IsDBNull(4) ? 0 : reader.GetDecimal("S_LI_INITIATIVEBUYMONEY");            // 主买总额(万元)     
                            objASHAREL2INDICATORS.S_LI_INITIATIVEBUYAMOUNT = reader.IsDBNull(5) ? 0 : reader.GetDecimal("S_LI_INITIATIVEBUYAMOUNT");            // 主买总量(手)  
                            objASHAREL2INDICATORS.S_LI_INITIATIVESELLRATE = reader.IsDBNull(6) ? 0 : reader.GetDecimal("S_LI_INITIATIVESELLRATE");            // 主卖比率(%)  
                            objASHAREL2INDICATORS.S_LI_INITIATIVESELLMONEY = reader.IsDBNull(7) ? 0 : reader.GetDecimal("S_LI_INITIATIVESELLMONEY");            // 主卖总额(万元)     
                            objASHAREL2INDICATORS.S_LI_INITIATIVESELLAMOUNT = reader.IsDBNull(8) ? 0 : reader.GetDecimal("S_LI_INITIATIVESELLAMOUNT");            // 主卖总量(手)  
                            objASHAREL2INDICATORS.S_LI_LARGEBUYRATE = reader.IsDBNull(9) ? 0 : reader.GetDecimal("S_LI_LARGEBUYRATE");            // 大买比率(%)  
                            objASHAREL2INDICATORS.S_LI_LARGEBUYMONEY = reader.IsDBNull(10) ? 0 : reader.GetDecimal("S_LI_LARGEBUYMONEY");            // 大买总额(万元)     
                            objASHAREL2INDICATORS.S_LI_LARGEBUYAMOUNT = reader.IsDBNull(11) ? 0 : reader.GetDecimal("S_LI_LARGEBUYAMOUNT");            // 大买总量(手)  
                            objASHAREL2INDICATORS.S_LI_LARGESELLRATE = reader.IsDBNull(12) ? 0 : reader.GetDecimal("S_LI_LARGESELLRATE");            // 大卖比率(%)  
                            objASHAREL2INDICATORS.S_LI_LARGESELLMONEY = reader.IsDBNull(13) ? 0 : reader.GetDecimal("S_LI_LARGESELLMONEY");            // 大卖总额(万元)     
                            objASHAREL2INDICATORS.S_LI_LARGESELLAMOUNT = reader.IsDBNull(14) ? 0 : reader.GetDecimal("S_LI_LARGESELLAMOUNT");            // 大卖总量(手)  
                            objASHAREL2INDICATORS.S_LI_ENTRUSTRATE = reader.IsDBNull(15) ? 0 : reader.GetDecimal("S_LI_ENTRUSTRATE");            // 总委比(%)   
                            objASHAREL2INDICATORS.S_LI_ENTRUDIFFERAMOUNT = reader.IsDBNull(16) ? 0 : reader.GetDecimal("S_LI_ENTRUDIFFERAMOUNT");            // 总委差量(手)  
                            objASHAREL2INDICATORS.S_LI_ENTRUDIFFERAMONEY = reader.IsDBNull(17) ? 0 : reader.GetDecimal("S_LI_ENTRUDIFFERAMONEY");            // 总委差额(万元)     
                            objASHAREL2INDICATORS.S_LI_ENTRUSTBUYMONEY = reader.IsDBNull(18) ? 0 : reader.GetDecimal("S_LI_ENTRUSTBUYMONEY");            // 总委买额(万元)     
                            objASHAREL2INDICATORS.S_LI_ENTRUSTSELLMONEY = reader.IsDBNull(19) ? 0 : reader.GetDecimal("S_LI_ENTRUSTSELLMONEY");            // 总委卖额(万元)     
                            objASHAREL2INDICATORS.S_LI_ENTRUSTBUYAMOUNT = reader.IsDBNull(20) ? 0 : reader.GetDecimal("S_LI_ENTRUSTBUYAMOUNT");            // 总委买量(手)  
                            objASHAREL2INDICATORS.S_LI_ENTRUSTSELLAMOUNT = reader.IsDBNull(21) ? 0 : reader.GetDecimal("S_LI_ENTRUSTSELLAMOUNT");            // 总委卖量(手)  
                            m_dictASHAREL2INDICATORS.Add(objASHAREL2INDICATORS.TRADE_DT, objASHAREL2INDICATORS);
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("ASHAREL2INDICATORS异常：" + ex);
                        lock (m_ErrorList)
                        {
                            if (wincode != "")
                                m_ErrorList.Add(wincode, 1);
                        }
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            
            return true;
        }

        private bool DealASHAREMONEYFLOW(string wincode,ref Dictionary<string, ASHAREMONEYFLOW> m_dictASHAREMONEYFLOW)
        {
            MySqlConnection conn = new MySqlConnection(connetStrRead);
            if (istoday)
            {
                try
                {
                    conn.Open();
                    string sql = "select * from ASHAREMONEYFLOW where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT = '" + date + "' ;";
                    //string sql = "select * from ASHAREMONEYFLOW where S_INFO_WINDCODE = '" + wincode + "' ;";
                    MySqlCommand cmdASHAREMONEYFLOW = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmdASHAREMONEYFLOW.ExecuteReader();
                    while (reader.Read())
                    {
                        ASHAREMONEYFLOW objASHAREMONEYFLOW = new ASHAREMONEYFLOW();
                        objASHAREMONEYFLOW.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString(1); // Wind代码  
                        objASHAREMONEYFLOW.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString(2);        // 日期  
                        objASHAREMONEYFLOW.BUY_VALUE_EXLARGE_ORDER = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3); //机构买入金额(万元) 
                        objASHAREMONEYFLOW.SELL_VALUE_EXLARGE_ORDER = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);              //机构卖出金额(万元)                 
                        objASHAREMONEYFLOW.BUY_VALUE_LARGE_ORDER = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);       //大户买入金额(万元)                 
                        objASHAREMONEYFLOW.SELL_VALUE_LARGE_ORDER = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);       //大户卖出金额(万元)       
                        objASHAREMONEYFLOW.BUY_VALUE_MED_ORDER = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);       //中户买入金额(万元)                 
                        objASHAREMONEYFLOW.SELL_VALUE_MED_ORDER = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);       //中户卖出金额(万元)                 
                        objASHAREMONEYFLOW.BUY_VALUE_SMALL_ORDER = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);       //散户买入金额(万元)                 
                        objASHAREMONEYFLOW.SELL_VALUE_SMALL_ORDER = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);       //散户卖出金额(万元)                 
                        objASHAREMONEYFLOW.BUY_VOLUME_EXLARGE_ORDER = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);       //机构买入总量(手)                  
                        objASHAREMONEYFLOW.SELL_VOLUME_EXLARGE_ORDER = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);       //机构卖出总量(手)                  
                        objASHAREMONEYFLOW.BUY_VOLUME_LARGE_ORDER = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);        //大户买入总量(手)                  
                        objASHAREMONEYFLOW.SELL_VOLUME_LARGE_ORDER = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14);        //大户卖出总量(手)                  
                        objASHAREMONEYFLOW.BUY_VOLUME_MED_ORDER = reader.IsDBNull(15) ? 0 : reader.GetDecimal(15);        //中户买入总量(手)                  
                        objASHAREMONEYFLOW.SELL_VOLUME_MED_ORDER = reader.IsDBNull(16) ? 0 : reader.GetDecimal(16);        //中户卖出总量(手)                  
                        objASHAREMONEYFLOW.BUY_VOLUME_SMALL_ORDER = reader.IsDBNull(17) ? 0 : reader.GetDecimal(17);        //散户买入总量(手)                  
                        objASHAREMONEYFLOW.SELL_VOLUME_SMALL_ORDER = reader.IsDBNull(18) ? 0 : reader.GetDecimal(18);        //散户卖出总量(手)                  
                        objASHAREMONEYFLOW.TRADES_COUNT = reader.IsDBNull(19) ? 0 : reader.GetDecimal(19);        //成交笔数(笔)                
                        objASHAREMONEYFLOW.BUY_TRADES_EXLARGE_ORDER = reader.IsDBNull(20) ? 0 : reader.GetDecimal(20);        //机构买入单数(单)                  
                        objASHAREMONEYFLOW.SELL_TRADES_EXLARGE_ORDER = reader.IsDBNull(21) ? 0 : reader.GetDecimal(21);        //机构卖出单数(单)                  
                        objASHAREMONEYFLOW.BUY_TRADES_LARGE_ORDER = reader.IsDBNull(22) ? 0 : reader.GetDecimal(22);        //大户买入单数(单)                  
                        objASHAREMONEYFLOW.SELL_TRADES_LARGE_ORDER = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23);        //大户卖出单数(单)                  
                        objASHAREMONEYFLOW.BUY_TRADES_MED_ORDER = reader.IsDBNull(24) ? 0 : reader.GetDecimal(24);        //中户买入单数(单)                  
                        objASHAREMONEYFLOW.SELL_TRADES_MED_ORDER = reader.IsDBNull(25) ? 0 : reader.GetDecimal(25);        //中户卖出单数(单)                  
                        objASHAREMONEYFLOW.BUY_TRADES_SMALL_ORDER = reader.IsDBNull(26) ? 0 : reader.GetDecimal(26);        //散户买入单数(单)                  
                        objASHAREMONEYFLOW.SELL_TRADES_SMALL_ORDER = reader.IsDBNull(27) ? 0 : reader.GetDecimal(27);        //散户卖出单数(单)                  
                        objASHAREMONEYFLOW.VOLUME_DIFF_SMALL_TRADER = reader.IsDBNull(28) ? 0 : reader.GetDecimal(28);        //散户量差(含主动被动)(手)				 
                        objASHAREMONEYFLOW.VOLUME_DIFF_SMALL_TRADER_ACT = reader.IsDBNull(29) ? 0 : reader.GetDecimal("VOLUME_DIFF_SMALL_TRADER_ACT");        //散户量差(仅主动)(手)				 
                        objASHAREMONEYFLOW.VOLUME_DIFF_MED_TRADER = reader.IsDBNull(30) ? 0 : reader.GetDecimal("VOLUME_DIFF_MED_TRADER");        //中户量差(含主动被动)(手)				 
                        objASHAREMONEYFLOW.VOLUME_DIFF_MED_TRADER_ACT = reader.IsDBNull(31) ? 0 : reader.GetDecimal("VOLUME_DIFF_MED_TRADER_ACT");        //中户量差(仅主动)(手)				 
                        objASHAREMONEYFLOW.VOLUME_DIFF_LARGE_TRADER = reader.IsDBNull(32) ? 0 : reader.GetDecimal("VOLUME_DIFF_LARGE_TRADER");        //大户量差(含主动被动)(手)				 
                        objASHAREMONEYFLOW.VOLUME_DIFF_LARGE_TRADER_ACT = reader.IsDBNull(33) ? 0 : reader.GetDecimal("VOLUME_DIFF_LARGE_TRADER_ACT");        //大户量差(仅主动)(手)				 
                        objASHAREMONEYFLOW.VOLUME_DIFF_INSTITUTE = reader.IsDBNull(34) ? 0 : reader.GetDecimal("VOLUME_DIFF_INSTITUTE");        //机构量差(含主动被动)(手)				 
                        objASHAREMONEYFLOW.VOLUME_DIFF_INSTITUTE_ACT = reader.IsDBNull(35) ? 0 : reader.GetDecimal("VOLUME_DIFF_INSTITUTE_ACT");        //机构量差(仅主动)(手)				 
                        objASHAREMONEYFLOW.VALUE_DIFF_SMALL_TRADER = reader.IsDBNull(36) ? 0 : reader.GetDecimal("VALUE_DIFF_SMALL_TRADER");        //散户金额差(含主动被动)(万元)				 
                        objASHAREMONEYFLOW.VALUE_DIFF_SMALL_TRADER_ACT = reader.IsDBNull(37) ? 0 : reader.GetDecimal("VALUE_DIFF_SMALL_TRADER_ACT");        //散户金额差(仅主动)(万元)				 
                        objASHAREMONEYFLOW.VALUE_DIFF_MED_TRADER = reader.IsDBNull(38) ? 0 : reader.GetDecimal("VALUE_DIFF_MED_TRADER");        //中户金额差(含主动被动)(万元)				 
                        objASHAREMONEYFLOW.VALUE_DIFF_MED_TRADER_ACT = reader.IsDBNull(39) ? 0 : reader.GetDecimal("VALUE_DIFF_MED_TRADER_ACT");        //中户金额差(仅主动)(万元)				 
                        objASHAREMONEYFLOW.VALUE_DIFF_LARGE_TRADER = reader.IsDBNull(40) ? 0 : reader.GetDecimal("VALUE_DIFF_LARGE_TRADER");        //大户金额差(含主动被动)(万元)				 
                        objASHAREMONEYFLOW.VALUE_DIFF_LARGE_TRADER_ACT = reader.IsDBNull(41) ? 0 : reader.GetDecimal("VALUE_DIFF_MED_TRADER_ACT");        //大户金额差(仅主动)(万元)				 
                        objASHAREMONEYFLOW.VALUE_DIFF_INSTITUTE = reader.IsDBNull(42) ? 0 : reader.GetDecimal("VALUE_DIFF_INSTITUTE");        //机构金额差(含主动被动)(万元)				 
                        objASHAREMONEYFLOW.VALUE_DIFF_INSTITUTE_ACT = reader.IsDBNull(43) ? 0 : reader.GetDecimal("VALUE_DIFF_INSTITUTE_ACT");        //机构金额差(仅主动)(万元)				 
                        objASHAREMONEYFLOW.S_MFD_INFLOWVOLUME = reader.IsDBNull(44) ? 0 : reader.GetDecimal("S_MFD_INFLOWVOLUME");        //净流入量(手)                
                        objASHAREMONEYFLOW.NET_INFLOW_RATE_VOLUME = reader.IsDBNull(45) ? 0 : reader.GetDecimal("NET_INFLOW_RATE_VOLUME");        //流入率(量)(%)			
                        objASHAREMONEYFLOW.S_MFD_INFLOW_OPENVOLUME = reader.IsDBNull(46) ? 0 : reader.GetDecimal("S_MFD_INFLOW_OPENVOLUME");        //开盘资金流入量(手)                 
                        objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VOLUME = reader.IsDBNull(47) ? 0 : reader.GetDecimal("OPEN_NET_INFLOW_RATE_VOLUME");        //开盘资金流入率(量)(%)				 
                        objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSEVOLUME = reader.IsDBNull(48) ? 0 : reader.GetDecimal("S_MFD_INFLOW_CLOSEVOLUME");        //尾盘资金流入量(手)                 
                        objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VOLUME = reader.IsDBNull(49) ? 0 : reader.GetDecimal("CLOSE_NET_INFLOW_RATE_VOLUME");        //尾盘资金流入率(量)(%)				 
                        objASHAREMONEYFLOW.S_MFD_INFLOW = reader.IsDBNull(50) ? 0 : reader.GetDecimal("S_MFD_INFLOW");        //净流入金额(万元)                  
                        objASHAREMONEYFLOW.NET_INFLOW_RATE_VALUE = reader.IsDBNull(51) ? 0 : reader.GetDecimal("NET_INFLOW_RATE_VALUE");        //流入率(金额)                
                        objASHAREMONEYFLOW.S_MFD_INFLOW_OPEN = reader.IsDBNull(52) ? 0 : reader.GetDecimal("S_MFD_INFLOW_OPEN");        //开盘资金流入金额(万元)                   
                        objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VALUE = reader.IsDBNull(53) ? 0 : reader.GetDecimal("OPEN_NET_INFLOW_RATE_VALUE");        //开盘资金流入率(金额)                
                        objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSE = reader.IsDBNull(54) ? 0 : reader.GetDecimal("S_MFD_INFLOW_CLOSE");        //尾盘资金流入金额(万元)                   
                        objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VALUE = reader.IsDBNull(55) ? 0 : reader.GetDecimal("CLOSE_NET_INFLOW_RATE_VALUE");        //尾盘资金流入率(金额)                
                        objASHAREMONEYFLOW.TOT_VOLUME_BID = reader.IsDBNull(56) ? 0 : reader.GetDecimal("TOT_VOLUME_BID");        //委买总量(手)                
                        objASHAREMONEYFLOW.TOT_VOLUME_ASK = reader.IsDBNull(57) ? 0 : reader.GetDecimal("TOT_VOLUME_ASK");        //委卖总量(手)                
                        objASHAREMONEYFLOW.MONEYFLOW_PCT_VOLUME = reader.IsDBNull(58) ? 0 : reader.GetDecimal("MONEYFLOW_PCT_VOLUME");        //资金流向占比(量)(%)				 
                        objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VOLUME = reader.IsDBNull(59) ? 0 : reader.GetDecimal("OPEN_MONEYFLOW_PCT_VOLUME");        //开盘资金流向占比(量)(%)				 
                        objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VOLUME = reader.IsDBNull(60) ? 0 : reader.GetDecimal("CLOSE_MONEYFLOW_PCT_VOLUME");        //尾盘资金流向占比(量)(%)				 
                        objASHAREMONEYFLOW.MONEYFLOW_PCT_VALUE = reader.IsDBNull(61) ? 0 : reader.GetDecimal("MONEYFLOW_PCT_VALUE");        //资金流向占比(金额)                 
                        objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VALUE = reader.IsDBNull(62) ? 0 : reader.GetDecimal("OPEN_MONEYFLOW_PCT_VALUE");        //开盘资金流向占比(金额)                   
                        objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VALUE = reader.IsDBNull(63) ? 0 : reader.GetDecimal("CLOSE_MONEYFLOW_PCT_VALUE");        //尾盘资金流向占比(金额)                   
                        objASHAREMONEYFLOW.S_MFD_INFLOWVOLUME_LARGE_ORDER = reader.IsDBNull(64) ? 0 : reader.GetDecimal("S_MFD_INFLOWVOLUME_LARGE_ORDER");        //大单净流入量(手)                  
                        objASHAREMONEYFLOW.NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(65) ? 0 : reader.GetDecimal("NET_INFLOW_RATE_VOLUME_L");       //大单流入率(量)(%)				 
                        objASHAREMONEYFLOW.S_MFD_INFLOW_LARGE_ORDER = reader.IsDBNull(66) ? 0 : reader.GetDecimal("S_MFD_INFLOW_LARGE_ORDER");        //大单净流入金额(万元)                
                        objASHAREMONEYFLOW.NET_INFLOW_RATE_VALUE_L = reader.IsDBNull(67) ? 0 : reader.GetDecimal("NET_INFLOW_RATE_VALUE_L");     //[内部]大单流入率(金额)(%)				 
                        objASHAREMONEYFLOW.MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(68) ? 0 : reader.GetDecimal("MONEYFLOW_PCT_VOLUME_L");         // 大单资金流向占比(量)(%)				 
                        objASHAREMONEYFLOW.MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(69) ? 0 : reader.GetDecimal("MONEYFLOW_PCT_VALUE_L");         // [内部]大单资金流向占比(金额)(%)				 
                        objASHAREMONEYFLOW.S_MFD_INFLOW_OPENVOLUME_L = reader.IsDBNull(70) ? 0 : reader.GetDecimal("S_MFD_INFLOW_OPENVOLUME_L");        //大单开盘资金流入量(手)                   
                        objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(71) ? 0 : reader.GetDecimal("OPEN_NET_INFLOW_RATE_VOLUME_L");         // [内部]大单开盘资金流入率(量)(%)				 
                        objASHAREMONEYFLOW.S_MFD_INFLOW_OPEN_LARGE_ORDER = reader.IsDBNull(72) ? 0 : reader.GetDecimal("S_MFD_INFLOW_OPEN_LARGE_ORDER");        //大单开盘资金流入金额(万元)                 
                        objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VALUE_L = reader.IsDBNull(73) ? 0 : reader.GetDecimal("OPEN_NET_INFLOW_RATE_VALUE_L");         // [内部]大单开盘资金流入率(金额)(%)				 
                        objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(74) ? 0 : reader.GetDecimal("OPEN_MONEYFLOW_PCT_VOLUME_L");         // [内部]大单开盘资金流向占比(量)(%)				 
                        objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(75) ? 0 : reader.GetDecimal("OPEN_MONEYFLOW_PCT_VALUE_L");         // 大单开盘资金流向占比(金额)(%)				 
                        objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSEVOLUME_L = reader.IsDBNull(76) ? 0 : reader.GetDecimal("S_MFD_INFLOW_CLOSEVOLUME_L");        //大单尾盘资金流入量(手)                   
                        objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(77) ? 0 : reader.GetDecimal("CLOSE_NET_INFLOW_RATE_VOLUME_L");         // [内部]大单尾盘资金流入率(量)(%)				 
                        objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSE_LARGE_ORDER = reader.IsDBNull(78) ? 0 : reader.GetDecimal("S_MFD_INFLOW_CLOSE_LARGE_ORDER");        //大单尾盘资金流入金额(万元)                 
                        objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VALU_L = reader.IsDBNull(79) ? 0 : reader.GetDecimal("CLOSE_NET_INFLOW_RATE_VALU_L");         // [内部]大单尾盘资金流入率(金额)(%)				 
                        objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(80) ? 0 : reader.GetDecimal("CLOSE_MONEYFLOW_PCT_VOLUME_L");         // 大单尾盘资金流向占比(量)(%)				 
                        objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(81) ? 0 : reader.GetDecimal("CLOSE_MONEYFLOW_PCT_VALUE_L");         // [内部]大单尾盘资金流向占比(金额)(%)				 
                        objASHAREMONEYFLOW.BUY_VALUE_EXLARGE_ORDER_ACT = reader.IsDBNull(82) ? 0 : reader.GetDecimal("BUY_VALUE_EXLARGE_ORDER_ACT");        //机构买入金额(仅主动)(万元)				 
                        objASHAREMONEYFLOW.SELL_VALUE_EXLARGE_ORDER_ACT = reader.IsDBNull(83) ? 0 : reader.GetDecimal("SELL_VALUE_EXLARGE_ORDER_ACT");        //机构卖出金额(仅主动)(万元)				 
                        objASHAREMONEYFLOW.BUY_VALUE_LARGE_ORDER_ACT = reader.IsDBNull(84) ? 0 : reader.GetDecimal("BUY_VALUE_LARGE_ORDER_ACT");        //大户买入金额(仅主动)(万元)				 
                        objASHAREMONEYFLOW.SELL_VALUE_LARGE_ORDER_ACT = reader.IsDBNull(85) ? 0 : reader.GetDecimal("SELL_VALUE_LARGE_ORDER_ACT");        //大户卖出金额(仅主动)(万元)				 
                        objASHAREMONEYFLOW.BUY_VALUE_MED_ORDER_ACT = reader.IsDBNull(86) ? 0 : reader.GetDecimal("BUY_VALUE_MED_ORDER_ACT");        //中户买入金额(仅主动)(万元)				 
                        objASHAREMONEYFLOW.SELL_VALUE_MED_ORDER_ACT = reader.IsDBNull(87) ? 0 : reader.GetDecimal("SELL_VALUE_MED_ORDER_ACT");        //中户卖出金额(仅主动)(万元)				 
                        objASHAREMONEYFLOW.BUY_VALUE_SMALL_ORDER_ACT = reader.IsDBNull(88) ? 0 : reader.GetDecimal("BUY_VALUE_SMALL_ORDER_ACT");        //散户买入金额(仅主动)(万元)				 
                        objASHAREMONEYFLOW.SELL_VALUE_SMALL_ORDER_ACT = reader.IsDBNull(89) ? 0 : reader.GetDecimal("SELL_VALUE_SMALL_ORDER_ACT");        //散户卖出金额(仅主动)(万元)				 
                        objASHAREMONEYFLOW.BUY_VOLUME_EXLARGE_ORDER_ACT = reader.IsDBNull(90) ? 0 : reader.GetDecimal("BUY_VOLUME_EXLARGE_ORDER_ACT");        //机构买入总量(仅主动)(万股)				 
                        objASHAREMONEYFLOW.SELL_VOLUME_EXLARGE_ORDER_ACT = reader.IsDBNull(91) ? 0 : reader.GetDecimal("SELL_VOLUME_EXLARGE_ORDER_ACT");        //机构卖出总量(仅主动)(万股)				 
                        objASHAREMONEYFLOW.BUY_VOLUME_LARGE_ORDER_ACT = reader.IsDBNull(92) ? 0 : reader.GetDecimal("BUY_VOLUME_LARGE_ORDER_ACT");        //大户买入总量(仅主动)(万股)				 
                        objASHAREMONEYFLOW.SELL_VOLUME_LARGE_ORDER_ACT = reader.IsDBNull(93) ? 0 : reader.GetDecimal("SELL_VOLUME_LARGE_ORDER_ACT");        //大户卖出总量(仅主动)(万股)				 
                        objASHAREMONEYFLOW.BUY_VOLUME_MED_ORDER_ACT = reader.IsDBNull(94) ? 0 : reader.GetDecimal("BUY_VOLUME_MED_ORDER_ACT");        //中户买入总量(仅主动)(万股)				 
                        objASHAREMONEYFLOW.SELL_VOLUME_MED_ORDER_ACT = reader.IsDBNull(95) ? 0 : reader.GetDecimal("SELL_VOLUME_MED_ORDER_ACT");        //中户卖出总量(仅主动)(万股)				 
                        objASHAREMONEYFLOW.BUY_VOLUME_SMALL_ORDER_ACT = reader.IsDBNull(96) ? 0 : reader.GetDecimal("BUY_VOLUME_SMALL_ORDER_ACT");        //散户买入总量(仅主动)(万股)				 
                        objASHAREMONEYFLOW.SELL_VOLUME_SMALL_ORDER_ACT = reader.IsDBNull(97) ? 0 : reader.GetDecimal("SELL_VOLUME_SMALL_ORDER_ACT");        //散户卖出总量(仅主动)(万股)	

                        m_dictASHAREMONEYFLOW.Add(objASHAREMONEYFLOW.TRADE_DT, objASHAREMONEYFLOW);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询ASHAREEODDERIVATIVEINDICATOR 异常：" + ex);
                    lock (m_ErrorList)
                    {
                        if (wincode != "")
                            m_ErrorList.Add(wincode, 1);
                    }
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                    foreach (var begindate in Begintime)
                    {
                        try
                        {
                            conn.Open();
                            string sql = "select * from ASHAREMONEYFLOW where S_INFO_WINDCODE = '" + wincode + "' and TRADE_DT like '" + begindate + "%' ;";
                            //string sql = "select * from ASHAREMONEYFLOW where S_INFO_WINDCODE = '" + wincode + "' ;";
                            MySqlCommand cmdASHAREMONEYFLOW = new MySqlCommand(sql, conn);
                            MySqlDataReader reader = cmdASHAREMONEYFLOW.ExecuteReader();
                            while (reader.Read())
                            {
                                ASHAREMONEYFLOW objASHAREMONEYFLOW = new ASHAREMONEYFLOW();
                                objASHAREMONEYFLOW.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString(1); // Wind代码  
                                objASHAREMONEYFLOW.TRADE_DT = reader.IsDBNull(2) ? "" : reader.GetString(2);        // 日期  
                                objASHAREMONEYFLOW.BUY_VALUE_EXLARGE_ORDER = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3); //机构买入金额(万元) 
                                objASHAREMONEYFLOW.SELL_VALUE_EXLARGE_ORDER = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);              //机构卖出金额(万元)                 
                                objASHAREMONEYFLOW.BUY_VALUE_LARGE_ORDER = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);       //大户买入金额(万元)                 
                                objASHAREMONEYFLOW.SELL_VALUE_LARGE_ORDER = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);       //大户卖出金额(万元)       
                                objASHAREMONEYFLOW.BUY_VALUE_MED_ORDER = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);       //中户买入金额(万元)                 
                                objASHAREMONEYFLOW.SELL_VALUE_MED_ORDER = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);       //中户卖出金额(万元)                 
                                objASHAREMONEYFLOW.BUY_VALUE_SMALL_ORDER = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);       //散户买入金额(万元)                 
                                objASHAREMONEYFLOW.SELL_VALUE_SMALL_ORDER = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);       //散户卖出金额(万元)                 
                                objASHAREMONEYFLOW.BUY_VOLUME_EXLARGE_ORDER = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);       //机构买入总量(手)                  
                                objASHAREMONEYFLOW.SELL_VOLUME_EXLARGE_ORDER = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);       //机构卖出总量(手)                  
                                objASHAREMONEYFLOW.BUY_VOLUME_LARGE_ORDER = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);        //大户买入总量(手)                  
                                objASHAREMONEYFLOW.SELL_VOLUME_LARGE_ORDER = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14);        //大户卖出总量(手)                  
                                objASHAREMONEYFLOW.BUY_VOLUME_MED_ORDER = reader.IsDBNull(15) ? 0 : reader.GetDecimal(15);        //中户买入总量(手)                  
                                objASHAREMONEYFLOW.SELL_VOLUME_MED_ORDER = reader.IsDBNull(16) ? 0 : reader.GetDecimal(16);        //中户卖出总量(手)                  
                                objASHAREMONEYFLOW.BUY_VOLUME_SMALL_ORDER = reader.IsDBNull(17) ? 0 : reader.GetDecimal(17);        //散户买入总量(手)                  
                                objASHAREMONEYFLOW.SELL_VOLUME_SMALL_ORDER = reader.IsDBNull(18) ? 0 : reader.GetDecimal(18);        //散户卖出总量(手)                  
                                objASHAREMONEYFLOW.TRADES_COUNT = reader.IsDBNull(19) ? 0 : reader.GetDecimal(19);        //成交笔数(笔)                
                                objASHAREMONEYFLOW.BUY_TRADES_EXLARGE_ORDER = reader.IsDBNull(20) ? 0 : reader.GetDecimal(20);        //机构买入单数(单)                  
                                objASHAREMONEYFLOW.SELL_TRADES_EXLARGE_ORDER = reader.IsDBNull(21) ? 0 : reader.GetDecimal(21);        //机构卖出单数(单)                  
                                objASHAREMONEYFLOW.BUY_TRADES_LARGE_ORDER = reader.IsDBNull(22) ? 0 : reader.GetDecimal(22);        //大户买入单数(单)                  
                                objASHAREMONEYFLOW.SELL_TRADES_LARGE_ORDER = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23);        //大户卖出单数(单)                  
                                objASHAREMONEYFLOW.BUY_TRADES_MED_ORDER = reader.IsDBNull(24) ? 0 : reader.GetDecimal(24);        //中户买入单数(单)                  
                                objASHAREMONEYFLOW.SELL_TRADES_MED_ORDER = reader.IsDBNull(25) ? 0 : reader.GetDecimal(25);        //中户卖出单数(单)                  
                                objASHAREMONEYFLOW.BUY_TRADES_SMALL_ORDER = reader.IsDBNull(26) ? 0 : reader.GetDecimal(26);        //散户买入单数(单)                  
                                objASHAREMONEYFLOW.SELL_TRADES_SMALL_ORDER = reader.IsDBNull(27) ? 0 : reader.GetDecimal(27);        //散户卖出单数(单)                  
                                objASHAREMONEYFLOW.VOLUME_DIFF_SMALL_TRADER = reader.IsDBNull(28) ? 0 : reader.GetDecimal(28);        //散户量差(含主动被动)(手)				 
                                objASHAREMONEYFLOW.VOLUME_DIFF_SMALL_TRADER_ACT = reader.IsDBNull(29) ? 0 : reader.GetDecimal("VOLUME_DIFF_SMALL_TRADER_ACT");        //散户量差(仅主动)(手)				 
                                objASHAREMONEYFLOW.VOLUME_DIFF_MED_TRADER = reader.IsDBNull(30) ? 0 : reader.GetDecimal("VOLUME_DIFF_MED_TRADER");        //中户量差(含主动被动)(手)				 
                                objASHAREMONEYFLOW.VOLUME_DIFF_MED_TRADER_ACT = reader.IsDBNull(31) ? 0 : reader.GetDecimal("VOLUME_DIFF_MED_TRADER_ACT");        //中户量差(仅主动)(手)				 
                                objASHAREMONEYFLOW.VOLUME_DIFF_LARGE_TRADER = reader.IsDBNull(32) ? 0 : reader.GetDecimal("VOLUME_DIFF_LARGE_TRADER");        //大户量差(含主动被动)(手)				 
                                objASHAREMONEYFLOW.VOLUME_DIFF_LARGE_TRADER_ACT = reader.IsDBNull(33) ? 0 : reader.GetDecimal("VOLUME_DIFF_LARGE_TRADER_ACT");        //大户量差(仅主动)(手)				 
                                objASHAREMONEYFLOW.VOLUME_DIFF_INSTITUTE = reader.IsDBNull(34) ? 0 : reader.GetDecimal("VOLUME_DIFF_INSTITUTE");        //机构量差(含主动被动)(手)				 
                                objASHAREMONEYFLOW.VOLUME_DIFF_INSTITUTE_ACT = reader.IsDBNull(35) ? 0 : reader.GetDecimal("VOLUME_DIFF_INSTITUTE_ACT");        //机构量差(仅主动)(手)				 
                                objASHAREMONEYFLOW.VALUE_DIFF_SMALL_TRADER = reader.IsDBNull(36) ? 0 : reader.GetDecimal("VALUE_DIFF_SMALL_TRADER");        //散户金额差(含主动被动)(万元)				 
                                objASHAREMONEYFLOW.VALUE_DIFF_SMALL_TRADER_ACT = reader.IsDBNull(37) ? 0 : reader.GetDecimal("VALUE_DIFF_SMALL_TRADER_ACT");        //散户金额差(仅主动)(万元)				 
                                objASHAREMONEYFLOW.VALUE_DIFF_MED_TRADER = reader.IsDBNull(38) ? 0 : reader.GetDecimal("VALUE_DIFF_MED_TRADER");        //中户金额差(含主动被动)(万元)				 
                                objASHAREMONEYFLOW.VALUE_DIFF_MED_TRADER_ACT = reader.IsDBNull(39) ? 0 : reader.GetDecimal("VALUE_DIFF_MED_TRADER_ACT");        //中户金额差(仅主动)(万元)				 
                                objASHAREMONEYFLOW.VALUE_DIFF_LARGE_TRADER = reader.IsDBNull(40) ? 0 : reader.GetDecimal("VALUE_DIFF_LARGE_TRADER");        //大户金额差(含主动被动)(万元)				 
                                objASHAREMONEYFLOW.VALUE_DIFF_LARGE_TRADER_ACT = reader.IsDBNull(41) ? 0 : reader.GetDecimal("VALUE_DIFF_MED_TRADER_ACT");        //大户金额差(仅主动)(万元)				 
                                objASHAREMONEYFLOW.VALUE_DIFF_INSTITUTE = reader.IsDBNull(42) ? 0 : reader.GetDecimal("VALUE_DIFF_INSTITUTE");        //机构金额差(含主动被动)(万元)				 
                                objASHAREMONEYFLOW.VALUE_DIFF_INSTITUTE_ACT = reader.IsDBNull(43) ? 0 : reader.GetDecimal("VALUE_DIFF_INSTITUTE_ACT");        //机构金额差(仅主动)(万元)				 
                                objASHAREMONEYFLOW.S_MFD_INFLOWVOLUME = reader.IsDBNull(44) ? 0 : reader.GetDecimal("S_MFD_INFLOWVOLUME");        //净流入量(手)                
                                objASHAREMONEYFLOW.NET_INFLOW_RATE_VOLUME = reader.IsDBNull(45) ? 0 : reader.GetDecimal("NET_INFLOW_RATE_VOLUME");        //流入率(量)(%)			
                                objASHAREMONEYFLOW.S_MFD_INFLOW_OPENVOLUME = reader.IsDBNull(46) ? 0 : reader.GetDecimal("S_MFD_INFLOW_OPENVOLUME");        //开盘资金流入量(手)                 
                                objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VOLUME = reader.IsDBNull(47) ? 0 : reader.GetDecimal("OPEN_NET_INFLOW_RATE_VOLUME");        //开盘资金流入率(量)(%)				 
                                objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSEVOLUME = reader.IsDBNull(48) ? 0 : reader.GetDecimal("S_MFD_INFLOW_CLOSEVOLUME");        //尾盘资金流入量(手)                 
                                objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VOLUME = reader.IsDBNull(49) ? 0 : reader.GetDecimal("CLOSE_NET_INFLOW_RATE_VOLUME");        //尾盘资金流入率(量)(%)				 
                                objASHAREMONEYFLOW.S_MFD_INFLOW = reader.IsDBNull(50) ? 0 : reader.GetDecimal("S_MFD_INFLOW");        //净流入金额(万元)                  
                                objASHAREMONEYFLOW.NET_INFLOW_RATE_VALUE = reader.IsDBNull(51) ? 0 : reader.GetDecimal("NET_INFLOW_RATE_VALUE");        //流入率(金额)                
                                objASHAREMONEYFLOW.S_MFD_INFLOW_OPEN = reader.IsDBNull(52) ? 0 : reader.GetDecimal("S_MFD_INFLOW_OPEN");        //开盘资金流入金额(万元)                   
                                objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VALUE = reader.IsDBNull(53) ? 0 : reader.GetDecimal("OPEN_NET_INFLOW_RATE_VALUE");        //开盘资金流入率(金额)                
                                objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSE = reader.IsDBNull(54) ? 0 : reader.GetDecimal("S_MFD_INFLOW_CLOSE");        //尾盘资金流入金额(万元)                   
                                objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VALUE = reader.IsDBNull(55) ? 0 : reader.GetDecimal("CLOSE_NET_INFLOW_RATE_VALUE");        //尾盘资金流入率(金额)                
                                objASHAREMONEYFLOW.TOT_VOLUME_BID = reader.IsDBNull(56) ? 0 : reader.GetDecimal("TOT_VOLUME_BID");        //委买总量(手)                
                                objASHAREMONEYFLOW.TOT_VOLUME_ASK = reader.IsDBNull(57) ? 0 : reader.GetDecimal("TOT_VOLUME_ASK");        //委卖总量(手)                
                                objASHAREMONEYFLOW.MONEYFLOW_PCT_VOLUME = reader.IsDBNull(58) ? 0 : reader.GetDecimal("MONEYFLOW_PCT_VOLUME");        //资金流向占比(量)(%)				 
                                objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VOLUME = reader.IsDBNull(59) ? 0 : reader.GetDecimal("OPEN_MONEYFLOW_PCT_VOLUME");        //开盘资金流向占比(量)(%)				 
                                objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VOLUME = reader.IsDBNull(60) ? 0 : reader.GetDecimal("CLOSE_MONEYFLOW_PCT_VOLUME");        //尾盘资金流向占比(量)(%)				 
                                objASHAREMONEYFLOW.MONEYFLOW_PCT_VALUE = reader.IsDBNull(61) ? 0 : reader.GetDecimal("MONEYFLOW_PCT_VALUE");        //资金流向占比(金额)                 
                                objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VALUE = reader.IsDBNull(62) ? 0 : reader.GetDecimal("OPEN_MONEYFLOW_PCT_VALUE");        //开盘资金流向占比(金额)                   
                                objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VALUE = reader.IsDBNull(63) ? 0 : reader.GetDecimal("CLOSE_MONEYFLOW_PCT_VALUE");        //尾盘资金流向占比(金额)                   
                                objASHAREMONEYFLOW.S_MFD_INFLOWVOLUME_LARGE_ORDER = reader.IsDBNull(64) ? 0 : reader.GetDecimal("S_MFD_INFLOWVOLUME_LARGE_ORDER");        //大单净流入量(手)                  
                                objASHAREMONEYFLOW.NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(65) ? 0 : reader.GetDecimal("NET_INFLOW_RATE_VOLUME_L");       //大单流入率(量)(%)				 
                                objASHAREMONEYFLOW.S_MFD_INFLOW_LARGE_ORDER = reader.IsDBNull(66) ? 0 : reader.GetDecimal("S_MFD_INFLOW_LARGE_ORDER");        //大单净流入金额(万元)                
                                objASHAREMONEYFLOW.NET_INFLOW_RATE_VALUE_L = reader.IsDBNull(67) ? 0 : reader.GetDecimal("NET_INFLOW_RATE_VALUE_L");     //[内部]大单流入率(金额)(%)				 
                                objASHAREMONEYFLOW.MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(68) ? 0 : reader.GetDecimal("MONEYFLOW_PCT_VOLUME_L");         // 大单资金流向占比(量)(%)				 
                                objASHAREMONEYFLOW.MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(69) ? 0 : reader.GetDecimal("MONEYFLOW_PCT_VALUE_L");         // [内部]大单资金流向占比(金额)(%)				 
                                objASHAREMONEYFLOW.S_MFD_INFLOW_OPENVOLUME_L = reader.IsDBNull(70) ? 0 : reader.GetDecimal("S_MFD_INFLOW_OPENVOLUME_L");        //大单开盘资金流入量(手)                   
                                objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(71) ? 0 : reader.GetDecimal("OPEN_NET_INFLOW_RATE_VOLUME_L");         // [内部]大单开盘资金流入率(量)(%)				 
                                objASHAREMONEYFLOW.S_MFD_INFLOW_OPEN_LARGE_ORDER = reader.IsDBNull(72) ? 0 : reader.GetDecimal("S_MFD_INFLOW_OPEN_LARGE_ORDER");        //大单开盘资金流入金额(万元)                 
                                objASHAREMONEYFLOW.OPEN_NET_INFLOW_RATE_VALUE_L = reader.IsDBNull(73) ? 0 : reader.GetDecimal("OPEN_NET_INFLOW_RATE_VALUE_L");         // [内部]大单开盘资金流入率(金额)(%)				 
                                objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(74) ? 0 : reader.GetDecimal("OPEN_MONEYFLOW_PCT_VOLUME_L");         // [内部]大单开盘资金流向占比(量)(%)				 
                                objASHAREMONEYFLOW.OPEN_MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(75) ? 0 : reader.GetDecimal("OPEN_MONEYFLOW_PCT_VALUE_L");         // 大单开盘资金流向占比(金额)(%)				 
                                objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSEVOLUME_L = reader.IsDBNull(76) ? 0 : reader.GetDecimal("S_MFD_INFLOW_CLOSEVOLUME_L");        //大单尾盘资金流入量(手)                   
                                objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VOLUME_L = reader.IsDBNull(77) ? 0 : reader.GetDecimal("CLOSE_NET_INFLOW_RATE_VOLUME_L");         // [内部]大单尾盘资金流入率(量)(%)				 
                                objASHAREMONEYFLOW.S_MFD_INFLOW_CLOSE_LARGE_ORDER = reader.IsDBNull(78) ? 0 : reader.GetDecimal("S_MFD_INFLOW_CLOSE_LARGE_ORDER");        //大单尾盘资金流入金额(万元)                 
                                objASHAREMONEYFLOW.CLOSE_NET_INFLOW_RATE_VALU_L = reader.IsDBNull(79) ? 0 : reader.GetDecimal("CLOSE_NET_INFLOW_RATE_VALU_L");         // [内部]大单尾盘资金流入率(金额)(%)				 
                                objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VOLUME_L = reader.IsDBNull(80) ? 0 : reader.GetDecimal("CLOSE_MONEYFLOW_PCT_VOLUME_L");         // 大单尾盘资金流向占比(量)(%)				 
                                objASHAREMONEYFLOW.CLOSE_MONEYFLOW_PCT_VALUE_L = reader.IsDBNull(81) ? 0 : reader.GetDecimal("CLOSE_MONEYFLOW_PCT_VALUE_L");         // [内部]大单尾盘资金流向占比(金额)(%)				 
                                objASHAREMONEYFLOW.BUY_VALUE_EXLARGE_ORDER_ACT = reader.IsDBNull(82) ? 0 : reader.GetDecimal("BUY_VALUE_EXLARGE_ORDER_ACT");        //机构买入金额(仅主动)(万元)				 
                                objASHAREMONEYFLOW.SELL_VALUE_EXLARGE_ORDER_ACT = reader.IsDBNull(83) ? 0 : reader.GetDecimal("SELL_VALUE_EXLARGE_ORDER_ACT");        //机构卖出金额(仅主动)(万元)				 
                                objASHAREMONEYFLOW.BUY_VALUE_LARGE_ORDER_ACT = reader.IsDBNull(84) ? 0 : reader.GetDecimal("BUY_VALUE_LARGE_ORDER_ACT");        //大户买入金额(仅主动)(万元)				 
                                objASHAREMONEYFLOW.SELL_VALUE_LARGE_ORDER_ACT = reader.IsDBNull(85) ? 0 : reader.GetDecimal("SELL_VALUE_LARGE_ORDER_ACT");        //大户卖出金额(仅主动)(万元)				 
                                objASHAREMONEYFLOW.BUY_VALUE_MED_ORDER_ACT = reader.IsDBNull(86) ? 0 : reader.GetDecimal("BUY_VALUE_MED_ORDER_ACT");        //中户买入金额(仅主动)(万元)				 
                                objASHAREMONEYFLOW.SELL_VALUE_MED_ORDER_ACT = reader.IsDBNull(87) ? 0 : reader.GetDecimal("SELL_VALUE_MED_ORDER_ACT");        //中户卖出金额(仅主动)(万元)				 
                                objASHAREMONEYFLOW.BUY_VALUE_SMALL_ORDER_ACT = reader.IsDBNull(88) ? 0 : reader.GetDecimal("BUY_VALUE_SMALL_ORDER_ACT");        //散户买入金额(仅主动)(万元)				 
                                objASHAREMONEYFLOW.SELL_VALUE_SMALL_ORDER_ACT = reader.IsDBNull(89) ? 0 : reader.GetDecimal("SELL_VALUE_SMALL_ORDER_ACT");        //散户卖出金额(仅主动)(万元)				 
                                objASHAREMONEYFLOW.BUY_VOLUME_EXLARGE_ORDER_ACT = reader.IsDBNull(90) ? 0 : reader.GetDecimal("BUY_VOLUME_EXLARGE_ORDER_ACT");        //机构买入总量(仅主动)(万股)				 
                                objASHAREMONEYFLOW.SELL_VOLUME_EXLARGE_ORDER_ACT = reader.IsDBNull(91) ? 0 : reader.GetDecimal("SELL_VOLUME_EXLARGE_ORDER_ACT");        //机构卖出总量(仅主动)(万股)				 
                                objASHAREMONEYFLOW.BUY_VOLUME_LARGE_ORDER_ACT = reader.IsDBNull(92) ? 0 : reader.GetDecimal("BUY_VOLUME_LARGE_ORDER_ACT");        //大户买入总量(仅主动)(万股)				 
                                objASHAREMONEYFLOW.SELL_VOLUME_LARGE_ORDER_ACT = reader.IsDBNull(93) ? 0 : reader.GetDecimal("SELL_VOLUME_LARGE_ORDER_ACT");        //大户卖出总量(仅主动)(万股)				 
                                objASHAREMONEYFLOW.BUY_VOLUME_MED_ORDER_ACT = reader.IsDBNull(94) ? 0 : reader.GetDecimal("BUY_VOLUME_MED_ORDER_ACT");        //中户买入总量(仅主动)(万股)				 
                                objASHAREMONEYFLOW.SELL_VOLUME_MED_ORDER_ACT = reader.IsDBNull(95) ? 0 : reader.GetDecimal("SELL_VOLUME_MED_ORDER_ACT");        //中户卖出总量(仅主动)(万股)				 
                                objASHAREMONEYFLOW.BUY_VOLUME_SMALL_ORDER_ACT = reader.IsDBNull(96) ? 0 : reader.GetDecimal("BUY_VOLUME_SMALL_ORDER_ACT");        //散户买入总量(仅主动)(万股)				 
                                objASHAREMONEYFLOW.SELL_VOLUME_SMALL_ORDER_ACT = reader.IsDBNull(97) ? 0 : reader.GetDecimal("SELL_VOLUME_SMALL_ORDER_ACT");        //散户卖出总量(仅主动)(万股)	

                                m_dictASHAREMONEYFLOW.Add(objASHAREMONEYFLOW.TRADE_DT, objASHAREMONEYFLOW);
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("查询ASHAREEODDERIVATIVEINDICATOR 异常：" + ex);
                            lock (m_ErrorList)
                            {
                                if (wincode != "")
                                    m_ErrorList.Add(wincode, 1);
                            }
                            return false;
                        }
                        finally
                        {
                            conn.Close();
                        }

                    }

                }
            return true;
        }

        private void GetChildTableDict()
        {            
            this.m_pASHAREDESCRIPTIONEvent = new AutoResetEvent(false);
            this.m_pASHARECALENDAREvent = new AutoResetEvent(false);
            this.m_pAshareIndustriesCodeMapEvent = new AutoResetEvent(false);
            this.m_pAINDEXEODPRICESEvent = new AutoResetEvent(false);
            //获取changecode，和之前changecode数量不一致就手动处理
            Task.Run(() =>
            {
                MySqlConnection conn = new MySqlConnection(connetStrRead);
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM CHANGEWINDCODE WHERE (S_INFO_OLDWINDCODE LIKE '0%.SZ' OR S_INFO_OLDWINDCODE LIKE '0%.SH' OR S_INFO_OLDWINDCODE LIKE '3%.SZ' OR S_INFO_OLDWINDCODE LIKE '3%.SH' OR S_INFO_OLDWINDCODE LIKE '6%.SZ'";
                    sql += " OR S_INFO_OLDWINDCODE LIKE '6%.SH') AND(S_INFO_NEWWINDCODE LIKE '0%.SZ' OR S_INFO_NEWWINDCODE LIKE '0%.SH' OR S_INFO_NEWWINDCODE LIKE '3%.SZ' OR S_INFO_NEWWINDCODE LIKE '3%.SH'";
                    sql += " OR S_INFO_NEWWINDCODE LIKE '6%.SZ' OR S_INFO_NEWWINDCODE LIKE '6%.SH')"; ;
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        CHANGEWINDCODE objCHANGEWINDCODE = new CHANGEWINDCODE();
                        objCHANGEWINDCODE.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                        objCHANGEWINDCODE.S_INFO_OLDWINDCODE = reader.IsDBNull(2) ? "0" : reader.GetString("S_INFO_OLDWINDCODE");
                        objCHANGEWINDCODE.S_INFO_NEWWINDCODE = reader.IsDBNull(3) ? "0" : reader.GetString("S_INFO_NEWWINDCODE");
                        objCHANGEWINDCODE.CHANGE_DATE = reader.IsDBNull(4) ? "0" : reader.GetString("CHANGE_DATE");
                        if(!m_ChangeCodeDict.ContainsKey(objCHANGEWINDCODE.S_INFO_WINDCODE))
                        {
                            m_ChangeCodeDict.Add(objCHANGEWINDCODE.S_INFO_WINDCODE,objCHANGEWINDCODE);
                        }
                    }

                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询CHANGEWINDCODE异常：" + ex);
                }
                finally
                {
                    conn.Close();
                }

            });
            //这里是取上市退市时间
            Task.Run(() =>
            {
                MySqlConnection conn = new MySqlConnection(connetStrRead);
                try
                {
                    conn.Open();
                    string sql = "select * from ASHAREDESCRIPTION where S_INFO_WINDCODE not like 'A%';";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ASHAREDESCRIPTION objASHAREDESCRIPTION = new ASHAREDESCRIPTION();
                        objASHAREDESCRIPTION.S_INFO_WINDCODE = reader.IsDBNull(1) ? "" : reader.GetString("S_INFO_WINDCODE");
                        objASHAREDESCRIPTION.S_INFO_LISTDATE = reader.IsDBNull(9) ? "0" : reader.GetString("S_INFO_LISTDATE");
                        objASHAREDESCRIPTION.S_INFO_DELISTDATE = reader.IsDBNull(10) ? "0" : reader.GetString("S_INFO_DELISTDATE");
                        m_listWINCODE.Add(objASHAREDESCRIPTION.S_INFO_WINDCODE,objASHAREDESCRIPTION);
                    }

                    this.m_pASHAREDESCRIPTIONEvent.Set();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询ASHAREEODPRICES异常：" + ex);
                }
                finally
                {
                    conn.Close();
                }

            });
            //取日期，用来取上一个交易日
            Task.Run(() =>
            {
                MySqlConnection conn = new MySqlConnection(connetStrRead);
                try
                {
                    conn.Open();
                    string sql = "select * from ASHARECALENDAR where S_INFO_EXCHMARKET = 'SZSE ' and TRADE_DAYS > '20051230' and TRADE_DAYS <= '" + DateTime.Now.ToString("yyyyMMdd")+ "' order by TRADE_DAYS;";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    string pretradeday = "";
                    while (reader.Read())
                    {
                        ASHARECALENDAR objASHARECALENDAR = new ASHARECALENDAR();
                        objASHARECALENDAR.TRADE_DAYS = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        objASHARECALENDAR.PRETRADE_DAYS = pretradeday;
                        if(!m_dictASHARECALENDAR.ContainsKey(objASHARECALENDAR.TRADE_DAYS))
                            m_dictASHARECALENDAR.Add(objASHARECALENDAR.TRADE_DAYS, objASHARECALENDAR);
                        pretradeday = objASHARECALENDAR.TRADE_DAYS;
                    }

                    this.m_pASHARECALENDAREvent.Set();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询ASHAREEODPRICES异常：" + ex);
                }
                finally
                {
                    conn.Close();
                }

            });
            //行业map
            Task.Run(() =>
            {
                MySqlConnection conn = new MySqlConnection(connetStrRead);
                try
                {
                    conn.Open();
                    string sql = "select * from ASHAREINDUSTRIESCODEMAP;";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        int ID =  reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        string str = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string INDUSTRIESCODE = str.Substring( 0, 4);
                        m_dictAshareIndustriesCodeMap.Add(INDUSTRIESCODE, ID);
                    }

                    this.m_pAshareIndustriesCodeMapEvent.Set();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询ASHAREEODPRICES异常：" + ex);
                
                }
                finally
                {
                    conn.Close();
                }

            });
            //取AINDEXEODPRICES
            Task.Run(() =>
            {
                MySqlConnection conn = new MySqlConnection(connetStrRead);
                try
                {
                    DealAINDEXPrice(conn);

                    this.m_pAINDEXEODPRICESEvent.Set();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("查询AINDEXEODPRICES异常：" + ex);
                }
                finally
                {
                    conn.Close();
                }

            });
        }
    }
}
