namespace Zero2UndubProcess.Constants
{
    public static class GameRegionConstants
    {
        public static class EuIsoConstants
        {
            public const string TitleId = "SLES_523.84";
            public const int NumberFiles = 0x879;
            public const long FileTableStartAddress = 0xA63000;
            public const long FileTypeTableStartAddress = 0x2082D000;
            public const long FileArchiveStartAddress = 0x30D40000;
            public const long FileArchiveEndAddress = 0x9168B000;
            public const long FileArchiveEndIsoAddress = 0xC23CB000;
        }

        public static class UsIsoConstants
        {
            public const string TitleId = "SLUS_207.66";
            public const int NumberFiles = 0x106B;
            public const long FileTableStartAddress = 0x2F90B8;
            public const long FileTypeTableStartAddress = 0x3055C0;
            public const long FileArchiveStartAddress = 0x30D40000;
            public const long FileArchiveEndAddress = 0x9168B000;
            public const long FileArchiveEndIsoAddress = 0xC23CB000;
        }

        public static class JpIsoConstants
        {
            public const string TitleId = "SLPS_253.03";
            public const int NumberFiles = 0x106B;
            public const long FileTableStartAddress = 0x002F85F8;
            public const long FileTypeTableStartAddress = 0x304B00;
            public const long FileArchiveStartAddress = 0x30D40000;
            public const long FileArchiveEndAddress = 0x91566000;
            public const long FileArchiveEndIsoAddress = 0xC22A6000;
        }
    }
}