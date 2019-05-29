using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Zwykły silnik który może obiektem obracać, go przesuwać i wydawać dzwięk 
/// </summary>
public class Engine : Photon.MonoBehaviour, ICanMove, ICanTurn
{
#pragma warning disable CS0108 // Składowa ukrywa dziedziczoną składową; brak słowa kluczowego new
    private Rigidbody2D rigidbody2D;
#pragma warning restore CS0108 // Składowa ukrywa dziedziczoną składową; brak słowa kluczowego new
    public virtual float MoveSpeed { get; set; }
    public virtual float TurnSpeed { get; set; }

    [Header("Engine: Audio")]
    [Space()]
    [SerializeField]
    private AudioSource m_MovementAudio; // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
    [SerializeField]
    private AudioClip m_EngineIdling;    // Audio to play when the tank isn't moving.
    [SerializeField]
    private AudioClip m_EngineDriving;   // Audio to play when the tank is moving.
    private float m_PitchRange = 1f;    // The amount by which the pitch of the engine noises can vary.


    public virtual void Setup()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Prousza obiekt według podanej prędkości
    /// </summary>
    /// <param name="moveSpeed">Prędkość z jaką ma się poruszać obiekt</param>
    /// <param name="inputValue">(od 0 do 1, domyślnie 1) Jeśli chcemy żeby obiekt miał "rozpęd" (np. Input.GetAxis("Vertical1"))</param>
    public virtual void Move(float moveSpeed, float inputValue = 1)
    {
        // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
        Vector2 movement = -transform.right * inputValue * moveSpeed * Time.deltaTime;
        // Apply this movement to the rigidbody's position.
        rigidbody2D.MovePosition(rigidbody2D.position + movement);
    }

    /// <summary>
    /// Obraca obiekt według podanej prędkości (na '-' jeśli w lewo, na '+' jeśli w prawo)
    /// </summary>
    /// <param name="turnSpeed">Prędkość z jaką ma się obracać obiekt</param>
    /// <param name="inputValue">(od 0 do 1, domyślnie 1) Jeśli chcemy żeby obiekt miał "rozpęd" (np. Input.GetAxis("Horizontal1"))</param>
    public virtual void TurnForValue(float turnSpeed, float inputValue)
    {
        // Determine the number of degrees to be turned based on the input, speed and time between frames.
        float turnValue = -inputValue * turnSpeed;
        // Apply this rotation to the rigidbody's rotation.
        rigidbody2D.MoveRotation(rigidbody2D.rotation + turnValue * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Obraca obiekt w stronę podanego obiektu z podaną prędkością 
    /// </summary>
    /// <param name="targetPos">Pozycja obiektu w którego stronę ma się obrócić </param>
    /// <param name="TurnSpeed">Prędkość poruszania</param>
    public void TurnToTarget(Vector2 targetPos, float TurnSpeed)
    {
        Vector2 point2Target = (Vector2)transform.position - targetPos;
        point2Target.Normalize();
        float value = Vector3.Cross(point2Target, transform.right).z;
        //Debug.Log(value);
        if (value > 0.01f)
            rigidbody2D.angularVelocity = -TurnSpeed;
        else if (value < -0.01f)
            rigidbody2D.angularVelocity = TurnSpeed;
        else
            rigidbody2D.angularVelocity = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="speedValue"></param>
    /// <param name="turnValue"></param>
    protected void EngineAudio(float speedValue, float turnValue)
    {
        // If there is no input (the tank is stationary)...
        if (Mathf.Abs(speedValue) < 0.1f && Mathf.Abs(turnValue) < 0.1f)
        {
            // ... and if the audio source is currently playing the driving clip...
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                // ... change the clip to idling and play it.
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = m_PitchRange;
                m_MovementAudio.Play();
            }
        }
        else
        {
            // Otherwise if the tank is moving and if the idling clip is currently playing...
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                // ... change the clip to driving and play.
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = m_PitchRange;
                m_MovementAudio.Play();
            }
        }
    }
}
