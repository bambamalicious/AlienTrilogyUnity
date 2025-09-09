using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Header("ALT Data")]
    public string installLocation;
    [Header("Original Data")]
    public List<Texture2D> imgData = new();
    public List<AudioClip> sfxList = new();
    public List<string> musicList = new();
    [Header("Options")]
    [Header("Sound Options")]
    [Range(0, 1)]
    public float soundVolume;
    [Range(0, 1)]
    public float musicVolume;
    public bool spatialAudio;
    [Header("Video Options")]
    public Resolution resolution;
    public bool cameraSway;
    [Header("Gameplay Options")]
    [Range(0, 2)]
    public int difficulty;
    public bool enchancedMode;



    public static DataManager data;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(data == null)
        {
            data = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
