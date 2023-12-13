using System;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public Sound[] sounds;
    public static AudioManager instance;

    // Effects = 0, Music = 1, Voice = 2
    [SerializeField] float[] volumes = { 1, 1, 1 };

    [SerializeField] float masterVolume = 1f;

    private void Awake() {
        // Singleton
        instance = this;

        foreach (var s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            //s.source.spatialBlend = 1;
        }
    }

    private void Start() {
        Play(SoundNames.backgroundMusic);
    }

    /// <summary>
    /// Plays audio using string as name
    /// </summary>
    /// <param name="name"></param>
    public void Play(string name) {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound name: " + name + " not found in list!");
            return;
        }

        // Get random out of the clips
        int index = UnityEngine.Random.Range(0, s.clips.Count);
        s.source.clip = s.clips[index];

        // randomize pitch
        if (s.randomizePitch)
            s.source.pitch = UnityEngine.Random.Range(s.pitchMin, s.pitchMax);

        // set volume
        s.source.volume = s.volume * masterVolume * volumes[(int)s.soundType];

        // Play audio
        s.source.Play();
    }

    /// <summary>
    /// Pauses audio
    /// </summary>
    /// <param name="name"></param>
    public void Pause(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound name: " + name + " not found in list!");    
            return;
        }
        s.source.Stop();
    }

    /// <summary>
    /// Volume change (Listener Function)
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="soundType"></param>
    public void ChangeVolume(float volume, SoundType soundType) {
        volume /= 100f;
        volumes[(int)soundType] = volume;
    }

    /// <summary>
    /// Master Volume change (Listener Function)
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="soundType"></param>
    public void ChangeMasterVolume(float volume) {
        volume /= 100f;
        masterVolume = volume;
    }
}
