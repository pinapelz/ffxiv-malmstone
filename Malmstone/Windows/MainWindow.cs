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
                    ImGui.BulletText($"Win: {xpResult.ActivityCounts["Crystalline Conflict Win"]} times");
                }
                if (xpResult.ActivityCounts.ContainsKey("Crystalline Conflict Lose"))
                {
                    ImGui.BulletText($"Lose: {xpResult.ActivityCounts["Crystalline Conflict Lose"]} times");
                }

                ImGui.Spacing();
                ImGui.Separator();

                // Frontlines Section
                ImGui.TextColored(new Vector4(0.8f, 0.6f, 0.6f, 1f), "Frontlines");
                ImGui.Spacing();
                if (xpResult.ActivityCounts.ContainsKey("Frontline Win"))
                {
                    ImGui.BulletText($"Take 1st Place: {xpResult.ActivityCounts["Frontline Win"]} times");
                }
                if (xpResult.ActivityCounts.ContainsKey("Frontline Lose 2nd"))
                {
                    ImGui.BulletText($"Take 2nd Place: {xpResult.ActivityCounts["Frontline Lose 2nd"]} times");
                }
                if (xpResult.ActivityCounts.ContainsKey("Frontline Lose 3rd"))
                {
                    ImGui.BulletText($"Take 3rd Place: {xpResult.ActivityCounts["Frontline Lose 3rd"]} times");
                }

                // Frontlines Roulette Section
                ImGui.TextColored(new Vector4(0.8f, 0.6f, 0.6f, 1f), "Frontlines (Roulette)");
                ImGui.Spacing();
                if (xpResult.ActivityCounts.ContainsKey("Frontline Daily Win"))
                {
                    ImGui.BulletText($"Take 1st Place: {xpResult.ActivityCounts["Frontline Daily Win"]} times");
                }
                if (xpResult.ActivityCounts.ContainsKey("Frontline Daily Lose 2nd"))
                {
                    ImGui.BulletText($"Take 2nd Place: {xpResult.ActivityCounts["Frontline Daily Lose 2nd"]} times");
                }
                if (xpResult.ActivityCounts.ContainsKey("Frontline Daily Lose 3rd"))
                {
                    ImGui.BulletText($"Take 3rd Place: {xpResult.ActivityCounts["Frontline Daily Lose 3rd"]} times");
                }

                ImGui.Spacing();
                ImGui.Separator();

                // Rival Wings Section
                ImGui.TextColored(new Vector4(0.6f, 0.8f, 0.6f, 1f), "Rival Wings");
                ImGui.Spacing();
                if (xpResult.ActivityCounts.ContainsKey("Rival Wings Win"))
                {
                    ImGui.BulletText($"Win: {xpResult.ActivityCounts["Rival Wings Win"]} times");
                }
                if (xpResult.ActivityCounts.ContainsKey("Rival Wings Lose"))
                {
                    ImGui.BulletText($"Lose: {xpResult.ActivityCounts["Rival Wings Lose"]} times");
                }
            }
            else
            {
                ImGui.Text("PvP Profile is not loaded.");
            }
        }
    }
}
