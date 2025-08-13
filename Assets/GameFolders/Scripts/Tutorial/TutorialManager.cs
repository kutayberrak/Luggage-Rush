using UnityEngine;
using TMPro;
using GameFolders.Scripts;
using GameFolders.Scripts.Managers;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class TutorialManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private MeshRenderer darkenStencil;
    [SerializeField] private Image backgroundImage;
    private bool isDarkened = false;
    private int currentLevel;
    private Color darkenColor = new Color(0.3f, 0.3f, 0.3f);
    private Color normalColor = Color.white;
    void OnEnable()
    {
        GameEvents.OnGameStart += StartTutorial;

        GameEvents.OnTutorialCompleted += StopTutorial;

        GameEvents.OnLevelFailed += StopTutorial;
        GameEvents.OnLevelRestarted += StopTutorial;
        GameEvents.OnLevelWin += StopTutorial;
        GameEvents.OnReturnToMainMenu += StopTutorial;
    }

    void OnDisable()
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
        currentLevel = GameManager.Instance.CurrentLevel;

        switch (currentLevel)
        {

            case 0:
                ActivateTutorialCamera("Level1 Tutorial"); //currentLevel 0 = Level 1
                break;
            case 1:
                ActivateTutorialCamera("Level2 Tutorial"); //currentLevel 1 = Level 2
                break;
            case 2:
                ActivateTutorialCamera("Level3 Tutorial");
                break;
            default:
                StopTutorial();
                break;
        }
    }

    private void ActivateTutorialCamera(string message)
    {
        //  tutorialCamera.SetActive(true);
        tutorialText.text = message;
        tutorialText.gameObject.SetActive(true);
        backgroundImage.color = darkenColor;

        // MeshRenderer'ı etkinleştir
        if (darkenStencil != null)
        {
            darkenStencil.enabled = true;
        }

        isDarkened = true;
    }
    private void StopTutorial()
    {
        // tutorialCamera.SetActive(false);
        tutorialText.gameObject.SetActive(false);
        backgroundImage.color = normalColor;
        // Volume weight'ini 0 yap
        if (darkenStencil != null)
        {
            darkenStencil.enabled = false;
        }

        isDarkened = false;
    }
}
