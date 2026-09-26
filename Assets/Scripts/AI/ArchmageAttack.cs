using UnityEngine;

public class ArchmageAttack : BaseAttack
{
    [Header("Archmage Projectile Configuration")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;

    protected override void ExecuteAttack(Transform target)
    {
        GameObject fireballInstance = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Fireball fireballScript = fireballInstance.GetComponent<Fireball>();
        fireballScript.Fire(target.position, damage);
    }
}