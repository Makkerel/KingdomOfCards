using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Marketplace : PhysicalBuilding
{
    [SerializeField] private float[] goldSpeedModifiers;

    [Header("Fortification Settings")]
    [SerializeField] private float fortifyGoldBonus = 0.02f;

    private int fortifyCount = 0;

    public override void initialize(Building building_card)
    {
        building_name = building_card.card_name;
        buildingTiers = new List<BuildingData>(building_card.buildingTiers);
        current_tier = 0;
        fortifyCount = 0;

        ActivateMarketEffect();
    }

    private void OnDestroy()
    {
        DeactivateMarketEffect();
    }

    private void ActivateMarketEffect()
    {
        if (CombatManager.Instance == null) return;
        float totalContribution = goldSpeedModifiers[current_tier] + (fortifyCount * fortifyGoldBonus);

        CombatManager.Instance.changeGoldRate(totalContribution);

        int basePercent = Mathf.RoundToInt(goldSpeedModifiers[current_tier] * 100f);
        if (fortifyCount > 0) {
            int extraPercent = Mathf.RoundToInt(fortifyCount * fortifyGoldBonus * 100f);
            SpawnPopUpText($"+{basePercent}% (+{extraPercent}% Fortified) Gold Speed!", Color.yellow);
        }
        else {
            SpawnPopUpText($"+{basePercent}% Gold Speed!", Color.yellow);
        }
    }

    private void DeactivateMarketEffect()
    {
        if (CombatManager.Instance == null) return;

        float totalContribution = goldSpeedModifiers[current_tier] + (fortifyCount * fortifyGoldBonus);

        CombatManager.Instance.changeGoldRate(-totalContribution);
    }

    public override void Upgrade()
    {
        if (current_tier + 1 >= buildingTiers.Count) {
            Fortify();
            return;
        }

        DeactivateMarketEffect();
        current_tier += 1;
        ActivateMarketEffect();

        AudioManager.Instance.PlayUISound("UpgradeBuilding");

        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        meshFilter.mesh = buildingTiers[current_tier].building_look;
    }
    public override void Fortify()
    {
        fortifyCount++;

        if (CombatManager.Instance != null) {
            CombatManager.Instance.changeGoldRate(fortifyGoldBonus);
        }

        int economyPercent = Mathf.RoundToInt(fortifyGoldBonus * 100f);
        AudioManager.Instance.PlayUISound("FortifyBuilding");
        SpawnPopUpText($"Fortified! +{economyPercent}% Gold Speed", Color.white);
    }


    public override string getEffects()
    {
        int basePercent = Mathf.RoundToInt(goldSpeedModifiers[current_tier] * 100f);
        int fortifyStepPercent = Mathf.RoundToInt(fortifyGoldBonus * 100f);
        int totalPercent = basePercent + (fortifyCount * fortifyStepPercent);

        StringBuilder sb = new StringBuilder(128);

        sb.Append("Generates gold <b><color=yellow>+").Append(totalPercent).Append("%</color></b> faster");
        sb.Append("\nFortifications: <b>").Append(fortifyCount).Append("</b>");

        if (current_tier + 1 < buildingTiers.Count) {
            int nextTierPercent = Mathf.RoundToInt(goldSpeedModifiers[current_tier + 1] * 100f);
            sb.Append("\n<b><color=#FFD700>On Upgrade:</color></b> Generates gold <b><color=yellow>+").Append(nextTierPercent).Append("%</color></b> faster");
        }

        sb.Append("\n<color=orange>Fortify Effect:</color>");
        sb.Append("\nIncreases gold generation speed by an additional <b><color=white>+").Append(fortifyStepPercent).Append("%</color></b> per stack");

        return sb.ToString();
    }
}