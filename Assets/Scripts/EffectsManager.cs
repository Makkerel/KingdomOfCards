using StrategyEngine.Modifiers;
using System.Threading.Tasks;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public void addActions(int action_num)
    {
        ActionManager.Instance.increaseActions(action_num);
    }

    public void moraleImprovement(float percentage)
    {
        CombatManager.Instance.changeMoraleDecayRate(percentage);
    }
    public void moreEnergy(float amount)
    {
        CombatManager.Instance.changeGoldRate(amount);
    }

    public void modifyTroop(Troop troop, bool add)
    {
        if (add) {
            CombatManager.Instance.addUnit(troop);
        }
        else {
            CombatManager.Instance.removeUnit(troop);
        }
    }

    public void increaseDamage(string sourceID, int percentage)
    {
        DamageModifier blacksmithDamage = new DamageModifier(sourceID, ModifierType.Percent, 10f);
        DamageModifierManager.Instance.AddGlobalModifier("Damage", blacksmithDamage);
    }

    public void increaseHealth(string sourceID, int percentage)
    {
        DamageModifier hospitalHealth = new DamageModifier(sourceID, ModifierType.Percent, 10f);
        DamageModifierManager.Instance.AddGlobalModifier("Health", hospitalHealth);
    }


    public async Task addCard(int new_cards)
    {
        await TurnManager.Instance.startAddDeck(new_cards);
    }

}
