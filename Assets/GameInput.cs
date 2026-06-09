using System;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.CarController.Enable();
    }

    private void OnDestroy()
    {
        playerInputActions.Dispose();
    }

    public float GetAccelerateInput()
    {
        return playerInputActions.CarController.Accelerate.ReadValue<float>();
    }

    public float GetDecelerateInput()
    {
        return playerInputActions.CarController.Decelerate.ReadValue<float>();
    }
    public float GetBreakInput()
    {
        return
            playerInputActions.CarController.Break.ReadValue<float>();
    }

}
