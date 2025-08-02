using GameFolders.Scripts.Interfaces;
using UnityEngine;

namespace GameFolders.Scripts.UI.Panels
{
    public class PanelManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject failPanel;
        [SerializeField] private GameObject mainMenuUpperPanel;

        private IAnimatedUI _mainPanel;
        private IAnimatedUI _gamePanel;

        private void Awake()
        {
            _mainPanel = mainPanel.GetComponent<IAnimatedUI>();
            _gamePanel = gamePanel.GetComponent<IAnimatedUI>();
        }
        private void OnEnable()
        {
            GameEvents.OnGameStart += OnGameStart;
            GameEvents.OnReturnToMainMenu += OnReturnToMainMenu;
            GameEvents.OnLevelWin += OnLevelWin;
            GameEvents.OnLevelFailed += OnLevelFailed;
        }
        private void OnDisable()
        {
            GameEvents.OnGameStart -= OnGameStart;
            GameEvents.OnReturnToMainMenu -= OnReturnToMainMenu;
            GameEvents.OnLevelWin -= OnLevelWin;
            GameEvents.OnLevelFailed -= OnLevelFailed;
        }
        #region Event Handlers
        private void OnLevelFailed()
        {
            EnablePanel(failPanel);
            _gamePanel.DeactivatePanel();

            AudioManager.Instance.PlaySFX("FailSFX_1");
            
            var failPanelAnimator = failPanel.GetComponent<FailPanelAnimator>();
            if (failPanelAnimator != null)
            {
                failPanelAnimator.PlayFailAnimation();
            }
        }
        private void OnLevelWin()
        {
            EnablePanel(winPanel);
            _gamePanel.DeactivatePanel();

            AudioManager.Instance.PlaySFX("WinSFX_1");
            var winPanelAnimator = winPanel.GetComponent<WinPanelAnimator>();
            if (winPanelAnimator != null)
            {
                winPanelAnimator.PlayWinAnimation();
            }
        }
        private void OnReturnToMainMenu()
        {
            DisablePanel(winPanel);
            DisablePanel(failPanel);
            _gamePanel.DeactivatePanel();
            EnablePanel(mainPanel);
            EnablePanel(mainMenuUpperPanel);
            _mainPanel.ActivatePanel();
        }
        private void OnGameStart()
        {
            DisablePanel(winPanel);
            DisablePanel(failPanel);
            DisablePanel(mainMenuUpperPanel);
            _mainPanel.DeactivatePanel();
            DisablePanel(mainPanel);
            _gamePanel.ActivatePanel();
        }
        #endregion
        #region Helper Methods
        private void EnablePanel(GameObject panel)
        {
            if (panel == null)
                return;
            if (!panel.activeSelf)
                panel.SetActive(true);
        }
        private void DisablePanel(GameObject panel)
        {
            if (panel == null)
                return;
            if (panel.activeSelf)
                panel.SetActive(false);
        }
        #endregion
        #region Button Functions
        public void ContinueButton() => GameEvents.TriggerReturnToMainMenu();
        public void MainMenuButton() => GameEvents.TriggerReturnToMainMenu();
        public void RestartLevelButton() => GameEvents.TriggerGameStart();
        #endregion
    }
}
