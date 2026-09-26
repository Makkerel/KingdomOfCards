using UnityEngine;

[CreateAssetMenu(fileName = "AddCard", menuName = "Scriptable Objects/Effects/AddCard")]
public class AddCard : Effects
{
    public int number_new_cards = 1;

    public override string getEffectDescription()
    {
        return $"Add <color=#FF5555>{number_new_cards}</color> cards to your deck every turn";
    }
}
