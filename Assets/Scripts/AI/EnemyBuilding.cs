using UnityEngine;

public class EnemyBuilding : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField] private int additionalEnergy = 1;
    [SerializeField] private int additionalMorale = 15;

    [Header("Destruction Assets")]
    [SerializeField] private GameObject explosionVfxPrefab;

    [Header("Audio Keys")]
    [SerializeField] private string explosionSoundKey = "BuildingExplosion";
    [SerializeField] private string collapseSoundKey = "BuildingCollapse";

    private Health buildingHealth;

    private void Awake()
    {
        buildingHealth = GetComponent<Health>();
        buildingHealth.on_die += Death;
    }

    private void Death(GameObject itself)
    {
        buildingHealth.on_die -= Death;

        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        AudioManager.Instance.PlayUISound(explosionSoundKey);
        AudioManager.Instance.PlayUISound(collapseSoundKey);

        Instantiate(explosionVfxPrefab, spawnPosition, Quaternion.identity);

        CombatManager.Instance.AddMorale(additionalMorale);
        CombatManager.Instance.addGold(additionalEnergy);

        Destroy(gameObject);
    }
}