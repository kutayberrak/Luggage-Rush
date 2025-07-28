using GameFolders.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpInventory : MonoBehaviour
{
    public static PowerUpInventory Instance { get; private set; }

    public ConveyorBeltController conveyor;
    private Dictionary<PowerUpType, int> powerUpCounts = new()
    {
        { PowerUpType.Freeze, 3 },
        { PowerUpType.SlotBomb,  3 },
        // ileride diðerleri için de ekle
    };
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool TryUse(PowerUpType type, out IPowerUp powerUp)
    {
        if (GetCount(type) <= 0)
        {
            powerUp = null;
            return false;
        }

        switch (type)
        {
            case PowerUpType.Freeze:
                powerUp = new FreezePowerUp(
                    duration: 3f,
                    conveyor: conveyor
                );
                break;
            case PowerUpType.SlotBomb:
                powerUp = new SlotBombPowerUp();
                break;

            // ileride eklenecekler:

            default:
                powerUp = null;
                break;
        }

        return powerUp != null;
    }
    public void DecreaseCount(PowerUpType type)
    {
        if (powerUpCounts.TryGetValue(type, out var c) && c > 0)
            powerUpCounts[type] = c - 1;
    }
    // UI binding için:
    public int GetCount(PowerUpType type)
        => powerUpCounts.TryGetValue(type, out var c) ? c : 0;
}
