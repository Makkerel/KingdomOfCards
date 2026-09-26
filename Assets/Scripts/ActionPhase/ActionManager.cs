using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ActionState
{
    Idle,
    CardSelected
}

public class ActionManager : MonoBehaviour
{
    [Header("References")]
    public static ActionManager Instance;
    [SerializeField] private handArea player_hand;
    [SerializeField] private handUI hand_ui;
    [SerializeField] private GameObject deployGround;
    [SerializeField] private LayerMask ally_layer;

    private ActionState currentState = ActionState.Idle;
    private Camera mainCamera;
    private InputSystem_Actions inputActions;

    [Header("Playing Card Information")]
    private int actions;
    public int max_actions;
    public event Action<CardInstance> play_card;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        mainCamera = Camera.main;

        TurnManager.Instance.action_phase_start += startActionPhase;
        handArea.card_is_selected += cardSelectedState;
        inputActions.Enable();
        inputActions.UI.Click.performed += OnMouseClick;
    }

    private void OnDisable()
    {
        TurnManager.Instance.action_phase_start -= startActionPhase;
        handArea.card_is_selected -= cardSelectedState;
        inputActions.UI.Click.performed -= OnMouseClick;
        inputActions.Disable();
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        if (currentState != ActionState.CardSelected) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ally_layer)) {
            OnBuildingClicked(hit.collider.gameObject);
        }
    }

    public void startActionPhase()
    {
        deployGround.SetActive(true);
        actions = max_actions;
        hand_ui.updateActions();
        AudioManager.Instance.PlayUISound("DrawCards");
        player_hand.renderCards();
    }

    public void endActionPhase()
    {
        deployGround.SetActive(false);
        SetState(ActionState.Idle);
        player_hand.DestroyHand();
    }

    public int getActions()
    {
        return actions;
    }

    public void increaseActions(int actions)
    {
        max_actions += actions;
    }

    private void cardSelectedState(bool is_selected)
    {
        AudioManager.Instance.PlayUISound("SelectCard");
        if (is_selected) {
            SetState(ActionState.CardSelected);
            return;
        }

        SetState(ActionState.Idle);
    }

    public void OnBuildingClicked(GameObject building)
    {
        if (currentState == ActionState.CardSelected) {
            PhysicalBuilding physicalBuilding = building.GetComponent<PhysicalBuilding>();
            playUpgrade(physicalBuilding);
        }
    }

    public void playUpgrade(PhysicalBuilding upgrade_building)
    {
        CardInstance the_card = player_hand.selected_card.GetComponent<cardUI>().backend_card;
        if (actions <= 0 || the_card.action_cost > actions) {
            return;
        }
        Building building = the_card.source as Building;

        if (upgrade_building.getName() == building.card_name) {
            Debug.Log("Here");
            upgrade_building.Upgrade();
        }
        else {
            upgrade_building.Fortify();
        }

        ConsumeAction(the_card.action_cost, the_card);
    }

    public void playCard()
    {
        if (currentState == ActionState.CardSelected && GridManager.Instance.is_building_here(GridUI.current_grid_world_position) == null) {
            CardInstance the_card = player_hand.selected_card.GetComponent<cardUI>().backend_card;
            if (the_card.action_cost > actions) {
                AudioManager.Instance.PlayUISound("PlaceFailure");
                return;
            }
            Building building = the_card.source as Building;

            GameObject physical_building = Instantiate(building.building_prefab, GridUI.current_grid_world_position, building.building_prefab.transform.rotation);
            GridManager.Instance.addGridState(physical_building.gameObject.transform.position, physical_building.GetComponent<PhysicalBuilding>());
            physical_building.GetComponent<PhysicalBuilding>().initialize(building);
            physical_building.GetComponent<Health>().on_die += GridManager.Instance.removeGridState;

            ConsumeAction(the_card.action_cost, the_card);
            AudioManager.Instance.PlayUISound("PlaceBuilding");
        }
    }

    private void ConsumeAction(int cost, CardInstance played_card)
    {
        actions -= cost;
        play_card?.Invoke(played_card);
        hand_ui.updateActions();
        player_hand.DestroySelectedCard();
        SetState(ActionState.Idle);
    }

    private void SetState(ActionState state)
    {
        currentState = state;
        switch (currentState) {
            case ActionState.CardSelected:
                GridUI.Instance.showIndicator(true);
                break;
            default:
                GridUI.Instance.showIndicator(false);
                break;
        }
    }
}