using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sounds")]
    [SerializeField] private SoundLibrary library;

    [Header("Mixer Configuration")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixerGroup uiMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Pool Sizes")]
    [SerializeField] private int uiPoolSize = 5;
    [SerializeField] private int combatPoolSize = 20;

    // CLEANED UP: Only keeping the distance parameters we need for custom math calculations
    [Header("Distance Falloff Settings")]
    [SerializeField] private float combatMinDistance = 5f;
    [SerializeField] private float combatMaxDistance = 35f;
    [SerializeField] private int maxConcurrentSameSounds = 3;

    [Header("Filter Settings")]
    [SerializeField] private float combatLowPassCutoff = 5500f;

    private List<AudioSource> uiPool = new List<AudioSource>();
    private List<AudioSource> combatPool = new List<AudioSource>();

    private Dictionary<string, SoundEffect> soundDictionary = new Dictionary<string, SoundEffect>();

    private Transform uiPoolContainer;
    private Transform combatPoolContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDictionary();
        InitializePools();
    }

    private void InitializeDictionary()
    {
        if (library == null || library.sounds == null) return;
        foreach (var sound in library.sounds) {
            if (!string.IsNullOrEmpty(sound.name) && !soundDictionary.ContainsKey(sound.name)) {
                soundDictionary.Add(sound.name, sound);
            }
        }
    }

    private void InitializePools()
    {
        GameObject uiContainerObj = new GameObject("UI_Audio_Pool");
        uiContainerObj.transform.SetParent(transform);
        uiPoolContainer = uiContainerObj.transform;

        for (int i = 0; i < uiPoolSize; i++) {
            AudioSource source = uiContainerObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            if (uiMixerGroup != null) source.outputAudioMixerGroup = uiMixerGroup;
            uiPool.Add(source);
        }

        GameObject combatContainerObj = new GameObject("Combat_Audio_Pool");
        combatContainerObj.transform.SetParent(transform);
        combatPoolContainer = combatContainerObj.transform;

        for (int i = 0; i < combatPoolSize; i++) {
            AudioSource source = combatContainerObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; 
            if (sfxMixerGroup != null) source.outputAudioMixerGroup = sfxMixerGroup;
            combatPool.Add(source);
        }
    }

    private SoundEffect GetSound(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundEffect sound)) return sound;
        Debug.LogWarning($"Sound sequence name '{soundName}' not found!");
        return null;
    }

    private AudioSource GetAvailableSource(List<AudioSource> pool, bool isUI)
    {
        for (int i = 0; i < pool.Count; i++) {
            if (!pool[i].isPlaying) return pool[i];
        }

        GameObject tempObj = new GameObject("Dynamic_AudioSource_Node");
        tempObj.transform.SetParent(isUI ? uiPoolContainer : combatPoolContainer);

        AudioSource source = tempObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; 

        if (isUI) {
            if (uiMixerGroup != null) source.outputAudioMixerGroup = uiMixerGroup;
        }
        else {
            if (sfxMixerGroup != null) source.outputAudioMixerGroup = sfxMixerGroup;
        }

        pool.Add(source);
        return source;
    }

    public void PlayUISound(string soundName)
    {
        SoundEffect s = GetSound(soundName);
        if (s == null) return;

        AudioSource source = GetAvailableSource(uiPool, isUI: true);
        ConfigureSourceAndPlay(source, s, isUI: true);
    }

    public void PlaySound3D(string soundName, Vector3 position)
    {
        SoundEffect s = GetSound(soundName);
        if (s == null) return;

        int activeCount = 0;
        for (int i = 0; i < combatPool.Count; i++) {
            if (combatPool[i].isPlaying && combatPool[i].clip == s.clip) {
                activeCount++;
            }
        }
        if (activeCount >= maxConcurrentSameSounds) return;

        AudioSource source = GetAvailableSource(combatPool, isUI: false);

        Vector3 focalPoint = GetScreenCenterOnGround();
        float distance = Vector3.Distance(position, focalPoint);

        float falloffFactor = 1f;
        if (distance > combatMinDistance) {
            float range = combatMaxDistance - combatMinDistance;
            falloffFactor = Mathf.Clamp01(1f - ((distance - combatMinDistance) / range));
        }

        source.spatialBlend = 0f;

        ConfigureSourceAndPlay(source, s, isUI: false);

        source.volume = s.volume * falloffFactor;
    }

    private void ConfigureSourceAndPlay(AudioSource source, SoundEffect s, bool isUI)
    {
        source.clip = s.clip;
        source.volume = s.volume;

        if (isUI) {
            source.pitch = 1f;
            var filter = source.GetComponent<AudioLowPassFilter>();
            if (filter != null) filter.enabled = false;
        }
        else {
            source.pitch = 1f + Random.Range(-s.pitchRandomness, s.pitchRandomness);

            var filter = source.GetComponent<AudioLowPassFilter>();
            if (s.bypassLowPass) {
                if (filter != null) filter.enabled = false;
            }
            else {
                if (filter == null) filter = source.gameObject.AddComponent<AudioLowPassFilter>();
                filter.enabled = true;
                filter.cutoffFrequency = combatLowPassCutoff;
            }
        }
        source.Play();
    }

    private Vector3 GetScreenCenterOnGround()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        groundPlane.Raycast(ray, out float enterDistance);
        return ray.GetPoint(enterDistance);
    }

    public AudioMixerGroup GetCombatMixerGroup() => sfxMixerGroup;
    public SoundEffect GetSoundData(string soundName) => GetSound(soundName);

    public void SetMasterVolume(float sliderValue) => ConvertAndSetVolume("masterVol", sliderValue);
    public void SetUIVolume(float sliderValue) => ConvertAndSetVolume("uiVol", sliderValue);
    public void SetCombatVolume(float sliderValue) => ConvertAndSetVolume("combatVol", sliderValue);

    private void ConvertAndSetVolume(string parameterName, float sliderValue)
    {
        if (masterMixer == null) return;
        if (sliderValue <= 0.0001f) {
            masterMixer.SetFloat(parameterName, -80f);
            return;
        }
        float decibelValue = Mathf.Log10(sliderValue) * 20f;
        masterMixer.SetFloat(parameterName, decibelValue);
    }
}