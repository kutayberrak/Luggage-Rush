using System;
using GameFolders.Scripts.Interfaces;
using UnityEngine;

namespace GameFolders.Scripts.UI.Panels
{
    public class PanelManager : MonoBehaviour
    {
        [Header("Panels")] 
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject mainPanel;

        private IAnimatedUI _gamePanel;
        private IAnimatedUI _mainPanel;

        private void Awake()
        {
            _gamePanel = gamePanel.GetComponent<IAnimatedUI>();
            _mainPanel = mainPanel.GetComponent<IAnimatedUI>();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += EnableGamePanel;
            GameEvents.OnGameStart += DisableMainPanel;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= EnableGamePanel;
            GameEvents.OnGameStart -= DisableMainPanel;
        }

        private void EnableGamePanel()
        {
            if (!gamePanel.activeSelf)
            {
                gamePanel.SetActive(true);
            }
        }
        private void DisableGamePanel()
        {
        }
        private void EnableMainPanel()
        {
        }
        private void DisableMainPanel()
        {
            if (mainPanel.activeSelf)
            {
                mainPanel.SetActive(false);
            }
        }
    }
}
