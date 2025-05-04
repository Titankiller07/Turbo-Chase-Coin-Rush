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

    [Header("Handbrake Settings")]
    public float handbrakeDrag = 10f;
    public float normalDrag = 0.5f;
    public KeyCode handbrakeKey = KeyCode.Space;
    
    [Header("Boost Settings")]
    public float boostMultiplier = 2f;
    public float boostDuration = 3f;
    public float boostRechargeRate = 1f;
    public float boostCooldown = 2f;
    public KeyCode boostKey = KeyCode.LeftShift;
    public TextMeshProUGUI boostText; // UI Text to display boost time

    [Header("Ground Check Settings")]
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    private bool isGrounded;

    private bool isMainCameraActive = true;
    private Rigidbody rb;
    private float currentBoostTime;
    private bool isBoostOnCooldown = false;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI healthText;
    public float invincibilityTime = 1f;
    private float lastHitTime;

    [Header("Physics Settings")]
    public float uprightForce = 10f; // Force to keep player upright
    public float uprightOffset = 0.2f; // How high the raycast starts from bottom
    public float uprightTorque = 10f; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = normalDrag; // Set the initial drag to normal drag
        currentBoostTime = boostDuration;

        currentHealth = maxHealth;
        UpdateHealthUI();

        // Ensure the main camera is active at the start
        if (mainCamera != null && secondaryCamera != null)
        {
            mainCamera.enabled = true;
            secondaryCamera.enabled = false;
        }
        
        UpdateBoostUI();

    }
    void FixedUpdate(){
      //Debug.Log($"Grounded: {isGrounded}, MoveInput: {Input.GetAxis("Vertical")}, TurnInput: {Input.GetAxis("Horizontal")}");
         isGrounded = Physics.CheckSphere(transform.position + (Vector3.down * 0.1f),0.5f,groundLayer);

        if (Input.GetKey(handbrakeKey))
        {
            rb.linearDamping = handbrakeDrag;
        }
        else
        {
            rb.linearDamping = normalDrag;
        }

        if(isGrounded){
        // Forward/Backward Movement
        float moveInput = Input.GetAxis("Vertical");
        Vector3 movement = transform.forward * moveInput * moveSpeed;
        if (Input.GetKey(boostKey) && currentBoostTime > 0 && !isBoostOnCooldown)
        {
            movement *= boostMultiplier;
            currentBoostTime -= Time.fixedDeltaTime;
            
            if (currentBoostTime <= 0)
            {
                isBoostOnCooldown = true;
                currentBoostTime = 0;
            }
            UpdateBoostUI();
        }
        else if (!Input.GetKey(boostKey) && !isBoostOnCooldown)
        {
            // Recharge boost when not in use
            float previousBoostTime = currentBoostTime;
            currentBoostTime = Mathf.Min(currentBoostTime + (boostRechargeRate * Time.fixedDeltaTime), boostDuration);
            if ((int)(previousBoostTime * 100) != (int)(currentBoostTime * 100)) // Only update if percentage changed
            {
                UpdateBoostUI();
            }
        }
        // Apply movement
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        // Left/Right Turning (rotation doesn't need physics)
        float turnInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
        }
        else
        {
            // If not grounded, set velocity to zero
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
        KeepUpright();
    }
    void Update()
    {
        if (Input.GetKeyDown(switchKey) && mainCamera != null && secondaryCamera != null)
        {
            ToggleCamera();
        }
        if (isBoostOnCooldown)
        {
            float previousBoostTime = currentBoostTime;
            currentBoostTime += boostRechargeRate * Time.deltaTime;
             if ((int)(previousBoostTime * 100) != (int)(currentBoostTime * 100)) // Only update if percentage changed
            {
                UpdateBoostUI();
            }
            if (currentBoostTime >= boostCooldown)
            {
                isBoostOnCooldown = false;
                currentBoostTime = Mathf.Min(currentBoostTime, boostDuration);
                UpdateBoostUI();
            }
        }
    }

    void KeepUpright()
    {
    // Apply torque to stay upright
    Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * uprightTorque);
    
    // Add downward force to prevent flying up
    if (!isGrounded)
    {
        rb.AddForce(Vector3.down * uprightForce, ForceMode.Acceleration);
    }
    }
    
    void UpdateBoostUI()
    {
        if (boostText != null)
        {
            float boostPercentage = (currentBoostTime / boostDuration) * 100f;
            boostText.text = $"Boost: {Mathf.RoundToInt(boostPercentage)}%";
            
            // Optional: Change color based on boost level
            if (boostPercentage < 30f)
            {
                boostText.color = Color.red;
            }
            else if (boostPercentage < 60f)
            {
                boostText.color = Color.yellow;
            }
            else
            {
                boostText.color = Color.green;
            }
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

        void OnCollisionEnter(Collision collision)
    {
        // Check if collided with an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(10); // Take 1 damage per hit
            Vector3 dir = (transform.position - collision.transform.position).normalized;
            rb.AddForce(dir * 5f, ForceMode.Impulse);
        }
    }


     void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth;
            
            // Optional: Change color based on health level
            if (currentHealth <= 1)
            {
                healthText.color = Color.red;
            }
            else if (currentHealth <= maxHealth / 2)
            {
                healthText.color = Color.yellow;
            }
            else
            {
                healthText.color = Color.green;
            }
        }
    }
    void TakeDamage(int damageAmount)
    {
        if (Time.time > lastHitTime + invincibilityTime)
        {
            currentHealth -= damageAmount;
            lastHitTime = Time.time;
            UpdateHealthUI();
            
            if (currentHealth <= 0)
            {
                GameOver();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Also handle trigger collisions if enemies use trigger colliders
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(10); // Take 1 damage per hit
        }
    }

    void GameOver()
    {
        // Load game over scene
        SceneManager.LoadScene(4);
    }
}