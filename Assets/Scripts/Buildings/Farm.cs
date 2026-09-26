using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Farm : PhysicalBuilding
{
    [SerializeField] private int[] decrees;

    [Header("Fortification Settings")]
    [SerializeField] private int fortifyHpBonus = 25;

    public override void initialize(Building building_card)
    {
        building_name = building_card.card_name;
        buildingTiers = new List<BuildingData>(building_card.buildingTiers);
        current_tier = 0;
        ActivateFarmEffect();
    }

    private void OnDestroy()
    {
        DeactivateFarmEffect();
    }

    private void ActivateFarmEffect()
    {
        EffectsManager.Instance.addActions(decrees[current_tier]);
        SpawnPopUpText($"+{decrees[current_tier]} Decree Income!", Color.green);
    }

    private void DeactivateFarmEffect()
    {
        EffectsManager.Instance.addActions(-decrees[current_tier]);
    }

    public override void Upgrade()
    {
        if (current_tier + 1 >= buildingTiers.Count) {
            Fortify();
            return;
        }

        DeactivateFarmEffect();
        current_tier += 1;
        ActivateFarmEffect();
        AudioManager.Instance.PlayUISound("UpgradeBuilding");

        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        meshFilter.mesh = buildingTiers[current_tier].building_look;
    }
    public override void Fortify()
    {
        if (TryGetComponent<Health>(out var health)) {
            health.increaseMaxHP(fortifyHpBonus);
            health.Heal(fortifyHpBonus);
        }

        AudioManager.Instance.PlayUISound("FortifyBuilding");
        SpawnPopUpText($"Fortified! +{fortifyHpBonus} Max HP", Color.white);
    }

    public override string getEffects()
    {
        StringBuilder sb = new StringBuilder(128);

        sb.Append("Generates <b><color=green>+").Append(decrees[current_tier]).Append("</color></b> Decree Income");

        if (current_tier + 1 < buildingTiers.Count) {
            sb.Append("\n<b><color=#FFD700>On Upgrade:</color></b> Generates <b><color=green>+").Append(decrees[current_tier + 1]).Append("</color></b> Decree Income");
        }

        sb.Append("\n<color=orange>Fortify Effect:</color>");
        sb.Append("\nIncreases structure durability by <b><color=white>+").Append(fortifyHpBonus).Append(" HP</color></b> per stack");

        return sb.ToString();
    }
}