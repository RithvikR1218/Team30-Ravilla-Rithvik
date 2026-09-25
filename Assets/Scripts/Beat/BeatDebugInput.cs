using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatTiming
{
    /// <summary>
    /// Temporary stand-in until the real Dash (#2) exists: Space calls the judge and the result is
    /// shown on screen, so the beat cue can be tested on the real player in the real scene.
    /// Remove this once Dash calls TimingJudge.Instance.Judge() itself.
    ///
    /// Controls: Space = judge a press, 1 = mute sound, 2 = hide ring, 3/4 = BPM down/up.
    /// Number keys are used so the test controls never clash with movement keys.
    /// </summary>
    public class BeatDebugInput : MonoBehaviour
    {
        [SerializeField] bool showOverlay = true;

        TimingJudge judge;
        BeatMetronome metronome;
        Judgement last;
        bool hasLast;
        int perfects, goods, misses;
        GUIStyle label, big;

        void Start()
        {
            judge = TimingJudge.Instance;
            metronome = FindFirstObjectByType<BeatMetronome>();
            if (judge != null) judge.OnJudged += OnJudged;
        }

        void OnDestroy()
        {
            if (judge != null) judge.OnJudged -= OnJudged;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.spaceKey.wasPressedThisFrame && judge != null) judge.Judge();
            if (kb.digit1Key.wasPressedThisFrame && metronome != null) metronome.Muted = !metronome.Muted;
            if (kb.digit2Key.wasPressedThisFrame)
                foreach (var ring in FindObjectsByType<BeatPulseRing>(FindObjectsSortMode.None)) ring.enabled = !ring.enabled;
            var conductor = BeatConductor.Instance;
            if (conductor != null && kb.digit4Key.wasPressedThisFrame) conductor.SetBpm(conductor.Bpm + 5f);
            if (conductor != null && kb.digit3Key.wasPressedThisFrame) conductor.SetBpm(Mathf.Max(30f, conductor.Bpm - 5f));
        }

        void OnJudged(Judgement j)
        {
            last = j;
            hasLast = true;
            if (j.Tier == Accuracy.Perfect) perfects++;
            else if (j.Tier == Accuracy.Good) goods++;
            else misses++;
        }

        void OnGUI()
        {
            if (!showOverlay) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 16 };
                big = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            }

            var conductor = BeatConductor.Instance;
            GUILayout.BeginArea(new Rect(12, 10, 520, 120));
            if (conductor != null) GUILayout.Label($"BPM {conductor.Bpm:0}   Perfect {perfects}  Good {goods}  Miss {misses}", label);
            GUILayout.Label($"Sound {(metronome != null && !metronome.Muted ? "on" : "OFF")}   [Space] press  [1] mute  [2] hide ring  [3/4] BPM -/+", label);
            GUILayout.EndArea();

            if (!hasLast) return;
            Color old = GUI.color;
            GUI.color = last.Tier switch
            {
                Accuracy.Perfect => new Color(1f, 0.85f, 0.2f),
                Accuracy.Good => new Color(0.35f, 0.9f, 1f),
                _ => new Color(1f, 0.35f, 0.35f),
            };
            GUI.Label(new Rect(0, Screen.height * 0.15f, Screen.width, 44), last.ToString(), big);
            GUI.color = old;
        }
    }
}
