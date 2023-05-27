using Helper;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Transform Camera;
    public float jumpFutureTime = 1.0f;
    
    private float jumpFalseAfter;
    
    private PlayerInput PlayerInput;

    private Rigidbody rb;
    
    public Inputs Inputs { get; private set; }
    public Vector2 movementInput { get; private set; }
    public Vector3 movement{ get; private set; }
    public float forward{ get; private set; }
    public float right{ get; private set; }
    public bool crouch{ get; private set; }
    public bool sprint{ get; private set; }
    public bool jump{ get; private set; }
    public Quaternion cameraLook3D { get; private set; }
    public Quaternion cameraLook2D { get; private set; }
    public Vector3 cameraForward3D { get; private set; }
    public Vector3 cameraForward2D { get; private set; }

    private bool InputEnabled;
    
    private void Awake()
    {
        if (!InputEnabled)
        {
            rb = GetComponent<Rigidbody>();
            PlayerInput = new PlayerInput();
            PlayerInput.Enable();
            InputEnabled = true;
            PlayerInput.Player.Movement.performed += MovementOnPerformed;
            PlayerInput.Player.Crouch.started+=CrouchOnStarted;
            PlayerInput.Player.Crouch.canceled+=CrouchOnCanceled;
            PlayerInput.Player.Sprint.started+=SprintOnStarted;
            PlayerInput.Player.Sprint.canceled+=SprintOnCanceled;
            PlayerInput.Player.Jump.started+=JumpOnStarted;
            Inputs = new Inputs();
        }
    }

    private void OnEnable()
    {
        if (!InputEnabled)
        {
            rb = GetComponent<Rigidbody>();
            PlayerInput = new PlayerInput();
            PlayerInput.Enable();
            InputEnabled = true;
            PlayerInput.Player.Movement.performed += MovementOnPerformed;
            PlayerInput.Player.Crouch.started+=CrouchOnStarted;
            PlayerInput.Player.Crouch.canceled+=CrouchOnCanceled;
            PlayerInput.Player.Sprint.started+=SprintOnStarted;
            PlayerInput.Player.Sprint.canceled+=SprintOnCanceled;
            PlayerInput.Player.Jump.started+=JumpOnStarted;
            Inputs = new Inputs();
        }
        
    }

    private void Update()
    {
        if (jump && Time.time > jumpFalseAfter) jump = false;
        cameraForward3D = Camera.forward;
        cameraForward2D = cameraForward3D;
        Vector3 forward2D = cameraForward2D;
        forward2D.y = 0;
        cameraForward2D = forward2D.normalized;
        cameraLook3D = Camera.rotation;
        Vector3 look2DEulerAngles = transform.position - Camera.position;
        look2DEulerAngles.y = 0;
        cameraLook2D = Quaternion.LookRotation(look2DEulerAngles.normalized, Vector3.up);
    }

    
    private void SprintOnCanceled(InputAction.CallbackContext obj)
    {
        sprint = false;
    }

    private void SprintOnStarted(InputAction.CallbackContext obj)
    {
        sprint = true;
    }
    
    private void JumpOnStarted(InputAction.CallbackContext obj)
    {
        jump = true;
        jumpFalseAfter = Time.time + jumpFutureTime;
    }

    private void CrouchOnCanceled(InputAction.CallbackContext obj)
    {
        crouch = false;
    }

    private void CrouchOnStarted(InputAction.CallbackContext obj)
    {
        crouch = true;
    }

    // TODO: used to check cayoty time
    public void UnGrounded()
    {
        jump = false;
    }

    private void MovementOnPerformed(InputAction.CallbackContext obj)
    {
        movementInput = obj.ReadValue<Vector2>();
        Vector3 move;
        move.x = movementInput.x;
        move.y = 0;
        move.z = movementInput.y;
        movement = Camera.rotation * move;
        forward = move.z;
        right = move.x;
        Inputs inputs = Inputs;
        inputs.movementInput = movementInput;
        inputs.movement = movement;
        inputs.forward = forward;
        inputs.right = right;
        Inputs = inputs;
    }
}
