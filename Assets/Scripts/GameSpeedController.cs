// ========== GameSpeedController.cs ==========
// Create new script - attach to any GameObject in the scene
using UnityEngine;
using TMPro;

public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float[] speedOptions = { 0f, 0.25f, 0.5f, 1f, 2f, 3f };
    [SerializeField] private int defaultSpeedIndex = 3; // Index for 1x speed

    [Header("UI References (Optional)")]
    [SerializeField] private TextMeshProUGUI speedDisplayText;

    private int currentSpeedIndex;

    private void Start()
    {
        currentSpeedIndex = defaultSpeedIndex;
        SetGameSpeed(speedOptions[currentSpeedIndex]);
    }

    private void Update()
    {
        // Pause/Unpause with Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.timeScale == 0f)
            {
                // Unpause - return to previous speed
                SetGameSpeed(speedOptions[currentSpeedIndex]);
            }
            else
            {
                // Pause
                SetGameSpeed(0f);
            }
        }

        // Speed up with Equals/Plus key
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (currentSpeedIndex < speedOptions.Length - 1)
            {
                currentSpeedIndex++;
                SetGameSpeed(speedOptions[currentSpeedIndex]);
            }
        }

        // Speed down with Minus key
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (currentSpeedIndex > 0)
            {
                currentSpeedIndex--;
                SetGameSpeed(speedOptions[currentSpeedIndex]);
            }
        }
    }

    private void SetGameSpeed(float speed)
    {
        Time.timeScale = speed;

        string speedText;
        if (speed == 0f)
        {
            speedText = "PAUSED";
        }
        else
        {
            speedText = $"{speed}x";
        }

        //  Debug.Log($"<color=cyan>Game Speed: {speedText}</color>");

        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"Speed: {speedText}";
        }
    }

    // Public methods for UI buttons if you want to add them
    public void PauseGame()
    {
        SetGameSpeed(0f);
    }

    public void ResumeGame()
    {
        SetGameSpeed(speedOptions[currentSpeedIndex]);
    }

    public void SetSpeed(float speed)
    {
        // Find closest speed option
        for (int i = 0; i < speedOptions.Length; i++)
        {
            if (Mathf.Approximately(speedOptions[i], speed))
            {
                currentSpeedIndex = i;
                SetGameSpeed(speed);
                return;
            }
        }
    }

    public void IncreaseSpeed()
    {
        if (currentSpeedIndex < speedOptions.Length - 1)
        {
            currentSpeedIndex++;
            SetGameSpeed(speedOptions[currentSpeedIndex]);
        }
    }

    public void DecreaseSpeed()
    {
        if (currentSpeedIndex > 0)
        {
            currentSpeedIndex--;
            SetGameSpeed(speedOptions[currentSpeedIndex]);
        }
    }
}