using UnityEngine;
using System.Collections.Generic;

public abstract class PhysicalBuilding : MonoBehaviour
{
    [Header("Base Building Settings")]
    protected string building_name;
    protected int current_tier;
    protected List<BuildingData> buildingTiers;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform textSpawnPoint;

    [SerializeField] private string explosionSoundKey = "BuildingExplosion";
    [Header("Destruction Assets")]
    [SerializeField] private GameObject explosionVfxPrefab;
    private Health buildingHealth;

    private void Awake()
    {
        buildingHealth = GetComponent<Health>();
        buildingHealth.on_die += Death;
    }

    private void Death(GameObject itself)
    {
        buildingHealth.on_die -= Death;
        AudioManager.Instance.PlayUISound(explosionSoundKey);
        Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    /// <summary>
    /// ABSTRACT FUNCTION: Every unique building script must define its own setup rules
    /// when spawned into the action phase grid.
    /// </summary>
    public abstract void initialize(Building building_card);

    /// <summary>
    /// ABSTRACT FUNCTION: Every unique building script must handle its own distinct 
    /// advancement parameters, phase changes, or tier-specific component lookups.
    /// </summary>
    public abstract void Upgrade();

    /// <summary>
    /// ABSTRACT FUNCTION: Every unique building script must define what special 
    /// mechanical, economic, or strategic bonus fires when it triggers fortification.
    /// </summary>
    public abstract void Fortify();

    /// <summary>
    /// ABSTRACT FUNCTION: Maintained from previous framework requirements.
    /// </summary>
    public abstract string getEffects();

    public int getTier()
    {
        return current_tier + 1;
    }

    public string getName()
    {
        return building_name;
    }

    public void SpawnPopUpText(string message, Color textColor, float textTime = 2.8f)
    {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPos = textSpawnPoint != null ? textSpawnPoint.position : transform.position + Vector3.up * 3f;
        GameObject textObj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);

        if (textObj.TryGetComponent<FloatingText>(out var floatingText)) {
            floatingText.Setup(message, textColor, textTime);
        }
    }
}