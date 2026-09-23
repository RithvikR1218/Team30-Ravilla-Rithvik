using System;
using UnityEngine;

namespace BeatTiming
{
    public enum Accuracy { Perfect, Good, Miss }

    public readonly struct Judgement
    {
        public readonly Accuracy Tier;
        /// <summary>Signed seconds from the beat. Negative = early, positive = late.</summary>
        public readonly double Offset;
        public readonly int Beat;

        public Judgement(Accuracy tier, double offset, int beat)
        {
            Tier = tier;
            Offset = offset;
            Beat = beat;
        }

        public bool IsHit => Tier != Accuracy.Miss;
        public int OffsetMs => (int)Math.Round(Offset * 1000.0);

        public override string ToString() =>
            Tier == Accuracy.Miss && Beat < 0 ? "MISS" : $"{Tier.ToString().ToUpper()} {(OffsetMs >= 0 ? "+" : "")}{OffsetMs}ms";
    }

    /// <summary>
    /// Turns "the player pressed Dash now" into Perfect / Good / Miss.
    /// Gameplay code calls Judge() when the ability is pressed and reacts to the result
    /// (or listens to OnJudged).
    /// </summary>
    public class TimingJudge : MonoBehaviour
    {
        public static TimingJudge Instance { get; private set; }

        [SerializeField] BeatConductor conductor;

        [Header("Windows (seconds either side of the beat)")]
        [SerializeField, Min(0f)] float perfectWindow = 0.10f;
        [SerializeField, Min(0f)] float goodWindow = 0.25f;

        [Header("Calibration")]
        [Tooltip("Subtracted from every press. If testers are consistently late by ~40ms, set this to 0.04.")]
        [SerializeField] float inputLatency = 0f;

        [Tooltip("Only one judged press per beat, so mashing can't hit every beat.")]
        [SerializeField] bool onePressPerBeat = true;

        /// <summary>Fired for every judged press.</summary>
        public event Action<Judgement> OnJudged;

        public float PerfectWindow { get => perfectWindow; set => perfectWindow = Mathf.Max(0f, value); }
        public float GoodWindow { get => goodWindow; set => goodWindow = Mathf.Max(0f, value); }
        public float InputLatency { get => inputLatency; set => inputLatency = value; }
        public BeatConductor Conductor => conductor;

        int lastJudgedBeat = int.MinValue;

        void Awake()
        {
            if (Instance == null) Instance = this;
            if (conductor == null) conductor = GetComponent<BeatConductor>();
            if (conductor == null) conductor = BeatConductor.Instance;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Judge a press happening right now.</summary>
        public Judgement Judge()
        {
            if (conductor == null || !conductor.IsRunning)
                return Emit(new Judgement(Accuracy.Miss, 0, -1));

            double offset = conductor.OffsetFromNearestBeat(conductor.Now - inputLatency, out int beat);
            Accuracy tier = Classify(offset, perfectWindow, goodWindow);

            if (onePressPerBeat && tier != Accuracy.Miss)
            {
                if (beat == lastJudgedBeat) tier = Accuracy.Miss;
                else lastJudgedBeat = beat;
            }

            return Emit(new Judgement(tier, offset, beat));
        }

        /// <summary>True while a press right now would score at least Good. Handy for UI.</summary>
        public bool InGoodWindow()
        {
            if (conductor == null || !conductor.IsRunning) return false;
            double offset = conductor.OffsetFromNearestBeat(conductor.Now - inputLatency, out _);
            return Math.Abs(offset) <= goodWindow;
        }

        public static Accuracy Classify(double offsetSeconds, float perfect, float good)
        {
            double a = Math.Abs(offsetSeconds);
            if (a <= perfect) return Accuracy.Perfect;
            if (a <= good) return Accuracy.Good;
            return Accuracy.Miss;
        }

        Judgement Emit(Judgement j)
        {
            OnJudged?.Invoke(j);
            return j;
        }
    }
}
