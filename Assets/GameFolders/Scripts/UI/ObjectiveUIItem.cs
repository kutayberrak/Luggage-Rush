using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameFolders.Scripts.Enums;
using DG.Tweening;

public class ObjectiveUIItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image luggageIcon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image checkmarkImage;
    
    private LuggageType luggageType;
    private int targetCount;
    private int currentCount;
    private bool isCompleted = false;
    
    // **YENİ**: Animasyon için
    private Vector3 originalIconScale;
    private Sequence pulseSequence;
    
    public void Initialize(LuggageType type, int target, Sprite icon, string name)
    {
        luggageType = type;
        targetCount = target;
        currentCount = 0;
        isCompleted = false;
        
        // UI elementlerini ayarla
        if (luggageIcon != null && icon != null)
        {
            luggageIcon.sprite = icon;
            // **YENİ**: Orijinal scale'i kaydet
            originalIconScale = luggageIcon.transform.localScale;
        }
        
        // Başlangıç durumunu ayarla
        UpdateVisuals();
        UpdateCountText();
    }
    
    public void UpdateProgress()
    {
        bool wasCompleted = isCompleted;
        isCompleted = currentCount >= targetCount;
        
        UpdateCountText();
        UpdateVisuals();
        
        // Tamamlandığında animasyon veya ses efekti eklenebilir
        if (!wasCompleted && isCompleted)
        {
            OnObjectiveCompleted();
        }
    }
    
    private void UpdateCountText()
    {
        if (countText != null)
        {
            Debug.Log("target= " + targetCount);
            Debug.Log("current= " + currentCount);
            countText.text = (targetCount - currentCount).ToString();
        }
    }
    
    private void UpdateVisuals()
    {
        if (isCompleted)
        {   
            if (checkmarkImage != null)
            {
                checkmarkImage.gameObject.SetActive(true);
                countText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (checkmarkImage != null)
            {
                checkmarkImage.gameObject.SetActive(false);
                countText.gameObject.SetActive(true);
            }
        }
    }
    
    private void OnObjectiveCompleted()
    {
        Debug.Log($"[ObjectiveUIItem] {luggageType} hedefi tamamlandı! ({currentCount}/{targetCount})");
        
        // Burada tamamlama animasyonu veya ses efekti eklenebilir
        // Örnek: DOTween animasyonu, particle effect, ses efekti vs.
        
        // Ses efekti çal
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ObjectiveComplete");
        }
    }
    
    // **YENİ**: Toplanan sayısını arttır (slot'a ulaştığında)
    public void IncreaseCollectCount()
    {
        currentCount++;

        Debug.Log("current count" + currentCount);
        Debug.Log("target = " + targetCount);
        UpdateProgress();
        
        // **YENİ**: Logo animasyonu başlat
        PlayIconPulseAnimation();

        // Eğer hedef tamamlandıysa tamamlandı olarak işaretle
        if (currentCount >= targetCount && !isCompleted)
        {
            isCompleted = true;
            OnObjectiveCompleted();
        }
    }
    
    // **YENİ**: Mevcut hedef sayısını döndür
    public int GetCurrentTarget()
    {
        return targetCount;
    }
    
    // **YENİ**: Mevcut toplanan sayısını döndür
    public int GetCurrentCollected()
    {
        return currentCount;
    }
    
    // **YENİ**: Hedefin tamamlanıp tamamlanmadığını döndür
    public bool IsCompleted()
    {
        return isCompleted;
    }
    
    // **YENİ**: Logo pulse animasyonu
    private void PlayIconPulseAnimation()
    {
        if (luggageIcon == null) return;
        
        // Önceki animasyonu durdur
        if (pulseSequence != null)
        {
            pulseSequence.Kill();
        }
        
        // Pulse animasyonu oluştur
        pulseSequence = DOTween.Sequence();
        
        // Büyütme animasyonu
        pulseSequence.Append(luggageIcon.transform.DOScale(originalIconScale * 1.3f, 0.1f).SetEase(Ease.OutQuad));
        
        // Küçültme animasyonu
        pulseSequence.Append(luggageIcon.transform.DOScale(originalIconScale, 0.1f).SetEase(Ease.InQuad));
        
        /*// Tekrar büyütme (daha hafif)
        pulseSequence.Append(luggageIcon.transform.DOScale(originalIconScale * 1.15f, 0.15f).SetEase(Ease.OutQuad));
        
        // Son küçültme
        pulseSequence.Append(luggageIcon.transform.DOScale(originalIconScale, 0.15f).SetEase(Ease.InQuad));*/
        
        Debug.Log($"[ObjectiveUIItem] {luggageType} logosu pulse animasyonu başlatıldı");
    }
    
    // **YENİ**: Obje yok edildiğinde temizlik
    private void OnDestroy()
    {
        if (pulseSequence != null)
        {
            pulseSequence.Kill();
        }
    }
}