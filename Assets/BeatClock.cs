using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    [Header("References")]
    public BeatMapData beatMap;        // drag BeatMapData asset here
    public AudioSource audioSource;    // drag AudioSource component here
    [Range(0.5f, 3f)] public float beatApproachTime = 1.5f;

    // ── Private state ────────────────────────────────────────
    private int _nextBeatIndex = 0;   // which beat we're watching for next
    private int _nextApproachIndex = 0;
    private double _songStartDsp;        // dspTime when song was started
    private bool _isPlaying = false;

    // ── Public accessors ─────────────────────────────────────
    // Other scripts use this to ask "what time is it in song time?"
    public double SongTime => AudioSettings.dspTime - _songStartDsp;
    public double SongStartDsp => _songStartDsp;

    // ── Lifecycle ────────────────────────────────────────────
    IEnumerator Start()
    {
        if (beatMap != null && beatMap.song != null &&
            (beatMap.beats == null || beatMap.beats.Length == 0))
        {
            yield return GenerateBeatMap();
        }

        StartSong();
    }

    IEnumerator GenerateBeatMap()
    {
        AudioClip clip = beatMap.song;
        if (clip.loadState == AudioDataLoadState.Unloaded)
        {
            clip.LoadAudioData();
        }

        while (clip.loadState == AudioDataLoadState.Loading)
        {
            yield return null;
        }

        if (clip.loadState != AudioDataLoadState.Loaded)
        {
            Debug.LogError($"Could not load audio data for beat detection: {clip.name}", this);
            yield break;
        }

        int channels = clip.channels;
        float[] samples = new float[clip.samples * channels];
        if (!clip.GetData(samples, 0))
        {
            Debug.LogError($"Could not read audio samples for beat detection: {clip.name}", this);
            yield break;
        }

        const int windowSize = 1024;
        const int historySize = 24;
        int windowCount = samples.Length / (windowSize * channels);
        float[] bassEnergy = new float[windowCount];

        for (int window = 0; window < windowCount; window++)
        {
            float[] real = new float[windowSize];
            float[] imag = new float[windowSize];

            int start = window * windowSize * channels;

            for (int sample = 0; sample < windowSize; sample++)
            {
                float mono = 0f;

                for (int channel = 0; channel < channels; channel++)
                {
                    mono += samples[start + sample * channels + channel];
                }

                mono /= channels;

                real[sample] = mono;
                imag[sample] = 0f;
            }

            FFT(real, imag);

            float bass = 0f;

            for (int bin = 1; bin < 20; bin++)
            {
                bass += Mathf.Sqrt(
                    real[bin] * real[bin] +
                    imag[bin] * imag[bin]);
            }

            bassEnergy[window] = bass;
        }

        List<float> detectedBeats = new List<float>();
        float minimumSpacing = Mathf.Max(0.1f, beatMap.minimumBeatSpacing);
        float lastBeatTime = -minimumSpacing;

        for (int window = historySize; window < windowCount - 1; window++)
        {
            float average = 0f;
            for (int history = window - historySize; history < window; history++)
            {
                average += bassEnergy[history];
            }
            average /= historySize;

            bool isPeak = bassEnergy[window] > bassEnergy[window - 1] &&
               bassEnergy[window] >= bassEnergy[window + 1];
            bool isOnset = bassEnergy[window] > average * beatMap.onsetSensitivity;
            float beatTime = window * windowSize / (float)clip.frequency;

            if (isPeak && isOnset && beatTime - lastBeatTime >= minimumSpacing)
            {
                detectedBeats.Add(beatTime);
                lastBeatTime = beatTime;
            }
        }

        if (detectedBeats.Count < 4)
        {
            float secondsPerBeat = 60f / Mathf.Max(1f, beatMap.fallbackBpm);
            for (float beatTime = 0f; beatTime < clip.length; beatTime += secondsPerBeat)
            {
                detectedBeats.Add(beatTime);
            }

            Debug.LogWarning(
                $"Waveform detection found too few beats. Using a {beatMap.fallbackBpm:0.#} BPM grid.",
                this
            );
        }

        beatMap.beats = detectedBeats.ToArray();
        Debug.Log($"Generated {beatMap.beats.Length} beats for {clip.name}.", this);
    }

    public void StartSong()
    {
        if (beatMap == null || audioSource == null)
        {
            Debug.LogError("BeatClock requires a BeatMapData asset and an AudioSource.", this);
            enabled = false;
            return;
        }

        // Capture the exact dspTime of song start
        // Schedule playback 0.1s in future for precision
        double startDelay = 0.1;
        _songStartDsp = AudioSettings.dspTime + startDelay;
        _nextBeatIndex = 0;
        _nextApproachIndex = 0;
        _isPlaying = true;

        audioSource.clip = beatMap.song;
        if (beatMap.song != null)
        {
            audioSource.PlayScheduled(_songStartDsp);   // precision scheduled start
        }
    }

    // ── Update: fire beat events at the right moment ─────────
    void Update()
    {
        if (!_isPlaying) return;
        if (beatMap == null || beatMap.beats == null || beatMap.beats.Length == 0) return;
        // Current position in the song (in seconds)
        double currentSongTime = SongTime;

        // Check if we've passed the next scheduled beat
        // TEACHING POINT: we fire SLIGHTLY BEFORE the beat
        // so visual feedback (beat ring flash) appears on time
        // The player sees it and THEN hits spacebar
        float lookAheadMs = 0f;  // set to 50-100ms for visual cue lead

        while (_nextApproachIndex < beatMap.beats.Length &&
               currentSongTime >= beatMap.beats[_nextApproachIndex] - beatApproachTime)
        {
            GameEvents.OnBeatApproaching?.Invoke(beatMap.beats[_nextApproachIndex]);
            _nextApproachIndex++;
        }

        while (_nextBeatIndex < beatMap.beats.Length &&
               currentSongTime >= beatMap.beats[_nextBeatIndex] - (lookAheadMs / 1000.0))
        {
            // Fire the beat event — HitDetector and HUD both listen
            GameEvents.OnBeat?.Invoke(beatMap.beats[_nextBeatIndex]);
            _nextBeatIndex++;
        }
    }

    // ── Helper: absolute dspTime of a beat ───────────────────
    // HitDetector uses this to compare with player's press time
    public double BeatToDspTime(float beatSongTime)
    {
        return _songStartDsp + beatSongTime;
    }

    private void FFT(float[] real, float[] imag)
    {
        int n = real.Length;

        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;

            while ((j & bit) != 0)
            {
                j ^= bit;
                bit >>= 1;
            }

            j ^= bit;

            if (i < j)
            {
                (real[i], real[j]) = (real[j], real[i]);
                (imag[i], imag[j]) = (imag[j], imag[i]);
            }
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            float angle = -2f * Mathf.PI / len;
            float wlenR = Mathf.Cos(angle);
            float wlenI = Mathf.Sin(angle);

            for (int i = 0; i < n; i += len)
            {
                float wr = 1f;
                float wi = 0f;

                for (int j = 0; j < len / 2; j++)
                {
                    int u = i + j;
                    int v = i + j + len / 2;

                    float vr = real[v] * wr - imag[v] * wi;
                    float vi = real[v] * wi + imag[v] * wr;

                    real[v] = real[u] - vr;
                    imag[v] = imag[u] - vi;

                    real[u] += vr;
                    imag[u] += vi;

                    float nextWr = wr * wlenR - wi * wlenI;
                    wi = wr * wlenI + wi * wlenR;
                    wr = nextWr;
                }
            }
        }
    }
}
