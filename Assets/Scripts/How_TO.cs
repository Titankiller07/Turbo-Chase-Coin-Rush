using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class How_TO : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void HowTo()
    {
        // Load the scene with the name "Game"
        SceneManager.LoadScene(3);
    }
    public void BackToMenu()
    {
        // Load the scene with the name "Game"
        SceneManager.LoadScene(0);
    }
}
