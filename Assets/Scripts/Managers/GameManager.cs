using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool IsInitialized { get; set; }
    public int CurrentScore { get; set; }

    const string highScoreKey = "HighScore";
    const string MainMenu = "MainMenu";
    const string GamePlay = "GamePlay";

    public int HighScore
    {
        get { return PlayerPrefs.GetInt(highScoreKey, 0); }
        set { PlayerPrefs.SetInt(highScoreKey, value); }
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenu);
    }

    public void GoToGamePlay()
    {
        // Load the gameplay scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(GamePlay);
    }
}
