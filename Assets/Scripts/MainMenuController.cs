using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class MainMenuController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource mainMenuAudio;

    [Header("Panel Navigasi UI")]
    public GameObject mainMenuPanel;
    public GameObject newGamePanel;  

    [Header("Komponen New Game")]
    public TMP_InputField seedInputField;
    public string gameSceneName = "SampleScene"; 
    private const string SAVE_KEY_SEED = "VoxelGame_SavedSeed";
    private const string SAVE_KEY_EXISTS = "VoxelGame_SaveExists";

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (mainMenuAudio != null && !mainMenuAudio.isPlaying)
        {
            mainMenuAudio.Play();
        }
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        if (newGamePanel != null) newGamePanel.SetActive(false);
    }
    public void ShowNewGamePanel()
    {
        mainMenuPanel.SetActive(false);
        if (newGamePanel != null)
        {
            newGamePanel.SetActive(true);
            if (seedInputField != null) seedInputField.text = "";
        }
    }
    public void StartNewGame()
    {
        string rawSeedInput = seedInputField != null ? seedInputField.text : "";

        int generatedSeed = GenerateSeedFromString(rawSeedInput);
        GameSessionData.CurrentSeed = generatedSeed;
        GameSessionData.SeedString = string.IsNullOrWhiteSpace(rawSeedInput) ? generatedSeed.ToString() : rawSeedInput;
        GameSessionData.IsLoadGame = false;
        SaveCurrentSession(generatedSeed);

        Debug.Log($"[MainMenu] Start New Game! String: '{GameSessionData.SeedString}' | Integer Seed: {GameSessionData.CurrentSeed}");
        ExecuteTransitionToGame();
    }

    private int GenerateSeedFromString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Random.Range(int.MinValue, int.MaxValue);
        }
        if (int.TryParse(input, out int numericSeed))
        {
            return numericSeed;
        }
        return input.GetHashCode();
    }

    public void LoadSavedGame()
    {
        if (PlayerPrefs.GetInt(SAVE_KEY_EXISTS, 0) == 1)
        {
            int savedSeed = PlayerPrefs.GetInt(SAVE_KEY_SEED, 1337);

            GameSessionData.CurrentSeed = savedSeed;
            GameSessionData.SeedString = savedSeed.ToString();
            GameSessionData.IsLoadGame = true;

            Debug.Log($"[MainMenu] Game Saved! Integer : {savedSeed}");
            ExecuteTransitionToGame();
        }
        else
        {
            Debug.LogWarning("[MainMenu] Cam't Find Save Files!");
        }
    }
    private void SaveCurrentSession(int seed)
    {
        PlayerPrefs.SetInt(SAVE_KEY_EXISTS, 1);
        PlayerPrefs.SetInt(SAVE_KEY_SEED, seed);
        PlayerPrefs.Save();
    }

    private void ExecuteTransitionToGame()
    {
        if (mainMenuAudio != null)
        {
            mainMenuAudio.Stop();
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Close App.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}