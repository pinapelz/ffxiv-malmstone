using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Malmstone.Windows;
using Malmstone.Services;
using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using System.Linq;
using Malmstone.Utils;

namespace Malmstone;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui Chat { get; private set; } = null!;

    private const string CommandName = "/pmalm";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Malmstone");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private PvPService PvPService;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        PvPService = new PvPService();

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "/pmalm <rank> <all/cc/fl/rw> -- Displays PVP games left until a target rank. cc = Crystalline Conflict, fl = Frontlines, rw = Rivalwings"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

private void OnCommand(string command, string args)
{
    if (string.IsNullOrWhiteSpace(args))
    {
        ToggleMainUI();
        return;
    }

    var splitArgs = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var specs = new HashSet<string>(splitArgs.Skip(1).Select(spec => spec.ToLower()));

    var pvpInfo = PvPService.GetPvPSeriesInfo();

    if (pvpInfo == null) return;
    if (!int.TryParse(splitArgs[0], out int targetRank))
    {
        if (splitArgs[0] == "next") targetRank = pvpInfo.CurrentSeriesRank + 1;
        else if (splitArgs[0] == "config")
        {
            ToggleConfigUI();
            return;
        }
        else return;

    }
    // Show games left in chat log when there are args

    if (targetRank < 1)
    {
        Chat.PrintError("Can't have a target rank less than 1");
        return;
    }

    if (targetRank > 107397)
    {
        Chat.PrintError("Can't have a target rank greater than 107397 (are you really gonna be able to reach that anyways?)");
        return;
    }

    if (targetRank < pvpInfo.CurrentSeriesRank)
    {
        Chat.PrintError("You've already surpassed Rank " + targetRank);
        return;
    }

    var xpResult = MalmstoneXPCalculator.CalculateXp(
        pvpInfo.CurrentSeriesRank,
        targetRank,
        pvpInfo.SeriesExperience);

    bool includeAll = specs.Contains("all");
    if (!specs.Any())
    {
        includeAll = true;
    }
    var seString = new SeString(new List<Payload>());
    seString.Append(new TextPayload("\n[To Series Level " + targetRank + "]"));

    // Crystalline Conflict
    if (includeAll || specs.Contains("cc"))
    {
        seString.Append(new TextPayload("\nCrystalline Conflict:\n"));
        seString.Append(new UIForegroundPayload(35));

        if (xpResult.CrystallineConflictWin > 0)
        {
            seString.Append(new TextPayload($"Win: {xpResult.CrystallineConflictWin} " + (xpResult.CrystallineConflictWin == 1 ? "time" : "times") + "\n"));
        }

        if (xpResult.CrystallineConflictLose > 0)
        {
            seString.Append(new TextPayload($"Lose: {xpResult.CrystallineConflictLose} " + (xpResult.CrystallineConflictLose == 1 ? "time" : "times") + "\n"));
        }

        seString.Append(UIForegroundPayload.UIForegroundOff);
    }

    //Frontlines
    if (includeAll || specs.Contains("fl"))
    {
        seString.Append(new TextPayload("\nFrontlines:\n"));
        seString.Append(new UIForegroundPayload(518));

        if (xpResult.FrontlineWin > 0)
        {
            seString.Append(new TextPayload($"Take 1st Place: {xpResult.FrontlineWin} " + (xpResult.FrontlineWin == 1 ? "time" : "times") + "\n"));
        }

        if (xpResult.FrontlineLose2nd > 0)
        {
            seString.Append(new TextPayload($"Take 2nd Place: {xpResult.FrontlineLose2nd} " + (xpResult.FrontlineLose2nd == 1 ? "time" : "times") + "\n"));
        }

        if (xpResult.FrontlineLose3rd > 0)
        {
            seString.Append(new TextPayload($"Take 3rd Place: {xpResult.FrontlineLose3rd} " + (xpResult.FrontlineLose3rd == 1 ? "time" : "times") + "\n"));
        }

        seString.Append(UIForegroundPayload.UIForegroundOff);

        seString.Append(new TextPayload("\nFrontlines (Roulette):\n"));
        seString.Append(new UIForegroundPayload(518));

        if (xpResult.FrontlineDailyWin > 0)
        {
            seString.Append(new TextPayload($"Take 1st Place: {xpResult.FrontlineDailyWin} " + (xpResult.FrontlineDailyWin == 1 ? "time" : "times") + "\n"));
        }

        if (xpResult.FrontlineDailyLose2nd > 0)
        {
            seString.Append(new TextPayload($"Take 2nd Place: {xpResult.FrontlineDailyLose2nd} " + (xpResult.FrontlineDailyLose2nd == 1 ? "time" : "times") + "\n"));
        }

        if (xpResult.FrontlineDailyLose3rd > 0)
        {
            seString.Append(new TextPayload($"Take 3rd Place: {xpResult.FrontlineDailyLose3rd} " + (xpResult.FrontlineDailyLose3rd == 1 ? "time" : "times") + "\n"));
        }

        seString.Append(UIForegroundPayload.UIForegroundOff);
    }

    // Rival Wings
    if (includeAll || specs.Contains("rw"))
    {
        seString.Append(new TextPayload("\nRival Wings:\n"));
        seString.Append(new UIForegroundPayload(43));

        if (xpResult.RivalWingsWin > 0)
        {
            seString.Append(new TextPayload($"Win: {xpResult.RivalWingsWin} " + (xpResult.RivalWingsWin == 1 ? "time" : "times") + "\n"));
        }

        if (xpResult.RivalWingsLose > 0)
        {
            seString.Append(new TextPayload($"Lose: {xpResult.RivalWingsLose} " + (xpResult.RivalWingsLose == 1 ? "time" : "times") + "\n"));
        }

        seString.Append(UIForegroundPayload.UIForegroundOff);
    }

    if (seString.Payloads.Count > 0) Chat.Print(seString);
}



    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
