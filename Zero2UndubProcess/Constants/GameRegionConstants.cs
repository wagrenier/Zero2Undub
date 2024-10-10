namespace Zero2UndubProcess.Constants
{
    public static class GameRegionConstants
    {
        public static class EuIsoConstants
        {
            public const string TitleId = "SLES_523.84";
            public const int NumberFiles = 0x118b;
            public const long FileTableStartAddress = 0x286FF8;
            public const long FileTypeTableStartAddress = 0x294280;
            public const long FileArchiveStartAddress = 0x493E0000;
            public const long FileArchiveEndAddress = 0x92A40000;
            public const long FileArchiveEndIsoAddress = 0xDBE20000;
            public const long LogoDatOffset = 0x251BD0;
            public static long[] DmyEntryOffset = { 0x828E4, 0x82926, 0x82A56 };
            public static long[] DmyEntrySize = { 1106860, 3465, 10944516 };
            public const long BinEntryOffset = 0x82A16;
        }

        public static class UsIsoConstants
        {
            public const string TitleId = "SLUS_207.66";
            public const int NumberFiles =                0x0000106B;
            public const long FileTableStartAddress =     0x002F90B8;
            public const long FileTypeTableStartAddress = 0x003055C0;
            public const long FileArchiveStartAddress =   0x30D40000;
            public const long FileArchiveEndAddress =     0x9168B000;
            public const long FileArchiveEndIsoAddress =  0xC23CB000;
            public const long LogoDatOffset =             0x002C4130;
            /// <summary>
            /// DMY_FILE.MAP, DMY_FILE.IRX, DMY_FILE.PSS
            /// </summary>
            public static long[] DmyEntryOffset = { 0x828E4, 0x82926, 0x82A56 };
            public static long[] DmyEntrySize = { 1106860, 3465, 10944516 };
            public const long BinEntryOffset = 0x82A16;
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
            public const long LogoDatOffset = 0x2C4130;
            public static long[] DmyEntryOffset = { 0x828E4, 0x82926, 0x82A56 };
            public static long[] DmyEntrySize = { 1106860, 3465, 10944516 };
            public const long BinEntryOffset = 0x82A16;
        }
    }
}