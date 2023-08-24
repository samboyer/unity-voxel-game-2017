using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelCharacterController : MonoBehaviour
{
    public float jumpVelocity = 40;
    public float jumpDuration = 1;
    public float gravity = 100;
    public float underwaterGravity = 50;

    public float movementMaxSpeed = 15;
    public float movementSprintSpeed = 30;
    public float movementSmoothing = 0.8f;
    public float maxPlayerRotationPerFrame = 10;
    public float groundClampDistance = 0.1f;

    public float capsuleRadius = 1; //for ground check sphere cast

    public bool isUnderwater;

    float currentMovementSpeed;
    float posY;
    float modelRotationDegrees;
    bool isGrounded = false;

    Vector3 desiredMovementDirection;

    Transform head;
    Transform cameraPivot;
    CharacterController cc;

    Animator anim;

    [Header("Particles")]
    public ParticleSystem particlesLand;
    public ParticleSystem particlesFootprint;

    public UnityEngine.UI.Text vehicleIndicatorText;

    public AudioClip sfxLandGrass;
    public AudioClip sfxLandWood;
    
    AudioSource spacialAudioSource;
        
    void Start()
    {
        anim = transform.Find("MODEL").GetComponent<Animator>();
        head = anim.transform.Find("Torso").Find("Head");
        cameraPivot = transform.Find("CAMERAPIVOT");
        //rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        spacialAudioSource = anim.GetComponent<AudioSource>();
    }

    float yVelo;
    bool wasGrounded;
    float raycastOffset = 0.05f;

    float timeOfJump;
    bool isJumping;

    Vector3 lastPos;

    [HideInInspector]
    public bool stationary;

    void Update()
    {
        if (!isInVehicle)
        {
            bool moving = Input.GetButton("Vertical") || Input.GetButton("Horizontal");
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);

            if (moving)
            {
                Vector3 inputVectors = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

                currentMovementSpeed = Mathf.Lerp((isSprinting ? movementSprintSpeed : movementMaxSpeed), currentMovementSpeed, movementSmoothing);

                Vector3 cameraForward = Vector3.ProjectOnPlane(cameraPivot.forward, Vector3.up).normalized;

                desiredMovementDirection = Quaternion.LookRotation(cameraForward) * inputVectors;

                //rotate the playermodel
                float angleFromModelToCamera = Quaternion.FromToRotation(anim.transform.forward, desiredMovementDirection).eulerAngles.y;
                angleFromModelToCamera = angleFromModelToCamera > 180 ? angleFromModelToCamera - 360 : angleFromModelToCamera;
                modelRotationDegrees += Mathf.Clamp(angleFromModelToCamera, -maxPlayerRotationPerFrame, maxPlayerRotationPerFrame);
                anim.transform.rotation = Quaternion.Euler(0, modelRotationDegrees, 0);
            }
            else
            {
                currentMovementSpeed *= movementSmoothing;
            }


            //apply gravity to yVelo
            //if(!isJumping)
            if(isUnderwater)
                yVelo -= underwaterGravity * Time.deltaTime;
            else
                yVelo -= gravity * Time.deltaTime;


            //clamp character to the ground & handle jumps
            float clampDistance = 0;

            if (wasGrounded) //if was grounded last frame,
            {
                Ray r = new Ray(transform.position + new Vector3(0, capsuleRadius + raycastOffset, 0), Vector3.down);
                RaycastHit hit;

                isGrounded = Physics.SphereCast(r, capsuleRadius, out hit, groundClampDistance); //use custom raycast system

                if (isGrounded) //if still grounded,
                {
                    clampDistance = hit.distance;
                    Debug.DrawRay(transform.position, Vector3.down * groundClampDistance, Color.green);

                    yVelo = 0;
                    if (Input.GetKeyDown(KeyCode.Space)) //Jump
                    {
                        anim.SetTrigger("Jump");
                        isGrounded = false; //is this needed?

                        timeOfJump = Time.time;
                        yVelo = jumpVelocity;
                        isJumping = true;
                    }
                }
            }
            else //player was airborne last frame,
            {
                isJumping = yVelo >= 0 && Input.GetKey(KeyCode.Space) && Time.time - timeOfJump < jumpDuration;
                if (isJumping)
                    yVelo = jumpVelocity;

                isGrounded = cc.isGrounded; //use the builtin ground check.

                Debug.DrawRay(transform.position, Vector3.down * groundClampDistance, Color.red);

                if (isGrounded)
                { //player has landed
                    DoLanding();
                    isJumping = false;
                }
            }
            Vector3 movement = new Vector3(desiredMovementDirection.x * currentMovementSpeed * Time.deltaTime, yVelo * Time.deltaTime - clampDistance, desiredMovementDirection.z * currentMovementSpeed * Time.deltaTime);
            cc.Move(movement);

            stationary = movement == Vector3.zero;

            anim.SetBool("Walking", moving);
            anim.SetBool("Airborne", !isGrounded);
            anim.SetBool("Sprinting", isSprinting);

            wasGrounded = isGrounded;

            if (lastPos.y == transform.position.y) //they've hit a ceiling
            {
                yVelo = 0;
                isJumping = false;
            }

            lastPos = transform.position;

            if (vehicleNearby)
            {
                //determine closest vehicle
                GameVehicle closestVehicle = null;
                float dist = float.MaxValue;
                foreach (GameVehicle veh in nearbyVehicles)
                {
                    float d = (veh.transform.position - lastPos).sqrMagnitude; //lastPos, cheeky speedup :P
                    if (d < dist)
                    {
                        closestVehicle = veh;
                        dist = d;
                    }
                }
                vehicleIndicatorText.text = "Press Tab to enter <i>" + closestVehicle.VehicleName + "</i>";

                if (Input.GetKeyDown(KeyCode.Tab)) //enter vehicle
                {
                    EnterVehicle(closestVehicle);

                }
            }
        }
        else //IN VEHICLE
        {
            if (Input.GetKeyDown(KeyCode.Tab)) //exit vehicle
            {
                ExitVehicle();
            }
        }

        //VEHICLE NEARBY
        vehicleIndicatorText.enabled = vehicleNearby && !isInVehicle;
    }


    #region HEADTRACKING
    private void LateUpdate()
    {
        UpdateHeadTracking();
    }

    [Header("POI Head Tracking / IK")]
    public float HeadTrackingSpeed = 0.5f;

    public float maxTrackAngleX = 45, maxTrackAngleYUp = 60, maxTrackAngleYDown = 40;

    public LayerMask POIOnlyLayerMask;
    Transform POI;
    bool POITargeted;
    Quaternion oldHeadRotation;

    void UpdateHeadTracking()
    {
        Quaternion desiredRotation;

        POITargeted &= POI != null; //WOOOAHHH

        if (POITargeted) //if already targeted (and not destroyed lol)
        {
            Debug.DrawLine(head.position, POI.position, Color.green);
            desiredRotation = Quaternion.LookRotation(POI.position - head.position, Vector3.up);

            //stop if distance is too high
            if((POI.position - head.position).magnitude > POI.GetComponent<SphereCollider>().radius)
            {
                POITargeted = false;
            }

            //stop if angle is too great
            float lookAngleX = desiredRotation.eulerAngles.x - head.parent.rotation.eulerAngles.x;
            float lookAngleY = desiredRotation.eulerAngles.y - head.parent.rotation.eulerAngles.y;

            lookAngleX = ((lookAngleX + 180) % 360) - 180;
            lookAngleY = ((lookAngleY + 180) % 360) - 180;

            if (Mathf.Abs(lookAngleY) > maxTrackAngleX || lookAngleX < -maxTrackAngleYUp || lookAngleX > maxTrackAngleYDown)
            {
                POITargeted = false;
            }
        }
        else 
        {
            desiredRotation = head.parent.rotation;

            Collider[] POIs = Physics.OverlapSphere(head.position, 0.01f, POIOnlyLayerMask);

            if (POIs.Length > 0)
            {
                foreach(Collider p in POIs)
                {
                    //just nab the first one
                    Quaternion testRot = Quaternion.LookRotation(p.transform.position - head.position, Vector3.up);
                    //check if angle is okay
                    float lookAngleX = testRot.eulerAngles.x - head.parent.rotation.eulerAngles.x;
                    float lookAngleY = testRot.eulerAngles.y - head.parent.rotation.eulerAngles.y;

                    lookAngleX = ((lookAngleX + 180) % 360) - 180;
                    lookAngleY = ((lookAngleY + 180) % 360) - 180;

                    if (Mathf.Abs(lookAngleY) < maxTrackAngleX && lookAngleX > -maxTrackAngleYUp && lookAngleX < maxTrackAngleYDown)
                    {
                        POI = p.transform;
                        POITargeted = true;
                        break;
                    }
                }

            }
        }

        oldHeadRotation = head.rotation = Quaternion.Lerp(oldHeadRotation, desiredRotation, HeadTrackingSpeed);
    }
#endregion

    void DoLanding()
    {
        ParticleSystem.EmitParams landParams = new ParticleSystem.EmitParams();

        Ray r = new Ray(transform.position + new Vector3(0, capsuleRadius + raycastOffset, 0), Vector3.down);
        RaycastHit hit;

        if (Physics.SphereCast(r, capsuleRadius, out hit, 100000))
        { //use custom raycast system

            SamBoyer.VoxelEngine.VoxelRenderer voxelRend = hit.transform.GetComponent<SamBoyer.VoxelEngine.VoxelRenderer>();
            if (voxelRend != null)
            {
                landParams.startColor = voxelRend.Model.palette.ToTexture().GetPixel(Mathf.FloorToInt(hit.textureCoord.x * 256), 0);
                WorldObject worldObj;
                if((worldObj = hit.transform.parent.GetComponent<WorldObject>()) != null)
                {
                    if (worldObj.isNaturalObject)
                        spacialAudioSource.PlayOneShot(sfxLandGrass);
                    else
                        spacialAudioSource.PlayOneShot(sfxLandWood);
                }
                else
                {
                    spacialAudioSource.PlayOneShot(sfxLandWood);
                }

            }
            else
            {
                spacialAudioSource.PlayOneShot(sfxLandGrass);
            }
            particlesLand.Emit(landParams, Random.Range(16, 40));
        }
    }

#region VEHICLES
    bool vehicleNearby;
    public bool isInVehicle;
    List<GameVehicle> nearbyVehicles = new List<GameVehicle>();
    GameVehicle currentVehicle; 

    public void EnterVehicleRadius(GameVehicle vehicle)
    {
        vehicleNearby = true;
        if(!nearbyVehicles.Contains(vehicle))
            nearbyVehicles.Add(vehicle);
    }
    public void ExitVehicleRadius(GameVehicle vehicle)
    {
        int i = nearbyVehicles.IndexOf(vehicle);
        if (i != -1) { //if not, welllllll.....
            nearbyVehicles.RemoveAt(i);
        }
        if (nearbyVehicles.Count == 0) vehicleNearby = false;
    }

    void EnterVehicle(GameVehicle veh)
    {
        currentVehicle = veh;
        veh.StartVehicle(this);
        isInVehicle = true;
        cc.enabled = false;
        transform.parent = veh.transform;
        transform.position = veh.transform.position + new Vector3(0,5,0);
        anim.gameObject.SetActive(false);

        print("entered");
    }
    void ExitVehicle()
    {
        currentVehicle.StopVehicle();
        isInVehicle = false;
        cc.enabled = true;
        anim.gameObject.SetActive(true);
        transform.parent = null;
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        print("exited");
    }

#endregion
}
