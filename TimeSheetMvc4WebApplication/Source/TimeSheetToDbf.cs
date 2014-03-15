using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Razor;
using DotNetDBF;

namespace TimeSheetMvc4WebApplication.Source
{
    public class TimeSheetToDbf
    {
        public TimeSheetToDbf()
        {
            
        }

        public byte[] GenerateDbf()
        {
            var field = new DBFField( "F1", NativeDbType.Numeric, 15, 0 );
            var field2 = new DBFField("F2", NativeDbType.Char, 15, 0);
            
            var writer = new DBFWriter { Fields = new[] { field,field2 } };
            //var field3 = new DBFField("F3", NativeDbType.Char, 10);
            //var writer = new DBFWriter { Fields = new[] { field,field3 } };
            writer.AddRecord(3,"s");

            var fos = new MemoryStream();
            {
                writer.Write(fos);
            }
            return fos.ToArray();
        }
    }
}