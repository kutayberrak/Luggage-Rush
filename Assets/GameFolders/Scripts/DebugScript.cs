using GameFolders.Scripts;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GameFolders.Scripts.Managers;

public class DebugScript : MonoBehaviour
{
    public static DebugScript Instance { get; private set; }
    public PowerUpInventory powerUpInventory;

    [SerializeField] private int powerUpCost = 5;

    [SerializeField] private TextMeshProUGUI freezeCountText;
    [SerializeField] private TextMeshProUGUI slotBombCountText;
    [SerializeField] private Sprite emptyPowerUpIcon;
    [SerializeField] private Sprite nonEmptyPowerUpIcon;
    [SerializeField] private Image freezeButton;
    [SerializeField] private Image slotBombButton;

    [SerializeField] private GameObject freezeBuyImage;
    [SerializeField] private GameObject slotBombBuyImage;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnGameStart += LoadPowerUpsCount;
    }
    private void OnDisable()
    {
        GameEvents.OnGameStart -= LoadPowerUpsCount;
    }
    public void TestFreeze()
    {
        if (powerUpInventory.TryUse(PowerUpType.Freeze, out var pu))
        {
            PowerUpScheduler.Instance.Schedule(pu, pu.Duration);
            Debug.Log("Freeze PowerUp aktifle�tirildi!");
        }
        else
        {

            if (MoneyManager.Instance.TrySpendMoney(powerUpCost))  //
            {
                powerUpInventory.IncreaseCount(PowerUpType.Freeze);
                //LoadPowerUpsCount();
            }
            else
            {
                Debug.Log("Freeze PowerUp and money is not enough.");
                return;
            }

        }
    }

    public void TestSlotBomb()
    {
        if (powerUpInventory.TryUse(PowerUpType.SlotBomb, out var pu))
        {
            PowerUpScheduler.Instance.Schedule(pu, pu.Duration);
            Debug.Log("SlotBomb PowerUp aktifle�tirildi!");
        }
        else
        {
            if (MoneyManager.Instance.TrySpendMoney(powerUpCost))
            {
                powerUpInventory.IncreaseCount(PowerUpType.SlotBomb);
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
        int freezeCount = powerUpInventory.GetCount(PowerUpType.Freeze);
        int slotBombCount = powerUpInventory.GetCount(PowerUpType.SlotBomb);

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
}
