using System;
using Zero2UndubProcess.GameFiles;
using Zero2UndubProcess.Iso;
using Zero2UndubProcess.Options;
using Zero2UndubProcess.Reporter;

namespace Zero2UndubProcess.Importer
{
    public sealed class ZeroFileImporter
    {
        public InfoReporter InfoReporterUi { get; private set; }
        private readonly IsoHandler IsoHandler;
        private readonly RegionHandler IsoRegionHandler;
        private readonly UndubOptions UndubOptions;

        public ZeroFileImporter(string originFile, string targetFile, UndubOptions options)
        {
            UndubOptions = options;

            IsoRegionHandler = new RegionHandler(GameRegions.Japan, GameRegions.USA);

            IsoHandler = new IsoHandler(originFile, targetFile, IsoRegionHandler);
            
            InfoReporterUi = new InfoReporter
            {
                 IsCompleted = false,
                 IsSuccess = false,
                 TotalFiles = IsoRegionHandler.TargetRegionInfo.NumberFiles,
                 FilesCompleted = 0
            };
        }

        public void RestoreGame()
        {
            try
            {
                for (var i = 0; i < IsoRegionHandler.TargetRegionInfo.NumberFiles; i++)
                {
                    InfoReporterUi.FilesCompleted += 1;
                    var targetFile = IsoHandler.TargetGetFile(i);
                    var originFile = IsoHandler.OriginGetFile(i);

                    if (targetFile.Type != FileType.VIDEO && targetFile.Type != FileType.AUDIO)
                    {
                        continue;
                    }
                    
                    if (originFile.Size <= targetFile.Size)
                    {
                        IsoHandler.WriteNewFile(originFile, targetFile);
                        
                        if (originFile.Type == FileType.AUDIO)
                        {
                            targetFile = IsoHandler.TargetGetFile(i - 1);
                            originFile = IsoHandler.OriginGetFile(i - 1);
                            IsoHandler.WriteNewFile(originFile, targetFile);
                        }
                    } 
                    else if (targetFile.Type == FileType.AUDIO)
                    {
                        IsoHandler.AudioUndub(originFile, targetFile);
                    }
                    else if (targetFile.Type == FileType.VIDEO)
                    {
                        IsoHandler.LargerVideoUndub(originFile, targetFile);
                    }
                }

                InfoReporterUi.IsSuccess = true;
            }
            catch (Exception e)
            {
                InfoReporterUi.ErrorMessage = e.Message;
                InfoReporterUi.IsSuccess = false;
            }

            InfoReporterUi.IsCompleted = true;
            CloseFiles();
        }
        
        private void CloseFiles()
        {
            IsoHandler.Close();
        }
    }
}