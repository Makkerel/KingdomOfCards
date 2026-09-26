using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoraleBarVisual : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider moraleSlider;
    [SerializeField] private RectTransform handleRect;
    [SerializeField] private TextMeshProUGUI moraleStatusText;

    [Header("PrimeTween Settings")]
    [SerializeField] private float jumpDuration = 0.25f;
    [SerializeField] private float popScaleMultiplier = 1.3f;
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private Ease jumpEase = Ease.OutQuad;

    private Vector3 originalFlagScale;
    private Tween sliderTween;
    private Tween flagTween;

    private void Awake() => originalFlagScale = handleRect.localScale;

    public void Initialize(float maxMorale, float startingMorale)
    {
        moraleSlider.maxValue = maxMorale;
        moraleSlider.value = startingMorale;
        SetStatusText("Morale");
    }

    public void UpdateValueDirect(float currentMorale)
    {
        if (!sliderTween.isAlive) moraleSlider.value = currentMorale;
    }

    public void AnimateValueJump(float targetMorale)
    {
        if (sliderTween.isAlive) sliderTween.Stop();

        sliderTween = Tween.Custom(
            target: moraleSlider,
            startValue: moraleSlider.value,
            endValue: targetMorale,
            duration: jumpDuration,
            onValueChange: (slider, val) => slider.value = val,
            ease: jumpEase
        );

        if (flagTween.isAlive) flagTween.Stop();

        flagTween = Tween.Scale(
            target: handleRect,
            endValue: originalFlagScale * popScaleMultiplier,
            duration: popDuration,
            ease: Ease.OutQuad,
            cycles: 2,
            cycleMode: CycleMode.Yoyo
        );
    }

    public void UpdatePanicTimer(float timeRemaining)
    {
        if (moraleStatusText != null && timeRemaining > 0f) {
            moraleStatusText.text = $"Panic: Survive for {Mathf.CeilToInt(timeRemaining)} seconds";
        }
    }
    public void SetStatusText(string newText)
    {
        if (moraleStatusText != null) {
            moraleStatusText.text = newText;
        }
    }
}