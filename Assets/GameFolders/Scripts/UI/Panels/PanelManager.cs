using UnityEngine;

namespace GameFolders.Scripts.UI.Panels
{
    public class PanelManager : MonoBehaviour
    {
        [Header("Panels")] 
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject mainPanel;
        
        private void OnEnable()
        {
            GameEvents.OnGameStart += ActivateGamePanel;
            GameEvents.OnLevelFailed += ActivateMainPanel;
            GameEvents.OnLevelWin += ActivateMainPanel;
        }

        private void ActivateMainPanel()
        {
            mainPanel.SetActive(true);
        }

        private void ActivateGamePanel()
        {
            gamePanel.SetActive(true);
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= ActivateGamePanel;
            GameEvents.OnLevelFailed -= ActivateMainPanel;
            GameEvents.OnLevelWin -= ActivateMainPanel;
        }
    }
}
