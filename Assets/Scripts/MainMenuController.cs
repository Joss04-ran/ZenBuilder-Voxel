using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuController : MonoBehaviour
{
    public AudioSource mainMenu;
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        mainMenu.Play();
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void QuitGame()
    {
        Debug.Log("Close Game"); 
        Application.Quit();
    }
}