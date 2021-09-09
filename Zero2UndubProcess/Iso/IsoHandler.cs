using System.IO;
using Zero2UndubProcess.GameFiles;
using Zero2UndubProcess.Pss;

namespace Zero2UndubProcess.Iso
{
    public class IsoHandler
    {
        private readonly IsoReader _originIsoReader;
        private readonly IsoReader _targetIsoReader;
        private readonly IsoWriter _targetIsoWriter;
        public RegionHandler IsoRegionHandler { get; private set; }

        public IsoHandler(string originFile, string targetFile)
        {
            var originIso = new FileInfo(originFile);
            var targetIso = new FileInfo(targetFile);
            
            IsoRegionHandler = new RegionHandler(originIso, targetIso);

            if (IsoRegionHandler.ShouldSwitch)
            {
                var temp = originIso;
                originIso = targetIso;
                targetIso = temp;
            }

            File.Copy(targetIso.FullName, $"{targetIso.DirectoryName}/pz2_redux.iso");
            
            var targetIsoInfo = new FileInfo($"{targetIso.DirectoryName}/pz2_redux.iso");

            _originIsoReader = new IsoReader(originIso, IsoRegionHandler.OriginRegionInfo);

            _targetIsoReader = new IsoReader(targetIso, IsoRegionHandler.TargetRegionInfo);

            _targetIsoWriter = new IsoWriter(targetIsoInfo, IsoRegionHandler.TargetRegionInfo);
        }

        public void Close()
        {
            _originIsoReader.Close();
            _targetIsoReader.Close();
            _targetIsoWriter.Close();
        }

        public void WriteNewFile(ZeroFile origin, ZeroFile target)
        {
            var originFileContent = GetFileContentOrigin(origin);
            _targetIsoWriter.OverwriteFile(origin, target, originFileContent);
        }

        public ZeroFile TargetGetFile(int fileId)
        {
            return _targetIsoReader.ExtractFileInfo(fileId);
        }
        
        public ZeroFile OriginGetFile(int fileId)
        {
            return _originIsoReader.ExtractFileInfo(fileId);
        }
        
        public void LargerVideoUndub(ZeroFile origin, ZeroFile target)
        {
            var originVideoContent = GetFileContentOrigin(origin);
            var targetVideoContent = GetFileContentTarget(target);

            var newVideoContent = PssAudioHandler.SwitchPssAudio(originVideoContent, targetVideoContent);
            
            _targetIsoWriter.OverwriteFile(origin, target, newVideoContent);
        }

        public void AudioUndub(ZeroFile origin, ZeroFile target)
        {
            var originHeaderFile = OriginGetFile(origin.FileId - 1);
            var targetHeaderFile = TargetGetFile(target.FileId - 1);
            
            _targetIsoWriter.AppendFile(origin, target, GetFileContentOrigin(origin));

            _targetIsoWriter.OverwriteFile(originHeaderFile, targetHeaderFile, GetFileContentOrigin(originHeaderFile));
        }

        private byte[] GetFileContentOrigin(ZeroFile origin)
        {
            return _originIsoReader.ExtractFileContent(origin);
        }
        
        private byte[] GetFileContentTarget(ZeroFile target)
        {
            return _targetIsoReader.ExtractFileContent(target);
        }
    }
}