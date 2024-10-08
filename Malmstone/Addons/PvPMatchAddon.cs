using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Malmstone.Services;
using Malmstone.Utils;
using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Memory;
using static Malmstone.Services.PvPService;

namespace Malmstone.Addons
{
    internal class PvPMatchAddon
    {
        private Plugin Plugin;
        public bool FrontlineRecordPostSetupEnabled = false;
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

        public void EnablePostMatchProgressionToast()
        {
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PreSetup, "MKSRecord", ShowSeriesProgressionToast);
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PreSetup, "FrontlineRecord", ShowSeriesProgressionToast);
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PreSetup, "ManeuversRecord", ShowSeriesProgressionToast);
        }

        public void DisablePostMatchProgressionToast()
        {
            Plugin.AddonLifeCycle.UnregisterListener(AddonEvent.PreSetup, "MKSRecord", ShowSeriesProgressionToast);
            Plugin.AddonLifeCycle.UnregisterListener(AddonEvent.PreSetup, "FrontlineRecord", ShowSeriesProgressionToast);
            Plugin.AddonLifeCycle.UnregisterListener(AddonEvent.PreSetup, "ManeuversRecord", ShowSeriesProgressionToast);
        }

        public void EnableCrystallineConflictPostMatch()
        {
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PostSetup, "MKSRecord", OnCrystallineConflictRecordTrigger);
        }

        public void DisableCrystallineConflictPostMatch()
        {
            Plugin.AddonLifeCycle.UnregisterListener(AddonEvent.PostSetup, "MKSRecord", OnCrystallineConflictRecordTrigger);
        }

        public void EnableFrontlinePostMatch()
        {
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PostSetup, "FrontlineRecord", OnFrontlineRecordTrigger);
            Plugin.PvPService.CurrentFrontlineLosingBonus = Plugin.Configuration.SavedFrontlineRewardBonus;
            Plugin.Configuration.OutdatedFrontlineRewardBonus = true;
            Plugin.Configuration.Save();
            FrontlineRecordPostSetupEnabled = true;
        }

        public void DisableFrontlinePostMatch()
        {
            Plugin.AddonLifeCycle.UnregisterListener(AddonEvent.PostSetup, "FrontlineRecord", OnFrontlineRecordTrigger);
            FrontlineRecordPostSetupEnabled = false;
        }

        public void EnableRivalWingsPostMatch()
        {
            Plugin.AddonLifeCycle.RegisterListener(AddonEvent.PostSetup, "ManeuversRecord", OnRivalWingsRecordTrigger);
        }
        public void DisableRivalWingsPostMatch()
        {
            Plugin.AddonLifeCycle.UnregisterListener(AddonEvent.PostSetup, "ManeuversRecord", OnRivalWingsRecordTrigger);
        }
        

        // Runs on the result screen of the respective game mode
        private void OnCrystallineConflictRecordTrigger(AddonEvent eventType, AddonArgs addonInfo)
        {
            PvPSeriesInfo? seriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (seriesInfo == null) return;
            if (Plugin.Configuration.ShowProgressionChatPostCC)
                ShowSeriesProgressionMessage(seriesInfo, PvPContentType.CrystallineConflict);
        }

        private void OnFrontlineRecordTrigger(AddonEvent eventType, AddonArgs addonInfo)
        {
            if (Plugin.Configuration.TrackFrontlineBonus)
                CheckFrontlineBonus(eventType, addonInfo);
            PvPSeriesInfo? seriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (seriesInfo == null) return;
            if (Plugin.Configuration.ShowProgressionChatPostFL)
                ShowSeriesProgressionMessage(seriesInfo, PvPContentType.Frontlines);
        }

        private void OnRivalWingsRecordTrigger(AddonEvent eventType, AddonArgs addonInfo)
        {
            PvPSeriesInfo? seriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (seriesInfo == null) return;
            if (Plugin.Configuration.ShowProgressionChatPostRW)
                ShowSeriesProgressionMessage(seriesInfo, PvPContentType.RivalWings);
        }

        private void CheckFrontlineBonus(AddonEvent eventType, AddonArgs addonInfo)
        {
            PVPProfileFrontlineResults CurrentFrontlineResults = Plugin.PvPService.GetPVPProfileFrontlineResults();
            if (CurrentFrontlineResults.FirstPlace == 0 &&
                CurrentFrontlineResults.SecondPlace == 0 &&
                CurrentFrontlineResults.ThirdPlace == 0) return;
            // Check placement of current Frontline match
            FrontlinePlacement FrontlineMatchResult = FrontlinePlacement.Unknown;
            if (CurrentFrontlineResults.FirstPlace > Plugin.PvPService.CachedFrontlineResults.FirstPlace)
            {
                FrontlineMatchResult = FrontlinePlacement.FirstPlace;
            }
            else if (CurrentFrontlineResults.SecondPlace > Plugin.PvPService.CachedFrontlineResults.SecondPlace)
            {
                FrontlineMatchResult = FrontlinePlacement.SecondPlace;
            }
            else if (CurrentFrontlineResults.ThirdPlace > Plugin.PvPService.CachedFrontlineResults.ThirdPlace)
            {
                FrontlineMatchResult = FrontlinePlacement.ThirdPlace;
            }
            Plugin.Logger.Debug("Frontline Match Result: " + FrontlineMatchResult.ToString());
            if (FrontlineMatchResult != FrontlinePlacement.Unknown)
            {
                unsafe
                {
                    var FrontlineResultUnit = (AtkUnitBase*)addonInfo.Addon;
                    if (FrontlineResultUnit == null) return;
                    var SeriesExpComponent = FrontlineResultUnit->GetComponentByNodeId(35);
                    var SeriesExpTextNode = (AtkTextNode*)SeriesExpComponent->GetTextNodeById(2);
                    byte* SeriesExpTextBytePointer = SeriesExpTextNode->GetText();
                    nint SeriesExpTextAddr = (nint)SeriesExpTextBytePointer;
                    string SeriesExpText = MemoryHelper.ReadStringNullTerminated(SeriesExpTextAddr);
                    if (int.TryParse(SeriesExpText, out int SeriesExpEarned))
                    {
                        int CurrentLossBonus = Plugin.PvPService.GenerateFrontlineBonus(FrontlineMatchResult, SeriesExpEarned);
                        Plugin.Logger.Debug("Series EXP Earned: " + SeriesExpEarned.ToString());
                        Plugin.Configuration.SavedFrontlineRewardBonus = CurrentLossBonus;
                        Plugin.Configuration.OutdatedFrontlineRewardBonus = false;
                        Plugin.Configuration.Save();
                    }
                    else
                    {
                        Plugin.Chat.PrintError("[Malmstone Calculator] Unable to get earned Series EXP: " + SeriesExpText);
                    }
                }
            }
            else
            {
                Plugin.Chat.PrintError("[Malmstone Calculator] Unable to get current Frontline match results");
            }
            if(Plugin.PvPService.ConsecutiveThirdPlaceFrontline >= 1 && Plugin.Configuration.SavedFrontlineRewardBonus == 0)
            {
                Plugin.Logger.Debug("Primed For Buff! Next 3rd place will trigger losing streak bonus");
                Plugin.Configuration.IsPrimedForBuff = true;
                Plugin.Configuration.Save();
            }
            else
            {
                Plugin.Configuration.IsPrimedForBuff = false;
                Plugin.Configuration.Save();
            }
            Plugin.PvPService.UpdateFrontlineResultCache();
            Plugin.Logger.Debug("Frontline Record Re-cached As: " + Plugin.PvPService.CachedFrontlineResults.FirstPlace +
                " Second: " + Plugin.PvPService.CachedFrontlineResults.SecondPlace + " Third: " + Plugin.PvPService.CachedFrontlineResults.ThirdPlace);
        }

        private void ShowSeriesProgressionToast(AddonEvent eventType, AddonArgs addonInfo)
        {
            PvPSeriesInfo? seriesInfo = Plugin.PvPService.GetPvPSeriesInfo();
            if (seriesInfo == null) return;
            var CurrentSeriesLevel = seriesInfo.CurrentSeriesRank + Plugin.GetSavedExtraLevels();
            if (Plugin.Configuration.SkipProgressionToastAfterGoal && CurrentSeriesLevel >= Plugin.Configuration.DefaultTargetRankProperty) return;
                    
            switch (Plugin.Configuration.PostmatchProgressionToastType)
            {
                case 0:
                    Plugin.ToastGui.ShowNormal("Series Level " + CurrentSeriesLevel +
                        "     " + seriesInfo.SeriesExperience + "/" + MalmstoneXPCalculator.GetXPTargetForCurrentLevel(seriesInfo.CurrentSeriesRank) + " EXP");
                    break;
                case 1:
                    Plugin.ToastGui.ShowQuest("Series Level " + CurrentSeriesLevel +
                        "     " + seriesInfo.SeriesExperience + "/" + MalmstoneXPCalculator.GetXPTargetForCurrentLevel(seriesInfo.CurrentSeriesRank) + " EXP");
                    break;
                case 2:
                    Plugin.ToastGui.ShowError("Series Level " + CurrentSeriesLevel +
                        "     " + seriesInfo.SeriesExperience + "/" + MalmstoneXPCalculator.GetXPTargetForCurrentLevel(seriesInfo.CurrentSeriesRank) + " EXP");
                    break;
                default:
                    Plugin.ToastGui.ShowNormal("Series Level " + CurrentSeriesLevel +
                        "     " + seriesInfo.SeriesExperience + "/" + MalmstoneXPCalculator.GetXPTargetForCurrentLevel(seriesInfo.CurrentSeriesRank) + " EXP");
                    break;
            }

        }

        private void ShowSeriesProgressionMessage(PvPSeriesInfo seriesInfo, PvPContentType contentType)
        {
            if (Plugin.Configuration.SkipProgressionChatAfterGoal && seriesInfo.CurrentSeriesRank >= Plugin.Configuration.DefaultTargetRankProperty) return;
            var seString = new SeString(new List<Payload>());
            var CurrentSeriesLevel = seriesInfo.CurrentSeriesRank + Plugin.GetSavedExtraLevels();
            int TargetGoal = CurrentSeriesLevel + 1;
            if(Plugin.Configuration.OverrideShowMatchesToDefaultTargetGoal && Plugin.Configuration.DefaultTargetRankProperty > CurrentSeriesLevel) 
                TargetGoal = Plugin.Configuration.DefaultTargetRankProperty;
            switch (contentType)
            {
                case PvPContentType.CrystallineConflict:
                    MalmstoneXPCalculator.XpCalculationResult ccResultData = MalmstoneXPCalculator.CalculateCrystallineConflictMatches(
                        CurrentSeriesLevel, TargetGoal, seriesInfo.SeriesExperience);
                    if (ccResultData.CrystallineConflictLose == 0) break;
                    seString.Append(new TextPayload("[Crystalline Conflict to Level " + TargetGoal + "]\n"));
                    seString.Append(new UIForegroundPayload(35));
                    seString.Append(new TextPayload($"Win: {ccResultData.CrystallineConflictWin} " + (ccResultData.CrystallineConflictWin == 1 ? "time" : "times") + "\n"));
                    seString.Append(new TextPayload($"Lose: {ccResultData.CrystallineConflictLose} " + (ccResultData.CrystallineConflictLose == 1 ? "time" : "times")));
                    seString.Append(UIForegroundPayload.UIForegroundOff);
                    break;
                case PvPContentType.Frontlines:
                    MalmstoneXPCalculator.XpCalculationResult flResultData = MalmstoneXPCalculator.CalculateFrontlineMatches(
                        CurrentSeriesLevel, TargetGoal, seriesInfo.SeriesExperience);
                    if (flResultData.FrontlineDailyLose3rd == 0) break;
                    seString.Append(new TextPayload("[Frontlines to Level " + TargetGoal+ "]\n"));
                    seString.Append(new UIForegroundPayload(518));
                    seString.Append(new TextPayload($"Take 1st Place: {flResultData.FrontlineWin} " + (flResultData.FrontlineWin == 1 ? "time" : "times") +" (" + (flResultData.FrontlineDailyWin) + ")\n"));
                    seString.Append(new TextPayload($"Take 2nd Place: {flResultData.FrontlineWin} " + (flResultData.FrontlineWin == 1 ? "time" : "times") + " (" + (flResultData.FrontlineDailyLose2nd) + ")\n"));
                    seString.Append(new TextPayload($"Take 3rd Place: {flResultData.FrontlineWin} " + (flResultData.FrontlineWin == 1 ? "time" : "times") + " (" + (flResultData.FrontlineDailyLose3rd) + ")\n"));
                    seString.Append(new TextPayload($"Frontline Roulette Shown in Parentheses"));
                    seString.Append(UIForegroundPayload.UIForegroundOff);
                    break;
                case PvPContentType.RivalWings:
                    MalmstoneXPCalculator.XpCalculationResult rwResultData = MalmstoneXPCalculator.CalculateRivalWingsMatches(
                        CurrentSeriesLevel, TargetGoal, seriesInfo.SeriesExperience);
                    if (rwResultData.RivalWingsLose == 0) break;
                    seString.Append(new TextPayload("[Rival Wings to Level " + TargetGoal + "]\n"));
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
