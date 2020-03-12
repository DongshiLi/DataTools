using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MerageTable;

namespace SuperTable
{
    class Program
    {
        static void Main(string[] args)
        {

            SuperTableService superTable = new SuperTableService();
            if (superTable.Init())
            {

               superTable.Start();
               
            }
        }
    }
}
