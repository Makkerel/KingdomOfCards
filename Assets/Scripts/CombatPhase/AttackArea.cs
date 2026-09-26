using PrimeTween;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject combat_card_prefab;

    [Header("Hand Slot Settings")]
    [SerializeField] private float cardSpacing = 200f; 
    [SerializeField] private float selectionHeightOffset = 50f; 
    [SerializeField] private float animationDuration = 0.2f;

    public GameObject selected_card;

    private readonly GameObject[] handSlots = new GameObject[4];
    private void OnEnable()
    {
        attackCardUI.attack_card_select += selectCard;
    }

    private void OnDisable()
    {
        attackCardUI.attack_card_select -= selectCard;
    }

    public void displayCard(Troop troop)
    {
        int targetSlotIndex = FindFirstEmptySlot();

        if (targetSlotIndex == -1) {
            Debug.LogWarning("Hand is already full! Cannot display more cards.");
            return;
        }

        GameObject newCard = Instantiate(combat_card_prefab, transform);
        handSlots[targetSlotIndex] = newCard;
        newCard.GetComponent<attackCardUI>().renderInfo(troop);
        
        RectTransform rect = newCard.GetComponent<RectTransform>();

        float targetX = (targetSlotIndex - 1.5f) * cardSpacing;
        rect.anchoredPosition = new Vector2(targetX, -100f);
        rect.localScale = Vector3.zero;

        Tween.UIAnchoredPosition(rect, endValue: new Vector2(targetX, 0f), duration: animationDuration + 0.1f, ease: Ease.OutBack);
        Tween.Scale(rect, endValue: Vector3.one, duration: animationDuration, ease: Ease.OutQuad);
    }

    public void AnimateAndDestroyCard(GameObject card)
    {
        if (card == null) return;

        for (int i = 0; i < handSlots.Length; i++) {
            if (handSlots[i] == card) handSlots[i] = null;
        }

        RectTransform rect = card.GetComponent<RectTransform>();
      
        if (!card.TryGetComponent<CanvasGroup>(out var canvasGroup)) {
            canvasGroup = card.AddComponent<CanvasGroup>();
        }

        Sequence.Create()
            .Group(Tween.Scale(rect, endValue: Vector3.one * 0.4f, duration: animationDuration, ease: Ease.InBack))
            .Group(Tween.Alpha(canvasGroup, endValue: 0f, duration: animationDuration, ease: Ease.InQuad))
            .ChainCallback(() => Destroy(card));
    }

    public void destroyHand()
    {
        for (int i = 0; i < handSlots.Length; i++) {
            if (handSlots[i] != null) {
                Destroy(handSlots[i]);
                handSlots[i] = null;
            }
        }
        selected_card = null;
    }

    private void selectCard(GameObject new_card_s)
    {
        AudioManager.Instance.PlayUISound("SelectCard");
        if (selected_card == new_card_s) {
            unSelect();
            return;
        }

        CombatManager.Instance.ToggleBuildingRestrictionRings(true);
        if (selected_card != null) {
            changeCardPosition(selected_card, false);
        }

        selected_card = new_card_s;
        changeCardPosition(selected_card, true);
    }

    private void unSelect()
    {
        if (selected_card == null) {
            return;
        }
        changeCardPosition(selected_card, false);
        selected_card = null;
        CombatManager.Instance.ToggleBuildingRestrictionRings(false);
    }

    private void changeCardPosition(GameObject card, bool is_selected)
    {
        if (card == null || !card.TryGetComponent<RectTransform>(out var rect)) return;

        float targetY = is_selected ? selectionHeightOffset : 0f;

        Tween.UIAnchoredPositionY(rect, endValue: targetY, duration: 0.15f, ease: Ease.OutCubic);
    }
    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < handSlots.Length; i++) {
            if (handSlots[i] == null) return i;
        }
        return -1;
    }
}
