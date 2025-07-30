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
    private Dictionary<PowerUpType, int> powerUpCounts = new()
    {
        { PowerUpType.Freeze, 3 },
        { PowerUpType.SlotBomb,  3 },
        // ileride di�erleri i�in de ekle
    };
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

    public void UseFreeze()
    {

        if (TryUse(PowerUpType.Freeze, out var pu))
        {
            PowerUpScheduler.Instance.Schedule(pu, pu.Duration);
        }
        else
        {

            if (MoneyManager.Instance.TrySpendMoney(powerUpCost))  //
            {
                IncreaseCount(PowerUpType.Freeze);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("CoinSFX");
                //LoadPowerUpsCount();
            }
            else
            {
                Debug.Log("Freeze PowerUp and money is not enough.");
                return;
            }

        }
    }

    public void UseSlotBomb()
    {



        if (TryUse(PowerUpType.SlotBomb, out var pu))
        {
            PowerUpScheduler.Instance.Schedule(pu, pu.Duration);
        }
        else
        {
            if (MoneyManager.Instance.TrySpendMoney(powerUpCost))
            {
                IncreaseCount(PowerUpType.SlotBomb);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("CoinSFX");
                //LoadPowerUpsCount();
            }
            else
            {
                Debug.Log("SlotBomb PowerUp and money is not enough.");
                return;
            }
        }
    }


    public void LoadPowerUpsCount() //method calls when game starts and clicks on the button
    {
        int freezeCount = GetCount(PowerUpType.Freeze);
        int slotBombCount = GetCount(PowerUpType.SlotBomb);

        if (freezeCount <= 0)
        {
            freezeBuyImage.SetActive(true);
            freezeButton.sprite = emptyPowerUpIcon;
        }
        else
        {
            freezeBuyImage.SetActive(false);
            freezeButton.sprite = nonEmptyPowerUpIcon;
        }

        if (slotBombCount <= 0)
        {
            slotBombBuyImage.SetActive(true);
            slotBombButton.sprite = emptyPowerUpIcon;
        }
        else
        {
            slotBombBuyImage.SetActive(false);
            slotBombButton.sprite = nonEmptyPowerUpIcon;
        }

        freezeCountText.text = freezeCount.ToString();

        slotBombCountText.text = slotBombCount.ToString();
    }


    public void DecreaseCount(PowerUpType type)
    {
        if (powerUpCounts.TryGetValue(type, out var c) && c > 0)
            powerUpCounts[type] = c - 1;
    }
    public void IncreaseCount(PowerUpType type)
    {
        if (powerUpCounts.TryGetValue(type, out var c))
            powerUpCounts[type] = c + 1;
        else
            powerUpCounts[type] = 1; // eğer yoksa, 1 olarak ekle
    }
    // UI binding i�in:
    public int GetCount(PowerUpType type)
        => powerUpCounts.TryGetValue(type, out var c) ? c : 0;
}
