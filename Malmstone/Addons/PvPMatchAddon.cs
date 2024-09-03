using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Malmstone.Services;
using Malmstone.Utils;
using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace Malmstone.Addons
{
    internal class PvPMatchAddon
    {
        private Plugin Plugin;
        private enum PvPContentType
        {
            CrystallineConflict = 1,
            RivalWings = 2,
            Frontlines = 3,
        }
        public PvPMatchAddon(Plugin Plugin)
        {
            this.Plugin = Plugin;
        }

        public void EnableCrystallineConflictPostMatch()
        {
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PostSetup, "MKSRecord", OnCrystallineConflictRecordTrigger);
        }

        public void DisableCrystallineConflictPostMatch()
        {
            Plugin.AddonLifeCycle.UnregisterListener(OnCrystallineConflictRecordTrigger);
        }

        public void EnableRivalWingsPostMatch()
        {
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PostSetup, "ManeuversRecord", OnRivalWingsRecordTrigger);
        }
        public void EnableFrontlinePostMatch()
        {
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PostSetup, "FrontlineRecord", OnRivalWingsRecordTrigger);
        }

        public void DisableFrontlinePostMatch()
        {
            Plugin.AddonLifeCycle.UnregisterListener(AddonEvent.PostSetup, "FrontlineRecord", OnRivalWingsRecordTrigger);
        }

        public void DisableRivalWingsPostMatch()
        {
            Plugin.AddonLifeCycle.UnregisterListener(OnRivalWingsRecordTrigger);
        }


        // Runs on the result screen of the respective game mode
        private void OnCrystallineConflictRecordTrigger(AddonEvent eventType, AddonArgs addonInfo)
        {
            Plugin.Chat.Print("Triggered MKS Record");
            PvPSeriesInfo? seriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (seriesInfo == null) return;
            if (Plugin.Configuration.ShowProgressionToastPostMatch)
                ShowSeriesProgressionToast(seriesInfo);
            if (Plugin.Configuration.ShowProgressionChatPostCC)
                ShowSeriesProgressionMessage(seriesInfo, PvPContentType.CrystallineConflict);
        }

        private void OnFrontlineRecordTrigger(AddonEvent eventType, AddonArgs addonInfo)
        {
            Plugin.Chat.Print("Triggered Frontline Record");
            PvPSeriesInfo? seriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (seriesInfo == null) return;
            if (Plugin.Configuration.ShowProgressionToastPostMatch)
                ShowSeriesProgressionToast(seriesInfo);
            if (Plugin.Configuration.ShowProgressionChatPostFL)
                ShowSeriesProgressionMessage(seriesInfo, PvPContentType.RivalWings);
        }

        private void OnRivalWingsRecordTrigger(AddonEvent eventType, AddonArgs addonInfo)
        {
            Plugin.Chat.Print("Triggered Maneuvers Record");
            PvPSeriesInfo? seriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (seriesInfo == null) return;
            if (Plugin.Configuration.ShowProgressionToastPostMatch)
                ShowSeriesProgressionToast(seriesInfo);
            if (Plugin.Configuration.ShowProgressionChatPostRW)
                ShowSeriesProgressionMessage(seriesInfo, PvPContentType.RivalWings);
        }


        private void ShowSeriesProgressionToast(PvPSeriesInfo seriesInfo)
        {
            switch (Plugin.Configuration.PostmatchProgressionToastType)
            {
                case 0:
                    Plugin.ToastGui.ShowNormal("Series Level " + seriesInfo.CurrentSeriesRank +
                        "     " + seriesInfo.SeriesExperience + "/" + MalmstoneXPCalculator.GetXPTargetForCurrentLevel(seriesInfo.CurrentSeriesRank) + " EXP");
                    break;
                case 1:
                    Plugin.ToastGui.ShowQuest("Series Level " + seriesInfo.CurrentSeriesRank +
                        "     " + seriesInfo.SeriesExperience + "/" + MalmstoneXPCalculator.GetXPTargetForCurrentLevel(seriesInfo.CurrentSeriesRank) + " EXP");
                    break;
                case 2:
                    Plugin.ToastGui.ShowError("Series Level " + seriesInfo.CurrentSeriesRank +
                        "     " + seriesInfo.SeriesExperience + "/" + MalmstoneXPCalculator.GetXPTargetForCurrentLevel(seriesInfo.CurrentSeriesRank) + " EXP");
                    break;
                default:
                    Plugin.ToastGui.ShowNormal("Series Level " + seriesInfo.CurrentSeriesRank +
                        "     " + seriesInfo.SeriesExperience + "/" + MalmstoneXPCalculator.GetXPTargetForCurrentLevel(seriesInfo.CurrentSeriesRank) + " EXP");
                    break;
            }

        }

        private void ShowSeriesProgressionMessage(PvPSeriesInfo seriesInfo, PvPContentType contentType)
        {
            var seString = new SeString(new List<Payload>());
            switch (contentType)
            {
                case PvPContentType.CrystallineConflict:
                    MalmstoneXPCalculator.XpCalculationResult ccResultData = MalmstoneXPCalculator.CalculateCrystallineConflictMatches(
                        seriesInfo.CurrentSeriesRank, seriesInfo.CurrentSeriesRank + 1, seriesInfo.SeriesExperience);
                    if (ccResultData.CrystallineConflictLose == 0) break;
                    seString.Append(new TextPayload("[Crystalline Conflict to Level " + (seriesInfo.CurrentSeriesRank + 1) + "]\n"));
                    seString.Append(new UIForegroundPayload(35));
                    seString.Append(new TextPayload($"Win: {ccResultData.CrystallineConflictWin} " + (ccResultData.CrystallineConflictWin == 1 ? "time" : "times") + "\n"));
                    seString.Append(new TextPayload($"Lose: {ccResultData.CrystallineConflictLose} " + (ccResultData.CrystallineConflictLose == 1 ? "time" : "times")));
                    seString.Append(UIForegroundPayload.UIForegroundOff);
                    break;
                case PvPContentType.Frontlines:
                    MalmstoneXPCalculator.XpCalculationResult flResultData = MalmstoneXPCalculator.CalculateCrystallineConflictMatches(
                        seriesInfo.CurrentSeriesRank, seriesInfo.CurrentSeriesRank + 1, seriesInfo.SeriesExperience);
                    if (flResultData.FrontlineDailyLose3rd == 0) break;
                    seString.Append(new TextPayload("[Frontlines to Level " + (seriesInfo.CurrentSeriesRank + 1) + "]\n"));
                    seString.Append(new UIForegroundPayload(518));
                    seString.Append(new TextPayload($"Take 1st Place: {flResultData.FrontlineWin} " + (flResultData.FrontlineWin == 1 ? "time" : "times") +" (" + (flResultData.FrontlineDailyWin) + ")\n"));
                    seString.Append(new TextPayload($"Take 2nd Place: {flResultData.FrontlineWin} " + (flResultData.FrontlineWin == 1 ? "time" : "times") + " (" + (flResultData.FrontlineDailyLose2nd) + ")\n"));
                    seString.Append(new TextPayload($"Take 3rd Place: {flResultData.FrontlineWin} " + (flResultData.FrontlineWin == 1 ? "time" : "times") + " (" + (flResultData.FrontlineDailyLose3rd) + ")\n"));
                    seString.Append(UIForegroundPayload.UIForegroundOff);
                    break;
                case PvPContentType.RivalWings:
                    MalmstoneXPCalculator.XpCalculationResult rwResultData = MalmstoneXPCalculator.CalculateRivalWingsMatches(
                        seriesInfo.CurrentSeriesRank, seriesInfo.CurrentSeriesRank + 1, seriesInfo.SeriesExperience);
                    if (rwResultData.RivalWingsLose == 0) break;
                    seString.Append(new TextPayload("[Rival Wings to Level " + (seriesInfo.CurrentSeriesRank + 1) + "]\n"));
                    seString.Append(new UIForegroundPayload(43));
                    seString.Append(new TextPayload($"Win: {rwResultData.RivalWingsWin} " + (rwResultData.RivalWingsWin == 1 ? "time" : "times") + "\n"));
                    seString.Append(new TextPayload($"Lose: {rwResultData.RivalWingsLose} " + (rwResultData.RivalWingsLose == 1 ? "time" : "times")));
                    seString.Append(UIForegroundPayload.UIForegroundOff);
                    break;
            }
            Plugin.Chat.Print(seString);
        }

        
    }
}
