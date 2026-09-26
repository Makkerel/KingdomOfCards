using UnityEngine;

[CreateAssetMenu(fileName = "MoreEnergy", menuName = "Scriptable Objects/Effects/MoreEnergy")]
public class MoreEnergy : Effects
{
    public float amount;
    public override string getEffectDescription()
    {
        return $"You now gain gold <color=#FF5555>{amount * 100}%</color> faster";
    }

}
