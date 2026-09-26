using UnityEngine;
using UnityEngine.InputSystem; // Modern input namespace

public class CursorDebugger : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;

    [Range(0, 128)] public int hotspotX = 0;
    [Range(0, 128)] public int hotspotY = 0;

    private int lastX, lastY;

    private void Update()
    {
        // Real-time slider checking loop
        if (hotspotX != lastX || hotspotY != lastY) {
            Vector2 liveHotspot = new Vector2(hotspotX, hotspotY);
            Cursor.SetCursor(cursorTexture, liveHotspot, CursorMode.Auto);

            lastX = hotspotX;
            lastY = hotspotY;
        }
    }

    private void OnGUI()
    {
        // Safety check to ensure a mouse device is actively connected
        if (Mouse.current == null) return;

        // Modern API vector readout block
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Geometric inversion math: Convert Bottom-Left (Input System) to Top-Left (OnGUI Layout)
        float guiY = Screen.height - mousePos.y;

        GUI.backgroundColor = Color.cyan;

        // Draws the 6x6 target box right where the new input system calculates clicks
        GUI.Box(new Rect(mousePos.x - 3, guiY - 3, 6, 6), "");
    }
}