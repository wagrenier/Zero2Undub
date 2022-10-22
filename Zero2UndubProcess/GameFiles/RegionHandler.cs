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

            if (_targetGameRegion == _originGameRegion)
            {
                throw new Exception(
                    $"You selected ISO for {_targetGameRegion} and {_originGameRegion}. Make sure to select US ISO then JP ISO.");
            }

            if (_targetGameRegion == GameRegions.EU)
            {
                throw new Exception($"Sorry the EU version is not yet supported by this version!");
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

            var titleId = System.Text.Encoding.UTF8.GetString(titleIdBytes);

            GameRegions gameRegion;
            
            switch (titleId)
            {
                case GameRegionConstants.JpIsoConstants.TitleId:
                    gameRegion = GameRegions.Japan;
                    break;
                case GameRegionConstants.UsIsoConstants.TitleId:
                    gameRegion = GameRegions.USA;
                    break;
                default:
                    binaryReader.BaseStream.Position = Ps2Constants.EuGameTitleIdAddress;
                    titleIdBytes = binaryReader.ReadBytes(Ps2Constants.GameTitleIdLength);
                    titleId = System.Text.Encoding.UTF8.GetString(titleIdBytes);

                    gameRegion = titleId == GameRegionConstants.EuIsoConstants.TitleId
                        ? GameRegions.EU
                        : GameRegions.UNKNOWN;
                    break;
            }
            
            binaryReader.Close();

            return gameRegion;
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