using UnityEngine;

public class HelicopterController : MonoBehaviour
{
    [Header("Controles")]
    [SerializeField] private float _responsiveness = 500f;
    [SerializeField] private float _throttleAmt = 25f;
    [SerializeField] private float _startingThrottle = 0f;
    [SerializeField] private float _inputReturnSpeed = 5f;
    [SerializeField] private float _rotationReturnSpeed = 3f;
    [SerializeField] private float _angularDamping = 5f;

    [Header("Clamp")]
    [SerializeField] private float _maxThrottle = 15f;
    [SerializeField] private float _maxYaw = 40f;
    [SerializeField] private float _maxPitch = 40f;
    [SerializeField] private float _maxRoll = 40f;

    [Header("Rotors")]
    [SerializeField] private float _rotorSpeedModifier = 10f;
    [SerializeField] private Transform _rotorsTransform;
    [SerializeField] private Transform _bladeTransform;

    private float _throttle;

    private Quaternion _startingRotation;

    private float _roll;
    private float _pitch;
    private float _yaw;
    private float _upDownInput;

    private Rigidbody _rigidbody;
    private Controls _controls;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _controls = new Controls();

        _throttle = _startingThrottle;
        _startingRotation = transform.rotation;
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    private void GetInput()
    {
        float targetRoll = _controls.Helicopter.Roll.ReadValue<float>();
        float targetPitch = _controls.Helicopter.Pitch.ReadValue<float>();
        float targetYaw = _controls.Helicopter.Yaw.ReadValue<float>();
        _upDownInput = _controls.Helicopter.UpDown.ReadValue<float>();

        _roll = Mathf.MoveTowards(_roll,targetRoll, _inputReturnSpeed * Time.deltaTime);
        _pitch = Mathf.MoveTowards(_pitch,targetPitch, _inputReturnSpeed * Time.deltaTime);
        _yaw = Mathf.MoveTowards(_yaw,targetYaw, _inputReturnSpeed * Time.deltaTime);

        _throttle += _upDownInput * _throttleAmt * Time.deltaTime;
        _throttle = Mathf.Clamp(_throttle, 0f, _maxThrottle);
        _roll = Mathf.Clamp(_roll, -1f, 1f);
        _yaw = Mathf.Clamp(_yaw, -1f, 1f);
        _pitch = Mathf.Clamp(_pitch, -1f, 1f);
    }

    private void HandleRotor()
    {
        _rigidbody.AddForce(transform.up * _throttle, ForceMode.Acceleration);
        _rigidbody.AddTorque(-transform.right * _pitch * _responsiveness, ForceMode.Force);
        _rigidbody.AddTorque(-transform.forward * _roll * _responsiveness, ForceMode.Force);
        _rigidbody.AddTorque(transform.up * _yaw * _responsiveness, ForceMode.Force);

        bool noRotationInput =Mathf.Abs(_pitch) < 0.01f && Mathf.Abs(_roll) < 0.01f && Mathf.Abs(_yaw) < 0.01f;

        if (noRotationInput)
        {
            Vector3 currentRotation = _rigidbody.rotation.eulerAngles;
            Vector3 startingRotation = _startingRotation.eulerAngles;
            Vector3 angularVelocity = _rigidbody.angularVelocity;

            Quaternion targetRotation = Quaternion.Euler(startingRotation.x,currentRotation.y,startingRotation.z);

            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation,targetRotation,_rotationReturnSpeed * Time.fixedDeltaTime));

            angularVelocity.x = Mathf.Lerp(angularVelocity.x,0f,_angularDamping * Time.fixedDeltaTime);
            angularVelocity.z = Mathf.Lerp(angularVelocity.z,0f,_angularDamping * Time.fixedDeltaTime);

            _rigidbody.angularVelocity = angularVelocity;
        }
    }

    private void RotorAnim()
    {
        _rotorsTransform.Rotate(Vector3.up * _throttle * _rotorSpeedModifier);
        _bladeTransform.Rotate(Vector3.right * _throttle * _rotorSpeedModifier);
    }

    void Update()
    {
        GetInput();
        RotorAnim();
    }

    private void FixedUpdate()
    {
        HandleRotor();
    }
}

