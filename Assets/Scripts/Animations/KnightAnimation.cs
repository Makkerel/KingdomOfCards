using UnityEngine;
using PrimeTween;

public class KnightAnimation : WeaponAnimator
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Animation Deltas")]
    [SerializeField] private Vector3 strikePositionDelta = new Vector3(0f, 0f, 0.6f);
    [SerializeField] private Vector3 strikeRotationDelta = new Vector3(30f, 0f, 0f);

    public override void PlayAttackAnimation()
    {
        if (weaponTransform == null) return;

        Tween.StopAll(weaponTransform);

        weaponTransform.localPosition = idleLocalPosition;
        weaponTransform.localRotation = idleLocalRotation;

        Vector3 targetPosition = idleLocalPosition + strikePositionDelta;
        Quaternion targetRotation = idleLocalRotation * Quaternion.Euler(strikeRotationDelta);

        Sequence.Create()
            .Group(Tween.LocalPosition(weaponTransform, targetPosition, animationDuration * 0.4f, Ease.OutQuad))
            .Group(Tween.LocalRotation(weaponTransform, targetRotation, animationDuration * 0.4f, Ease.OutQuad))
            .Chain(Tween.LocalPosition(weaponTransform, idleLocalPosition, animationDuration * 0.6f, Ease.InQuad))
            .Chain(Tween.LocalRotation(weaponTransform, idleLocalRotation, animationDuration * 0.6f, Ease.InQuad));
    }
}