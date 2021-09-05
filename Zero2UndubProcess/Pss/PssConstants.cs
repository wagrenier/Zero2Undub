namespace Zero2UndubProcess.Pss
{
    public static class PssConstants
    {
          public static readonly byte[]  AudioSegment = new byte[4] {0x00, 0x00, 0x01, 0xBD};
          public static readonly byte[]  PackStart = new byte[4] {0x00, 0x00, 0x01, 0xBA};
          public static readonly byte[] EndFile = new byte[4] {0x00, 0x00, 0x01, 0xB9};
          public const int FirstHeaderSize = 0x3F;
          public const int HeaderSize = 0x17;
    }
}