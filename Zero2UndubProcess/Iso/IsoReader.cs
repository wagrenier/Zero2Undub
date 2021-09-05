using System;
using System.IO;
using Zero2UndubProcess.Audio;
using Zero2UndubProcess.Constants;
using Zero2UndubProcess.GameFiles;

namespace Zero2UndubProcess.Iso
{
    public sealed class IsoReader
    {
        private readonly BinaryReader _reader;
        private readonly RegionInfo _regionInfo;

        public IsoReader(FileInfo isoFile, RegionInfo regionInfo)
        {
            _reader = new BinaryReader(File.OpenRead(isoFile.FullName));
            _regionInfo = regionInfo;
        }

        public void Close()
        {
            _reader.Close();
        }
        
        public ZeroFile ExtractFileInfo(int fileId)
        {
            var fileInfoOffset = fileId * GameConstants.FileInfoByteSize;

            SeekFileTableOffset(fileInfoOffset);
            
            var fileInfo = _reader.ReadUInt32();
            var fileSize = _reader.ReadUInt32();
            var fileSizeCompressed = _reader.ReadUInt32();

            var fileStartAddress = (fileInfo >> 2) * Ps2Constants.SectorSize;

            var fileStatus = FileEvaluations.EvaluateFileStatus(fileInfo);

            var sizeInArchive = fileStatus == FileStatus.FileCompressed ? fileSizeCompressed : fileSize;
            
            var fileType = ReadFileTypeTableOffset(fileId);

            AudioFileInfo audioFileInfo = null;

            if (fileType == FileType.AUDIO_HEADER)
            {
                if (fileSize > 0x60)
                {
                    Console.WriteLine($"{fileId} has weird long audio header");
                }
                
                audioFileInfo = ReadAudioFileInfo(fileStartAddress);
            }

            return new ZeroFile
            {
                FileId = fileId,
                Offset = fileStartAddress,
                Size = sizeInArchive,
                SizeUncompressed = fileSize,
                SizeCompress = fileSizeCompressed,
                Status = fileStatus,
                Type = fileType,
                AudioHeader = audioFileInfo
            };
        }

        public byte[] ExtractFileContent(ZeroFile zeroFile)
        {
            SeekFile(zeroFile);
            return _reader.ReadBytes((int) zeroFile.Size);
        }

        private AudioFileInfo ReadAudioFileInfo(long audioHeaderFileAddress)
        {
            var headerOffset = ReadAudioValueAtOffset(audioHeaderFileAddress, 0x4);
            var numChannel = ReadAudioValueAtOffset(audioHeaderFileAddress, 0x8);
            var interleave = ReadAudioValueAtOffset(audioHeaderFileAddress, 0x14);
            var audioFrequency = ReadAudioValueAtOffset(audioHeaderFileAddress, 0x20);
            
            // Read weird header value
            SeekFile(audioHeaderFileAddress);
            _reader.BaseStream.Seek(0x29, SeekOrigin.Current);
            var playbackSpeed = _reader.ReadByte();

            return new AudioFileInfo
            {
                Channel = numChannel,
                Frequency = audioFrequency,
                Interleave = interleave,
                Offset = headerOffset,
                PlaybackSpeed = playbackSpeed
            };
        }

        private int ReadAudioValueAtOffset(long audioHeaderFileAddress, long valueOffset)
        {
            SeekFile(audioHeaderFileAddress);
            _reader.BaseStream.Seek(valueOffset, SeekOrigin.Current);
            return (int) _reader.ReadUInt32();
        }
        
        private void SeekFileTableOffset(long offset)
        {
            _reader.BaseStream.Seek(_regionInfo.FileTableStartAddress, SeekOrigin.Begin);
            _reader.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        private FileType ReadFileTypeTableOffset(int fileId)
        {
            _reader.BaseStream.Seek(_regionInfo.FileTypeTableStartAddress, SeekOrigin.Begin);
            _reader.BaseStream.Seek(fileId, SeekOrigin.Current);
            
            var fileType = _reader.ReadByte();

            return fileType switch
            {
                0xC => FileType.AUDIO_HEADER,
                0xD => FileType.AUDIO,
                0xE => FileType.AUDIO,
                0xF => FileType.VIDEO,
                _ => FileType.UNKNOWN
            };
        }
        
        private void SeekFile(long fileAddress)
        {
            _reader.BaseStream.Seek(_regionInfo.FileArchiveStartAddress, SeekOrigin.Begin);
            _reader.BaseStream.Seek(fileAddress, SeekOrigin.Current);
        }

        private void SeekFile(ZeroFile zeroFile)
        {
            _reader.BaseStream.Seek(_regionInfo.FileArchiveStartAddress, SeekOrigin.Begin);
            _reader.BaseStream.Seek(zeroFile.Offset, SeekOrigin.Current);
        }
    }
}