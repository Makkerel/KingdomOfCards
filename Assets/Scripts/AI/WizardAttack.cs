using UnityEngine;

public class WizardPolygonBeam : BaseAttack
{
    [Header("Polygon Arsenal Prefabs")]
    [SerializeField] private GameObject beamLineRendererPrefab;
    [SerializeField] private GameObject beamStartPrefab;
    [SerializeField] private GameObject beamEndPrefab;

    [Header("Beam Options")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private bool beamCollides = true;
    [SerializeField] private float maxBeamLength = 50f;
    [SerializeField] private float targetHeightOffset = 1.2f;
    [SerializeField] private float textureScrollSpeed = 3f;
    [SerializeField] private float textureLengthScale = 1f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Width Pulse Options")]
    [SerializeField] private float widthMultiplier = 1.5f;
    [SerializeField] private float pulseSpeed = 5.0f;

    private GameObject instantiatedBeamParent;
    private GameObject beamStartInstance;
    private GameObject beamEndInstance;
    private LineRenderer lineRenderer;
    private Material beamMaterial;

    private Transform currentTarget;
    private float originalWidth;
    private float customWidth;

    protected override void Awake()
    {
        base.Awake();
        InitializePolygonBeam();
        enabled = false;
    }

    private void InitializePolygonBeam()
    {
        instantiatedBeamParent = Instantiate(beamLineRendererPrefab, transform);
        instantiatedBeamParent.transform.position = firePoint.position;

        lineRenderer = instantiatedBeamParent.GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        beamMaterial = lineRenderer.material;

        originalWidth = lineRenderer.startWidth;
        customWidth = originalWidth * widthMultiplier;

        beamStartInstance = Instantiate(beamStartPrefab, instantiatedBeamParent.transform);
        beamEndInstance = Instantiate(beamEndPrefab, instantiatedBeamParent.transform);

        instantiatedBeamParent.SetActive(false);
    }

    protected override void ExecuteAttack(Transform target)
    {
        currentTarget = target;

        if (target.TryGetComponent(out Health enemyHealth)) {
            enemyHealth.TakeDamage(damage);
        }

        RunBeamVisualTracking();

        if (!enabled) {
            enabled = true;
            instantiatedBeamParent.SetActive(true);
        }
    }

    private void Update()
    {
        float dynamicBuffer = Mathf.Max(attack_cooldown * 1.2f, 0.1f);
        bool isAttacking = currentTarget != null && Time.time <= last_attack_time + dynamicBuffer;

        if (isAttacking) RunBeamVisualTracking();
        else ShutDownBeamVisuals();
    }

    private void RunBeamVisualTracking()
    {
        Vector3 origin = firePoint.position;
        Vector3 targetPosition = currentTarget.position + (Vector3.up * targetHeightOffset);
        Vector3 direction = (targetPosition - origin).normalized;
        Vector3 endPoint = origin + (direction * maxBeamLength);

        if (beamCollides && Physics.Raycast(origin, direction, out RaycastHit hit, maxBeamLength, enemyLayer)) {
            endPoint = hit.point;
        }

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, endPoint);

        beamStartInstance.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
        beamEndInstance.transform.SetPositionAndRotation(endPoint, Quaternion.LookRotation(-direction));

        float distance = Vector3.Distance(origin, endPoint);
        beamMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        beamMaterial.mainTextureOffset = new Vector2(Time.time * textureScrollSpeed, 0);

        float pulseFactor = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float currentWidth = Mathf.Lerp(originalWidth, customWidth, pulseFactor);

        lineRenderer.startWidth = currentWidth;
        lineRenderer.endWidth = currentWidth;
    }

    private void ShutDownBeamVisuals()
    {
        currentTarget = null;
        instantiatedBeamParent.SetActive(false);
        enabled = false;
    }

    private void OnDisable() => ShutDownBeamVisuals();
}