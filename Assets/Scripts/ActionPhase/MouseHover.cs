using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MouseHover : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask building_layer;
    [SerializeField] private float max_distance = 100f;
    [SerializeField] private float hover_delay = 0.3f; 

    private Camera mainCamera;
    private Hoverable current_hoverable;
    private Collider previous_collider;

    private float hover_timer = 0f;
    private bool hover_event_triggered = false;

    public static event Action<GameObject> OnHoverTriggered;
    public static event Action OnHoverCleared;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, max_distance, building_layer)) {
            Collider collider = hit.collider;
            Hoverable hoverable = collider.GetComponentInParent<Hoverable>();

            if (hoverable != null) {
                if (collider == previous_collider) {
                    if (!hover_event_triggered) {
                        hover_timer += Time.deltaTime;
                        if (hover_timer >= hover_delay) {
                            hover_event_triggered = true;
                            OnHoverTriggered?.Invoke(hoverable.gameObject);
                        }
                    }
                    return;
                }

                ClearCurrentHover();
                current_hoverable = hoverable;
                current_hoverable.OutlineStatus(true);
                previous_collider = collider;
                return;
            }
        }
        ClearCurrentHover();
    }

    private void ClearCurrentHover()
    {
        if (current_hoverable != null) {
            current_hoverable.OutlineStatus(false);
            current_hoverable = null;
            previous_collider = null;
        }

        if (hover_event_triggered || hover_timer > 0f) {
            hover_timer = 0f;
            hover_event_triggered = false;
            OnHoverCleared?.Invoke();
        }
    }
}