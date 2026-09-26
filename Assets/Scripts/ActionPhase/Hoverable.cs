using UnityEngine;
public class Hoverable : MonoBehaviour
{
    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null) {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 4f;
        outline.enabled = false;
    }

    public void OutlineStatus(bool show)
    {
        if (outline != null) {
            outline.enabled = show;
        }
    }

    public void ChangeColor(Color color)
    {
        if (outline != null) {
            outline.OutlineColor = color;
        }
    }
}