using System.IO;
using Zero2UndubProcess.Constants;
using Zero2UndubProcess.GameFiles;

namespace Zero2UndubProcess.Iso
{
    public sealed class IsoReader
    {
        private readonly BinaryReader _reader;
        private readonly RegionInfo _regionInfo;

        public IsoReader(FileSystemInfo isoFile, RegionInfo regionInfo)
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

            return new ZeroFile
            {
                FileId = fileId,
                Offset = fileStartAddress,
                Size = sizeInArchive,
                SizeUncompressed = fileSize,
                SizeCompress = fileSizeCompressed,
                Status = fileStatus,
                Type = fileType
            };
        }

        public byte[] ExtractFileContent(ZeroFile zeroFile)
        {
            SeekFile(zeroFile);
            return _reader.ReadBytes((int) zeroFile.Size);
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
                0xE => FileType.SOUNDEFFECT,
                0xF => FileType.VIDEO,
                _ => FileType.UNKNOWN
            };
        }

        private void SeekFile(ZeroFile zeroFile)
        {
            _reader.BaseStream.Seek(_regionInfo.FileArchiveStartAddress, SeekOrigin.Begin);
            _reader.BaseStream.Seek(zeroFile.Offset, SeekOrigin.Current);
        }
    }
}