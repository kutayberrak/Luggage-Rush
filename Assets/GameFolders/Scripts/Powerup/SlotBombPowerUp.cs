using GameFolders.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotBombPowerUp : IPowerUp
{
    public PowerUpType Type => PowerUpType.SlotBomb;
    public float Duration => 0f;

    public void Activate(object context = null)
    {
        List<Slot> slots = SlotManager.Instance.slots;
        Slot slot = null;
        foreach (var s in slots)
        {
            if (!s.IsOccupied)
                continue;

            var clickable = s.Occupant.GetComponent<ClickableObject>();
            if (clickable == null)
                continue;

            if (Enum.TryParse<JunkPieceType>(clickable.UniqueID, out var jpType))
            {
                if (jpType == JunkPieceType.None)
                {
                    break;
                }
                slot = s;
            }
        }

        if (slot == null)
        {
            Debug.Log("SlotBomb: Hi� JunkPiece bulunamad�, power-up iptal edildi.");
            return;
        }

        var go = slot.Occupant;

        slot.ClearSlot();

        SlotManager.Instance.CompactSlots3D();
        ObjectPoolManager.Instance.ReturnObjectToPool(go);

        PowerUpInventory.Instance.DecreaseCount(PowerUpType.SlotBomb);

        AudioManager.Instance?.PlaySFX("BombSFX");
    }

    public void Deactivate() { }
}
