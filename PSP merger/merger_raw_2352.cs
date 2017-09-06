using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace Walrus_Merger
{
    class merger_raw_2352
    {
        internal static void Merge(string file, ref Dictionary<string, BinaryWriter> Writers, ref Dictionary<string, int> WritersCursors, ref Dictionary<string, int> duplicates, ref Dictionary<string, MD5> Checksums_MD5)
        {
            using (BinaryReader FileReader = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                using (BinaryWriter MapWriter = new BinaryWriter(new MemoryStream()))
                {
                    while (FileReader.BaseStream.Position != FileReader.BaseStream.Length)
                    {
                        byte[] temp = FileReader.ReadBytes(2352);

                        if (CalculatorRoutines.SyncCompare(ref temp))
                        {
                            switch (temp[15])
                            {
                                case 1:
                                    //reserved for mode1
                                    break;
                                case 2:
                                    switch (temp[18] & 0x20)
                                    {
                                        default:
                                            string BlockMD5 = CalculatorRoutines.GetBlockMD5(ref temp, 24, 2048);
                                            switch (BlockMD5)
                                            {
                                                case "C9-9A-74-C5-55-37-1A-43-3D-12-1F-55-1D-6C-63-98":
                                                    MapWriter.Write((byte)2);
                                                    MapWriter.Write(temp, 16, 8);
                                                    MapWriter.Write(0xffffffffu);
                                                    break;
                                                default:
                                                    if (duplicates.ContainsKey(BlockMD5))
                                                    {
                                                        MapWriter.Write((byte)2);
                                                        MapWriter.Write(temp, 16, 8);
                                                        MapWriter.Write(duplicates[BlockMD5]);
                                                    }
                                                    else
                                                    {
                                                        MapWriter.Write((byte)2);
                                                        MapWriter.Write(temp, 16, 8);
                                                        MapWriter.Write(WritersCursors["2048"]);

                                                        duplicates.Add(BlockMD5, WritersCursors["2048"]);
                                                        Writers["2048"].Write(temp, 24, 2048);
                                                        WritersCursors["2048"]++;

                                                        Checksums_MD5["2048"].TransformBlock(temp, 24, 2048, null, 0);
                                                    }
                                                    break;
                                            }
                                            break;
                                        case 0x20:
                                            string Form2BlockMD5 = CalculatorRoutines.GetBlockMD5(ref temp, 24, 2324);
                                            switch (Form2BlockMD5)
                                            {
                                                case "B4-07-91-E2-24-BD-42-5C-59-F0-05-55-1D-A1-16-45":
                                                    MapWriter.Write((byte)2);
                                                    MapWriter.Write(temp, 16, 8);
                                                    MapWriter.Write(0xffffffffu);
                                                    break;
                                                default:
                                                    if (duplicates.ContainsKey(Form2BlockMD5))
                                                    {
                                                        MapWriter.Write((byte)2);
                                                        MapWriter.Write(temp, 16, 8);
                                                        MapWriter.Write(duplicates[Form2BlockMD5]);
                                                    }
                                                    else
                                                    {
                                                        MapWriter.Write((byte)2);
                                                        MapWriter.Write(temp, 16, 8);
                                                        MapWriter.Write(WritersCursors["2324"]);

                                                        duplicates.Add(Form2BlockMD5, WritersCursors["2324"]);
                                                        Writers["2324"].Write(temp, 24, 2324);
                                                        WritersCursors["2324"]++;

                                                        Checksums_MD5["2324"].TransformBlock(temp, 24, 2324, null, 0);
                                                    }
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                    }

                    byte[] map = new byte[MapWriter.BaseStream.Length];
                    MapWriter.BaseStream.Position = 0;
                    MapWriter.BaseStream.CopyTo(new MemoryStream(map));

                    Checksums_MD5["map"].TransformBlock(map, 0, map.Length, null, 0);

                    Writers["map"].Write(map);
                }
                //copy map to main stream
            }
        }
    }
}
