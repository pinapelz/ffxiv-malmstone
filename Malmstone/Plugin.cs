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
using Malmstone.Addons;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

namespace Malmstone;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui Chat { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifeCycle { get; private set; } = null!;
    [PluginService] internal static IToastGui ToastGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; set; } = default!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    private const string CommandName = "/pmalm";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Malmstone");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    internal readonly PvPService PvPService;
    internal PvPMatchAddon PvPAddon;
    internal int CachedSeriesLevel;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        PvPService = new PvPService();
        PvPAddon = new PvPMatchAddon(this);
        if (Configuration.ShowProgressionChatPostCC)
            PvPAddon.EnableCrystallineConflictPostMatch();
        if (Configuration.ShowProgressionChatPostRW)
            PvPAddon.EnableRivalWingsPostMatch();
        if (Configuration.ShowProgressionChatPostFL || Configuration.TrackFrontlineBonus)
            PvPAddon.EnableFrontlinePostMatch();
        if (Configuration.ShowProgressionToastPostMatch)
            PvPAddon.EnablePostMatchProgressionToast();
        if (Configuration.ShowMainWindowOnPVPReward)
            EnablePVPRewardWindowAddon();

        if (Configuration.IsPrimedForBuff)
            PvPService.ConsecutiveThirdPlaceFrontline = 1;

        if (Configuration.PostmatchProgressionToastType < 0 || Configuration.PostmatchProgressionToastType > 2)
        {
            Configuration.PostmatchProgressionToastType = 0;
        }

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "/pmalm <rank> <all/cc/fl/rw> -- Displays PVP games left until a target rank. cc = Crystalline Conflict, fl = Frontlines, rw = Rivalwings"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        Framework.Update += CheckPlayerLoaded;
        ClientState.Login += OnLogin;
        ClientState.Logout += OnLogout;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        if (Configuration.ShowProgressionChatPostCC)
            PvPAddon.DisableCrystallineConflictPostMatch();
        if (Configuration.ShowProgressionChatPostRW)
            PvPAddon.DisableRivalWingsPostMatch();
        if (Configuration.ShowProgressionChatPostFL || Configuration.TrackFrontlineBonus)
            PvPAddon.DisableFrontlinePostMatch();
        if (Configuration.ShowProgressionToastPostMatch)
            PvPAddon.DisablePostMatchProgressionToast();
        if(Configuration.ShowMainWindowOnPVPReward)
            DisablePVPRewardWindowAddon();
        
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
    var CurrentSeriesLevel = pvpInfo.CurrentSeriesRank + GetSavedExtraLevels(); 
    if (!int.TryParse(splitArgs[0], out int targetRank))
    {
        if (splitArgs[0] == "next")
        {
            targetRank = CurrentSeriesLevel + 1;
        }
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

    if (targetRank < CurrentSeriesLevel)
    {
        Chat.PrintError("You've already surpassed Rank " + targetRank);
        return;
    }

    var xpResult = MalmstoneXPCalculator.CalculateXp(
        CurrentSeriesLevel,
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


    private void CheckPlayerLoaded(IFramework framework)
    {
        if(ClientState.LocalPlayer != null)
        {
            if (Configuration.TrackFrontlineBonus)
            {
                Logger.Debug("Player has loaded in. Attempting to get Frontline PVP Profile Data");
                PvPService.UpdateFrontlineResultCache();
                Logger.Debug("Initial Frontline Data Cached As: First: " + PvPService.CachedFrontlineResults.FirstPlace + 
                    " Second: " + PvPService.CachedFrontlineResults.SecondPlace + " Third: " + PvPService.CachedFrontlineResults.ThirdPlace);
                Framework.Update -= CheckPlayerLoaded;
            }
        }
    }

    private void OnLogin()
    {
        Logger.Debug("Player has logged in. Waiting for player data to load...");
        Framework.Update += CheckPlayerLoaded;
    }

    private void OnLogout() => Framework.Update -= CheckPlayerLoaded;
    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    public void OnOpenPVPRewardWindow(AddonEvent eventType, AddonArgs addonInfo)
    {
        if(PvPService.GetPvPSeriesInfo() != null)
            CachedSeriesLevel = PvPService.GetPvPSeriesInfo().CurrentSeriesRank;
        Logger.Debug("PVPRewardWindow Open, Current Series Level Cached: " + CachedSeriesLevel);
        MainWindow.OnOpenPVPRewardWindow();
    }

    public void OnClosePVPRewardWindow(AddonEvent eventType, AddonArgs addonInfo) =>
        MainWindow.OnClosePVPRewardWindow();

    public void UpdateExtraLevels(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (PvPService.GetPvPSeriesInfo() != null)
        {
            // If player claimed Extra Level reward (above Level 30) and we detect a decrease in Series level
            if(PvPService.GetPvPSeriesInfo().CurrentSeriesRank < CachedSeriesLevel && CachedSeriesLevel > 30)
            {
                var extraLevels = CachedSeriesLevel - 30;
                Logger.Debug("Player claimed extra levels: " +  extraLevels+ ", new ExtraLevels is " + GetSavedExtraLevels() + extraLevels);
                IncrementExtraLevels(extraLevels);
                Configuration.Save();
            }
            else
            {
                Logger.Debug("Player did not claim any extra ranks");
            }
        }
    }
    
    public int GetSavedExtraLevels()
    {
        ulong contentId = ClientState.LocalContentId;
        if (Configuration.ExtraLevelsMap.TryGetValue(contentId, out var extraLevels))
        {
            return extraLevels;
        }
        Logger.Debug("No Extra Levels saved for this character");
        int CurrentSeriesLevel = PvPService.GetPvPSeriesInfo()?.CurrentSeriesRank ?? 0;
        if (CurrentSeriesLevel > 30)
        {
            Configuration.ExtraLevelsMap[contentId] = CurrentSeriesLevel - 30;
            Configuration.Save();
            Logger.Debug("Extra Levels saved for this character: " + (CurrentSeriesLevel - 30));
            return CurrentSeriesLevel - 30;
        }
        Logger.Debug("Extra Levels saved for this character: 0");
        Configuration.ExtraLevelsMap[contentId] = 0;
        Configuration.Save();
        return 0;
    }

    public bool IncrementExtraLevels(int amount)
    {
        ulong contentId = ClientState.LocalContentId;
        if (Configuration.ExtraLevelsMap.TryGetValue(contentId, out var extraLevels))
        {
            Configuration.ExtraLevelsMap[contentId] = extraLevels + amount;
            Configuration.Save();
            Logger.Debug("Extra Levels incremented for this character: " + (extraLevels + amount));
            return true;
        }
        Logger.Debug("Failed to increment extra levels for this character");
        return false;
    }

    public void EnablePVPRewardWindowAddon()
    {
        AddonLifeCycle.RegisterListener(AddonEvent.PostSetup, "PvpReward", OnOpenPVPRewardWindow);
        AddonLifeCycle.RegisterListener(AddonEvent.PostRefresh, "PvpReward", UpdateExtraLevels);
        AddonLifeCycle.RegisterListener(AddonEvent.PreFinalize, "PvpReward", OnClosePVPRewardWindow);
    }
    public void DisablePVPRewardWindowAddon()
    {
        AddonLifeCycle.UnregisterListener(AddonEvent.PostSetup, "PvpReward", OnOpenPVPRewardWindow);
        AddonLifeCycle.UnregisterListener(AddonEvent.PostRefresh, "PvpReward", UpdateExtraLevels);
        AddonLifeCycle.UnregisterListener(AddonEvent.PreFinalize, "PvpReward", OnClosePVPRewardWindow);
    }

}

