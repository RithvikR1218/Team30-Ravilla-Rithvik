using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(0f, 1f);
    public float smoothTime = 0.15f;

    public bool useBounds = false;
    public Vector2 minPosition = new Vector2(-10f, -5f);
    public Vector2 maxPosition = new Vector2(10f, 5f);

    private Vector3 velocity;


    void Start()
    {
        if (target != null)
        {
            transform.position = GetTargetPosition();
        }
    }

    // LateUpdate runs after the player has moved this frame
    void LateUpdate()
    {
        if (target == null) return;

        transform.position = Vector3.SmoothDamp(transform.position, GetTargetPosition(), ref velocity, smoothTime);
    }

    // Jump straight to the target with no smoothing, e.g. after the player respawns
    public void SnapToTarget()
    {
        if (target == null) return;

        transform.position = GetTargetPosition();
        velocity = Vector3.zero;
    }

    private Vector3 GetTargetPosition()
    {
        Vector2 pos = (Vector2)target.position + offset;

        if (useBounds)
        {
            pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
        }

        // Keep the camera's own z so it stays in front of the 2D scene
        return new Vector3(pos.x, pos.y, transform.position.z);
    }
}
