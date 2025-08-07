using System.IO;
using UnityEngine;

public class MapFinder : MonoBehaviour
{
    public string gameDirectory = "C:\\Program Files (x86)\\Collection Chamber\\Alien Trilogy\\";
    public string levelPath1 = "";
    public string levelPath2 = "";
    public string levelPath3 = "";
    public string levelPath4 = "";
    public string levelPath5 = "";
    public string levelPath6 = "";
    public string levelPath7 = "";
    public string[] levels;

    void Start()
    {
        CheckDirectory();
    }

    public void CheckDirectory()
    {
        if (File.Exists(gameDirectory + "Run.exe")) { gameDirectory =gameDirectory + "HDD\\TRILOGY\\CD\\"; }
        else if (File.Exists(gameDirectory + "TRILOGY.EXE")) { gameDirectory = gameDirectory + "CD\\"; }
        levelPath1 = gameDirectory + "SECT11\\";
        levelPath2 = gameDirectory + "SECT12\\";
        levelPath3 = gameDirectory + "SECT21\\";
        levelPath4 = gameDirectory + "SECT22\\";
        levelPath5 = gameDirectory + "SECT31\\";
        levelPath6 = gameDirectory + "SECT32\\";
        levelPath7 = gameDirectory + "SECT90\\";
        levels =  new string[]{ levelPath1, levelPath2, levelPath3, levelPath4, levelPath5, levelPath6, levelPath7 };
        ListLevels();
    }

    public void ListLevels()
    {
        foreach (string level in levels)
        {
            string[] levelFiles = Directory.GetFiles(level, "*.MAP");
            foreach (string levelFile in levelFiles) { 
                UnityEngine.Debug.Log(Path.GetFileNameWithoutExtension(levelFile)); 
            }
        }
    }

    [ContextMenu("Generate Level")]
    public void GeneratateLevel()
    {
        AlienTrilogyMapLoader.loader.Initiate(levelPath1+"L131LEV.MAP", levelPath1 + "131GFX.B16");
    }

    [ContextMenu("Generate Obj Data")]
    public void GeneratateObjects()
    {
        ObjDataPuller.objectPuller.Initiate(levelPath1 + "L131LEV.MAP", levelPath1 + "131GFX.B16");
    }
}
