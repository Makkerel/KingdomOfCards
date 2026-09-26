using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class attackCardUI : MonoBehaviour
{
    public Troop troop_card;
    public TextMeshProUGUI troop_name;
    public TextMeshProUGUI cost_text;
    public Image card_art;
    private int cost;

    public static event Action<GameObject> attack_card_select;
    
    public void renderInfo(Troop troop_info)
    {
        troop_card = troop_info;
        troop_name.text = troop_info.card_name;
        cost = troop_info.action_cost;
        cost_text.text = $"x{cost}";
        card_art.sprite = troop_info.card_art;
    }

    public void selectAttackCard()
    {
        attack_card_select?.Invoke(gameObject);
    }
}
