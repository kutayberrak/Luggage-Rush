using UnityEngine;

namespace GameFolders.Scripts.UI.Buttons
{
    public class PurchaseButton : MonoBehaviour
    {
        public void Purchase(int amount)
        {
            if (MoneyManager.Instance.TrySpendMoney(amount))
            {
                Debug.Log("Purchased");
            }
        }
    }
}
