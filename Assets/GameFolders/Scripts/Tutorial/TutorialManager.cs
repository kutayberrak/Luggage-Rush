using UnityEngine;
using TMPro;
using GameFolders.Scripts;
using GameFolders.Scripts.Managers;
using UnityEngine.Rendering;

public enum TutorialLevels
{
    Level1,
    Level2,
    Level3
}
public class TutorialManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private MeshRenderer darkenStencil;
    private bool isDarkened = false;
    private int currentLevel;
    void OnEnable()
    {
        GameEvents.OnGameStart += StartTutorial;
        GameEvents.OnLevelFailed += StopTutorial;
        GameEvents.OnLevelRestarted += StopTutorial;
        GameEvents.OnLevelWin += StopTutorial;
        GameEvents.OnReturnToMainMenu += StopTutorial;
    }

    void OnDisable()
    {
        GameEvents.OnGameStart -= StartTutorial;
        GameEvents.OnLevelFailed -= StopTutorial;
        GameEvents.OnLevelRestarted -= StopTutorial;
        GameEvents.OnLevelWin -= StopTutorial;
        GameEvents.OnReturnToMainMenu -= StopTutorial;
    }

    private void StartTutorial()
    {
        currentLevel = GameManager.Instance.CurrentLevel;

        TutorialLevels tutorialLevel = (TutorialLevels)currentLevel;
        switch (tutorialLevel)
        {

            case TutorialLevels.Level1:
                ActivateTutorialCamera("Level1 Tutorial");
                break;
            case TutorialLevels.Level2:
                ActivateTutorialCamera("Level2 Tutorial");
                break;
            case TutorialLevels.Level3:
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

        // Volume weight'ini 0 yap
        if (darkenStencil != null)
        {
            darkenStencil.enabled = false;
        }

        isDarkened = false;
    }
}
