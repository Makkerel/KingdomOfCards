using StrategyEngine.Modifiers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Blacksmith : PhysicalBuilding
{
    [SerializeField] private float[] damageModifiers;

    [Header("Fortification Settings")]
    [Tooltip("The fractional damage bonus added per fortification. e.g., 0.02 = +2% damage.")]
    [SerializeField] private float fortifyDamageBonus = 0.02f;

    private int fortifyCount = 0;
    private string uniqueSourceId;

    public override void initialize(Building building_card)
    {
        building_name = building_card.card_name;
        buildingTiers = new List<BuildingData>(building_card.buildingTiers);
        current_tier = 0;
        fortifyCount = 0;

        uniqueSourceId = $"Blacksmith_{gameObject.GetInstanceID()}";

        ActivateBlacksmithEffect();
    }

    private void OnDestroy()
    {
        DeactivateBlacksmithEffect();
    }

    private void ActivateBlacksmithEffect()
    {
        if (DamageModifierManager.Instance == null) return;

        float totalContribution = damageModifiers[current_tier] + (fortifyCount * fortifyDamageBonus);
        DamageModifier blacksmithBuff = new DamageModifier(uniqueSourceId, ModifierType.Percent, totalContribution);

        DamageModifierManager.Instance.AddGlobalModifier("Damage", blacksmithBuff);

        int basePercent = Mathf.RoundToInt(damageModifiers[current_tier] * 100f);
        if (fortifyCount > 0) {
            int extraPercent = Mathf.RoundToInt(fortifyCount * fortifyDamageBonus * 100f);
            SpawnPopUpText($"+{basePercent}% (+{extraPercent}% Fortified) Army Damage!", Color.red);
        }
        else {
            SpawnPopUpText($"+{basePercent}% Army Damage!", Color.red);
        }
    }
    private void DeactivateBlacksmithEffect()
    {
        if (!string.IsNullOrEmpty(uniqueSourceId)) {
            DamageModifierManager.Instance.RemoveGlobalModifier("Damage", uniqueSourceId);
        }
    }

    public override void Upgrade()
    {
        if (current_tier + 1 >= buildingTiers.Count) {
            Fortify();
            return;
        }
        DeactivateBlacksmithEffect();
        current_tier += 1;
        ActivateBlacksmithEffect();

        AudioManager.Instance.PlayUISound("UpgradeBuilding");

        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        meshFilter.mesh = buildingTiers[current_tier].building_look;
    }

    public override void Fortify()
    {
        fortifyCount++;

        if (DamageModifierManager.Instance != null) {
            float totalContribution = damageModifiers[current_tier] + (fortifyCount * fortifyDamageBonus);
            DamageModifier blacksmithBuff = new DamageModifier(uniqueSourceId, ModifierType.Percent, totalContribution);
            DamageModifierManager.Instance.AddGlobalModifier("Damage", blacksmithBuff);
        }

        AudioManager.Instance.PlayUISound("FortifyBuilding");
        int displayPercent = Mathf.RoundToInt(fortifyDamageBonus * 100f);
        SpawnPopUpText($"Fortified! +{displayPercent}% Army Damage", Color.white);
    }

    public override string getEffects()
    {
        int basePercent = Mathf.RoundToInt(damageModifiers[current_tier] * 100f);
        int fortifyStepPercent = Mathf.RoundToInt(fortifyDamageBonus * 100f);
        int totalPercent = basePercent + (fortifyCount * fortifyStepPercent);

        StringBuilder sb = new StringBuilder(128);

        sb.Append("Increases global army damage by <b><color=red>+").Append(totalPercent).Append("%</color></b>");
        sb.Append("\nFortifications: <b>").Append(fortifyCount).Append("</b>");

        if (current_tier + 1 < buildingTiers.Count) {
            int nextTierPercent = Mathf.RoundToInt(damageModifiers[current_tier + 1] * 100f);
            sb.Append("\n<b><color=#FFD700>On Upgrade:</color></b> Increases global army damage by <b><color=red>+").Append(nextTierPercent).Append("%</color></b>");
        }

        sb.Append("\n<color=orange>Fortify Effect:</color>");
        sb.Append("\nGrants an additional <b><color=white>+").Append(fortifyStepPercent).Append("%</color></b> damage per stack");

        return sb.ToString();
    }
}