using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GCNLibrary;
using KclLibrary;
using EveryFileExplorer;

namespace SM3DAllStarsGalaxyConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("Usage: SM3DAllStarsGalaxyConverter.exe filename.arc");
                return;
            }

            if (!Directory.Exists("Output"))
                Directory.CreateDirectory("Output");

            foreach (var arg in args)
            {
                string ext = Path.GetExtension(arg);
                switch (ext)
                {
                    case ".arc": ConvertARC(arg); break;
                    case ".kcl": ConvertKCL(WriteToFile(arg)); break;
                    case ".bdl": ConvertBMD(WriteToFile(arg)); break;
                    case ".ba":
                    case ".bcam":
                    case ".bcsv":
                        ConvertBCSV(WriteToFile(arg)); break;
                }
            }
        }

        static Stream WriteToFile(string filePath) {
            string name = Path.GetFileName(filePath);
            return new FileStream($"Output/{name}", FileMode.Create, FileAccess.Write);
        }

        static void ConvertARC(string filePath)
        {
            string name = Path.GetFileName(filePath);
            
            RARC_Parser rarc = new RARC_Parser(new MemoryStream(YAZ0.Decompress(filePath)));
            rarc.IsLittleEndian = true;
            foreach (var file in rarc.Files)
                ConvertFile(file);

            var mem = new MemoryStream();
            rarc.Save(mem);
            File.WriteAllBytes($"Output/{name}", YAZ0.Compress(mem.ToArray()));
        }

        static void ConvertFile(RARC_Parser.FileEntry file)
        {
            string ext = Path.GetExtension(file.FileName);
            if (file.FileName.Contains("jmp") || ext == ".bcam" || ext == ".ba" || ext == ".bcsv")
            {
                Console.WriteLine($"Converting BCSV {file.FileName}");
                file.FileData = ConvertBCSV(file.FileData);
            }
            else if (ext == ".kcl") 
                file.FileData = ConvertKCL(file.FileData);
            else if (ext == ".bdl")
                file.FileData = ConvertBMD(file.FileData);
           // else
        //        throw new Exception($"Unsupported file format! Ext: {ext} FileName: {file.FileName}");
        }

        static Stream ConvertBMD(Stream data)
        {
            SuperBMDLib.Model bmd = new SuperBMDLib.Model(data);
            bmd.littleEndian = true;
            var mem = new MemoryStream();
            bmd.Save(mem, true);
            return new MemoryStream(mem.ToArray());
        }

        static Stream ConvertBCSV(Stream data)
        {
            BCSV bcsv = new BCSV(data);
            bcsv.IsBigEndian = false;
            var mem = new MemoryStream();
            bcsv.Save(mem);
            return new MemoryStream(mem.ToArray());
        }

        static Stream ConvertKCL(Stream data)
        {
            KCLFile kcl = new KCLFile(data);
            kcl.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
            var mem = new MemoryStream();
            kcl.Save(mem);
            return mem;
        }
    }
}
