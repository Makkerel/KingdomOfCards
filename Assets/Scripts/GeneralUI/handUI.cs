using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class handUI : MonoBehaviour
{
    public TextMeshProUGUI turn;
    public GameObject actions_count;

    public void updateTurnText(string text)
    {
        turn.text = text;
    }

    public void updateText(int turnNum, string phase)
    {
        turn.text = $"Turn {turnNum}\n{phase}";
    }

    public void updateActions()
    {
        actions_count.GetComponent<TextMeshProUGUI>().text = ActionManager.Instance.getActions().ToString();
    }
}
