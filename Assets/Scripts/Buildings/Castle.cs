using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Castle : PhysicalBuilding
{
    [SerializeField] private int[] newCards;

    [Header("Fortification Settings")]
    [SerializeField] private int fortifyHpBonus = 25;

    private void OnEnable()
    {
        TurnManager.Instance.activate_effects += HandleEveryTurnEffect;
    }

    private void OnDisable()
    {
        TurnManager.Instance.activate_effects -= HandleEveryTurnEffect;
    }

    public override void initialize(Building building_card)
    {
        building_name = building_card.card_name;
        buildingTiers = new List<BuildingData>(building_card.buildingTiers);
        current_tier = 0;
    }

    public override void Upgrade()
    {
        if (current_tier + 1 >= buildingTiers.Count) {
            Fortify();
            return;
        }

        current_tier += 1;
        AudioManager.Instance.PlayUISound("UpgradeBuilding");
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        meshFilter.mesh = buildingTiers[current_tier].building_look;
        SpawnPopUpText($"+{newCards[current_tier]} New Cards Every Turn!", Color.green);
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

        sb.Append("Draws <b><color=green>+").Append(newCards[current_tier]).Append("</color></b> additional cards at the start of each turn");

        if (current_tier + 1 < buildingTiers.Count) {
            sb.Append("\n<b><color=#FFD700>On Upgrade:</color></b> Draws <b><color=green>+").Append(newCards[current_tier + 1]).Append("</color></b> cards");
        }

        sb.Append("\n<color=orange>Fortify Effect:</color>");
        sb.Append("\nIncreases structure durability by <b><color=white>+").Append(fortifyHpBonus).Append(" HP</color></b> per stack");

        return sb.ToString();
    }
    private async Task HandleEveryTurnEffect()
    {
        if (TurnManager.Instance.turnNumber == 1) {
            return;
        }
        await EffectsManager.Instance.addCard(newCards[current_tier]);
    }
}