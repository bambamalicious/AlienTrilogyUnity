using FFmpegUnityBind2;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CreateMovies : MonoBehaviour
{

    string[] gameMovies;
    public static CreateMovies encoder;
    
    public RawImage image;
    //Set from the Editor
    public List<string> videoClipList;

    public List<VideoPlayer> videoPlayerList = new();
    public int videoIndex = 0;
    public bool videoPlaying;

    VideoPlayer videoPlayer;

    bool escapePressed = false;

    private void Start()
    {
        //set public class available to be called in FolderFinder (and later to play other videos in game)
        if(encoder == null)
        {
            encoder = this;
        }
        else { Destroy(gameObject);}
        image = GetComponent<RawImage>();
    }

    private void Update()
    {
        //When video playing, check for ESC key and give 5 seconds to enable skipping of videos.
        if (Input.GetKeyDown(KeyCode.Escape) && escapePressed == false && videoPlayerList.Count > 0) {
            FlairText.flair.TypeText("Press ESC again to skip");
            escapePressed = true;
            Invoke(nameof(EscapeOff), 5f);
        }
        //Skip videos, close down existing videoPlayers and clear list.
        else if(Input.GetKeyDown(KeyCode.Escape) && escapePressed == true && videoPlayerList[videoIndex].isPlaying)
        {
            EndOfMovies();
        }
        if (videoPlaying && !videoPlayerList[videoIndex].isPlaying && videoIndex == videoPlayerList.Count-1)
        {
            EndOfMovies();
        }
    }

    //Boolian flip to check for second press of escape
    void EscapeOff()
    {
        FlairText.flair.flairText.text = "";
        escapePressed = false;
    }

    [ContextMenu("Find Videos")] // Get all videos in ALT directory and push to FFMpeg for transcoding)
    public void FindVideos()
    {
        gameMovies = Directory.GetFiles(DataManager.data.installLocation + "\\AVI\\");
        string[] existingFiles = Directory.GetFiles(Application.dataPath + "/StreamingAssets/");
        foreach (string file in existingFiles) {
            if (file.Contains("wmv"))
            {
                File.Delete(file);
            }
        }
        for (int i = 0; i < gameMovies.Length; i++)
        {
          CreateVideo(gameMovies[i], Application.dataPath + "/StreamingAssets/", Path.GetFileName(gameMovies[i]));
        }        
    }

    //Send video commands to FFMpeg
    public void CreateVideo(string fileName, string outputDirectory, string file)
    {
        string outputPath = Path.Combine(outputDirectory, $"{file}.wmv");
        string command = new FFmpegConvertCommand(fileName, outputPath).ToString();
        FFmpeg.Execute(command);
        Debug.Log(command);
    }

    //Check for longest video transcode and start playback on successful ALT folder.
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
        if (logString.Contains("On Success. Execution Id: 3017"))
        {
            FlairText.flair.flairText.text = "";
            StartFilms();
        }
    }


    //Load up intro clips and start sequential play.
    public void StartFilms()
    {
        videoClipList.Add(Application.dataPath + "/StreamingAssets/FOXDKAUD.AVI.wmv");
        videoClipList.Add(Application.dataPath + "/StreamingAssets/ALOGODUK.AVI.wmv");
        videoClipList.Add(Application.dataPath + "/StreamingAssets/PRBLOGO.AVI.wmv");
        videoClipList.Add(Application.dataPath + "/StreamingAssets/INTRO.AVI.wmv");
        image.gameObject.SetActive(true);
        StartCoroutine(playVideo());
        FolderFinder.finder.gameObject.SetActive(false);
    }

    //Sequential play co-routine
    IEnumerator playVideo(bool firstRun = true)
    {
        if (videoClipList == null || videoClipList.Count <= 0)
        {
            yield break;
        }
        image.enabled = true;
        MainMenuActions.menuBackground.image1.texture = MainMenuActions.menuBackground.background;
        MainMenuActions.menuBackground.image2.texture = MainMenuActions.menuBackground.background;
        //Init videoPlayerList first time this function is called
        if (firstRun)
        {
            for (int i = 0; i < videoClipList.Count; i++)
            {
                //Create new Object to hold the Video and the sound then make it a child of this object
                GameObject vidHolder = new GameObject("VP" + i);
                vidHolder.transform.SetParent(transform);

                //Add VideoPlayer to the GameObject
                videoPlayer = vidHolder.AddComponent<VideoPlayer>();
                videoPlayerList.Add(videoPlayer);

                //Add AudioSource to  the GameObject
                AudioSource audioSource = vidHolder.AddComponent<AudioSource>();

                //Disable Play on Awake for both Video and Audio
                videoPlayer.playOnAwake = false;
                audioSource.playOnAwake = false;

                //We want to play from video clip not from url
                videoPlayer.source = VideoSource.VideoClip;

                //Set Audio Output to AudioSource
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

                //Assign the Audio from Video to AudioSource to be played
                videoPlayer.EnableAudioTrack(0, true);
                videoPlayer.SetTargetAudioSource(0, audioSource);

                //Set video Clip To Play 
                videoPlayer.url = videoClipList[i];
            }
        }

        //Make sure that the NEXT VideoPlayer index is valid
        if (videoIndex >= videoPlayerList.Count)
            yield break;

        //Prepare video
        videoPlayerList[videoIndex].Prepare();

        //Wait until this video is prepared
        while (!videoPlayerList[videoIndex].isPrepared)
        {
            Debug.Log("Preparing Index: " + videoIndex);
            yield return null;
        }
        Debug.LogWarning("Done Preparing current Video Index: " + videoIndex);

        //Assign the Texture from Video to RawImage to be displayed
        image.texture = videoPlayerList[videoIndex].texture;

        //Play first video
        videoPlayerList[videoIndex].Play();

        //Wait while the current video is playing
        bool reachedHalfWay = false;
        int nextIndex = (videoIndex + 1);
        while (videoPlayerList[videoIndex].isPlaying)
        {
            //(Check if we have reached half way)
            if (!reachedHalfWay && videoPlayerList[videoIndex].time >= (videoPlayerList[videoIndex].length / 2))
            {
                reachedHalfWay = true; //Set to true so that we don't evaluate this again

                //Make sure that the NEXT VideoPlayer index is valid. Othereise Exit since this is the end
                if (nextIndex >= videoPlayerList.Count)
                {
                    Debug.LogWarning("End of All Videos: " + videoIndex);
                    videoPlaying = true;
                    yield break;
                }

                //Prepare the NEXT video
                Debug.LogWarning("Ready to Prepare NEXT Video Index: " + nextIndex);
                videoPlayerList[nextIndex].Prepare();
            }
            yield return null;
        }
        Debug.Log("Done Playing current Video Index: " + videoIndex);

        //Wait until NEXT video is prepared
        while (!videoPlayerList[nextIndex].isPrepared)
        {
            Debug.Log("Preparing NEXT Video Index: " + nextIndex);
            yield return null;
        }
        Debug.LogWarning("Done Preparing NEXT Video Index: " + videoIndex);
        //Increment Video index
        videoIndex++;

        //Play next prepared video. Pass false to it so that some codes are not executed at-all
        StartCoroutine(playVideo(false));
    }

    void EndOfMovies()
    {
        StopAllCoroutines();
        videoPlaying = false;
        FlairText.flair.flairText.text = "";
        image.enabled = false;
        videoPlayerList[videoIndex].Stop();
        MainMenuActions.menuBackground.SetImages(6, 7, true);
        foreach (VideoPlayer player in videoPlayerList)
        {
            Destroy(player.gameObject);
        }
        videoPlayerList.Clear();
        MainMenuActions.menuBackground.EnableMenu();
        this.gameObject.SetActive(false);
    }
}

