using PrimeTween;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
public enum GamePhase
{
    GameStart,
    PickCards,
    PlaceCastle,
    ActionPhase,
    CombatPhase
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public int turnNumber { get; private set; } = 0;
    private bool end_turn = false;

    [SerializeField] private CinemachineCamera action_cam;
    [SerializeField] private CinemachineCamera combat_cam;

    [Header("UI For Phases")]
    [SerializeField] private GameObject PauseMenuUI;
    [SerializeField] private Canvas CardPickUI;
    [SerializeField] private Canvas handUI;
    [SerializeField] private Canvas combatUI;
    [SerializeField] private Canvas GameOverUI;


    [Header("Start of Game Effects")]
    [SerializeField] private handArea handArea;
    [SerializeField] private Card castleCard;
    private AwaitableCompletionSource placedCastle;

    [SerializeField] private int initial_cards = 5;
    private AwaitableCompletionSource<Card> picked_card;

    [Header("Cheats")]
    public bool cheatOn = false;

    //Events
    public event Func<Task> activate_effects;
    public event Action draw_phase_start;
    public event Action action_phase_start;
    public event Action combat_phase_start;
    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        PauseMenuUI.SetActive(true);
        TutorialManager.Instance.changePhase(GamePhase.GameStart);
        MusicManager.Instance.ChangeMusic("DrawMusic");
        await startAddDeck(initial_cards);
        TutorialManager.Instance.changePhase(GamePhase.PlaceCastle);
        await placeStartingCastle();
        DeckManager.Instance.createDeck();
        StartCoroutine(gameLoop());
    }

    public async Task placeStartingCastle()
    {
        handUI.gameObject.SetActive(true);
        handUI.GetComponent<handUI>().updateTurnText("Place Your\nCastle!");
        CardInstance castle = new CardInstance(castleCard);
        castle.action_cost = 0;
        handArea.renderCard(castle);
        GridManager.Instance.placedBuilding += Handler;

        void Handler() {
            placedCastle.SetResult();
        }

        await placeCastle();
        GridManager.Instance.placedBuilding -= Handler;
    }

    public async Task startAddDeck(int card_add_num)
    {
        if (turnNumber > 0) {
            TutorialManager.Instance.changePhase(GamePhase.PickCards);
        }
        CardPicker card_picker = CardPickUI.GetComponent<CardPicker>();
        CardPickUI.gameObject.SetActive(true);
        card_picker.on_card_selected += Handler;

        void Handler(Card the_card) {
            picked_card.SetResult(the_card);
        }

        for (int i = 0; i < card_add_num; i++) {
            card_picker.updateText(card_add_num - i);
            card_picker.displayCards();
            Card selected_card = await pickCard();
            AudioManager.Instance.PlayUISound("SelectCard");
            DeckManager.Instance.addCard(selected_card);
        }
        card_picker.on_card_selected -= Handler;
        CardPickUI.gameObject.SetActive(false);
    }

    private Awaitable<Card> pickCard()
    {
        picked_card = new AwaitableCompletionSource<Card>();
        return picked_card.Awaitable;
    }

    private Awaitable placeCastle()
    {
        placedCastle = new AwaitableCompletionSource();
        return placedCastle.Awaitable;
    }

    private IEnumerator gameLoop()
    {
        while (true) {
            turnNumber += 1;
            yield return StartCoroutine(drawPhase());
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(actionPhase());
            yield return StartCoroutine(combatPhase());
            yield return new WaitForSeconds(1f);
        }
    }

    private async Task TriggerTurnStartEffects()
    {
        if (activate_effects != null) {
            Delegate[] listeners = activate_effects.GetInvocationList();
            foreach (Func<Task> listener in listeners) {
                await listener.Invoke();
            }
        }
    }

    private IEnumerator drawPhase()
    {
        Debug.Log("Draw Phase");
        
        action_cam.Priority.Value = 1;
        combat_cam.Priority.Value = 0;

        MusicManager.Instance.ChangeMusic("DrawMusic");
        combatUI.gameObject.SetActive(false);
        handUI.GetComponent<handUI>().updateText(turnNumber, "Draw Phase");
        draw_phase_start?.Invoke();

        Task start_turn_effect = TriggerTurnStartEffects();

        yield return new WaitUntil(() => start_turn_effect.IsCompleted);
        
        handUI.gameObject.SetActive(true);
        DeckManager.Instance.drawCards();
        
        end_turn = false;
        yield return null;
    }

    private IEnumerator actionPhase()
    {
        Debug.Log("Action Phase");
        TutorialManager.Instance.changePhase(GamePhase.ActionPhase);
        handUI.GetComponent<handUI>().updateText(turnNumber, "Action Phase");
        action_phase_start?.Invoke();

        yield return new WaitUntil(() => end_turn);
        
        ActionManager.Instance.endActionPhase();
        DeckManager.Instance.discardCards();
        end_turn = false;
    }

    private IEnumerator combatPhase()
    {
        Debug.Log("Combat Phase");
        MusicManager.Instance.ChangeMusic("CombatMusic");
        TutorialManager.Instance.changePhase(GamePhase.CombatPhase);
        combatUI.gameObject.SetActive(true);
        handUI.gameObject.SetActive(false);
        combat_cam.Priority.Value = 1;
        action_cam.Priority.Value = 0;

        combat_phase_start?.Invoke();
        yield return new WaitUntil(() => end_turn);
        end_turn = false;
    }

    public void endTurn()
    {
        end_turn = true;
    }

    public void EndLevel(bool isWin)
    {
        combatUI.gameObject.SetActive(false);
        PauseMenuUI.SetActive(false);

        if (isWin) {
            MusicManager.Instance.ChangeMusic("WinGame");
        }
        else {
            MusicManager.Instance.ChangeMusic("LoseGame");
        }

        StartCoroutine(ShowGameOverUIDelayed(isWin, 1.5f));
    }

    private IEnumerator ShowGameOverUIDelayed(bool isWin, float delay)
    {
        yield return new WaitForSeconds(delay);
        CanvasGroup group = GameOverUI.GetComponent<CanvasGroup>();
        if (group != null) {
            group.alpha = 0f;
        }
        GameOverUI.gameObject.SetActive(true);

        GameOverUI uiScript = GameOverUI.GetComponent<GameOverUI>();
        if (isWin) {
            uiScript.SetText("You Beat The Red Kingdom!");
        }
        else {
            uiScript.SetText("The Red Kingdom Destroyed You.");
        }
        if (group != null) {
            PrimeTween.Tween.Alpha(group, endValue: 1f, duration: 0.5f, ease: Ease.OutQuad);
        }
    }
}
