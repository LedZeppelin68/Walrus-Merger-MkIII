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
    class merger_iso_2048
    {
        internal static void Merge(string file, ref Dictionary<string, BinaryWriter> Writers, ref Dictionary<string, int> WritersCursors, ref Dictionary<string, int> duplicates, ref Dictionary<string, MD5> Checksums_MD5, ref XmlElement file_XML)
        {
            using (BinaryReader FileReader = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                MD5 file_MD5 = MD5.Create();

                file_XML.SetAttribute("name", Path.GetFileName(file));
                file_XML.SetAttribute("size", FileReader.BaseStream.Length.ToString());
                file_XML.SetAttribute("date", "debug");

                using (BinaryWriter MapWriter = new BinaryWriter(new MemoryStream()))
                {
                    while (FileReader.BaseStream.Position != FileReader.BaseStream.Length)
                    {
                        byte[] temp = FileReader.ReadBytes(2048);
                        file_MD5.TransformBlock(temp, 0, 2048, null, 0);
                        string BlockMD5 = CalculatorRoutines.GetBlockMD5(ref temp, 0, 2048);

                        switch (BlockMD5)
                        {
                            case "C9-9A-74-C5-55-37-1A-43-3D-12-1F-55-1D-6C-63-98":
                                MapWriter.Write(0xffffffffu);
                                break;
                            default:
                                if (duplicates.ContainsKey(BlockMD5))
                                {
                                    MapWriter.Write(duplicates[BlockMD5]);
                                }
                                else
                                {
                                    MapWriter.Write(WritersCursors["2048"]);

                                    duplicates.Add(BlockMD5, WritersCursors["2048"]);
                                    Writers["2048"].Write(temp);
                                    WritersCursors["2048"]++;

                                    Checksums_MD5["2048"].TransformBlock(temp, 0, 2048, null, 0);
                                }
                                break;
                        }
                    }

                    file_MD5.TransformFinalBlock(new byte[0], 0, 0);
                    string file_MD5_string = BitConverter.ToString(file_MD5.Hash).Replace("-", "").ToLower();
                    file_MD5.Dispose();

                    file_XML.SetAttribute("md5", file_MD5_string);
                    file_XML.SetAttribute("map_offset", Writers["map"].BaseStream.Position.ToString());
                    file_XML.SetAttribute("map_size", MapWriter.BaseStream.Length.ToString());

                    byte[] map = new byte[MapWriter.BaseStream.Length];
                    MapWriter.BaseStream.Position = 0;
                    MapWriter.BaseStream.CopyTo(new MemoryStream(map));

                    Checksums_MD5["map"].TransformBlock(map, 0, map.Length, null, 0);

                    Writers["map"].Write(map);
                }
            }
        }
    }
}
