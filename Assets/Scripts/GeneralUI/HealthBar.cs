using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider), typeof(CanvasGroup))]
public class HealthBar : MonoBehaviour
{
    private Health health;

    [Header("Fade Settings")]
    [SerializeField] private float delayBeforeFade = 10f; 
    [SerializeField] private float fadeDuration = 1.5f;  

    private Slider slider;
    private CanvasGroup group;
    private Sequence fadeSequence;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        group = GetComponent<CanvasGroup>();
        health = GetComponentInParent<Health>();

        slider.maxValue = health.getMaxHP();
        slider.value = health.getMaxHP();
        group.alpha = 0f;
        group.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        health.on_damaged += SetHealth;
        health.on_health_changed += SetMaxCurrentHP;
    }

    public void SetMaxHP(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        ResetFadeTimer();
    }

    public void SetMaxCurrentHP(int currentHP, int MaxHP)
    {
        slider.maxValue = MaxHP;
        slider.value = currentHP;
        ResetFadeTimer();
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        ResetFadeTimer();
    }

    private void ResetFadeTimer()
    {
        if (fadeSequence.isAlive) {
            fadeSequence.Stop();
        }

        group.alpha = 1f;

        fadeSequence = Sequence.Create()
            .ChainDelay(delayBeforeFade)
            .Chain(Tween.Alpha(group, endValue: 0f, duration: fadeDuration));
    }
}