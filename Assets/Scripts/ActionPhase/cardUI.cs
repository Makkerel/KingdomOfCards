using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class cardUI : MonoBehaviour
{
    public TextMeshProUGUI title;
    public Image cardArt;
    public TextMeshProUGUI description;
    public TextMeshProUGUI action_count;

    public CardInstance backend_card;
    public static event Action<GameObject> select;


    public void RenderInfo(CardInstance the_card)
    {
        backend_card = the_card;
        cardArt.sprite = the_card.source.card_art;
        title.text = the_card.source.card_name;
        description.text = the_card.source.description;
        action_count.text = the_card.action_cost.ToString();
    }

    public void onClicked()
    {
        select?.Invoke(gameObject);
    }
}
