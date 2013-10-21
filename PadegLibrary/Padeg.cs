using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PadegLibrary
{
    public enum Padeg
    {
        Именительный=1,
        Родительный=2,
        Дательный=3,
        Винительный=4,
        Творительный=5,
        Предложный=6
    }

    public class PadegConverter
    {
        [DllImport(@"Padeg.dll", CharSet = CharSet.Ansi)]
        static extern int GetFIOPadegFSAS(
            [In]        string pFIO,
            [In]        int nPadeg,
            [In, Out]   StringBuilder pResult,
            [In, Out]   ref int nLen
            );

        public static string Convert(string word, Padeg padeg)
        {
            var trimed = word.Trim();
            var buffer = new StringBuilder(trimed.Length + 56);
            int len = buffer.Capacity;

            switch (GetFIOPadegFSAS(trimed, (int)padeg, buffer, ref len))
            {
                case 0:
                    return buffer.ToString();
                case (-1):
                   throw new System.Exception("Недопустимое значение падежа.");
                case (-3):
                    throw new System.Exception("Результат не поместился в буфере.");
                default:
                    throw new NotSupportedException();
            }
        }
    }
}