using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public SoundEffect[] sounds;
}

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.5f;
    [Range(0f, 0.3f)] public float pitchRandomness = 0.05f;
    public bool bypassLowPass = false;
}
