using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI cheatText;
    private InputSystem_Actions togglePauseAction;


    private bool isPaused = false;

    private void Awake()
    {
        togglePauseAction = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        togglePauseAction.Enable();
        togglePauseAction.UI.Escape.performed += OnTogglePause;
    }

    private void OnDisable()
    {
        togglePauseAction.UI.Escape.performed -= OnTogglePause;
        togglePauseAction.Disable();
    }

    private void OnTogglePause(InputAction.CallbackContext context)
    {
        if (isPaused) {
            ResumeGame();
        }
        else {
            PauseGame();
        }
    }

    public void OnTogglePause()
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        if (isPaused) {
            ResumeGame();
        }
        else {
            PauseGame();
        }
    }

    public void PauseGame() 
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void Quit()
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        confirmationPanel.SetActive(true);

    }
    public void OnCancelQuitPressed()
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        confirmationPanel.SetActive(false);
    }

    public void OnConfirmQuitPressed()
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

    }

    public void Restart()
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void MainMenu()
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void cheats()
    {
        AudioManager.Instance.PlayUISound("ClickButton");
        if (TurnManager.Instance.cheatOn) {
            TurnManager.Instance.cheatOn = false;
            cheatText.text = "Cheats: Off";
        }
        else {
            TurnManager.Instance.cheatOn = true;
            cheatText.text = "Cheats: On";
        }
    }
}
