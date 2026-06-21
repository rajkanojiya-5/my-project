using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{

    [Header("Combo")]
    public TextMeshProUGUI comboText;

    [Header("Hit Result")]
    public TextMeshProUGUI hitResultText;
    public float hitResultFadeDuration = 0.6f;

    [Header("Driving")]
    public CarController carController;
    public TextMeshProUGUI speedText;

    [Header("Beat Ring")]
    public Image beatRing;
    public float ringPulseDuration = 0.2f;

    private float ringTimer;
    private float _hitResultTimer;

    void OnEnable()
    {
        GameEvents.OnHit += HandleHit;
        GameEvents.OnMiss += HandleMiss;
        GameEvents.OnComboChanged += HandleComboChanged;
        GameEvents.OnBeat += HandleBeat;
    }

    void OnDisable()
    {
        GameEvents.OnHit -= HandleHit;
        GameEvents.OnMiss -= HandleMiss;
        GameEvents.OnComboChanged -= HandleComboChanged;
        GameEvents.OnBeat += HandleBeat;
    }

    void Update()
    {
        if (carController == null)
            carController = FindObjectOfType<CarController>();

        if (speedText != null && carController != null)
        {
            speedText.text = $"{carController.CurrentSpeedKph:000} km/h";
        }

        if (ringTimer > 0f && beatRing != null)
        {
            ringTimer -= Time.deltaTime;

            float t = ringTimer / ringPulseDuration;

            beatRing.transform.localScale =
                Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, t);
        }



        if (_hitResultTimer > 0f)
        {
            _hitResultTimer -= Time.deltaTime;
            float alpha = _hitResultTimer / hitResultFadeDuration;

            if (hitResultText != null)
            {
                Color c = hitResultText.color;
                hitResultText.color = new Color(c.r, c.g, c.b, alpha);
            }
        }
    }

    void HandleHit(HitResult result, float errorMs)
    {
        (string text, Color color) = result switch
        {
            HitResult.Perfect => ("PERFECT!", Color.yellow),
            HitResult.Good => ("GOOD", Color.green),
            HitResult.Ok => ("OK", Color.white),
            _ => ("", Color.clear)
        };

        if (hitResultText != null)
        {
            hitResultText.text = text;
            hitResultText.color = color;
        }

        _hitResultTimer = hitResultFadeDuration;
    }

    void HandleMiss()
    {
        if (hitResultText != null)
        {
            hitResultText.text = "MISS";
            hitResultText.color = Color.red;
        }

        _hitResultTimer = hitResultFadeDuration;
    }

    void HandleComboChanged(int combo)
    {
        if (comboText != null)
        {
            comboText.text = combo > 1 ? $"x{combo} COMBO" : "";
        }
    }
    void HandleBeat(float beatTime)
    {
        ringTimer = ringPulseDuration;
        if(beatRing != null)
        {
            beatRing.transform.localScale = Vector3.one * 1.5f;
        }
    }
}
