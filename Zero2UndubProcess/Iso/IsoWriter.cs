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

        public IsoWriter(FileInfo isoFile, RegionInfo regionInfo)
        {
            _writer = new BinaryWriter(File.OpenWrite(isoFile.FullName));
            _regionInfo = regionInfo;
        }

        public void Close()
        {
            _writer.Close();
        }

        public void OverwriteFile(ZeroFile origin, ZeroFile target, byte[] buffer)
        {
            var newFileSize = buffer.Length;
            
            // Reimplement Sector Check For Additional Space
            if (newFileSize > (int) target.Size)
            {
                Console.WriteLine("big");
                return;
            }

            WriteNewSizeFile(origin, target, newFileSize);

            SeekFile(target);
            _writer.Write(buffer);
        }

        public void WriteAudioHeader(ZeroFile origin, ZeroFile target, int frequency)
        {
            var newAudioPlayback = new byte[] {GetNewAudioPlayback(origin.AudioHeader.PlaybackSpeed)};
            
            WriteAudioValueAtOffset(target, 0x4, BitConverter.GetBytes(origin.AudioHeader.Offset));
            WriteAudioValueAtOffset(target, 0x20, BitConverter.GetBytes(frequency));
            WriteAudioValueAtOffset(target, 0x29, newAudioPlayback);

            if (origin.AudioHeader.Channel == 2)
            {
                WriteAudioValueAtOffset(target, 0x14, BitConverter.GetBytes(origin.AudioHeader.Interleave));
                WriteAudioValueAtOffset(target, 0x3C, BitConverter.GetBytes(frequency));
                WriteAudioValueAtOffset(target, 0x45, newAudioPlayback);
            }
        }
        
        private byte GetNewAudioPlayback(byte originPlayback)
        {
            return originPlayback switch
            {
                0x8 => 0x6, // None
                0x9 => 0x6, // None
                0xa => 0x6, // 32000Hz
                0xb => 0x3,
                0xc => 0x4,
                0xd => 0x5,
                0xe => 0x6, // 44100Hz
                0xf => 0x7,
                _ => originPlayback
            };
        }
        
        private void WriteAudioValueAtOffset(ZeroFile origin, long valueOffset, byte[] content)
        {
            SeekFile(origin);
            _writer.BaseStream.Seek(valueOffset, SeekOrigin.Current);
            _writer.Write(content);
        }

        private void WriteNewSizeFile(ZeroFile origin, ZeroFile target, int newSize)
        {
            var fileSizeOffset = target.FileId * GameConstants.FileInfoByteSize + 0x4;
            SeekFileTableOffset(fileSizeOffset);
            _writer.Write((uint) newSize);
            
            // TODO: Add code to handle when I rewrite compressed files
            //_writer.Write(oldFile.Status == FileStatus.FileCompressed ? newFile.SizeUncompressed : newSize);
            //_writer.Write(oldFile.Status == FileStatus.FileCompressed ? newSize : 0);
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
    }
}