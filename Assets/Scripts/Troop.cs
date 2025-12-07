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

    // Visual indicator for selection
    private bool isSelected = false;
    private GameObject rangeIndicator;

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        if (Vector3.Distance(transform.position, target.position) > range)
        {
            target = null;
            return;
        }

        // Aim at target
        Vector3 direction = target.position - transform.position;
        direction.y = 0; // Keep tower upright
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Shoot    
        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / firerate)
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
            if (distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
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

    // Stat getters and setters for upgrade system
    public float GetRange() => range;
    public void SetRange(float value) => range = value;

    public float GetDamage() => damage;
    public void SetDamage(float value) => damage = value;

    public float GetFireRate() => firerate;
    public void SetFireRate(float value) => firerate = value;

    // Selection methods
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (isSelected)
        {
            ShowRangeIndicator();
        }
        else
        {
            HideRangeIndicator();
        }
    }

    private void ShowRangeIndicator()
    {
        if (rangeIndicator == null)
        {
            rangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rangeIndicator.name = "RangeIndicator";
            Destroy(rangeIndicator.GetComponent<Collider>());

            rangeIndicator.transform.SetParent(transform);
            rangeIndicator.transform.localPosition = new Vector3(0, 0.05f, 0);
            rangeIndicator.transform.localRotation = Quaternion.identity;

            // Scale to match range
            float diameter = range * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, 0.01f, diameter);

            // Make it semi-transparent green
            Renderer rend = rangeIndicator.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            // If URP shader not found, try Standard
            if (mat.shader.name == "Hidden/InternalErrorShader")
            {
                mat = new Material(Shader.Find("Standard"));
            }

            mat.color = new Color(0, 1, 0, 0.3f); // Green with 30% opacity

            // Set rendering mode to transparent
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            rend.material = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        rangeIndicator.SetActive(true);
    }

    private void HideRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (rangeIndicator != null)
        {
            Destroy(rangeIndicator);
        }
    }
}