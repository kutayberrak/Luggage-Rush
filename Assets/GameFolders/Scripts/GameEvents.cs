using System;

namespace GameFolders.Scripts
{
    public static class GameEvents
    {
        public static event Action OnLevelWin;
        public static event Action OnLevelFailed;
        public static event Action OnGameStart;
        public static void TriggerGameStart()
        {
            OnGameStart?.Invoke();
        }

        public static void TriggerLevelWin()
        {
            OnLevelWin?.Invoke();
        }

        public static void TriggerLevelFailed()
        {
            OnLevelFailed?.Invoke();
        }
    }
}
