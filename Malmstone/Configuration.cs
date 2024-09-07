using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Malmstone;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int DefaultTargetRankProperty { get; set; } = 1;
    public int PostmatchProgressionToastType { get; set; } = 0; // 0 = normal, 1=quest, 2=error
    public bool ShowProgressionToastPostMatch { get; set; } = true;
    public bool ShowProgressionChatPostRW { get; set; } = true;
    public bool ShowProgressionChatPostCC { get; set; } = true;
    public bool ShowProgressionChatPostFL { get; set; } = true;
    public bool ShowMainWindowOnPVPReward { get; set; } = true;
    public bool SkipProgressionToastAfterGoal { get; set; } = false;
    public bool SkipProgressionChatAfterGoal { get; set; } = false;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
