using UnityEngine;
using UnityEngine.AI;

public class TroopInfo : MonoBehaviour
{
    public Troop troop_card;
    public TroopType type = TroopType.Melee;
    public float danger_level;
    [HideInInspector] public float runtimeMoraleReward = 2f;
    [HideInInspector] public TroopStats stats;
    private BaseAttack attack;
    private NavMeshAgent agent;

    private void Awake()
    {
        stats = GetComponent<TroopStats>();
        attack = GetComponent<BaseAttack>();
        agent = GetComponent<NavMeshAgent>();
    }
    public string description()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(64);

        sb.Append("Damage: <b><color=red>").Append(stats.RuntimeDamage).Append("</color></b>");
        sb.Append("\nAttack Speed: <b><color=yellow>").Append(stats.RuntimeAttackSpeed).Append("</color></b>");

        sb.Append("\nRange: <b><color=orange>").Append(attack.attack_range).Append("</color></b>");
        sb.Append("\nMove Speed: <b><color=#00E6FF>").Append(agent.speed).Append("</color></b>");

        return sb.ToString();
    }
}
