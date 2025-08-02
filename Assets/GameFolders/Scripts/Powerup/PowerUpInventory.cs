using GameFolders.Scripts;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GameFolders.Scripts.Managers;

public class PowerUpInventory : MonoBehaviour
{
    public static PowerUpInventory Instance { get; private set; }

    [SerializeField] private int powerUpCost = 5;

    [SerializeField] private TextMeshProUGUI freezeCountText;
    [SerializeField] private TextMeshProUGUI slotBombCountText;
    [SerializeField] private Sprite emptyPowerUpIcon;
    [SerializeField] private Sprite nonEmptyPowerUpIcon;
    [SerializeField] private Image freezeButton;
    [SerializeField] private Image slotBombButton;

    [SerializeField] private GameObject freezeBuyImage;
    [SerializeField] private GameObject slotBombBuyImage;
    public ConveyorBeltController conveyor;

    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject freezeEffect;
    [SerializeField] private MeshRenderer conveyorMeshRenderer;
    [SerializeField] private Texture2D conveyorDefaultMaterial;
    [SerializeField] private Texture2D conveyorFrozenMaterial;
    private Vector3 explosionParticleOffset = Vector3.up * 0.3f;

    private const int DefaultCount = 3;
    private const string PrefKeyPrefix = "PowerUpCount_";

    // Holds current counts in memory
    private Dictionary<PowerUpType, int> powerUpCounts = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadCountsFromPrefs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnGameStart += LoadPowerUpsCount;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStart -= LoadPowerUpsCount;
    }

    public bool TryUse(PowerUpType type, out IPowerUp powerUp)
    {
        int count = GetCount(type);
        if (count <= 0)
        {
            powerUp = null;
            return false;
        }

        switch (type)
        {
            case PowerUpType.Freeze:
                powerUp = new FreezePowerUp(duration: 3f, conveyor: conveyor);
                break;
            case PowerUpType.SlotBomb:
                powerUp = new SlotBombPowerUp();
                break;
            default:
                powerUp = null;
                break;
        }

        return powerUp != null;
    }

    public void UseFreeze()
    {
        if (TryUse(PowerUpType.Freeze, out var pu))
        {
            PowerUpScheduler.Instance.Schedule(pu, pu.Duration);
            LoadPowerUpsCount();

        }
        else
        {
            PurchasePowerUp(PowerUpType.SlotBomb);
        }
    }

    public void UseSlotBomb()
    {
        if (TryUse(PowerUpType.SlotBomb, out var pu))
        {
            PowerUpScheduler.Instance.Schedule(pu, pu.Duration);
            LoadPowerUpsCount();

        }
        else
        {
            PurchasePowerUp(PowerUpType.SlotBomb);
        }
    }
    private bool PurchasePowerUp(PowerUpType type)
    {
        if (MoneyManager.Instance.TrySpendMoney(powerUpCost))
        {
            IncreaseCount(type);
            AudioManager.Instance?.PlaySFX("CoinSFX");
            LoadPowerUpsCount();
            return true;
        }
        Debug.Log($"{type} purchase failed: not enough money.");
        return false;
    }
    public void BuyFreeze()
    {
        PurchasePowerUp(PowerUpType.Freeze);
    }
    public void BuySlotBomb()
    {
        PurchasePowerUp(PowerUpType.SlotBomb);
    }

    public void LoadPowerUpsCount()
    {
        int freezeCount = GetCount(PowerUpType.Freeze);
        int slotBombCount = GetCount(PowerUpType.SlotBomb);

        freezeBuyImage.SetActive(freezeCount <= 0);
        freezeButton.sprite = freezeCount > 0 ? nonEmptyPowerUpIcon : emptyPowerUpIcon;

        slotBombBuyImage.SetActive(slotBombCount <= 0);
        slotBombButton.sprite = slotBombCount > 0 ? nonEmptyPowerUpIcon : emptyPowerUpIcon;

        freezeCountText.text = freezeCount.ToString();
        slotBombCountText.text = slotBombCount.ToString();
    }

    private void LoadCountsFromPrefs()
    {
        foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
        {
            int count = PlayerPrefs.GetInt(PrefKeyPrefix + type.ToString(), DefaultCount);
            powerUpCounts[type] = count;
        }
    }

    public void DecreaseCount(PowerUpType type)
    {
        if (powerUpCounts.TryGetValue(type, out var c) && c > 0)
        {
            powerUpCounts[type] = c - 1;
            SaveCountToPrefs(type);
        }
    }

    public void IncreaseCount(PowerUpType type)
    {
        if (powerUpCounts.TryGetValue(type, out var c))
        {
            powerUpCounts[type] = c + 1;
        }
        else
        {
            powerUpCounts[type] = DefaultCount + 1;
        }
        SaveCountToPrefs(type);
    }

    private void SaveCountToPrefs(PowerUpType type)
    {
        PlayerPrefs.SetInt(PrefKeyPrefix + type.ToString(), GetCount(type));
        PlayerPrefs.Save();
    }
    public void PlayExplosionAtPosition(Vector3 position)
    {
        if (explosionEffect == null) return;

        Vector3 newPos = position + explosionParticleOffset;

        GameObject effect = Instantiate(explosionEffect, newPos, Quaternion.identity);
        Destroy(effect, 2f);
    }

    public void PlayFreezeEffectAtPosition()
    {
        if (freezeEffect == null) return;

        GameObject effect = Instantiate(freezeEffect, freezeEffect.transform.position, freezeEffect.transform.rotation);


        Destroy(effect, 7f);

        if (conveyorMeshRenderer != null && conveyorFrozenMaterial != null)
        {
            conveyorMeshRenderer.material.mainTexture = conveyorFrozenMaterial;
            Invoke(nameof(ResetConveyorMaterial), 3f);
        }
    }
    public void ResetConveyorMaterial()
    {
        if (conveyorMeshRenderer != null && conveyorDefaultMaterial != null)
        {
            conveyorMeshRenderer.material.mainTexture = conveyorDefaultMaterial;
        }
    }
    // UI binding
    public int GetCount(PowerUpType type)
        => powerUpCounts.TryGetValue(type, out var c) ? c : 0;
}
