using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraControl : MonoBehaviour
{
    [Header("Panning Settings")]
    [SerializeField] private float panSpeed = 15f;

    [Header("Zoom & Tilt Settings")]
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float zoomSpeed = 4f;

    [SerializeField] private float zoomOutHeight = 12f;
    [SerializeField] private float zoomInHeight = 3f;

    [SerializeField] private float zoomOutPitch = 60f;
    [SerializeField] private float zoomInPitch = 25f;

    private InputSystem_Actions inputActions;
    private CinemachineCamera cmCamera;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Vector3 m_PanOffset;
    private float targetZoom = 0f;
    private float currentZoom = 0f;

    void Awake()
    {
        cmCamera = GetComponent<CinemachineCamera>();
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        CinemachineCore.CameraActivatedEvent.AddListener(OnCameraChanged);
        enabled = CinemachineCore.IsLive(cmCamera);
    }

    void OnEnable()
    {
        inputActions.Enable();
    }
    void OnDisable()
    {
        inputActions.Disable();
    }
    void OnDestroy()
    {
        CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraChanged);
        inputActions?.Dispose();
    }

    private void OnCameraChanged(ICinemachineCamera.ActivationEventParams evt)
    {
        if (evt.IncomingCamera == (ICinemachineCamera)cmCamera) {
            enabled = true;
        }
        else {
            enabled = false;
        }
    }

    private void Update()
    {
        HandlePanning();
        HandleZooming();
        UpdateCameraTransform();
    }

    private void HandlePanning()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if (moveInput != Vector2.zero) {

            Quaternion horizontalHeading = Quaternion.Euler(0, originalLocalRotation.eulerAngles.y, 0);

            Vector3 movementDirection = new Vector3(moveInput.x, 0, moveInput.y);
            Vector3 orientedMove = horizontalHeading * movementDirection;

            m_PanOffset += orientedMove * panSpeed * Time.deltaTime;
        }
    }

    private void HandleZooming()
    {
        Vector2 scrollInput = inputActions.UI.ScrollWheel.ReadValue<Vector2>();
        if (scrollInput.y != 0) {
            float scrollDirection = Mathf.Sign(scrollInput.y);

            targetZoom = Mathf.Clamp01(targetZoom + (scrollDirection * zoomSensitivity));
        }

        currentZoom = Mathf.MoveTowards(currentZoom, targetZoom, zoomSpeed * Time.deltaTime);
    }

    private void UpdateCameraTransform()
    {
        float currentHeight = Mathf.Lerp(zoomOutHeight, zoomInHeight, currentZoom);
        float dynamicPitch = Mathf.Lerp(zoomOutPitch, zoomInPitch, currentZoom);

        Vector3 targetPosition = new Vector3(
            originalLocalPosition.x + m_PanOffset.x,
            currentHeight,
            originalLocalPosition.z + m_PanOffset.z
        );

        transform.localPosition = targetPosition;
        transform.localRotation = Quaternion.Euler(dynamicPitch, originalLocalRotation.eulerAngles.y, originalLocalRotation.eulerAngles.z);
    }

    public void ResetToHome()
    {
        m_PanOffset = Vector3.zero;
        targetZoom = 0f;
        currentZoom = 0f;
        UpdateCameraTransform();
    }
}