using UnityEngine;

// Blue box that blocks the way. Only a Perfect dash breaks it; a Good dash or walking into it just gets blocked.
[RequireComponent(typeof(Collider2D))]
public class BlueEnemy : MonoBehaviour
{
    // Brief flash when hit by a dash that wasn't Perfect, so the player knows the timing was off
    public Color blockedFlashColor = Color.white;
    public float flashDuration = 0.12f;

    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private float flashTimer;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
    }

    void Update()
    {
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f && spriteRenderer != null) spriteRenderer.color = baseColor;
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckHit(collision);

    // Also checked on Stay, for when the player starts a dash while already touching the box
    private void OnCollisionStay2D(Collision2D collision) => CheckHit(collision);

    private void CheckHit(Collision2D collision)
    {
        PlayerDash dash = collision.collider.GetComponentInParent<PlayerDash>();
        if (dash == null || !dash.IsDashing) return;

        if (dash.IsPerfectDash)
        {
            Break();
        }
        else if (flashTimer <= 0f && spriteRenderer != null)
        {
            spriteRenderer.color = blockedFlashColor;
            flashTimer = flashDuration;
        }
    }

    private void Break()
    {
        // Turn the collider off right away so the dash carries on through, Destroy only happens at the end of the frame
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject);
    }
}
