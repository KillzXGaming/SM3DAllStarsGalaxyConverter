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
                if (ext == ".arc")
                    ConvertARC(arg);
                if (ext == ".kcl")
                    ConvertKCL(WriteToFile(arg));
                if (ext == ".ba" || ext == ".bcam" || ext == ".bcsv")
                    ConvertBCSV(WriteToFile(arg));
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
            if (file.FileName.Contains("jmp") || ext == ".bcam" || ext == ".ba")
            {
                Console.WriteLine($"Converting BCSV {file.FileName}");
                file.FileData = ConvertBCSV(file.FileData);
            }
            else if (ext == ".kcl") {
                file.FileData = ConvertKCL(file.FileData);
            }
            else
                throw new Exception($"Unsupported file format! Ext: {ext} FileName: {file.FileName}");
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
