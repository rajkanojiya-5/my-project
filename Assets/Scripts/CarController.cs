using System;
using System.Collections.Generic;
using UnityEngine;
using static CarController;

public class CarController : MonoBehaviour
{

    public enum Axel
    {
        Front,
        Back,
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelMesh;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    [Header("Base Stats")]
    [SerializeField] private float motorTorque = 300f;
    [SerializeField] private float brakeTorque = 1000f;
    [SerializeField] private float turnSensitivity = 0.6f;
    [SerializeField] private float maxSteerAngle = 45f;

    [SerializeField] private float fallbackMaxSpeedKph = 200f;

    [SerializeField] private Vector3 centerOfMass;

    [Header("Stability")]
    [SerializeField] private float downforce = 90f;
    [SerializeField] private float uprightStrength = 35f;
    [SerializeField] private float uprightDamping = 6f;
    [SerializeField] private float maxAngularVelocity = 4f;

    [Header("References")]
    [SerializeField] private List<Wheel> wheelsList;
    [Header("Trail Effects")]
    [SerializeField] private TrailRenderer eff_FL;
    [SerializeField] private TrailRenderer eff_FR;
    [SerializeField] private TrailRenderer eff_BL;
    [SerializeField] private TrailRenderer eff_BR;

    // Drag NitroSystem GameObject here in Inspector
    // If left empty, car runs without nitro (safe fallback)
    [SerializeField] private NitroSystem nitroSystem;

    // ── Private state ─────────────────────────────────────────
    private float moveInput;
    private float steerInput;
    private Rigidbody carRb;
    private float currentSpeed;
    public float CurrentSpeedKph => carRb != null ? carRb.velocity.magnitude * 3.6f : 0f;

    // ── Lifecycle ─────────────────────────────────────────────
    private void Awake()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = centerOfMass == Vector3.zero
            ? new Vector3(0f, -0.55f, 0f)
            : centerOfMass;
        carRb.maxAngularVelocity = maxAngularVelocity;
        carRb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        // carRb.centerOfMass = centerOfMass;
    }

    private void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        Move();
        Steer();
        StabilizeCar();
        UpdateTrailEffects();
    }

    private void LateUpdate()
    {
        AnimateWheels();
    }

    // ── Input ─────────────────────────────────────────────────
    private void GetInput()
    {
        moveInput = 0f;
       steerInput = GameInput.Instance != null
            ? GameInput.Instance.GetSteerInput()
            : Input.GetAxisRaw("Horizontal");

        if (GameInput.Instance != null)
        {
            moveInput += GameInput.Instance.GetAccelerateInput();
            moveInput -= GameInput.Instance.GetDecelerateInput();
        }
    }

    // ── Drive ─────────────────────────────────────────────────
    private void Move()
    {
        float brakeInput = GameInput.Instance.GetBrakeInput();
        // Speed is calculated from wheel RPM — same as your original
        currentSpeed = 2 * Mathf.PI
                         * wheelsList[0].wheelCollider.radius
                         * wheelsList[0].wheelCollider.rpm
                         * 60f;

        float activeMaxSpeedKph = (nitroSystem != null)
            ? nitroSystem.ActiveSpeedCapKph
            : fallbackMaxSpeedKph;

        // Nitro bonus torque: added on top of base motorTorque
        // NitroSystem.ActiveForce returns 0 when no boost is active
        float nitroTorque = (nitroSystem != null)
            ? nitroSystem.ActiveForce
            : 0f;

        float totalTorque = motorTorque + nitroTorque;

        bool underSpeedCap = CurrentSpeedKph < activeMaxSpeedKph;
        float forwardVelocityDir = Mathf.Sign(transform.InverseTransformDirection(carRb.velocity).z);

        foreach (Wheel wheel in wheelsList)
        {
            wheel.wheelCollider.brakeTorque = brakeInput * brakeTorque;
            if(brakeInput> 0)
            {
                wheel.wheelCollider.motorTorque = 0;
            }

            if (underSpeedCap || moveInput < 0f)
            {
                // Same logic as yours — brake if reversing direction
                if (forwardVelocityDir * moveInput > 0)
                {
                    wheel.wheelCollider.motorTorque = moveInput * totalTorque;
                }
                else
                {
                    wheel.wheelCollider.motorTorque = moveInput * brakeTorque;
                }
            }
            else
            {
                // At speed cap — cut motor torque
                wheel.wheelCollider.motorTorque = 0;
            }
        }

        ApplySpeedCap(activeMaxSpeedKph);
    }

    // ── Steer ─────────────────────────────────────────────────
    private void Steer()
    {
        float steerAngle = steerInput * maxSteerAngle;

        foreach (Wheel wheel in wheelsList)
        {
            if (wheel.axel == Axel.Front)
            {
                wheel.wheelCollider.steerAngle = Mathf.Lerp(
                    wheel.wheelCollider.steerAngle,
                    steerAngle,
                    turnSensitivity * Time.fixedDeltaTime
                );
            }
        }
    }
    

    private void StabilizeCar()
    {
        float speed = carRb.velocity.magnitude;
        carRb.AddForce(-transform.up * downforce * speed, ForceMode.Force);

        Vector3 correctionAxis = Vector3.Cross(transform.up, Vector3.up);
        Vector3 correctionTorque = correctionAxis * uprightStrength;
        Vector3 dampingTorque = -carRb.angularVelocity * uprightDamping;
        carRb.AddTorque(correctionTorque + dampingTorque, ForceMode.Acceleration);
    }

    private void ApplySpeedCap(float capKph)
    {
        float capMetersPerSecond = capKph / 3.6f;
        if (carRb.velocity.magnitude <= capMetersPerSecond) return;

        carRb.velocity = carRb.velocity.normalized * capMetersPerSecond;
    }

    // ── Wheel mesh sync ───────────────────────────────────────
    private void AnimateWheels()
    {
        foreach (Wheel wheel in wheelsList)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheel.wheelMesh.transform.position = pos;
            wheel.wheelMesh.transform.rotation = rot;
        }
    }
    private void UpdateTrailEffects()
    {
        float brake = GameInput.Instance != null ? GameInput.Instance.GetBrakeInput() : 0f;

        bool emit = brake > 0.1f;

        eff_FL.emitting = emit;
        eff_FR.emitting = emit;
        eff_BL.emitting = emit;
        eff_BR.emitting = emit;
    }
}
