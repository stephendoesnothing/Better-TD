// ========== EnemyHealthBarUI.cs ==========
// Create new script - attach to a GameObject under your Canvas
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    public static EnemyHealthBarUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject healthBarPanel;
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(0, 20f); // Offset from cursor
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30%

    private EnemyHealth trackedEnemy;
    private RectTransform healthBarRect;
    private Canvas canvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate references
        if (healthBarPanel == null)
        {
            Debug.LogError("HealthBarPanel is not assigned in EnemyHealthBarUI!");
            return;
        }

        if (healthBarSlider == null)
        {
            Debug.LogError("HealthBarSlider is not assigned in EnemyHealthBarUI!");
        }

        if (healthText == null)
        {
            Debug.LogWarning("HealthText is not assigned in EnemyHealthBarUI!");
        }

        healthBarRect = healthBarPanel.GetComponent<RectTransform>();
        if (healthBarRect == null)
        {
            Debug.LogError("HealthBarPanel doesn't have a RectTransform!");
        }

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("EnemyHealthBarUI is not under a Canvas!");
        }

        // Start hidden
        healthBarPanel.SetActive(false);
    }

    private void Update()
    {
        if (trackedEnemy != null && healthBarPanel.activeSelf)
        {
            UpdateHealthBar();
            UpdatePosition();
        }
    }

    public void ShowHealthBar(EnemyHealth enemy)
    {
        trackedEnemy = enemy;

        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(true);
        }

        UpdateHealthBar();
    }

    public void HideHealthBar()
    {
        trackedEnemy = null;

        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        if (trackedEnemy == null) return;

        float currentHealth = trackedEnemy.CurrentHealth;
        float maxHealth = trackedEnemy.MaxHealth;
        float healthPercent = currentHealth / maxHealth;

        // Update slider
        if (healthBarSlider != null)
        {
            // Set slider min/max to match health values
            healthBarSlider.minValue = 0;
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;

            // Update color of the fill (if it has an Image component)
            Image fillImage = healthBarSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor,
                                             healthPercent / lowHealthThreshold);
            }
        }

        // Update text
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)}/{Mathf.Ceil(maxHealth)}";
        }
    }

    private void UpdatePosition()
    {
        if (canvas == null || healthBarRect == null) return;

        // Get mouse position and convert to canvas space
        Vector2 mousePosition = Input.mousePosition;

        // Apply offset
        Vector2 targetPosition = mousePosition + offset;

        // Convert to local position in canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            targetPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );

        healthBarRect.localPosition = localPoint;

        // Keep health bar within canvas bounds
        ClampToCanvas();
    }

    private void ClampToCanvas()
    {
        if (canvas == null || healthBarRect == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector3 pos = healthBarRect.localPosition;

        Vector2 canvasSize = canvasRect.sizeDelta;
        Vector2 healthBarSize = healthBarRect.sizeDelta;

        // Clamp to canvas bounds
        float minX = -canvasSize.x / 2 + healthBarSize.x / 2;
        float maxX = canvasSize.x / 2 - healthBarSize.x / 2;
        float minY = -canvasSize.y / 2 + healthBarSize.y / 2;
        float maxY = canvasSize.y / 2 - healthBarSize.y / 2;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        healthBarRect.localPosition = pos;
    }

    public EnemyHealth GetTrackedEnemy() => trackedEnemy;
}