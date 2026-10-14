using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    // Procedural sounds so the game has audio before any real assets exist.
    public class ToneSynth : MonoBehaviour
    {
        const int SampleRate = 44100;

        AudioSource source;
        readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        Coroutine melody;

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            if (!MainCameraHasListener()) gameObject.AddComponent<AudioListener>();
        }

        static bool MainCameraHasListener()
        {
            var cam = Camera.main;
            return cam != null && cam.GetComponent<AudioListener>() != null;
        }

        public void PlayNote(float frequency, float duration, float volume = 0.5f)
        {
            string key = $"note{frequency:F1}_{duration:F2}";
            if (!cache.TryGetValue(key, out var clip))
            {
                clip = Generate(key, duration, (t, i) =>
                {
                    float env = Mathf.Exp(-t * 4f / duration) * Mathf.Min(1f, t * 200f);
                    float phase = 2f * Mathf.PI * frequency * t;
                    return env * (Mathf.Sin(phase) * 0.7f + Mathf.Sin(phase * 2f) * 0.2f + Mathf.Sin(phase * 3f) * 0.1f);
                });
                cache[key] = clip;
            }
            source.PlayOneShot(clip, volume);
        }

        public void PlaySweep(Era to)
        {
            string key = "sweep" + to;
            if (!cache.TryGetValue(key, out var clip))
            {
                float from = to == Era.Future ? 300f : 900f;
                float end = to == Era.Future ? 900f : 300f;
                const float duration = 0.35f;
                float phase = 0f;
                clip = Generate(key, duration, (t, i) =>
                {
                    float f = Mathf.Lerp(from, end, t / duration);
                    phase += 2f * Mathf.PI * f / SampleRate;
                    return Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * t / duration) * 0.6f;
                });
                cache[key] = clip;
            }
            source.PlayOneShot(clip, 0.35f);
        }

        public void PlayBlocked() => PlayNote(110f, 0.25f, 0.4f);

        // Echo: a quick falling glide, like tape running backwards.
        public void PlayRewind()
        {
            const string key = "rewind";
            if (!cache.TryGetValue(key, out var clip))
            {
                const float duration = 0.5f;
                float phase = 0f;
                clip = Generate(key, duration, (t, i) =>
                {
                    float f = Mathf.Lerp(1400f, 200f, Mathf.Sqrt(t / duration));
                    phase += 2f * Mathf.PI * f / SampleRate;
                    float wobble = 0.6f + 0.4f * Mathf.Sin(t * 60f);
                    return Mathf.Sin(phase) * wobble * (1f - t / duration) * 0.5f;
                });
                cache[key] = clip;
            }
            source.PlayOneShot(clip, 0.4f);
        }

        // Fermata: a held open fifth that fades out.
        public void PlayFermata()
        {
            const string key = "fermata";
            if (!cache.TryGetValue(key, out var clip))
            {
                const float duration = 1.6f;
                clip = Generate(key, duration, (t, i) =>
                {
                    float env = Mathf.Min(1f, t * 20f) * Mathf.Exp(-t * 1.5f);
                    float a = Mathf.Sin(2f * Mathf.PI * 261.6f * t);
                    float b = Mathf.Sin(2f * Mathf.PI * 392f * t);
                    return env * (a * 0.5f + b * 0.35f);
                });
                cache[key] = clip;
            }
            source.PlayOneShot(clip, 0.4f);
        }

        public void PlayMelody(IEnumerable<int> noteIndices)
        {
            var notes = new List<int>(noteIndices);
            notes.Sort();
            if (melody != null) StopCoroutine(melody);
            melody = StartCoroutine(PlaySequence(notes));
        }

        IEnumerator PlaySequence(List<int> notes)
        {
            yield return new WaitForSeconds(0.3f);
            foreach (int n in notes)
            {
                PlayNote(LevelData.MelodyNote(n), 0.9f);
                yield return new WaitForSeconds(0.35f);
            }
        }

        static AudioClip Generate(string name, float duration, System.Func<float, int, float> sample)
        {
            int length = Mathf.CeilToInt(duration * SampleRate);
            var data = new float[length];
            for (int i = 0; i < length; i++) data[i] = sample(i / (float)SampleRate, i);
            var clip = AudioClip.Create(name, length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
