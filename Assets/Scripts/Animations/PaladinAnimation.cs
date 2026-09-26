using PrimeTween;
using UnityEngine;

public class PaladinAnimation : WeaponAnimator
{
    [Header("Sword Settings")]
    [SerializeField] private float totalDuration = 0.4f;
    [SerializeField] private Vector3 windUpPosOffset = new Vector3(0.2f, 0f, -0.1f);
    [SerializeField] private Vector3 windUpRotOffset = new Vector3(0f, 45f, -10f);
    [SerializeField] private Vector3 slashPosOffset = new Vector3(-0.5f, 0f, 0.4f);
    [SerializeField] private Vector3 slashRotOffset = new Vector3(10f, -80f, 30f);

    public override void PlayAttackAnimation()
    {
        if (weaponTransform == null) return;

        ResetWeaponState();

        Vector3 peakWindUpPos = idleLocalPosition + windUpPosOffset;
        Quaternion peakWindUpRot = idleLocalRotation * Quaternion.Euler(windUpRotOffset);
        Vector3 peakSlashPos = idleLocalPosition + slashPosOffset;
        Quaternion peakSlashRot = idleLocalRotation * Quaternion.Euler(slashRotOffset);

        Sequence.Create()
            .Group(Tween.LocalPosition(weaponTransform, peakWindUpPos, totalDuration * 0.25f, Ease.OutQuad))
            .Group(Tween.LocalRotation(weaponTransform, peakWindUpRot, totalDuration * 0.25f, Ease.OutQuad))
            .Chain(Tween.LocalPosition(weaponTransform, peakSlashPos, totalDuration * 0.25f, Ease.InQuad))
            .Group(Tween.LocalRotation(weaponTransform, peakSlashRot, totalDuration * 0.25f, Ease.InQuad))
            .Chain(Tween.LocalPosition(weaponTransform, idleLocalPosition, totalDuration * 0.5f, Ease.OutCubic))
            .Group(Tween.LocalRotation(weaponTransform, idleLocalRotation, totalDuration * 0.5f, Ease.OutCubic));
    }
}
