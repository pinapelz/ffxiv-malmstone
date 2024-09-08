using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Malmstone.Services
{
    public class PvPService
    {
        public int CurrentFrontlineLosingBonus = -1;

        public enum FrontlinePlacement
        {
            FirstPlace = 1, SecondPlace = 2, ThirdPlace = 3, Unknown=4
        }

        public struct PVPProfileFrontlineResults
        {
            public uint FirstPlace;
            public uint SecondPlace;
            public uint ThirdPlace;
        }

        public PVPProfileFrontlineResults CachedFrontlineResults;

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

        public bool UpdateFrontlineResultCache()
        {
            unsafe
            {
                var pvpProfile = PvPProfile.Instance();
                if (pvpProfile != null && pvpProfile->IsLoaded != 0)
                {
                    CachedFrontlineResults = new PVPProfileFrontlineResults
                    {
                        FirstPlace = pvpProfile->FrontlineTotalFirstPlace,
                        SecondPlace = pvpProfile->FrontlineTotalSecondPlace,
                        ThirdPlace = pvpProfile->FrontlineTotalThirdPlace
                    };
                    return true;
                }
                return false;
            }
        }

        public int GenerateFrontlineBonus(FrontlinePlacement FrontlineResult, int EarnedSeriesEXP)
        {
            // Calculates the current Frontline Bonus
            // 1000 (no bonus), 1100, 1200, 1300, 1400, 1500 3rd
            // 1250 (no bonus), 1375, 1500, 1625, 1750, 1875 2nd
            // 1500 (no bonus), 1650, 1800, 1950, 2100, 2250 1st
            if (FrontlineResult == FrontlinePlacement.ThirdPlace)
            {
                switch (EarnedSeriesEXP)
                {
                    case 1000:
                        CurrentFrontlineLosingBonus = 0;
                        return 0;
                    case 1100:
                        CurrentFrontlineLosingBonus = 10;
                        return 10;
                    case 1200:
                        CurrentFrontlineLosingBonus = 20;
                        return 20;
                    case 1300:
                        CurrentFrontlineLosingBonus = 30;
                        return 30;
                    case 1400:
                        CurrentFrontlineLosingBonus = 40;
                        return 40;
                    case 1500:
                        CurrentFrontlineLosingBonus = 50;
                        return 50;
                    default:
                        return -1;
                }
            }
            else if (FrontlineResult == FrontlinePlacement.SecondPlace)
            {
                switch (EarnedSeriesEXP)
                {
                    case 1250:
                        CurrentFrontlineLosingBonus = 0;
                        return 0;
                    case 1375:
                        CurrentFrontlineLosingBonus = 10;
                        return 10;
                    case 1500:
                        CurrentFrontlineLosingBonus = 20;
                        return 20;
                    case 1625:
                        CurrentFrontlineLosingBonus = 30;
                        return 30;
                    case 2100:
                        CurrentFrontlineLosingBonus = 40;
                        return 40;
                    case 2250:
                        CurrentFrontlineLosingBonus = 50;
                        return 50;
                    default:
                        return -1;
                }
            }
            else if (FrontlineResult == FrontlinePlacement.FirstPlace)
            {
                switch (EarnedSeriesEXP)
                {
                    case 1500:
                        CurrentFrontlineLosingBonus = 0;
                        return 0;
                    case 1650:
                        CurrentFrontlineLosingBonus = 10;
                        return 10;
                    case 1800:
                        CurrentFrontlineLosingBonus = 20;
                        return 20;
                    case 1950:
                        CurrentFrontlineLosingBonus = 30;
                        return 30;
                    case 1750:
                        CurrentFrontlineLosingBonus = 40;
                        return 40;
                    case 1875:
                        CurrentFrontlineLosingBonus = 50;
                        return 50;
                    default:
                        return -1;
                }
            }
            return -1;
        }

        public PVPProfileFrontlineResults GetPVPProfileFrontlineResults()
        {
            unsafe
            {
                var pvpProfile = PvPProfile.Instance();
                if (pvpProfile != null && pvpProfile->IsLoaded != 0)
                {
                    return new PVPProfileFrontlineResults
                    {
                        FirstPlace = pvpProfile->FrontlineTotalFirstPlace,
                        SecondPlace = pvpProfile->FrontlineTotalSecondPlace,
                        ThirdPlace = pvpProfile->FrontlineTotalThirdPlace,

                    };
                }
            }
            return new PVPProfileFrontlineResults
            {
                FirstPlace = 0,
                SecondPlace = 0,
                ThirdPlace = 0,

            };
        }

    }

    public class PvPSeriesInfo
    {
        public byte CurrentSeriesRank { get; set; }
        public byte ClaimedSeriesRank { get; set; }
        public ushort SeriesExperience { get; set; }
    }
}
