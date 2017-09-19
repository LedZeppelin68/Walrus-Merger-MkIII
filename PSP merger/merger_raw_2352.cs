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
    class merger_raw_2352
    {
        //public struct map
        //{
        //    public long map_offset;
        //    public long map_length;
        //}

        internal static void Merge(string file, ref Dictionary<string, BinaryWriter> Writers, ref Dictionary<string, int> WritersCursors, ref Dictionary<string, int> duplicates, ref Dictionary<string, MD5> Checksums_MD5, ref XmlElement file_XML, ref Dictionary<string, Program.map> Relink_Map)
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
                        byte[] temp = FileReader.ReadBytes(2352);
                        //file_MD5.TransformBlock(temp, 0, 2352, null, 0);
                        string BlockMD5 = string.Empty;

                        if (CalculatorRoutines.SyncCompare(ref temp))
                        {
                            file_MD5.TransformBlock(temp, 0, 2352, null, 0);
                            switch (temp[15])
                            {
                                case 1:
                                    BlockMD5 = CalculatorRoutines.GetBlockMD5(ref temp, 16, 2048);
                                    switch (BlockMD5)
                                    {
                                        case "C9-9A-74-C5-55-37-1A-43-3D-12-1F-55-1D-6C-63-98":
                                            MapWriter.Write((byte)1);
                                            MapWriter.Write(0xffffffffu);
                                            break;
                                        default:
                                            if (duplicates.ContainsKey(BlockMD5))
                                            {
                                                MapWriter.Write((byte)1);
                                                MapWriter.Write(duplicates[BlockMD5]);
                                            }
                                            else
                                            {
                                                MapWriter.Write((byte)1);
                                                MapWriter.Write(WritersCursors["2048"]);

                                                duplicates.Add(BlockMD5, WritersCursors["2048"]);
                                                Writers["2048"].Write(temp, 16, 2048);
                                                WritersCursors["2048"]++;

                                                Checksums_MD5["2048"].TransformBlock(temp, 16, 2048, null, 0);
                                            }
                                            break;
                                    }
                                    break;
                                case 2:
                                    //int parameter = 0;
                                    switch (temp[18] & 0x20)
                                    {
                                        default:
                                            BlockMD5 = CalculatorRoutines.GetBlockMD5(ref temp, 24, 2048);
                                            int ecc_error = CalculatorRoutines.CheckECCerror(ref temp);
                                            switch (BlockMD5)
                                            {
                                                case "C9-9A-74-C5-55-37-1A-43-3D-12-1F-55-1D-6C-63-98":
                                                    MapWriter.Write((byte)(2 | ecc_error));//
                                                    MapWriter.Write(temp, 16, 8);
                                                    MapWriter.Write(0xffffffffu);
                                                    break;
                                                default:
                                                    if (duplicates.ContainsKey(BlockMD5))
                                                    {
                                                        MapWriter.Write((byte)(2 | ecc_error));//
                                                        MapWriter.Write(temp, 16, 8);
                                                        MapWriter.Write(duplicates[BlockMD5]);
                                                    }
                                                    else
                                                    {
                                                        MapWriter.Write((byte)(2 | ecc_error));//
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
                                            int null_edc = CalculatorRoutines.CheckNulledc(ref temp);
                                            switch (Form2BlockMD5)
                                            {
                                                case "B4-07-91-E2-24-BD-42-5C-59-F0-05-55-1D-A1-16-45":
                                                    MapWriter.Write((byte)(2 | null_edc));
                                                    MapWriter.Write(temp, 16, 8);
                                                    MapWriter.Write(0xffffffffu);
                                                    break;
                                                default:
                                                    if (duplicates.ContainsKey(Form2BlockMD5))
                                                    {
                                                        MapWriter.Write((byte)(2 | null_edc));
                                                        MapWriter.Write(temp, 16, 8);
                                                        MapWriter.Write(duplicates[Form2BlockMD5]);
                                                    }
                                                    else
                                                    {
                                                        MapWriter.Write((byte)(2 | null_edc));
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
                        else
                        {
                            long audio_start = FileReader.BaseStream.Position - 2352;

                            FileReader.BaseStream.Seek(audio_start, SeekOrigin.Begin);

                            int audio_sector_count = 0;

                            while (FileReader.BaseStream.Position != FileReader.BaseStream.Length)
                            {
                                byte[] audio = FileReader.ReadBytes(2352);

                                if (!CalculatorRoutines.SyncCompare(ref audio)) { audio_sector_count++; }
                                else { break; }
                            }

                            long audio_end = audio_start + (audio_sector_count * 2352);

                            FileReader.BaseStream.Seek(audio_start, SeekOrigin.Begin);

                            while (FileReader.BaseStream.Position != audio_end)
                            {
                                int null_samples = 0;
                                UInt32 sample = 0;
                                while ((sample == 0) & (FileReader.BaseStream.Position != audio_end))
                                {
                                    sample = FileReader.ReadUInt32();
                                    if (sample == 0) null_samples++;
                                }

                                if (null_samples != 0)
                                {
                                    MapWriter.Write((byte)(0x80 | 0));
                                    MapWriter.Write(null_samples);
                                }

                                for (int x = 0; x < null_samples; x++)
                                {
                                    file_MD5.TransformBlock(new byte[4], 0, 4, null, 0);
                                }

                                if (FileReader.BaseStream.Position != audio_end)
                                {
                                    FileReader.BaseStream.Seek(-4, SeekOrigin.Current);
                                    byte[] audio_temp = new byte[2352];

                                    switch ((audio_end - FileReader.BaseStream.Position) >= 2352)
                                    {
                                        case true:
                                            audio_temp = FileReader.ReadBytes(2352);
                                            BlockMD5 = CalculatorRoutines.GetBlockMD5(ref audio_temp, 0, 2352);
                                            file_MD5.TransformBlock(audio_temp, 0, 2352, null, 0);
                                            break;
                                        case false:
                                            int audio_chunk_size = (int)(audio_end - FileReader.BaseStream.Position);
                                            byte[] audio_chunk_last = FileReader.ReadBytes(audio_chunk_size);
                                            BlockMD5 = CalculatorRoutines.GetBlockMD5(ref audio_chunk_last, 0, audio_chunk_size);
                                            file_MD5.TransformBlock(audio_chunk_last, 0, audio_chunk_size, null, 0);
                                            Array.Copy(audio_chunk_last, audio_temp, audio_chunk_size);
                                            break;
                                    }

                                    switch (BlockMD5)
                                    {
                                        default:
                                            if (duplicates.ContainsKey(BlockMD5))
                                            {
                                                MapWriter.Write((byte)0);
                                                MapWriter.Write(duplicates[BlockMD5]);
                                            }
                                            else
                                            {
                                                MapWriter.Write((byte)0);
                                                MapWriter.Write(WritersCursors["pcm"]);

                                                duplicates.Add(BlockMD5, WritersCursors["pcm"]);
                                                Writers["pcm"].Write(audio_temp, 0, 2352);
                                                WritersCursors["pcm"]++;

                                                Checksums_MD5["pcm"].TransformBlock(audio_temp, 0, 2352, null, 0);
                                            }
                                            break;
                                        case "9E-29-7E-FC-7A-52-24-80-EF-89-A4-A7-F3-9C-E5-60":
                                            FileReader.BaseStream.Seek(-2352, SeekOrigin.Current);
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    file_MD5.TransformFinalBlock(new byte[0], 0, 0);
                    string file_MD5_string = BitConverter.ToString(file_MD5.Hash).Replace("-", "").ToLower();
                    file_MD5.Dispose();

                    long map_offset = Writers["map"].BaseStream.Position;
                    long map_size = MapWriter.BaseStream.Length;

                    switch (Relink_Map.ContainsKey(file_MD5_string))
                    {
                        case true:
                            file_XML.SetAttribute("md5", file_MD5_string);
                            file_XML.SetAttribute("map_offset", Relink_Map[file_MD5_string].map_offset.ToString());
                            file_XML.SetAttribute("map_size", Relink_Map[file_MD5_string].map_length.ToString());
                            break;
                        case false:
                            file_XML.SetAttribute("md5", file_MD5_string);
                            file_XML.SetAttribute("map_offset", map_offset.ToString());
                            file_XML.SetAttribute("map_size", map_size.ToString());

                            Relink_Map.Add(file_MD5_string, new Program.map { map_offset = map_offset, map_length = map_size });
                            break;
                    }

                    //file_XML.SetAttribute("md5", file_MD5_string);
                    //file_XML.SetAttribute("map_offset", Writers["map"].BaseStream.Position.ToString());
                    //file_XML.SetAttribute("map_size", MapWriter.BaseStream.Length.ToString());

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
