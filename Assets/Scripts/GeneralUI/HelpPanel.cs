using UnityEngine;
using TMPro;
using PrimeTween;

public class HelpPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI helpTextDisplay;

    [Tooltip("How fast the text types out (characters per second)")]
    [SerializeField] private float typingSpeed = 45f;

    private Tween typewriterTween;

    private const string TutorialSeenKey = "HasSeenInitialTutorial";

    private void Start()
    {
        TutorialManager.Instance.stateChanged += updateText;
    }

    private void OnDisable()
    {
        TutorialManager.Instance.stateChanged -= updateText;
    }

    public void OnHelpButtonPressed()
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        if (typewriterTween.isAlive && typewriterTween.progress < 1f) {
            typewriterTween.Complete();
            return;
        }
        bool isWindowOpen = !panelRoot.activeSelf;
        panelRoot.SetActive(isWindowOpen);
        if (isWindowOpen) {
            StartTypewriterText();
        }
        else {
            typewriterTween.Stop();
        }
    }

    private void updateText()
    {
        if (panelRoot.activeSelf) {
            typewriterTween.Stop();
            StartTypewriterText();
        }
        else if (TutorialManager.Instance.GetGamePhase() == GamePhase.GameStart) {
            if (PlayerPrefs.GetInt(TutorialSeenKey, 0) == 0) {
                panelRoot.SetActive(true);
                StartTypewriterText();
                MarkTutorialAsSeen();
            }
        }
    }

    private void StartTypewriterText()
    {
        string fullText = TutorialManager.Instance.getExplanation();
        helpTextDisplay.text = fullText;

        helpTextDisplay.maxVisibleCharacters = 0;
        float duration = fullText.Length / typingSpeed;

        typewriterTween = Tween.TextMaxVisibleCharacters(
            target: helpTextDisplay,
            startValue: 0,
            endValue: fullText.Length,
            duration: duration,
            ease: Ease.Linear
        );
    }

    private void MarkTutorialAsSeen()
    {
        PlayerPrefs.SetInt(TutorialSeenKey, 1);
        PlayerPrefs.Save();
    }
}