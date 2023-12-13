using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound {
    public string name;

    public SoundType soundType;

    public List<AudioClip> clips = new List<AudioClip>();

    [Range(0, 3f)]
    public float volume = 0.5f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;

    public bool randomizePitch;

    [Range(0.1f, 3f)]
    public float pitchMin = 0;
    [Range(0.1f, 3f)]
    public float pitchMax = 3;

    [HideInInspector]
    public AudioSource source;
}

public enum SoundType {
    Effects = 0,
    Music,
    Voice
}

public class SoundNames {
    // Effects
    public static string cardPress = "card press";
    public static string cardHover = "card hover";
    public static string cardPickup = "card pickup";
    public static string attack = "attack";
    public static string burn = "burn";
    public static string error = "error";
    public static string nextRound = "next round";

    // Music
    public static string backgroundMusic = "background music";
}
