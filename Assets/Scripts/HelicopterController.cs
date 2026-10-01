using UnityEngine;

public class HelicopterController : MonoBehaviour
{
    [SerializeField] private float _responsiveness = 500f;
    [SerializeField] private float _throttleAmt = 25f;
    [SerializeField] private float _rotorSpeedModifier = 10f;
    [SerializeField] private Transform _rotorsTransform;
    [SerializeField] private Transform _bladeTransform;

    private float _throttle;

    private float _roll;
    private float _pitch;
    private float _yaw;



    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    private void HandleInput()
    {
        _roll = Input.GetAxis("Roll");
        _pitch = Input.GetAxis("Pitch");
        _yaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.Space))
        {
            _throttle += Time.deltaTime * _throttleAmt;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            _throttle -= Time.deltaTime * _throttleAmt;
        }

        _throttle = Mathf.Clamp(_throttle, 0f, 80f);
    }

    void Update()
    {
        HandleInput();
        _rotorsTransform.Rotate(Vector3.up *  _throttle * _rotorSpeedModifier);
        _bladeTransform.Rotate(Vector3.right *  _throttle * _rotorSpeedModifier);
    }

    private void FixedUpdate()
    {
        _rigidbody.AddForce(transform.up * _throttle, ForceMode.Impulse);

        _rigidbody.AddTorque(transform.right * _pitch * _responsiveness);
        _rigidbody.AddTorque(-transform.forward * _roll * _responsiveness);
        _rigidbody.AddTorque(transform.up * _yaw * _responsiveness);
    }
}

