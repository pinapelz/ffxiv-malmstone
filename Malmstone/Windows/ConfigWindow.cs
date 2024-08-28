using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Malmstone.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base("Malmstone Config")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 150);
        Configuration = plugin.Configuration;
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
            if (savedTargetSeriesRank < 107397) savedTargetSeriesRank = 107397;
            Configuration.DefaultTargetRankProperty = savedTargetSeriesRank;
        }

        ImGui.Spacing();

        if (ImGui.Button("Save and Close"))
        {
            Configuration.Save();
            IsOpen = false;
        }
    }
}
