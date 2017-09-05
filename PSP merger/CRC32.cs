using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Walrus_Merger
{
    //class CRC32_init
    //{
    //    public UInt32[] crc_table = new UInt32[256];
    //    public uint Checksum;
    //}

    public class Crc32
    {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;

        private UInt32 hash;
        private UInt32 seed;
        private UInt32[] table;
        private static UInt32[] defaultTable;

        public Crc32()
        {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            //Initialize();
        }

        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = seed;
            //Initialize();
        }

        private uint[] InitializeTable(uint polynomial)
        {
            UInt32[] createTable = new UInt32[256];
            UInt32 crc, i, j;
            for (i = 0; i < 256; i++)
            {
                crc = i;
                for (j = 0; j < 8; j++)
                {
                    crc = (crc & 1) == 1 ? ((crc >> 1) ^ 0xedb88320) : (crc >> 1);
                }

                createTable[i] = crc;
            }
            return createTable;
        }
       
        internal void CalculateBlock(byte[] temp)
        {
            foreach (byte b in temp)
            {
                seed = table[(seed ^ b) & 0xff] ^ (seed >> 8);
            }
        }

        internal void CalculateFinalBlock()
        {
            hash = seed ^ 0xffffffff;
        }
    }
}
