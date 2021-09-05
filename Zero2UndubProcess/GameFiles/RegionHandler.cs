using Zero2UndubProcess.Constants;

namespace Zero2UndubProcess.GameFiles
{
    public class RegionHandler
    {
        private readonly GameRegions OriginGameRegion;
        private readonly GameRegions TargetGameRegion;
        public readonly RegionInfo OriginRegionInfo;
        public readonly RegionInfo TargetRegionInfo;

        public RegionHandler(GameRegions originGameRegion, GameRegions targetGameRegion)
        {
            OriginGameRegion = originGameRegion;
            TargetGameRegion = targetGameRegion;
            
            // TODO: Add game region check logic

            OriginRegionInfo = new RegionInfo
            {
                FileArchiveStartAddress = GameRegionConstants.JpIsoConstants.FileArchiveStartAddress,
                FileTableStartAddress = GameRegionConstants.JpIsoConstants.FileTableStartAddress,
                FileTypeTableStartAddress = GameRegionConstants.JpIsoConstants.FileTypeTableStartAddress,
                NumberFiles = GameRegionConstants.JpIsoConstants.NumberFiles
            };
            
            TargetRegionInfo = new RegionInfo
            {
                FileArchiveStartAddress = GameRegionConstants.UsIsoConstants.FileArchiveStartAddress,
                FileTableStartAddress = GameRegionConstants.UsIsoConstants.FileTableStartAddress,
                FileTypeTableStartAddress = GameRegionConstants.UsIsoConstants.FileTypeTableStartAddress,
                NumberFiles = GameRegionConstants.UsIsoConstants.NumberFiles
            };
        }
    }

    public class RegionInfo
    {
        public long FileArchiveStartAddress { get; set; }
        public long FileTableStartAddress { get; set; }
        public long FileTypeTableStartAddress { get; set; }
        public int NumberFiles { get; set; }
    }

    public enum GameRegions
    {
        Japan,
        USA,
        EU
    }
}