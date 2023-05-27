using UnityEngine;

namespace Helper
{
    public struct Inputs
    {
        public Vector2 movementInput;
        public Vector3 movement;
        public float right,forward;
    }
    
    public enum MovementType
    {
        Stand,
        Walk,
        Sprint,
        Jump,
        Slide,
        WallRunL,
        WallRunR,
        WallRunF,
        WallJump,
        Fall,
        LedgeGrab,
        LedgeMove,
        Await,
    }
    
    public class Movement : MonoBehaviour
    {
        /// <summary>
        /// Object used only for calculating rotations
        /// </summary>
        public Transform rotation;

        public MovementType movementType;
        public MovementType previousMovementType { get; private set; }

        public bool endStateFlag;

        [SerializeField] private Transform handPositionWhileHanging;
        
        [SerializeField]private Transform Camera;
        private Inputs Input;

        private new Rigidbody rigidbody;

        /// <summary>
        /// rotation in next frame
        /// </summary>
        private Quaternion nextRotation;
        /// <summary>
        /// velocity in next frame
        /// </summary>
        private Vector3 nextVelocity;
        /// <summary>
        /// position in next frame
        /// </summary>
        private Vector3 nextPosition;
        /// <summary>
        /// velocity at the start of a state
        /// </summary>
        private Vector3 startVelocity;
        /// <summary>
        /// position at the start of a state
        /// </summary>
        private Vector3 startPosition;
        /// <summary>
        /// used by start functions
        /// </summary>
        private Vector3 tempDirection;
        /// <summary>
        /// name
        /// </summary>
        private float tempJumpVelocity;
        /// <summary>
        /// to check time since the state started
        /// </summary>
        public float currentTime { get; private set; }
        /// <summary>
        /// to check time since the character has been moving at full speed
        /// </summary>
        public float fullSpeedTime { get; private set; }
        /// <summary>
        /// to check time since the character has been moving at full input
        /// </summary>
        public float fullInputTime { get; private set; }

        public void SetInput(Inputs inputs) => Input = inputs;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        public void StartStanding()
        {
            rigidbody.useGravity = true;
            StartStateSetup();
            movementType = MovementType.Stand;
        }
        public void Stand()
        {
            nextVelocity = rigidbody.velocity;
            nextVelocity.x = nextVelocity.z = 0;
            rigidbody.velocity = nextVelocity;
            rigidbody.angularVelocity = Vector3.zero;
            currentTime += Time.deltaTime;
            // TODO: Show different waiting Animations
        }

        public void StartWalking()
        {
            rigidbody.useGravity = true;
            StartStateSetup();
            movementType = MovementType.Walk;
        }
        public void Walk(float forwardSpeed, float rightSpeed, float acceleration, float rotationSpeed, float spreadToBeConsideredFast)
        {
            currentTime += Time.deltaTime;
            fullInputTime += Time.deltaTime;
            fullSpeedTime += Time.deltaTime;
            if (Input.movementInput.magnitude < 0.8f) fullInputTime = 0;
            if (rigidbody.velocity.magnitude < spreadToBeConsideredFast) fullSpeedTime = 0;

            if (Input.movement.magnitude > 0.1)
            {
                var position = transform.position;
                var cameraPosition = Camera.position;
                Vector3 dir = CameraDirection;
                dir = dir.normalized;
                nextRotation = Quaternion.LookRotation(dir,Vector3.up);
                rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, nextRotation, rotationSpeed);

                var rigidbodyVelocity = rigidbody.velocity;
                nextVelocity = new Vector3(Input.right * rightSpeed, rigidbodyVelocity.y, Input.forward * forwardSpeed);
                nextVelocity = Quaternion.Euler(dir) * transform.rotation * nextVelocity;
                rigidbody.velocity = Vector3.Lerp(rigidbodyVelocity, nextVelocity, acceleration);
            }
            else
            {
                rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, new Vector3(0, rigidbody.velocity.y, 0),
                    Time.deltaTime * 1000 * acceleration);
            }

            rigidbody.angularVelocity = Vector3.zero;
        }

        public void StartSprinting()
        {
            rigidbody.useGravity = true;
            StartStateSetup();
            movementType = MovementType.Sprint;
        }
        public void Sprint(float speed, float acceleration, float rotationSpeed, float spreadToBeConsideredFast)
        {
            currentTime += Time.deltaTime;
            fullInputTime += Time.deltaTime;
            fullSpeedTime += Time.deltaTime;
            if (Input.movementInput.magnitude < 0.8f) fullInputTime = 0;
            if (rigidbody.velocity.magnitude < spreadToBeConsideredFast) fullSpeedTime = 0;
            
            var position = transform.position;
            var cameraPosition = Camera.position;
            if (Input.movement.magnitude > 0.1f)
            {
                Vector3 dir = CameraDirection;
                dir = dir.normalized;
                rotation.forward = dir;
                nextVelocity = (rotation.forward * Input.forward + rotation.right * Input.right) * speed;
                nextRotation = Quaternion.LookRotation(
                    (nextVelocity).normalized, Vector3.up);
                rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, nextRotation, rotationSpeed);
                nextVelocity.y = rigidbody.velocity.y;
            }
            else
            {
                nextVelocity = new Vector3(0, rigidbody.velocity.y, 0);
            }
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, nextVelocity, acceleration);
        }

        public void StartJump(float jumpVelocity)
        {
            rigidbody.useGravity = false;
            StartStateSetup();
            tempJumpVelocity = jumpVelocity;
            movementType = MovementType.Jump;
        }
        public void Jump(AnimationCurve gravity, float duration, float maxGravity, float movementAdjustmentSpeed)
        {
            currentTime += Time.deltaTime;
            
            tempJumpVelocity -= gravity.Evaluate(currentTime/duration) * maxGravity * Time.deltaTime;
            nextVelocity = rigidbody.velocity + 
                           transform.right * Input.right * movementAdjustmentSpeed * Time.deltaTime * 1000 +
                           transform.forward * Input.forward * movementAdjustmentSpeed * Time.deltaTime * 1000;
            nextVelocity.y = tempJumpVelocity;
            rigidbody.velocity = nextVelocity;
            
            rigidbody.angularVelocity = Vector3.zero;
            

            CheckEndOfStateFlag(duration);
        }

        public void StartSliding()
        {
            rigidbody.useGravity = true;
            StartStateSetup();
            movementType = MovementType.Slide;
        }
        public void Slide(AnimationCurve speedFall, float speed, float duration, float movementAdjustmentSpeed)
        {
            currentTime += Time.deltaTime;
            if (currentTime / duration > 1) endStateFlag = true;
            nextVelocity = transform.forward * speed * speedFall.Evaluate(currentTime / duration) + 
                           transform.forward * Input.forward * Time.deltaTime * 1000 * movementAdjustmentSpeed+ 
                           transform.right * Input.right * Time.deltaTime * 1000 * movementAdjustmentSpeed;
            nextVelocity.y = rigidbody.velocity.y;
            rigidbody.velocity = nextVelocity;
        }

        public void StartWallRunL(float jumpVelocity)
            => StartWallRunLeftRight(jumpVelocity, MovementType.WallRunL);
        public void StartWallRunR(float jumpVelocity)
            => StartWallRunLeftRight(jumpVelocity, MovementType.WallRunR);
        public void WallRunLeft(AnimationCurve gravity, RaycastHit hit, float speed, float duration, float maxGravity) 
            => WallRunLeftRight(1,gravity,hit,speed,duration,maxGravity);
        public void WallRunRight(AnimationCurve gravity, RaycastHit hit, float speed, float duration, float maxGravity)
            => WallRunLeftRight(-1,gravity,hit,speed,duration,maxGravity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jumpVelocity"></param>
        /// <param name="type"></param>
        private void StartWallRunLeftRight(float jumpVelocity, MovementType type)
        {
            rigidbody.useGravity = false;
            StartStateSetup();
            tempJumpVelocity = jumpVelocity;
            
            movementType = type;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction">(-1) -> wall on right  ||   (1) -> wall on left</param>
        /// <param name="gravity"></param>
        /// <param name="hit"></param>
        /// <param name="speed"></param>
        /// <param name="duration"></param>
        /// <param name="maxGravity"></param>
        private void WallRunLeftRight(int direction, AnimationCurve gravity, RaycastHit hit, float speed, float duration, float maxGravity)
        {
            currentTime += Time.deltaTime;
            
            Vector3 cross = Vector3.Cross(transform.right, hit.normal);
            // TODO: cross function to check if the character can actually wall run further
            transform.right = hit.normal * direction;
            
            tempJumpVelocity -= gravity.Evaluate(currentTime / duration) * maxGravity * Time.deltaTime;
            nextVelocity = transform.forward * speed + 
                           Vector3.up * tempJumpVelocity + 
                           hit.normal * 0.5f * direction;
            rigidbody.velocity = nextVelocity;
            
            CheckEndOfStateFlag(duration);
        }
        
        public void StartWallRunF(float jumpVelocity)
        {
            rigidbody.useGravity = false;
            StartStateSetup();
            tempJumpVelocity = jumpVelocity;
            movementType = MovementType.WallRunF;
        }
        public void WallRunF(AnimationCurve gravity, float duration, float maxGravity, RaycastHit hit)
        {
            currentTime += Time.deltaTime;
            tempJumpVelocity -= gravity.Evaluate(currentTime / duration) * maxGravity * Time.deltaTime;
            nextVelocity = Vector3.up * tempJumpVelocity;
            rigidbody.velocity = nextVelocity;
            CheckEndOfStateFlag(duration);
        }

        public void StartWallJump(Vector3 direction, float jumpVelocity)
        {
            rigidbody.useGravity = false;
            StartStateSetup();
            startVelocity = rigidbody.velocity;
            tempDirection = direction;
            tempDirection.y = 0;
            tempDirection.Normalize();
            tempJumpVelocity = jumpVelocity;
            movementType = MovementType.WallJump;
        }
        public void WallJump(AnimationCurve gravity, float speed, float duration, float maxGravity)
        {
            currentTime += Time.deltaTime;
            transform.forward = tempDirection;
            rigidbody.angularVelocity = Vector3.zero;
            tempJumpVelocity -= gravity.Evaluate(currentTime / duration) * maxGravity * Time.deltaTime;
            nextVelocity = tempDirection * speed + Vector3.up * tempJumpVelocity;
            rigidbody.velocity = nextVelocity;
            CheckEndOfStateFlag(duration);
        }

        public void StartLedgeGrab()
        {
            rigidbody.useGravity = false;
            StartStateSetup();
            movementType = MovementType.LedgeGrab;
        }
        public void LedgeGrab(RaycastHit ledgeGrabPoint, float speed)
        {
            
        }

        public void StartLedgeMove()
        {
            rigidbody.useGravity = false;
            StartStateSetup();
            movementType = MovementType.LedgeMove;
        }
        public void LedgeMove()
        {
            
        }

        public void StartFall()
        {
            rigidbody.useGravity = true;
            endStateFlag = false;
            currentTime = 0;
            previousMovementType = movementType;
            movementType = MovementType.Fall;
        }
        public void Fall()
        {
            currentTime += Time.deltaTime;
            // TODO: manage falling animations
        }
        
        
        public void StartClimbUp()
        {
            rigidbody.useGravity = false;
            currentTime = 0;
            fullInputTime = 0;
            fullSpeedTime = 0;
            startPosition = transform.position;
            startVelocity = rigidbody.velocity;
            endStateFlag = false;
            movementType = MovementType.Await;
        }
        public void ClimbUo()
        {
            
        }
        
        public void StartClimbDown()
        {
            rigidbody.useGravity = false;
            currentTime = 0;
            fullInputTime = 0;
            fullSpeedTime = 0;
            startPosition = transform.position;
            startVelocity = rigidbody.velocity;
            endStateFlag = false;
            movementType = MovementType.Await;
        }
        public void ClimbDown()
        {
            
        }
        
        /// <summary>
        /// Direction from camera position to character position
        /// </summary>
        private Vector3 CameraDirection=>  new Vector3(transform.position.x, Camera.position.y, transform.position.z) - Camera.position;
        
        private void StartStateSetup()
        {
            currentTime = 0;
            fullInputTime = 0;
            fullSpeedTime = 0;
            startPosition = transform.position;
            startVelocity = rigidbody.velocity;
            endStateFlag = false;
            previousMovementType = movementType;
        }

        private void CheckEndOfStateFlag(float duration)
        {
            if (currentTime / duration > 1) this.endStateFlag = false;
        }
        
        public void ConnectWithPlatform(Transform platform)
        {
            transform.parent = platform;
        }

        public void DisconnectWithPlatform(Transform platform)
        {
            Rigidbody platformRB = platform.GetComponent<Rigidbody>();
            Rigidbody transformRB = transform.GetComponent<Rigidbody>();
            transform.parent = null;
            transformRB.velocity += platformRB.velocity;
        }
    }
}