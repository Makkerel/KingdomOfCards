using System.Collections.Generic;
using UnityEngine;
using StrategyEngine.Modifiers;

public class DamageModifierManager : MonoBehaviour
{
    public static DamageModifierManager Instance;

    private readonly Dictionary<string, List<DamageModifier>> modifierRegistry = new();
    private readonly Dictionary<string, List<DamageModifier>> globalModifiers = new();

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }
    public void AddModifier(string categoryKey, DamageModifier modifier)
    {
        if (!modifierRegistry.TryGetValue(categoryKey, out var modifierList)) {
            modifierList = new List<DamageModifier>();
            modifierRegistry.Add(categoryKey, modifierList);
        }

        modifierList.RemoveAll(m => m.SourceId == modifier.SourceId);
        modifierList.Add(modifier);
    }

    public void RemoveModifier(string categoryKey, string sourceId)
    {
        var modifierList = modifierRegistry[categoryKey];
        modifierList.RemoveAll(m => m.SourceId == sourceId);
    }
    /// <param name="statType">The target stat classification (e.g., "Damage", "MaxHealth", "AttackSpeed").</param>
    /// <param name="modifier">The custom modifier data package.</param>
    public void AddGlobalModifier(string statType, DamageModifier modifier)
    {
        if (!globalModifiers.TryGetValue(statType, out var modifierList)) {
            modifierList = new List<DamageModifier>();
            globalModifiers.Add(statType, modifierList);
        }

        modifierList.RemoveAll(m => m.SourceId == modifier.SourceId);
        modifierList.Add(modifier);

        Debug.Log($"Global Modifier Active for [{statType}]! Source: {modifier.SourceId}, Value: {modifier.Value}");
    }

    /// <summary>
    /// Removes a targeted global modifier using its stat classification and Source ID.
    /// </summary>
    public void RemoveGlobalModifier(string statType, string sourceId)
    {
        var modifierList = globalModifiers[statType];
        modifierList.RemoveAll(m => m.SourceId == sourceId);
        Debug.Log($"Global Modifier Removed from [{statType}]! Source: {sourceId}");
    }
    /// <summary>
    /// Evaluates both local category-specific AND stat-filtered global modifiers against a base value.
    /// </summary>
    public float GetModifiedValue(string categoryKey, float baseValue)
    {
        float sumFlat = 0f;
        float sumPercent = 0f;

        foreach (var kvp in globalModifiers) {
            // If the requested local category (e.g., "Knight_Damage") ends with the global filter ("Damage")
            if (categoryKey.EndsWith(kvp.Key)) {
                List<DamageModifier> globalList = kvp.Value;
                for (int i = 0; i < globalList.Count; i++) {
                    DamageModifier mod = globalList[i];
                    if (mod.Type == ModifierType.Flat) sumFlat += mod.Value;
                    else if (mod.Type == ModifierType.Percent) sumPercent += mod.Value;
                }
            }
        }

        if (modifierRegistry.TryGetValue(categoryKey, out var modifierList)) {
            for (int i = 0; i < modifierList.Count; i++) {
                DamageModifier mod = modifierList[i];
                if (mod.Type == ModifierType.Flat) sumFlat += mod.Value;
                else if (mod.Type == ModifierType.Percent) sumPercent += mod.Value;
            }
        }

        return (baseValue + sumFlat) * (1f + sumPercent);
    }

    public void ResetAllRegistryData()
    {
        modifierRegistry.Clear();
        globalModifiers.Clear();
    }
}