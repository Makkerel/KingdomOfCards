using System;
using UnityEngine;

public interface IsDamageable
{
    void TakeDamage(int amount);
}

public class Health : MonoBehaviour, IsDamageable
{
    private HealthBar healthBar;

    [Header("Health")]
    [SerializeField] private int max_health = 100;
    public int current_health { get; private set; }
    public bool is_dead { get; private set; }

    public event Action<int, int> on_health_changed; 
    public event Action<int> on_damaged;            
    public event Action<GameObject> on_die;

    private void Awake()
    {
        current_health = max_health;
        is_dead = false;
    }

    public int getMaxHP()
    {
        return max_health;
    }

    public void initializeHealth(int amount)
    {
        max_health = amount;
        current_health = max_health;
    }

    public void increaseMaxHP(int amount)
    {
        max_health += amount;
    }

    public void TakeDamage(int amount)
    {
        if (is_dead || amount <= 0) return;

        current_health -= amount;

        on_damaged?.Invoke(amount);
        on_health_changed?.Invoke(current_health, max_health);

        if (current_health <= 0) {
            Die();
        }
    }
    public void Heal(int amount)
    {
        if (is_dead|| amount <= 0) return;

        current_health = Mathf.Clamp(current_health + amount, 0, max_health);
        on_health_changed?.Invoke(current_health, max_health);
    }

    public void setHP(int amount){
        if (is_dead || amount <= 0) return;
        int oldhealth = current_health;
        current_health = Mathf.Clamp(amount, 0, max_health);
        if(oldhealth > current_health) {
            on_damaged?.Invoke(oldhealth - current_health);
        }
        on_health_changed?.Invoke(current_health, max_health);
    }

    private void Die()
    {
        is_dead = true;
        on_die?.Invoke(gameObject);
        Destroy(gameObject);
    }
}
