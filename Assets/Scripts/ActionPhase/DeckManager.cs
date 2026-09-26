using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardInstance
{
    public Card source;
    public int action_cost;

    public CardInstance(Card card_source)
    {
        source = card_source;
        action_cost = card_source.action_cost;
    }
}

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public int card_draw;

    public List<Card> possible_cards;
    public Card[] default_cards;

    private List<CardInstance> deck;
    public List<CardInstance> hand;
    private List<CardInstance> discard;

    private void OnEnable()
    {
        ActionManager.Instance.play_card += playCard;
    }

    private void OnDisable()
    {
        ActionManager.Instance.play_card -= playCard;
    }

    private void Awake()
    {
        if (Instance == null) { 
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }

        deck = new List<CardInstance>();
        hand = new List<CardInstance>();
        discard = new List<CardInstance>();
    }

    public void createDeck()
    {
        foreach (Card card_data in default_cards) {
            deck.Add(new CardInstance(card_data));
        }
        shuffle(deck);
    }

    public void addCard(Card new_card)
    {
        deck.Add(new CardInstance(new_card));
        shuffle(deck);
    }

    public void drawCards()
    {
        for(int i = 0; i < card_draw; i++) {
            if (deck.Count == 0) {
                if (discard.Count == 0) {
                    Debug.Log("Wtf happened!");
                    break; 
                }
                reshuffleDiscardDraw();
            }
            CardInstance next_card = deck.Last();
            hand.Add(next_card);
            deck.RemoveAt(deck.Count -1);
        }
    }

    public void playCard(CardInstance r_card)
    {   
        hand.Remove(r_card);
    }

    public void discardCards()
    {
        foreach (CardInstance r_card in hand) {
            discard.Add(r_card);
        }
        hand.Clear();
    }

    public void reshuffleDiscardDraw()
    {
        if(deck.Count == 0) {
            foreach(CardInstance discard_card in discard) {
                deck.Add(discard_card);
            }
            discard.Clear();
            shuffle(deck);
        }
    }

    private void shuffle(List<CardInstance> deck)
    {
        for (int i = 0; i < deck.Count; i++) {
            CardInstance temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    public void shuffle(List<Card> deck)
    {
        for (int i = 0; i < deck.Count; i++) {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }


}
