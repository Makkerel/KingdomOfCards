using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Baseline References")]
    [SerializeField] private Transform mainCastleBase;
    [SerializeField] private float threatRadius = 30f;
    [SerializeField] private List<TroopWeights> enemyCards;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private float evaluationInterval = 0.7f;
    [SerializeField] private float randomEval = 0.2f;

    [Header("Global Pacing Setup")]
    [SerializeField] private float offensiveCooldown = 1.5f;
    [Range(0f, 1f)][SerializeField] private float initialBudgetRatio = 0.5f;
    private float nextAllowedSpawnTime;

    [Header("Director Real-Time Economy (TP)")]
    [SerializeField] private float baseMaxBudget = 10f;
    [SerializeField] private float budgetCapPerTurn = 3f;
    [SerializeField] private float baseRegenRate = 1f;
    [SerializeField] private float regenScalingPerTurn = 0.25f;

    [Header("Threat Evaluation Tuning")]
    [SerializeField] private float minimumThreatCutoff = 15f;
    [SerializeField] private float highSeverityCeiling = 200f;

    [Header("Perimeter Multiplier Config")]
    [Range(0f, 1f)][SerializeField] private float innerKeepPercent = 0.25f;
    [SerializeField] private float innerKeepThreatMultiplier = 5.0f;
    [Range(0f, 1f)][SerializeField] private float townWallsPercent = 0.60f;
    [SerializeField] private float townWallsThreatMultiplier = 2.5f;

    [Header("Defensive Spawn Offsets")]
    [SerializeField] private float maxMeleeSpawnDistance = 3f;
    [SerializeField] private float meleeSpawnRatio = 0.5f;
    [SerializeField] private float idealRangedBuffer = 6f;
    [SerializeField] private float rangedFlankOffset = 4f;

    [Header("Miniboss Progression Matrix")]
    [SerializeField] private float bossVisualScaleMultiplier = 2f;
    [SerializeField] private float baseBossHpMultiplier = 2f;
    [SerializeField] private int bossScalingTurnInterval = 3;
    [SerializeField] private float bossHpStepPerInterval = 2f;
    [SerializeField] private float maxBossHpMultiplierCap = 10f;

    [Header("Macro Boss Trigger Configuration")]
    [SerializeField] private int swarmPlayerBodyThreshold = 20;
    [SerializeField] private float beefyPlayerDangerThreshold = 50f;
    [SerializeField] private int beefyPlayerBodyThreshold = 2;
    [SerializeField] private int maxActiveMagesPerType = 2;

    [Header("Explicit Elite Card Assignments")]
    [SerializeField] private TroopWeights archmageCard;
    [SerializeField] private TroopWeights wizardCard;

    private HashSet<GameObject> alive_enemies;
    private CancellationTokenSource aiStopSpawning;
    private readonly List<TroopWeights> unlockedCardsCache = new();

    private int totalPlayerTroops = 0;
    private int beefyPlayerTroops = 0;
    private int currentActiveArchmages = 0;
    private int currentActiveWizards = 0;

    private float currentMaxBudget;
    private float currentBudget;
    private float currentRegenRate;

    private void Awake() => alive_enemies = new HashSet<GameObject>();

    public async void StartAI()
    {
        aiStopSpawning = new CancellationTokenSource();

        int currentTurn = TurnManager.Instance.turnNumber;
        currentMaxBudget = baseMaxBudget + (currentTurn * budgetCapPerTurn);
        currentRegenRate = baseRegenRate + (currentTurn * regenScalingPerTurn);
        currentBudget = currentMaxBudget * initialBudgetRatio;

        CacheUnlockedCards();
        totalPlayerTroops = 0;
        beefyPlayerTroops = 0;
        currentActiveArchmages = 0;
        currentActiveWizards = 0;
        Debug.Log($"[AI Director] Wave Started! Gated Cards: {unlockedCardsCache.Count} | Max Cap: {currentMaxBudget} TP");

        await AILoop(aiStopSpawning.Token);
    }

    private void OnDestroy()
    {
        if (aiStopSpawning != null) {
            aiStopSpawning.Cancel();
            aiStopSpawning.Dispose();
        }
    }

    private async Awaitable AILoop(CancellationToken cancellationToken)
    {
        try {
            while (!cancellationToken.IsCancellationRequested) {
                float currentDelay = evaluationInterval + UnityEngine.Random.Range(-randomEval, randomEval);
                currentDelay = Mathf.Max(0.1f, currentDelay);

                currentBudget = Mathf.Min(currentBudget + (currentRegenRate * currentDelay), currentMaxBudget);

                EvaluateAction();

                await Awaitable.WaitForSecondsAsync(currentDelay, cancellationToken);
            }
        }
        catch (OperationCanceledException) {
            Debug.Log("AI Evaluation Loop successfully halted.");
        }
    }

    public void AccountNewPlayerTroop(TroopInfo troop)
    {
        if (troop == null) return;

        totalPlayerTroops++;
        if (troop.danger_level >= beefyPlayerDangerThreshold) {
            beefyPlayerTroops++;
        }
    }

    public void AccountPlayerTroopDeath(TroopInfo troop)
    {
        if (troop == null) return;

        totalPlayerTroops = Mathf.Max(0, totalPlayerTroops - 1);
        if (troop.danger_level >= beefyPlayerDangerThreshold) {
            beefyPlayerTroops = Mathf.Max(0, beefyPlayerTroops - 1);
        }
    }

    private void HandleAttack()
    {
        if (Time.time < nextAllowedSpawnTime || unlockedCardsCache.Count == 0) return;

        TroopWeights attack_troop = unlockedCardsCache[UnityEngine.Random.Range(0, unlockedCardsCache.Count)];
        float cost = attack_troop.deploymentCost;

        if (currentBudget < cost) return;

        currentBudget -= cost;
        nextAllowedSpawnTime = Time.time + offensiveCooldown;

        ExecuteSpawn(attack_troop, spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].position, null, attack_troop.normal_spawn);
    }

    public void EvaluateAction()
    {
        TroopInfo priorityThreat = FindHighestPriorityThreat(out float highestThreatScore);

        if (Time.time >= nextAllowedSpawnTime) {

            if (totalPlayerTroops >= swarmPlayerBodyThreshold && beefyPlayerTroops >= beefyPlayerBodyThreshold) {
                if (currentActiveArchmages < maxActiveMagesPerType && currentActiveWizards < maxActiveMagesPerType) {
                    float comboCost = archmageCard.deploymentCost + wizardCard.deploymentCost;
                    if (currentBudget >= comboCost) {
                        currentBudget -= comboCost;
                        nextAllowedSpawnTime = Time.time + offensiveCooldown;

                        Vector3 spawnPos = priorityThreat != null ? CalculateDefensiveSpawnPoint(priorityThreat, archmageCard) : spawnPoints[0].position;
                        ExecuteSpawn(archmageCard, spawnPos, priorityThreat, 1);
                        ExecuteSpawn(wizardCard, spawnPos + Vector3.right * 2f, priorityThreat, 1);
                        return;
                    }
                }
            }

            if (totalPlayerTroops >= swarmPlayerBodyThreshold && currentActiveArchmages < maxActiveMagesPerType) {
                if (currentBudget >= archmageCard.deploymentCost) {
                    currentBudget -= archmageCard.deploymentCost;
                    nextAllowedSpawnTime = Time.time + offensiveCooldown;

                    Vector3 spawnPos = priorityThreat != null ? CalculateDefensiveSpawnPoint(priorityThreat, archmageCard) : spawnPoints[0].position;
                    ExecuteSpawn(archmageCard, spawnPos, priorityThreat, 1);
                    return;
                }
            }

            if (beefyPlayerTroops >= beefyPlayerBodyThreshold && currentActiveWizards < maxActiveMagesPerType) {
                if (currentBudget >= wizardCard.deploymentCost) {
                    currentBudget -= wizardCard.deploymentCost;
                    nextAllowedSpawnTime = Time.time + offensiveCooldown;

                    Vector3 spawnPos = priorityThreat != null ? CalculateDefensiveSpawnPoint(priorityThreat, wizardCard) : spawnPoints[0].position;
                    ExecuteSpawn(wizardCard, spawnPos, priorityThreat, 1);
                    return;
                }
            }
        }

        if (priorityThreat == null || highestThreatScore < minimumThreatCutoff) {
            HandleAttack();
            return;
        }

        if (Time.time < nextAllowedSpawnTime) return;

        TroopWeights bestCard = GetCounter(priorityThreat);
        if (bestCard == null) { HandleAttack(); return; }

        float cardCost = bestCard.deploymentCost;

        if (currentBudget >= cardCost) {
            currentBudget -= cardCost;
            nextAllowedSpawnTime = Time.time + offensiveCooldown;

            int spawnCount = bestCard.isMiniboss ? 1 : Mathf.RoundToInt(Mathf.Lerp(bestCard.normal_spawn, bestCard.max_spawn, Mathf.InverseLerp(minimumThreatCutoff, highSeverityCeiling, highestThreatScore)));
            ExecuteSpawn(bestCard, CalculateDefensiveSpawnPoint(priorityThreat, bestCard), priorityThreat, spawnCount);
        }
        else if (highestThreatScore >= highSeverityCeiling) {
            TroopWeights emergencyCard = GetBestAffordableCounter();
            if (emergencyCard != null) {
                float emergencyCost = emergencyCard.deploymentCost;
                currentBudget -= emergencyCost;
                nextAllowedSpawnTime = Time.time + offensiveCooldown;

                int spawnCount = emergencyCard.isMiniboss ? 1 : Mathf.RoundToInt(Mathf.Lerp(emergencyCard.normal_spawn, emergencyCard.max_spawn, Mathf.InverseLerp(minimumThreatCutoff, highSeverityCeiling, highestThreatScore)));
                ExecuteSpawn(emergencyCard, CalculateDefensiveSpawnPoint(priorityThreat, emergencyCard), priorityThreat, spawnCount);
            }
            else {
                HandleAttack();
            }
        }
        else {
            HandleAttack();
        }
    }

    private TroopInfo FindHighestPriorityThreat(out float highestScore)
    {
        TroopInfo targetTroop = null;
        highestScore = float.MinValue;
        Vector3 castlePos = mainCastleBase.position;

        float innerKeepDist = threatRadius * innerKeepPercent;
        float townWallsDist = threatRadius * townWallsPercent;

        foreach (var deployed_troop in CombatManager.Instance.deployedTroops) {
            if (deployed_troop == null) continue;

            if (deployed_troop.TryGetComponent<TroopInfo>(out var troop)) {
                float distance = Vector3.Distance(castlePos, troop.transform.position);
                if (distance > threatRadius) continue;

                float perimeterMultiplier = distance <= innerKeepDist ? innerKeepThreatMultiplier : (distance <= townWallsDist ? townWallsThreatMultiplier : 1.0f);
                float proximityPercentage = (1f - (distance / threatRadius)) * 100f;
                float currentThreatScore = troop.danger_level * proximityPercentage * perimeterMultiplier;

                if (currentThreatScore > highestScore) {
                    highestScore = currentThreatScore;
                    targetTroop = troop;
                }
            }
        }
        return targetTroop;
    }

    private TroopWeights GetCounter(TroopInfo playerTroop)
    {
        TroopWeights bestCard = null;
        float bestMatchupScore = float.MinValue;

        for (int i = 0; i < unlockedCardsCache.Count; i++) {
            float matchupWeight = unlockedCardsCache[i].getMatchupScore(playerTroop.type);
            if (matchupWeight > bestMatchupScore) {
                bestMatchupScore = matchupWeight;
                bestCard = unlockedCardsCache[i];
            }
        }
        return bestCard;
    }

    private TroopWeights GetBestAffordableCounter()
    {
        TroopWeights bestAffordableCard = null;
        float highestUnlockThreshold = float.MinValue;

        for (int i = 0; i < unlockedCardsCache.Count; i++) {
            TroopWeights card = unlockedCardsCache[i];
            if (currentBudget < card.deploymentCost) continue;

            if (card.unlockBudgetThreshold > highestUnlockThreshold) {
                highestUnlockThreshold = card.unlockBudgetThreshold;
                bestAffordableCard = card;
            }
        }
        return bestAffordableCard;
    }

    private void CacheUnlockedCards()
    {
        unlockedCardsCache.Clear();
        for (int i = 0; i < enemyCards.Count; i++) {
            if (enemyCards[i] != null && enemyCards[i].unlockBudgetThreshold <= currentMaxBudget) {
                unlockedCardsCache.Add(enemyCards[i]);
            }
        }
    }

    private Vector3 CalculateDefensiveSpawnPoint(TroopInfo threat, TroopWeights cardToPlay)
    {
        Vector3 threatPos = threat.transform.position;
        Vector3 destinationPoint = threat.TryGetComponent<NavMeshAgent>(out var agent) ? agent.destination : mainCastleBase.position;

        Vector3 vectorToTarget = (destinationPoint - threatPos);
        vectorToTarget.y = 0f;

        float distance = vectorToTarget.magnitude;
        Vector3 directionToTarget = distance > 0.01f ? vectorToTarget / distance : threat.transform.forward;

        Vector3 spawnPos = cardToPlay.type == TroopType.Melee
            ? threatPos + (directionToTarget * Mathf.Min(maxMeleeSpawnDistance, distance * meleeSpawnRatio))
            : threatPos + (directionToTarget * Mathf.Min(distance - 0.5f, idealRangedBuffer)) + (new Vector3(-directionToTarget.z, 0f, directionToTarget.x) * (UnityEngine.Random.value > 0.5f ? rangedFlankOffset : -rangedFlankOffset));

        return NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 3f, NavMesh.AllAreas) ? hit.position : spawnPos;
    }

    private void ExecuteSpawn(TroopWeights card, Vector3 position, TroopInfo threat, int spawnCount)
    {
        Quaternion spawnRotation = Quaternion.identity;
        if (threat != null) {
            Vector3 lookDir = threat.transform.position - position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f) spawnRotation = Quaternion.LookRotation(lookDir);
        }

        Vector3 rightOffset = threat != null ? Vector3.Cross(Vector3.up, (threat.transform.position - position).normalized) : Vector3.right;

        for (int i = 0; i < spawnCount; i++) {
            Vector3 finalSpawnPos = position;
            if (spawnCount > 1) {
                float sideMultiplier = (i % 2 == 0) ? 1f : -1f;
                finalSpawnPos += rightOffset * (sideMultiplier * (i + 1) * 0.5f * 1.2f);
            }

            GameObject enemy = Instantiate(card.enemy_prefrab, finalSpawnPos, spawnRotation);
            alive_enemies.Add(enemy);

            if (enemy.TryGetComponent<TroopInfo>(out var troopInfo)) {
                troopInfo.runtimeMoraleReward = card.moraleReward;
            }

            enemy.GetComponent<Health>().on_die += RemoveEnemy;

            if (card.isAntiSwarmAoe) {
                currentActiveArchmages++;
                AudioManager.Instance.PlayUISound("BossSpawnAlert");
            }
            if (card.isAntiBeefSingleTarget) {
                currentActiveWizards++;
                AudioManager.Instance.PlayUISound("BossSpawnAlert");
            }

            if (card.isMiniboss) {
                enemy.transform.localScale *= bossVisualScaleMultiplier;

                int currentTurn = TurnManager.Instance.turnNumber;
                float bossHpMultiplier = Mathf.Min(baseBossHpMultiplier + Mathf.FloorToInt(currentTurn / (float)bossScalingTurnInterval) * bossHpStepPerInterval, maxBossHpMultiplierCap);

                if (enemy.TryGetComponent<TroopStats>(out var stats)) {
                    stats.SetAbsoluteUnitHPBonus(bossHpMultiplier);
                }

                AudioManager.Instance.PlayUISound("MinibossSpawnAlert");
            }
        }
    }
    public void EndAI()
    {
        if (aiStopSpawning != null && !aiStopSpawning.IsCancellationRequested) {
            aiStopSpawning.Cancel();
            aiStopSpawning.Dispose();
        }
    }

    public bool enemies_alive() => alive_enemies.Count > 0;

    public void clearEnemies()
    {
        foreach (GameObject enemy in alive_enemies) {
            if (enemy != null) {
                if (enemy.TryGetComponent<Health>(out var health)) health.on_die -= RemoveEnemy;
                Destroy(enemy);
            }
        }
        alive_enemies.Clear();
        currentActiveArchmages = 0;
        currentActiveWizards = 0;
    }

    private void RemoveEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        float payout = 2f;
        if (enemy.TryGetComponent<TroopInfo>(out var troopInfo)) {
            payout = troopInfo.runtimeMoraleReward;

            if (enemy.name.Contains(archmageCard?.enemy_prefrab.name ?? "Archmage")) currentActiveArchmages = Mathf.Max(0, currentActiveArchmages - 1);
            if (enemy.name.Contains(wizardCard?.enemy_prefrab.name ?? "Wizard")) currentActiveWizards = Mathf.Max(0, currentActiveWizards - 1);
        }

        enemy.GetComponent<Health>().on_die -= RemoveEnemy;
        alive_enemies.Remove(enemy);

        CombatManager.Instance.AddMorale(payout);
    }
}