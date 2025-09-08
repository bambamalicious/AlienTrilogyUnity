using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FolderFinder : MonoBehaviour
{

    public string path;
    private List<string> previousPath = new();

    public DriveInfo[] drives;
    public List<string> directories = new();
    public GameObject buttonTemplate;
    public RectTransform folderPanel;
    public GameObject submitButton;

    private List<GameObject> spawnedButtons = new();

    public static FolderFinder finder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(finder == null)
        {
            finder = this;
        }
        else { Destroy(gameObject);}
        drives =  DriveInfo.GetDrives();
        GetDrives();
        Invoke("EntryText", 0.3f);
    }

    void EntryText()
    {
        FlairText.flair.TypeText("Searching For MU / TH / UR 9000 Directory");
    }

    public void OnClick(Text text)
    {
        previousPath.Add(path);
        path = text.text;
        GetDirectories();
    }


    public void PreviousPath()
    {
        if (path.Length > 3) {
            path = previousPath.Last();
            previousPath.Remove(previousPath.Last());
            GetDirectories();
        }
        else
        {
            ClearList();
            GetDrives();
        }
    }

    void GetDirectories()
    {
        ClearList();
        DirectoryInfo dir = new(path);
        DirectoryInfo[] info = dir.GetDirectories();
        foreach (DirectoryInfo folder in info)
        {
            GameObject newButton = Instantiate(buttonTemplate, folderPanel);
            newButton.GetComponentInChildren<Text>().text = folder.ToString();
            spawnedButtons.Add(newButton);
            directories.Add(folder.ToString());
        }
    }

    void GetDrives()
    {
        foreach (DriveInfo drive in drives)
        {
            GameObject newButton = Instantiate(buttonTemplate, folderPanel);
            newButton.GetComponentInChildren<Text>().text = drive.ToString();
            spawnedButtons.Add(newButton);
        }
    }

    void ClearList()
    {
        directories.Clear();
        foreach (GameObject button in spawnedButtons)
        {
            Destroy(button);
        }
        spawnedButtons.Clear();
    }

    public void OnSelect()
    {
        if (File.Exists(path + "\\Run.exe")) { 
            string musicPath = path + "\\CD\\ALT\\";
            path = path + "\\HDD\\TRILOGY\\CD\\"; 
            DataManager.data.installLocation = path;
            LoadData(musicPath);
        }
        else if (File.Exists(path + "\\TRILOGY.EXE")) { 
            path = path + "\\CD\\"; DataManager.data.installLocation = path;
            string musicPath = path + "\\CD\\ALT\\";
            LoadData(musicPath);
        }
        else { 
            StopAllCoroutines();
            FlairText.flair.TypeText("MU/TH/UR 9000 not found"); 
        }
    }

    void LoadData(string musicPath)
    {
        StopAllCoroutines();
        FlairText.flair.TypeText("MU/TH/UR 9000 located - Loading...");
        CreateMovies.encoder.FindVideos();
        MainMenuActions.menuBackground.Startup();
        MainMenuActions.menuBackground.SetImages(4, 5, false);
        foreach (string fileLocation in Directory.GetFiles(musicPath))
        {
            if (fileLocation.EndsWith("ogg"))
            {
                DataManager.data.musicList.Add(fileLocation);
            }
        }
        string[] audioFiles = Directory.GetFiles(DataManager.data.installLocation + "\\SFX\\", "*.RAW");
        foreach (string file in audioFiles)
        {
            AudioClip audioClip = AudioConverter.ConvertRawToAudioClip(file);
            audioClip.name = Path.GetFileNameWithoutExtension(file);
            DataManager.data.sfxList.Add(audioClip);
        }
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString.Contains("network path") || logString.Contains("denied"))
        {
            FlairText.flair.TypeText("Access Denied, Please Try Again");
            PreviousPath();
        }
    }
}
