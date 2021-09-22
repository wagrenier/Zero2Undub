namespace Zero2UndubProcess.GameFiles
{
    public class ZeroFile
    {
        public int FileId { get; init; }
        public long Offset { get; init; }
        public long Size { get; set; }
        public long SizeUncompressed { get; init; }
        public long SizeCompress { get; init; }
        public FileStatus Status { get; init; }
        public FileType Type { get; init; }
    }
}