using StrategyEngine.Modifiers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Church : PhysicalBuilding
{
    [SerializeField] private float[] morale_percentage;

    [Header("Fortification Settings")]
    [SerializeField] private float fortifyMoraleBonus = 0.02f;

    private int fortifyCount = 0;

    public override void initialize(Building building_card)
    {
        building_name = building_card.card_name;
        buildingTiers = new List<BuildingData>(building_card.buildingTiers);
        current_tier = 0;
        fortifyCount = 0;

        ActivateEffect();
    }

    private void OnDestroy()
    {
        DeactivateEffect();
    }

    private void ActivateEffect()
    {
        if (EffectsManager.Instance == null) return;

        float totalContribution = morale_percentage[current_tier] + (fortifyCount * fortifyMoraleBonus);

        EffectsManager.Instance.moraleImprovement(totalContribution);

        int basePercent = Mathf.RoundToInt(morale_percentage[current_tier] * 100f);
        if (fortifyCount > 0) {
            int extraPercent = Mathf.RoundToInt(fortifyCount * fortifyMoraleBonus * 100f);
            SpawnPopUpText($"Morale Decays {basePercent}% (+{extraPercent}% Fortified) Slower Now!", Color.cyan);
        }
        else {
            SpawnPopUpText($"Morale Decays {basePercent}% Slower Now!", Color.cyan);
        }
    }

    private void DeactivateEffect()
    {
        if (EffectsManager.Instance == null) return;
        float totalContribution = morale_percentage[current_tier] + (fortifyCount * fortifyMoraleBonus);

        EffectsManager.Instance.moraleImprovement(-totalContribution);
    }

    public override void Upgrade()
    {
        if (current_tier + 1 >= buildingTiers.Count) {
            Fortify();
            return;
        }

        DeactivateEffect();
        current_tier += 1;
        ActivateEffect();

        AudioManager.Instance.PlayUISound("UpgradeBuilding");

        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        meshFilter.mesh = buildingTiers[current_tier].building_look;
    }

    public override void Fortify()
    {
        fortifyCount++;

        if (EffectsManager.Instance != null) {
            EffectsManager.Instance.moraleImprovement(fortifyMoraleBonus);
        }
        int moraleDisplayPercent = Mathf.RoundToInt(fortifyMoraleBonus * 100f);
        AudioManager.Instance.PlayUISound("FortifyBuilding");
        SpawnPopUpText($"Fortified! Morale Decays {moraleDisplayPercent}% Slower", Color.white);
    }

    public override string getEffects()
    {
        int basePercent = Mathf.RoundToInt(morale_percentage[current_tier] * 100f);
        int fortifyStepPercent = Mathf.RoundToInt(fortifyMoraleBonus * 100f);
        int totalPercent = basePercent + (fortifyCount * fortifyStepPercent);

        StringBuilder sb = new StringBuilder(128);

        sb.Append("Slowing down Morale Decay rate by <b><color=#00E6FF>").Append(totalPercent).Append("%</color></b>");
        sb.Append("\nFortifications: <b>").Append(fortifyCount).Append("</b>");

        if (current_tier + 1 < buildingTiers.Count) {
            int nextTierPercent = Mathf.RoundToInt(morale_percentage[current_tier + 1] * 100f);
            sb.Append("\n<b><color=#FFD700>On Upgrade:</color></b> Slows down Morale Decay rate by <b><color=#00E6FF>").Append(nextTierPercent).Append("%</color></b>");
        }

        sb.Append("\n<color=#FF9800>Fortify Effect:</color>");
        sb.Append("\nReduces Morale Decay speed by an additional <b><color=white>+").Append(fortifyStepPercent).Append("%</color></b> per stack");

        return sb.ToString();
    }
}