using System;
using System.Collections;
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

    [Header("Stats")]
    [SerializeField] private float motorTorque = 300f;
    [SerializeField] private float brakeTorque = 1000f;

    [SerializeField] private float turnSensitivity = 0.6f;
    [SerializeField] private float maxSteerAngle = 45f;
    [SerializeField] private float maxSpeed = 50000;

    [SerializeField] private Vector3 centerOfMass;

    [Header("References")]
    [SerializeField] private List<Wheel> wheelsList;

    private float moveInput;
    private float steerInput;
    private Rigidbody carRb;
    private float currentSpeed;

    private void Awake()
    {
        carRb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        //carRb.centerOfMass = centerOfMass;
    }

    private void Update()
    {
        GetInput();

        Debug.Log(moveInput);

    }

    private void LateUpdate()
    {
        Move();
        Steer();
        AnimateWheels();
    }

    private void GetInput()
    {
        moveInput = 0;

        steerInput = Input.GetAxisRaw("Horizontal");

        moveInput += GameInput.Instance.GetAccelerateInput();
        moveInput -= GameInput.Instance.GetDecelerateInput();
    }

    private void Move()
    {
        currentSpeed = 2 * Mathf.PI * wheelsList[0].wheelCollider.radius * wheelsList[0].wheelCollider.rpm * 60;

        float forwardVelocityDir = Mathf.Sign(carRb.velocity.z);
        float breakInput =
               GameInput.Instance.GetBreakInput();
        foreach (Wheel wheel in wheelsList)
        {
            if (breakInput > 0)
            {
                wheel.wheelCollider.brakeTorque = 5000f;
            }
            else
            { wheel.wheelCollider.brakeTorque = 0f; }


            if (currentSpeed < maxSpeed)
            {
                if (forwardVelocityDir * moveInput > 0)
                {
                    wheel.wheelCollider.motorTorque = moveInput * motorTorque;
                }
                else
                {
                    wheel.wheelCollider.motorTorque = moveInput * brakeTorque;
                }
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0;
            }
          //  if(breakInput > 0)
          //  {
           //     wheel.wheelCollider.brakeTorque = 5000f;
           // }
           // else
           // { wheel.wheelCollider.brakeTorque = 0f; }

        }

    }

    private void Steer()
    {
        float steerAngle = steerInput * maxSteerAngle;

        foreach (Wheel wheel in wheelsList)
        {
            if (wheel.axel == Axel.Front)
            {
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, turnSensitivity * Time.deltaTime);
            }
        }
    }

    private void AnimateWheels()
    {
        foreach (Wheel wheel in wheelsList)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);

            wheel.wheelMesh.transform.position = pos;
            wheel.wheelMesh.transform.rotation = rot;
        }
    }



}
