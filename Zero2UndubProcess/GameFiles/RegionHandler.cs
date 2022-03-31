using System;
using System.IO;
using Zero2UndubProcess.Constants;

namespace Zero2UndubProcess.GameFiles
{
    public class RegionHandler
    {
        private readonly GameRegions _originGameRegion;
        private readonly GameRegions _targetGameRegion;
        public readonly RegionInfo OriginRegionInfo;
        public readonly RegionInfo TargetRegionInfo;
        public readonly bool ShouldSwitch;

        public RegionHandler(FileSystemInfo origin, FileSystemInfo target)
        {
            _originGameRegion = GetGameRegionFromTitleId(origin);
            _targetGameRegion = GetGameRegionFromTitleId(target);
            
            if (_targetGameRegion == GameRegions.Japan)
            {
                ShouldSwitch = true;

                (_targetGameRegion, _originGameRegion) = (_originGameRegion, _targetGameRegion);
            }

            OriginRegionInfo = GetRegionInfoFromGameRegion(_originGameRegion);

            TargetRegionInfo = GetRegionInfoFromGameRegion(_targetGameRegion);
        }

        private static RegionInfo GetRegionInfoFromGameRegion(GameRegions gameRegion)
        {
            return gameRegion switch
            {
                GameRegions.EU => new RegionInfo
                {
                    FileArchiveStartAddress = GameRegionConstants.EuIsoConstants.FileArchiveStartAddress,
                    FileTableStartAddress = GameRegionConstants.EuIsoConstants.FileTableStartAddress,
                    FileTypeTableStartAddress = GameRegionConstants.EuIsoConstants.FileTypeTableStartAddress,
                    NumberFiles = GameRegionConstants.EuIsoConstants.NumberFiles,
                    FileArchiveEndAddress = GameRegionConstants.EuIsoConstants.FileArchiveEndAddress,
                    FileArchiveEndIsoAddress = GameRegionConstants.EuIsoConstants.FileArchiveEndIsoAddress,
                    LogoDatOffset = GameRegionConstants.EuIsoConstants.LogoDatOffset
                },
                GameRegions.USA => new RegionInfo
                {
                    FileArchiveStartAddress = GameRegionConstants.UsIsoConstants.FileArchiveStartAddress,
                    FileTableStartAddress = GameRegionConstants.UsIsoConstants.FileTableStartAddress,
                    FileTypeTableStartAddress = GameRegionConstants.UsIsoConstants.FileTypeTableStartAddress,
                    NumberFiles = GameRegionConstants.UsIsoConstants.NumberFiles,
                    FileArchiveEndAddress = GameRegionConstants.UsIsoConstants.FileArchiveEndAddress,
                    FileArchiveEndIsoAddress = GameRegionConstants.UsIsoConstants.FileArchiveEndIsoAddress,
                    LogoDatOffset = GameRegionConstants.UsIsoConstants.LogoDatOffset
                },
                GameRegions.Japan => new RegionInfo
                {
                    FileArchiveStartAddress = GameRegionConstants.JpIsoConstants.FileArchiveStartAddress,
                    FileTableStartAddress = GameRegionConstants.JpIsoConstants.FileTableStartAddress,
                    FileTypeTableStartAddress = GameRegionConstants.JpIsoConstants.FileTypeTableStartAddress,
                    NumberFiles = GameRegionConstants.JpIsoConstants.NumberFiles,
                    FileArchiveEndAddress = GameRegionConstants.JpIsoConstants.FileArchiveEndAddress,
                    FileArchiveEndIsoAddress = GameRegionConstants.JpIsoConstants.FileArchiveEndIsoAddress,
                    LogoDatOffset = GameRegionConstants.JpIsoConstants.LogoDatOffset
                },
                GameRegions.UNKNOWN => throw new Exception("Unknown game region."),
                _=> throw new Exception("Unknown game region.")
            };
        }

        private static GameRegions GetGameRegionFromTitleId(FileSystemInfo file)
        {
            var binaryReader = new BinaryReader(File.OpenRead(file.FullName));

            binaryReader.BaseStream.Position = Ps2Constants.GameTitleIdAddress;
            var titleIdBytes = binaryReader.ReadBytes(Ps2Constants.GameTitleIdLength);
            
            binaryReader.Close();
            
            var titleId = System.Text.Encoding.UTF8.GetString(titleIdBytes);

            return titleId switch
            {
                GameRegionConstants.EuIsoConstants.TitleId => GameRegions.EU,
                GameRegionConstants.JpIsoConstants.TitleId => GameRegions.Japan,
                GameRegionConstants.UsIsoConstants.TitleId => GameRegions.USA,
                _ => GameRegions.UNKNOWN
            };
        }
    }

    public class RegionInfo
    {
        public long FileArchiveStartAddress { get; init; }
        public long FileTableStartAddress { get; init; }
        public long FileTypeTableStartAddress { get; init; }
        public long FileArchiveEndAddress { get; set; }
        public long FileArchiveEndIsoAddress { get; set; }
        public long LogoDatOffset { get; set; }
        public int NumberFiles { get; init; }
    }

    public enum GameRegions
    {
        Japan,
        USA,
        EU,
        UNKNOWN
    }
}