using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 15f;

    [Header("Explosion & AOE")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float aoeRadius = 3.5f;
    [SerializeField] private LayerMask damageableLayers;

    private int damage;
    private Vector3 moveDirection;
    private bool exploded;

    public void Fire(Vector3 targetPosition, int incomingDamage)
    {
        damage = incomingDamage;

        moveDirection = (targetPosition - transform.position).normalized;

        if (moveDirection != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;
        exploded = true;
        Explode();
    }

    private void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius, damageableLayers);
        foreach (Collider col in hitColliders) {
            if (col.TryGetComponent(out IsDamageable targetHealth)) {
                targetHealth.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}