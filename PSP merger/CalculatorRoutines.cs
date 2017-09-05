using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Walrus_Merger
{
    class CalculatorRoutines
    {
        static byte[] sync = { 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00 };

        internal static string GetBlockMD5(ref byte[] temp, int offset, int count)
        {
            string md5 = string.Empty;
            using (MD5 hash = MD5.Create())
            {
                md5 = BitConverter.ToString(hash.ComputeHash(temp, offset, count));
            }
            return md5;
        }

        internal static bool SyncCompare(ref byte[] temp)
        {
            bool i = false;

            for (int j = 0; j < sync.Length; j++)
            {
                if (sync[j] != temp[j])
                {
                    return false;
                }
                else
                {
                    i = true;
                }
            }
            return i;
        }
    }
}
