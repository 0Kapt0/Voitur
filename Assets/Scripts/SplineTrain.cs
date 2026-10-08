using UnityEngine;
using UnityEngine.Splines;

public class TrainController : MonoBehaviour
{
    [Header("Spline")]
    [SerializeField] private SplineContainer _spline;
    [SerializeField] private float _bogieSpacing = 4f;

    [Header("Offset")]
    [SerializeField] private float _lateralOffset = 0f;
    [SerializeField] private Vector3 _rotationOffset;

    [Header("Physics")]
    [SerializeField] private float _maxSpeed = 30f;
    [SerializeField] private float _acceleration = 1f;
    [SerializeField] private float _brakeDeceleration = 1.5f;
    [SerializeField] private float _drag = 0.02f;
    [SerializeField] private float _throttleResponse = 0.5f;

    [Header("Derailment")]
    [SerializeField] private float _maxLateralAcceleration = 4f;
    [SerializeField] private float _curvatureSampleDistance = 5f;

    public float Speed => _speed;
    public float Danger => _danger;
    public bool IsDerailed => _isDerailed;

    public float Progress => _distance / _splineLength;

    private Controls _controls;
    private float _splineLength;
    private float _distance;
    private float _speed;
    private float _smoothedThrottle;
    private float _danger;
    private bool _isDerailed;

    private void Awake()
    {
        _controls = new Controls();
    }

    private void OnEnable()
    {
        _controls.Train.Enable();
    }

    private void OnDisable()
    {
        _controls.Train.Disable();
    }

    private void Start()
    {
        _splineLength = _spline.CalculateLength();
    }

    private void MoveAlongSpline()
    {
        _distance = Mathf.Repeat(_distance + _speed * Time.deltaTime, _splineLength);
    }

    private void UpdateSpeed()
    {
        float throttle = _controls.Train.Accel.ReadValue<float>();
        float brake = _controls.Train.Brake.ReadValue<float>();

        _smoothedThrottle = Mathf.MoveTowards(_smoothedThrottle, throttle, _throttleResponse * Time.deltaTime);

        float acceleration = _smoothedThrottle * _acceleration * (1f - _speed / _maxSpeed);
        float braking = brake * _brakeDeceleration;
        float friction = _speed * _drag;

        _speed += (acceleration - braking - friction) * Time.deltaTime;
        _speed = Mathf.Clamp(_speed, 0f, _maxSpeed);
    }

    private Vector3 GetPoint(float distance)
    {
        float normalizedPosition = Mathf.Repeat(distance, _splineLength) / _splineLength;
        return _spline.EvaluatePosition(normalizedPosition);
    }

    private void UpdateTransform()
    {
        Vector3 front = GetPoint(_distance + _bogieSpacing * 0.5f);
        Vector3 rear = GetPoint(_distance - _bogieSpacing * 0.5f);

        Vector3 position = (front + rear) * 0.5f;
        Vector3 forward = (front - rear).normalized;

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        position += right * _lateralOffset;

        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(_rotationOffset);
        transform.SetPositionAndRotation(position, rotation);
    }

    private float GetCurvature(float distance)
    {
        float startPosition = Mathf.Repeat(distance, _splineLength) / _splineLength;
        float endPosition = Mathf.Repeat(distance + _curvatureSampleDistance, _splineLength) / _splineLength;

        Vector3 tangentA = ((Vector3)_spline.EvaluateTangent(startPosition)).normalized;
        Vector3 tangentB = ((Vector3)_spline.EvaluateTangent(endPosition)).normalized;

        float angle = Vector3.Angle(tangentA, tangentB) * Mathf.Deg2Rad;
        return angle / _curvatureSampleDistance;
    }

    private void CheckDerailment()
    {
        float lateralAcceleration = _speed * _speed * GetCurvature(_distance);
        _danger = lateralAcceleration / _maxLateralAcceleration;

        if (_danger >= 1f) Derail();
    }

    private void Derail()
    {
        _isDerailed = true;

        Rigidbody body = gameObject.AddComponent<Rigidbody>();
        body.mass = 5000f;
        body.linearVelocity = transform.forward * _speed;
        body.AddTorque(transform.forward * _speed * 200f, ForceMode.Impulse);
    }

    private void Update()
    {
        if (_isDerailed) return;

        UpdateSpeed();
        MoveAlongSpline();
        UpdateTransform();
        CheckDerailment();
    }
}