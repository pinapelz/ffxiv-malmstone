using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Malmstone;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int DefaultTargetRankProperty { get; set; } = 1;
    public bool ShowProgressionToastPostMatch { get; set; } = true;
    public bool ShowProgressionChatPostRW { get; set; } = true;
    public bool ShowProgressionChatPostCC { get; set; } = true;
    public bool ShowProgressionChatPostFL { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
