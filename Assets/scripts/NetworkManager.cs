using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
	
	[Header("Player Controller Values")]
	public float jumpStrength = 1;
	public float speed = 1;
	public Animator animator;
	public GameObject modelPrefab;
	public Transform spawnpoint;
	public Transform cameraOffset;
	[SerializeField] float sensitivity = 100;
	
	
	
	
	[Header("Network Values")]
	public UnityClient client;
	public string playerId = Guid.NewGuid().ToString();

	public int networkTickrate = 10;
	public int inputTickrate;
    
	public List<PlayerEntity> Players = new List<PlayerEntity>();

	private string ServerIp = "127.0.0.1";

	private void Start() {
		DontDestroyOnLoad(gameObject);
		client = gameObject.AddComponent<UnityClient>();
		client.ConnectInBackground(IPAddress.Parse(ServerIp), 4296, 4296, true, ClientConnected);
		client.MessageReceived += ReceiveMessage;
		
		LocalPlayer localPlayer = new LocalPlayer(inputTickrate, jumpStrength, speed, animator, modelPrefab, spawnpoint, cameraOffset, sensitivity);
		
		SchedulerSystem.AddJob(delegate { SendMovement(localPlayer.entity); }, 0, networkTickrate, -1);
	}

	private void ClientConnected(Exception e) {
		using (DarkRiftWriter w = DarkRiftWriter.Create()) {
			w.Write(playerId);
			using (Message m = Message.Create((ushort)Tag.OnPlayerJoined, w)) {
				client.SendMessage(m, SendMode.Reliable);
			}
		}
	}


	private void ReceiveMessage(object s, MessageReceivedEventArgs e) {
		switch ((Tag)e.Tag) {
			case Tag.OnPlayerJoined:
				CreatePlayer(e.GetMessage());
				break;
			case Tag.OnPlayerLeft:
				break;
			case Tag.OnPlayerMove:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void SendMovement(PlayerEntity player) {
		if (client.ConnectionState != ConnectionState.Connected) return;
		using (DarkRiftWriter w = DarkRiftWriter.Create())
		{
			w.Write(player.playerId); //Player ID 
			w.Write(player.rootPositionX); //Root Pos X
			w.Write(player.rootPositionY); //Root Pos Y
			w.Write(player.rootPositionZ); //Root Pos Z
			w.Write(player.rootRotationX); //Root Rot X
			w.Write(player.rootRotationY); //Root Rot Y
			w.Write(player.rootRotationZ); //Root Rot Z
			w.Write(player.rightSpeed); //Right Speed
			w.Write(player.frontSpeed); //Front Speed
			w.Write(player.jump); //Jump
			w.Write(player.isGrounded); //Grounded
			w.Write(player.isCrouching); //Crouching
		}
	}
	
	private void CreatePlayer(Message message) {
		using (DarkRiftReader r = message.GetReader())
		{
			string Player = r.ReadString();
			//Instantiate Player

			GameObject go = gameObject;
            
			Players.Add(new PlayerEntity()
			{
				playerId = Player,
				playerRoot = go
			});
		}
	}

	private void MovePlayer(Message message) {
		string player;
			using (DarkRiftReader r = message.GetReader())
		{
			player = r.ReadString();
			PlayerEntity e = Players.Find(match => match.playerId == player);
			if (e.playerRoot == null) return;
			e.SetNetworkValues(r);
		}
	}
    
}

public class PlayerEntity
{
	public string playerId;

	public GameObject playerRoot;
    
	public float rootPositionX = 0f;
	public float rootPositionY = 0f;
	public float rootPositionZ = 0f;
	public float rootRotationX = 0f;
	public float rootRotationY = 0f;
	public float rootRotationZ = 0f;

	public float rightSpeed = 0f;
	public float frontSpeed = 0f;
	public float jump = 0f;

	public bool isGrounded;
	public bool isCrouching;

	public void SetNetworkValues(DarkRiftReader r) {
		rootPositionX = r.ReadSingle();
		rootPositionY = r.ReadSingle();
		rootPositionZ = r.ReadSingle();
	            
		rootRotationX = r.ReadSingle();
		rootRotationY = r.ReadSingle();
		rootRotationZ = r.ReadSingle();
	            
		rightSpeed = r.ReadSingle();
		frontSpeed = r.ReadSingle();
		jump = r.ReadSingle();

		isGrounded = r.ReadBoolean();
		isCrouching = r.ReadBoolean();
	}
}

public class LocalPlayer : UnityEngine.Object {
	
	public PlayerEntity entity;

	public float jumpStrength = 1;
	public float speed = 1;
	public Animator animator;
	public GameObject modelPrefab;
	public Transform spawnpoint;
	public Transform cameraOffset;
	public float sensitivity = 100;

	//private CharacterController _characterController;
	//private Rigidbody _rigidbody;
	private Camera cam;
	private Transform _camTransform;
	private float headRotation = 0f;
	private bool didntMove;
	private bool isGrounded;
	private GameObject model;
	private List<Transform> spawnpoints = new List<Transform>();
    
	private Dictionary<AmmoType, int> ammo = new Dictionary<AmmoType, int>();
    
	private GameObject lastFocussedInteractable;
    
	private static readonly int OnGround = Animator.StringToHash("OnGround");
	private static readonly int Right = Animator.StringToHash("Right");
	private static readonly int Forward = Animator.StringToHash("Forward");
	private static readonly int Crouch = Animator.StringToHash("Crouch");
	private static readonly int Jump = Animator.StringToHash("Jump");

	public LocalPlayer(int inputTickrate, float jumpStrength, float speed, Animator animator, GameObject modelPrefab, Transform spawnpoint, Transform cameraOffset, float sensitivity) {
		this.entity = new PlayerEntity();
		this.jumpStrength = jumpStrength;
		this.speed = speed;
		this.animator = animator;
		this.modelPrefab = modelPrefab;
		this.spawnpoint = spawnpoint;
		this.cameraOffset = cameraOffset;
		this.sensitivity = sensitivity;
		
		SchedulerSystem.AddJob(Update, 0, inputTickrate, -1);
		
		
        
		// lock cursor
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		foreach (Spawnpoint spawnpoint2 in FindObjectsOfType<Spawnpoint>()) {
			spawnpoints.Add(spawnpoint2.transform);
		}

		spawnpoint = spawnpoints[UnityEngine.Random.Range(0, spawnpoints.Count-1)];

		model = Instantiate(modelPrefab, spawnpoint.position, spawnpoint.rotation);
		animator = model.GetComponent<Animator>();
        
		model.SetActive(false);
        
		model.transform.position = spawnpoint.position;
		model.transform.rotation = spawnpoint.rotation;
        
		model.SetActive(true);

		//_characterController = GetComponent<CharacterController>();
		//_rigidbody = GetComponent<Rigidbody>();

		// locally attach camera to player object
	    GameObject camGO = new GameObject();
	    camGO.tag = "MainCamera";
	    cam = camGO.AddComponent<Camera>();
	    cam.transform.SetParent(cameraOffset);
	    cam.transform.position = cameraOffset.position;
	    cam.transform.rotation = cameraOffset.rotation;

	    
		// set model transform (pos, rot, parent)
		Rigidbody rb = model.gameObject.AddComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		CapsuleCollider col = model.gameObject.AddComponent<CapsuleCollider>();
		col.center = new Vector3(0, 0.5f, 0);
	}

	public void Update() {
		float rightSpeed = 0;
        float frontSpeed = 0;
        float mouseX = 0;
        float mouseY = 0;
        /*if (isLocalPlayer) {*/
            
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
        /*}*/

        //check if character is grounded
        //isGrounded = _characterController.isGrounded;
        isGrounded = Physics.Raycast(model.transform.position + new Vector3(0, 0.5f, 0), Vector3.down, 2f, 1);
        
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
            model.transform.RotateAround(cameraOffset.parent.transform.position, Vector3.up, mouseX);
            didntMove = true;
        } else {
            if (didntMove) {
                animator.transform.rotation = model.transform.rotation;
                didntMove = false;
            }
            //targetModel.SetFloat("Turn", x);
            animator.transform.Rotate(0f, mouseX, 0f);
        
            //targetModel.transform.position = transform.position;
            //transform.rotation = animator.transform.rotation;
        }
        //transform.position = animator.transform.position;
        cameraOffset.parent.localEulerAngles = new Vector3(headRotation, mouseX, 0f);
        
    }

    private void AddAmmo(AmmoType ammoPrefab, int amnt) {
        ammo[ammoPrefab] += amnt;
    }
}