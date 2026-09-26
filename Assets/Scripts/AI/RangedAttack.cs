using UnityEngine;

public class RangedAttack : BaseAttack
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectile_prefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private string attackSound;

    protected override void ExecuteAttack(Transform target)
    {
        AudioManager.Instance.PlaySound3D(attackSound, transform.position);
        GameObject arrow = Instantiate(projectile_prefab, firePoint.position, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.Fire(target.position, damage);
    }
}
