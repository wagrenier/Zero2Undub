using System;
using System.Collections.Generic;
using static System.Enum;
using static System.IO.File;
using System.Text.Json;

namespace Zero2UndubProcess
{
    public static class FileExtensions
    {
        
    }

    public static class Ps2Constants
    {
        public static int SectorSize = 0x800;

        public static int NumberFiles = 0x106B;
    }

    public class Zero2TocFile
    {
        private readonly int _pCdDat;
        private readonly Dictionary<string, long> _cdDatTbl;
        private readonly List<int> _fileExtDat;
        public List<Zero2File> Zero2Files { get; set; }

        public Zero2TocFile(int p_cd_dat, string cd_dat_tbl, string file_ext_dat)
        {
            _pCdDat = p_cd_dat;
            
            _cdDatTbl = JsonSerializer.Deserialize<Dictionary<string, long>>(ReadAllText(cd_dat_tbl));
            _fileExtDat = JsonSerializer.Deserialize<List<int>>(ReadAllText(file_ext_dat));
            Zero2Files = new List<Zero2File>();
        }

        public static Zero2TocFile CreateUsFileDb()
        {
            var usDb = new Zero2TocFile(0x002F30B8, "cd_dat_tbl.json", "file_ext_tbl.json");
            usDb.BuildFileDb();

            return usDb;
        }
        
        public static Zero2TocFile CreateJpFileDb()
        {
            var jpDb = new Zero2TocFile(0x002F25F8, "cd_dat_tbl_jp.json", "file_ext_tbl.json");
            jpDb.BuildFileDb();

            return jpDb;
        }

        private long GetValueAtAddress(int address)
        {
            return _cdDatTbl[$"0x{Convert.ToString(address, 16)}"];
        }

        private long ComputeImgBinFileAddress(int fileIndex)
        {
            return (GetValueAtAddress(GetFileFromIndex(fileIndex)) >> 2) * Ps2Constants.SectorSize;
        }

        private void BuildFileDb()
        {
            for (var i = 0; i < Ps2Constants.NumberFiles; i++)
            {
                ExtractFile(i);
            }
        }

        private void ExtractFile(int fileIndex)
        {
            var file = GetFileFromIndex(fileIndex);

            var fileStatus = GetFileStatus(fileIndex);

            var fileSize = GetFileSize(fileIndex);

            var fileSizeCompressed = GetFileSizeCompressed(fileIndex);

            var fileBdAddress = ComputeImgBinFileAddress(fileIndex);

            long fileBdEndAddress = 0;

            if (fileStatus == FileStatus.FileCompressed)
            {
                fileBdEndAddress = fileBdAddress + fileSizeCompressed;
            }
            else
            {
                fileBdEndAddress = fileBdAddress + fileSize;
            }

            Zero2Files.Add(new Zero2File
            {
                Size = fileSize,
                Status = fileStatus,
                FileExtension = GetFileExtension(_fileExtDat[fileIndex]),
                FileId = fileIndex,
                SizeCompress = fileSizeCompressed,
                StartSector = GetFileStartSector(fileIndex),
                BinStartAddress = fileBdAddress,
                BinEndAddress = fileBdEndAddress,
                CdStartAddress = file
            });
        }

        private string GetFileExtension(int fileType)
        {
            switch (fileType)
            {
                case 0xC: return "DXH";
                case 0xD: return "str";
                case 0xE: return "str";
                case 0xF: return "pss";
                default: return "out";
            }
        }
        
        private int GetFileStartSector(int fileIndex)
        {
            // Wrong value right now, but it is never used so I won't waste time for now
            return GetFileFromIndex(fileIndex) >> 2;
        }
        
        private long GetFileSizeCompressed(int fileIndex)
        {
            var fileSize = GetFileFromIndex(fileIndex) + 0x8;
            return GetValueAtAddress(fileSize);
        }

        private long GetFileSize(int fileIndex)
        {
            var fileSize = GetFileFromIndex(fileIndex) + 0x4;
            return GetValueAtAddress(fileSize);
        }

        private FileStatus GetFileStatus(int fileIndex)
        {
            var fileStatus = GetValueAtAddress(GetFileFromIndex(fileIndex)) & 0b00000011;

            var tryParse = TryParse<FileStatus>(fileStatus.ToString(), out var returnValue);

            return !tryParse ? FileStatus.Unknown : returnValue;
        }

        private int GetFileFromIndex(int file)
        {
            return _pCdDat + file * 0xC;
        }
    }
}
