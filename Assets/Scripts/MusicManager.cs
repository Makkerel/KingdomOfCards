using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Mixer Routing")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    private AudioSource musicSourceA;
    private AudioSource musicSourceB;

    private bool isSourceAPlaying = true;
    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSources();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1) {
            FadeOutMusic(2f);
        }
    }

    private void InitializeSources()
    {
        GameObject musicContainer = new GameObject("Music_Channels");
        musicContainer.transform.SetParent(transform);

        musicSourceA = musicContainer.AddComponent<AudioSource>();
        musicSourceB = musicContainer.AddComponent<AudioSource>();

        musicSourceA.playOnAwake = false;
        musicSourceA.loop = true;
        musicSourceA.spatialBlend = 0f;
        musicSourceA.outputAudioMixerGroup = musicMixerGroup;

        musicSourceB.playOnAwake = false;
        musicSourceB.loop = true;
        musicSourceB.spatialBlend = 0f;
        musicSourceB.outputAudioMixerGroup = musicMixerGroup;
    }

    public void ChangeMusic(string trackName, float fadeDuration = 1.5f)
    {
        SoundEffect trackData = AudioManager.Instance.GetSoundData(trackName);
        AudioClip newClip = trackData.clip;
        AudioSource activeSource = isSourceAPlaying ? musicSourceA : musicSourceB;
        if (activeSource.clip == newClip && activeSource.isPlaying) return;

        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        AudioSource targetSource = isSourceAPlaying ? musicSourceB : musicSourceA;

        isSourceAPlaying = !isSourceAPlaying;

        targetSource.clip = newClip;
        musicFadeCoroutine = StartCoroutine(CrossfadeMusicRoutine(activeSource, targetSource, trackData.volume, fadeDuration));
    }
    public void FadeOutMusic(float fadeDuration = 1.5f)
    {
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);

        AudioSource activeSource = isSourceAPlaying ? musicSourceA : musicSourceB;
        if (!activeSource.isPlaying) return;

        musicFadeCoroutine = StartCoroutine(FadeOutRoutine(activeSource, fadeDuration));
    }

    private IEnumerator CrossfadeMusicRoutine(AudioSource activeSource, AudioSource targetSource, float targetMaxVolume, float duration)
    {
        targetSource.volume = 0f;
        targetSource.Play();

        float startVolume = activeSource.volume;
        float timer = 0f;

        while (timer < duration) {
            timer += Time.deltaTime;
            float progress = timer / duration;

            activeSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            targetSource.volume = Mathf.Lerp(0f, targetMaxVolume, progress);
            yield return null;
        }

        activeSource.Stop();
    }

    private IEnumerator FadeOutRoutine(AudioSource activeSource, float duration)
    {
        float startVolume = activeSource.volume;
        float timer = 0f;

        while (timer < duration) {
            timer += Time.deltaTime;
            float progress = timer / duration;

            activeSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = 0f;
    }
}