using UnityEngine;

[CreateAssetMenu(fileName = "MoraleImprovement", menuName = "Scriptable Objects/Effects/MoraleImprovement")]
public class MoraleImprovement : Effects
{
    public float moralePercentage = 0.2f;

    public override string getEffectDescription()
    {
        return $"Morale decreases <color=#FF5555>{moralePercentage * 100}%</color> slower";
    }
}
