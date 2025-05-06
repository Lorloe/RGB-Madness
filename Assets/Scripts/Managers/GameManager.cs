using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool IsInitialized { get; set; }
    public int CurrentScore { get; set; }

    const string HIGH_SCORE_KEY = "HighScore";
    const string MAIN_MENU = "MainMenu";
    const string GAMEPLAY = "Gameplay";

    public int HighScore
    {
        get { return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0); }
        set { PlayerPrefs.SetInt(HIGH_SCORE_KEY, value); }
    }

    private void Init()
    {
        // Initialize game state, load resources, etc.
        // Debug.Log("GameManager Initialized");
        CurrentScore = 0;
        IsInitialized = false;
    }

    private void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
            return;
        }
        else
            Destroy(gameObject);
    }

    public void GoToMainMenu()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(MAIN_MENU);
    }

    public void GoToGamePlay()
    {
        // Load the gameplay scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(GAMEPLAY);
    }
}
