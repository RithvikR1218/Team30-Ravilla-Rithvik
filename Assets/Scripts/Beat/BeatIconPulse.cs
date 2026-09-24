using UnityEngine;
using UnityEngine.UI;

namespace BeatTiming
{
    /// <summary>
    /// Secondary cue for the ability icon: it "punches" on every beat and lights up while a press
    /// would count, so the player can read the timing in peripheral vision.
    /// Works on a UI Image/Graphic or a SpriteRenderer.
    /// </summary>
    public class BeatIconPulse : MonoBehaviour
    {
        [SerializeField] BeatConductor conductor;
        [SerializeField] TimingJudge judge;
        [SerializeField] Graphic uiGraphic;
        [SerializeField] SpriteRenderer spriteRenderer;

        [SerializeField, Min(0f)] float punchScale = 0.25f;
        [SerializeField, Min(0.01f)] float punchDecay = 12f;
        [SerializeField] Color idleColor = new Color(0.55f, 0.55f, 0.6f, 1f);
        [SerializeField] Color readyColor = new Color(0.35f, 0.9f, 1f, 1f);

        Vector3 baseScale;
        float punch;

        void Awake()
        {
            baseScale = transform.localScale;
            if (uiGraphic == null) uiGraphic = GetComponent<Graphic>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            if (conductor == null) conductor = BeatConductor.Instance;
            if (judge == null) judge = TimingJudge.Instance;
            if (conductor != null) conductor.OnBeat += Punch;
        }

        void OnDisable()
        {
            if (conductor != null) conductor.OnBeat -= Punch;
            transform.localScale = baseScale;
        }

        void Punch(int beat) => punch = 1f;

        void Update()
        {
            punch *= Mathf.Exp(-punchDecay * Time.deltaTime);
            transform.localScale = baseScale * (1f + punchScale * punch);

            bool ready = judge != null && judge.InGoodWindow();
            Color c = ready ? readyColor : idleColor;
            if (uiGraphic != null) uiGraphic.color = c;
            if (spriteRenderer != null) spriteRenderer.color = c;
        }
    }
}
