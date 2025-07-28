using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI moneyText;
    private int currentMoney;

    private const string MONEY_KEY = "Money";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadMoney();
        UpdateMoneyUI();
    }

    public void EarnMoney(int amount)
    {
        currentMoney += amount;
        SaveMoney();
        UpdateMoneyUI();
    }

    public void SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            SaveMoney();
            UpdateMoneyUI();
        }
        else
        {
            Debug.LogWarning("Yetersiz para.");
        }
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = currentMoney.ToString();
    }

    private void SaveMoney()
    {
        PlayerPrefs.SetInt(MONEY_KEY, currentMoney);
        PlayerPrefs.Save();
    }

    private void LoadMoney()
    {
        currentMoney = PlayerPrefs.GetInt(MONEY_KEY, 0);
    }

    public int GetCurrentMoney()
    {
        return currentMoney;
    }

    public void ResetMoney()
    {
        currentMoney = 0;
        SaveMoney();
        UpdateMoneyUI();
    }
}
