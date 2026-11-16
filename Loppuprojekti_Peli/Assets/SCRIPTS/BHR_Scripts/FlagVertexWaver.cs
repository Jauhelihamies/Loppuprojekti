using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FlagGridWaver : MonoBehaviour
{
    public enum FlagState
    {
        Initializing,
        Error_NoMeshFilter,
        Error_MeshNotReadable,
        Error_NoTargetAssigned,
        Paused_TooFarAway,
        Active_SlowTickRate,
        Active_FullTickRate
    }

    [Header("Live Debugger (Read-Only)")]
    [SerializeField] private FlagState currentState = FlagState.Initializing;
    [SerializeField] private float currentDistance = 0f;
    [SerializeField] private float activeTickRate = 0f;

    [Header("Wave Motion (Proportional to Flag Size)")]
    public float waveSpeed = 4.5f;
    public float horizontalFrequency = 3.0f;
    public float verticalFrequency = 1.5f;
    [Tooltip("Wave depth as a fraction of the flag's length (e.g. 0.08 = 8% of flag width).")]
    [Range(0.01f, 0.5f)]
    public float waveStrength = 0.08f;
    [Tooltip("Downward fabric droop as a fraction of flag length (e.g. 0.03 = 3%).")]
    [Range(0f, 0.3f)]
    public float droopStrength = 0.03f;

    [Header("Pinning Direction")]
    [Tooltip("Check this if the wrong end of the flag is pinned.")]
    public bool flipPinnedSide = false;

    [Header("Distance & Tick Rate Optimization")]
    public Transform playerOrCamera;
    public float maxWaveDistance = 80f;
    public float midWaveDistance = 25f;
    public float closeTickRate = 60f;
    public float midTickRate = 15f;

    [Header("Debug Visuals")]
    public bool showGizmos = true;

    private Mesh _mesh;
    private Renderer _renderer;
    private Vector3[] _baseVertices;
    private Vector3[] _displacedVertices;

    private Vector3 _autoLengthAxis;
    private Vector3 _autoHeightAxis;
    private Vector3 _autoOutAxis;

    private float _minLength, _maxLength, _totalLength;
    private float _minHeight, _maxHeight, _totalHeight;
    private float _tickTimer;
    private float _phaseOffset;
    private bool _isReset = false;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        _renderer = GetComponent<Renderer>();

        if (mf == null || mf.sharedMesh == null)
        {
            SetState(FlagState.Error_NoMeshFilter);
            return;
        }

        _mesh = mf.mesh;
        if (!_mesh.isReadable)
        {
            SetState(FlagState.Error_MeshNotReadable);
            enabled = false;
            return;
        }

        _mesh.MarkDynamic();
        _baseVertices = _mesh.vertices;
        _displacedVertices = new Vector3[_baseVertices.Length];

        if (playerOrCamera == null && Camera.main != null)
        {
            playerOrCamera = Camera.main.transform;
        }

        _phaseOffset = Random.Range(0f, 10f);
        _tickTimer = Random.Range(0f, 0.05f);

        AutoDetectMeshAxes();
    }

    void AutoDetectMeshAxes()
    {
        Bounds b = _mesh.bounds;
        Vector3 size = b.size;

        if (size.x <= size.y && size.x <= size.z)
        {
            _autoOutAxis = Vector3.right;
            _autoLengthAxis = (size.z >= size.y) ? Vector3.forward : Vector3.up;
            _autoHeightAxis = (size.z >= size.y) ? Vector3.up : Vector3.forward;
        }
        else if (size.y <= size.x && size.y <= size.z)
        {
            _autoOutAxis = Vector3.up;
            _autoLengthAxis = (size.x >= size.z) ? Vector3.right : Vector3.forward;
            _autoHeightAxis = (size.x >= size.z) ? Vector3.forward : Vector3.right;
        }
        else
        {
            _autoOutAxis = Vector3.forward;
            _autoLengthAxis = (size.x >= size.y) ? Vector3.right : Vector3.up;
            _autoHeightAxis = (size.x >= size.y) ? Vector3.up : Vector3.right;
        }

        _minLength = Vector3.Dot(b.min, _autoLengthAxis);
        _maxLength = Vector3.Dot(b.max, _autoLengthAxis);
        _totalLength = Mathf.Max(0.001f, _maxLength - _minLength);

        _minHeight = Vector3.Dot(b.min, _autoHeightAxis);
        _maxHeight = Vector3.Dot(b.max, _autoHeightAxis);
        _totalHeight = Mathf.Max(0.001f, _maxHeight - _minHeight);
    }

    Vector3 GetFlagCenter()
    {
        return (_renderer != null) ? _renderer.bounds.center : transform.position;
    }

    void Update()
    {
        if (playerOrCamera == null)
        {
            if (Camera.main != null) playerOrCamera = Camera.main.transform;
            else
            {
                SetState(FlagState.Error_NoTargetAssigned);
                return;
            }
        }

        float sqrDist = (GetFlagCenter() - playerOrCamera.position).sqrMagnitude;
        currentDistance = Mathf.Sqrt(sqrDist);

        if (sqrDist > maxWaveDistance * maxWaveDistance)
        {
            activeTickRate = 0f;
            SetState(FlagState.Paused_TooFarAway);
            if (!_isReset) ResetMesh();
            return;
        }

        if (sqrDist > midWaveDistance * midWaveDistance)
        {
            activeTickRate = midTickRate;
            SetState(FlagState.Active_SlowTickRate);
        }
        else
        {
            activeTickRate = closeTickRate;
            SetState(FlagState.Active_FullTickRate);
        }

        float tickInterval = 1f / Mathf.Max(1f, activeTickRate);
        _tickTimer += Time.deltaTime;

        if (_tickTimer >= tickInterval)
        {
            _tickTimer %= tickInterval;
            AnimateGrid();
            _isReset = false;
        }
    }

    void AnimateGrid()
    {
        float time = (Time.time + _phaseOffset) * waveSpeed;

        // Scale wave amplitude proportionally to the flag's local length
        float actualAmplitude = _totalLength * waveStrength;
        float actualDroop = _totalLength * droopStrength;

        for (int i = 0; i < _baseVertices.Length; i++)
        {
            Vector3 v = _baseVertices[i];

            float u = Mathf.Clamp01((Vector3.Dot(v, _autoLengthAxis) - _minLength) / _totalLength);
            if (flipPinnedSide) u = 1f - u;

            float w = Mathf.Clamp01((Vector3.Dot(v, _autoHeightAxis) - _minHeight) / _totalHeight);
            float pinFactor = u * u * (3f - 2f * u);

            float primaryWave = Mathf.Sin((u * horizontalFrequency * Mathf.PI) + (w * verticalFrequency) - time);
            float flutter = Mathf.Cos((u * horizontalFrequency * 2.2f * Mathf.PI) - (time * 1.4f)) * 0.2f;

            float outDisplacement = (primaryWave + flutter) * actualAmplitude * pinFactor;
            float droopOffset = -actualDroop * (u * u);

            _displacedVertices[i] = v + (_autoOutAxis * outDisplacement) + (_autoHeightAxis * droopOffset);
        }

        _mesh.vertices = _displacedVertices;
        _mesh.RecalculateNormals();
    }

    void ResetMesh()
    {
        _mesh.vertices = _baseVertices;
        _mesh.RecalculateNormals();
        _isReset = true;
        _tickTimer = 0f;
    }

    void SetState(FlagState newState)
    {
        if (currentState != newState) currentState = newState;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 center = (_renderer != null) ? _renderer.bounds.center : transform.position;

        Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
        Gizmos.DrawWireSphere(center, midWaveDistance);

        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawWireSphere(center, maxWaveDistance);
    }
}