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
    [Range(0, 100)]
    public int soundVolume;
    [Range(0, 100)]
    public int musicVolume;
    [Range(0, 2)]
    public int difficulty;
    public Vector2 resolution;
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
