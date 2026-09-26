using UnityEngine;

public enum TroopType { Melee, Ranged }

[CreateAssetMenu(fileName = "TroopWeights", menuName = "Scriptable Objects/TroopWeights")]
public class TroopWeights : ScriptableObject
{
    [Header("Prefab & Logic Alignment")]
    public GameObject enemy_prefrab;
    public TroopType type;

    [Header("Dynamic Economy Options")]
    public float unlockBudgetThreshold = 10f;
    public float deploymentCost = 1f;
    public float moraleReward = 2f;
    public bool isMiniboss = false;

    [Header("Boss Identity")]
    public bool isAntiSwarmAoe = false;
    public bool isAntiBeefSingleTarget = false;

    [Header("Threat Spawn Boundaries")]
    public int normal_spawn = 1;
    public int max_spawn = 3;

    [Header("Weights For Matchups")]
    public float vsMelee;
    public float vsRanged;

    public float getMatchupScore(TroopType troopType)
    {
        return troopType == TroopType.Melee ? vsMelee : vsRanged;
    }
}