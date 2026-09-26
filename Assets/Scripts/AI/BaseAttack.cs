using UnityEngine;

public abstract class BaseAttack : MonoBehaviour
{
    [Header("Base Attack Configuration")]
    public float attack_range = 2.5f;

    public float attack_cooldown => 1f / troopStats.RuntimeAttackSpeed;
    public int damage => Mathf.RoundToInt(troopStats.RuntimeDamage);

    protected float last_attack_time;
    protected TroopStats troopStats;

    protected virtual void Awake()
    {
        troopStats = GetComponent<TroopStats>();
    }
    public virtual bool TryAttack(Transform target)
    {
        if (Time.time < last_attack_time + attack_cooldown)
            return false;

        ExecuteAttack(target);
        last_attack_time = Time.time;
        return true;
    }

    public float IsInRange(Transform target)
    {
        if (target.TryGetComponent(out Collider targetCollider)) {
            Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
            float distance = Vector3.Distance(transform.position, closestPoint);
            return distance;
        }
        return Vector3.Distance(transform.position, target.position);
    }

    protected abstract void ExecuteAttack(Transform target);
}