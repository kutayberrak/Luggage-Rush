using GameFolders.Scripts.Enums;
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

        var slot = SlotManager.Instance.slots
            .FirstOrDefault(s => s.IsOccupied && s.Occupant.GetComponent<ClickableObject>().UniqueID != JunkPieceType.None.ToString());
       
        if (slot == null)
        {
            Debug.Log("SlotBomb: Hiç JunkPiece bulunamadý, power-up iptal edildi.");
            return;
        }

        var go = slot.Occupant;

        slot.ClearSlot();

        ObjectPoolManager.Instance.ReturnObjectToPool(go);

        PowerUpInventory.Instance.DecreaseCount(PowerUpType.SlotBomb);
    }

    public void Deactivate() { }
}
