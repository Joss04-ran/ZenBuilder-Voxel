using UnityEngine;
using UnityEngine.Profiling; 

public class PerformanceMonitor : MonoBehaviour
{
    [Header("Toggle Settings")]
    public KeyCode toggleKey = KeyCode.F2;

    private bool showStats = false;
    private float deltaTime = 0.0f;

    void Update()
    {
        // Toggle the performance display on/off
        if (Input.GetKeyDown(toggleKey))
        {
            showStats = !showStats;
        }

        // Calculate a smoothed delta time for more stable FPS reading
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        // If the toggle is off, do not draw anything
        if (!showStats) return;

        // Calculate Frame Rate (FPS) and Milliseconds (ms)
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        // Calculate Memory Usage in Megabytes (MB)
        long memoryBytes = Profiler.GetTotalAllocatedMemoryLong();
        float memoryMB = memoryBytes / (1024f * 1024f);

        // Format the display text
        string text = string.Format("Performance Stats:\n{0:0.0} ms ({1:0.} FPS)\nMemory: {2:0.0} MB", msec, fps, memoryMB);

        // Setup the visual style for the text
        int width = Screen.width;
        int height = Screen.height;
        GUIStyle style = new GUIStyle();

        // Position the text at the top left corner with a little padding
        Rect rect = new Rect(20, 20, width, height * 2 / 100);

        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 24; // You can adjust the font size here

        // Color the text based on FPS (Green = Good, Red = Bad)
        if (fps >= 60f)
            style.normal.textColor = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green
        else if (fps >= 30f)
            style.normal.textColor = new Color(1.0f, 0.92f, 0.016f, 1.0f); // Yellow
        else
            style.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red

        // 5. Draw the text on the screen
        GUI.Label(rect, text, style);
    }
}