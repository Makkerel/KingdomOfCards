using PrimeTween;
using UnityEngine;

public abstract class WeaponAnimator : MonoBehaviour
{
    [Header("Base References")]
    [SerializeField] protected Transform weaponTransform;

    protected Vector3 idleLocalPosition;
    protected Quaternion idleLocalRotation;

    protected virtual void Awake()
    {
        if (weaponTransform != null) {
            idleLocalPosition = weaponTransform.localPosition;
            idleLocalRotation = weaponTransform.localRotation;
        }
    }

    public abstract void PlayAttackAnimation();

    protected void ResetWeaponState()
    {
        if (weaponTransform == null) return;

        Tween.StopAll(weaponTransform);
        weaponTransform.localPosition = idleLocalPosition;
        weaponTransform.localRotation = idleLocalRotation;
    }

}
