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
                }

                if (FileReader.BaseStream.Length >= 0x9318)
                {
                    FileReader.BaseStream.Seek(0x9318, SeekOrigin.Begin);
                    if (FileReader.ReadBytes(magic_word_iso.Length).SequenceEqual(magic_word_iso))
                    {
                        return "raw";
                    }
                }
            }

            return type;
        }
    }
}
