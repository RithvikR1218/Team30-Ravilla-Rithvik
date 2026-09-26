using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows the player's health as a row of hearts and whether Dash is charged
public class PlayerHUD : MonoBehaviour
{
    public PlayerHealth health;
    public PlayerDash dash;

    [Header("Health")]
    // One copy of this is made per point of max health
    public Image heartTemplate;
    public Color heartFullColor = new Color(1f, 0.3f, 0.35f, 1f);
    public Color heartEmptyColor = new Color(0.25f, 0.25f, 0.28f, 0.8f);

    [Header("Dash")]
    // Shown behind the Dash icon while a charge is stored for the next beat
    public GameObject chargedIndicator;
    public float chargedPulseSpeed = 10f;

    private readonly List<Image> hearts = new List<Image>();


    void Awake()
    {
        if (health == null) health = FindFirstObjectByType<PlayerHealth>();
        if (dash == null && health != null) dash = health.GetComponent<PlayerDash>();
        if (heartTemplate != null) heartTemplate.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (health != null) health.OnHealthChanged += UpdateHearts;
    }

    void OnDisable()
    {
        if (health != null) health.OnHealthChanged -= UpdateHearts;
    }

    void Start()
    {
        if (health != null) UpdateHearts(health.CurrentHealth, health.maxHealth);
    }

    void Update()
    {
        if (chargedIndicator == null) return;

        bool charged = dash != null && dash.IsCharged;
        chargedIndicator.SetActive(charged);

        if (charged)
        {
            float pulse = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * chargedPulseSpeed);
            chargedIndicator.transform.localScale = Vector3.one * pulse;
        }
    }

    private void UpdateHearts(int current, int max)
    {
        if (heartTemplate == null) return;

        // Make sure there is exactly one heart per point of max health
        while (hearts.Count < max)
        {
            Image heart = Instantiate(heartTemplate, heartTemplate.transform.parent);
            heart.gameObject.SetActive(true);
            hearts.Add(heart);
        }
        while (hearts.Count > max)
        {
            Destroy(hearts[hearts.Count - 1].gameObject);
            hearts.RemoveAt(hearts.Count - 1);
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].color = i < current ? heartFullColor : heartEmptyColor;
        }
    }
}
