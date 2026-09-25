using UnityEngine;

// Spins an object around its Z axis (e.g. a saw blade's sprite)
public class Spinner : MonoBehaviour
{
    // Degrees per second; negative spins clockwise
    public float speed = -360f;

    void Update()
    {
        transform.Rotate(0f, 0f, speed * Time.deltaTime);
    }
}
