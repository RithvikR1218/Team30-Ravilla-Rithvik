using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public Vector2 travel = new Vector2(4f, 0f);
    public float speed = 2f;
    public float waitTime = 0.5f;

    public Vector2 Velocity { get; private set; }

    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 endPos;
    private bool movingToEnd = true;
    private float waitTimer;


    void Reset()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        startPos = rb.position;
        endPos = startPos + travel;
    }

    void FixedUpdate()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            Velocity = Vector2.zero;
            return;
        }

        Vector2 target = movingToEnd ? endPos : startPos;
        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);

        Velocity = (next - rb.position) / Time.fixedDeltaTime;
        rb.MovePosition(next);

        if (next == target)
        {
            movingToEnd = !movingToEnd;
            waitTimer = waitTime;
        }
    }
}
