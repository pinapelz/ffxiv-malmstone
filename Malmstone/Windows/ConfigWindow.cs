using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Malmstone.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin Plugin;
    private string[] ToastOptions = {"Normal", "Quest", "Error"};

    public ConfigWindow(Plugin Plugin) : base("Malmstone Config")
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(540, 390),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Configuration = Plugin.Configuration;
        this.Plugin = Plugin;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("Settings");
        
        // PVP Match Tab
        if (ImGui.BeginTabItem("PVP Match"))
        {
            ImGui.Text("Default Target Series Level");
            var savedTargetSeriesRank = Configuration.DefaultTargetRankProperty;
            if (ImGui.InputInt("##SavedTargetSeriesRank", ref savedTargetSeriesRank, 1))
            {
                if (savedTargetSeriesRank < 1) savedTargetSeriesRank = 1;
                if (savedTargetSeriesRank > 107397) savedTargetSeriesRank = 107397;
                Configuration.DefaultTargetRankProperty = savedTargetSeriesRank;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Calculator will auto-populate with this number after initializing" +
                           "\nAlso controls the notification override settings below");
                ImGui.EndTooltip();
            }

            var skipProgressionToastAfterGoal = Configuration.SkipProgressionToastAfterGoal;
            if (ImGui.Checkbox("###SkipProgressionToastAfterGoal", ref skipProgressionToastAfterGoal))
            {
                Configuration.SkipProgressionToastAfterGoal = skipProgressionToastAfterGoal;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
                    "Automatically stops showing EXP progression notification after reaching the Default Target Series Level" +
                    "\nOverrides other EXP notification settings");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("Skip EXP progression notifications after default level is achieved");



            var skipProgressionChatAfterGoal = Configuration.SkipProgressionChatAfterGoal;
            if (ImGui.Checkbox("###SkipProgressionChatAfterGoal", ref skipProgressionChatAfterGoal))
            {
                Configuration.SkipProgressionChatAfterGoal = skipProgressionChatAfterGoal;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
                    "Automatically stops showing matches remaining chat messages after reaching the the Default Target Series Level" +
                    "\nOverrides other post-match chat notification settings");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("Skip remaining matches chat notifications after default level is achieved");


            ImGui.Separator();

            var showProgressionToastPostMatch = Configuration.ShowProgressionToastPostMatch;
            if (ImGui.Checkbox("##ShowProgressionToastPostMatch", ref showProgressionToastPostMatch))
            {
                Configuration.ShowProgressionToastPostMatch = showProgressionToastPostMatch;
                if (showProgressionToastPostMatch)
                    Plugin.PvPAddon.EnablePostMatchProgressionToast();
                else
                    Plugin.PvPAddon.DisablePostMatchProgressionToast();
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Shows a notification with current series level EXP progression after ALL PVP matches");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("Show EXP progression after PVP matches");

            ImGui.Text("Notification Type");
            int selectedPostMatchToastType = Configuration.PostmatchProgressionToastType;
            if (ImGui.Combo("##MatchOptions", ref selectedPostMatchToastType, ToastOptions, ToastOptions.Length))
            {
                switch (selectedPostMatchToastType)
                {
                    case 0:
                        Plugin.ToastGui.ShowNormal("[Malmstone Calculator] Normal Toast Selected");
                        break;
                    case 1:
                        Plugin.ToastGui.ShowQuest("[Malmstone Calculator] Quest Toast Selected");
                        break;
                    case 2:
                        Plugin.ToastGui.ShowError("[Malmstone Calculator] Error Toast Selected");
                        break;
                }

                Configuration.PostmatchProgressionToastType = selectedPostMatchToastType;
                Configuration.Save();
            }

            ImGui.Separator();
            ImGui.Text("Show matches until next level in chat post-game");


            var showCCMatchesRemainingPostGame = Configuration.ShowProgressionChatPostCC;
            if (ImGui.Checkbox("##ShowCCMatchesRemainingPostGame", ref showCCMatchesRemainingPostGame))
            {
                Configuration.ShowProgressionChatPostCC = showCCMatchesRemainingPostGame;
                if (showCCMatchesRemainingPostGame)
                    Plugin.PvPAddon.EnableCrystallineConflictPostMatch();
                else
                    Plugin.PvPAddon.DisableCrystallineConflictPostMatch();
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
                    "Show Wins/Losses needed until next Series Level in chat after Crystalline Conflict matches");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("Crystalline Conflict");


            var showFLMatchesRemainingPostGame = Configuration.ShowProgressionChatPostFL;
            if (ImGui.Checkbox("##ShowFLMatchesRemainingPostGame", ref showFLMatchesRemainingPostGame))
            {
                Configuration.ShowProgressionChatPostFL = showFLMatchesRemainingPostGame;
                if (showFLMatchesRemainingPostGame && !Plugin.PvPAddon.FrontlineRecordPostSetupEnabled)
                    Plugin.PvPAddon.EnableFrontlinePostMatch();
                else if (!showFLMatchesRemainingPostGame && !Configuration.TrackFrontlineBonus)
                    Plugin.PvPAddon.DisableFrontlinePostMatch();
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
                    "Show placements needed until next Series Level in chat after Frontline matches\nRoulettes shown in parentheses");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("Frontlines");

            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();

            var trackFrontlineBonus = Configuration.TrackFrontlineBonus;
            if (ImGui.Checkbox("##TrackFrontlineBonus", ref trackFrontlineBonus))
            {
                Configuration.TrackFrontlineBonus = trackFrontlineBonus;
                if (trackFrontlineBonus && !Plugin.PvPAddon.FrontlineRecordPostSetupEnabled)
                    Plugin.PvPAddon.EnableFrontlinePostMatch();
                else if (!trackFrontlineBonus && !Configuration.ShowProgressionChatPostFL)
                    Plugin.PvPAddon.DisableFrontlinePostMatch();
                Configuration.OutdatedFrontlineRewardBonus = true;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("(EXPERIMENTAL) Track the reward bonus you get for consecutive losses in Frontline" +
                           "\n3rd place = +10 percent bonus (max 50 percent)" +
                           "\n2nd place = Current bonus is kept" +
                           "\n1st Place = Bonus reset to 0\n");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("Track Frontline Reward Bonus");


            var showRWMatchesRemainingPostGame = Configuration.ShowProgressionChatPostRW;
            if (ImGui.Checkbox("##ShowRWMatchesRemainingPostGame", ref showRWMatchesRemainingPostGame))
            {
                Configuration.ShowProgressionChatPostRW = showRWMatchesRemainingPostGame;
                if (showRWMatchesRemainingPostGame)
                    Plugin.PvPAddon.EnableRivalWingsPostMatch();
                else
                    Plugin.PvPAddon.DisableRivalWingsPostMatch();
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Show Wins/Losses needed until next Series Level in chat after Rival Wings matches");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("Rival Wings");

            var OverrideShowMatchesToDefaultTargetGoal = Configuration.OverrideShowMatchesToDefaultTargetGoal;
            if (ImGui.Checkbox("##OverrideShowMatchesToDefaultTargetGoal", ref OverrideShowMatchesToDefaultTargetGoal))
            {
                Configuration.OverrideShowMatchesToDefaultTargetGoal = OverrideShowMatchesToDefaultTargetGoal;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
                    "Show remaining matches to the Default Target rank instead of the next rank for postmatch chat notifications" +
                    "\nThis only works if the Default Target rank is higher than your current rank, otherwise this setting will be ignored");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("Show matches until Default Target Rank instead of next rank");


            ImGui.EndTabItem();
        }


        // User Interface Tab
        if (ImGui.BeginTabItem("User Interface"))
        {
            var showMainWindowOnPVPReward = Configuration.ShowMainWindowOnPVPReward;
            if (ImGui.Checkbox("##ShowMainWindowOnPVPReward", ref showMainWindowOnPVPReward))
            {
                if (showMainWindowOnPVPReward)
                    Plugin.EnablePVPRewardWindowAddon();
                else
                    Plugin.DisablePVPRewardWindowAddon();
                Configuration.ShowMainWindowOnPVPReward = showMainWindowOnPVPReward;
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Automatically open the calculator window when viewing Series Malmstone rewards");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("Show calculations when viewing Series Malmstones");
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
        ImGui.Separator();
        ImGui.Text("Changes saved automatically");

    }
}
