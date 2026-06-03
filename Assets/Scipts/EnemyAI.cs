using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform target;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
        }
        agent.radius = 0.4f;
        agent.avoidancePriority = Random.Range(0, 100);
        agent.obstacleAvoidanceType = (ObstacleAvoidanceType)Random.Range(1, 5);
        agent.autoBraking = false;
        agent.speed += Random.Range(-0.4f, 0.6f);

        GameObject baseObj = GameObject.Find("DefenceTarget");

        if (baseObj != null)
        {
            target = baseObj.transform;
            agent.SetDestination(target.position);
        }
    }

    void Update()
    {
        if (target != null && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    OnReachBase();
                }
            }
        }
    }
    void OnReachBase()
    {
        Destroy(gameObject);
    }
}