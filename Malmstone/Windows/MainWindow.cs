using System;
using System.Numerics;
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
        private Malmstone.Utils.MalmstoneXPCalculator.XpCalculationResult _cachedXpResult;

        public MainWindow(Plugin plugin)
            : base("Malmstone")
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(440, 480),
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
                ImGui.Text($"Current Level Experience Gained: {pvpInfo.SeriesExperience}");
                if (pvpInfo.CurrentSeriesRank != pvpInfo.ClaimedSeriesRank)
                {
                    ImGui.Text("Don't forget to claim your rank rewards!");
                }
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
                if (xpResult.ActivityCounts.ContainsKey("Crystalline Conflict Win"))
                {
                    var winCount = xpResult.ActivityCounts["Crystalline Conflict Win"];
                    ImGui.BulletText($"Win: {winCount} " + (winCount == 1 ? "time" : "times"));
                }
                if (xpResult.ActivityCounts.ContainsKey("Crystalline Conflict Lose"))
                {
                    var loseCount = xpResult.ActivityCounts["Crystalline Conflict Lose"];
                    ImGui.BulletText($"Lose: {loseCount} " + (loseCount == 1 ? "time" : "times"));
                }

                ImGui.Spacing();
                ImGui.Separator();

                // Frontlines Section
                ImGui.TextColored(new Vector4(0.8f, 0.6f, 0.6f, 1f), "Frontlines");
                ImGui.Spacing();
                if (xpResult.ActivityCounts.ContainsKey("Frontline Win"))
                {
                    var frontlineWinCount = xpResult.ActivityCounts["Frontline Win"];
                    ImGui.BulletText($"Take 1st Place: {frontlineWinCount} " + (frontlineWinCount == 1 ? "time" : "times"));
                }
                if (xpResult.ActivityCounts.ContainsKey("Frontline Lose 2nd"))
                {
                    var frontlineLose2ndCount = xpResult.ActivityCounts["Frontline Lose 2nd"];
                    ImGui.BulletText($"Take 2nd Place: {frontlineLose2ndCount} " + (frontlineLose2ndCount == 1 ? "time" : "times"));
                }
                if (xpResult.ActivityCounts.ContainsKey("Frontline Lose 3rd"))
                {
                    var frontlineLose3rdCount = xpResult.ActivityCounts["Frontline Lose 3rd"];
                    ImGui.BulletText($"Take 3rd Place: {frontlineLose3rdCount} " + (frontlineLose3rdCount == 1 ? "time" : "times"));
                }

                // Frontlines Roulette Section
                ImGui.TextColored(new Vector4(0.8f, 0.6f, 0.6f, 1f), "Frontlines (Roulette)");
                ImGui.Spacing();
                if (xpResult.ActivityCounts.ContainsKey("Frontline Daily Win"))
                {
                    var frontlineDailyWinCount = xpResult.ActivityCounts["Frontline Daily Win"];
                    ImGui.BulletText($"Take 1st Place: {frontlineDailyWinCount} " + (frontlineDailyWinCount == 1 ? "time" : "times"));
                }
                if (xpResult.ActivityCounts.ContainsKey("Frontline Daily Lose 2nd"))
                {
                    var frontlineDailyLose2ndCount = xpResult.ActivityCounts["Frontline Daily Lose 2nd"];
                    ImGui.BulletText($"Take 2nd Place: {frontlineDailyLose2ndCount} " + (frontlineDailyLose2ndCount == 1 ? "time" : "times"));
                }
                if (xpResult.ActivityCounts.ContainsKey("Frontline Daily Lose 3rd"))
                {
                    var frontlineDailyLose3rdCount = xpResult.ActivityCounts["Frontline Daily Lose 3rd"];
                    ImGui.BulletText($"Take 3rd Place: {frontlineDailyLose3rdCount} " + (frontlineDailyLose3rdCount == 1 ? "time" : "times"));
                }

                ImGui.Spacing();
                ImGui.Separator();

                // Rival Wings Section
                ImGui.TextColored(new Vector4(0.6f, 0.8f, 0.6f, 1f), "Rival Wings");
                ImGui.Spacing();
                if (xpResult.ActivityCounts.ContainsKey("Rival Wings Win"))
                {
                    var rivalWingsWinCount = xpResult.ActivityCounts["Rival Wings Win"];
                    ImGui.BulletText($"Win: {rivalWingsWinCount} " + (rivalWingsWinCount == 1 ? "time" : "times"));
                }
                if (xpResult.ActivityCounts.ContainsKey("Rival Wings Lose"))
                {
                    var rivalWingsLoseCount = xpResult.ActivityCounts["Rival Wings Lose"];
                    ImGui.BulletText($"Lose: {rivalWingsLoseCount} " + (rivalWingsLoseCount == 1 ? "time" : "times"));
                }

            }
            else
            {
                ImGui.Text("PvP Profile is not loaded.");
            }
        }
    }
}
