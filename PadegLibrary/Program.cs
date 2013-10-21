using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PadegLibrary
{
    class Program
    {
        [DllImport(@"Padeg.dll", CharSet = CharSet.Ansi)]
        static extern int GetFIOPadegFSAS(
            [In]        string pFIO,
            [In]        int nPadeg,
            [In, Out]   StringBuilder pResult,
            [In, Out]   ref int nLen
            );

        static void Main(string[] args)
        {
            const string word = "Иванов Иван Иванович";
            var trimed = word.Trim();
            var buffer = new StringBuilder(trimed.Length + 56);
            int len = buffer.Capacity;

            switch (GetFIOPadegFSAS(trimed, 4, buffer, ref len))
            {
                case 0:
                    Console.Write(buffer);
                    return; //buffer.ToString();
                case (-1):
                    throw new System.Exception("Недопустимое значение падежа.");
                case (-3):
                    throw new System.Exception("Результат не поместился в буфере.");
                default:
                    throw new NotSupportedException();
            }
            //Console.Write(buffer);
        }
    }
}
