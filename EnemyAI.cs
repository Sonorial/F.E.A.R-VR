using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyAI : MonoBehaviourPun
{
    private NavMeshAgent agent;
    public float roamingRadius = 10f;
    public float chaseSpeed = 5f;
    public float searchCooldown = 5f;
    public float fieldOfViewAngle = 90f;
    private GameObject player;
    private bool isChasing = false;
    private bool inWanderZone = false;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    private float defaultSpeed;
    public float MonsterHealth = 100f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        defaultSpeed = agent.speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine && collision.gameObject.CompareTag("Bullet"))
        {
            MonsterHealth -= 25f;
            if (MonsterHealth <= 0f)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
    void Update()
    {
        if (!inWanderZone && photonView.IsMine)
        {
            if (!isChasing)
            {
                if (!agent.pathPending && agent.remainingDistance < 0.1f)
                {
                    SetRandomDestination();
                }

                if (CanSeePlayer())
                {
                    StartChasing();
                }
            }
            else
            {
                if (CanSeePlayer())
                {
                    agent.SetDestination(player.transform.position);
                    lastKnownPlayerPosition = player.transform.position;
                }
                else
                {
                    searchTimer += Time.deltaTime;
                    if (searchTimer >= searchCooldown)
                    {
                        StopChasing();
                    }
                }
            }

            if (MonsterHealth <= 0f)
            {
                PhotonNetwork.Destroy(gameObject); // Destroy object across the network
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.1f)
            {
                SetRandomDestination();
            }
        }
    }

    void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamingRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamingRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 directionToPlayer = player.transform.position - transform.position;
        if (Vector3.Angle(transform.forward, directionToPlayer) < fieldOfViewAngle / 2)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void StartChasing()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.transform.position);
        lastKnownPlayerPosition = player.transform.position;
    }

    void StopChasing()
    {
        isChasing = false;
        searchTimer = 0f;
        agent.speed = defaultSpeed;
        SetRandomDestination();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WanderZone"))
        {
            inWanderZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WanderZoneExit"))
        {
            inWanderZone = false;
        }
    }
}
