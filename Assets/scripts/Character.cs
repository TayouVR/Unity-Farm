using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Rigidbody))]
public class Character : MonoBehaviour {

    public float jumpStrength = 1;
    public float speed = 1;
    public Camera cam;
    public Animator targetModel;
    public Transform spawnpoint;
    public Transform cameraOffset;
    [SerializeField] float sensitivity = 100;

    //private CharacterController _characterController;
    //private Rigidbody _rigidbody;
    private Transform _camTransform;
    float headRotation = 0f;
    private bool didntMove;

    // Start is called before the first frame update
    void Start() {
        
        // lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        transform.position = spawnpoint.position;
        transform.rotation = spawnpoint.rotation;

        //_characterController = GetComponent<CharacterController>();
        //_rigidbody = GetComponent<Rigidbody>();
        
        // putCamera into Camera offset 
        cam.transform.SetParent(cameraOffset);
        cam.transform.position = cameraOffset.position;
        cam.transform.rotation = cameraOffset.rotation;
        
        // set model transform (pos, rot, parent)
        //targetModel.GetComponent<Transform>().SetParent(GetComponent<Transform>());
        targetModel.transform.position = transform.position;
        targetModel.transform.rotation = new Quaternion();
        targetModel.gameObject.AddComponent<Rigidbody>();
        Rigidbody rb = targetModel.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        CapsuleCollider col = targetModel.gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.5f, 0);
    }

    // Update is called once per frame
    void FixedUpdate() {
        
        // mouse, camera
        float x = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float y = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime * -1f;
        headRotation += y;
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

        float rightSpeed = Input.GetAxis("Horizontal") / 2 * speedModifier;
        float frontSpeed = Input.GetAxis("Vertical") / 2 * speedModifier;
        
        bool isGrounded;
        //isGrounded = _characterController.isGrounded;
        isGrounded = Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, 2f, 1);
        targetModel.SetBool("OnGround", isGrounded);
        if (isGrounded) {
            //Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            //_rigidbody.AddForce(movement * speed);
            // SetAnimator Info
            targetModel.SetFloat("Right", rightSpeed); //movement.x);
            targetModel.SetFloat("Forward", frontSpeed); //movement.z);
            //_rigidbody.AddForce(new Vector3(0.0f, Input.GetAxis("Jump") * jumpStrength, 0.0f));
            targetModel.SetBool("Crouch", Input.GetAxis("Crouch") > 0);
        } else {
            targetModel.SetFloat("Jump", Input.GetAxis("Jump"));  //_rigidbody.velocity.y);
        }

        if (frontSpeed == 0 && rightSpeed == 0) {
            transform.RotateAround(cameraOffset.parent.transform.position, Vector3.up, x);
            //cameraOffset.parent.transform.Rotate(0f, x, 0f);
            didntMove = true;
        } else {
            if (didntMove) {
                targetModel.transform.rotation = transform.rotation;
                didntMove = false;
            }
            //targetModel.SetFloat("Turn", x);
            targetModel.transform.Rotate(0f, x, 0f);
        
            //targetModel.transform.position = transform.position;
            transform.position = targetModel.transform.position;
            transform.rotation = targetModel.transform.rotation;
        }
        cameraOffset.parent.localEulerAngles = new Vector3(headRotation, x, 0f);
    }
}
