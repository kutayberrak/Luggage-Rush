using GameFolders.Scripts.Managers;
using UnityEngine;

namespace GameFolders.Scripts.UI.Buttons
{
    public class PurchaseButton : MonoBehaviour
    {
        public void Purchase(int amount)
        {
            if (MoneyManager.Instance.TrySpendMoney(amount))
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("CoinSFX");
                Debug.Log("Purchased");
            }
        }
    }
}
