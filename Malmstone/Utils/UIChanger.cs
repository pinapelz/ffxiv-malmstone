using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Malmstone.Services;

namespace Malmstone.Utils;

public class UIChanger
{
    private Plugin Plugin;
    public UIChanger(Plugin Plugin)
    {
        this.Plugin = Plugin;
    }

    public void ReplacePVPRewardWindowSeriesRank(AddonEvent eventType, AddonArgs addonInfo)
    {
        unsafe
        {
            var PvpRewardWindow = (AtkUnitBase*)addonInfo.Addon;
            var SeriesLevelTextNode = PvpRewardWindow->GetTextNodeById(16);
            PvPSeriesInfo? PvPSeriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (SeriesLevelTextNode != null && PvPSeriesInfo != null)
            {
                var CurrentSeriesRank = PvPSeriesInfo.CurrentSeriesRank +
                                        Plugin.GetSavedExtraLevels();
                SeriesLevelTextNode->SetText(CurrentSeriesRank.ToString());
            }
        }
    }

    public void ReplacePVPProfileWindowSeriesRank(AddonEvent eventType, AddonArgs addonInfo)
    {
        unsafe
        {
            var PvpProfileWindow = (AtkUnitBase*)addonInfo.Addon;
            var SeriesLevelTextNode = PvpProfileWindow->GetTextNodeById(24);
            PvPSeriesInfo? PvPSeriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (PvPSeriesInfo == null)
                return;
            var CurrentSeriesRank = PvPSeriesInfo.CurrentSeriesRank +
                                    Plugin.GetSavedExtraLevels();
            if (SeriesLevelTextNode != null)
            {
                SeriesLevelTextNode->SetText(CurrentSeriesRank.ToString());
            }
        }
    }
}
