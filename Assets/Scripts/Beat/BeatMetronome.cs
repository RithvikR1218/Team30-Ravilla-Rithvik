using UnityEngine;

namespace BeatTiming
{
    /// <summary>
    /// Plays a short tick on every beat, scheduled on the audio clock so it lands exactly on time.
    /// Sound reinforces the visual cue; the game must still be playable with this muted.
    /// Generates its own click sound if no clip is assigned.
    /// </summary>
    public class BeatMetronome : MonoBehaviour
    {
        [SerializeField] BeatConductor conductor;
        [SerializeField] AudioClip tick;
        [SerializeField] AudioClip accentTick;
        [Tooltip("Every Nth beat uses the accent tick (0 = never).")]
        [SerializeField, Min(0)] int accentEvery = 4;
        [SerializeField, Range(0f, 1f)] float volume = 0.6f;
        [SerializeField] bool muted;

        // Two sources, alternated, so a scheduled tick never cuts off the previous one.
        AudioSource[] sources;
        int nextSource;
        int nextScheduledBeat;
        const double LookAhead = 0.1;

        public bool Muted
        {
            get => muted;
            set => muted = value;
        }

        void Awake()
        {
            if (conductor == null) conductor = GetComponent<BeatConductor>();
            if (conductor == null) conductor = BeatConductor.Instance;
            if (tick == null) tick = MakeClick("Tick", 1000f);
            if (accentTick == null) accentTick = MakeClick("AccentTick", 1500f);

            sources = new AudioSource[2];
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = gameObject.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
            }
        }

        void Update()
        {
            if (conductor == null || !conductor.IsRunning) return;

            // Skip any beats that are already in the past (e.g. after a hitch or restart).
            int firstFuture = Mathf.Max(0, Mathf.CeilToInt((float)conductor.SongBeats));
            if (nextScheduledBeat < firstFuture) nextScheduledBeat = firstFuture;

            double t = conductor.DspTimeOfBeat(nextScheduledBeat);
            if (t - AudioSettings.dspTime > LookAhead) return;

            if (!muted)
            {
                bool accent = accentEvery > 0 && nextScheduledBeat % accentEvery == 0;
                AudioSource s = sources[nextSource];
                nextSource = (nextSource + 1) % sources.Length;
                s.clip = accent ? accentTick : tick;
                s.volume = volume;
                s.PlayScheduled(t);
            }
            nextScheduledBeat++;
        }

        static AudioClip MakeClick(string name, float frequency)
        {
            int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
            int length = rate / 20; // 50ms
            var data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Exp(-t * 60f);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
            }
            AudioClip clip = AudioClip.Create(name, length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
