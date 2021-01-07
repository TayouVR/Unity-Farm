using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[System.Serializable]
public class Character {
	
	public PlayerEntity entity;

	public float jumpStrength = 1;
	public float speed = 1;
	public Animator animator;
	public GameObject modelPrefab;
	public Transform spawnpoint;
	public Vector3 cameraOffset;
	public float sensitivity = 100;

	private Transform raycastSource;

	//private CharacterController _characterController;
	private Rigidbody _rigidbody;
	private Camera cam;
	private Transform _camTransform;
	private float headRotation = 0f;
	private bool didntMove;
	private bool isGrounded;
	private GameObject model;
	private List<Transform> spawnpoints = new List<Transform>();
	private GameObject camera;
    
	private List<Ammo> ammo = new List<Ammo>();

	public int selectedAmmoIndex;
    
	private GameObject lastFocussedInteractable;
    
	private static readonly int OnGround = Animator.StringToHash("OnGround");
	private static readonly int Right = Animator.StringToHash("Right");
	private static readonly int Forward = Animator.StringToHash("Forward");
	private static readonly int Crouch = Animator.StringToHash("Crouch");
	private static readonly int Jump = Animator.StringToHash("Jump");

	public Character(string playerId, int inputTickrate, float jumpStrength, float speed, AnimatorOverrideController animator, GameObject modelPrefab, Transform spawnpoint, Vector3 cameraOffset, float sensitivity) {
		this.entity = new PlayerEntity() {
			playerId = playerId
		};
		this.jumpStrength = jumpStrength;
		this.speed = speed;
		this.modelPrefab = modelPrefab;
		this.spawnpoint = spawnpoint;
		this.cameraOffset = cameraOffset;
		this.sensitivity = sensitivity;
		
		SchedulerSystem.AddJob(Update, 0, inputTickrate, -1);
		
		
        
		// lock cursor
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		foreach (Spawnpoint spawnpoint2 in Object.FindObjectsOfTypeAll(typeof(Spawnpoint))) {
			spawnpoints.Add(spawnpoint2.transform);
		}

		spawnpoint = spawnpoints[Random.Range(0, spawnpoints.Count-1)];

		model = Object.Instantiate(modelPrefab, spawnpoint.position, spawnpoint.rotation);
		this.animator = model.GetComponent<Animator>();
		this.animator.runtimeAnimatorController = animator;
        
		model.SetActive(false);
        
		model.transform.position = spawnpoint.position;
		model.transform.rotation = spawnpoint.rotation;
        
		model.SetActive(true);

		//_characterController = GetComponent<CharacterController>();

		// locally attach camera to player object
		GameObject camRotationPoint = new GameObject();
		raycastSource = Object.Instantiate(new GameObject(), model.gameObject.transform).transform;
		raycastSource.localPosition = new Vector3(0, 1.6f, 0);
		camRotationPoint.transform.SetParent(model.transform);
		camRotationPoint.transform.position = this.animator.GetBoneTransform(HumanBodyBones.Head).position;
		camera = new GameObject();
		camera.tag = "MainCamera";
		cam = camera.AddComponent<Camera>();
		cam.transform.SetParent(camRotationPoint.transform);
		cam.transform.position = camRotationPoint.transform.position;
		cam.transform.position += cameraOffset;

	    
		// set model transform (pos, rot, parent)
		_rigidbody = model.gameObject.AddComponent<Rigidbody>();
		_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		CapsuleCollider col = model.gameObject.AddComponent<CapsuleCollider>();
		col.center = new Vector3(0, 0.5f, 0);
	}

	public void Update() {
		animator.speed = speed;
		float rightSpeed = 0;
		float frontSpeed = 0;
		float mouseX = 0;
		float mouseY = 0;
            
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
		raycastSource.transform.localEulerAngles = new Vector3(headRotation, mouseX, 0f);
#if UNITY_EDITOR
		Debug.DrawRay(raycastSource.position, raycastSource.forward * 3, Color.green, 1);
#endif
		if (Physics.Raycast(new Ray(raycastSource.position, raycastSource.forward * 3), 
		                    out var hit, 
		                    5, 
		                    LayerMask.GetMask("Interactable"))) {
			GameObject hitGO = hit.collider.gameObject;
			if (lastFocussedInteractable != hitGO) {
				if (lastFocussedInteractable != null) {
					lastFocussedInteractable.GetComponent<Interactable>().Deselect();
				}
				Debug.Log("true");
				hitGO.GetComponent<Interactable>().Select();
				lastFocussedInteractable = hitGO;
			}

			Debug.Log("Interact Input: " + Input.GetAxis("Interact"));
			if (Input.GetAxis("Interact") > 0) {
				// harvest plant
				if (hitGO.TryGetComponent(out Plant plant)) {
					if (plant.growthStage == 100) { 
						AddAmmo(plant.ammoObject, plant.Harvest());
					}
				}
				// collect simple Pickup
				if (hitGO.TryGetComponent(out SimplePickup pickup)) {
					if (pickup.isAmmo) { 
						AddAmmo(pickup.ammoObject, pickup.PickUp());
					} else {
						// Do things when its not ammo
					}
				}
			}
		} else if (lastFocussedInteractable != null){
			lastFocussedInteractable.GetComponent<Interactable>().Deselect();
			lastFocussedInteractable = null;
		}

		// Schießen
		if (Input.GetAxis("Fire1") > 0) {
			Shoot();
		}

		// Munition Wechseln
		if (Input.mouseScrollDelta.y > 0) {
			Debug.Log(selectedAmmoIndex + 1 >= ammo.Count ? 0 : selectedAmmoIndex + 1);
			selectedAmmoIndex = selectedAmmoIndex + 1 >= ammo.Count ? 0 : selectedAmmoIndex + 1;
			Debug.Log(ammo[selectedAmmoIndex].type);
		} else if (Input.mouseScrollDelta.y < 0) {
			Debug.Log(selectedAmmoIndex - 1 < 0 ? ammo.Count - 1 : selectedAmmoIndex - 1);
			selectedAmmoIndex = selectedAmmoIndex - 1 <= 0 ? ammo.Count - 1 : selectedAmmoIndex - 1;
			Debug.Log(ammo[selectedAmmoIndex].type);
		}
		
		
		

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
			_rigidbody.AddForce(new Vector3(0.0f, Input.GetAxis("Jump") * jumpStrength, 0.0f));
			animator.SetBool(Crouch, Input.GetAxis("Crouch") > 0);
			animator.SetFloat(Jump, Input.GetAxis("Jump"));  //_rigidbody.velocity.y);
		}

		// if character doesn't move free camera rotation, otherwise rotate character
		if (frontSpeed == 0 && rightSpeed == 0) {
			model.transform.RotateAround(camera.transform.parent.transform.position, Vector3.up, mouseX);
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
		camera.transform.parent.localEulerAngles = new Vector3(headRotation, mouseX, 0f);
        
	}

	private void Shoot() {
		GameObject shot = Object.Instantiate(ammo[selectedAmmoIndex].type.gameObject, raycastSource.position, raycastSource.rotation);
		shot.AddComponent<Rigidbody>().AddForce(raycastSource.forward * 1000);
	}

	private void AddAmmo(AmmoType ammoPrefab, int amnt) {
		bool exists = false;
		for (var i = 0; i < ammo.Count; i++) {
			var ammo1 = ammo[i];
			if (ammo1.type == ammoPrefab) {
				ammo1.amount += amnt;
				exists = true;
			}
		}

		if (!exists) {
			ammo.Add(new Ammo { type = ammoPrefab, amount = amnt});
		}
		
	}
	
	private class Ammo {
		public AmmoType type;
		public int amount;
	}
	
	
}