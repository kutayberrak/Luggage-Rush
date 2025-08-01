using System;
using UnityEngine;

namespace GameFolders.Scripts
{
    public static class GameEvents
    {
        public static event Action OnLevelWin;
        public static event Action OnLevelFailed;
        public static event Action OnGameStart;
        public static event Action OnReturnToMainMenu;
        public static event Action OnLevelRestarted;

        // Cooldown variables for level end events
        private static float lastLevelEndTime = -1f;
        private static readonly float levelEndCooldown = 2f; // 2 seconds cooldown

        public static void TriggerLevelRestarted() => OnLevelRestarted?.Invoke();
        public static void TriggerReturnToMainMenu() => OnReturnToMainMenu?.Invoke();
        public static void TriggerGameStart() => OnGameStart?.Invoke();

        public static void TriggerLevelWin()
        {
            if (CanTriggerLevelEndEvent())
            {
                lastLevelEndTime = Time.time;
                OnLevelWin?.Invoke();
            }
        }

        public static void TriggerLevelFailed()
        {
            if (CanTriggerLevelEndEvent())
            {
                lastLevelEndTime = Time.time;
                OnLevelFailed?.Invoke();
            }
        }

        private static bool CanTriggerLevelEndEvent()
        {
            return lastLevelEndTime < 0f || (Time.time - lastLevelEndTime) >= levelEndCooldown;
        }
    }
}
