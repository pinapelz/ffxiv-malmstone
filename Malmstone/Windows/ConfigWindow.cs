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
            MinimumSize = new Vector2(350, 300),
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

        ImGui.Text("Show EXP progression after PVP matches");
        ImGui.SameLine();
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
        ImGui.Text("Show matches until next rank in chat after");


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
