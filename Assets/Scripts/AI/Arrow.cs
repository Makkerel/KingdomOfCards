using UnityEngine;
public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    private int damage;
    [SerializeField] private int punchThrough = 1;
    [SerializeField] private float speed;
    [SerializeField] private float lifespan = 4f;
    [SerializeField] private float height_offset = 1.5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        Destroy(gameObject, lifespan);
    }

    public void Fire(Vector3 direction, int archer_damage)
    {
        damage = archer_damage;
        transform.LookAt(direction + Vector3.up * height_offset);
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IsDamageable damageable)) {
            damageable.TakeDamage(damage);
        }
        else {
            Destroy(gameObject);
        }
        punchThrough -= 1;
        if (punchThrough <= 0) {
            Destroy(gameObject);
        }
    }
}