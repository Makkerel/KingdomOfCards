using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    private GamePhase currentPhase = GamePhase.GameStart;

    private Dictionary<GamePhase, string> explanations = new Dictionary<GamePhase, string>();
    public event Action stateChanged;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
        initializeExplanations();
    }

    public GamePhase GetGamePhase()
    {
        return currentPhase;
    }

    private void initializeExplanations()
    {
        // Game Start 
        explanations[GamePhase.GameStart] =
            "Welcome! Assemble your starting <b><color=#4AE3FF>deck</color></b>.\n\n" +
            "• <indent=5%>Use <b>WASD</b> to pan the camera across your kingdom.</indent>\n" +
            "• <indent=5%>Use the <b>Scroll Wheel</b> to zoom in and out of the battlefield.</indent>\n\n" +
            "If you ever get stuck, click the <b><color=#FFCC00>[i]</color> Help Button</b> at any time to review your goals!";

        // Pick Cards
        explanations[GamePhase.PickCards] =
            "Drafting Phase: Choose new cards to add to your <b><color=#4AE3FF>deck</color></b> and expand your kingdom's strategy!";

        // Place Castle
        explanations[GamePhase.PlaceCastle] =
            "Deploy your <b><color=#FFCC00>Castle card</color></b> to found your kingdom!";

        // Action Phase
        explanations[GamePhase.ActionPhase] =
            "<b><color=#FF4D4D>ACTION PHASE</color></b>\n" +
            "• <indent=5%><b>Build Your Kingdom:</b> Select and position building cards on the field.</indent>\n" +
            "• <indent=5%><b>Manage <color=#4AE3FF>Decrees</color>:</b> Every building costs Decrees. If you run out, production stops.</indent>\n" +
            "• <indent=5%><b><color=#FFD700>UPGRADE</color> Buildings:</b> Drop a card of the <color=#FFD700><b>SAME TYPE</b></color> onto a building to level it up.</indent>\n" +
            "• <indent=5%><b><color=#FF9800>FORTIFY</color> Positions:</b> Drop a card of a <color=#FF9800><b>DIFFERENT TYPE</b></color> onto a building to stack its custom traits instead.</indent>\n" +
            "• <indent=5%><b>Max Level Rule:</b> Once a building reaches <b>Level 2</b>, it is at max tier and can <i>only</i> be <color=#FF9800><b>Fortified</b></color>!</indent>\n" +
            "• <indent=5%><b>Prepare for War:</b> Always build structures that recruit troops, or you will have no one to fight for you in the next phase!</indent>";

        // Combat Phase
        explanations[GamePhase.CombatPhase] =
            "<b><color=#FF4D4D>COMBAT PHASE</color></b>\n" +
            "• <indent=5%><b>Deploy Troops:</b> Click troop cards and place them on the battlefield using <b><color=#FFCC00>Gold</color></b>.</indent>\n" +
            "• <indent=5%><b>Watch the Clock:</b> Your <b><color=#FF4D4D>Morale bar</color></b> acts as your combat timer.</indent>\n" +
            "• <indent=5%><b><color=#FF3333>PANIC MODE:</color></b> If your Morale completely empties, <b>Panic Mode</b> triggers! Your combat window gets tight and survival pressure skyrockets.</indent>\n" +
            "• <indent=5%><b>Keep Driving:</b> Slay enemy units and demolish their structures to restore your <b><color=#FF4D4D>Morale</color></b> and keep the assault alive!</indent>";
    }
    public string getExplanation()
    {
        return explanations[currentPhase];
    }

    public void changePhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
        stateChanged?.Invoke();
    }
}