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
        public static byte[] riff = {
                                 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x41,
                                 0x56, 0x45, 0x66, 0x6d, 0x74, 0x20, 0x10, 0x00, 0x00, 0x00, 
                                 0x01, 0x00, 0x02, 0x00, 0x44, 0xac, 0x00, 0x00, 0x10, 0xb1,
                                 0x02, 0x00, 0x04, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61, 
                                 0x00, 0x00, 0x00, 0x00
                             };

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

        internal static int CheckECCerror(ref byte[] temp)
        {
            int error = 0;

            for (int i = 0; i < 8; i++)
            {
                if (temp[i + 16] != 0) return error;
            }

            for (int i = 0; i < 280; i++)
            {
                if (temp[i + 2072] != 0)
                {
                    return 0x80;
                }
            }

            return error;
        }

        internal static int CheckNulledc(ref byte[] temp)
        {
            for (int i = 0; i < 4; i++)
            {
                if (temp[i + 2348] != 0) return 0;
            }

            return 0x40;
        }
    }
}
