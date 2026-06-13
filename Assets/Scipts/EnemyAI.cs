using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent Agent;
    private Transform _destination;

    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();

        if (!Agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
        }
        Agent.radius = 0.4f;
        Agent.avoidancePriority = Random.Range(0, 100);
        Agent.obstacleAvoidanceType = (ObstacleAvoidanceType)Random.Range(1, 5);
        Agent.autoBraking = false;
        Agent.speed += Random.Range(-0.4f, 0.6f);
    }
    void Update()
    {
        if (_destination == null && Agent.pathPending) return;
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            if (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f) 
                OnDestinationReach();
        }
    }
    void OnDestinationReach()
    {
        Destroy(gameObject);
    }
}