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

    private List<GameObject> activeCollectibles = new List<GameObject>();

    
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
