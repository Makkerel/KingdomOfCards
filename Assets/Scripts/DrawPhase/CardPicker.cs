using UnityEngine;
using System;
using TMPro;

public class CardPicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI cardPickText;
    [SerializeField] private Transform card_display;
    [SerializeField] private GameObject card_ui_prefab;
    [SerializeField] private int card_selections = 3;
    public event Action<Card> on_card_selected;

    private void OnEnable()
    {
        cardUI.select += selectCard;
    }

    private void OnDisable()
    {
        cardUI.select -= selectCard;
    }

    public void updateText(int cardsToSelect)
    {
        if(cardsToSelect == 1) {
            cardPickText.text = "Pick 1 Card";
        }
        else {
            cardPickText.text = $"Pick {cardsToSelect} Cards";
        }
    }

    public void displayCards()
    {
        DeckManager.Instance.shuffle(DeckManager.Instance.possible_cards);
        for(int i = 0; i < card_selections; i++) {
            CardInstance new_card = new CardInstance(DeckManager.Instance.possible_cards[i]);
            cardUI card_ui = Instantiate(card_ui_prefab, card_display).GetComponent<cardUI>();
            card_ui.RenderInfo(new_card);
        }
    }

    public void selectCard(GameObject selected_card)
    {
        Card the_card = selected_card.GetComponent<cardUI>().backend_card.source;
        for(int i = card_display.childCount - 1; i >= 0; i--) {
            Destroy(card_display.GetChild(i).gameObject);
        }
        on_card_selected?.Invoke(the_card);
    } 
}
