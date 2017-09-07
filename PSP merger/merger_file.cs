using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Security.Cryptography;

namespace Walrus_Merger
{
    class merger_file
    {
        internal static void Merge(string file, ref Dictionary<string, System.IO.BinaryWriter> Writers, ref Dictionary<string, int> WritersCursors, ref Dictionary<string, int> duplicates, ref Dictionary<string, System.Security.Cryptography.MD5> Checksums_MD5, ref System.Xml.XmlElement file_XML)
        {
            using(BinaryReader FileReader = new BinaryReader(new FileStream(file,FileMode.Open)))
            {
                MD5 file_MD5 = MD5.Create();

                file_XML.SetAttribute("name", Path.GetFileName(file));
                file_XML.SetAttribute("size", FileReader.BaseStream.Length.ToString());
                file_XML.SetAttribute("date", "debug");

                using (BinaryWriter MapWriter = new BinaryWriter(new MemoryStream()))
                {
                    long even_block_count = FileReader.BaseStream.Length / 2048;
                    long last_block_count = FileReader.BaseStream.Length % 2048;

                    string BlockMD5 = string.Empty;

                    for (int i = 0; i < even_block_count; i++)
                    {
                        byte[] even_block = FileReader.ReadBytes(2048);
                        file_MD5.TransformBlock(even_block, 0, 2048, null, 0);

                        BlockMD5 = CalculatorRoutines.GetBlockMD5(ref even_block, 0, 2048);

                        switch(BlockMD5)
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
                                    Writers["2048"].Write(even_block, 0, 2048);
                                    WritersCursors["2048"]++;

                                    Checksums_MD5["2048"].TransformBlock(even_block, 0, 2048, null, 0);
                                }
                                break;
                        }
                    }

                    byte[] last_block = FileReader.ReadBytes((Int32)last_block_count);
                    file_MD5.TransformBlock(last_block, 0, last_block.Length, null, 0);

                    BlockMD5 = CalculatorRoutines.GetBlockMD5(ref last_block, 0, last_block.Length);

                    if (duplicates.ContainsKey(BlockMD5))
                    {
                        MapWriter.Write(duplicates[BlockMD5]);
                    }
                    else
                    {
                        MapWriter.Write(WritersCursors["2048"]);

                        duplicates.Add(BlockMD5, WritersCursors["2048"]);
                        byte[] temp = new byte[2048];
                        last_block.CopyTo(temp, 0);
                        Writers["2048"].Write(temp, 0, 2048);
                        WritersCursors["2048"]++;

                        Checksums_MD5["2048"].TransformBlock(temp, 0, 2048, null, 0);
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
