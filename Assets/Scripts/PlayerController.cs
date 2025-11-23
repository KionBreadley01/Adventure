using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class PlayerController : MonoBehaviour
{
 // Rigidbody of the player.
 private Rigidbody rb; 
 private int count;
 public TextMeshProUGUI countText;


 // Movement along X and Y axes.
 private float movementX;
 private float movementY;

    // Speed properties
    public float slowSpeed = 5f; // Velocidad inicial lenta
    public float maxSpeed = 20f; // Velocidad máxima
    public float jumpForce = 8f; // Fuerza del salto
    private float speed;

    // Time tracking for speed progression
    private float timeElapsed = 0f;
    private float initialPhaseDuration = 180f; // 1 minuto en segundos
    private float rampUpDuration = 300f; // 5 minutos en segundos

 // Start is called before the first frame update.
 void Start()
    {
 // Get and store the Rigidbody component attached to the player.
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();

        // Initialize speed to slow speed
        speed = slowSpeed;
    }
 
 // This function is called when a move input is detected.
 void OnMove(InputValue movementValue)
    {
 // Convert the input value into a Vector2 for movement.
        Vector2 movementVector = movementValue.Get<Vector2>();

 // Store the X and Y components of the movement.
        movementX = movementVector.x; 
        movementY = movementVector.y; 
    }
    void SetCountText()
    {
        countText.text = "Puntos: " + count.ToString();
    }

    void Update()
    {
        // Increment the elapsed time
        timeElapsed += Time.deltaTime;

        // Check the time to adjust the speed
        if (timeElapsed <= initialPhaseDuration)
        {
            // During the first minute, maintain slow speed.
            speed = slowSpeed;
        }
        else if (timeElapsed <= initialPhaseDuration + rampUpDuration)
        {
            // During the next 5 minutes, gradually increase speed from slow to max.
            float rampUpProgress = (timeElapsed - initialPhaseDuration) / rampUpDuration;
            speed = Mathf.Lerp(slowSpeed, maxSpeed, rampUpProgress);
        }

        // Comprobar si se pulsa la tecla de salto
        if (IsGrounded() && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
 // FixedUpdate is called once per fixed frame-rate frame.
 private void FixedUpdate() 
    {
        // Aplicamos una fuerza constante hacia adelante y una fuerza lateral basada en el input.
        // Esto se hace tanto en el suelo como en el aire para mantener la velocidad.
        Vector3 movement = new Vector3(movementX, 0.0f, 1.0f);
        
        rb.AddForce(movement * speed);
    }
    private void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);   
            count = count + 1;

            SetCountText();
        }
      
    }

    private bool IsGrounded()
    {
        // Lanza un rayo hacia abajo desde el centro del jugador.
        // La longitud del rayo es un poco más que la mitad de la altura del jugador.
        // El 0.1f es un pequeño margen para asegurar que detecte el suelo.
        return Physics.Raycast(transform.position, Vector3.down, GetComponent<Collider>().bounds.extents.y + 0.1f);
    }
}