
namespace MerageTable.asharetable
{
    class ASHARESUPERTABLE
    {
        public string S_INFO_WINDCODE { get; set; }
        public string TRADE_DT { get; set; }

        public ASHAREEODPRICES objASHAREEODPRICES;
        public ASHAREMONEYFLOW objASHAREMONEYFLOW;
        public ASHAREEODDERIVATIVEINDICATOR objASHAREEODDERIVATIVEINDICATOR;
        public ASHAREL2INDICATORS objASHAREL2INDICATORS;

        public AINDEXHS300FREEWEIGHT objAINDEXSH50;
        public AINDEXHS300FREEWEIGHT objAINDEXHS300;
        public AINDEXHS300FREEWEIGHT objAINDEXCS500;
        public AINDEXHS300FREEWEIGHT objAINDEXCS1000;
        public AINDEXHS300FREEWEIGHT objAINDEXMKT;

        public AINDEXEODPRICES objAINDEXEODPRICESSH50;
        public AINDEXEODPRICES objAINDEXEODPRICESHS300;
        public AINDEXEODPRICES objAINDEXEODPRICESCS500;
        public AINDEXEODPRICES objAINDEXEODPRICESCS1000;
        public AINDEXEODPRICES objAINDEXEODPRICESMKT;

        public int SUSPEND_FLAG { get; set; }
        public decimal S_DQ_SUSPENDTYPE { get; set; }
        public int ST_FLAG { get; set; }
        public int CITICS_IND_CODE { get;  set; }
        public bool isDeal { get; set; }

        public ASHARESUPERTABLE()
        {
            objASHAREMONEYFLOW = new ASHAREMONEYFLOW();
            objASHAREL2INDICATORS = new ASHAREL2INDICATORS();
            objASHAREEODPRICES = new ASHAREEODPRICES();
            objASHAREEODDERIVATIVEINDICATOR = new ASHAREEODDERIVATIVEINDICATOR();

            objAINDEXSH50 = new AINDEXHS300FREEWEIGHT();
            objAINDEXHS300 = new AINDEXHS300FREEWEIGHT();
            objAINDEXCS500 = new AINDEXHS300FREEWEIGHT();
            objAINDEXCS1000 = new AINDEXHS300FREEWEIGHT();
            objAINDEXMKT = new AINDEXHS300FREEWEIGHT();

            objAINDEXEODPRICESSH50 = new AINDEXEODPRICES();
            objAINDEXEODPRICESHS300 = new AINDEXEODPRICES();
            objAINDEXEODPRICESCS500 = new AINDEXEODPRICES();
            objAINDEXEODPRICESCS1000 = new AINDEXEODPRICES();
            objAINDEXEODPRICESMKT = new AINDEXEODPRICES();
        }
    }
}
