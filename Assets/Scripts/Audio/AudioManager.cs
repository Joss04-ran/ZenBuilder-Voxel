using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float ambienceVolume = 0.4f;

    private AudioSource _sfxSource;
    private AudioSource _ambienceSource;
    private Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
    private Coroutine _fadeCoroutine;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _ambienceSource = gameObject.AddComponent<AudioSource>();
        _ambienceSource.loop = true;
        _ambienceSource.spatialBlend = 0f; // 2D ambience

        LoadClips();
    }
    private void LoadClips()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Music");
        foreach (AudioClip clip in clips)
            _clips[clip.name] = clip;
        Debug.Log($"[AudioManager] Loaded {_clips.Count} clips: {string.Join(", ", _clips.Keys)}");
    }

    public void PlaySFX(string clipName, float volumeScale = 1f)
    {
        if (!_clips.TryGetValue(clipName, out AudioClip clip))
        {
            Debug.LogWarning($"[AudioManager] Clip not found: '{clipName}'");
            return;
        }
        _sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }
    public void PlayAmbience(string clipName, float volumeScale = 1f)
    {
        if (!_clips.TryGetValue(clipName, out AudioClip clip)) return;
        if (_ambienceSource.clip == clip && _ambienceSource.isPlaying) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossFadeAmbience(clip, ambienceVolume * volumeScale));
    }

    public void StopAmbience(float fadeDuration = 1.5f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOut(fadeDuration));
    }
    private IEnumerator CrossFadeAmbience(AudioClip newClip, float targetVolume)
    {
        float duration = 2f;

        // Fade out current
        float startVol = _ambienceSource.volume;
        for (float t = 0; t < duration * 0.5f; t += Time.deltaTime)
        {
            _ambienceSource.volume = Mathf.Lerp(startVol, 0f, t / (duration * 0.5f));
            yield return null;
        }

        _ambienceSource.clip = newClip;
        _ambienceSource.volume = 0f;
        _ambienceSource.Play();

        // Fade in new
        for (float t = 0; t < duration * 0.5f; t += Time.deltaTime)
        {
            _ambienceSource.volume = Mathf.Lerp(0f, targetVolume, t / (duration * 0.5f));
            yield return null;
        }
        _ambienceSource.volume = targetVolume;
    }

    private IEnumerator FadeOut(float duration)
    {
        float start = _ambienceSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            _ambienceSource.volume = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        _ambienceSource.Stop();
    }
}