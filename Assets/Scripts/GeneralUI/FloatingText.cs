using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class FloatingText : MonoBehaviour
{
    private TextMeshPro textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public async void Setup(string text, Color color, float duration = 1.2f, float speed = 1.5f)
    {
        textMesh.text = text;
        textMesh.color = color;

        float elapsedTime = 0f;
        Color originalColor = textMesh.color;

        while (elapsedTime < duration) {
            if (this == null || gameObject == null) return;
            transform.Translate(Vector3.up * (speed * Time.deltaTime), Space.World);
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            await Task.Yield();
        }

        if (this != null && gameObject != null) {
            Destroy(gameObject);
        }
    }
}