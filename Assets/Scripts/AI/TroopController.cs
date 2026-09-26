using System;
using UnityEngine;
using UnityEngine.AI;

public class TroopController : MonoBehaviour
{
    public enum TroopState { Idle, MoveToGlobalTarget, Chase, Attack }

    private TroopState currentState = TroopState.Idle;
    [SerializeField] private LayerMask enemy_layer;
    [SerializeField] private float aggro_radius = 15f;
    [SerializeField] private float global_aggro_radius = 100f;
    [SerializeField] private float localUpdateInterval = 0.2f;
    [SerializeField] private float globalUpdateInterval = 1.5f;
    [SerializeField] private float timerJitter = 0.05f;

    private NavMeshAgent agent;
    private BaseAttack attacker;
    private Transform current_target;
    private Vector3 lastSetDestination;
    private float nextLocalSearchTime;
    private float nextGlobalSearchTime;
    private readonly Collider[] detectedEnemies = new Collider[20];

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        attacker = GetComponent<BaseAttack>();
        agent.avoidancePriority = UnityEngine.Random.Range(0, 100);
    }

    private void Start()
    {
        agent.stoppingDistance = attacker.attack_range;
        ForceImmediateSensoryScan();
    }

    private void Update()
    {
        switch (currentState) {
            case TroopState.Idle: HandleIdleState(); break;
            case TroopState.MoveToGlobalTarget: HandleMoveState(); break;
            case TroopState.Chase: HandleChaseState(); break;
            case TroopState.Attack: HandleAttackState(); break;
        }
    }

    private void HandleIdleState()
    {
        agent.isStopped = true;
        if (RunLocalSearch()) ChangeState(TroopState.Chase);
        else if (RunGlobalSearch()) ChangeState(TroopState.MoveToGlobalTarget);
    }

    private void HandleMoveState()
    {
        if (!IsTargetValid()) { ChangeState(TroopState.Idle); return; }

        agent.isStopped = false;
        UpdateAgentDestination();

        if (RunLocalSearch()) ChangeState(TroopState.Chase);
        else if (attacker.IsInRange(current_target) <= attacker.attack_range) ChangeState(TroopState.Attack);
    }

    private void HandleChaseState()
    {
        if (!IsTargetValid()) { ChangeState(TroopState.Idle); return; }

        agent.isStopped = false;
        UpdateAgentDestination();

        if (RunLocalSearch()) lastSetDestination = Vector3.positiveInfinity;

        if (attacker.IsInRange(current_target) <= attacker.attack_range) ChangeState(TroopState.Attack);
    }

    private void HandleAttackState()
    {
        if (!IsTargetValid()) { ChangeState(TroopState.Idle); return; }

        agent.isStopped = true;

        Vector3 direction = (current_target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        if (attacker.IsInRange(current_target) >= attacker.attack_range + 0.2f) ChangeState(TroopState.Chase);
        else attacker.TryAttack(current_target);
    }

    private void ChangeState(TroopState newState)
    {
        currentState = newState;
        if (newState == TroopState.Idle) {
            current_target = null;
            lastSetDestination = Vector3.positiveInfinity;
            ForceImmediateSensoryScan();
        }
    }

    private void ForceImmediateSensoryScan() => nextLocalSearchTime = nextGlobalSearchTime = 0f;

    private bool IsTargetValid() => current_target != null && current_target.gameObject.activeInHierarchy;

    private void UpdateAgentDestination()
    {
        if (current_target == null) return;

        bool targetMoved = Vector3.SqrMagnitude(lastSetDestination - current_target.position) > 0.25f;

        bool pathIsBroken = agent.pathStatus == NavMeshPathStatus.PathInvalid;

        if (targetMoved || pathIsBroken) {
            lastSetDestination = current_target.position;
            agent.SetDestination(current_target.position);
        }
    }

    private bool RunLocalSearch()
    {
        if (Time.time < nextLocalSearchTime) return false;
        nextLocalSearchTime = Time.time + localUpdateInterval + UnityEngine.Random.Range(-timerJitter, timerJitter);
        return FindClosestTargetInRange(aggro_radius);
    }

    private bool RunGlobalSearch()
    {
        if (Time.time < nextGlobalSearchTime) return false;
        nextGlobalSearchTime = Time.time + globalUpdateInterval + UnityEngine.Random.Range(-timerJitter, timerJitter);
        return FindClosestTargetInRange(global_aggro_radius);
    }

    private bool FindClosestTargetInRange(float radius)
    {
        int numEnemiesFound = Physics.OverlapSphereNonAlloc(transform.position, radius, detectedEnemies, enemy_layer);
        if (numEnemiesFound == 0) return false;

        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        for (int i = 0; i < numEnemiesFound; i++) {
            if (detectedEnemies[i] == null || !detectedEnemies[i].gameObject.activeInHierarchy) continue;

            float distance = Vector3.SqrMagnitude(transform.position - detectedEnemies[i].transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                bestTarget = detectedEnemies[i].transform;
            }
        }

        if (bestTarget == null) return false;

        if (IsTargetValid()) {
            if (bestTarget == current_target) return false;
            float currentTargetDistSqr = Vector3.SqrMagnitude(transform.position - current_target.position);
            if (closestDistance > currentTargetDistSqr * 0.75f) return false;
        }

        current_target = bestTarget;
        return true;
    }
}