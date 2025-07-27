using UnityEngine;

public class DebugController : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private KeyCode debugSlotStatesKey = KeyCode.F1;
    [SerializeField] private KeyCode emergencyResetKey = KeyCode.F2;
    [SerializeField] private KeyCode testRapidClickKey = KeyCode.F3;
    [SerializeField] private KeyCode testShiftKey = KeyCode.F4;

    private void Update()
    {
        // Debug slot durumları
        if (Input.GetKeyDown(debugSlotStatesKey))
        {
            if (SlotManager.Instance != null)
            {
                SlotManager.Instance.DebugSlotStates();
            }
            else
            {
                Debug.LogWarning("SlotManager.Instance is null!");
            }
        }

        // Emergency reset
        if (Input.GetKeyDown(emergencyResetKey))
        {
            if (SlotManager.Instance != null)
            {
                SlotManager.Instance.EmergencyReset();
            }
            else
            {
                Debug.LogWarning("SlotManager.Instance is null!");
            }
        }

        // Test rapid click
        if (Input.GetKeyDown(testRapidClickKey))
        {
            Debug.Log("Testing rapid click simulation...");
            // Burada test objeleri oluşturabilirsiniz
        }

        // Test shift
        if (Input.GetKeyDown(testShiftKey))
        {
            Debug.Log("Testing shift operation...");
            if (SlotManager.Instance != null && SlotManager.Instance.slots.Count > 0)
            {
                // Slot durumlarını göster
                SlotManager.Instance.DebugSlotStates();
                
                // Test senaryosu: Aynı ID'li objeleri test et
                Debug.Log("=== Shift Test Scenario ===");
                Debug.Log("1. Aynı ID'li obje varsa -> sağına yerleştir");
                Debug.Log("2. Sağında farklı ID varsa -> shift yap, sonra yerleştir");
                Debug.Log("3. Hiç aynı ID yoksa -> ilk boş slot'a yerleştir");
                Debug.Log("==========================");
            }
        }
    }

    // **YENİ**: Inspector'dan çağrılabilir debug metodları
    [ContextMenu("Debug Slot States")]
    public void DebugSlotStates()
    {
        if (SlotManager.Instance != null)
        {
            SlotManager.Instance.DebugSlotStates();
        }
    }

    [ContextMenu("Emergency Reset")]
    public void EmergencyReset()
    {
        if (SlotManager.Instance != null)
        {
            SlotManager.Instance.EmergencyReset();
        }
    }
} 