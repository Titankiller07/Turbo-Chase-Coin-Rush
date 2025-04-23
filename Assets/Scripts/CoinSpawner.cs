using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private int maxCollectibles = 10;
    [SerializeField] private float spawnHeight = 1f;
    [SerializeField] private LayerMask spawnCollisionLayer;
    [SerializeField] private float spawnCheckRadius = 1f;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private List<int> scoreThresholds = new List<int> { 10, 25, 50 };
    [SerializeField] private float enemySpawnHeight = 0.5f;
    [SerializeField] private float minEnemySpawnDistance = 20f;
    [SerializeField] private float maxEnemySpawnDistance = 50f;

    private List<GameObject> activeCollectibles = new List<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private PlayerController PlayerController;
    private int currentThresholdIndex = 0;

    
    private const float minX = -220f;
    private const float maxX = 220f;
    private const float minZ = -220f;
    private const float maxZ = 220f;

    void Start()
    {
        // Initial spawn of collectibles
        for (int i = 0; i < maxCollectibles; i++)
        {
            SpawnCollectible();
        }

        StartCoroutine(InitializeEnemySpawning());
    }

    IEnumerator InitializeEnemySpawning()
    {
        // Wait until player is found
        while (PlayerController == null)
        {
            PlayerController = FindAnyObjectByType<PlayerController>();
            yield return null;
        }

        // Initial spawn of enemies
        for (int i = 0; i < Mathf.Min(2, maxEnemies); i++)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        if (PlayerController != null && currentThresholdIndex < scoreThresholds.Count)
        {
            // Check if player reached next score threshold
            if (PlayerController.coinsCollected >= scoreThresholds[currentThresholdIndex])
            {
                SpawnAdditionalEnemies();
                currentThresholdIndex++;
            }
        }
    }

    void SpawnAdditionalEnemies()
    {
        int enemiesToSpawn = Mathf.Min(2, maxEnemies - activeEnemies.Count); // Spawn 2 at a time, up to max
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
        }
    }

    Vector3 GetValidEnemySpawnPosition()
    {
        Vector3 spawnPosition;
        int attempts = 0;
        bool positionValid = false;

        do
        {
            // Get position in ring around player (not too close, not too far)
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minEnemySpawnDistance, maxEnemySpawnDistance);
            spawnPosition = PlayerController.transform.position + 
                          new Vector3(randomCircle.x, 0, randomCircle.y) * distance;
            
            spawnPosition.y = enemySpawnHeight;

            // Check if position is valid
            positionValid = !Physics.CheckSphere(spawnPosition, spawnCheckRadius, spawnCollisionLayer);
            attempts++;

        } while (!positionValid && attempts < 50);

        if (attempts >= 50)
        {
            Debug.LogWarning("Failed to find valid enemy spawn position after 50 attempts");
            spawnPosition = GetRandomSpawnPosition(); // Fallback to random position
        }

        return spawnPosition;
    }

    void SpawnEnemy()
    {
        if (activeEnemies.Count >= maxEnemies) return;

        Vector3 spawnPosition = GetValidEnemySpawnPosition();

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(newEnemy);

        // Set up enemy AI if needed
        var enemyAI = newEnemy.GetComponent<EnemyAIFollow>();
        if (enemyAI != null && PlayerController != null)
        {
            enemyAI.target = PlayerController.transform;
        }
    }

    void SpawnCollectible()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Check if position is valid (not colliding with objects in spawnCollisionLayer)
        int attempts = 0;
        while (Physics.CheckSphere(spawnPosition, spawnCheckRadius, spawnCollisionLayer) && attempts < 50)
        {
            spawnPosition = GetRandomSpawnPosition();
            attempts++;
        }

        if (attempts >= 50)
        {
            Debug.LogWarning("Failed to find valid spawn position after 50 attempts");
            return;
        }

        GameObject newCollectible = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);
        activeCollectibles.Add(newCollectible);

        // Ensure the collectible has proper components
        SetupCollectible(newCollectible);
    }

    Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        return new Vector3(x, spawnHeight, z);
    }

    void SetupCollectible(GameObject collectible)
    {
        // Add collider if not present
        if (!collectible.GetComponent<Collider>())
        {
            var collider = collectible.AddComponent<SphereCollider>();
            collider.isTrigger = true;
        }
        else
        {
            collectible.GetComponent<Collider>().isTrigger = true;
        }

        // Tag as collectible
        collectible.tag = "Collectible";

        // Add collectible component if not present
        if (!collectible.GetComponent<Collectible>())
        {
            var collector = collectible.AddComponent<Collectible>();
            collector.spawner = this;
        }
    }

    public void CollectiblePickedUp(GameObject collectedObject)
    {
        if (activeCollectibles.Contains(collectedObject))
        {
            // Remove from active list
            activeCollectibles.Remove(collectedObject);

            // Destroy the collected object
            Destroy(collectedObject);

            // Spawn a new one
            SpawnCollectible();
        }
    }

     public void EnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            Destroy(enemy);
            
            // Optionally respawn enemy after delay
            StartCoroutine(RespawnEnemyAfterDelay(Random.Range(5f, 10f)));
        }
    }

    IEnumerator RespawnEnemyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnEnemy();
    }
}

// This script should be attached to your collectible prefab
public class Collectible : MonoBehaviour
{
    [HideInInspector] public CoinSpawner spawner;

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider is the player
        if (other.CompareTag("Player"))
        {
            // Notify spawner that this was collected
            if (spawner != null)
            {
                spawner.CollectiblePickedUp(gameObject);
            }
            else
            {
                Debug.LogWarning("Collectible has no reference to spawner!");
            }
        }
    }
}
