using UnityEngine;

namespace BeatTiming
{
    /// <summary>
    /// The primary timing cue: a ring around the character that shrinks onto them as the beat
    /// approaches, changes color inside the Good/Perfect window, and flashes on the beat.
    /// Add it to the player (or any object the player is looking at). It creates its own visuals.
    /// </summary>
    public class BeatPulseRing : MonoBehaviour
    {
        [SerializeField] BeatConductor conductor;
        [SerializeField] TimingJudge judge;

        [Header("Shape (world units)")]
        [SerializeField, Min(0.1f)] float startDiameter = 3.2f;
        [SerializeField, Min(0.1f)] float endDiameter = 1.15f;
        [Tooltip("Fraction of each beat during which the ring is visible and closing in.")]
        [SerializeField, Range(0.1f, 1f)] float approachFraction = 0.8f;

        [Header("Colors")]
        [SerializeField] Color approachColor = new Color(1f, 1f, 1f, 0.55f);
        [SerializeField] Color goodColor = new Color(0.35f, 0.9f, 1f, 0.95f);
        [SerializeField] Color perfectColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] Color missColor = new Color(1f, 0.25f, 0.25f, 1f);

        [Header("Flash")]
        [SerializeField, Min(0.01f)] float flashDuration = 0.15f;
        [SerializeField, Min(0.1f)] float flashDiameter = 1.9f;

        [SerializeField] int sortingOrder = 50;

        SpriteRenderer ring;
        SpriteRenderer flash;
        Color feedbackColor;
        float feedbackUntil;

        void Awake()
        {
            if (conductor == null) conductor = BeatConductor.Instance;
            if (judge == null) judge = TimingJudge.Instance;
            ring = CreateChild("BeatRing", ProceduralSprites.Ring, sortingOrder);
            flash = CreateChild("BeatFlash", ProceduralSprites.Disc, sortingOrder - 1);
        }

        void OnEnable()
        {
            if (judge == null) judge = TimingJudge.Instance;
            if (judge != null) judge.OnJudged += ShowFeedback;
            SetVisible(true);
        }

        void OnDisable()
        {
            if (judge != null) judge.OnJudged -= ShowFeedback;
            SetVisible(false);
        }

        void LateUpdate()
        {
            if (conductor == null) conductor = BeatConductor.Instance;
            if (conductor == null || !conductor.IsRunning || conductor.SongTime < -conductor.SecondsPerBeat)
            {
                ring.enabled = flash.enabled = false;
                return;
            }

            float phase = conductor.SongTime < 0 ? 1f + (float)(conductor.SongTime / conductor.SecondsPerBeat) : conductor.BeatPhase;
            float secondsSinceBeat = conductor.SongTime < 0 ? float.MaxValue : phase * (float)conductor.SecondsPerBeat;

            // Ring: closes from startDiameter to endDiameter over the last part of each beat.
            float t = Mathf.InverseLerp(1f - approachFraction, 1f, phase);
            ring.enabled = t > 0f;
            ring.transform.localScale = WorldToLocal(Mathf.Lerp(startDiameter, endDiameter, t * t));

            Color c = approachColor;
            if (judge != null)
            {
                double offset = conductor.OffsetFromNearestBeat(conductor.Now - judge.InputLatency, out _);
                double a = System.Math.Abs(offset);
                if (a <= judge.PerfectWindow) c = perfectColor;
                else if (a <= judge.GoodWindow && offset < 0) c = goodColor;
            }
            c.a *= Mathf.Lerp(0.3f, 1f, t);
            ring.color = c;

            // Flash: bright burst right on the beat, fading quickly.
            float f = 1f - secondsSinceBeat / flashDuration;
            bool feedback = Time.time < feedbackUntil;
            flash.enabled = f > 0f || feedback;
            if (flash.enabled)
            {
                Color fc = feedback ? feedbackColor : perfectColor;
                fc.a = feedback ? Mathf.Clamp01((feedbackUntil - Time.time) / 0.25f) * 0.8f : f * 0.7f;
                flash.color = fc;
                float grow = feedback ? 1.3f : Mathf.Lerp(1.25f, 1f, f);
                flash.transform.localScale = WorldToLocal(flashDiameter * grow);
            }
        }

        void ShowFeedback(Judgement j)
        {
            feedbackColor = j.Tier switch
            {
                Accuracy.Perfect => perfectColor,
                Accuracy.Good => goodColor,
                _ => missColor,
            };
            feedbackUntil = Time.time + 0.25f;
        }

        SpriteRenderer CreateChild(string childName, Sprite sprite, int order)
        {
            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        // Keep the ring a fixed world size even if the parent is scaled.
        Vector3 WorldToLocal(float diameter)
        {
            Vector3 s = transform.lossyScale;
            return new Vector3(diameter / Mathf.Max(0.0001f, Mathf.Abs(s.x)), diameter / Mathf.Max(0.0001f, Mathf.Abs(s.y)), 1f);
        }

        void SetVisible(bool visible)
        {
            if (ring != null) ring.enabled = visible;
            if (flash != null) flash.enabled = visible;
        }
    }
}
