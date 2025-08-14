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
        [SerializeField] private Image background;
        private Color darkenColor = new Color(0.3f, 0.3f, 0.3f);
        private Color normalColor = Color.white;

        private int tempNum = 0;


        private int _currentLevel;

        private void Start()
        {
            pointerHand.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += StartTutorial;
            GameEvents.OnTutorialStarted += StartTutorial;
            GameEvents.OnTutorialCompleted += StopTutorial;
            GameEvents.OnLevelFailed += StopTutorial;
            GameEvents.OnLevelRestarted += StopTutorial;
            GameEvents.OnLevelWin += StopTutorial;
            GameEvents.OnReturnToMainMenu += StopTutorial;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= StartTutorial;
            GameEvents.OnTutorialStarted -= StartTutorial;

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
                    if (tempNum == 0)
                    {
                        ActivateTutorialCamera("Try to collect garbage", false);
                    }
                    if (tempNum == 1)
                    {
                        StopTutorial();
                        ActivateTutorialCamera("Garbage can't be matched. Use bomb power-up!");
                        pointerHand.transform.DOScale(Vector3.one, 0.1f);
                        ActivateTutorialObjects();
                        pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerBombPosition.anchoredPosition;
                        pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                        bombButton.onClick.AddListener(DeactivateTutorialObjects);
                        bombButton.interactable = true;

                    }
                    tempNum++;
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
            background.color = darkenColor;
            // MeshRenderer'ı etkinleştir
            if (darkenStencil != null && GameManager.Instance.CurrentLevel != 3)
            {
                darkenStencil.enabled = true;
            }
        }
        private void ActivateTutorialCamera(string message, bool havePointer)
        {
            //  tutorialCamera.SetActive(true);
            infoBox.SetActive(true);
            tutorialText.text = message;
            tutorialText.gameObject.SetActive(true);
            if (havePointer)
            {
                pointerHand.SetActive(true);
            }
            else
            {
                pointerHand.SetActive(false);
            }
            background.color = darkenColor;
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
            background.color = normalColor;

            // Volume weight'ini 0 yap
            if (darkenStencil != null)
            {
                darkenStencil.enabled = false;
            }
        }

        private void DeactivateTutorialObjects()
        {
            GameEvents.TriggerTutorialCompleted();
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
