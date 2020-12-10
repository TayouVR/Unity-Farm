using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Rigidbody))]
public class Character : NetworkBehaviour {

    public float jumpStrength = 1;
    public float speed = 1;
    public Animator animator;
    public GameObject modelPrefab;
    public Transform spawnpoint;
    public Transform cameraOffset;
    [SerializeField] float sensitivity = 100;

    //private CharacterController _characterController;
    //private Rigidbody _rigidbody;
    private Camera cam;
    private Transform _camTransform;
    float headRotation = 0f;
    private bool didntMove;
    private bool isGrounded;
    private GameObject model;
    private List<Transform> spawnpoints = new List<Transform>();
    private NetworkAnimator ntwkAnimator;
    
    private Dictionary<AmmoType, int> ammo = new Dictionary<AmmoType, int>();
    
    private GameObject lastFocussedInteractable;
    
    private static readonly int OnGround = Animator.StringToHash("OnGround");
    private static readonly int Right = Animator.StringToHash("Right");
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Crouch = Animator.StringToHash("Crouch");
    private static readonly int Jump = Animator.StringToHash("Jump");

    // Start is called before the first frame update
    void Start() {
        
        // lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        foreach (Spawnpoint spawnpoint in FindObjectsOfType<Spawnpoint>()) {
            spawnpoints.Add(spawnpoint.transform);
        }

        spawnpoint = spawnpoints[Random.Range(0, spawnpoints.Count-1)];
        
        transform.position = spawnpoint.position;
        transform.rotation = spawnpoint.rotation;

        model = Instantiate(modelPrefab, spawnpoint.position, spawnpoint.rotation, transform.parent);
        animator = model.GetComponent<Animator>();
        
        model.SetActive(false);
        
        ntwkAnimator = model.AddComponent<NetworkAnimator>();
        model.AddComponent<NetworkTransform>();
        ntwkAnimator.animator = animator;
        ntwkAnimator.clientAuthority = true;
        
        model.SetActive(true);

        //_characterController = GetComponent<CharacterController>();
        //_rigidbody = GetComponent<Rigidbody>();

        // locally attach camera to player object
        if (isLocalPlayer) {
            GameObject camGO = new GameObject();
            cam = camGO.AddComponent<Camera>();
            cam.transform.SetParent(cameraOffset);
            cam.transform.position = cameraOffset.position;
            cam.transform.rotation = cameraOffset.rotation;
        }

        // set model transform (pos, rot, parent)
        model.gameObject.AddComponent<Rigidbody>();
        Rigidbody rb = animator.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        CapsuleCollider col = animator.gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.5f, 0);
    }

    // Update is called once per frame
    void FixedUpdate() {
        float rightSpeed = 0;
        float frontSpeed = 0;
        float mouseX = 0;
        float mouseY = 0;
        if (isLocalPlayer) {
            
            // mouse, camera
            mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime * -1f;
            headRotation += mouseY;
            if (headRotation <= -90) {
                headRotation = -90;
            }
            if (headRotation >= 90) {
                headRotation = 90;
            }
            
            // sprinting
            int speedModifier = 1;
            if (Input.GetAxis("Sprint") > 0) {
                speedModifier = 2;
            }

            // set speed to var
            rightSpeed = Input.GetAxis("Horizontal") / 2 * speedModifier;
            frontSpeed = Input.GetAxis("Vertical") / 2 * speedModifier;

            // interaction
            if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f)),
                                out var hit, Mathf.Infinity, LayerMask.GetMask("Interactable"))) {
                GameObject hitGO = hit.collider.gameObject;
                if (lastFocussedInteractable != hitGO && lastFocussedInteractable != null) {
                    lastFocussedInteractable.GetComponent<Interactable>().Deselect();
                    hitGO.GetComponent<Interactable>().Select();
                    lastFocussedInteractable = hitGO;
                }
                if (Input.GetAxis("Interact") > 0) {
                    // harvest plant
                    if (hitGO.GetComponent<Plant>()) {
                        Plant plant = hitGO.GetComponent<Plant>();
                        if (plant.growthStage == 100) { 
                            AddAmmo(plant.ammoObject, plant.Harvest());
                        }
                    }
                }
            } else if (lastFocussedInteractable != null){
                lastFocussedInteractable.GetComponent<Interactable>().Deselect();
            }
            
            //TODO interact stuff
        }

        //check if character is grounded
        //isGrounded = _characterController.isGrounded;
        isGrounded = Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, 2f, 1);
        
        // set animator param for grounhded
        animator.SetBool(OnGround, isGrounded);
        if (isGrounded) {
            //Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            //_rigidbody.AddForce(movement * speed);
            // SetAnimator Info
            animator.SetFloat(Right, rightSpeed); //movement.x);
            animator.SetFloat(Forward, frontSpeed); //movement.z);
            //_rigidbody.AddForce(new Vector3(0.0f, Input.GetAxis("Jump") * jumpStrength, 0.0f));
            animator.SetBool(Crouch, Input.GetAxis("Crouch") > 0);
            animator.SetFloat(Jump, Input.GetAxis("Jump"));  //_rigidbody.velocity.y);
        }

        // if character doesn't move free camera rotation, otherwise rotate character
        if (frontSpeed == 0 && rightSpeed == 0) {
            transform.RotateAround(cameraOffset.parent.transform.position, Vector3.up, mouseX);
            didntMove = true;
        } else {
            if (didntMove) {
                animator.transform.rotation = transform.rotation;
                didntMove = false;
            }
            //targetModel.SetFloat("Turn", x);
            animator.transform.Rotate(0f, mouseX, 0f);
        
            //targetModel.transform.position = transform.position;
            transform.rotation = animator.transform.rotation;
        }
        transform.position = animator.transform.position;
        cameraOffset.parent.localEulerAngles = new Vector3(headRotation, mouseX, 0f);
    }

    private void AddAmmo(AmmoType ammoPrefab, int amnt) {
        ammo[ammoPrefab] += amnt;
    }
}
