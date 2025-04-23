using UnityEngine;
using UnityEngine.AI; // Required for NavMesh functionality

[RequireComponent(typeof(NavMeshAgent))] // Ensures the GameObject has a NavMeshAgent
public class EnemyAIFollow : MonoBehaviour
{
    [Header("Navigation Settings")]
    public Transform target;
    public float updateRate = 0.1f;
    public float stoppingDistance = 0;

    private NavMeshAgent agent;
    private float lastUpdateTime;
    private Rigidbody enemyRb;
    private bool isAgentActive = true;

    [Header("Physics Settings")]
    public float enemyUprightForce = 5f;
    public float enemyUprightTorque = 5f;
    public float collisionDisableTime = 0.5f;

    void Awake()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        enemyRb = GetComponent<Rigidbody>();
        
        // Configure agent settings
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.autoRepath = true;

        enemyRb.constraints  = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
        if (isAgentActive && agent.isOnNavMesh && target != null && Time.time - lastUpdateTime > updateRate)
        {
            lastUpdateTime = Time.time;
            agent.SetDestination(target.position);
        }
    }

    void FixedUpdate()
    {
    // Keep enemy upright
    if (!agent.enabled) // Only if navmesh is disabled (during physics)
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * enemyUprightTorque);
    }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Disable agent temporarily during collision
            if (isAgentActive)
            {
                isAgentActive = false;
                agent.enabled = false;
                
                // Apply small repulsion force
                Vector3 dir = (transform.position - collision.transform.position).normalized;
                enemyRb.AddForce(dir * 3f, ForceMode.Impulse);
                
                // Re-enable after delay
                Invoke("EnableNavMesh", collisionDisableTime);
            }
        }
    }
    void EnableNavMesh()
    {
        if (!agent.isOnNavMesh)
        {
            // If agent somehow got off NavMesh, try to warp it back
            agent.Warp(transform.position);
        }
        
        isAgentActive = true;
        agent.enabled = true;
        enemyRb.linearVelocity = Vector3.zero;
        enemyRb.angularVelocity = Vector3.zero;
        
        // Reset destination
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}
