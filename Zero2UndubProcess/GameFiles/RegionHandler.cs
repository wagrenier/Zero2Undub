using System;
using System.IO;
using Zero2UndubProcess.Constants;

namespace Zero2UndubProcess.GameFiles
{
    public class RegionHandler
    {
        private readonly GameRegions OriginGameRegion;
        private readonly GameRegions TargetGameRegion;
        public readonly RegionInfo OriginRegionInfo;
        public readonly RegionInfo TargetRegionInfo;
        public readonly bool ShouldSwitch = false;

        public RegionHandler(FileInfo origin, FileInfo target)
        {
            OriginGameRegion = GetGameRegionFromTitleId(origin);
            TargetGameRegion = GetGameRegionFromTitleId(target);

            if (TargetGameRegion == GameRegions.Japan)
            {
                ShouldSwitch = true;

                var temp = TargetGameRegion;
                TargetGameRegion = OriginGameRegion;
                OriginGameRegion = temp;
            }

            OriginRegionInfo = GetRegionInfoFromGameRegion(OriginGameRegion);

            TargetRegionInfo = GetRegionInfoFromGameRegion(TargetGameRegion);
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
                        NumberFiles = GameRegionConstants.EuIsoConstants.NumberFiles
                    },
                GameRegions.USA => new RegionInfo
                    {
                        FileArchiveStartAddress = GameRegionConstants.UsIsoConstants.FileArchiveStartAddress,
                        FileTableStartAddress = GameRegionConstants.UsIsoConstants.FileTableStartAddress,
                        FileTypeTableStartAddress = GameRegionConstants.UsIsoConstants.FileTypeTableStartAddress,
                        NumberFiles = GameRegionConstants.UsIsoConstants.NumberFiles
                    },
                GameRegions.Japan => new RegionInfo
                    {
                        FileArchiveStartAddress = GameRegionConstants.JpIsoConstants.FileArchiveStartAddress,
                        FileTableStartAddress = GameRegionConstants.JpIsoConstants.FileTableStartAddress,
                        FileTypeTableStartAddress = GameRegionConstants.JpIsoConstants.FileTypeTableStartAddress,
                        NumberFiles = GameRegionConstants.JpIsoConstants.NumberFiles
                    },
                GameRegions.UNKNOWN => throw new Exception("Unknown game region."),
                _=> throw new Exception("Unknown game region.")
            };
        }

        private static GameRegions GetGameRegionFromTitleId(FileInfo file)
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