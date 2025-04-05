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

                /*
                    self  => Wird sind der Spieler der die PlayerHealthGrab-Funktionalität ausführt.
                    other => Ist der Spieler der gerade gegriffen wird (also der Spieler, der in der playerGrabbing-Liste enthalten ist).

                    => self (sind wir mit wenig HP) "greift" (grabbing) other (mit ggf. mehr HP)
                    => Es war genau anders herum!!!! (obviously, aber die Logik mit self und other bleibt trotzdem bestehen)
                */

                ShadyMod.Logger.LogDebug($"self is {self.playerName} with {self.playerHealth.health} HP");
                ShadyMod.Logger.LogDebug($"other is {other.playerName} with {other.playerHealth.health} HP");

                // Zunächst prüfen, ob self überhaupt berechtigt ist, HP abzuziehen (wenn self <= 5 HP hat)
                if (self.playerHealth.health <= 5)
                {
                    // Prüfen, ob other genug hat, um HP abzuziehen
                    int val = ShadyMod.MinPlayerHpConfig?.Value ?? 20;
                    if (other.playerHealth.health >= val)
                    {
                        int health = other.playerHealth.health / 2;

                        // Okay, Player hat genug HP, um self zu heilen.
                        ShadyMod.Logger.LogDebug($"Stealing health from player (Amount: {health}) ...");

                        if (ShadyMod.SpeakConfig?.Value ?? true)
                            SpeakHealMessage(self, other);

                        self.playerHealth.HealOther(health, true);
                        other.playerHealth.HurtOther(health, Vector3.zero, savingGrace: false);

                        // Alter Code:
                        // __instance.grabbingTimer = 0f;
                    }
                    else
                        ShadyMod.Logger.LogDebug("Not stealing health from player (other player has too few HP)");
                }
                else
                    ShadyMod.Logger.LogDebug("Not stealing health from player (health <= 5)");
            }
        }
    }

    // Alter Code:
    /*
    [HarmonyPostfix, HarmonyPatch(typeof(PlayerHealthGrab), nameof(PlayerHealthGrab.Update))]
    public static void OnPlayerHealthGrab(PlayerHealthGrab __instance)
    {
        bool flag = true;
        if (__instance.playerAvatar.isDisabled || __instance.hideLerp > 0f)
            flag = false;

        if (__instance.colliderActive != flag)
        {
            __instance.colliderActive = flag;
            __instance.physCollider.enabled = __instance.colliderActive;
        }

        if (!__instance.colliderActive || (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient))
            return;

        if (__instance.staticGrabObject.playerGrabbing.Count > 0)
        {
            __instance.grabbingTimer += Time.deltaTime;
            TryStealHealthFromOtherPlayer(__instance, __instance.staticGrabObject.playerGrabbing);

            if (__instance.grabbingTimer >= 1f)
                __instance.grabbingTimer = 0f;
        }
        else
            __instance.grabbingTimer = 0f;
    }*/

    private static readonly string[] messagesOther = new string[] 
    {
        "Was soll das {self}?",
        "Spinnst du?",
        "Hast du noch all Tassen im Schrank?",
        "Please leave me alone!",
        "Neieieiein! Kochen Sie mir keinen!",
        "Wer war das?",
        "Das war aber nicht nett",
        "Warum machst du sowas?",
        "Outsch!",
        "Das ist beschissen!",
        "Bist du dumm?",
        "Lass das",
        "Willst du Stress?",
        "Ich habe dir das verboten.",
        "Das ist nicht fair",
        "Das habe ich gesehen",
        "Was machst du denn?"
    };

    private static readonly string[] messagesSelf = new string[]
    {
        "Danke für das Healing {other}.",
        "shady >> suple",
        "HOOHOOHHOOOHHOHOHOH",
        "I love you <3",       
        "Selber schuld",
        "Das hast du jetzt davon",
        "Huhoh, jetzt bist du dran!"
    };

    private static void SpeakHealMessage(PlayerAvatar self, PlayerAvatar other)
    {
        int whichArray = Random.Range(1, 2);
        string[] toChoose = whichArray == 1 ? messagesSelf : messagesOther;
        PlayerAvatar? who = whichArray == 1 ? self : other;

        int rngMessage = Random.Range(0, toChoose.Length);        
        string message = toChoose[rngMessage].Replace("{self}", self.playerName).Replace("{other}", other.playerName);

        if (!string.IsNullOrEmpty(message))
        {
            ShadyMod.Logger.LogDebug($"Sending message: {message} to {who?.playerName}");
            who?.ChatMessageSend(message, who.isCrouching);
        }
    }
}