
namespace MerageTable.asharetable
{
    class ASHAREINDUSTRIESCODE
    {
        public string OBJECT_ID{ get; set; }
        public string INDUSTRIESCODE{ get; set; } // 行业代码 
        public string INDUSTRIESNAME{ get; set; } // 行业名称 
        public decimal LEVELNUM{ get; set; }      // 级数				
        public decimal USED{ get; set; }      // 是否有效				
        public string INDUSTRIESALIAS{ get; set; }  //	板块别名
        public decimal SEQUENCE{ get; set; } // 展示序号				
        public string MEMO{ get; set; }    //备注 
        public string CHINESEDEFINITION{ get; set; }   	// 板块中文定义 
        public string WIND_NAME_ENG{ get; set; }   // 板块英文名称 					

    }
}
