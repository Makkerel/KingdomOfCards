using UnityEngine;

[CreateAssetMenu(fileName = "GlobalDamage", menuName = "Scriptable Objects/Effects/GlobalDamage")]
public class GlobalDamage : Effects
{
    public float amount;
    public override string getEffectDescription()
    {
        return $"Your army does <color=#FF5555>{amount * 100}%</color> more damage";
    }

}
