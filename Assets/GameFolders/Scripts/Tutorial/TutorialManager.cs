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
        public static TutorialManager Instance { get; private set; }

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

        public bool isTutorialActive0 = true;
        public bool isTutorialActive2 = true;

        // Cache edilen pozisyonlar
        private Vector2 infoBoxOriginalPosition;
        private Vector2 infoBoxStartPosition;
        private RectTransform infoBoxRect;

        private int _currentLevel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            pointerHand.SetActive(false);

            // InfoBox pozisyonlarını cache le
            infoBoxRect = infoBox.GetComponent<RectTransform>();
            infoBoxOriginalPosition = infoBoxRect.anchoredPosition;
            infoBoxStartPosition = new Vector2(infoBoxOriginalPosition.x + 300f, infoBoxOriginalPosition.y - 200f);
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
                    ActivateTutorialCamera("Tap three identical luggages to match them!"); //currentLevel 0 = Level 1
                    pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerLuggagePosition.anchoredPosition;
                    pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                    bombButton.interactable = false;
                    bombButton.transform.GetChild(3).gameObject.SetActive(true);
                    freezeButton.interactable = false;
                    freezeButton.transform.GetChild(3).gameObject.SetActive(true);
                    break;
                case 1:
                    ActivateTutorialCamera("Tap the glowing piece to complete your collection - no matching needed!"); //currentLevel 1 = Level 2
                    pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerLuggagePosition.anchoredPosition;
                    pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                    bombButton.interactable = false;
                    freezeButton.interactable = false;
                    bombButton.transform.GetChild(3).gameObject.SetActive(true);
                    freezeButton.transform.GetChild(3).gameObject.SetActive(true);
                    break;
                case 2:
                    if (tempNum == 0)
                    {
                        bombButton.interactable = false;
                        freezeButton.interactable = false;
                        bombButton.transform.GetChild(3).gameObject.SetActive(true);
                        freezeButton.transform.GetChild(3).gameObject.SetActive(true);
                        ActivateTutorialCamera("Try to collect garbage!", false, true);

                    }
                    if (tempNum == 1)
                    {
                        //StopTutorial();
                        HideInfoBoxAnimated();
                        DOVirtual.DelayedCall(1f, () => ActivateTutorialCamera("Garbage can't be matched. Use dynamite power-up!", true, false));
                        pointerHand.transform.DOScale(Vector3.one, 0.1f);
                        ActivateTutorialObjects();
                        pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerBombPosition.anchoredPosition;
                        pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                        bombButton.onClick.AddListener(DeactivateTutorialObjects);
                        bombButton.interactable = true;
                        bombButton.transform.GetChild(3).gameObject.SetActive(false);
                        freezeButton.transform.GetChild(3).gameObject.SetActive(true);

                        if (GameManager.Instance.CurrentLevel == 2)
                        {
                            isTutorialActive2 = false;
                        }
                        GameEvents.TriggerHighlightCompleted();
                    }
                    tempNum++;
                    break;
                case 3:
                    ActivateTutorialCamera("Use your freeze power to slow down the conveyor belt!");
                    ActivateTutorialObjects();
                    pointerHand.GetComponent<RectTransform>().anchoredPosition = pointerFreezePosition.anchoredPosition;
                    pointerHand.GetComponent<PointerHandAnimation>().PointerAnimation(MoveDirection.Y);
                    freezeButton.onClick.AddListener(DeactivateTutorialObjects);
                    bombButton.onClick.RemoveListener(DeactivateTutorialObjects);
                    freezeButton.interactable = true;
                    bombButton.transform.GetChild(3).gameObject.SetActive(false);
                    freezeButton.transform.GetChild(3).gameObject.SetActive(false);
                    break;
                case 4:
                    ActivateTutorialCamera("Careful! Bombs on the conveyor!", 2f);
                    break;
                case 5:
                    ActivateTutorialCamera("Hourglass on conveyor helps you!", 2f);
                    break;
                default:
                    StopTutorial();
                    break;
            }
        }

        private void ActivateTutorialCamera(string message)
        {
            tutorialText.text = message;
            tutorialText.gameObject.SetActive(true);
            pointerHand.SetActive(true);
            background.color = darkenColor;
            // MeshRenderer'ı etkinleştir
            if (darkenStencil != null && GameManager.Instance.CurrentLevel != 3)
            {
                darkenStencil.enabled = true;
            }
            ShowInfoBoxAnimated();
        }
        private void ActivateTutorialCamera(string message, bool havePointer, bool haveDarken)
        {
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
                if (haveDarken)
                {
                    darkenStencil.enabled = true;
                }
                else
                {
                    darkenStencil.enabled = false;
                }
            }
            ShowInfoBoxAnimated();

        }
        private void ActivateTutorialCamera(string message, float time)
        {
            tutorialText.text = message;
            tutorialText.gameObject.SetActive(true);

            ShowInfoBoxAnimated(() =>
            {
                // Belirtilen süre sonunda kapanma animasyonu
                DOVirtual.DelayedCall(time, () =>
                {
                    HideInfoBoxAnimated();
                });
            });
        }

        private void ShowInfoBoxAnimated(System.Action onComplete = null)
        {
            // InfoBox'ı aktif et
            infoBox.SetActive(true);

            // Başlangıç durumu ayarla (cache'lenmiş değerleri kullan)
            infoBoxRect.anchoredPosition = infoBoxStartPosition;
            infoBox.transform.localScale = Vector3.zero;

            // DOTween ile hem position hem scale animasyonu
            Sequence openSequence = DOTween.Sequence();
            openSequence.Append(infoBoxRect.DOAnchorPos(infoBoxOriginalPosition, 0.5f).SetEase(Ease.OutBack));
            openSequence.Join(infoBox.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

            openSequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        private void HideInfoBoxAnimated(System.Action onComplete = null)
        {
            Sequence closeSequence = DOTween.Sequence();
            closeSequence.Append(infoBoxRect.DOAnchorPos(infoBoxStartPosition, 0.4f).SetEase(Ease.InBack));
            closeSequence.Join(infoBox.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));

            closeSequence.OnComplete(() =>
            {
                infoBox.SetActive(false);
                tutorialText.gameObject.SetActive(false);
                // Pozisyonu orijinal haline getir (bir sonrakine hazırlık)
                infoBoxRect.anchoredPosition = infoBoxOriginalPosition;
                onComplete?.Invoke();
            });
        }
        private void StopTutorial()
        {
            HideInfoBoxAnimated(() =>
            {
                pointerHand.SetActive(false);
                background.color = normalColor;

                // Volume weight'ini 0 yap
                if (darkenStencil != null)
                {
                    darkenStencil.enabled = false;
                }
            });
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
