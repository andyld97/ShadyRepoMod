using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace ShadyMod;

[HarmonyPatch(typeof(PlayerHealthGrab))]
public static class PlayerHealthGrabpatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerHealthGrab), nameof(PlayerHealthGrab.Update))]
    public static bool OnPlayerHealthGrabPrefix()
    {
        // Note: The first prefix that returns false will skip all remaining prefixes unless they have no side effects
        // (no return value, no ref arguments) and will skip the original too. Postfixes and Finalizers are not affected.
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayerHealthGrab), nameof(PlayerHealthGrab.Update))]
    public static void OnPlayerHealthGrabNew(PlayerHealthGrab __instance)
    {
        if (__instance.playerAvatar.isTumbling || SemiFunc.RunIsShop() || SemiFunc.RunIsArena())
        {
            if (__instance.hideLerp < 1f)
            {
                __instance.hideLerp += Time.deltaTime * 5f;
                __instance.hideLerp = Mathf.Clamp(__instance.hideLerp, 0f, 1f);
                __instance.hideTransform.localScale = new Vector3(1f, __instance.hideCurve.Evaluate(__instance.hideLerp), 1f);
                if (__instance.hideLerp >= 1f)
                {
                    __instance.hideTransform.gameObject.SetActive(value: false);
                }
            }
        }
        else if (__instance.hideLerp > 0f)
        {
            if (!__instance.hideTransform.gameObject.activeSelf)
            {
                __instance.hideTransform.gameObject.SetActive(value: true);
            }
            __instance.hideLerp -= Time.deltaTime * 2f;
            __instance.hideLerp = Mathf.Clamp(__instance.hideLerp, 0f, 1f);
            __instance.hideTransform.localScale = new Vector3(1f, __instance.hideCurve.Evaluate(__instance.hideLerp), 1f);
        }
        bool flag = true;
        if (__instance.playerAvatar.isDisabled || __instance.hideLerp > 0f)
        {
            flag = false;
        }
        if (__instance.colliderActive != flag)
        {
            __instance.colliderActive = flag;
            __instance.physCollider.enabled = __instance.colliderActive;
        }
        __instance.transform.position = __instance.followTransform.position;
        __instance.transform.rotation = __instance.followTransform.rotation;
        if (!__instance.colliderActive || (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient))
        {
            return;
        }

        if (__instance.staticGrabObject.playerGrabbing.Count > 0)
        {
            __instance.grabbingTimer += Time.deltaTime;

            bool foundPlayerToHeal = false;
            foreach (PhysGrabber item in __instance.staticGrabObject.playerGrabbing)
            {
                if (__instance.grabbingTimer >= 1f)
                {
                    PlayerAvatar playerAvatar = item.playerAvatar;
                    if (__instance.playerAvatar.playerHealth.health != __instance.playerAvatar.playerHealth.maxHealth && playerAvatar.playerHealth.health > 10)
                    {
                        ShadyMod.Logger.LogDebug($"Executing almost original game code: Player {playerAvatar.playerName} heals {__instance.playerAvatar.playerName} with 10 HP!");

                        __instance.playerAvatar.playerHealth.HealOther(10, effect: true);
                        playerAvatar.playerHealth.HurtOther(10, Vector3.zero, savingGrace: false);
                        playerAvatar.HealedOther();
                        foundPlayerToHeal = true;
                    }
                }
            }

            if (!foundPlayerToHeal)
                TryStealHealthFromOtherPlayer(__instance, __instance.staticGrabObject.playerGrabbing);

            if (__instance.grabbingTimer >= 1f)
                __instance.grabbingTimer = 0f;
        }
        else
            __instance.grabbingTimer = 0f;
    }

    private static void TryStealHealthFromOtherPlayer(PlayerHealthGrab __instance, List<PhysGrabber> physicsGrabs)
    {
        foreach (PhysGrabber item in __instance.staticGrabObject.playerGrabbing)
        {
            if (__instance.grabbingTimer >= 1f)
            {
                PlayerAvatar self = item.playerAvatar;
                PlayerAvatar other = __instance.playerAvatar;

                // ShadyMod.Logger.LogDebug($"self is {self.playerName} with {self.playerHealth.health} HP");
                // ShadyMod.Logger.LogDebug($"other is {other.playerName} with {other.playerHealth.health} HP");

                int maxHpSelf = ShadyMod.MaxPlayerHpConfig?.Value ?? 9;
                int minHpOtherPlayer = ShadyMod.MinPlayerHpConfig?.Value ?? 20;

                if (maxHpSelf <= 0)
                    maxHpSelf = 1;    
                if (maxHpSelf > 9)
                    maxHpSelf = 9;

                if (minHpOtherPlayer < 15)
                    minHpOtherPlayer = 15;
                else if (minHpOtherPlayer > 100)
                    minHpOtherPlayer = 100; 

                // Check if player is allowed to stehal health from other player
                if (self.playerHealth.health <= maxHpSelf)
                {
                    // Check if other player has enough HP to steal
                    if (other.playerHealth.health >= minHpOtherPlayer)
                    {
                        int health = other.playerHealth.health / 2;

                        ShadyMod.Logger.LogDebug($"Stealing health from player (Amount: {health}) ...");

                        if (ShadyMod.EnableTalkConfig?.Value ?? true)
                            SpeakHelper.SpeakHealMessage(self, other, ShadyMod.UseShadyLanguageConfig?.Value ?? true);

                        self.playerHealth.HealOther(health, true);
                        other.playerHealth.HurtOther(health, Vector3.zero, savingGrace: false);
                    }
                }
            }
        }
    }    
}