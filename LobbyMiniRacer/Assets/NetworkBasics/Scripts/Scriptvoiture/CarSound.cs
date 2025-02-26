using UnityEngine;

public class CarSound : MonoBehaviour
{
    public float minSpeed;
    public float maxSpeed;

    private float currentSpeed;

    private Rigidbody carRb;
    private AudioSource carAudio;

    public float minstep;
    public float maxstep;

    private float pitchFromCar;
    void Start()
    {
        carAudio = GetComponent<AudioSource>();
        carRb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        engineSound();
    }

    void engineSound()
    {
        currentSpeed = carRb.linearVelocity.magnitude;
        pitchFromCar = carRb.linearVelocity.magnitude / 50f;

        if(currentSpeed < minSpeed)
        {
            carAudio.pitch = minstep;
        }

        if(currentSpeed > minSpeed && currentSpeed < maxSpeed)
        {
            carAudio.pitch = minstep + pitchFromCar;
        }

        if(currentSpeed > maxSpeed)
        {
            carAudio.pitch = maxSpeed;
        }
    }
}
