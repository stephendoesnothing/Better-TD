using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 5f;

    private int currentWaypointIndex = 0;
    private EnemySpawner spawner;

    private void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null) Debug.Log("Found spawner");
    }

    private void Update()
    {
        if (waypoints.Length == 0 || waypoints == null) return;
        if (currentWaypointIndex >= waypoints.Length) return;

        // Moves the enemy towards the next waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotates the enemy towards the next waypoint
        if(direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Check if we reached the waypoint
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);
        if(distance <= 0.1f)
        {
            currentWaypointIndex++;

            if(currentWaypointIndex >= waypoints.Length)
            {
                ReachedEnd();
            }
        }
    }

    private void ReachedEnd()
    {
        if (spawner != null) spawner.OnEnemyReachedEnd();

        EnemyHealth health = GetComponent<EnemyHealth>();
        if(health != null)
        {
            health.ReachedEndWithHealth();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
    }

    private void OnDrawGizmos()
    {
        // Visualize the path

        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for(int i = 0; i < waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
