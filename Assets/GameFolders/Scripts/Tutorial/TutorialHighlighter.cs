using System.Collections.Generic;
using UnityEngine;
using GameFolders.Scripts;
using GameFolders.Scripts.Managers;

public class TutorialHighlighter : MonoBehaviour
{
    [SerializeField] private int highlightLevel;
    private int currentLevel;

    void OnEnable()
    {
        Invoke(nameof(StartHighlighting), 0.1f);

        //     GameEvents.OnLevelFailed += StopHighlighting;
        //     GameEvents.OnLevelRestarted += StopHighlighting;
        //     GameEvents.OnLevelWin += StopHighlighting;
        //     GameEvents.OnReturnToMainMenu += StopHighlighting;
    }

    // private void OnDisable()
    // {

    //     // GameEvents.OnLevelFailed -= StopHighlighting;
    //     // GameEvents.OnLevelRestarted -= StopHighlighting;
    //     // GameEvents.OnLevelWin -= StopHighlighting;
    //     // GameEvents.OnReturnToMainMenu -= StopHighlighting;
    // }

    private void StartHighlighting()
    {
        currentLevel = GameManager.Instance.CurrentLevel;

        if (currentLevel != highlightLevel)
        {
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("Highlight");

    }

    // private void StopHighlighting()
    // {
    //     if (currentLevel == highlightLevel)
    //     {
    //         gameObject.layer = LayerMask.NameToLayer("Clickable");
    //     }
    // }
}
