using TMPro;
using UnityEngine;

namespace GameFolders.Scripts.Managers
{
    public class MoneyManager : MonoBehaviour
    {
        public static MoneyManager Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI moneyText, moneyText2;
        private int _currentMoney;

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
            _currentMoney += amount;
            SaveMoney();
            UpdateMoneyUI();
        }

        public bool TrySpendMoney(int amount)
        {
            if (_currentMoney >= amount)
            {
                _currentMoney -= amount;
                SaveMoney();
                UpdateMoneyUI();
                return true;
            }
            return false;
        }

        private void UpdateMoneyUI()
        {
            if (moneyText != null)
                moneyText.text = _currentMoney.ToString();

            if (moneyText2 != null)
                moneyText2.text = _currentMoney.ToString();
        }

        private void SaveMoney()
        {
            PlayerPrefs.SetInt(MONEY_KEY, _currentMoney);
            PlayerPrefs.Save();
        }

        private void LoadMoney()
        {
            _currentMoney = PlayerPrefs.GetInt(MONEY_KEY, 0);
        }

        public int GetCurrentMoney()
        {
            return _currentMoney;
        }

        public void ResetMoney()
        {
            _currentMoney = 0;
            SaveMoney();
            UpdateMoneyUI();
        }
    }
}
