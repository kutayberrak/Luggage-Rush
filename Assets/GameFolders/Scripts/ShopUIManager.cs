using GameFolders.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    //[SerializeField] private TextMeshProUGUI slotBombCountText;
    //[SerializeField] private TextMeshProUGUI freezeCountText;

    [Header("Buttons")]
    [SerializeField] private Button slotBombBuyButton;
    [SerializeField] private Button freezeBuyButton;

    private void Awake()
    {
        if (slotBombBuyButton != null)
            slotBombBuyButton.onClick.AddListener(OnBuySlotBomb);
        if (freezeBuyButton != null)
            freezeBuyButton.onClick.AddListener(OnBuyFreeze);
    }

    private void Start()
    {
        UpdateUI();
    }
    public void UpdateUI()
    {
        if (moneyText != null && MoneyManager.Instance != null)
        {
            moneyText.text = MoneyManager.Instance.GetCurrentMoney().ToString();
        }
        /*
        if (slotBombCountText != null && PowerUpInventory.Instance != null)
        {
            int count = PowerUpInventory.Instance.GetCount(PowerUpType.SlotBomb);
            slotBombCountText.text = count.ToString();
        }

        if (freezeCountText != null && PowerUpInventory.Instance != null)
        {
            int count = PowerUpInventory.Instance.GetCount(PowerUpType.Freeze);
            freezeCountText.text = count.ToString();
        }
        */
    }

    public void OnBuySlotBomb()
    {
        if (PowerUpInventory.Instance != null)
            PowerUpInventory.Instance.BuySlotBomb();
        UpdateUI();
    }
    public void OnBuyFreeze()
    {
        if (PowerUpInventory.Instance != null)
            PowerUpInventory.Instance.BuyFreeze();
        UpdateUI();
    }
}