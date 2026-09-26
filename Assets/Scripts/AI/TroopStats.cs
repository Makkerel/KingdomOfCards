using UnityEngine;

public class TroopStats : MonoBehaviour
{
    [Header("Faction Settings")]
    [Tooltip("If true, this unit bypasses the player's global/local modifier registries entirely.")]
    [SerializeField] private bool isEnemy = false;

    [Header("Troop Identity")]
    public string troopClassName = "Knight";

    [Header("Hardcoded Prefab Defaults")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseMaxHealth = 50f;

    private float hpBonus = 1f;
    public float RuntimeDamage { get; private set; }
    public float RuntimeAttackSpeed { get; private set; }

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Start()
    {
        ApplyCalculatedStats();
    }

    public void SetAbsoluteUnitHPBonus(float percentage)
    {
        hpBonus = percentage;
        ApplyCalculatedStats();
    }

    public void ApplyCalculatedStats()
    {
        if (isEnemy) {
            RuntimeDamage = baseDamage;
            RuntimeAttackSpeed = baseAttackSpeed;
            health.initializeHealth(Mathf.RoundToInt(baseMaxHealth * hpBonus));
            return;
        }

        RuntimeDamage = DamageModifierManager.Instance.GetModifiedValue($"{troopClassName}_Damage", baseDamage);
        RuntimeAttackSpeed = DamageModifierManager.Instance.GetModifiedValue($"{troopClassName}_AttackSpeed", baseAttackSpeed);

        float modifiedMaxHealth = DamageModifierManager.Instance.GetModifiedValue($"{troopClassName}_Health", baseMaxHealth);
        if(TurnManager.Instance.cheatOn) {
            health.initializeHealth(50000);
        }
        else {
            health.initializeHealth(Mathf.RoundToInt(modifiedMaxHealth * hpBonus));
        }
    }
}