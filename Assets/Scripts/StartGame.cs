using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public GameObject startMenu; // Reference to the start menu GameObject

    public GameObject mainMenu; // Reference to the main menu GameObject

    public void Gameinfo(){
        // Activate the start menu and deactivate the main menu
        startMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayGame()
    {
        // Load the scene with the name "Game"
        SceneManager.LoadScene(1);
    }
    public void MainMenu()
    {
        // Load the scene with the name "MainMenu"
        SceneManager.LoadScene(0);
    }
}
