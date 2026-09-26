using UnityEngine;

public class Card : ScriptableObject
{
    public string card_name;
    [TextArea]
    public string description;
    public int action_cost;
    public Sprite card_art;
}
