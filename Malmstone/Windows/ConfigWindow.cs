using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Malmstone.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin Plugin;

    public ConfigWindow(Plugin Plugin) : base("Malmstone Config")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;
        Size = new Vector2(350, 400);
        Configuration = Plugin.Configuration;
        this.Plugin = Plugin;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
    }

    public override void Draw()
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

        ImGui.Separator();

        ImGui.Text("Show XP to next level after PVP matches");
        var showProgressionToastPostMatch = Configuration.ShowProgressionToastPostMatch;
        if (ImGui.Checkbox("##ShowProgressionToastPostMatch", ref showProgressionToastPostMatch))
        {
            Configuration.ShowProgressionToastPostMatch = showProgressionToastPostMatch;
            Configuration.Save();
        }

        ImGui.Separator();
        ImGui.Text("Show matches to next rank in chat postmatch");


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
        ImGui.SameLine();
        ImGui.Text("Crystalline Conflict");


        var showFLMatchesRemainingPostGame = Configuration.ShowProgressionChatPostFL;
        if (ImGui.Checkbox("##ShowFLMatchesRemainingPostGame", ref showFLMatchesRemainingPostGame))
        {
            Configuration.ShowProgressionChatPostFL = showFLMatchesRemainingPostGame;
            if (showFLMatchesRemainingPostGame)
                Plugin.PvPAddon.EnableFrontlinePostMatch();
            else
                Plugin.PvPAddon.DisableFrontlinePostMatch();
            Configuration.Save();
        }
        ImGui.SameLine();
        ImGui.Text("Frontlines");


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
        ImGui.SameLine();
        ImGui.Text("Rival Wings");


        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Text("Changes saved automatically");

    }
}
