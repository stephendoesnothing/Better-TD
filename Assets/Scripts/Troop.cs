using System.Threading;
using UnityEngine;

public class Troop : MonoBehaviour
{
    [Header("Troop Stats")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float firerate = 1f; // Shots per second

    [Header("References")]
    [SerializeField] private Transform firePoint;

    private Transform target;
    private float fireTimer = 0f;

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }
        
        if(Vector3.Distance(transform.position, target.position) > range)
        {
            target = null;
            return;
        }

        // Aim at target

        Vector3 direction = target.position - transform.position;
        direction.y = 0; // Keep tower upright
        if(direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Shoot    
        fireTimer += Time.deltaTime;
        if(fireTimer >= 1f / firerate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    private void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if(distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if(nearestEnemy != null)
        {
            target = nearestEnemy.transform;
        }
    }

    private void Shoot()
    {
        if (target == null) return;

        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null) enemyHealth.TakeDamage(damage);

        Debug.DrawLine(firePoint != null ? firePoint.position : transform.position, target.position, Color.red, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
