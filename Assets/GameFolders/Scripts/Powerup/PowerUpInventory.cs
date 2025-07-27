using GameFolders.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpInventory : MonoBehaviour
{
    public ConveyorBeltController conveyor;
    private Dictionary<PowerUpType, int> powerUpCounts = new()
    {
        { PowerUpType.Freeze, 3 },
        // ileride diðerleri için de ekle
    };

    public bool TryUse(PowerUpType type, out IPowerUp powerUp)
    {
        if (GetCount(type) <= 0)
        {
            powerUp = null;
            return false;
        }

        powerUpCounts[type]--;


        switch (type)
        {
            case PowerUpType.Freeze:
                powerUp = new FreezePowerUp(
                    duration: 3f,
                    conveyor: conveyor
                );
                break;

            // ileride eklenecekler:

            default:
                powerUp = null;
                break;
        }

        return powerUp != null;
    }

    // UI binding için:
    public int GetCount(PowerUpType type)
        => powerUpCounts.TryGetValue(type, out var c) ? c : 0;
}
