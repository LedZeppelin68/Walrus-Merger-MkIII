using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Walrus_Merger
{
    class TypeCheck
    {
        static byte[] magic_word_iso = { 0x01, 0x43, 0x44, 0x30, 0x30, 0x31 };
        static byte[] magic_word_3do = { 0x01, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x01 };

        internal static string FileType(string file)
        {
            string type = "file";

            using (BinaryReader FileReader = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                if (FileReader.BaseStream.Length >= 0x8000)
                {
                    FileReader.BaseStream.Seek(0x8000, SeekOrigin.Begin);
                    if (FileReader.ReadBytes(magic_word_iso.Length).SequenceEqual(magic_word_iso))
                    {
                        return "iso";
                    }
                    //3do check
                    FileReader.BaseStream.Seek(0x0, SeekOrigin.Begin);
                    if (FileReader.ReadBytes(magic_word_3do.Length).SequenceEqual(magic_word_3do))
                    {
                        return "iso";
                    }
                }

                if (FileReader.BaseStream.Length >= 0x9318)
                {
                    FileReader.BaseStream.Seek(0x9318, SeekOrigin.Begin);
                    if (FileReader.ReadBytes(magic_word_iso.Length).SequenceEqual(magic_word_iso))
                    {
                        return "raw";
                    }
                    //3do check
                    FileReader.BaseStream.Seek(0x10, SeekOrigin.Begin);
                    if (FileReader.ReadBytes(magic_word_3do.Length).SequenceEqual(magic_word_3do))
                    {
                        return "raw";
                    }
                }

                if((FileReader.BaseStream.Length >= 352800) & ((FileReader.BaseStream.Length % 2352) == 0))
                {
                    return "raw";
                }
            }

            return type;
        }
    }
}
