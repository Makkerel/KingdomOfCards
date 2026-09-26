using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using PrimeTween;

public class BuildingInfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI effectsText;

    [Header("Tooltip Position Settings")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(15f, 0f);

    [Header("Tween Settings")]
    [SerializeField] private float fadeDuration = 0.15f;

    private RectTransform panelRectTransform;
    private Tween fadeTween;

    private void Start()
    {
        panelRectTransform = uiPanel.GetComponent<RectTransform>();

        // Kept your exact events intact
        MouseHover.OnHoverTriggered += DisplayHoverInfo;
        MouseHover.OnHoverCleared += HideUI;

        uiPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        MouseHover.OnHoverTriggered -= DisplayHoverInfo;
        MouseHover.OnHoverCleared -= HideUI;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (uiPanel.activeSelf) {
            UpdatePanelPosition();
        }
    }

    private void DisplayHoverInfo(GameObject hoveredObject)
    {
        fadeTween.Stop();
        Cursor.visible = false;

        uiPanel.SetActive(true);
        UpdatePanelPosition();

        Health hp = hoveredObject.GetComponent<Health>();

        if (hoveredObject.TryGetComponent<PhysicalBuilding>(out var building)) {
            nameText.text = building.getName();
            healthText.text = hp != null ? $"HP: {hp.current_health} / {hp.getMaxHP()}" : "HP: -- / --";
            levelText.text = $"Level {building.getTier()}";
            effectsText.text = building.getEffects();
        }
        else if (hoveredObject.TryGetComponent<TroopInfo>(out var troop)) {
            nameText.text = troop.stats.troopClassName;
            healthText.text = $"Max HP: {hp.getMaxHP()}";
            levelText.text = troop.type.ToString();
            effectsText.text = troop.description();
        }

        fadeTween = Tween.Alpha(canvasGroup, endValue: 1f, duration: fadeDuration, ease: Ease.OutQuad);
    }

    private void HideUI()
    {
        fadeTween.Stop();
        Cursor.visible = true;

        fadeTween = Tween.Alpha(canvasGroup, endValue: 0f, duration: fadeDuration, ease: Ease.InQuad)
            .OnComplete(() => uiPanel.SetActive(false));
    }

    private void UpdatePanelPosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 targetPosition = mousePosition + cursorOffset;

        float panelWidth = panelRectTransform.rect.width * panelRectTransform.lossyScale.x;
        float panelHeight = panelRectTransform.rect.height * panelRectTransform.lossyScale.y;

        targetPosition.x = Mathf.Clamp(targetPosition.x, 0f, Screen.width - panelWidth);
        targetPosition.y = Mathf.Clamp(targetPosition.y, 0f, Screen.height - panelHeight);

        uiPanel.transform.position = targetPosition;
    }
}