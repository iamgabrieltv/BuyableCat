using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using LethalLib.Modules;
using Unity.Netcode.Components;
using UnityEngine;

namespace BuyableCat;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("evaisa.lethallib")]
[BepInDependency("BMX.LobbyCompatibility")]
[BepInDependency("Jordo.NeedyCats")]
[BepInDependency("com.sigurd.csync", "5.0.0")]
[LobbyCompatibility(CompatibilityLevel.Everyone, VersionStrictness.None)]
public class BuyableCat : BaseUnityPlugin
{
    public static BuyableCat Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal new static MyConfig Config;
    private static Item CatItem;

    private void Awake()
    {
        Config = new MyConfig(base.Config);
        Logger = base.Logger;
        Logger.LogInfo("Loaded BuyableCat");
        harmony.PatchAll(typeof(BuyableCat));

        Config.InitialSyncCompleted += (sender, args) =>
        {
            Logger.LogInfo("InitialSyncCompleted");
            Logger.LogInfo(Config.CatPriceConfig.Value);
            Items.UpdateShopItemPrice(CatItem, Config.CatPriceConfig.Value);
        };
    }

    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    [HarmonyPostfix]
    [HarmonyAfter("Jordo.NeedyCats")]
    static void AddItemToStore(StartOfRound __instance)
    {
        Logger.LogInfo("Running Patch");
        List<Item> AllItems = Resources.FindObjectsOfTypeAll<Item>().ToList();
        CatItem = AllItems.FirstOrDefault(item => item.name.Equals("CatItem") && item.spawnPrefab != null);
        Item CatFoodItem = AllItems.FirstOrDefault(item => item.name.Equals("CatFoodItem") && item.spawnPrefab != null);
        
        if (CatFoodItem != null)
        {
            CatFoodItem.itemName = "CatFood";
            Items.RegisterShopItem(CatFoodItem, 5);
            Logger.LogInfo($"Added {CatFoodItem.name} to store");
        }
        else
        {
            Logger.LogError("Could not find CatFoodItem");
        }

        if (CatItem != null)
        {
            var networkTransform = CatItem.spawnPrefab.GetComponent<NetworkTransform>();
            if (networkTransform != null)
            {
                networkTransform.InLocalSpace = false;
                networkTransform.Interpolate = true;
                networkTransform.SyncPositionX = true;
                networkTransform.SyncPositionY = true;
                networkTransform.SyncPositionZ = true;
                networkTransform.SyncRotAngleX = true;
                networkTransform.SyncRotAngleY = true;
                networkTransform.SyncRotAngleZ = true;
                networkTransform.SyncScaleX = false;
                networkTransform.SyncScaleY = false;
                networkTransform.SyncScaleZ = false;
            }
            
            Items.RegisterShopItem(CatItem, Config.CatPriceConfig.Value);
            Logger.LogInfo($"Added {CatItem.name} to store");
        }
        else
        {
            Logger.LogError("Could not find CatItem");
        }
    }
}