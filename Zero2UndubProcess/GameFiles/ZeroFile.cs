using Zero2UndubProcess.Audio;

namespace Zero2UndubProcess.GameFiles
{
    public class ZeroFile
    {
        public int FileId { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }
        public long SizeUncompressed { get; set; }
        public long SizeCompress { get; set; }
        public FileStatus Status { get; set; }
        public FileType Type { get; set; }
        public AudioFileInfo AudioHeader { get; set; }
    }
}