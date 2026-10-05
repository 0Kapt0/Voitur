using UnityEngine;
using TMPro;

public class CarScore : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text multiplierText;

    [Header("Réglages du score")]
    [SerializeField] private float minimumHighSpeed = 50f;
    [SerializeField] private float pointsPerSecond = 1f;

    [Header("Réglages du multiplicateur")]
    [SerializeField] private float multiplierIncreaseTime = 3f;
    [SerializeField] private float maximumMultiplier = 10f;
    [SerializeField] private float multiplierDecreaseSpeed = 2f;

    private float score;
    private float highSpeedTime;
    private float multiplier = 1f;

    public float Score => score;
    public float Multiplier => multiplier;

    private void Awake()
    {
        if (carRigidbody == null)
        {
            carRigidbody = GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        float speedInKmh = carRigidbody.linearVelocity.magnitude * 3.6f;

        if (speedInKmh >= minimumHighSpeed)
        {
            highSpeedTime += Time.fixedDeltaTime;

            // Le multiplicateur augmente progressivement
            multiplier = Mathf.Clamp(
                1f + highSpeedTime / multiplierIncreaseTime,
                1f,
                maximumMultiplier
            );

            // Plus la voiture va vite, plus elle gagne de points
            float speedBonus = speedInKmh / minimumHighSpeed;

            score += pointsPerSecond
                     * speedBonus
                     * multiplier
                     * Time.fixedDeltaTime;
        }
        else
        {
            highSpeedTime = 0f;

            // Le multiplicateur redescend progressivement
            multiplier = Mathf.MoveTowards(
                multiplier,
                1f,
                multiplierDecreaseSpeed * Time.fixedDeltaTime
            );
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score : {Mathf.FloorToInt(score)}";
        }

        if (multiplierText != null)
        {
            multiplierText.text = $"x{multiplier:0.0}";
        }
    }

    public void ResetScore()
    {
        score = 0f;
        highSpeedTime = 0f;
        multiplier = 1f;
    }
}