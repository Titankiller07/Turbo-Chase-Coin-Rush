using UnityEngine;
using UnityEngine.AI; // Required for NavMesh functionality

[RequireComponent(typeof(NavMeshAgent))] // Ensures the GameObject has a NavMeshAgent
public class EnemyAIFollow : MonoBehaviour
{
    [Header("Navigation Settings")]
    [Tooltip("The target to follow (usually the player)")]
    public Transform target;              // Player's transform
    
    [Tooltip("How often to update the path (in seconds)")]
    public float updateRate = 0.1f;       // How frequently to recalculate path
    
    [Tooltip("Stopping distance from target")]
    public float stoppingDistance = 0; // Distance to stop from player

    private NavMeshAgent agent;
    private float lastUpdateTime;
    
    [Header("Debug")]
    public bool drawPathGizmo = true;     // Visualize path in Scene view

    void Awake()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        
        // Configure agent settings
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.autoRepath = true;
    }

    void Start()
    {
        // If target isn't assigned, try to find player by tag
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
        
        // Set initial destination
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            Debug.LogWarning("No target assigned and no Player found with tag!");
        }
    }

    void Update()
    {
        // Only update path at specified intervals for performance
        if (target != null && Time.time - lastUpdateTime > updateRate)
        {
            lastUpdateTime = Time.time;
            agent.SetDestination(target.position);
        }
    }

    // Optional: Visualize the path in Scene view
    void OnDrawGizmos()
    {
        if (drawPathGizmo && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
            }
        }
    }
}
