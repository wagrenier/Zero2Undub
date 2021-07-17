namespace Zero2UndubProcess
{
    public enum FileStatus
    {
        NoFile,
        FileNotCompressed = 2,
        FileCompressed = 3,
        Unknown = 4
    }

    public static class FileExtension
    {
        public const string Less = "LESS";

        public const string Dxh = "DXH";

        public const string Audio = "str";

        public const string Video = "pss";
    }

    public class Zero2File
    {
        public int FileId { get; set; }

        public int CdStartAddress { get; set; }

        public int StartSector { get; set; }

        public FileStatus Status { get; set; }

        public long BinStartAddress { get; set; }

        public long BinEndAddress { get; set; }

        public long Size { get; set; }

        public long SizeCompress { get; set; }

        public string FileExtension { get; set; }
    }
}