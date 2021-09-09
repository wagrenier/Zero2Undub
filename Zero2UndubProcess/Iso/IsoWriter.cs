using System;
using System.IO;
using Zero2UndubProcess.Constants;
using Zero2UndubProcess.GameFiles;

namespace Zero2UndubProcess.Iso
{
    public sealed class IsoWriter
    {
        private readonly BinaryWriter _writer;
        private readonly RegionInfo _regionInfo;

        public IsoWriter(FileSystemInfo isoFile, RegionInfo regionInfo)
        {
            _writer = new BinaryWriter(File.OpenWrite(isoFile.FullName));
            _regionInfo = regionInfo;
        }

        public void Close()
        {
            _writer.Close();
        }

        public void OverwriteFile(ZeroFile origin, ZeroFile target, byte[] fileContent)
        {
            var newFileSize = fileContent.Length;
            
            if (newFileSize > (int) target.Size && target.Type != FileType.AUDIO_HEADER)
            {
                Console.WriteLine($"Cannot undub file {target.FileId} of type {target.Type}");
                return;
            }

            WriteNewSizeFile(origin, target, newFileSize);

            SeekFile(target);
            _writer.Write(fileContent);
        }
        
        public void AppendFile(ZeroFile origin, ZeroFile target, byte[] fileContent)
        {
            _writer.BaseStream.Seek(_regionInfo.FileArchiveEndIsoAddress, SeekOrigin.Begin);
            var startAddress = ((uint) (_regionInfo.FileArchiveEndAddress / Ps2Constants.SectorSize) << 2) + 2;
            
            _writer.Write(fileContent);
            
            var blankBytes = Ps2Constants.SectorSize - _writer.BaseStream.Position % Ps2Constants.SectorSize;
            
            WriteEmptyByte((int) blankBytes);

            _regionInfo.FileArchiveEndIsoAddress += fileContent.Length + blankBytes;
            _regionInfo.FileArchiveEndAddress += fileContent.Length + blankBytes;
            
            WriteNewAddressFile(origin, target, startAddress);
        }

        private void WriteNewAddressFile(ZeroFile origin, ZeroFile target, uint newStartAddress)
        {
            var fileSizeOffset = target.FileId * GameConstants.FileInfoByteSize;
            
            SeekFileTableOffset(fileSizeOffset);
            _writer.Write(newStartAddress);
            WriteNewSizeFile(origin, target, (int)origin.Size);
        }

        private void WriteNewSizeFile(ZeroFile origin, ZeroFile target, int newSize)
        {
            var fileSizeOffset = target.FileId * GameConstants.FileInfoByteSize + 0x4;
            SeekFileTableOffset(fileSizeOffset);
            _writer.Write((uint) newSize);
        }
        
        private void SeekFileTableOffset(long offset)
        {
            _writer.BaseStream.Seek(_regionInfo.FileTableStartAddress, SeekOrigin.Begin);
            _writer.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        private void SeekFile(ZeroFile zeroFile)
        {
            _writer.BaseStream.Seek(_regionInfo.FileArchiveStartAddress, SeekOrigin.Begin);
            _writer.BaseStream.Seek(zeroFile.Offset, SeekOrigin.Current);
        }

        private void WriteEmptyByte(int numBlankBytes)
        {
            for (var i = 0; i < numBlankBytes; i++)
            {
                _writer.Write(0x0);
            }
        }
    }
}