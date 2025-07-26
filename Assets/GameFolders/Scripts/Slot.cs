using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Slot : MonoBehaviour
{
    public Image iconImage;

    private bool isOccupied = false;
    public bool IsOccupied => isOccupied;

    private bool isReserved = false;
    public bool IsReserved => isReserved;

    public string StoredUniqueID;

    public void SetReserved(bool value)
    {
        isReserved = value;
    }

    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    public void SetStoredID(string id)
    {
        StoredUniqueID = id;
    }

    public void FillSlot(Sprite sprite, string id)
    {
        iconImage.sprite = sprite;
        iconImage.enabled = true;
        StoredUniqueID = id;
        isOccupied = true;
        isReserved = false; // 🔥 bu çok önemli!
        StartCoroutine(PunchScale());
    }
    public void ClearSlot()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
        StoredUniqueID = null;
        isOccupied = false;
        isReserved = false;
        transform.localScale = Vector3.one;
    }

    public void CopyFrom(Slot other)
    {
        FillSlot(other.iconImage.sprite, other.StoredUniqueID);
    }

    private IEnumerator PunchScale()
    {
        Vector3 original = transform.localScale;
        transform.localScale = original * 1.2f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = original;
    }
}
