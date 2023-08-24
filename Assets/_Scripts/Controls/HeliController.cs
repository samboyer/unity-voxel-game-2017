using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliController : GameVehicle {
    public override string VehicleName { get { return "Helicopter"; } }

    public AudioSource heliSoundSource;
    public StupidRotor rotor, rotor2;
    public float movementSpeed, verticalMovementSpeed;

    public float maxPlayerRotationPerFrame = 10;
    public float accelerationRate = 1;
    public float decellerationRate;

    public float inactiveFallingAcceleration = 0.5f;
    public float inactiveFallingSpeed = 25;

    public float maxModelTilt = 20; 

    public float startDuration;
    public float rotorSpeed = 50;

    Transform cameraPivot;

    Vector3 desiredMovement;
    float modelRotationDegrees;

    Transform model;

    CharacterController cc;

    VoxelCharacterController player;

    void Start () {
        cameraPivot = Camera.main.transform.parent;
        model = transform.Find("MODEL");
        cc = GetComponent<CharacterController>();
    }

    float velocity, verticalVelocity;

	void Update () {        

        if (vehicleActive)
        {

            bool moving = Input.GetButton("Vertical") || Input.GetButton("Horizontal");
            bool movingVertical = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E);

            if (moving)
            {
                velocity = Mathf.Min(velocity + accelerationRate, movementSpeed);

                Vector3 inputVectors = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
                Debug.DrawRay(transform.position, inputVectors * 10, Color.red);

                Vector3 cameraForward = Vector3.ProjectOnPlane(cameraPivot.forward, Vector3.up).normalized;

                desiredMovement = Quaternion.LookRotation(cameraForward) * inputVectors;

                //rotate the playermodel
                float angleFromModelToCamera = Quaternion.FromToRotation(model.transform.forward, desiredMovement).eulerAngles.y;
                angleFromModelToCamera = angleFromModelToCamera > 180 ? angleFromModelToCamera - 360 : angleFromModelToCamera;

                modelRotationDegrees += Mathf.Clamp(angleFromModelToCamera, -maxPlayerRotationPerFrame, maxPlayerRotationPerFrame);
            }
            else
                velocity *= decellerationRate;

            if (movingVertical)
            {

                if (Input.GetKey(KeyCode.Q)) verticalVelocity = Mathf.Min(verticalVelocity + accelerationRate, verticalMovementSpeed);
                else verticalVelocity = Mathf.Max(verticalVelocity - accelerationRate, -verticalMovementSpeed);
            }
            else
                verticalVelocity *= decellerationRate;
        }
        else
        {
            velocity *= decellerationRate;
            //verticalVelocity *= decellerationRate;
            verticalVelocity = Mathf.Max(verticalVelocity - inactiveFallingAcceleration, -inactiveFallingSpeed);
        }

        model.transform.rotation = Quaternion.Euler(Mathf.Lerp(0, maxModelTilt, velocity / movementSpeed), modelRotationDegrees + 180, 0);
        cc.Move(new Vector3(desiredMovement.x * velocity, verticalVelocity, desiredMovement.z * velocity) * Time.deltaTime);
    }

    public override void StartVehicle(VoxelCharacterController player)
    {
        heliSoundSource.Play();
        if(StartStopCo!=null)
            StopCoroutine(StartStopCo);
        StartStopCo = StartCoroutine(HeliStartCo());
    }

    public override void StopVehicle()
    {
        vehicleActive = false;
        if (StartStopCo != null)
            StopCoroutine(StartStopCo);
        StartStopCo = StartCoroutine(HeliStopCo());
    }

    Coroutine StartStopCo;

    IEnumerator HeliStartCo()
    {
        float t = 0;

        while (t <= 1)
        {
            t += Time.deltaTime / startDuration;
            rotor2.AngularVelocityPerFrame.x = rotor.AngularVelocityPerFrame.y = t * rotorSpeed;
            
            heliSoundSource.pitch = t;

            yield return true;
        }
        vehicleActive = true;
        rotor2.AngularVelocityPerFrame.x = rotor.AngularVelocityPerFrame.y = rotorSpeed;
        heliSoundSource.pitch = 1;
    }

    IEnumerator HeliStopCo()
    {
        float t = 1;

        while (t >= 0)
        {
            t -= Time.deltaTime / startDuration;
            rotor2.AngularVelocityPerFrame.x = rotor.AngularVelocityPerFrame.y = t * rotorSpeed;
            heliSoundSource.pitch = t;

            yield return true;
        }
        rotor2.AngularVelocityPerFrame.x = rotor.AngularVelocityPerFrame.y = 0;
        heliSoundSource.Stop();
    }
}
