using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

public class handArea : MonoBehaviour
{
    [Header("References/Defaults")]
    [SerializeField] private GameObject card_prefab;
    [SerializeField] private RectTransform spawnPoint;
    private List<GameObject> cardsInHand = new();

    [Header("Hand Layout Settings")]
    [Tooltip("The absolute maximum pixel width the entire hand can occupy across the screen.")]
    [SerializeField] private float maxHandWidth = 600f;
    [Tooltip("The ideal horizontal pixel gap between cards when you only have a few.")]
    [SerializeField] private float maxCardSpacing = 400f; 
    [Tooltip("How much the cards drop on the left and right edges (higher numbers = deeper arc).")]
    [SerializeField] private float arcIntensity = 30f;
    [Tooltip("The maximum Z-rotation tilt applied to the cards at the far edges of your hand.")]
    [SerializeField] private float maxRotation = 12f;
    [Tooltip("How much the cards move when selected")]
    [SerializeField] private float selectionOffset = 60f;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.25f;
    [SerializeField] private Ease transitionEase = Ease.OutQuad;
    [Tooltip("The time gap (in seconds) between each card starting its animation.")]
    [SerializeField] private float staggerDelay = 0.05f; 

    public GameObject selected_card;
    public static event Action<bool> card_is_selected;
    private void OnEnable()
    {
        cardUI.select += selectCard;
    }

    private void OnDisable()
    {
        cardUI.select -= selectCard;
    }

    public void renderCards()
    {
        foreach (CardInstance r_card in DeckManager.Instance.hand) {
            GameObject cardObject = Instantiate(card_prefab, transform);
            cardObject.GetComponent<cardUI>().RenderInfo(r_card);
            RectTransform cardRect = cardObject.GetComponent<RectTransform>();
            cardRect.position = spawnPoint.position;
            cardRect.rotation = spawnPoint.rotation;
            cardsInHand.Add(cardObject);
        }
        UpdateCardPositions();
    }
    public void renderCard(CardInstance card_info)
    {
        GameObject cardObject = Instantiate(card_prefab, transform);
        cardObject.GetComponent<cardUI>().RenderInfo(card_info);
        cardsInHand.Add(cardObject);
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.position = spawnPoint.position;
        cardRect.rotation = spawnPoint.rotation;
        UpdateCardPositions();
    }

    private void UpdateCardPositions()
    {
        int count = cardsInHand.Count;
        if (count == 0) return;

        float currentSpacing = maxCardSpacing;
        if (count > 1) {
            float allowedSqueezeSpacing = maxHandWidth / (count - 1);
            currentSpacing = Mathf.Min(maxCardSpacing, allowedSqueezeSpacing);
        }

        float totalWidth = (count - 1) * currentSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++) {
            if (cardsInHand[i] == null) continue;
            RectTransform cardRect = cardsInHand[i].GetComponent<RectTransform>();

            float factor = (count > 1) ? ((float)i / (count - 1) * 2f - 1f) : 0f;

            float targetX = startX + (i * currentSpacing);
            float targetY = -Mathf.Pow(factor, 2) * arcIntensity;

            if (cardsInHand[i] == selected_card) {
                targetY += selectionOffset;
            }

            float targetZRotation = -factor * maxRotation;

            Vector2 targetPosition = new Vector2(targetX, targetY);
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZRotation);

            float cardDelay = i * staggerDelay;

            Tween.UIAnchoredPosition(
                cardRect,
                endValue: targetPosition,
                duration: transitionDuration,
                ease: transitionEase,
                startDelay: cardDelay
            );
            Tween.LocalRotation(
                cardRect,
                endValue: targetRotation,
                duration: transitionDuration,
                ease: transitionEase,
                startDelay: cardDelay
            );
        }
    }
    private void selectCard(GameObject new_card_s)
    {
        if (selected_card == new_card_s) {
            unSelect();
            return;
        }
        if (selected_card != null) {
            changeCardPosition(selected_card, false);
        }
        card_is_selected?.Invoke(true);
        selected_card = new_card_s;
        changeCardPosition(selected_card, true);
    }

    private void unSelect()
    {
        if (selected_card == null) {
            return;
        }
        card_is_selected?.Invoke(false);
        changeCardPosition(selected_card, false);
        selected_card = null;
    }
    private void changeCardPosition(GameObject card, bool is_selected)
    {
        if (card == null || !card.TryGetComponent<RectTransform>(out var rect)) return;

        int cardIdx = cardsInHand.IndexOf(card);
        if (cardIdx == -1) return;

        int count = cardsInHand.Count;
        float currentSpacing = (count > 1) ? Mathf.Min(maxCardSpacing, maxHandWidth / (count - 1)) : maxCardSpacing;
        float totalWidth = (count - 1) * currentSpacing;
        float startX = -totalWidth / 2f;

        float factor = (count > 1) ? ((float)cardIdx / (count - 1) * 2f - 1f) : 0f;
        float targetY = -Mathf.Pow(factor, 2) * arcIntensity;

        if (is_selected) {
            targetY += selectionOffset;
        }
        Tween.UIAnchoredPositionY(rect, endValue: targetY, duration: 0.15f, ease: Ease.OutCubic);
    }

    public void DestroyHand()
    {
        foreach (GameObject card in cardsInHand) {
            Destroy(card);
        }
        cardsInHand.Clear();
        selected_card = null;
    }

    public void DestroySelectedCard()
    {
        if (selected_card) {
            cardsInHand.Remove(selected_card);
            Destroy(selected_card);
            selected_card = null;
            UpdateCardPositions();
        }
    }
}

