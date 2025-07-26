using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image iconImage;
    private bool isOccupied = false;
    public bool IsOccupied => isOccupied;

    public string StoredUniqueID;

    public void FillSlotWithSprite(Sprite newSprite)
    {
        if (iconImage == null)
        {
            Debug.LogWarning("Slot içinde Image atanmadı!");
            return;
        }

        iconImage.sprite = newSprite;
        iconImage.enabled = true;
        isOccupied = true;
        StartCoroutine(PunchScale());
    }

    public void ClearSlot()
    {
        if (iconImage == null) return;
        iconImage.sprite = null;
        iconImage.enabled = false;
        isOccupied = false;
        StoredUniqueID = null;
    }

    public void CopyFrom(Slot other)
    {
        this.iconImage.sprite = other.iconImage.sprite;
        this.StoredUniqueID = other.StoredUniqueID;
        this.SetOccupied(true);
    }

    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    private IEnumerator PunchScale()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
}
