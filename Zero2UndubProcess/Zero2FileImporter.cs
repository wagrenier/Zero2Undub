using System;
using System.IO;
using System.Linq;

namespace Zero2UndubProcess
{
    public class Zero2FileImporter
    {
        public int UndubbedFiles { get; private set; }
        public bool IsCompleted { get; private set; } = false;

        private static int TocLocationInUsIso = 0x2F90B8;
        private static int ImgBinStartAddressInIso = 0x30D40000;
        private FileInfo JpIsoFile { get; set; }
        private FileInfo UsIsoFile { get; set; }
        private BinaryReader jpIsoData { get; set; }
        private BinaryWriter usIsoData { get; set; }
        private Zero2TocFile _usFileDb { get; set; }
        private Zero2TocFile _jpFileDb { get; set; }

        public Zero2FileImporter(string usIsoFile, string jpIsoFile)
        {
            var tempFile = new FileInfo(usIsoFile);
            File.Copy(tempFile.FullName, $"{tempFile.DirectoryName}/ff2_undub.iso");
            UsIsoFile = new FileInfo($"{tempFile.DirectoryName}/ff2_undub.iso");
            JpIsoFile = new FileInfo(jpIsoFile);
            jpIsoData = new BinaryReader(File.OpenRead(JpIsoFile.FullName));
            usIsoData = new BinaryWriter(File.OpenWrite(UsIsoFile.FullName));
            _usFileDb = Zero2TocFile.CreateUsFileDb();
            _jpFileDb = Zero2TocFile.CreateJpFileDb();
        }

        public static void LaunchUndub(string usIsoFile, string jpIsoFile)
        {
            var fileImporter = new Zero2FileImporter(usIsoFile, jpIsoFile);
            fileImporter.UndubGame();
        }

        public void UndubGame()
        {
            for (var i = 0; i < Ps2Constants.NumberFiles; i++)
            {
                var currentFileJp = _jpFileDb.Zero2Files[i];
                var currentFileUs = _usFileDb.Zero2Files[i];
                UndubbedFiles = i;

                if (currentFileJp.FileExtension == FileExtension.Video || currentFileJp.FileExtension == FileExtension.Audio)
                {
                    if (currentFileJp.Size <= currentFileUs.Size)
                    {
                        Console.WriteLine($"Undubbing file {currentFileJp.FileId} of type {currentFileJp.FileExtension}");
                        WriteNewFile(currentFileUs, currentFileJp);
                    }
                    else
                    {
                        Console.WriteLine($"File {currentFileJp.FileId} of type {currentFileJp.FileExtension} is too big");
                    }
                }
            }
            
            Console.WriteLine("Done!");

            IsCompleted = true;
        }

        private void WriteNewFile(Zero2File usFile, Zero2File jpFile)
        {
            JpSeekFile(jpFile);
            UsSeekFile(usFile);
            
            var newFileBuffer = jpIsoData.ReadBytes((int)jpFile.Size);
            usIsoData.Write(newFileBuffer);

            if (jpFile.Size < usFile.Size)
            {
                WriteFileNewSize(jpFile.FileId, (int)jpFile.Size);
            }
        }

        private void JpSeekFile(Zero2File zero2File)
        {
            jpIsoData.BaseStream.Seek(ImgBinStartAddressInIso, SeekOrigin.Begin);
            jpIsoData.BaseStream.Seek(zero2File.BinStartAddress, SeekOrigin.Current);
        }
        
        private void UsSeekFile(Zero2File zero2File)
        {
            usIsoData.BaseStream.Seek(ImgBinStartAddressInIso, SeekOrigin.Begin);
            usIsoData.BaseStream.Seek(zero2File.BinStartAddress, SeekOrigin.Current);
        }

        private void WriteFileNewSize(int fileIndex, int newSize)
        {
            var fileLocation = fileIndex * 0xc + 4;
            usIsoData.Seek(TocLocationInUsIso, SeekOrigin.Begin);
            usIsoData.Seek(fileLocation, SeekOrigin.Current);
            usIsoData.Write(BitConverter.GetBytes(newSize).Reverse().ToArray());
        }
    }
}