using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ArcherTower : PhysicalBuilding
{
    private string uniqueSourceId;
    [SerializeField] private List<Troop> troops;

    private int fortifyCount = 0; 

    public override void initialize(Building building_card)
    {
        building_name = building_card.card_name;
        buildingTiers = new List<BuildingData>(building_card.buildingTiers);
        current_tier = 0;
        fortifyCount = 0;

        uniqueSourceId = $"ArcherTower_{gameObject.GetInstanceID()}";

        ActivateEffect();
    }

    private void OnDestroy()
    {
        DeactivateEffect();
    }

    private void ActivateEffect()
    {
        if (troops[current_tier] == null) return;

        int totalInstances = 1 + fortifyCount;
        CombatManager.Instance.addUnit(troops[current_tier], totalInstances);

        string troopName = troops[current_tier].card_name;
        SpawnPopUpText(fortifyCount > 0 ? $"{troopName} Added x{totalInstances}!" : $"{troopName} Added!", Color.red);
    }

    private void DeactivateEffect()
    {
        if (troops[current_tier] == null) return;

        int totalInstances = 1 + fortifyCount;
        CombatManager.Instance.removeUnit(troops[current_tier], totalInstances);
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

        Troop currentTroop = troops[current_tier];
        CombatManager.Instance.addUnit(currentTroop, 1);

        AudioManager.Instance.PlayUISound("FortifyBuilding");

        string troopName = currentTroop.card_name;

        if (currentTroop.duplicateBonus == DuplicateBonusType.ExtraBody) {
            SpawnPopUpText($"Fortified! Squad Size Up! +1 {troopName}", Color.white);
        }
        else if (currentTroop.duplicateBonus == DuplicateBonusType.AbsoluteUnit) {
            SpawnPopUpText($"Fortified! Bigger {troopName}", Color.magenta);
        }
    }

    public override string getEffects()
    {
        Troop currentTroop = troops[current_tier];
        StringBuilder sb = new StringBuilder(128);

        sb.Append("Adds <b><color=red>").Append(currentTroop.card_name).Append("</color></b> to troop deck");
        sb.Append("\nFortifications: <b>").Append(fortifyCount).Append("</b>");

        if (current_tier + 1 < buildingTiers.Count && current_tier + 1 < troops.Count) {
            Troop nextTroop = troops[current_tier + 1];
            sb.Append("\n<b><color=#FFD700>On Upgrade:</color></b> Replaces <b><color=red>")
              .Append(currentTroop.card_name).Append("</color></b> with <b><color=red>")
              .Append(nextTroop.card_name).Append("</color></b>");
        }

        sb.Append("\n<color=orange>Fortify Effect:</color>");
        if (currentTroop.duplicateBonus == DuplicateBonusType.ExtraBody) {
            sb.Append("\nSpawns <b>+1</b> additional unit per fortification");
        }
        else if (currentTroop.duplicateBonus == DuplicateBonusType.AbsoluteUnit) {
            sb.Append("\nGrants <b>+15% Size</b> and <b>+25% HP</b> per fortification");
        }

        return sb.ToString();
    }
}