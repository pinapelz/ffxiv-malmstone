using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Malmstone.Services
{
    public class PvPService
    {
        public PvPSeriesInfo? GetPvPSeriesInfo()
        {
            unsafe
            {
                var pvpProfile = PvPProfile.Instance();
                if (pvpProfile != null && pvpProfile->IsLoaded != 0)
                {
                    return new PvPSeriesInfo
                    {
                        CurrentSeriesRank = pvpProfile->GetSeriesCurrentRank(),
                        ClaimedSeriesRank = pvpProfile->GetSeriesClaimedRank(),
                        SeriesExperience = pvpProfile->GetSeriesExperience()
                    };
                }
                return null;
            }
        }
    }

    public class PvPSeriesInfo
    {
        public byte CurrentSeriesRank { get; set; }
        public byte ClaimedSeriesRank { get; set; }
        public ushort SeriesExperience { get; set; }
    }
}
