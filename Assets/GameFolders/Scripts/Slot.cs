using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening; // DOTween kütüphanesini dahil et!

public class Slot : MonoBehaviour
{
    public Image iconImage; // Öğe ikonunu gösteren Image bileşenine referans.

    private bool isOccupied = false;
    public bool IsOccupied => isOccupied; // Doluluk durumu için salt okunur özellik.

    private bool isReserved = false;
    public bool IsReserved => isReserved; // Rezerve durumu için salt okunur özellik.

    public string StoredUniqueID; // Bu slottaki öğenin benzersiz kimliğini saklar.

    /// <summary>
    /// Slotun başlangıçta varsayılan ölçeğinde (Vector3.one) olmasını sağlar.
    /// İkonun başlangıçta gizli ve şeffaf olmasını garanti eder.
    /// </summary>
    private void Awake()
    {
        transform.localScale = Vector3.one;
        if (iconImage != null)
        {
            iconImage.enabled = false; // İkonu başlangıçta gizle
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0); // Tamamen şeffaf yap
            iconImage.rectTransform.localScale = Vector3.one; // İkonun ölçeğini de sıfırla
        }
    }

    /// <summary>
    /// Slotun rezerve durumunu ayarlar. Rezerve edilmiş bir slot boş slot olarak kullanılamaz.
    /// </summary>
    /// <param name="value">Slotu rezerve etmek için true, aksi takdirde false.</param>
    public void SetReserved(bool value)
    {
        isReserved = value;
    }

    /// <summary>
    /// Slotun doluluk durumunu ayarlar.
    /// </summary>
    /// <param name="value">Slot doluysa true, aksi takdirde false.</param>
    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    /// <summary>
    /// Bu slottaki öğenin benzersiz kimliğini ayarlar.
    /// </summary>
    /// <param name="id">Benzersiz ID dizesi.</param>
    public void SetStoredID(string id)
    {
        StoredUniqueID = id;
    }

    /// <summary>
    /// Slotu yeni bir öğe sprite'ı ve benzersiz ID ile doldurur.
    /// Bu metod sadece slotun VERİSİNİ günceller, görseli doğrudan yönetmez.
    /// </summary>
    /// <param name="sprite">Gösterilecek sprite.</param>
    /// <param name="id">Öğenin benzersiz ID'si.</param>
    public void FillSlotDataOnly(Sprite sprite, string id)
    {
        if (sprite == null)
            Debug.LogWarning($"FillSlotDataOnly: incoming sprite is NULL for id {id}");

        if (iconImage != null)
        {
            iconImage.sprite = sprite;
        }

        StoredUniqueID = id;
        isOccupied = true;
        isReserved = false;
    }

    /// <summary>
    /// Slotun verilerini temizler. Görseli doğrudan etkilemez.
    /// </summary>
    public void ClearDataOnly()
    {
        StoredUniqueID = null; // ID'yi temizle.
        isOccupied = false; // Boş olarak işaretle.
        isReserved = false; // Rezerve edilmemiş olarak işaretle.
    }

    /// <summary>
    /// Slotu temizler, öğesini kaldırır ve durumunu sıfırlar.
    /// Hem veri hem de görsel sıfırlamayı içerir.
    /// </summary>
    public void ClearSlot()
    {
        ClearDataOnly(); // Önce veriyi temizle
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false; // Görseli gizle.
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0); // Tamamen şeffaf yap
            iconImage.rectTransform.localScale = Vector3.one; // İkonun ölçeğini de sıfırla
        }
        transform.localScale = Vector3.one; // Slot GameObject'inin ölçeğini varsayılana sıfırla.
    }

    /// <summary>
    /// İçeriği (sprite ve ID) başka bir slottan bu slota kopyalar.
    /// Bu metod sadece veriyi kopyalar.
    /// </summary>
    /// <param name="other">Kopyalanacak Slot objesi.</param>
    public void CopyDataFrom(Slot other)
    {
        if (other == null)
        {
            Debug.LogWarning("CopyDataFrom: other is null!");
            return;
        }

        if (other.iconImage == null)
        {
            Debug.LogWarning($"CopyDataFrom: other.iconImage is null! Slot name: {other.name}");
        }
        else
        {
            string spriteName = other.iconImage.sprite != null ? other.iconImage.sprite.name : "NULL";
            Debug.Log($"CopyDataFrom: copying from slot {other.name}, sprite = {spriteName}, id = {other.StoredUniqueID}");
        }

        ClearDataOnly();
        FillSlotDataOnly(other.iconImage?.sprite, other.StoredUniqueID);
    }

    /// <summary>
    /// Slot doldurulduğunda hızlı bir "punch" ölçek animasyonu için Coroutine.
    /// İkonun her zaman varsayılan ölçeğine (Vector3.one) dönmesini sağlar.
    /// </summary>
    public IEnumerator PunchScaleCoroutine() // Public yapıldı, böylece SlotManager çağırabilir
    {
        if (iconImage == null) yield break;

        // Animasyon başlamadan önce ikonun görünür ve tam opak olduğundan emin ol.
        iconImage.enabled = true;
        // iconImage'ın rengi zaten PlaceObjectDataOnly'de ayarlanacak, burada sadece tam opaklık sağla.
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1);

        Vector3 originalIconScale = Vector3.one; // İkonun varsayılan ölçeği
        Vector3 punchScale = originalIconScale * 1.2f; // Punch animesi için büyütme miktarı

        // İkonun RectTransform'unu (veya normal transform'unu) ölçeklendir
        iconImage.rectTransform.localScale = punchScale; // Anlık olarak büyüt
        yield return new WaitForSeconds(0.1f); // Kısa bir süre bekle
        iconImage.rectTransform.localScale = originalIconScale; // Temel boyuta geri dön
    }
}
