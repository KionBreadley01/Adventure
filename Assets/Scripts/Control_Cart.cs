using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Control_Cart : MonoBehaviour
{
    [SerializeField]
    private WheelCollider llantaDD;
    [SerializeField]
    private WheelCollider llantaDI;
    [SerializeField]
    private WheelCollider llantaTD;
    [SerializeField]
    private WheelCollider llantaTI;

    private float axisY;
    private float axisX;

    private float velocidad = 18.0f;
    private float anguloGiro = 30.0f; // Ángulo máximo de giro

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("¡Falta el componente Rigidbody en el objeto del coche!");
        }
    }

    void Update()
    {
        // Suavizado de entrada
        float targetY = Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0;
        float targetX = Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0;

        axisY = Mathf.Lerp(axisY, targetY, Time.deltaTime * 5f);
        axisX = Mathf.Lerp(axisX, targetX, Time.deltaTime * 5f);

        // Solo las ruedas traseras reciben torque
        llantaTD.motorTorque = velocidad * axisY;
        llantaTI.motorTorque = velocidad * axisY;

        // Solo las ruedas delanteras giran
        llantaDD.steerAngle = anguloGiro * axisX;
        llantaDI.steerAngle = anguloGiro * axisX;
    }
}