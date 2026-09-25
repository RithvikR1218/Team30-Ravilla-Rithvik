using System;
using UnityEngine;

namespace BeatTiming
{
    /// <summary>
    /// The single source of truth for "where are we in the music".
    /// Time comes from the audio hardware clock (AudioSettings.dspTime), not frame time,
    /// so the beat never drifts away from the audio.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class BeatConductor : MonoBehaviour
    {
        public static BeatConductor Instance { get; private set; }

        [Header("Tempo")]
        [SerializeField, Min(1f)] float bpm = 90f;

        [Header("Music (optional - the clock runs without it)")]
        [SerializeField] AudioSource music;
        [Tooltip("Seconds from the start of the music clip to its first beat.")]
        [SerializeField] float firstBeatOffset = 0f;

        [Header("Start")]
        [SerializeField] bool playOnStart = true;
        [Tooltip("Delay before beat 0 so the audio can be scheduled precisely.")]
        [SerializeField, Min(0.1f)] float startDelay = 1f;

        /// <summary>Fired once per beat with the beat index (0, 1, 2, ...).</summary>
        public event Action<int> OnBeat;

        public float Bpm => bpm;
        public double SecondsPerBeat => 60.0 / bpm;
        public bool IsRunning { get; private set; }

        /// <summary>dspTime at which beat 0 happens.</summary>
        public double BeatZeroDspTime { get; private set; }

        /// <summary>Smoothed audio-clock time. Use this instead of AudioSettings.dspTime for visuals/input.</summary>
        public double Now => smoothedDsp;

        /// <summary>Seconds since beat 0 (negative during the start delay).</summary>
        public double SongTime => IsRunning ? smoothedDsp - BeatZeroDspTime : 0.0;

        /// <summary>Position in beats, e.g. 4.25 = a quarter of the way from beat 4 to beat 5.</summary>
        public double SongBeats => SongTime / SecondsPerBeat;

        /// <summary>0..1 progress through the current beat (0 = on the beat, 0.99 = just before the next one).</summary>
        public float BeatPhase
        {
            get
            {
                double b = SongBeats;
                return b < 0 ? 0f : (float)(b - Math.Floor(b));
            }
        }

        public int NearestBeat => (int)Math.Round(SongBeats);
        public double DspTimeOfBeat(int beat) => BeatZeroDspTime + beat * SecondsPerBeat;

        /// <summary>Signed seconds from the nearest beat to the given clock time (negative = early).</summary>
        public double OffsetFromNearestBeat(double dspTime, out int nearestBeat)
        {
            double beats = (dspTime - BeatZeroDspTime) / SecondsPerBeat;
            nearestBeat = (int)Math.Round(beats);
            return dspTime - DspTimeOfBeat(nearestBeat);
        }

        double smoothedDsp;
        double lastRawDsp;
        int lastBeatFired = -1;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More than one BeatConductor in the scene; using the first one.", this);
                return;
            }
            Instance = this;
            smoothedDsp = lastRawDsp = AudioSettings.dspTime;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            double now = AudioSettings.dspTime;
            double musicStart = now + startDelay;
            BeatZeroDspTime = musicStart + firstBeatOffset;
            if (music != null && music.clip != null) music.PlayScheduled(musicStart);
            lastBeatFired = -1;
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
            if (music != null) music.Stop();
        }

        public void SetBpm(float newBpm)
        {
            if (newBpm <= 0f) return;
            // Keep the current beat position continuous when the tempo changes.
            double beats = SongBeats;
            bpm = newBpm;
            if (IsRunning) BeatZeroDspTime = smoothedDsp - beats * SecondsPerBeat;
        }

        void Update()
        {
            // dspTime only advances once per audio buffer, so it "steps".
            // Extrapolate with frame time between steps to keep visuals smooth.
            double raw = AudioSettings.dspTime;
            if (raw != lastRawDsp)
            {
                lastRawDsp = raw;
                smoothedDsp = raw;
            }
            else
            {
                smoothedDsp += Time.unscaledDeltaTime;
            }

            if (!IsRunning) return;

            int current = (int)Math.Floor(SongBeats);
            while (lastBeatFired < current)
            {
                lastBeatFired++;
                if (lastBeatFired >= 0) OnBeat?.Invoke(lastBeatFired);
            }
        }
    }
}
