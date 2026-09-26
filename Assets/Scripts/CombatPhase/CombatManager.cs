using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
public enum CombatState { Skirmish, PanicMode, Concluded }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Global Combat State")]
    public CombatState currentState { get; private set; } = CombatState.Concluded;
    public bool combat_over => currentState == CombatState.Concluded;

    [Header("Morale Settings")]
    [SerializeField] private MoraleBarVisual moraleVisuals;
    [SerializeField] private float base_max_morale = 50f;
    [SerializeField] private float morale_decay_rate = 1.5f;

    [Header("Morale Progression & Limits")]
    [SerializeField] private float moraleScalingPerTurn = 15f;
    [SerializeField] private float absoluteMaxMoraleCap = 150f;
    [Range(0f, 1f)][SerializeField] private float initialMoraleRatio = 0.5f;
    [SerializeField] private float base_decay_acceleration_factor = 0.015f;
    [SerializeField] private float acceleration_scaling_per_turn = 0.005f;

    private float active_decay_acceleration_factor;
    private float morale_decay_modifier = 0f;
    private float cached_actual_decay_rate;
    private float max_morale;
    private float current_morale;
    private float combat_time_elapsed;

    [Header("Panic Mode (Overtime Countdown)")]
    [SerializeField] private float basePanicDuration = 13f;
    [SerializeField] private float panicScalingPerTurn = 3f;
    [SerializeField] private float maxPanicDuration = 40f;

    private float activePanicDuration;
    private float panicTimer;

    [Header("Gold Settings & Limitations")]
    [SerializeField] private GoldVisual goldVisuals;
    public const int MAX_GOLD = 10;
    [SerializeField] private int startingGold = 3;
    [SerializeField] private float base_gold_rate = 2f;

    private int current_gold;
    private float gold_speed_modifier = 0f;
    private float gold_rate;
    private float gold_regen_timer;

    [Header("Enemy AI")]
    [SerializeField] private EnemyAI enemyAI;

    [Header("Cards & Hand Layout Tuning")]
    public AttackArea attack_area;
    [SerializeField] private int initialCardsDrawn = 4;
    private readonly Dictionary<Troop, int> troopProviderRegistry = new();
    public HashSet<GameObject> deployedTroops;

    [Header("Deployment Physics & Sampling")]
    [SerializeField] private LayerMask deploy_ground;
    [SerializeField] private float maxRaycastDistance = 500f;
    [SerializeField] private float navMeshSampleRadius = 1.5f;
    [SerializeField] private float spawnSpreadRadius = 0.6f;

    [SerializeField] private LayerMask enemyBuildingLayer;
    [SerializeField] private float buildingRestrictionRadius = 18f;
    [SerializeField] private string enemyZoneTag = "EnemyTerritory";

    private readonly List<GameObject> enemyTerritory = new List<GameObject>();

    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }

        deployedTroops = new HashSet<GameObject>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        TurnManager.Instance.combat_phase_start += startCombatPhase;
        CacheEnemyTerritory();
        ToggleBuildingRestrictionRings(false);
    }

    private void OnDisable()
    {
        if (TurnManager.Instance != null) {
            TurnManager.Instance.combat_phase_start -= startCombatPhase;
        }
    }

    private void CacheEnemyTerritory()
    {
        enemyTerritory.Clear();
        GameObject[] buildings = GameObject.FindGameObjectsWithTag(enemyZoneTag);
        enemyTerritory.AddRange(buildings);
    }

    public void ToggleBuildingRestrictionRings(bool showRings)
    {
        for (int i = 0; i < enemyTerritory.Count; i++) {
            if (enemyTerritory[i] != null) {
                enemyTerritory[i].SetActive(showRings);
            }
        }
    }
    private void startCombatPhase()
    {
        AudioManager.Instance.PlayUISound("StartCombat");
        enemyAI.StartAI();
        currentState = CombatState.Skirmish;
        combat_time_elapsed = 0f;

        int turnCount = TurnManager.Instance.turnNumber;

        max_morale = Mathf.Min(base_max_morale + (turnCount * moraleScalingPerTurn), absoluteMaxMoraleCap);
        current_morale = max_morale * initialMoraleRatio;

        active_decay_acceleration_factor = base_decay_acceleration_factor + (turnCount * acceleration_scaling_per_turn);
        activePanicDuration = Mathf.Min(basePanicDuration + (turnCount * panicScalingPerTurn), maxPanicDuration);

        RecalculateDecayVelocityCache();
        moraleVisuals.Initialize(max_morale, current_morale);

        current_gold = startingGold;
        gold_rate = base_gold_rate / (1f + gold_speed_modifier);
        gold_regen_timer = gold_rate;
        goldVisuals.RefreshGridDirect(current_gold);

        for (int i = 0; i < initialCardsDrawn; i++) {
            DrawCard();
        }
    }

    private void Update()
    {
        if (currentState == CombatState.Concluded) return;

        switch (currentState) {
            case CombatState.Skirmish:
                combat_time_elapsed += Time.deltaTime;
                regenGold();
                processMoraleDecay();
                break;

            case CombatState.PanicMode:
                regenGold();
                panicTimer -= Time.deltaTime;
                moraleVisuals.UpdatePanicTimer(panicTimer);

                if (panicTimer <= 0f) {
                    endCombatPhase();
                }
                break;
        }
    }

    private void endCombatPhase()
    {
        currentState = CombatState.Concluded;
        moraleVisuals.SetStatusText("Morale");
        EndTurn();
        TurnManager.Instance.endTurn();
    }

    private void processMoraleDecay()
    {
        if (current_morale > 0) {
            float activeDecayRate = cached_actual_decay_rate * (1f + (combat_time_elapsed * active_decay_acceleration_factor));

            current_morale -= activeDecayRate * Time.deltaTime;
            current_morale = Mathf.Clamp(current_morale, 0f, max_morale);

            moraleVisuals.UpdateValueDirect(current_morale);

            if (current_morale <= 0f) {
                TriggerPanicMode();
            }
        }
    }

    public void AddMorale(float amount)
    {
        if (currentState != CombatState.Skirmish) return;

        current_morale += amount;
        current_morale = Mathf.Clamp(current_morale, 0f, max_morale);

        moraleVisuals.AnimateValueJump(current_morale);
    }

    public void changeMoraleDecayRate(float amount)
    {
        morale_decay_modifier += amount;

        if (morale_decay_modifier <= -0.9f) {
            morale_decay_modifier = -0.9f;
        }

        RecalculateDecayVelocityCache();
    }

    private void RecalculateDecayVelocityCache()
    {
        cached_actual_decay_rate = morale_decay_rate / (1f + morale_decay_modifier);
    }

    private void TriggerPanicMode()
    {
        currentState = CombatState.PanicMode;
        panicTimer = activePanicDuration;
    }

    private void regenGold()
    {
        if (current_gold < MAX_GOLD) {
            gold_regen_timer -= Time.deltaTime;

            float progress = 1f - (gold_regen_timer / gold_rate);
            goldVisuals.UpdateRegenProgress(current_gold, progress);

            if (gold_regen_timer <= 0) {
                current_gold++;
                gold_regen_timer = gold_rate;
                goldVisuals.RefreshGridDirect(current_gold);
            }
        }
    }

    public void changeGoldRate(float amount)
    {
        gold_speed_modifier += amount;

        if (gold_speed_modifier <= -0.9f) {
            gold_speed_modifier = -0.9f;
        }
        gold_rate = base_gold_rate / (1f + gold_speed_modifier);
    }

    public void addGold(int more_gold)
    {
        current_gold = Mathf.Min(current_gold + more_gold, MAX_GOLD);
        goldVisuals.RefreshGridDirect(current_gold);
    }

    public void WinGame() => KillSwitch(true);
    public void LoseGame() => KillSwitch(false);

    private void KillSwitch(bool isWin)
    {
        if (currentState == CombatState.Concluded) return;
        currentState = CombatState.Concluded;

        moraleVisuals.SetStatusText("Morale");
        EndTurn();

        TurnManager.Instance.EndLevel(isWin);
    }

    private void EndTurn()
    {
        enemyAI.EndAI();
        enemyAI.clearEnemies();

        foreach (GameObject troop in deployedTroops) {
            if (troop != null) {
                if (troop.TryGetComponent<Health>(out var health)) {
                    health.on_die -= removeDeployedTroop;
                }
                Destroy(troop);
            }
        }
        deployedTroops.Clear();
        attack_area.destroyHand();
    }

    private void DrawCard()
    {
        if (troopProviderRegistry.Count == 0) return;

        float total_weight = 0f;
        foreach (KeyValuePair<Troop, int> kvp in troopProviderRegistry) {
            total_weight += kvp.Key.weight * kvp.Value;
        }

        if (total_weight <= 0f) return;

        float random_draw = Random.Range(0f, total_weight);
        foreach (KeyValuePair<Troop, int> kvp in troopProviderRegistry) {
            float entryWeightSnapshot = kvp.Key.weight * kvp.Value;
            random_draw -= entryWeightSnapshot;

            if (random_draw <= 0f) {
                attack_area.displayCard(kvp.Key);
                return;
            }
        }
    }

    public void addUnit(Troop the_unit, int amount = 1)
    {
        if (the_unit == null || amount <= 0) return;

        if (!troopProviderRegistry.ContainsKey(the_unit)) {
            troopProviderRegistry[the_unit] = 0;
        }
        troopProviderRegistry[the_unit] += amount;

        Debug.Log($"[Registry] Added x{amount} {the_unit.name}. Total active slots: {troopProviderRegistry[the_unit]}");
    }

    public void removeUnit(Troop remove_unit, int amount = 1)
    {
        if (remove_unit == null || amount <= 0) return;

        if (troopProviderRegistry.ContainsKey(remove_unit)) {
            troopProviderRegistry[remove_unit] -= amount;

            if (troopProviderRegistry[remove_unit] <= 0) {
                troopProviderRegistry.Remove(remove_unit);
            }
        }
    }

    private void removeDeployedTroop(GameObject troop)
    {
        if (troop == null) return;

        if (troop.TryGetComponent<TroopInfo>(out var troopInfo)) {
            enemyAI.AccountPlayerTroopDeath(troopInfo);
        }

        deployedTroops.Remove(troop);
    }

    public int GetDuplicateBuildingCount(Troop target_troop)
    {
        if (troopProviderRegistry.TryGetValue(target_troop, out int count)) {
            return Mathf.Max(0, count - 1);
        }
        return 0;
    }

    public void deployOntoBattlefield()
    {
        if (currentState == CombatState.Concluded) return;
        if (attack_area.selected_card == null) return;
        if (!attack_area.selected_card.TryGetComponent<attackCardUI>(out var selected_card)) return;

        Troop troopCardData = selected_card.troop_card;
        int cost = troopCardData.action_cost;

        if (cost > current_gold) {
            AudioManager.Instance.PlayUISound("PlaceFailure");
            goldVisuals.PlayDenialJuice();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, deploy_ground)) return;
        if (!NavMesh.SamplePosition(hit.point, out NavMeshHit nav_hit, navMeshSampleRadius, NavMesh.AllAreas)) return;

        bool isInsideEnemyTerritory = Physics.CheckSphere(hit.point, buildingRestrictionRadius, enemyBuildingLayer);

        if (isInsideEnemyTerritory) {
            AudioManager.Instance.PlayUISound("PlaceFailure");
            goldVisuals.PlayDenialJuice();
            Debug.LogWarning("[Placement Blocked] Position falls within an enemy structure's defensive radius.");
            return;
        }

        float currentProgress = 1f - (gold_regen_timer / gold_rate);
        goldVisuals.AnimateSpendCascade(current_gold, cost, currentProgress);
        current_gold -= cost;

        int activeDuplicates = GetDuplicateBuildingCount(troopCardData);
        int finalSpawnCount = troopCardData.spawnCount;

        if (troopCardData.duplicateBonus == DuplicateBonusType.ExtraBody) {
            finalSpawnCount += activeDuplicates;
        }

        GameObject prefab = troopCardData.troop_prefab;

        for (int i = 0; i < finalSpawnCount; i++) {
            Vector3 spawnOffset = Vector3.zero;
            if (finalSpawnCount > 1) {
                Vector2 randomCircle = Random.insideUnitCircle * spawnSpreadRadius;
                spawnOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            Vector3 finalSpawnPos = nav_hit.position + spawnOffset;
            GameObject troop = Instantiate(prefab, finalSpawnPos, Quaternion.identity);

            if (troopCardData.duplicateBonus == DuplicateBonusType.AbsoluteUnit && activeDuplicates > 0) {
                troop.transform.localScale *= (1f + (activeDuplicates * 0.15f));

                if (troop.TryGetComponent<TroopStats>(out var troopStats)) {
                    troopStats.SetAbsoluteUnitHPBonus(1f + (activeDuplicates * 0.25f));
                }
            }

            deployedTroops.Add(troop);

            if (troop.TryGetComponent<TroopInfo>(out var troopInfo)) {
                enemyAI.AccountNewPlayerTroop(troopInfo);
            }

            if (troop.TryGetComponent<Health>(out var health)) {
                health.on_die += removeDeployedTroop;
            }
        }

        ToggleBuildingRestrictionRings(false);
        AudioManager.Instance.PlayUISound("PlaceTroop");
        attack_area.AnimateAndDestroyCard(selected_card.gameObject);
        DrawCard();
    }
}