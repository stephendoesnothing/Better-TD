using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TroopUpgradeUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject upgradePanel;

    [Header("Troop Info")]
    [SerializeField] private TextMeshProUGUI troopNameText;
    [SerializeField] private TextMeshProUGUI troopStatsText;
    [SerializeField] private TextMeshProUGUI totalSpentText;

    [Header("Upgrade Paths")]
    [SerializeField] private UpgradePathUI[] pathUIs = new UpgradePathUI[3];

    [Header("Sell Button")]
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellValueText;

    private TroopUpgradeController currentController;
    private Troop currentTroop;

    private void Start()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        if (sellButton != null)
        {
            Debug.Log("Sell button found, adding listener");
            sellButton.onClick.AddListener(OnSellClicked);
        }
        else
        {
            Debug.LogError("Sell button is not assigned in TroopUpgradeUI!");
        }

        // Setup path UI buttons
        for (int i = 0; i < pathUIs.Length; i++)
        {
            if (pathUIs[i] != null && pathUIs[i].upgradeButton != null)
            {
                int pathIndex = i; // Capture for lambda
                Debug.Log($"Adding listener for path {pathIndex} upgrade button");
                pathUIs[i].upgradeButton.onClick.AddListener(() => OnUpgradeClicked(pathIndex));
            }
            else
            {
                Debug.LogWarning($"Path {i} UI or upgrade button is not assigned!");
            }
        }
    }

    private void Update()
    {
        if (currentController != null && upgradePanel.activeSelf)
        {
            UpdateUI();
        }
    }

    public void ShowUpgradeUI(TroopUpgradeController controller)
    {
        currentController = controller;
        currentTroop = controller.GetComponent<Troop>();

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        }

        UpdateUI();
    }

    public void HideUpgradeUI()
    {
        currentController = null;
        currentTroop = null;

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    public GameObject GetUpgradePanel()
    {
        return upgradePanel;
    }

    private void UpdateUI()
    {
        if (currentController == null) return;

        // Update troop name - get from GameObject name
        if (troopNameText != null)
        {
            string displayName = currentTroop.name.Replace("(Clone)", "").Trim();
            troopNameText.text = displayName;
        }

        // Update stats
        if (troopStatsText != null && currentTroop != null)
        {
            troopStatsText.text = $"DMG: {currentTroop.GetDamage():F1}  |  " +
                                 $"RNG: {currentTroop.GetRange():F1}  |  " +
                                 $"FR: {currentTroop.GetFireRate():F2}";
        }

        // Update total spent
        if (totalSpentText != null)
        {
            totalSpentText.text = $"Total Spent: ${currentController.GetTotalSpent()}";
        }

        // Update sell value
        if (sellValueText != null)
        {
            int sellValue = currentController.GetSellValue();
            sellValueText.text = $"${sellValue}";
        }

        // Update each path
        for (int i = 0; i < 3; i++)
        {
            UpdatePathUI(i);
        }
    }

    private void UpdatePathUI(int pathIndex)
    {
        if (pathIndex >= pathUIs.Length) return;

        UpgradePathUI pathUI = pathUIs[pathIndex];
        if (pathUI == null) return;

        UpgradePath path = currentController.GetPath(pathIndex);
        if (path == null)
        {
            Debug.LogWarning($"Path {pathIndex} is null on upgrade controller");
            return;
        }

        int currentTier = currentController.GetCurrentTier(pathIndex);

        // Update path name
        if (pathUI.pathNameText != null)
        {
            pathUI.pathNameText.text = path.pathName;
        }

        // Update tier indicator
        if (pathUI.tierText != null)
        {
            pathUI.tierText.text = $"{currentTier}/5";
        }

        // Update tier dots
        if (pathUI.tierDots != null)
        {
            for (int i = 0; i < pathUI.tierDots.Length && i < 5; i++)
            {
                if (pathUI.tierDots[i] != null)
                {
                    pathUI.tierDots[i].color = i < currentTier ? path.pathColor : Color.gray;
                }
            }
        }

        // Get next upgrade
        UpgradeData nextUpgrade = path.GetUpgrade(currentTier);
        bool canUpgrade = currentController.CanUpgrade(pathIndex);

        // Update button
        if (pathUI.upgradeButton != null)
        {
            pathUI.upgradeButton.interactable = canUpgrade && nextUpgrade != null;

            if (nextUpgrade != null)
            {
                // Update upgrade name
                if (pathUI.upgradeNameText != null)
                {
                    pathUI.upgradeNameText.text = nextUpgrade.upgradeName;
                }

                // Update cost
                if (pathUI.upgradeCostText != null)
                {
                    pathUI.upgradeCostText.text = $"${nextUpgrade.cost}";
                }

                // Update description
                if (pathUI.upgradeDescriptionText != null)
                {
                    pathUI.upgradeDescriptionText.text = nextUpgrade.description;
                }

                // Update icon
                if (pathUI.upgradeIcon != null)
                {
                    if (nextUpgrade.icon != null)
                    {
                        pathUI.upgradeIcon.sprite = nextUpgrade.icon;
                        pathUI.upgradeIcon.enabled = true;
                    }
                    else
                    {
                        pathUI.upgradeIcon.enabled = false;
                    }
                }
            }
            else
            {
                // Max tier reached
                if (pathUI.upgradeNameText != null)
                {
                    pathUI.upgradeNameText.text = "MAX";
                }
                if (pathUI.upgradeCostText != null)
                {
                    pathUI.upgradeCostText.text = "";
                }
                if (pathUI.upgradeDescriptionText != null)
                {
                    pathUI.upgradeDescriptionText.text = "Maximum tier reached";
                }
                if (pathUI.upgradeIcon != null)
                {
                    pathUI.upgradeIcon.enabled = false;
                }
            }
        }
    }

    private void OnUpgradeClicked(int pathIndex)
    {
        Debug.Log($"OnUpgradeClicked called for path {pathIndex}!");

        if (currentController == null)
        {
            Debug.LogError("currentController is null!");
            return;
        }

        if (currentController.CanUpgrade(pathIndex))
        {
            Debug.Log($"Purchasing upgrade for path {pathIndex}");
            currentController.PurchaseUpgrade(pathIndex);
            UpdateUI();
        }
        else
        {
            Debug.Log($"Cannot upgrade path {pathIndex + 1}");
        }
    }

    public void OnSellClicked()
    {
        Debug.Log("OnSellClicked called!");

        if (currentController == null || currentTroop == null)
        {
            Debug.LogError("currentController or currentTroop is null!");
            return;
        }

        // Refund money
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            int sellValue = currentController.GetSellValue();
            gm.AddCash(sellValue);
            Debug.Log($"Sold troop for ${sellValue}");
        }

        // Destroy troop
        GameObject troopObj = currentTroop.gameObject;
        HideUpgradeUI();
        Destroy(troopObj);

        Debug.Log("Troop sold and destroyed!");
    }
}

[System.Serializable]
public class UpgradePathUI
{
    [Header("Path Info")]
    public TextMeshProUGUI pathNameText;
    public TextMeshProUGUI tierText;
    public Image[] tierDots = new Image[5];

    [Header("Next Upgrade")]
    public Button upgradeButton;
    public Image upgradeIcon;
    public TextMeshProUGUI upgradeNameText;
    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI upgradeDescriptionText;
}