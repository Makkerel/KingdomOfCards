using UnityEngine;

public class MainCastle : MonoBehaviour
{
    private Health building_health;
    [Header("Audio Keys")]
    [SerializeField] private string explosionSoundKey = "BuildingCollapse";
    [Header("Destruction Assets")]
    [SerializeField] private GameObject explosionVfxPrefab;


    private void Awake()
    {
        building_health = GetComponent<Health>();
        building_health.on_die += Death;
    }

    private void Death(GameObject itself)
    {
        building_health.on_die -= Death;
        AudioManager.Instance.PlayUISound(explosionSoundKey);
        Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
        CombatManager.Instance.WinGame();
    }
}
