using HarmonyLib;

namespace ShadyMod
{
    [HarmonyPatch(typeof(PlayerController))]
    public static class PlayerControllerPatch
    {
        [HarmonyPrefix, HarmonyPatch(nameof(PlayerController.Start))]
        private static void Start_Prefix(PlayerController __instance)
        {
            // Code to execute for each PlayerController *before* Start() is called.
            // ShadyMod.Logger.LogDebug($"{__instance} Start Prefix");
        }

        [HarmonyPostfix, HarmonyPatch(nameof(PlayerController.Update))]
        private static void Start_Postfix(PlayerController __instance)
        {
            // Code to execute for each PlayerController *after* Start() is called.
            // ShadyMod.Logger.LogDebug($"{__instance} Start Postfix");
        }
    }
}