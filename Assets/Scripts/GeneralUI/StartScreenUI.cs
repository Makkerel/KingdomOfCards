using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StartScreenUI : MonoBehaviour
{
    private UIDocument mainMenu;
    private Button startButton;
    private Button quitButton;
    private Button creditsButton;

    private void Awake()
    {
        mainMenu = GetComponent<UIDocument>();
        startButton = mainMenu.rootVisualElement.Q("Start") as Button;
        creditsButton = mainMenu.rootVisualElement.Q("Credits") as Button;
        quitButton = mainMenu.rootVisualElement.Q("Quit") as Button;


        startButton.RegisterCallback<ClickEvent>(StartGame);
        creditsButton.RegisterCallback<ClickEvent>(CreditsScene);
        quitButton.RegisterCallback<ClickEvent>(QuitGame);
    }

    public void StartGame(ClickEvent evt)
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        SceneManager.LoadScene("Game");
    }

    public void CreditsScene(ClickEvent evt)
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        SceneManager.LoadScene(3);
    }

    public void QuitGame(ClickEvent evt)
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
