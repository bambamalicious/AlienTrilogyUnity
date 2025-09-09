using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using UnityEngine.Networking;

public class MainMenuActions : MonoBehaviour
{
    public RawImage image1;
    public RawImage image2;
    public Texture background;
    public GameObject menu;
    public GameObject optionsPanel;
    public List<Button> buttons = new();
    public AudioSource sfxAudio;
    string gfxDirectory;
    string paletteDirectory;
    List<string> fileNames = new();
    List<string> paletteNames = new();
    [HideInInspector]
    public byte[] currentPalette; // current palette data for the selected file
    private byte[] currentFrame; // current frame data for compressed files
    List<BndSection> currentSections = new();

    public AudioSource musicAudio;

    private static string[] removal = { "DEMO111", "DEMO211", "DEMO311", "PICKMOD", "OPTOBJ", "OBJ3D", "PANEL3GF", "PANELGFX" }; // unused demo files and models
    private static string[] duplicate = { "EXPLGFX", "FLAME", "MM9", "OPTGFX", "PULSE", "SHOTGUN", "SMART" }; // remove duplicate entries & check for weapons
    private static string[] weapons = { "FLAME","EXPLGFX", "FONT1GFX","OPTGFX","PICKGFX", "MM9", "PULSE", "SHOTGUN", "SMART" }; // check for weapons
    private static string[] excluded = { "LEV", "GUNPALS", "SPRITES", "WSELECT", "PANEL", "NEWFONT", "MBRF_PAL" }; // excluded palettes
                                                                                                                   // PANEL & NEWFONT are used by the game but not required for rendering

    public static MainMenuActions menuBackground;
    public class BndSection
    {
        public string Name { get; set; } = "";
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
    private void Start()
    {
        if (menuBackground == null)
        {
            menuBackground = this;
        }
        else { Destroy(gameObject); }
        musicAudio = GetComponent<AudioSource>();
    }

    //Startup Script and load Main Menu
    public void Startup()
    {
        DataManager.data.imgData.Clear();
        fileNames.Clear();
        paletteNames.Clear();
        gfxDirectory = DataManager.data.installLocation + "GFX\\";
        paletteDirectory = DataManager.data.installLocation + "PALS\\";
        ListFiles(gfxDirectory, ".BND", ".B16", true);
    }

    public void ButtonSound(int sound)
    {
        sfxAudio.clip = DataManager.data.sfxList[sound];
        sfxAudio.volume = DataManager.data.soundVolume;
        sfxAudio.Stop();
        sfxAudio.Play();
    }

    public void OnClick(int item)
    {
        ButtonSound(1);
        switch (item)
        {
            case 0: break;
                //Play game actions
            case 1: break;
                //Load Game Actions
            case 2: break;
                //Multiplayer Actions
            case 3:
                menu.SetActive(false);
                SetImages(8,9,false);
                optionsPanel.SetActive(true);
                optionsPanel.GetComponent<OptionsManager>().StartUp();
                break;
            case 4:
                Application.Quit();
                break;
            case 5:
                menu.SetActive(true);
                SetImages(6, 7, false);
                optionsPanel.SetActive(false);
                break;
        }
    }

    public void EnableMenu()
    {
        menu.SetActive(true);
        foreach (Transform child in menu.transform)
        {
            buttons.Add(child.GetComponent<Button>());
        }
        int fontSize = buttons[1].GetComponent<Text>().cachedTextGenerator.fontSizeUsedForBestFit;
        foreach (Button button in buttons)
        {
            button.transform.GetComponentInChildren<Text>().fontSize = fontSize;
        }
    }

    public void ListFiles(string path, string type1 = ".BND", string type2 = ".B16", bool enabled = false)
    { 
        foreach (string file in DiscoverFiles(path, type1, type2)) { fileNames.Add(Path.GetFileNameWithoutExtension(file)); }
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var toRemove = new List<string>(); // Items to remove
        foreach (string item in fileNames) // Count occurrences
        {
            if (!counts.ContainsKey(item)) { counts[item] = 0; }
            counts[item]++;
        }
        foreach (var file in weapons) { if (fileNames.Contains(file)) { toRemove.Add(file); } } // Add always-remove items
        foreach (var file in removal) { if (fileNames.Contains(file)) { toRemove.Add(file); } } // Add always-remove items
        foreach (var file in duplicate) { if (fileNames.Contains(file)) { toRemove.Add(file); } } // Add always-remove items
        foreach (var file in toRemove) { fileNames.Remove(file); } // Remove items
        for (int i = 0; i < fileNames.Count; i++)
        {
            RenderImage(gfxDirectory + fileNames[i] + ".BND", paletteDirectory + fileNames[i] + ".PAL", fileNames[i]);
        }
    }
    private string[] DiscoverFiles(string path, string type1 = ".BND", string type2 = ".B16")
    {
        return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(file => file.EndsWith(type1) || file.EndsWith(type2)).ToArray();
    }

    //Load Selected Image Palette
    private void LoadPalette(string palettePath, bool trim, int start, int end, bool full)
    {
        Array.Clear(currentPalette, 0, currentPalette.Length);
        if (full) { 
        currentPalette = File.ReadAllBytes(palettePath); }
        else
        {
           byte[] loaded = File.ReadAllBytes(palettePath);
           currentPalette = new byte[768];
           Array.Copy(loaded, 0, currentPalette, start, end); // 96 padded bytes at the beginning for these palettes
        }
        paletteNames.Add(Path.GetFileNameWithoutExtension(palettePath));
    }

    // loads and renders the selected image
    private void RenderImage(string binbnd, string palettePath, string fileName)
    {
        if (palettePath.Contains("LOGOSGFX")) { LoadPalette(palettePath, false, 0, 576, false); }
        else if (palettePath.Contains("PRISHOLD") || palettePath.Contains("COLONY") || palettePath.Contains("BONESHIP")) { LoadPalette(palettePath, true, 96, 672, false); }
        else if (palettePath.Contains("LEGAL")) { LoadPalette(palettePath, false, 0, 0, true); }

        // Parse all sections (TP00, TP01, etc.)
        currentSections = ParseBndFormSections(File.ReadAllBytes(binbnd), "TP");
        for (int i = 0; i < currentSections.Count(); i++)
        {
            Texture2D texture = RenderRaw8bppImageUnity(currentSections[i].Data, currentPalette, 256);
            texture.filterMode = FilterMode.Point;
            texture.name = $"Tex_{fileName:D2}_" + i;
            DataManager.data.imgData.Add(texture);
        }
    }

    public void SetImages(int item1, int item2, bool music)
    {
        image1.texture = DataManager.data.imgData[item1];
        image2.texture = DataManager.data.imgData[item2];
        if (music)
        {
            StartCoroutine(GetAudioFile(DataManager.data.musicList[0]));
        }
    }


    //Create 8-bit texture from 16-bit palette, with transparency based on palette indices for given map id and image index
	
    private Texture2D RenderRaw8bppImageUnity(
        byte[] pixelData,
        byte[] rgbPalette,
        int dimension)
    {

        // Number of colors in palette
        int numColors = rgbPalette.Length / 3;

        // Create color array
        Color32[] pixels = new Color32[dimension * dimension];

        // Create texture object
        Texture2D texture = new Texture2D(dimension, dimension, TextureFormat.RGBA32, false);

        // Write pixels to texture
        for (int y = 0; y < dimension; y++)
        {
            for (int x = 0; x < dimension; x++)
            {
                int srcIndex = y * dimension + x;
                if (srcIndex >= pixelData.Length)
                    continue;

                byte colorIndex = pixelData[srcIndex];
                Color32 color;

                // Defensive: if palette data is incomplete or index out of range, fallback magenta
                if (colorIndex < numColors && (colorIndex * 3 + 2) < rgbPalette.Length)
                {

                    byte r = rgbPalette[colorIndex * 3];
                    byte g = rgbPalette[colorIndex * 3 + 1];
                    byte b = rgbPalette[colorIndex * 3 + 2];
                    r = (byte)(r * 4);
                    g = (byte)(g * 4);
                    b = (byte)(b * 4);
                    color = new Color32(r, g, b, 255);
                }
                else
                {
                    color = new Color32(255, 0, 255, 255); // Magenta fallback
                }

                // Handle transparency: if pixel color index is in transparentValues, make fully transparent
                //if (transparentValues != null && transparentValues.Contains(colorIndex))
                //{
                //    color.a = 0;  // Fully transparent
                //}

                // Vertical flip: write pixels from bottom to top
                int flippedY = dimension - 1 - y;
                int dstIndex = flippedY * dimension + x;
                pixels[dstIndex] = color;
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    public static List<BndSection> ParseBndFormSections(byte[] bnd, string section)
    {
        var sections = new List<BndSection>();
        using var br = new BinaryReader(new MemoryStream(bnd));
        string formTag = Encoding.ASCII.GetString(br.ReadBytes(4)); // Read FORM header
        if (formTag != "FORM") { throw new Exception("Invalid BND file: missing FORM header."); }
        int formSize = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
        string platform = Encoding.ASCII.GetString(br.ReadBytes(4)); // e.g., "PSXT"
        while (br.BaseStream.Position + 8 <= br.BaseStream.Length) // Parse chunks
        {
            string chunkName = Encoding.ASCII.GetString(br.ReadBytes(4));
            int chunkSize = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
            if (br.BaseStream.Position + chunkSize > br.BaseStream.Length) { break; }
            byte[] chunkData = br.ReadBytes(chunkSize);
            if (chunkName.StartsWith(section)) { sections.Add(new BndSection { Name = chunkName, Data = chunkData }); }
            if ((chunkSize % 2) != 0) { br.BaseStream.Seek(1, SeekOrigin.Current); } // IFF padding to 2-byte alignment
        }
        return sections;
    }
    IEnumerator GetAudioFile(string url_voice)
    {
        WWW w = new WWW(url_voice);
        yield return w;

        var ac = w.GetAudioClip();
        //var tempClip = w.audioClip;
        GetComponent<AudioSource>().clip = ac;
        GetComponent<AudioSource>().volume = DataManager.data.musicVolume/100;
        GetComponent<AudioSource>().Play();
        StopAllCoroutines();
    }
}


