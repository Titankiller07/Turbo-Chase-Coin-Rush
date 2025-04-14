using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Make sure to include this for scene management


public class PlayerController : MonoBehaviour
{
[Header("Settings")]
    public float moveSpeed = 10f;
    public float turnSpeed = 100f;
    
    [Header("Coin Settings")]
    public int coinsCollected = 0;
    public TMPro.TextMeshProUGUI coinText;
    public int coinsToWin = 10; // Number of coins to collect to win the game

    [Header("Camera Settings")]
    public Camera mainCamera;      // Assign your main camera in the Inspector
    public Camera secondaryCamera; // Assign your alternate camera in the Inspector
    public KeyCode switchKey = KeyCode.C; // Key to press for switching cameras
    
    private bool isMainCameraActive = true;

    void Start()
    {
        
        // Ensure the main camera is active at the start
        if (mainCamera != null && secondaryCamera != null)
        {
            mainCamera.enabled = true;
            secondaryCamera.enabled = false;
        }

    }

     public void AddCoins(int amount)
    {
        coinsCollected += amount;
        UpdateCoinUI();
         if (coinsCollected >= coinsToWin)
        {
            LoadGameOverScene();
        }
    }
    void Update()
    {
        // Forward/Backward Movement
        float moveInput = Input.GetAxis("Vertical");
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        
        // Left/Right Turning
        float turnInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);

        if (Input.GetKeyDown(switchKey) && mainCamera != null && secondaryCamera != null)
        {
            ToggleCamera();
        }
    }
    void ToggleCamera()
    {
        isMainCameraActive = !isMainCameraActive;
        mainCamera.enabled = isMainCameraActive;
        secondaryCamera.enabled = !isMainCameraActive;
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coins Collected: " + coinsCollected;
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("Game_Over"); // Exact name from Build Settings
    }
}