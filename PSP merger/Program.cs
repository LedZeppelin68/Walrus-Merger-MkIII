using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace Walrus_Merger
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[] { @"J:\PSX\ace3\E" };//for debug

            foreach (string folder in args)
            {
                string[] files = Directory.GetFiles(folder);

                Dictionary<string, Int32> duplicates = new Dictionary<string, Int32>();

                Dictionary<string, MD5> Checksums_MD5 = new Dictionary<string, MD5>();
                InitChecksums_MD5(ref Checksums_MD5);

                Dictionary<string, BinaryWriter> Writers = new Dictionary<string, BinaryWriter>();
                InitWriters(ref Writers, folder);

                Dictionary<string, int> WritersCursors = new Dictionary<string, int>();
                InitWritersCursors(ref WritersCursors);

                XmlDocument ControlFileXML = new XmlDocument();
                ControlFileXML.LoadXml("<root></root>");

                foreach (string file in files)
                {
                    string type = TypeCheck.FileType(file);

                    switch (type)
                    {
                        case "iso":
                            merger_iso_2048.Merge(file, ref Writers, ref WritersCursors, ref duplicates, ref Checksums_MD5);
                            break;
                        case "raw":
                            merger_raw_2352.Merge(file, ref Writers, ref WritersCursors, ref duplicates, ref Checksums_MD5);
                            break;
                    }
                }

                FinalizeChecksums_MD5(ref Checksums_MD5);
                CloseWriters(ref Writers, ref Checksums_MD5, folder, ref ControlFileXML);

                ControlFileXML.Save(Path.Combine(folder, "test.xml"));
            }
        }

        private static void FinalizeChecksums_MD5(ref Dictionary<string, MD5> Checksums_MD5)
        {
            Checksums_MD5["2048"].TransformFinalBlock(new byte[0], 0, 0);
            Checksums_MD5["2324"].TransformFinalBlock(new byte[0], 0, 0);
            Checksums_MD5["2352"].TransformFinalBlock(new byte[0], 0, 0);
            Checksums_MD5["map"].TransformFinalBlock(new byte[0], 0, 0);
        }

        private static void InitChecksums_MD5(ref Dictionary<string, MD5> Checksums_MD5)
        {
            Checksums_MD5.Add("2048", MD5.Create());
            Checksums_MD5.Add("2324", MD5.Create());
            Checksums_MD5.Add("2352", MD5.Create());
            Checksums_MD5.Add("map", MD5.Create());
        }

        private static void CloseWriters(ref Dictionary<string, BinaryWriter> Writers, ref Dictionary<string, MD5> Checksums_MD5, string folder, ref XmlDocument ControlFileXML)
        {
            XmlElement XML_partitions = ControlFileXML.CreateElement("partition");

            foreach (KeyValuePair<string, BinaryWriter> Writer in Writers)
            {
                FileStream fs = (FileStream)Writer.Value.BaseStream;
                string Filename = fs.Name;
                Writer.Value.Close();
                if (new FileInfo(Filename).Length == 0)
                {
                    File.Delete(Filename);
                }
                else
                {
                    string NewFilename = BitConverter.ToString(Checksums_MD5[Writer.Key].Hash).Replace("-","").ToLower();
                    File.Move(Filename, Path.Combine(folder, NewFilename));

                    XmlElement XML_partition = ControlFileXML.CreateElement("record");
                    XML_partition.SetAttribute("type", Writer.Key);
                    XML_partition.SetAttribute("md5", NewFilename);
                    XML_partitions.AppendChild(XML_partition);
                }
            }

            ControlFileXML.AppendChild(XML_partitions);
        }

        private static void InitWritersCursors(ref Dictionary<string, int> WritersCursors)
        {
            WritersCursors.Add("2048", 0);
            WritersCursors.Add("2324", 0);
            WritersCursors.Add("2352", 0);
        }

        private static void InitWriters(ref Dictionary<string, BinaryWriter> Writers, string folder)
        {
            Writers.Add("2048", new BinaryWriter(new FileStream(Path.Combine(folder, (new Random(2048).Next()).ToString()), FileMode.Create)));
            Writers.Add("2324", new BinaryWriter(new FileStream(Path.Combine(folder, (new Random(2324).Next()).ToString()), FileMode.Create)));
            Writers.Add("2352", new BinaryWriter(new FileStream(Path.Combine(folder, (new Random(2352).Next()).ToString()), FileMode.Create)));
            Writers.Add("map", new BinaryWriter(new FileStream(Path.Combine(folder, (new Random(666).Next()).ToString()), FileMode.Create)));
        }

        private static void SaveStatistic(Statistic stat, string file)
        {
            List<string> txt = new List<string>();

            txt.Add(string.Format("Null: {0:#,#};", stat.NullData << 11));
            txt.Add(string.Format("Unique: {0:#,#};", stat.UniqueData << 11));
            txt.Add(string.Format("Dupe: {0:#,#};", stat.DupeData << 11));

            File.WriteAllLines(file + ".txt", txt);
        }

        private static void SaveMap(Stream stream, string file)
        {
            stream.Position = 0;
            stream.CopyTo(new FileStream(file + ".map", FileMode.Create));
        }

    }

    struct Statistic
    {
        public Int64 NullData;
        public Int64 UniqueData;
        public Int64 DupeData;
    }
}
