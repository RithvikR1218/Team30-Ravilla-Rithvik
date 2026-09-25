using BeatTiming;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerMovement))]
public class PlayerDash : MonoBehaviour
{
    //Dash distance depends on how close to the beat Space was pressed
    public float perfectDashDistance = 4f;
    public float goodDashDistance = 2.5f;
    public float dashDuration = 0.12f;

    public bool IsDashing => dashTimer > 0f;
    // True after a successful first press, waiting for the second press on the next beat
    public bool IsCharged => chargedBeat != NoCharge;

    private const int NoCharge = int.MinValue;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private float normalGravity;
    private float dashTimer;
    private float dashSpeed;
    private int chargedBeat = NoCharge;
    private Accuracy chargedTier;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        normalGravity = rb.gravityScale;
    }

    void Update()
    {
        // Judge in Update so the press is timed on the exact frame it happened
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !IsDashing && TimingJudge.Instance != null)
        {
            HandlePress(TimingJudge.Instance.Judge());
        }
    }

    // Dash takes two on-beat presses in a row: the first charges it, the second on the next beat fires it
    private void HandlePress(Judgement judgement)
    {
        // Miss: lose the charge, the pulse ring flashes red
        if (!judgement.IsHit)
        {
            chargedBeat = NoCharge;
            return;
        }

        if (IsCharged && judgement.Beat == chargedBeat + 1)
        {
            // Perfect dash only if both presses were Perfect
            bool bothPerfect = chargedTier == Accuracy.Perfect && judgement.Tier == Accuracy.Perfect;
            StartDash(bothPerfect ? perfectDashDistance : goodDashDistance);
            chargedBeat = NoCharge;
            return;
        }

        // First press, or the previous charge was too old: this press starts a new charge
        chargedBeat = judgement.Beat;
        chargedTier = judgement.Tier;
    }

    private void FixedUpdate()
    {
        if (!IsDashing) return;

        // Straight horizontal dash, no gravity
        rb.linearVelocity = new Vector2(dashSpeed, 0f);

        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            rb.gravityScale = normalGravity;
        }
    }

    private void StartDash(float distance)
    {
        dashSpeed = movement.FacingDirection * distance / dashDuration;
        dashTimer = dashDuration;
        rb.gravityScale = 0f;
    }
}
