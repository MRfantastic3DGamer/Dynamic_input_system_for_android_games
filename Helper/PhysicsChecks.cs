using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhysicsChecks : MonoBehaviour
{
    public LayerMask ground, wallRun, ledge;
    public bool Grounded { get; private set; }
    public bool WallOnRight { get; private set; }
    public bool WallOnLeft { get; private set; }
    public bool WallOnFront { get; private set; }
    public bool LedgeDetected { get; private set; }
    public bool LedgeGrabPointDetected { get; private set; }

    [SerializeField] private bool debugFront;
    public Transform frontRaycastPoint;
    public float forwardRaycastLength;
    [SerializeField] private bool debugLeftRight;
    public Transform wallLeftRaycastPoint;
    public Transform wallRightRaycastPoint;
    public float lrRaycastLength;
    [SerializeField] private bool debugGround;
    public Transform groundCastPoint;
    public float groundCastLength,groundCastRadious;
    [SerializeField] private bool debugLedge;
    public Transform ledgeCastPoint;
    public float ledgeCastDepth;
    
    public RaycastHit FrontHit{ get; private set; }
    public RaycastHit LeftHit{ get; private set; }
    public RaycastHit RightHit{ get; private set; }
    public RaycastHit GroundHit{ get; private set; }
    public RaycastHit LedgeTopHit{ get; private set; }
    public RaycastHit LedgeGrabPointHit{ get; private set; }

    private RaycastHit hit;

    private void Update()
    {
        WallOnFront = Physics.Raycast(frontRaycastPoint.position,frontRaycastPoint.forward, out hit, forwardRaycastLength, wallRun);
        FrontHit = hit;
        WallOnLeft = Physics.Raycast(wallLeftRaycastPoint.position,wallLeftRaycastPoint.forward, out hit, lrRaycastLength, wallRun);
        LeftHit = hit;
        WallOnRight = Physics.Raycast(wallRightRaycastPoint.position,wallRightRaycastPoint.forward, out hit, lrRaycastLength, wallRun);
        RightHit = hit;
        Grounded = Physics.SphereCast(groundCastPoint.position, groundCastRadious,Vector3.down, out hit, groundCastLength, ground);
        GroundHit = hit;
        LedgeDetected = Physics.BoxCast(center: ledgeCastPoint.position, halfExtents: ledgeCastPoint.localScale/2,
            direction: Vector3.down, out hit, orientation: ledgeCastPoint.rotation, maxDistance: ledgeCastDepth, layerMask: ledge);
        LedgeTopHit = hit;
        if (LedgeDetected)
        {
            var forward = transform.forward;
            LedgeGrabPointDetected = Physics.Raycast(origin: LedgeTopHit.point + Vector3.down * 0.01f + forward * -1,
                direction: forward, out hit, maxDistance: 1.1f, ledge);
            LedgeGrabPointHit = hit;
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (debugFront)
        {
            Gizmos.color = WallOnFront ? Color.green : Color.gray;
            var position = frontRaycastPoint.position;
            Gizmos.DrawLine(position, position + frontRaycastPoint.forward * forwardRaycastLength);
        }

        if (debugLeftRight)
        {
            Gizmos.color = WallOnLeft ? Color.green : Color.gray;
            var position = wallLeftRaycastPoint.position;
            Gizmos.DrawLine(position, position + wallLeftRaycastPoint.forward * lrRaycastLength);
            Gizmos.color = WallOnRight ? Color.green : Color.gray;
            position = wallRightRaycastPoint.position;
            Gizmos.DrawLine(position, position + wallRightRaycastPoint.forward * lrRaycastLength);
        }

        if (debugGround)
        {
            Gizmos.color = Grounded ? Color.green : Color.gray;
            if (Grounded) Gizmos.DrawSphere(GroundHit.point, groundCastRadious);
            else
            {
                var position = groundCastPoint.position;
                Gizmos.DrawSphere(position, groundCastRadious);
                Gizmos.DrawSphere(position + Vector3.down * groundCastLength, groundCastRadious);
            }
        }

        if (debugLedge)
        {
            if (LedgeDetected)
            {
                Gizmos.color = LedgeGrabPointDetected? Color.green : Color.red;
                Gizmos.DrawSphere(LedgeTopHit.point, 0.04f);
                if(LedgeGrabPointDetected)
                    Gizmos.DrawSphere(LedgeGrabPointHit.point, 0.05f);
            }
            else
            {
                Gizmos.color = Color.gray;
                Handles.matrix = transform.localToWorldMatrix;
                var position = ledgeCastPoint.localPosition;
                var localScale = ledgeCastPoint.localScale;
                Handles.DrawWireCube(position + Vector3.down * ledgeCastDepth * 0.5f,
                    new Vector3(localScale.x*2,ledgeCastDepth,localScale.z*2));
            }
        }
    }
    #endif
}
