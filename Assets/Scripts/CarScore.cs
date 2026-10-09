using UnityEngine;
using TMPro;

public class CarScore : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _carRigidbody;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _multiplierText;

    [Header("Score settings")]
    [SerializeField] private float _minimumHighSpeed = 25f;
    [SerializeField] private float _pointsPerSecond = 100f;

    [Header("Multiplier settings")]
    [SerializeField] private float _multiplierIncreaseTime = 3f;
    [SerializeField] private float _maximumMultiplier = 10f;
    [SerializeField] private float _multiplierDecreaseSpeed = 2f;

    [Header("Collision")]
    [SerializeField] private string _wallTag = "Wall";

    private Controls controls;
    private float score;
    private float multiplier = 1f;

    public float Score => score;
    public float Multiplier => multiplier;

    private void Awake()
    {
        controls = new Controls();

        if (_carRigidbody == null)
        {
            _carRigidbody = GetComponent<Rigidbody>();
        }
    }

    private void OnEnable()
    {
        controls.Car.Enable();
    }

    private void OnDisable()
    {
        controls.Car.Disable();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_wallTag))
        {
            ResetMultiplier();
        }
    }

    private void UpdateMultiplier()
    {
        bool isThrottlePressed = controls.Car.Throttle.ReadValue<float>() > 0f;

        if (isThrottlePressed)
        {
            multiplier = Mathf.Min(multiplier + Time.fixedDeltaTime / _multiplierIncreaseTime, _maximumMultiplier);
        }
        else
        {
            multiplier = Mathf.MoveTowards(multiplier, 1f, _multiplierDecreaseSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateScore()
    {
        float speedInKmh = _carRigidbody.linearVelocity.magnitude * 3.6f;

        if (speedInKmh < _minimumHighSpeed) return;

        float speedBonus = speedInKmh / _minimumHighSpeed;
        score += _pointsPerSecond * speedBonus * multiplier * Time.fixedDeltaTime;
    }

    private void ResetMultiplier()
    {
        multiplier = 1f;
    }

    private void UpdateUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"Score : {Mathf.FloorToInt(score)}";
        }

        if (_multiplierText != null)
        {
            _multiplierText.text = $"x{multiplier:0.0}";
        }
    }

    public void ResetScore()
    {
        score = 0f;
        ResetMultiplier();
    }

    private void FixedUpdate()
    {
        UpdateMultiplier();
        UpdateScore();
        UpdateUI();
    }

}