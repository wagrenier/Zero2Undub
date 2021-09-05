using System;
using System.IO;
using Zero2UndubProcess.Audio;
using Zero2UndubProcess.GameFiles;
using Zero2UndubProcess.Process;
using Zero2UndubProcess.Pss;

namespace Zero2UndubProcess.Iso
{
    public class IsoHandler
    {
        private readonly IsoReader OriginIsoReader;
        private readonly IsoReader TargetIsoReader;
        private readonly IsoWriter TargetIsoWriter;
        private readonly RegionHandler IsoRegionHandler;
        private readonly string Folder;

        public IsoHandler(string originFile, string targetFile, RegionHandler regionHandler)
        {
            IsoRegionHandler = regionHandler;
            var originIso = new FileInfo(originFile);
            var targetIso = new FileInfo(targetFile);

            Folder = targetIso.DirectoryName;

            File.Copy(targetIso.FullName, $"{targetIso.DirectoryName}/pz2_redux.iso");
            
            var targetIsoInfo = new FileInfo($"{targetIso.DirectoryName}/pz2_redux.iso");

            OriginIsoReader = new IsoReader(originIso, regionHandler.OriginRegionInfo);

            TargetIsoReader = new IsoReader(targetIso, regionHandler.TargetRegionInfo);

            TargetIsoWriter = new IsoWriter(targetIsoInfo, regionHandler.TargetRegionInfo);
        }

        public void Close()
        {
            OriginIsoReader.Close();
            TargetIsoReader.Close();
            TargetIsoWriter.Close();
        }

        public void WriteNewFile(ZeroFile origin, ZeroFile target)
        {
            var originFileContent = GetFileContentOrigin(origin);
            TargetIsoWriter.OverwriteFile(origin, target, originFileContent);
        }

        public ZeroFile TargetGetFile(int fileId)
        {
            return TargetIsoReader.ExtractFileInfo(fileId);
        }
        
        public ZeroFile OriginGetFile(int fileId)
        {
            return OriginIsoReader.ExtractFileInfo(fileId);
        }
        
        public void LargerVideoUndub(ZeroFile origin, ZeroFile target)
        {
            var originVideoContent = GetFileContentOrigin(origin);
            var targetVideoContent = GetFileContentTarget(target);

            var newVideoBuffer = PssAudioHandler.SwitchPssAudio(originVideoContent, targetVideoContent);
            
            TargetIsoWriter.OverwriteFile(origin, target, newVideoBuffer);
        }

        public void AudioUndub(ZeroFile origin, ZeroFile target)
        {
            var originHeaderFile = OriginGetFile(origin.FileId - 1);
            var targetHeaderFile = TargetGetFile(target.FileId - 1);
            
            ExtractAudioFile(origin);

            var newFrequency = 20000;

            var compressedAudioContent = ExternalProcess.MfAudioCompress(origin, originHeaderFile.AudioHeader, newFrequency, Folder);
            
            if (compressedAudioContent == null || target.Size < compressedAudioContent.Length)
            {
                File.Delete($"{Folder}/{origin.FileId}.str");
                return;
            }
            
            TargetIsoWriter.OverwriteFile(origin, target, compressedAudioContent);
            TargetIsoWriter.OverwriteFile(originHeaderFile, targetHeaderFile, GetFileContentOrigin(originHeaderFile));
            
            TargetIsoWriter.WriteAudioHeader(originHeaderFile, targetHeaderFile, newFrequency);
            
            File.Delete($"{Folder}/{origin.FileId}.str");
        }

        private void ExtractAudioFile(ZeroFile origin)
        {
            var fileContent = GetFileContentOrigin(origin);
            var binaryWriter = new BinaryWriter(File.OpenWrite($"{Folder}/{origin.FileId}.str"));
            binaryWriter.Write(fileContent);
            binaryWriter.Close();
        }

        private byte[] GetFileContentOrigin(ZeroFile origin)
        {
            return OriginIsoReader.ExtractFileContent(origin);
        }
        
        private byte[] GetFileContentTarget(ZeroFile target)
        {
            return TargetIsoReader.ExtractFileContent(target);
        }
    }
}