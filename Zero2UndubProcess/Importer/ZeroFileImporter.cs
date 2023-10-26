using System;
using Zero2UndubProcess.Constants;
using Zero2UndubProcess.GameFiles;
using Zero2UndubProcess.Iso;
using Zero2UndubProcess.Reporter;

namespace Zero2UndubProcess.Importer
{
    public sealed class ZeroFileImporter
    {
        public InfoReporter InfoReporterUi { get; private set; }
        private readonly UndubOptions _undubOptions;
        private readonly IsoHandler _isoHandler;

        public ZeroFileImporter(UndubOptions undubOptions, string originFile, string targetFile)
        {
            _undubOptions = undubOptions;
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
                    if (targetFile.FileId == 2)
                    {
                        _isoHandler.OverwriteSplashScreen(originFile, targetFile);
                        continue;
                    }

                    if (targetFile.Type != FileType.VIDEO && targetFile.Type != FileType.AUDIO && targetFile.Type != FileType.SOUNDEFFECT)
                    {
                        continue;
                    }

                    if (originFile.Size <= targetFile.Size)
                    {
                        _isoHandler.WriteNewFile(originFile, targetFile);

                        if (originFile.Type != FileType.AUDIO && originFile.Type != FileType.SOUNDEFFECT)
                        {
                            continue;
                        }

                        HandleAudioFile(originFile, targetFile);
                    }
                    else if (targetFile.Type is FileType.AUDIO or FileType.SOUNDEFFECT
                             && !_undubOptions.SafeUndub)
                    {
                        HandleAudioFile(originFile, targetFile);
                        _isoHandler.AppendFile(originFile, targetFile);
                    }
                    else if (targetFile.Type == FileType.VIDEO)
                    {
                        _isoHandler.VideoAudioSwitch(originFile, targetFile);
                    }
                }

                _isoHandler.FillIso();
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

        private void HandleAudioFile(ZeroFile origin, ZeroFile target)
        {
            var originHeaderFile = _isoHandler.OriginGetFile(origin.FileId - 1);
            var targetHeaderFile = _isoHandler.TargetGetFile(target.FileId - 1);

            if (originHeaderFile.Status is FileStatus.Unknown or FileStatus.NoFile
                || originHeaderFile.Type != FileType.AUDIO_HEADER
                || targetHeaderFile.Type != FileType.AUDIO_HEADER)
            {
                return;
            }

            if (targetHeaderFile.Size > originHeaderFile.Size)
            {
                // Makes sure the entire sector of the file is overwritten
                // whenever the source audio header is smaller than the target.
                // This is for handling the edge case of file 0x3133 that
                // caused the game to crash
                originHeaderFile.Size = Ps2Constants.SectorSize - 1;
            }

            _isoHandler.WriteNewFile(originHeaderFile, targetHeaderFile);
        }

        private void CloseFiles()
        {
            _isoHandler.Close();
        }
    }
}