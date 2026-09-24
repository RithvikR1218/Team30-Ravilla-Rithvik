using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatTiming
{
    /// <summary>
    /// Stand-alone test bench for the rhythm system (issue #1: "use placeholder values to simulate
    /// the player trying to use abilities"). A square stands in for the player and Space stands in
    /// for Dash. Not meant for the real game scene.
    ///
    /// Controls: Space = Dash, 1 = mute sound, 2 = hide visual cue, 3/4 = BPM down/up, 5 = reset stats.
    /// </summary>
    public class BeatTestHarness : MonoBehaviour
    {
        [SerializeField] BeatConductor conductor;
        [SerializeField] TimingJudge judge;
        [SerializeField] BeatMetronome metronome;

        [Header("Placeholder Dash")]
        [SerializeField] float perfectDashDistance = 4f;
        [SerializeField] float goodDashDistance = 2.5f;
        [SerializeField] float dashDuration = 0.12f;
        [SerializeField] float laneMinX = -6.5f;
        [SerializeField] float laneMaxX = 6.5f;

        Transform player;
        SpriteRenderer playerSprite;
        BeatPulseRing ring;
        Transform icon;

        Vector3 dashFrom, dashTo;
        float dashStart = -1f;
        float missUntil;

        Judgement last;
        bool hasLast;
        int perfects, goods, misses;
        readonly List<double> hitOffsets = new List<double>();

        GUIStyle label, big;

        void Start()
        {
            if (conductor == null) conductor = BeatConductor.Instance;
            if (judge == null) judge = TimingJudge.Instance;
            if (metronome == null) metronome = FindFirstObjectByType<BeatMetronome>();

            player = new GameObject("Player (placeholder)").transform;
            player.position = new Vector3(laneMinX, 0f, 0f);
            playerSprite = player.gameObject.AddComponent<SpriteRenderer>();
            playerSprite.sprite = ProceduralSprites.Square;
            playerSprite.sortingOrder = 60;
            ring = player.gameObject.AddComponent<BeatPulseRing>();

            icon = new GameObject("Dash Icon (placeholder)").transform;
            icon.position = new Vector3(0f, -3.6f, 0f);
            icon.localScale = Vector3.one * 1.1f;
            var iconSprite = icon.gameObject.AddComponent<SpriteRenderer>();
            iconSprite.sprite = ProceduralSprites.Disc;
            icon.gameObject.AddComponent<BeatIconPulse>();

            if (judge != null) judge.OnJudged += OnJudged;
        }

        void OnDestroy()
        {
            if (judge != null) judge.OnJudged -= OnJudged;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.spaceKey.wasPressedThisFrame && judge != null) judge.Judge();
                if (kb.digit1Key.wasPressedThisFrame && metronome != null) metronome.Muted = !metronome.Muted;
                if (kb.digit2Key.wasPressedThisFrame && ring != null) ring.enabled = !ring.enabled;
                if (kb.digit4Key.wasPressedThisFrame && conductor != null) conductor.SetBpm(conductor.Bpm + 5f);
                if (kb.digit3Key.wasPressedThisFrame && conductor != null) conductor.SetBpm(Mathf.Max(30f, conductor.Bpm - 5f));
                if (kb.digit5Key.wasPressedThisFrame) ResetStats();
            }

            AnimatePlayer();
        }

        void OnJudged(Judgement j)
        {
            last = j;
            hasLast = true;

            switch (j.Tier)
            {
                case Accuracy.Perfect: perfects++; hitOffsets.Add(j.Offset); StartDash(perfectDashDistance); break;
                case Accuracy.Good: goods++; hitOffsets.Add(j.Offset); StartDash(goodDashDistance); break;
                default: misses++; missUntil = Time.time + 0.3f; break;
            }
        }

        void StartDash(float distance)
        {
            dashFrom = player.position;
            if (dashFrom.x + distance > laneMaxX) dashFrom.x = laneMinX - distance; // wrap around to the left edge
            dashTo = dashFrom + Vector3.right * distance;
            dashStart = Time.time;
        }

        void AnimatePlayer()
        {
            if (player == null) return;

            if (dashStart >= 0f)
            {
                float t = Mathf.Clamp01((Time.time - dashStart) / dashDuration);
                player.position = Vector3.Lerp(dashFrom, dashTo, 1f - (1f - t) * (1f - t));
                if (t >= 1f) dashStart = -1f;
            }

            bool missing = Time.time < missUntil;
            playerSprite.color = missing ? new Color(1f, 0.3f, 0.3f) : Color.white;
            Vector3 p = player.position;
            p.y = missing ? Mathf.Sin(Time.time * 80f) * 0.08f : 0f;
            player.position = p;
        }

        void ResetStats()
        {
            perfects = goods = misses = 0;
            hitOffsets.Clear();
            hasLast = false;
        }

        void OnGUI()
        {
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 16 };
                big = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            }

            GUILayout.BeginArea(new Rect(12, 10, 520, 260));
            if (conductor != null) GUILayout.Label($"BPM {conductor.Bpm:0}   beat {Mathf.Max(0, (int)conductor.SongBeats)}", label);
            if (judge != null) GUILayout.Label($"Windows: Perfect ±{judge.PerfectWindow * 1000:0}ms  Good ±{judge.GoodWindow * 1000:0}ms  Latency {judge.InputLatency * 1000:0}ms", label);
            GUILayout.Label($"Perfect {perfects}   Good {goods}   Miss {misses}", label);
            if (hitOffsets.Count >= 5)
            {
                double sum = 0;
                foreach (double o in hitOffsets) sum += o;
                double mean = sum / hitOffsets.Count * 1000.0;
                GUILayout.Label($"Average hit offset {mean:+0;-0}ms " + (System.Math.Abs(mean) > 25 ? "(set Input Latency to about this)" : "(well calibrated)"), label);
            }
            GUILayout.Label($"Sound: {(metronome != null && !metronome.Muted ? "on" : "OFF")}   Visual cue: {(ring != null && ring.enabled ? "on" : "OFF")}", label);
            GUILayout.Label("Space = Dash   1 = mute   2 = hide cue   3/4 = BPM -/+   5 = reset", label);
            GUILayout.EndArea();

            if (hasLast)
            {
                Color old = GUI.color;
                GUI.color = last.Tier switch
                {
                    Accuracy.Perfect => new Color(1f, 0.85f, 0.2f),
                    Accuracy.Good => new Color(0.35f, 0.9f, 1f),
                    _ => new Color(1f, 0.35f, 0.35f),
                };
                GUI.Label(new Rect(0, Screen.height * 0.18f, Screen.width, 50), last.ToString(), big);
                GUI.color = old;
            }

            Camera cam = Camera.main;
            if (cam != null && icon != null)
            {
                Vector3 s = cam.WorldToScreenPoint(icon.position);
                GUI.Label(new Rect(s.x - 50, Screen.height - s.y + 28, 100, 24), "DASH", new GUIStyle(label) { alignment = TextAnchor.MiddleCenter });
            }
        }
    }
}
