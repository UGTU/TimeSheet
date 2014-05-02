using System;
using System.Data.Linq;
using System.IO;

namespace TimeSheetMvc4WebApplication.Source
{
    public class DataContextLoger: IDisposable
    {
        private readonly FileStream _fs;
        private readonly StreamWriter _sr;
        public DataContextLoger(string filename, FileMode fileMode,DataContext context)
        {
            #if DEBUG
             _fs = new FileStream("G:\\"+filename, fileMode);
             _sr = new StreamWriter(_fs);
             _sr.WriteLine(string.Format("##############################     {0}     ##############################",DateTime.Now));
             context.Log = _sr;
            #endif
        }

        public void Dispose()
        {
            #if DEBUG
            _sr.Flush();
            _sr.Close();
            _sr.Dispose();
            _fs.Close();
            #endif
        }

     
    }
}