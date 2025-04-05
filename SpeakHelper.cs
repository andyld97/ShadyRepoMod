using UnityEngine;

namespace ShadyMod
{
    public static class SpeakHelper
    {
        #region Messages

        private static readonly string[] shadyMessagesOther = new string[]
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

        private static readonly string[] shadyMessagesSelf = new string[]
        {
            "Danke für das Healing {other}.",
            "shady >> suple",
            "HOOHOOHHOOOHHOHOHOH",
            "Du bist toll!",
            "Selber schuld",    
            "Das hast du jetzt davon",
            "Huhoh, jetzt bist du dran!"
        };

        private static readonly string[] messagesOther = new string[]
        {
            "What are you doing {self}?",
            "Are you crazy?",
            "Please leave me alone!",
            "Who was that?",
            "That was not nice!",
            "Why are you doing this?",
            "Ouch!",
            "That's not fair!",
            "Are you stupid?",
            "Stop it!",
            "Do you want to fight?",
            "I told you not to do that.",
            "I saw that!",
        };

        private static readonly string[] messagesSelf = new string[]
        {
            "Thanks for the healing {other}.",
            "HOOHOOHHOOOHHOHOHOH",
            "You are great, thanks!",
            "You are so nice!",
            "You are so generous!",
            "You are so kind!",
            "Huoh, gotcha!",
        };

        #endregion

        public static void SpeakHealMessage(PlayerAvatar self, PlayerAvatar other, bool useShadyInsiders = true)
        {
            int talkingPlayer = Random.Range(1, 2);

            string[] toChoose;

            if (useShadyInsiders)
                toChoose = (talkingPlayer == 1 ? shadyMessagesSelf : shadyMessagesOther);
            else
                toChoose = (talkingPlayer == 1 ? messagesSelf : messagesOther);

            PlayerAvatar? who = talkingPlayer == 1 ? self : other;

            int rngMessage = Random.Range(0, toChoose.Length);
            string message = toChoose[rngMessage].Replace("{self}", self.playerName).Replace("{other}", other.playerName);

            if (!string.IsNullOrEmpty(message))
            {
                ShadyMod.Logger.LogDebug($"Sending message: {message} to {who?.playerName}");
                who?.ChatMessageSend(message, who.isCrouching);
            }
        }
    }
}