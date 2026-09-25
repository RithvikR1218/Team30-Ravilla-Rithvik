using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;

    // Short window after taking damage where further hits are ignored
    public float invincibleTime = 1f;

    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvincible => invincibleTimer > 0f;

    // (current, max) - for the UI
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private float invincibleTimer;


    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Update()
    {
        if (invincibleTimer > 0f) invincibleTimer -= Time.deltaTime;
    }

    // Called by enemies and hazards (see Hazard)
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead || IsInvincible) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        invincibleTimer = invincibleTime;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead) OnDied?.Invoke();
    }

    // Instant death, e.g. falling out of the level
    public void Kill()
    {
        if (IsDead) return;

        CurrentHealth = 0;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnDied?.Invoke();
    }

    // Full health plus a moment of invincibility, so nothing can hit the player the instant they respawn
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        invincibleTimer = invincibleTime;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
