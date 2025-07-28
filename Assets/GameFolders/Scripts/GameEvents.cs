using System;

public static class GameEvents
{
    public static event Action OnLevelWin;
    public static event Action OnLevelFailed;

    public static void TriggerLevelWin()
    {
        OnLevelWin?.Invoke();
    }

    public static void TriggerLevelFailed()
    {
        OnLevelFailed?.Invoke();
    }
}
