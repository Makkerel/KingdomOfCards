using UnityEngine;

public class KnightAttack : BaseAttack
{
    [SerializeField] private WeaponAnimator knightAnimation;
    [SerializeField] private string attackSound;
    protected override void ExecuteAttack(Transform target)
    {
        AudioManager.Instance.PlaySound3D(attackSound, transform.position);
        knightAnimation.PlayAttackAnimation();
        if (target.TryGetComponent(out IsDamageable damageable)) {
            damageable.TakeDamage(damage);
        }
    }
}
