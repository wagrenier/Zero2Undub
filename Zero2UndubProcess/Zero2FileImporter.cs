using System;
using System.IO;
using System.Linq;

namespace Zero2UndubProcess
{
    public class Zero2FileImporter
    {
        public int UndubbedFiles { get; private set; }
        public bool IsCompleted { get; private set; } = false;

        public bool IsSuccess { get; private set; } = false;
        
        public string ErrorMessage { get; private set; }

        private const int TocLocationInUsIso = 0x2F90B8;
        private const int ImgBinStartAddressInIso = 0x30D40000;
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
            try
            {
                for (var i = 0; i < Ps2Constants.NumberFiles; i++)
                {
                    var currentFileJp = _jpFileDb.Zero2Files[i];
                    var currentFileUs = _usFileDb.Zero2Files[i];
                    UndubbedFiles = i;

                    if (currentFileJp.FileExtension != FileExtension.Video &&
                        currentFileJp.FileExtension != FileExtension.Audio)
                    {
                        continue;
                    }
                
                    if (currentFileJp.Size <= currentFileUs.Size)
                    {
                        Console.WriteLine($"Undubbing file {currentFileJp.FileId} of type {currentFileJp.FileExtension}");
                        WriteNewFile(currentFileUs, currentFileJp);
                    }
                    else if (currentFileJp.FileExtension == FileExtension.Audio)
                    {
                        CompressAudioFile(currentFileUs, currentFileJp);
                    }
                    else
                    {
                        Console.WriteLine($"File {currentFileJp.FileId} of type {currentFileJp.FileExtension} is too big");
                    }
                }
            
                Console.WriteLine("Done!");

                IsCompleted = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                IsSuccess = false;
                IsCompleted = true;
                return;
            }

            IsSuccess = true;
        }

        private void CompressAudioFile(Zero2File usFile, Zero2File jpFile)
        {
            /**
             * Frequency : 44100, located at address -> 0x20, 0x3C
             * Channels : 2, located at address -> 0x8
             * Samples : 16 bits always
             * Interleave : 800, located at address -> 0x14
             * Offset : 1000, located at address -> 0x4
             *
             * SndBnkPlay
             *
             * MFAudio Command
             * .\MFAudio.exe /IF44100 /IC2 /II800 /IH1000 /OTRAWC /OF44100 /OC2 /OI800 "2581_JP.str" "compressed.str"
             */

            if (_usFileDb.Zero2Files[usFile.FileId - 1].FileExtension != FileExtension.Dxh)
            {
                Console.WriteLine($"Ignore file {usFile.FileId} because its header could not be found");
            }
            
            ExtractAudioFile(jpFile);
            var fileInfo = GetAudioFileInformation(jpFile);
            
            CompressAudio(usFile, jpFile, fileInfo);

            File.Delete($"{UsIsoFile.DirectoryName}/{jpFile.FileId}.{jpFile.FileExtension}");
        }

        private void CompressAudio(Zero2File usFile, Zero2File jpFile, AudioFileInfo fileInfo)
        {
            var wasSuccessful = ExternalProcesses.MfAudioCompress(jpFile, fileInfo, 20000, UsIsoFile.DirectoryName);

            if (!wasSuccessful)
            {
                return;
            }

            fileInfo.Frequency = 20000;
            WriteNewCompressedFile(usFile, jpFile, fileInfo);
            
            File.Delete($"{UsIsoFile.DirectoryName}/{jpFile.FileId}_compressed.{jpFile.FileExtension}");
        }
        
        private AudioFileInfo GetAudioFileInformation(Zero2File audioFile)
        {
            /**
             * Frequency : 44100, located at address -> 0x20, 0x3C
             * Channels : 2, located at address -> 0x8
             * Samples : 16 bits always
             * Interleave : 800, located at address -> 0x14
             * Offset : 1000, located at address -> 0x4
             *
             * 0x28 & 0x44 -> write 6555306
             *
             * When there is only 1 audio channel, then there is not interleave
             */

            var headerFile = _jpFileDb.Zero2Files[audioFile.FileId - 1];
            
            JpSeekFile(headerFile);

            jpIsoData.BaseStream.Seek(0x20, SeekOrigin.Current);
            
            var audioFrequency = jpIsoData.ReadUInt16();
            
            JpSeekFile(headerFile);
            
            jpIsoData.BaseStream.Seek(0x14, SeekOrigin.Current);

            var interleave = jpIsoData.ReadInt16();
            
            JpSeekFile(headerFile);

            jpIsoData.BaseStream.Seek(0x4, SeekOrigin.Current);
            
            var offset = jpIsoData.ReadInt16();
            
            JpSeekFile(headerFile);

            jpIsoData.BaseStream.Seek(0x8, SeekOrigin.Current);
            
            var channel = jpIsoData.ReadInt16();

            return new AudioFileInfo
            {
                Frequency = audioFrequency,
                Interleave = interleave,
                Offset = offset,
                Channel = channel
            };
        }

        private void WriteAudioFileHeader(Zero2File audioFile, AudioFileInfo fileInfo)
        {
            var headerFile = _usFileDb.Zero2Files[audioFile.FileId - 1];
            var headerFileJp = _jpFileDb.Zero2Files[audioFile.FileId - 1];
            
            JpSeekFile(headerFileJp);

            var buff = jpIsoData.ReadBytes((int)headerFileJp.Size);
            
            UsSeekFile(headerFile);
            
            usIsoData.Write(buff);
            
            UsSeekFile(headerFile);
            
            Console.WriteLine($"Stream Offset {usIsoData.BaseStream.Position}");

            usIsoData.Seek(0x20, SeekOrigin.Current);
            
            usIsoData.Write((UInt16)fileInfo.Frequency);

            if (fileInfo.Channel == 2)
            {
                UsSeekFile(headerFile);
            
                usIsoData.Seek(0x3C, SeekOrigin.Current);
            
                usIsoData.Write((UInt16)fileInfo.Frequency);
                
                UsSeekFile(headerFile);

                usIsoData.Seek(0x14, SeekOrigin.Current);
            
                usIsoData.Write(fileInfo.Interleave);
                
                // 0x28 & 0x44 -> write 6555306
                UsSeekFile(headerFile);

                usIsoData.Seek(0x44, SeekOrigin.Current);
            
                usIsoData.Write(6555306);
            }
            
            // 0x28 & 0x44 -> write 6555306
            UsSeekFile(headerFile);

            usIsoData.Seek(0x28, SeekOrigin.Current);
            
            usIsoData.Write(6555306);
            
            UsSeekFile(headerFile);

            usIsoData.Seek(0x4, SeekOrigin.Current);
            
            usIsoData.Write(fileInfo.Offset);
        }

        private void ExtractAudioFile(Zero2File audioFile)
        {
            JpSeekFile(audioFile);
            var audioFileBuffer = jpIsoData.ReadBytes((int)audioFile.Size);
            var binaryWriter = new BinaryWriter(File.OpenWrite($"{UsIsoFile.DirectoryName}/{audioFile.FileId}.{audioFile.FileExtension}"));
            binaryWriter.Write(audioFileBuffer);
            binaryWriter.Close();
        }

        private bool WriteNewCompressedFile(Zero2File usFile, Zero2File jpFile, AudioFileInfo fileInfo)
        {
            var compressedFileInfo = new FileInfo($"{UsIsoFile.DirectoryName}/{jpFile.FileId}_compressed.{jpFile.FileExtension}");

            if (compressedFileInfo.Length > usFile.Size)
            {
                Console.WriteLine($"File size {compressedFileInfo.Length} is still too big");
                return false;
            }
            
            var binaryReader = new BinaryReader(File.OpenRead($"{UsIsoFile.DirectoryName}/{jpFile.FileId}_compressed.{jpFile.FileExtension}"));
            var fileBuffer = binaryReader.ReadBytes((int) compressedFileInfo.Length);
            binaryReader.Close();
            
            UsSeekFile(usFile);
            
            usIsoData.Write(fileBuffer);
            
            WriteFileNewSize(usFile.FileId, (int) compressedFileInfo.Length);
            
            WriteAudioFileHeader(usFile, fileInfo);

            return true;
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

            var buffer = BitConverter.GetBytes(newSize);
            usIsoData.Write(buffer);
        }
    }
    
    public class AudioFileInfo
    {
        public int Frequency { get; set; }
        public int Interleave { get; set; }
        public int Offset { get; set; }
        public int Channel { get; set; }
    }
}