
namespace MerageTable.asharetable
{
    class ASHARETRADINGSUSPENSION
    {
        public string S_INFO_WINDCODE{ get; set; }
        public string S_DQ_SUSPENDDATE{ get; set; }  // 停牌日期
        public decimal S_DQ_SUSPENDTYPE{ get; set; } // 停牌类型代码
        public string S_DQ_RESUMPDATE{ get; set; }   // 复牌日期
        //public string S_DQ_CHANGEREASON{ get; set; } // 停牌原因
        public string S_DQ_TIME{ get; set; }         // 停复牌时间
        //public decimal S_DQ_CHANGEREASONTYPE{ get; set; } // 停牌原因代码

    }
}
