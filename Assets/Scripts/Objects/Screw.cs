using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using Utils;

public class Screw : MonoBehaviour
{
    private const float HEIGHT_VELOCITY = 4f;
    private const float DECAY_ANGLE_RATE = 60f;
    private const float HEIGHT_PER_ANGLE = 0.005f;
    private const float ZIP_PART_HEIGHT = 0.6f;

    public float radius;
    [Range(0, 30)]
    public int screwTurns;
    public Transform screw;
    public Transform screwRender;
    public GameObject[] zipParts;

    private Rigidbody screwRigidBody;
    private SurfaceProperties surfaceBase;
    private SurfaceProperties surfaceScrew;
    private Vector3 originDirection;
    private Vector3 currentDirection;
    private Vector3 currentDirectionPerp;
    private Vector3 playerRelativePos;
    private Vector3 lastPlayerDirection;
    private float screwMaxAngle;
    private float currentScrewValue;
    private Timestamp timerMovedUp;

    private void Awake()
    {
        surfaceBase = GetComponent<SurfaceProperties>();
        surfaceScrew = screw.GetComponent<SurfaceProperties>();
        screwRigidBody = screw.GetComponent<Rigidbody>();
        screwMaxAngle = screwTurns * 45f;
    }
    private void Start()
    {
        originDirection = PosterizedDirection(screw.forward);
        screwRender.forward = originDirection;
    }
    private void FixedUpdate()
    {
        if (surfaceBase.landed || surfaceScrew.landed)
        {
            if (Player.Instance.Screw != this)
            {
                float dot = Vector3.Dot(currentDirection, Player.Instance.LookingDirection);
                float distance = (Player.Instance.transform.position - transform.position).FlattenY().magnitude;
                if (dot > 0.9f && distance < radius)
                    ScrewIn();
            }
            else
            {
                Player.Instance.transform.position = screw.TransformPoint(playerRelativePos);
                Player.Instance.LookAt(currentDirection);
            }
        }

        if (timerMovedUp.elapsed > 0.3f)
            UpdateScrewValue(-DECAY_ANGLE_RATE * Time.fixedDeltaTime);

        Vector3 target = Vector3.up * currentScrewValue * HEIGHT_PER_ANGLE;
        screw.localPosition = Vector3.MoveTowards
            (screw.localPosition,
            target,
            HEIGHT_VELOCITY * Time.fixedDeltaTime);

        int activePart = Mathf.FloorToInt(screw.localPosition.y / ZIP_PART_HEIGHT);
        for (int i = 0; i < zipParts.Length; i++)
            zipParts[i].gameObject.SetActive(i <= activePart);
    }

    public void PlayerRotationInput(Vector3 input, float magnitude)
    {
        float angle = Vector3.SignedAngle(lastPlayerDirection, input, Vector3.up);
        if (magnitude < 0.5f)
        {
            lastPlayerDirection = input;
            return;
        }

        if (Mathf.Abs(angle) > 1f && Mathf.Abs(angle) < 45f)
        {
            UpdateScrewValue(-angle);
            lastPlayerDirection = input;
        }
    }
    void ScrewIn ()
    {
        Player.Instance.ScrewIn(this);
        Player.Instance.transform.position = 
            screw.transform.position + Vector3.up * Player.Instance.coll.bounds.extents.y;
        lastPlayerDirection = Player.Instance.transform.forward;
        playerRelativePos = screw.InverseTransformPoint(Player.Instance.transform.position);
        UpdateScrewValue(0);      
    }
    private Vector3 PosterizedDirection (Vector3 direction )
    {
        Vector3 forward = Camera.main.RotateTowardsCamera(new Vector2(0f, 1f));
        Vector3 right = Camera.main.RotateTowardsCamera(new Vector2(1f, 0f));
        float angle = Vector3.Angle(forward, direction);
        float side = Mathf.Sign(Vector3.Dot(direction, right));
        float saturatedAngle = Mathf.Round(angle / 45f) * 45f * side;

        return Quaternion.AngleAxis(saturatedAngle, Vector3.up) * forward;
    } 
    private void UpdateScrewValue (float addAngle)
    {
        if (addAngle > 0f)
            timerMovedUp.Set();

        currentScrewValue = Mathf.Clamp(currentScrewValue + addAngle, 0, screwMaxAngle);  
        currentDirection = PosterizedDirection(Quaternion.AngleAxis(-currentScrewValue, Vector3.up) * originDirection);
        currentDirectionPerp = Quaternion.AngleAxis(90f, Vector3.up) * currentDirection;
        screwRender.forward = currentDirection;
    }
    private void OnDrawGizmos()
    {
        GizmosExtensions.DrawWireCircle(transform.position, radius);
    }
}
