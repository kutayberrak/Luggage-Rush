using Cysharp.Threading.Tasks;
using GameFolders.Scripts.Interfaces;
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
        }

        private async void ActivateMainPanel()
        {
            mainPanel.SetActive(true);
            await UniTask.DelayFrame(1);
            mainPanel.GetComponent<IAnimatedUI>().ActivatePanel();
            gamePanel.GetComponent<IAnimatedUI>().DeactivatePanel();
            await UniTask.DelayFrame(1);
            gamePanel.SetActive(false);
        }

        private async void ActivateGamePanel()
        {
            gamePanel.SetActive(true);
            await UniTask.DelayFrame(1);
            gamePanel.GetComponent<IAnimatedUI>().ActivatePanel();
            mainPanel.GetComponent<IAnimatedUI>().DeactivatePanel();
            await UniTask.DelayFrame(1);
            mainPanel.SetActive(false);
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= ActivateGamePanel;
        }
    }
}
