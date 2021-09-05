namespace Zero2UndubProcess.Audio
{
    public class AudioFileInfo
    {
        public int Frequency { get; set; }
        public int Interleave { get; set; }
        public int Offset { get; set; }
        public int Channel { get; set; }
        public byte PlaybackSpeed { get; set; }
    }
}