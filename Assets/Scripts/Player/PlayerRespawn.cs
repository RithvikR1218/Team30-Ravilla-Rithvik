using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerHealth), typeof(PlayerDash))]
public class PlayerRespawn : MonoBehaviour
{
    // Falling below this height counts as falling out of the level
    public float fallDeathY = -10f;

    public Vector2 RespawnPoint { get; private set; }

    public event Action OnRespawned;

    private Rigidbody2D rb;
    private PlayerHealth health;
    private PlayerDash dash;
    private CameraFollow cameraFollow;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
        dash = GetComponent<PlayerDash>();

        // Until a checkpoint is reached, respawn where the level started
        RespawnPoint = transform.position;
    }

    void Start()
    {
        foreach (CameraFollow cam in FindObjectsByType<CameraFollow>(FindObjectsSortMode.None))
        {
            if (cam.target == transform) cameraFollow = cam;
        }
    }

    void OnEnable()
    {
        health.OnDied += Respawn;
    }

    void OnDisable()
    {
        health.OnDied -= Respawn;
    }

    void FixedUpdate()
    {
        if (rb.position.y < fallDeathY)
        {
            health.Kill();
        }
    }

    public void SetCheckpoint(Vector2 point)
    {
        RespawnPoint = point;
    }

    public void Respawn()
    {
        // Cancel any dash or stored charge so the player doesn't respawn mid-dash
        dash.ResetDash();

        rb.linearVelocity = Vector2.zero;
        rb.position = RespawnPoint;
        transform.position = RespawnPoint;

        health.ResetHealth();

        // Jump the camera straight to the player instead of sliding across the level
        if (cameraFollow != null) cameraFollow.SnapToTarget();

        OnRespawned?.Invoke();
    }
}
