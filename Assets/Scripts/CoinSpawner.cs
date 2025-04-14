using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Benjathemaker;

public class CoinSpawner : MonoBehaviour
{
        [Header("Spawn Settings")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private int maxCollectibles = 10;
    [SerializeField] private float spawnHeight = 1f;
    [SerializeField] private LayerMask spawnCollisionLayer;
    [SerializeField] private float spawnCheckRadius = 1f;

    private List<GameObject> activeCollectibles = new List<GameObject>();

    // Spawn area boundaries (X: -250 to 250, Z: -250 to 250)
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
    // Add SimpleGemsAnim if not present
    var gemAnim = collectible.GetComponent<SimpleGemsAnim>() ?? collectible.AddComponent<SimpleGemsAnim>();
    
    // Configure animation settings
    gemAnim.isRotating = true;
    gemAnim.rotateY = true;
    gemAnim.isFloating = true;
    gemAnim.useEasingForFloating = true;
    
    // Tag as collectible
    collectible.tag = "Collectible";
    
    // Ensure collider is set up
    if (!collectible.GetComponent<Collider>())
    {
        var collider = collectible.AddComponent<SphereCollider>();
        collider.isTrigger = true;
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
