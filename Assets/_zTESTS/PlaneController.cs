using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : GameVehicle {

    public override string VehicleName { get { if (type == PlaneType.Propeller) return "Propeller Plane"; else return "Jet Plane"; } }

    public enum PlaneType { Propeller, Jet}

    public PlaneType type;

    public StupidRotor propellerObject;
    public float propellerSpeed = 0.01f;
    public ParticleSystem[] smokeParticles;
    public ParticleSystem[] jetParticles;
    public float smokeParticlesMaxSize = 10, jetParticlesMaxSpeed = 50;

    public AudioSource soundLoop;
    public AudioSource soundStart;
    public float soundLoopStartDelay; //also used as fade duration in Jet type
    public float soundLoopStopDuration = 1;
    public float soundLoopMinPitch;


    public float maxForwardVelocity;
    [Tooltip("The velocity at which maneuverability will be possible.")]
    public float minManeuverVelocity;
    public float activeAcceleration, activeDecelleration;
    public float passiveDecelleration;

    public float gravity = 10f;

    [Range(0, 1)]
    public float turnSpeed = 0.2f;

    float desiredVelocity;
    float forwardVelocity;

    Rigidbody rb;
    Transform camTransform;

    //CharacterController cc;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //cc = GetComponent<CharacterController>();
        camTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        float speedFraction = forwardVelocity / maxForwardVelocity;
        float maneuverabilityFraction = Mathf.Clamp01((forwardVelocity-minManeuverVelocity) / (maxForwardVelocity - minManeuverVelocity));

        soundLoop.pitch = Mathf.Lerp(soundLoopMinPitch, 1, speedFraction);

        if (type == PlaneType.Propeller)
            propellerObject.AngularVelocityPerFrame.z = forwardVelocity * propellerSpeed;
        else
        {
            float smokeSize = Mathf.Lerp(0, smokeParticlesMaxSize, speedFraction);
            smokeParticles[0].startSize = smokeSize;
            smokeParticles[1].startSize = smokeSize;
            float jetSpeed = Mathf.Lerp(0, jetParticlesMaxSpeed, speedFraction);
            jetParticles[0].startSpeed = jetSpeed;
            jetParticles[1].startSpeed = jetSpeed;
        }

        if (vehicleActive)
        {
            if (Input.GetKey(KeyCode.W))
            {
                forwardVelocity = Mathf.Min(forwardVelocity + activeAcceleration * Time.fixedDeltaTime, maxForwardVelocity);

                transform.forward = Vector3.Lerp(transform.forward, camTransform.forward, Mathf.Lerp(0, turnSpeed, maneuverabilityFraction));
            }
            else if (Input.GetKey(KeyCode.S))
            {
                forwardVelocity = Mathf.Max(forwardVelocity - activeDecelleration * Time.fixedDeltaTime, 0);
            }
            else
            {
                forwardVelocity = Mathf.Max(forwardVelocity - passiveDecelleration * Time.fixedDeltaTime, 0);
            }
            //cc.Move((transform.forward * forwardVelocity + Vector3.down * gravity * (1-maneuverabilityFraction)) * Time.fixedDeltaTime);
        }
        else
        {
            forwardVelocity = Mathf.Max(forwardVelocity - activeDecelleration * Time.fixedDeltaTime, 0);
        }
        rb.velocity = (transform.forward * forwardVelocity + Vector3.down * gravity * (1 - speedFraction));
        //rb.AddForce(transform.forward * forwardVelocity + Vector3.down * gravity * (1 - speedFraction), ForceMode.Acceleration);
        //rb.angularVelocity = Vector3.zero;
    }

    public override void StartVehicle(VoxelCharacterController player)
    {
        vehicleActive = true;
        //Set different physics materials?
        if(soundStart!=null) soundStart.Play();
        if (StartStopCo != null)
            StopCoroutine(StartStopCo);
        if (type == PlaneType.Jet)
            StartCoroutine(JetStartCo());
        else
            soundLoop.PlayDelayed(soundLoopStartDelay);
    }

    public override void StopVehicle()
    {
        if (StartStopCo != null)
            StopCoroutine(StartStopCo);
        StartCoroutine(PlaneStopCo());
        vehicleActive = false;
    }

    Coroutine StartStopCo;

    IEnumerator JetStartCo()
    {
        soundLoop.Play();
        float t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / soundLoopStartDelay;
            soundLoop.volume = t;
            yield return true;
        }
        soundLoop.volume = 1;
    }

    IEnumerator PlaneStopCo()
    {
        float start = 1, end = 0;
        float t = 0;

        while (t <= 1)
        {
            t += Time.deltaTime / soundLoopStopDuration;
            soundLoop.volume = 1-t;
            yield return true;
        }
        soundLoop.Stop();
        soundLoop.volume = 1;
    }
}
