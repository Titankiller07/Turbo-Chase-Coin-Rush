using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class PlayerController : MonoBehaviour
{
[Header("Settings")]
    public float moveSpeed = 10f;
    public float turnSpeed = 100f;
    
    [Header("Coin Settings")]
    public int coinsCollected = 0;
    public TMPro.TextMeshProUGUI coinText;
    public int CoinToWin = 10;

    [Header("Camera Settings")]
    public Camera mainCamera;      // Assign your main camera in the Inspector
    public Camera secondaryCamera; // Assign your alternate camera in the Inspector
    public KeyCode switchKey = KeyCode.C; // Key to press for switching cameras
    
    private bool isMainCameraActive = true;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Ensure the main camera is active at the start
        if (mainCamera != null && secondaryCamera != null)
        {
            mainCamera.enabled = true;
            secondaryCamera.enabled = false;
        }

    }
    void FixedUpdate(){
        // Forward/Backward Movement
        float moveInput = Input.GetAxis("Vertical");
        Vector3 movement = transform.forward * moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        
        // Left/Right Turning (rotation doesn't need physics)
        float turnInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
    }
    void Update()
    {
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
 public void AddCoins(int amount)
    {
        coinsCollected += amount;
        UpdateCoinUI();
        if (coinsCollected >= CoinToWin)
        {
            SceneManager.LoadScene(2);
            // Add your win logic here (e.g., load a new scene, show a win screen, etc.)
        }
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coins Collected: " + coinsCollected;
        }
    }
}