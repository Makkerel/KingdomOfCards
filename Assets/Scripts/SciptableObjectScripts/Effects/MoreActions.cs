using UnityEngine;

[CreateAssetMenu(fileName = "MoreActions", menuName = "Scriptable Objects/Effects/MoreActions")]
public class MoreActions : Effects
{
    public int additional_actions = 1;
    public override string getEffectDescription()
    {
        return $"Increases your decrees by <color=#FF5555>{additional_actions}</color>";
    }
}
