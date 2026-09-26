using UnityEngine;

public enum DuplicateBonusType { ExtraBody, AbsoluteUnit }

[CreateAssetMenu(fileName = "Troop", menuName = "Scriptable Objects/Troop")]
public class Troop : Card
{
    public GameObject troop_prefab;
    public int spawnCount;
    public float weight;
    public DuplicateBonusType duplicateBonus;
}
