using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public static DebugScript Instance { get; private set; }
    public PowerUpInventory powerUpInventory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TestFreeze()
    {
        if (powerUpInventory.TryUse(PowerUpType.Freeze, out var pu))
        {
            PowerUpScheduler.Instance.Schedule(pu, pu.Duration);
            Debug.Log("Freeze PowerUp aktifleþtirildi!");
        }
        else
        {
            Debug.Log("Freeze PowerUp kalmadý.");
        }
    }

    public void TestSlotBomb()
    {
        if (powerUpInventory.TryUse(PowerUpType.SlotBomb, out var pu))
        {
            PowerUpScheduler.Instance.Schedule(pu, pu.Duration);
            Debug.Log("SlotBomb PowerUp aktifleþtirildi!");
        }
        else
        {
            Debug.Log("SlotBomb PowerUp kalmadý.");
        }
    }
}
