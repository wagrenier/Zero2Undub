using static System.Enum;

namespace Zero2UndubProcess.GameFiles
{
    public enum FileStatus
    {
        NoFile,
        FileNotCompressed = 2,
        FileCompressed = 3,
        Unknown = 4
    }
    
    public enum FileType
    {
        UNKNOWN,
        AUDIO,
        AUDIO_HEADER,
        VIDEO,
        SOUNDEFFECT
    }

    public static class FileEvaluations
    {
        public static FileStatus EvaluateFileStatus(uint valueRead)
        {
            var fileStatus = valueRead & 0b00000011;
            
            var tryParse = TryParse<FileStatus>(fileStatus.ToString(), out var returnValue);

            return !tryParse ? FileStatus.Unknown : returnValue;
        }
    }
}