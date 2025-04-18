using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;

namespace BuyableCat;

public class MyConfig : SyncedConfig2<MyConfig>
{
    [SyncedEntryField] public SyncedEntry<int> CatPriceConfig;

    public MyConfig(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_GUID)
    {
        CatPriceConfig = cfg.BindSyncedEntry(
            new ConfigDefinition("Prices", "Cat Price"),
            50,
            new ConfigDescription("Credits needed to purchase a cat.")
        );
        
        ConfigManager.Register(this);
    }
}