using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [System.Serializable]
    public class RadioTrack
    {
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;
    }


    public AudioSource _radio;
    public RadioTrack[] _radioTracks;

    private int _currentTrackIndex;

    public Rigidbody _rigidBody;

    public Light _leftPointLight;
    public Light _rightPointLight;

    public Image _arrowSpeed;
    public RawImage _boy;

    public TMP_Text _musicNameText;
    public float _scrollSpeed = 80f;
    public RectTransform _musicNameZone;

    private float _scrollOffset;
    private float _musicNameWidth;

    private Quaternion _frontLeftOffset;
    private Quaternion _frontRightOffset;
    private Quaternion _backLeftOffset;
    private Quaternion _backRightOffset;

    private float _horizontalInput;
    private float _verticalInput;
    private float _currentSteerAngle;
    private float _currentBrakeForce;
    private bool _isBraking;

    private Controls _controls;

    private float _carSpeed;

    private void Awake()
    {
        _controls = new Controls();
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
        PlayCurrentTrack();

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

        if (_controls.Car.RadioChange.WasPressedThisFrame())
        {
            ChangeRadioTrack();
        }

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

    private void Speedometer()
    {
        _carSpeed = _rigidBody.linearVelocity.magnitude;

        float boyTrans = Mathf.Lerp(-847, -332, Mathf.Clamp01(_carSpeed / 50f));
        float angle = Mathf.Lerp(211.53f, -58.47f, Mathf.Clamp01(_carSpeed / 50f));

        _arrowSpeed.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        _boy.rectTransform.localPosition = new Vector3(683f, boyTrans, 0f);
    }

    private void RadioSpeed()
    {
        _carSpeed = _rigidBody.linearVelocity.magnitude;
        _radio.volume = Mathf.Lerp(Mathf.Clamp(_carSpeed * 0.01f, 0.05f, 0.4f), _radio.volume, Time.deltaTime);

    }

    private void ChangeRadioTrack()
    {
        if (_radioTracks.Length == 0) return;

        _currentTrackIndex = (_currentTrackIndex + 1) % _radioTracks.Length;

        PlayCurrentTrack();
    }


    private void PlayCurrentTrack()
    {
        if (_radioTracks.Length == 0) return;

        RadioTrack currentTrack = _radioTracks[_currentTrackIndex];

        _radio.clip = currentTrack.clip;
        _radio.volume = currentTrack.volume;

        _radio.Play();

        _musicNameText.text = _radio.clip.name;
        ResetMusicNamePosition();
    }

    private void ResetMusicNamePosition()
    {
        _musicNameWidth = _musicNameText.preferredWidth;
        _scrollOffset = _musicNameText.rectTransform.rect.width;
        ApplyScrollOffset();
    }

    private void ScrollMusicName()
    {
        _scrollOffset -= _scrollSpeed * Time.deltaTime;

        if (_scrollOffset < -_musicNameWidth)
        {
            ResetMusicNamePosition();
            return;
        }

        ApplyScrollOffset();
    }

    private void ApplyScrollOffset()
    {
        _musicNameText.margin = new Vector4(_scrollOffset, 0f, 0f, 0f);
    }

    private void PlayNextTrackWhenFinished()
    {
        if (!_radio.isPlaying)
        {
            ChangeRadioTrack();
        }
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
        RadioSpeed();
        Speedometer();
        PlayNextTrackWhenFinished();
        ScrollMusicName();
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        TailLight();
    }
}
