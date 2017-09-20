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
        internal static List<byte> msf_table = new List<byte>() {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29,
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
            0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
            0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
            0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
            0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79,
            0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
            0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99,
            0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9,
            0xb0, 0xb1, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9,
            0xc0, 0xc1, 0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9,
            0xd0, 0xd1, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9,
            0xe0, 0xe1, 0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9,
            0xf0, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9
        };

        //List<int>.

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

        internal static byte CheckMSF(ref byte[] temp, ref int msf_counter)
        {
            byte[] msf = new byte[3];
            int minutes = msf_counter / 4500;
            int seconds = msf_counter % 4500 / 75;
            int frames = msf_counter % 75;

            msf[0] = msf_table[minutes];
            msf[1] = msf_table[seconds];
            msf[2] = msf_table[frames];

            //msf_table.i
            //byte msf_correction = 0;
            for (int i = 0; i < 3; i++)
            {
                if (temp[12 + i] != msf[i])
                {
                    minutes = msf_table.IndexOf(temp[12]) * 4500;
                    seconds = msf_table.IndexOf(temp[13]) * 75;
                    frames = msf_table.IndexOf(temp[14]);
                    msf_counter = minutes + seconds + frames;
                    return 0x20;
                }
            }

            return 0;
        }
    }
}
