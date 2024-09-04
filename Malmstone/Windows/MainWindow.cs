using System;
using System.Numerics;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Malmstone.Services;
using Malmstone.Utils;

namespace Malmstone.Windows
{
    public class MainWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private PvPService PvPService;
        private int TargetSeriesRank;

        // Cache-related fields
        private int _lastSeriesRank;
        private int _lastTargetSeriesRank;
        private int _lastSeriesExperience;
        private MalmstoneXPCalculator.XpCalculationResult _cachedXpResult;

        public MainWindow(Plugin plugin)
            : base("Malmstone")
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(460, 510),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            Plugin = plugin;
            PvPService = new PvPService();
            TargetSeriesRank = Plugin.Configuration.DefaultTargetRankProperty;
        }

        public void Dispose() { }

        public override void Draw()
        {
            if (!IsOpen) return;
            var pvpInfo = PvPService.GetPvPSeriesInfo();
            if (pvpInfo != null)
            {
                ImGui.Text($"Current Series Level: {pvpInfo.CurrentSeriesRank}");
                ImGui.Text($"Current Level Experience Progress: {pvpInfo.SeriesExperience} XP");
                ImGui.Spacing();

                ImGui.Text("Target Series Level:");
                ImGui.InputInt("##TargetSeriesRank", ref TargetSeriesRank, 1);

                // Bounds checking to ensure no overflows
                if (TargetSeriesRank < 1) TargetSeriesRank = 1;
                if (TargetSeriesRank > 107397) TargetSeriesRank = 107397;

                if (TargetSeriesRank <= pvpInfo.CurrentSeriesRank) TargetSeriesRank = pvpInfo.CurrentSeriesRank + 1;

                ImGui.Spacing();
                ImGui.Separator();

                // Only recalculate if the relevant data has changed
                if (pvpInfo.CurrentSeriesRank != _lastSeriesRank ||
                    TargetSeriesRank != _lastTargetSeriesRank ||
                    pvpInfo.SeriesExperience != _lastSeriesExperience)
                {
                    _cachedXpResult = MalmstoneXPCalculator.CalculateXp(pvpInfo.CurrentSeriesRank, TargetSeriesRank, pvpInfo.SeriesExperience);
                    _lastSeriesRank = pvpInfo.CurrentSeriesRank;
                    _lastTargetSeriesRank = TargetSeriesRank;
                    _lastSeriesExperience = pvpInfo.SeriesExperience;
                }

                var xpResult = _cachedXpResult;

                ImGui.Spacing();
                ImGui.Text($"You have {xpResult.RemainingXp} remaining series EXP to go until you reach level {xpResult.TargetLevel}");

                ImGui.Spacing();
                ImGui.Separator();

                // Crystalline Conflict Section
                ImGui.TextColored(new Vector4(0.6f, 0.8f, 1f, 1f), "Crystalline Conflict");
                ImGui.Spacing();
                ImGui.BulletText($"Win: {xpResult.CrystallineConflictWin} " + (xpResult.CrystallineConflictWin == 1 ? "time" : "times"));
                ImGui.BulletText($"Lose: {xpResult.CrystallineConflictLose} " + (xpResult.CrystallineConflictLose == 1 ? "time" : "times"));

                ImGui.Spacing();
                ImGui.Separator();

                // Frontlines Section
                ImGui.TextColored(new Vector4(0.8f, 0.6f, 0.6f, 1f), "Frontlines");
                ImGui.Spacing();
                ImGui.BulletText($"Take 1st Place: {xpResult.FrontlineWin} " + (xpResult.FrontlineWin == 1 ? "time" : "times"));
                ImGui.BulletText($"Take 2nd Place: {xpResult.FrontlineLose2nd} " + (xpResult.FrontlineLose2nd == 1 ? "time" : "times"));
                ImGui.BulletText($"Take 3rd Place: {xpResult.FrontlineLose3rd} " + (xpResult.FrontlineLose3rd == 1 ? "time" : "times"));

                // Frontlines Roulette Section
                ImGui.TextColored(new Vector4(0.8f, 0.6f, 0.6f, 1f), "Frontlines (Roulette)");
                ImGui.Spacing();
                ImGui.BulletText($"Take 1st Place: {xpResult.FrontlineDailyWin} " + (xpResult.FrontlineDailyWin == 1 ? "time" : "times"));
                ImGui.BulletText($"Take 2nd Place: {xpResult.FrontlineDailyLose2nd} " + (xpResult.FrontlineDailyLose2nd == 1 ? "time" : "times"));
                ImGui.BulletText($"Take 3rd Place: {xpResult.FrontlineDailyLose3rd} " + (xpResult.FrontlineDailyLose3rd == 1 ? "time" : "times"));

                ImGui.Spacing();
                ImGui.Separator();

                // Rival Wings Section
                ImGui.TextColored(new Vector4(0.6f, 0.8f, 0.6f, 1f), "Rival Wings");
                ImGui.Spacing();
                ImGui.BulletText($"Win: {xpResult.RivalWingsWin} " + (xpResult.RivalWingsWin == 1 ? "time" : "times"));
                ImGui.BulletText($"Lose: {xpResult.RivalWingsLose} " + (xpResult.RivalWingsLose == 1 ? "time" : "times"));

                ImGui.Separator();
                ImGui.Spacing();
                if (ImGui.Button("Settings"))
                    Plugin.ToggleConfigUI();
                ImGui.SameLine();
                if (pvpInfo.CurrentSeriesRank != pvpInfo.ClaimedSeriesRank)
                {
                    ImGui.Text("Don't forget to claim your Series Malmstone rewards!");
                }

            }
            else
            {
                ImGui.Text("PvP Profile is not loaded.");
            }
        }
        public void OnOpenPVPRewardWindow(AddonEvent eventType, AddonArgs addonInfo)
        {
            IsOpen = true;
        }

        public void OnClosePVPRewardWindow(AddonEvent eventType, AddonArgs addonInfo)
        {
            IsOpen = false;
        }

    }
}
