using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class CarController : MonoBehaviour
{
    public float _motorForce = 1500f;
    public float _brakeForce = 1000f;
    public float _maxSteerAngle = 30f;

    public WheelCollider _frontLeftWheelCollider;
    public WheelCollider _frontRightWheelCollider;
    public WheelCollider _backLeftWheelCollider;
    public WheelCollider _backRightWheelCollider;

    public Transform _frontLeftWheelTransform;
    public Transform _frontRightWheelTransform;
    public Transform _backLeftWheelTransform;
    public Transform _backRightWheelTransform;

    public Light _leftPointLight;
    public Light _rightPointLight;

    private Quaternion _frontLeftOffset;
    private Quaternion _frontRightOffset;
    private Quaternion _backLeftOffset;
    private Quaternion _backRightOffset;

    private float _horizontalInput;
    private float _verticalInput;
    private float _currentSteerAngle;
    private float _currentBrakeForce;
    private bool _isBraking;

    private CarControls _controls;

    private void Awake()
    {
        _controls = new CarControls();
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    private void Start()
    {
        _frontLeftOffset = GetOffset(_frontLeftWheelCollider, _frontLeftWheelTransform);
        _frontRightOffset = GetOffset(_frontRightWheelCollider, _frontRightWheelTransform);
        _backRightOffset = GetOffset(_backRightWheelCollider, _backRightWheelTransform);
        _backLeftOffset = GetOffset(_backLeftWheelCollider, _backLeftWheelTransform);

    }

    private Quaternion GetOffset(WheelCollider col, Transform mesh)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        return Quaternion.Inverse(rot) * mesh.rotation;
    }

    private void GetInput()
    {
        _horizontalInput = _controls.Car.Steer.ReadValue<float>();
        _verticalInput = _controls.Car.Throttle.ReadValue<float>();
        _isBraking = _controls.Car.Brake.IsPressed();

    }

    private void HandleMotor()
    {
        _frontLeftWheelCollider.motorTorque = _verticalInput * _motorForce;
        _frontRightWheelCollider.motorTorque = _verticalInput * _motorForce;
        _backLeftWheelCollider.motorTorque = _verticalInput * _motorForce;
        _backRightWheelCollider.motorTorque = _verticalInput * _motorForce;

        _currentBrakeForce = _isBraking ? _brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        _frontLeftWheelCollider.brakeTorque = _currentBrakeForce;
        _frontRightWheelCollider.brakeTorque = _currentBrakeForce;
        _backLeftWheelCollider.brakeTorque = _currentBrakeForce;
        _backRightWheelCollider.brakeTorque = _currentBrakeForce;

    }

    private void HandleSteering()
    {
        _currentSteerAngle = _maxSteerAngle * _horizontalInput;
        _frontRightWheelCollider.steerAngle = _currentSteerAngle;
        _frontLeftWheelCollider.steerAngle = _currentSteerAngle;
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform, Quaternion offset)
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot * offset;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(_frontRightWheelCollider, _frontRightWheelTransform, _frontRightOffset);
        UpdateSingleWheel(_frontLeftWheelCollider, _frontLeftWheelTransform, _frontLeftOffset);
        UpdateSingleWheel(_backLeftWheelCollider, _backLeftWheelTransform, _backLeftOffset);
        UpdateSingleWheel(_backRightWheelCollider,_backRightWheelTransform, _backRightOffset);
    }

    private void TailLight()
    {
        _leftPointLight.intensity = Mathf.Clamp(_leftPointLight.intensity, 1.20f, 2.80f);
        _rightPointLight.intensity = Mathf.Clamp(_rightPointLight.intensity, 1.20f, 2.80f);
        if (_isBraking)
        {
            
            _leftPointLight.intensity = _leftPointLight.intensity * 1.05f;
            _rightPointLight.intensity = _leftPointLight.intensity * 1.05f;
        }
        else
        {
            _leftPointLight.intensity = _leftPointLight.intensity * 0.95f;
            _rightPointLight.intensity = _leftPointLight.intensity * 0.95f;
        }

    }

    private void Update()
    {
        GetInput();
        UpdateWheels();
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        TailLight();
    }
}
