using UnityEngine;

public class TroopSelectionManager : MonoBehaviour
{
    private Troop selectedTroop;
    private TroopUpgradeController selectedUpgradeController;
    private Camera mainCam;
    private TroopUpgradeUI upgradeUI;
    private TowerPlacement towerPlacement;

    private bool isProcessingInput = true;

    private void Start()
    {
        mainCam = Camera.main;
        upgradeUI = FindFirstObjectByType<TroopUpgradeUI>();
        towerPlacement = FindFirstObjectByType<TowerPlacement>();
    }

    private void Update()
    {
        // HOTKEYS FOR TESTING UPGRADES
        if (selectedUpgradeController != null)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (selectedUpgradeController.CanUpgrade(0))
                {
                    selectedUpgradeController.PurchaseUpgrade(0);
                }
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                if (selectedUpgradeController.CanUpgrade(1))
                {
                    selectedUpgradeController.PurchaseUpgrade(1);
                }
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (selectedUpgradeController.CanUpgrade(2))
                {
                    selectedUpgradeController.PurchaseUpgrade(2);
                }
            }
        }

        // Don't process selection if we're in placement mode
        if (towerPlacement != null && towerPlacement.IsInPlacementMode)
            return;

        // Check for troop selection
        if (Input.GetMouseButtonDown(0))
        {
            // If upgrade UI is open, allow clicks to close it
            if (!isProcessingInput)
            {
                // Allow deselecting when clicking outside troops
                TryDeselectOrSelect();
            }
            else
            {
                // Normal selection behavior
                TrySelectTroop();
            }
        }

        // Deselect with Escape or right click
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            DeselectTroop();
        }
    }

    private void TryDeselectOrSelect()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Troop troop = hit.collider.GetComponent<Troop>();

            if (troop != null)
            {
                // If clicking on a different troop, switch selection
                if (troop != selectedTroop)
                {
                    SelectTroop(troop);
                }
                // If clicking on same troop, do nothing (keep menu open)
            }
            else
            {
                // Clicked on ground/something else, deselect
                DeselectTroop();
            }
        }
        else
        {
            // Clicked on nothing, deselect
            DeselectTroop();
        }
    }

    private void TrySelectTroop()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Troop troop = hit.collider.GetComponent<Troop>();

            if (troop != null)
            {
                SelectTroop(troop);
            }
            else
            {
                DeselectTroop();
            }
        }
    }

    private void SelectTroop(Troop troop)
    {
        if (selectedTroop != null)
        {
            selectedTroop.SetSelected(false);
        }

        selectedTroop = troop;
        selectedUpgradeController = troop.GetComponent<TroopUpgradeController>();
        selectedTroop.SetSelected(true);

        if (upgradeUI != null && selectedUpgradeController != null)
        {
            upgradeUI.ShowUpgradeUI(selectedUpgradeController);
            isProcessingInput = false; // Disable normal input processing when UI opens
        }
    }

    public void DeselectTroop()
    {
        if (selectedTroop != null)
        {
            selectedTroop.SetSelected(false);
            selectedTroop = null;
            selectedUpgradeController = null;
        }

        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradeUI();
        }

        isProcessingInput = true; // Re-enable normal input processing when UI closes
    }

    public Troop GetSelectedTroop() => selectedTroop;
    public TroopUpgradeController GetSelectedUpgradeController() => selectedUpgradeController;
}