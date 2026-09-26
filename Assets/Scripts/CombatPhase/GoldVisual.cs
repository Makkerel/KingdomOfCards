using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GoldVisual : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform goldContainerPanel;
    [SerializeField] private CanvasGroup[] coinCanvasGroups = new CanvasGroup[10];

    [Header("PrimeTween Animation Settings")]
    [SerializeField] private float shakeStrength = 15f;      
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private int shakeFrequency = 20;
    [SerializeField] private float spendStaggerDelay = 0.05f;

    private Vector2 originalAnchoredPosition;
    private Tween shakeTween;
    private List<Tween> activeCoinTweens = new();

    private void Awake()
    {
        originalAnchoredPosition = goldContainerPanel.anchoredPosition;
    }

    public void RefreshGridDirect(int currentGold)
    {
        ClearActiveCoinAnimations();

        for (int i = 0; i < coinCanvasGroups.Length; i++) {
            if (coinCanvasGroups[i] == null) continue;

            coinCanvasGroups[i].alpha = (i < currentGold) ? 1f : 0f;
        }
    }

    public void UpdateRegenProgress(int currentGold, float progress)
    {
        if (currentGold >= coinCanvasGroups.Length || currentGold < 0) return;
        coinCanvasGroups[currentGold].alpha = progress;
    }

    public void AnimateSpendCascade(int goldBeforeSpend, int cost, float progress)
    {
        ClearActiveCoinAnimations();

        int newGold = goldBeforeSpend - cost;
        if(goldBeforeSpend != 10) {
            coinCanvasGroups[goldBeforeSpend].alpha = 0f;
        }

        //Fade out ONLY the coins being completely removed
        int startIdx = goldBeforeSpend - 1;
        int endIdx = newGold + 1;
        float runningDelay = 0f;

        for (int i = startIdx; i >= endIdx; i--) {
            if (i < 0 || i >= coinCanvasGroups.Length || coinCanvasGroups[i] == null) continue;

            int index = i;

            Tween fadeTween = Tween.Alpha(
                target: coinCanvasGroups[index],
                endValue: 0f,
                duration: 0.12f,
                ease: Ease.OutQuad,
                startDelay: runningDelay
            );

            activeCoinTweens.Add(fadeTween);
            runningDelay += spendStaggerDelay;
        }
        coinCanvasGroups[newGold].alpha = progress;
    }
    public void PlayDenialJuice()
    {
        if (shakeTween.isAlive) shakeTween.Stop();

        if (goldContainerPanel != null) {
            ShakeSettings shakeSettings = new ShakeSettings
            {
                strength = new Vector3(shakeStrength, 0f, 0f), 
                duration = shakeDuration,
                frequency = shakeFrequency,
                enableFalloff = true 
            };

            shakeTween = Tween.ShakeCustom(
                target: goldContainerPanel,
                startValue: (Vector3)originalAnchoredPosition,
                settings: shakeSettings,
                onValueChange: (rect, val) => rect.anchoredPosition = val
            );
        }
    }
    private void ClearActiveCoinAnimations()
    {
        foreach (var tween in activeCoinTweens) {
            if (tween.isAlive) tween.Stop();
        }
        activeCoinTweens.Clear();
    }

}
