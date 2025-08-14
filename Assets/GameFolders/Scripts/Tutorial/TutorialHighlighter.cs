using GameFolders.Scripts.Managers;
using UnityEngine;

namespace GameFolders.Scripts.Tutorial
{
    public class TutorialHighlighter : MonoBehaviour
    {
        [SerializeField] private int highlightLevel;

        private static readonly int GlowSpeed = Shader.PropertyToID("_GlowSpeed");
        private static readonly int _GlowRepeatTime = Shader.PropertyToID("_GlowRepeatTime");
        private static readonly int _Brightness = Shader.PropertyToID("_Brightness");
        private Material _material;
        private int _currentLevel;

        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            if (_material == null)
            {
                Debug.LogError("Material not found on the TutorialHighlighter GameObject.");
                return;
            }
            _material.SetFloat(GlowSpeed, 0f);
        }

        private void OnEnable()
        {
            Invoke(nameof(StartHighlighting), 0.1f);

            GameEvents.OnTutorialCompleted += StopHighlighting;
            GameEvents.OnLevelWin += StopHighlighting;
            GameEvents.OnLevelFailed += StopHighlighting;
        }

        private void OnDisable()
        {
            Invoke(nameof(StopHighlighting), 0.1f);

            GameEvents.OnTutorialCompleted -= StopHighlighting;
            GameEvents.OnLevelWin -= StopHighlighting;
            GameEvents.OnLevelFailed -= StopHighlighting;
        }

        private void StartHighlighting()
        {
            _currentLevel = GameManager.Instance.CurrentLevel;
            if (_currentLevel != highlightLevel)
            {
                return;
            }
            _material.SetFloat(GlowSpeed, 1.8f);

            if (GameManager.Instance.CurrentLevel == 2 && gameObject.TryGetComponent(out GarbageItem garbageItem))
            {
                _material.SetFloat(_GlowRepeatTime, 1.5f);
                _material.SetFloat(_Brightness, 2f);
                //_material.SetFloat(GlowSpeed, 0.1f);
            }
            gameObject.layer = LayerMask.NameToLayer("Highlight");
        }

        private void StopHighlighting()
        {
            _material.SetFloat(GlowSpeed, 0f);


            if ((GameManager.Instance.CurrentLevel == 2 || GameManager.Instance.CurrentLevel == 3) && gameObject.TryGetComponent(out GarbageItem garbageItem))
            {
                _material.SetFloat(_GlowRepeatTime, 3.25f);
                _material.SetFloat(_Brightness, 1f);
            }

            gameObject.layer = LayerMask.NameToLayer("Clickable");

        }
    }
}
