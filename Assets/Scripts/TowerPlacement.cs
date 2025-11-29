using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float gridSize = 1f;

    [Header("Visual Feedback")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;

    private TroopData selectedTroop = null;
    private GameObject placementPreview;
    private bool canPlace = false;
    private Vector3 placementPosition;
    private Camera mainCam;
    private int previewLayer;

    private void Start()
    {
        mainCam = Camera.main;
        previewLayer = LayerMask.NameToLayer("Preview");
    }

    private void Update()
    {
        // Check for deselect key (X)
        if (Input.GetKeyDown(KeyCode.X))
        {
            DeselectTroop();
            return;
        }

        // Check for number key selection (1-9)
        HandleNumberKeySelection();

        // Only show preview if a troop is selected
        if (selectedTroop == null || placementPreview == null) return;

        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCam.ScreenPointToRay(mousePos);

        // Check if mouse is over UI to prevent placing on UI
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            placementPreview.SetActive(false);
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Snap to grid
            placementPosition = new Vector3(hit.point.x, hit.point.y + 1.1f, hit.point.z);

            canPlace = IsValidPlacement(placementPosition);

            // Update preview
            placementPreview.SetActive(true);
            placementPreview.transform.position = placementPosition;

            UpdatePreviewColor(canPlace);

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceTroop();
            }
        }
        else
        {
            placementPreview.SetActive(false);
        }
    }

    public void SelectTroop(TroopData troopData)
    {
        // Clean up old preview
        if (placementPreview != null)
        {
            Destroy(placementPreview);
        }

        selectedTroop = troopData;

        // Create new preview
        if (troopData != null && troopData.prefab != null)
        {
            placementPreview = Instantiate(troopData.prefab);
            placementPreview.GetComponent<Troop>().enabled = false;

            // Set preview and all children to Preview layer
            SetLayerRecursively(placementPreview, previewLayer);

            // Make transparent
            Renderer[] renderers = placementPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Material[] mats = r.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Color c = mats[i].color;
                    c.a = 0.5f;
                    mats[i].color = c;
                }
                r.materials = mats;
            }

            placementPreview.SetActive(false);
        }
    }

    public void DeselectTroop()
    {
        selectedTroop = null;

        if (placementPreview != null)
        {
            Destroy(placementPreview);
            placementPreview = null;
        }

        // Deselect all buttons
        TroopButton[] allButtons = FindObjectsByType<TroopButton>(FindObjectsSortMode.None);
        foreach (TroopButton btn in allButtons)
        {
            btn.SetSelected(false);
        }
    }

    private void HandleNumberKeySelection()
    {
        // Find all troop buttons
        TroopButton[] allButtons = FindObjectsByType<TroopButton>(FindObjectsSortMode.None);

        // Check keys 1-9
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(GetNumberKeyCode(i)))
            {
                // Find button with matching index (i-1 because array is 0-indexed but keys are 1-9)
                foreach (TroopButton btn in allButtons)
                {
                    if (btn.GetButtonIndex() == i - 1)
                    {
                        btn.SimulateClick();
                        break;
                    }
                }
                break;
            }
        }
    }

    private KeyCode GetNumberKeyCode(int number)
    {
        return number switch
        {
            1 => KeyCode.Alpha1,
            2 => KeyCode.Alpha2,
            3 => KeyCode.Alpha3,
            4 => KeyCode.Alpha4,
            5 => KeyCode.Alpha5,
            6 => KeyCode.Alpha6,
            7 => KeyCode.Alpha7,
            8 => KeyCode.Alpha8,
            9 => KeyCode.Alpha9,
            _ => KeyCode.None
        };
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private bool IsValidPlacement(Vector3 position)
    {
        // Check for already existing troop (excluding preview layer)
        Collider[] colliders = Physics.OverlapSphere(position, gridSize / 2f);

        foreach (Collider col in colliders)
        {
            // Skip if this is the preview object
            if (col.gameObject.layer == previewLayer)
            {
                continue;
            }

            if (col.GetComponent<Troop>() != null)
            {
                return false;
            }
        }

        // Check if player has enough cash
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null && gameManager.CurrentCash < selectedTroop.cost)
        {
            return false;
        }

        return true;
    }

    private void UpdatePreviewColor(bool valid)
    {
        Renderer[] renderers = placementPreview.GetComponentsInChildren<Renderer>();
        Material matToUse = valid ? validPlacementMaterial : invalidPlacementMaterial;

        if (matToUse != null)
        {
            foreach (Renderer r in renderers)
            {
                Material[] mats = r.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = matToUse;
                    Color c = mats[i].color;
                    c.a = 0.5f;
                    mats[i].color = c;
                }
                r.materials = mats;
            }
        }
    }

    private void PlaceTroop()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return;

        if (gameManager.CurrentCash < selectedTroop.cost) return;

        gameManager.SpendCash(selectedTroop.cost);

        GameObject troop = Instantiate(selectedTroop.prefab, placementPosition, Quaternion.identity);

        // Deselect troop after placing
        DeselectTroop();
    }
}