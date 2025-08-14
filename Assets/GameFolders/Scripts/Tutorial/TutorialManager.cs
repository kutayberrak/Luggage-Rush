using DG.Tweening;
using GameFolders.Scripts.Managers;
using GameFolders.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameFolders.Scripts.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI tutorialText;
        [SerializeField] private MeshRenderer darkenStencil;
        
        [Header("Tutorial Objects")]
        [SerializeField] private Button bombButton;
        [SerializeField] private Button freezeButton;
        
        [Header("Tutorial Items")]
        [SerializeField] private GameObject pointerHand;
        [SerializeField] private GameObject infoBox;

        [Header("Settings")] 
        [SerializeField] private RectTransform pointerBombPosition;
        [SerializeField] private RectTransform pointerLuggagePosition;
        [SerializeField] private RectTransform pointerFreezePosition;
        
        
        private int _currentLevel;
        
        private void Start()
        {
            pointerHand.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += StartTutorial;
            GameEvents.OnTutorialCompleted += StopTutorial;
            GameEvents.OnLevelFailed += StopTutorial;
            GameEvents.OnLevelRestarted += StopTutorial;
            GameEvents.OnLevelWin += StopTutorial;
            GameEvents.OnReturnToMainMenu += StopTutorial;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= StartTutorial;
            GameEvents.OnTutorialCompleted -= StopTutorial;
            GameEvents.OnLevelFailed -= StopTutorial;
            GameEvents.OnLevelRestarted -= StopTutorial;
            GameEvents.OnLevelWin -= StopTutorial;
            GameEvents.OnReturnToMainMenu -= StopTutorial;
        }

        private void StartTutorial()
        {
            _currentLevel = GameManager.Instance.CurrentLevel;

            switch (_currentLevel)
            {
                case 0:
                    ActivateTutorialCamera("Tap the target luggage to complete level "); //currentLevel 0 = Level 1
                    pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerLuggagePosition.anchoredPosition;
                    pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                    bombButton.interactable = false;
                    freezeButton.interactable = false;
                    break;
                case 1:
                    ActivateTutorialCamera("To complete your collection, pick up the collection piece that glowing"); //currentLevel 1 = Level 2
                    pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerLuggagePosition.anchoredPosition;
                    pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                    bombButton.interactable = false;
                    freezeButton.interactable = false;
                    break;
                case 2:
                    ActivateTutorialCamera("Use your bomb power to clean slot from garbage");
                    pointerHand.transform.DOScale(Vector3.one, 0.1f);
                    ActivateTutorialObjects();
                    pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerBombPosition.anchoredPosition;
                    pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                    bombButton.onClick.AddListener(DeactivateTutorialObjects);
                    bombButton.interactable = true;
                    break;
                case 3:
                    ActivateTutorialCamera("Use your freeze power to slow down the conveyor belt");
                    ActivateTutorialObjects();
                    pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerFreezePosition.anchoredPosition;
                    pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                    freezeButton.onClick.AddListener(DeactivateTutorialObjects);
                    bombButton.onClick.RemoveListener(DeactivateTutorialObjects);
                    freezeButton.interactable = true;
                    break;
                default:
                    StopTutorial();
                    break;
            }
        }

        private void ActivateTutorialCamera(string message)
        {
            //  tutorialCamera.SetActive(true);
            infoBox.SetActive(true);
            tutorialText.text = message;
            tutorialText.gameObject.SetActive(true);
            pointerHand.SetActive(true);

            // MeshRenderer'ı etkinleştir
            if (darkenStencil != null)
            {
                darkenStencil.enabled = true;
            }
        }
        private void StopTutorial()
        {
            // tutorialCamera.SetActive(false);
            infoBox.SetActive(false);
            tutorialText.gameObject.SetActive(false);
            pointerHand.SetActive(false);
            // Volume weight'ini 0 yap
            if (darkenStencil != null)
            {
                darkenStencil.enabled = false;
            }
        }

        private void DeactivateTutorialObjects()
        {
            infoBox.transform.DOScale(Vector3.zero, 0.1f).OnComplete(() => infoBox.SetActive(false));
            pointerHand.transform.DOScale(Vector3.zero, 0.1f).OnComplete(() => pointerHand.SetActive(false));
        }

        private void ActivateTutorialObjects()
        {
            infoBox.SetActive(true);
            infoBox.transform.DOScale(Vector3.one, 0.1f);
            pointerHand.SetActive(true);
            pointerHand.transform.DOScale(Vector3.one * 5, 0.1f);
        }
    }
}
