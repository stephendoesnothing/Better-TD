using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TroopButton : MonoBehaviour
{
    [Header("Troop Info")]
    [SerializeField] private TroopData troopData;
    [SerializeField] private int buttonIndex = 0; // Index for number key binding (0-8 for keys 1-9)

    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;

    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color cannotAffordColor = Color.red;
    [SerializeField] private Image backgroundImage;

    private TowerPlacement towerPlacement;
    private GameManager gameManager;
    private bool isSelected = false;

    private void Start()
    {
        towerPlacement = FindFirstObjectByType<TowerPlacement>();
        gameManager = FindFirstObjectByType<GameManager>();

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);

        // Initialize UI
        if (troopData != null)
        {
            if (iconImage != null && troopData.icon != null)
                iconImage.sprite = troopData.icon;

            if (costText != null)
                costText.text = $"${troopData.cost}";

            if (nameText != null)
                nameText.text = troopData.troopName;
        }
    }

    private void Update()
    {
        // Update visual state based on affordability
        if (troopData != null && gameManager != null && backgroundImage != null)
        {
            // Can't afford takes priority over selected state
            if (gameManager.CurrentCash < troopData.cost)
            {
                backgroundImage.color = cannotAffordColor;
            }
            else if (isSelected)
            {
                backgroundImage.color = selectedColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }
    }

    private void OnButtonClicked()
    {
        if (troopData == null || towerPlacement == null) return;

        // Check if player can afford
        if (gameManager != null && gameManager.CurrentCash < troopData.cost)
        {
            Debug.Log("Cannot afford this troop!");
            return;
        }

        // If clicking the same button again, deselect it
        if (isSelected)
        {
            SetSelected(false);
            towerPlacement.DeselectTroop();
            return;
        }

        // Deselect all other buttons
        TroopButton[] allButtons = FindObjectsByType<TroopButton>(FindObjectsSortMode.None);
        foreach (TroopButton btn in allButtons)
        {
            btn.SetSelected(false);
        }

        // Select this troop
        SetSelected(true);
        towerPlacement.SelectTroop(troopData);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    // Allow setting troop data at runtime
    public void SetTroopData(TroopData data)
    {
        troopData = data;

        if (troopData != null)
        {
            if (iconImage != null && troopData.icon != null)
                iconImage.sprite = troopData.icon;

            if (costText != null)
                costText.text = $"${troopData.cost}";

            if (nameText != null)
                nameText.text = troopData.troopName;
        }
    }

    // Allow external trigger of button click
    public void SimulateClick()
    {
        OnButtonClicked();
    }

    // Get button index for number key binding
    public int GetButtonIndex()
    {
        return buttonIndex;
    }
}