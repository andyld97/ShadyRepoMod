using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ShadyMod;

[BepInPlugin("Andy.ShadyMod", "ShadyMod", "1.0.4")]
public class ShadyMod : BaseUnityPlugin
{
    #region Properties
    internal static ShadyMod Instance { get; private set; } = null!;

    internal new static ManualLogSource Logger => Instance._logger;

    private ManualLogSource _logger => base.Logger;

    internal Harmony? Harmony { get; set; }

    #endregion

    #region Config Properties

    public static ConfigEntry<int>? MinPlayerHpConfig { get; private set; } = null;

    public static ConfigEntry<int>? MaxPlayerHpConfig { get; private set; } = null;

    public static ConfigEntry<bool>? EnableTalkConfig { get; private set; } = null;

    public static ConfigEntry<bool>? UseShadyLanguageConfig { get; private set; } = null;

    #endregion

    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        MinPlayerHpConfig = Config.Bind("Health", "MinPlayerHealth", 20, "Required minimum health of the target player to steal (Min: 15 | Max: 100).");
        MaxPlayerHpConfig = Config.Bind("Health", "MaxPlayerHealth", 9, "Maximum health you can have to steal from the target (Min: 1 | Max: 9).");

        EnableTalkConfig = Config.Bind("Health", "Speak", true, "Enable players to talk when their life is stolen.");
        UseShadyLanguageConfig = Config.Bind("Health", "SpeakShadyLanguage", true, "If false, [shady] insiders are excluded, and only English is used.");
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