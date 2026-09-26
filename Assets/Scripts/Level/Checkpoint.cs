using UnityEngine;

// Touching a checkpoint makes it the player's respawn point
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    // Where the player reappears, relative to the checkpoint
    public Vector2 spawnOffset = new Vector2(0f, 0.5f);

    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    public Color activeColor = new Color(0.3f, 1f, 0.45f, 1f);

    private static Checkpoint current;

    private SpriteRenderer spriteRenderer;


    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetColor(inactiveColor);
    }

    void OnDestroy()
    {
        if (current == this) current = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (current == this) return;

        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
        if (player == null) return;

        player.SetCheckpoint((Vector2)transform.position + spawnOffset);

        if (current != null) current.SetColor(current.inactiveColor);
        current = this;
        SetColor(activeColor);
    }

    private void SetColor(Color color)
    {
        if (spriteRenderer != null) spriteRenderer.color = color;
    }
}
