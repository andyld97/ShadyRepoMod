using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ShadyMod;

[BepInPlugin("Andy.ShadyMod", "ShadyMod",  "1.0.3")]
public class ShadyMod : BaseUnityPlugin
{
    internal static ShadyMod Instance { get; private set; } = null!;

    internal new static ManualLogSource Logger => Instance._logger;

    private ManualLogSource _logger => base.Logger;

    internal Harmony? Harmony { get; set; }

    public static ConfigEntry<bool>? SpeakConfig = null;   
    public static ConfigEntry<int>? MinPlayerHpConfig;

    private void Awake()
    {
        Instance = this;
        
        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        SpeakConfig = Config.Bind("Health", "Speak", true, "True, when the player should speak");
        MinPlayerHpConfig = Config.Bind("Health", "MinPlayerHealth", 20, "The amount the other player must have at least to steal from!");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }

    private void Update()
    {
        // Code that runs every frame goes here
    }
}