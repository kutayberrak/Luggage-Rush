using System;

namespace GameFolders.Scripts
{
    public static class GameEvents
    {
        public static event Action OnLevelWin;
        public static event Action OnLevelFailed;
        public static event Action OnGameStart;
        public static event Action OnReturnToMainMenu;
        public static event Action OnLevelRestarted;
        
        public static void TriggerLevelRestarted() => OnLevelRestarted?.Invoke();
        public static void TriggerReturnToMainMenu()=>OnReturnToMainMenu?.Invoke();
        public static void TriggerGameStart() => OnGameStart?.Invoke();
        public static void TriggerLevelWin() => OnLevelWin?.Invoke();
        public static void TriggerLevelFailed() => OnLevelFailed?.Invoke();
    }
}
