using UnityEngine;

[CreateAssetMenu(fileName = "SpawnTroop", menuName = "Scriptable Objects/Effects/SpawnTroop")]
public class SpawnTroop : Effects
{
    public Troop troop_card;

    public override string getEffectDescription()
    {
        return $"Your troop deck will now have <color=#FF5555>{troop_card.card_name}</color> appear";
    }

}
