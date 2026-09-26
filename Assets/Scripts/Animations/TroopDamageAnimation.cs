using PrimeTween;
using UnityEngine;

public class TroopDamageAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer troopRenderer;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Material flashMaterial;

    [Header("Flash Configuration")]
    [SerializeField] private float flashDuration = 0.12f;

    [Header("Impact Shudder Configuration")]
    [SerializeField] private Vector3 punchStrength = new Vector3(0f, 0.15f, -0.2f);
    [SerializeField] private float punchDuration = 0.18f;

    private Material originalMaterial;
    private Vector3 originalLocalPosition;
    private Health health;

    private Tween flashTween;
    private Tween punchTween;

    private void Awake()
    {
        health = GetComponent<Health>();
        originalMaterial = troopRenderer.sharedMaterial;
        originalLocalPosition = visualRoot.localPosition;
    }

    private void OnEnable()
    {
        health.on_damaged += PlayDamageAnimation;
    }

    private void OnDisable()
    {
        health.on_damaged -= PlayDamageAnimation;
        flashTween.Stop();
        punchTween.Stop();
    }

    public void PlayDamageAnimation(int damage)
    {
        if (!flashTween.isAlive) {
            troopRenderer.sharedMaterial = flashMaterial;

            flashTween = Tween.Delay(
                target: this,
                duration: flashDuration,
                onComplete: script => script.troopRenderer.sharedMaterial = script.originalMaterial,
                warnIfTargetDestroyed: false
            );
        }
        if (!punchTween.isAlive) {
            visualRoot.localPosition = originalLocalPosition;

            punchTween = Tween.PunchLocalPosition(
                target: visualRoot,
                strength: punchStrength,
                duration: punchDuration,
                frequency: 12
            );
        }
    }
}