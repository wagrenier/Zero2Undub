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
        private readonly IsoHandler _isoHandler;
        private readonly UndubOptions _undubOptions;

        public ZeroFileImporter(string originFile, string targetFile, UndubOptions options)
        {
            _undubOptions = options;

            _isoHandler = new IsoHandler(originFile, targetFile);
            
            InfoReporterUi = new InfoReporter
            {
                 IsCompleted = false,
                 IsSuccess = false,
                 TotalFiles = _isoHandler.IsoRegionHandler.TargetRegionInfo.NumberFiles,
                 FilesCompleted = 0
            };
        }

        public void RestoreGame()
        {
            try
            {
                for (var i = 0; i < _isoHandler.IsoRegionHandler.TargetRegionInfo.NumberFiles; i++)
                {
                    InfoReporterUi.FilesCompleted += 1;
                    var targetFile = _isoHandler.TargetGetFile(i);
                    var originFile = _isoHandler.OriginGetFile(i);

                    // Check for splash screen logo
                    if (targetFile.FileId == 3)
                    {
                        
                    }

                    if (targetFile.Type != FileType.VIDEO && targetFile.Type != FileType.AUDIO)
                    {
                        continue;
                    }

                    if (originFile.Size <= targetFile.Size)
                    {
                        _isoHandler.WriteNewFile(originFile, targetFile);

                        if (originFile.Type != FileType.AUDIO)
                        {
                            continue;
                        }

                        targetFile = _isoHandler.TargetGetFile(i - 1);
                        originFile = _isoHandler.OriginGetFile(i - 1);
                        _isoHandler.WriteNewFile(originFile, targetFile);
                    } 
                    else if (targetFile.Type == FileType.AUDIO)
                    {
                        _isoHandler.AppendFile(originFile, targetFile);
                    }
                    else if (targetFile.Type == FileType.VIDEO)
                    {
                        _isoHandler.LargerVideoUndub(originFile, targetFile);
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
            _isoHandler.Close();
        }
    }
}