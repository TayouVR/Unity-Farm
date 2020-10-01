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
        
        float x = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float y = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime * -1f;
        //targetModel.SetFloat("Turn", x);
        targetModel.transform.Rotate(0f, x, 0f);
        headRotation += y;
        if (headRotation <= -90) {
            headRotation = -90;
        }
        if (headRotation >= 90) {
            headRotation = 90;
        }
        cameraOffset.parent.localEulerAngles = new Vector3(headRotation, 0f, 0f);
        
        bool isGrounded;
        //isGrounded = _characterController.isGrounded;
        isGrounded = Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, 2f, 1);
        targetModel.SetBool("OnGround", isGrounded);
        if (isGrounded) {
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            //_rigidbody.AddForce(movement * speed);
            // SetAnimator Info
            targetModel.SetFloat("Right", movement.x);
            targetModel.SetFloat("Forward", movement.z);
            //_rigidbody.AddForce(new Vector3(0.0f, Input.GetAxis("Jump") * jumpStrength, 0.0f));
            targetModel.SetBool("Crouch", Input.GetAxis("Crouch") > 0);
        } else {
            targetModel.SetFloat("Jump", Input.GetAxis("Jump"));  //_rigidbody.velocity.y);
        }
        //targetModel.transform.position = transform.position;
        transform.position = targetModel.transform.position;
        transform.rotation = targetModel.transform.rotation;
    }
}
