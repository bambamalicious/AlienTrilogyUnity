using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class TimerExample : MonoBehaviour
{
    private Stopwatch stopwatch;

    void Start()
    {
        stopwatch = new Stopwatch();

        // Start timer
        stopwatch.Start();
        UnityEngine.Debug.Log("Timer started.");
    }

    void Update()
    {
        // Example: stop timer when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Timer stopped. Elapsed: {stopwatch.ElapsedMilliseconds} ms");

            // If you want to restart for another run:
            stopwatch.Reset();
            stopwatch.Start();
        }
    }
}

public class BndSection
{
    public string Name { get; set; } = "";
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

[System.Serializable] // Makes the class visible in the Inspector

public class Header
{
    public byte unknown1;
    public byte unknown2;
    public byte unknown3;
    public byte unknown4;
    public byte unknown5;
    public byte unknown6;
    public byte unknown7;
    public byte unknown8;
    public byte unknown9;
    public byte unknown10;
    public byte unknown11;
    public byte unknown12;
    public byte unknown13;
    public byte unknown14;
    public byte unknown15;
    public byte unknown16;
    public byte unknown17;
    public byte unknown18;
    public byte unknown19;
    public byte unknown20;
}

[System.Serializable] // Makes the class visible in the Inspector
public class CollisionNode
{
    public string Name;
    public GameObject obj;
    public byte unknown1;
    public byte unknown2;
    public byte unknown3;
    public byte unknown4;
    public byte unknown5;
    public byte unknown6;
    public byte unknown7;
    public byte unknown8;
    public byte ceilingFog;
    public byte floorFog;
    public byte ceilingHeight;
    public byte floorHeight;
    public byte unknown13;
    public byte unknown14;
    public byte Lighting;
    public byte scriptAction;
}

[System.Serializable] // Makes the class visible in the Inspector
public class PathNode
{
    public string Name;
    public GameObject obj;
    public byte X;
    public byte Y;
    public byte unused;
    public byte nodeState;
    public byte NodeA;
    public byte NodeB;
    public byte NodeC;
    public byte NodeD;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Monster
{
    public string Name;
    public GameObject spawnedObj;
    public byte Type;
    public byte X;
    public byte Y;
    public byte Z;
    public byte Rotation;
    public byte Health;
    public byte Drop;
    public byte unknown2;
    public byte Difficulty;
    public byte unknown4;
    public byte unknown5;
    public byte unknown6;
    public byte unknown7;
    public byte unknown8;
    public byte Speed;
    public byte unknown9;
    public byte unknown10;
    public byte unknown11;
    public byte unknown12;
    public byte unknown13;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Crate
{
    public string Name;
    public GameObject spawnedObject;
    public byte X;
    public byte Y;
    public byte Type;
    public byte Drop;
    public byte unknown1;
    public byte unknown2;
    public byte Drop1;
    public byte Drop2;
    public byte unknown3;
    public byte unknown4;
    public byte unknown5;
    public byte unknown6;
    public byte unknown7;
    public byte unknown8;
    public byte Rotation;
    public byte unknown10;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Pickup
{
    public string Name;
    public GameObject spawnedObject;
    public byte X;
    public byte Y;
    public byte Type;
    public byte Amount;
    public byte Multiplier;
    public byte unknown1;
    public byte Z;
    public byte unknown2;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Door
{
    public string name;
    public GameObject spawnedObject;
    public byte X;
    public byte Y;
    public byte unknown;
    public byte Time;
    public byte LockState;
    public byte unknown2;
    public byte Rotation;
    public byte modelIndex;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Lifts
{
    public string Name;
    public GameObject spawnedObject;
    public byte X;
    public byte Y;
    public byte Z;
    public byte unknown1;
    public byte unknown2;
    public byte unknown3;
    public byte unknown4;
    public byte unknown5;
    public byte unknown6;
    public byte unknown7;
    public byte unknown8;
    public byte unknown9;
    public byte unknown10;
    public byte unknown11;
    public byte unknown12;
    public byte unknown13;
}

[System.Serializable] // Makes the class visible in the Inspector
public class ActionGroup
{
    public string name;
    public byte actionType;
    public byte logicStep; 
    public byte byte3;
    public byte byte4;
}

[System.Serializable] // Makes the class visible in the Inspector
public class LogicGroup
{
    public string name;
    public byte action;
    public byte nextStep;
    public byte modifier;
    public byte objectIndex;
}

[System.Serializable] // Makes the class visible in the Inspector
public class RemainderBytes
{
    public byte byte1;
    public byte byte2;
    public byte byte3;
    public byte byte4;
    public byte byte5;
    public byte byte6;        
    public byte byte7;
    public byte byte8;
}

/*
	Alien Trilogy Data Loader
	Load data directly from original Alien Trilogy files to use it in Unity
*/
public class AlienTrilogyMapLoader : MonoBehaviour
{
    //[Header("PATHS")]
    private string levelPath = ""; // path to the .MAP file
    private string texturePath = ""; // path to the .B16 file

    [Header("Object Lists")]
    [HideInInspector]
    public List<PathNode> pathNodes;
    [HideInInspector]
    public List<CollisionNode> collisions;
    [HideInInspector]
    public List<Monster> monsters;
    [HideInInspector]
    public List<Crate> boxes;
    public List<Pickup> pickups;
    [HideInInspector]
    public List<Door> doors;
    public List<Lifts> lifts;
    public List<ActionGroup> actions;
    public List<LogicGroup> logics;
    public List<RemainderBytes> remainderBytes;

    [Header("Map Values")]
    //Map values
    public byte pathCount;
    public UInt16 vertCount, quadCount, mapLength, mapWidth, playerStartX, playerStartY, monsterCount, pickupCount, boxCount, doorCount, liftCount, playerStart, unknownByte1, unknownByte2, unknownByte3;
    public int noOfInteractables = 0;


    [Header("Settings")]
    // TODO : Adjust this dynamically
    public int textureSize = 256; // pixel dimensions
    public bool generateCSV;
    private float scalingFactor = 1 / 512f; // scaling corrections
    public Material baseMaterial;

    // These store the mesh data for Unity
    private List<Vector3> meshVertices = new();
    private List<Vector2> meshUVs = new();
    private Dictionary<int, List<int>> meshTriangles = new();

    // Original vertex data from MAP0 before duplication
    private List<Vector3> originalVertices = new();

    // UV rectangles for each texture group
    private List<List<(int X, int Y, int Width, int Height)>> uvRects = new();

    // Texture image data list
    private List<Texture2D> imgData = new();

    //Final Map Output
    public GameObject child;

    public static AlienTrilogyMapLoader loader;

    /*
		Called once as soon as this script is loaded
	*/
    void Start()
    {
        if (loader == null)
        {
            loader = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /*
		Load the file from a given path and build the map in Unity
	*/
    public void Initiate(string levelToLoad, string texturesToLoad)
    {
        //baseMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        levelPath = levelToLoad;
        texturePath = texturesToLoad;
        BuildMapTextures(); // Build map textures
        BuildMapGeometry(); // Build map geometry
        BuildMapMesh();     // Build map mesh
    }
    /*
		Build the map textures
	*/
    private void BuildMapTextures()
    {
        Texture2D texture = null!;
        int levelID = int.Parse(levelPath.Substring(levelPath.Length - 10, 3)); //Get Level ID from levelPath String
        List<(int X, int Y, int Width, int Height)> rectangles = new();
        using var br = new BinaryReader(File.OpenRead(texturePath));
        br.BaseStream.Seek(36, SeekOrigin.Current);         // skip base header (36)
        for (int i = 0; i < 5; i++)                         // perfect read order TP/CL/BX*5
        {
            br.BaseStream.Seek(8, SeekOrigin.Current);      // TP header
            byte[] TP = br.ReadBytes(65536);                // texture
            br.BaseStream.Seek(12, SeekOrigin.Current);     // CL header
            byte[] CL = br.ReadBytes(512);                  // palette
            br.BaseStream.Seek(8, SeekOrigin.Current);      // BX header
            rectangles = new();                             // renew rectangles list
            int rectCount = br.ReadInt16();                 // UV rectangle count
            for (int j = 0; j < rectCount; j++)
            {
                byte x = br.ReadByte();
                byte y = br.ReadByte();
                byte width = br.ReadByte();
                byte height = br.ReadByte();
                br.BaseStream.Seek(2, SeekOrigin.Current);  // unknown bytes
                rectangles.Add((x, y, width + 1, height + 1));
            }
            if (rectCount % 2 == 0) { br.BaseStream.Seek(2, SeekOrigin.Current); }    // if number of UVs is even, read forward two extra bytes
            uvRects.Add(rectangles);
            texture = RenderRaw8bppImageUnity(TP, Convert16BitPaletteToRGB(CL), textureSize, levelID, i);
            texture.filterMode = FilterMode.Point;
            texture.name = $"Tex_{i:D2}";
            imgData.Add(texture);
        }
    }
    /*
		Create 16-bit RGB palette
	*/
    public byte[] Convert16BitPaletteToRGB(byte[] rawPalette)
    {
        if (rawPalette == null || rawPalette.Length < 2)
            throw new ArgumentException("Palette data is missing or too short.");

        int colorCount = rawPalette.Length / 2;
        byte[] rgbPalette = new byte[256 * 3]; // max 256 colors RGB

        for (int i = 0; i < colorCount && i < 256; i++)
        {
            // Read 16-bit color (little endian)
            ushort color = (ushort)((rawPalette[i * 2 + 1] << 8) | rawPalette[i * 2]);

            int r5 = color & 0x1F;
            int g5 = (color >> 5) & 0x1F;
            int b5 = (color >> 10) & 0x1F;

            // Convert 5-bit color to 8-bit using bit replication
            byte r8 = (byte)((r5 << 3) | (r5 >> 2));
            byte g8 = (byte)((g5 << 3) | (g5 >> 2));
            byte b8 = (byte)((b5 << 3) | (b5 >> 2));

            rgbPalette[i * 3 + 0] = r8;
            rgbPalette[i * 3 + 1] = g8;
            rgbPalette[i * 3 + 2] = b8;
        }
        return rgbPalette;
    }
    /*
		Returns transparent palette indices based on map id and image index
	*/
    private int[] GetTransparencyValues(int id, int index)
    {
        int[] values = null;

        switch (id)
        {
            case 111:
            case 113:
            case 114:
            case 115:
            case 121:
                switch (index)
                {
                    case 0:
                    case 2:
                    case 4:
                        values = new int[] { 255 };
                        break;
                    case 1:
                    case 3:
                        values = new int[] { 0 };
                        break;
                }
                break;

            case 112:
                switch (index)
                {
                    case 0:
                        return values;
                    case 1:
                    case 3:
                        values = new int[] { 0 };
                        break;
                    case 2:
                    case 4:
                        values = new int[] { 255 };
                        break;
                }
                break;

            case 122:
            case 213:
                switch (index)
                {
                    case 0:
                        values = new int[] { 255 };
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        return values;
                }
                break;

            case 131:
            case 211:
            case 212:
            case 231:
            case 232:
            case 242:
            case 243:
            case 262:
            case 331:
            case 361:
            case 391:
            case 901:
            case 906:
            case 907:
                return values;

            case 141:
            case 155:
            case 161:
            case 162:
            case 263:
            case 311:
            case 352:
            case 353:
            case 381:
            case 902:
            case 903:
            case 908:
            case 909:
                switch (index)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        return values;
                    case 4:
                        values = new int[] { 255 };
                        break;
                }
                break;

            case 154:
            case 321:
            case 322:
            case 323:
            case 324:
            case 325:
                switch (index)
                {
                    case 0:
                    case 4:
                        values = new int[] { 255 };
                        break;
                    case 1:
                    case 2:
                    case 3:
                        return values;
                }
                break;

            case 222:
                switch (index)
                {
                    case 0:
                    case 1:
                    case 3:
                    case 4:
                        return values;
                    case 2:
                        values = new int[] { 255 };
                        break;
                }
                break;

            case 351:
            case 371:
                switch (index)
                {
                    case 0:
                    case 1:
                    case 3:
                        return values;
                    case 2:
                    case 4:
                        values = new int[] { 255 };
                        break;
                }
                break;

            case 900:
                switch (index)
                {
                    case 0:
                        values = new int[] { 255 };
                        break;
                    case 1:
                    case 3:
                        values = new int[] { 0 };
                        break;
                    case 2:
                    case 4:
                        return values;
                }
                break;

            case 904:
            case 905:
                switch (index)
                {
                    case 0:
                        values = new int[] { 255 };
                        break;
                    case 1:
                    case 2:
                    case 3:
                        return values;
                    case 4:
                        values = new int[] { 255 };
                        break;
                }
                break;

            default:
                break;
        }

        return values;
    }
    /*
		Create 8-bit texture from 16-bit palette, with transparency based on palette indices for given map id and image index
	*/
    private Texture2D RenderRaw8bppImageUnity(
        byte[] pixelData,
        byte[] rgbPalette,
        int dimension,
        int mapId,
        int imageIndex)
    {
        // Get transparency for this image
        int[] transparentValues = GetTransparencyValues(mapId, imageIndex);

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
                    color = new Color32(r, g, b, 255);
                }
                else
                {
                    color = new Color32(255, 0, 255, 255); // Magenta fallback
                }

                // Handle transparency: if pixel color index is in transparentValues, make fully transparent
                if (transparentValues != null && transparentValues.Contains(colorIndex))
                {
                    color.a = 0;  // Fully transparent
                }

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
    /*
		Build the map geometry and prepare mesh data (vertices, uvs, triangles)
	*/
    private void BuildMapGeometry()
    {
        // Clear all lists to avoid artefacts
        meshVertices.Clear();
        meshUVs.Clear();
        meshTriangles.Clear();
        originalVertices.Clear();
        // load MAP0 chunk
        using var br = new BinaryReader(File.OpenRead(levelPath));
        br.BaseStream.Seek(20, SeekOrigin.Current);     // Skip header bytes
        vertCount = br.ReadUInt16();             // Read number of vertices
        quadCount = br.ReadUInt16();             // Read number of quads
        mapLength = br.ReadUInt16();             // map length & display in inspector
        mapWidth = br.ReadUInt16();              // map width & display in inspector
        playerStartX = br.ReadUInt16();          // player start X coordinate
        playerStartY = br.ReadUInt16();          // player start Y coordinate
        pathCount = br.ReadByte();               // path count
        br.ReadByte();                           // UNKNOWN 0 ( unused? 128 on all levels ) - possibly lighting related             
        monsterCount = br.ReadUInt16();          // monster count
        pickupCount = br.ReadUInt16();           // pickup count
        boxCount = br.ReadUInt16();              // object count
        doorCount = br.ReadUInt16();             // door count
        liftCount = br.ReadUInt16();             // lift count
        playerStart = br.ReadUInt16();           // player start angle
        // unknown bytes
        unknownByte1 = br.ReadUInt16();          // unknown 1
        br.ReadBytes(2);                         // always 0x4040    Action block, 2 x 64 bytes.
        unknownByte2 = br.ReadUInt16();          // unknown2
        ushort enemyTypes = br.ReadUInt16();     // Available Enemy Types
        // Chapter 1 ( enemyTypes )
        // L111LEV - 22 00 // 2 / 6
        // L112LEV - 22 00 // 2 / 6 / 16
        // L113LEV - 00 00 // null
        // L122LEV - 22 04 // 2 / 6 / 11 / 16
        // L131LEV - 26 04 // 2 / 6 / 11 / 3
        // L114LEV - 00 00 // null
        // L141LEV - 23 00 // 6 / 1 / 2
        // L115LEV - 00 00 // null
        // L154LEV - 23 10 // 6 / 1 / 2 / 13 / 16 / 17 / 19
        // L155LEV - 0C 00 // 18 / 16 / 19
        // L161LEV - A7 02 // 6 / 1 / 2 / 8 / 10 / 3
        // L162LEV - 43 00 // 7 / 1 / 2
        // Chapter 2 ( enemyTypes )
        // L211LEV - 0E 08 // 4 / 12 / 2 / 3
        // L212LEV - 0A 08 // 4 / 2 / 12
        // L213LEV - 02 08 // 2 / 12
        // L222LEV - 0B 08 // 1 / 2 / 12 / 4 / 17 / 19
        // L242LEV - 00 00 // null
        // L231LEV - 14 21 // 14 / 5 / 3 / 9
        // L232LEV - 13 10 // 1 / 2 / 5 / 13 / 16 / 17
        // L243LEV - 00 00 // null
        // L262LEV - 17 02 // 5 / 1 / 2 / 10 / 3
        // L263LEV - 43 00 // 7 / 1 / 2
        // Chapter 3 ( enemyTypes )
        // L311LEV - 10 20 // 5 / 14
        // L321LEV - 02 00 // 2
        // L331LEV - 12 21 // 5 / 14 / 2 / 9
        // L322LEV - 08 00 // 4
        // L351LEV - 24 12 // 6 / 13 / 10 / 3
        // L352LEV - 00 00 // null
        // L323LEV - 10 00 // 5
        // L371LEV - 20 10 // 6 / 13
        // L353LEV - 00 00 // null
        // L324LEV - 22 00 // 2 / 6
        // L381LEV - 23 00 // 6 / 1 / 2
        // L325LEV - 36 00 // 3 / 5 / 2 / 6
        // L391LEV - 43 00 // 7 / 1 / 2
        // Multiplayer Levels ( enemyTypes )
        // All = 00 10
        unknownByte3 = br.ReadUInt16();              // unknown3
        for (int i = 0; i < vertCount; i++)
        {
            try
            {
                short x = br.ReadInt16();
                short y = br.ReadInt16();
                short z = br.ReadInt16();
                br.ReadBytes(2); // unknown bytes
                originalVertices.Add(new Vector3(x, y, z));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed at vertex index {i}: {e}");
                break;
            }
        }

        //UnityEngine.Debug.Log("originalVertices.Count = " + originalVertices.Count);

        // Read and process quads, duplicating vertices & uvs per quad
        for (int i = 0; i < quadCount; i++)
        {
            int a = 0;
            int b = 0;
            int c = 0;
            int d = 0;
            try
            {
                a = br.ReadInt32();
                b = br.ReadInt32();
                c = br.ReadInt32();
                d = br.ReadInt32();
                ushort texIndex = br.ReadUInt16();
                byte flags = br.ReadByte();
                byte other = br.ReadByte();

                // Determine texture group based on texIndex
                int localIndex = texIndex;
                int texGroup = 0;
                for (int t = 0; t < uvRects.Count; t++)
                {
                    int count = uvRects[t].Count;
                    if (localIndex < count)
                    {
                        texGroup = t;
                        break;
                    }
                    localIndex -= count;
                }

                Vector3 vA, vB, vC, vD;
                bool issueFound = false;

                // Validate vertex indices
                if (a < 0 || b < 0 || c < 0 || d < 0 ||
                    a >= originalVertices.Count || b >= originalVertices.Count ||
                    c >= originalVertices.Count || d >= originalVertices.Count)
                {
                    //UnityEngine.Debug.LogWarning($"Invalid quad indices at quad {i}: {a}, {b}, {c}, {d}, {flags}");
                    d = a; // Triangle instead of quad
                    issueFound = true;
                }

                // Get quad vertices positions
                vA = originalVertices[a];
                vB = originalVertices[b];
                vC = originalVertices[c];
                vD = originalVertices[d];

                // Get UV rectangle for texture
                var rect = uvRects[texGroup][localIndex];

                // Calculate UV coords (Unity uses bottom-left origin for UV)
                float texSize = (float)textureSize;
                float uMin = rect.X / texSize;
                float vMin = 1f - (rect.Y + rect.Height) / texSize;
                float uMax = (rect.X + rect.Width) / texSize;
                float vMax = 1f - rect.Y / texSize;

                Vector2[] baseUvs = new Vector2[]
                {
                        new Vector2(uMin, vMin), // A
						new Vector2(uMax, vMin), // B
						new Vector2(uMax, vMax), // C
						new Vector2(uMin, vMax)  // D
                };

                Vector2[] quadUvs;

                // Adjust UVs based on quad flags
                switch (flags)
                {
                    case 2:
                        // Flip texture 180 degrees
                        quadUvs = new Vector2[] { baseUvs[1], baseUvs[0], baseUvs[3], baseUvs[2] };
                        break;
                    case 11:
                        // Special triangle case: repeat D's UV for the 4th vertex
                        quadUvs = new Vector2[] { baseUvs[0], baseUvs[2], baseUvs[3], baseUvs[3] };
                        break;
                    default:
                        // Default UV mapping
                        quadUvs = baseUvs;
                        break;
                }

                // Adjust UVs based on triangles
                if (issueFound)
                {
                    quadUvs = new Vector2[] { baseUvs[0], baseUvs[2], baseUvs[3], baseUvs[3] };
                }

                // Add duplicated vertices for this quad
                int baseIndex = meshVertices.Count;

                meshVertices.Add(vA);
                meshVertices.Add(vB);
                meshVertices.Add(vC);
                meshVertices.Add(vD);

                // Add UVs for each vertex
                meshUVs.AddRange(quadUvs);

                // Add triangles for this quad (two triangles)
                if (!meshTriangles.TryGetValue(texGroup, out List<int> tris))
                {
                    tris = new List<int>();
                    meshTriangles[texGroup] = tris;
                }

                tris.Add(baseIndex + 0);
                tris.Add(baseIndex + 1);
                tris.Add(baseIndex + 2);

                tris.Add(baseIndex + 2);
                tris.Add(baseIndex + 3);
                tris.Add(baseIndex + 0);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Failed at quad index {i}:  {a}, {b}, {c}, {d} - {e}");
                break;
            }
        }
        int collisionBlockCount = mapLength * mapWidth;
        // collision block formula = multiply length by width - (16 bytes per collision node.)
        for (int i = 0; i < collisionBlockCount; i++)
        {
            CollisionNode node = new()
            {
                unknown1 = br.ReadByte(),       // all values exist from 0-255                      ( 256 possible values )
                unknown2 = br.ReadByte(),       // all values exist from 0-55 and 255               ( 57 possible values )
                unknown3 = br.ReadByte(),       // only ever 255 or 0 across all levels in the game ( 255 = wall / 0 == traversable )
                unknown4 = br.ReadByte(),       // only ever 255 or 0 across all levels in the game ( 255 = wall / 0 == traversable )
                unknown5 = br.ReadByte(),       // only ever 0 across every level in the game       ( 1 possible value ) - blocked from CollisonObj script
                unknown6 = br.ReadByte(),       // only ever 0-21 across every level in the game    ( 22 possible values )
                unknown7 = br.ReadByte(),       // only ever 0-95 across every level in the game    ( 96 possible values )
                unknown8 = br.ReadByte(),       // only ever 0 across every level in the game       ( 1 possible value ) - Blocked from collision script for simplicity.
                ceilingFog = br.ReadByte(),     // a range of different values                      ( 43 possible values ) - Useable height of collider
                floorFog = br.ReadByte(),       // a range of different values                      ( 40 possible values )
                ceilingHeight = br.ReadByte(),  // a range of different values                      ( 30 possible values ) 
                floorHeight = br.ReadByte(),    // a range of different values                      ( 206 possible values )
                unknown13 = br.ReadByte(),      // a range of different values                      ( 26 possible values )
                unknown14 = br.ReadByte(),      // a range of different values                      ( 167 possible values )
                Lighting = br.ReadByte(),       // a range of different values                      ( 120 possible values )
                scriptAction = br.ReadByte(),         // only ever 0-41 across every level in the game    ( 42 possible values ) (See Actions)
                Name = "Collision Node " + i
            };
            if(node.scriptAction > noOfInteractables)
            {
                noOfInteractables = node.scriptAction;
            }
            collisions.Add(node);
            /*if (generateCSV)
            {
                string filePath = Application.dataPath + "/" + MapPuller.finder.levelNumber + "ExportedData.csv";
                ExportToCSV(collisions, filePath);
                UnityEngine.Debug.Log("CSV file exported to: " + filePath);
            }*/
        }
        // path node formula = number of elements multiplied by 8 - (8 bytes per path node)
        for (int i = 0; i < pathCount; i++)
        {
            PathNode obj = new PathNode
            {
                X = br.ReadByte(),              // x coordinate of the pathing object
                Y = br.ReadByte(),              // y coordinate of the pathing object
                unused = br.ReadByte(),         // only ever 0 across every level in the game - blocked from path script.
                nodeState = br.ReadByte(),      // 0, active node. 2, inactive pending action, 3, one way only.
                NodeA = br.ReadByte(),          // node A of the pathing object
                NodeB = br.ReadByte(),          // node B of the pathing object
                NodeC = br.ReadByte(),          // node C of the pathing object
                NodeD = br.ReadByte(),          // node D of the pathing object
            };
            pathNodes.Add(obj);
        }
        // monster formula = number of elements multiplied by 20 - (20 bytes per monster)
        for (int i = 0; i < monsterCount; i++)
        {
            Monster monster = new Monster
            {
                Type = br.ReadByte(),           // type of the monster
                // Monster Types (0x)
                // 01 - Egg
                // 02 - Face Hugger
                // 03 - Chest Burster
                // 04 - Bambi
                // 05 - Dog Alien
                // 06 - Warrior Drone
                // 07 - Queen
                // 08 - Ceiling Warrior Drone
                // 09 - Ceiling Dog Alien
                // 10 - Colonist
                // 0B - Guard
                // 0C - Soldier
                // 0D - Synthetic
                // 0E - Handler
                // 0F - Value not used in any level ( possibly the player )
                // 10 - Horizontal Steam Vent
                // 11 - Horizontal Flame Vent
                // 12 - Vertical Steam Vent
                // 13 - Vertical Flame Vent
                X = br.ReadByte(),              // x coordinate of the monster
                Y = br.ReadByte(),              // y coordinate of the monster
                Z = br.ReadByte(),              // Definitely not a Z value, only used on L222LEV for the monsters that spawn after first flame vent closed.
                Rotation = br.ReadByte(),       // Byte Direction  Facing
                                                // 00 - North       // Y+
                                                // 01 - North East  // X+ Y+
                                                // 02 - East        // X+
                                                // 03 - South East  // X+ Y-
                                                // 04 - South       // Y-
                                                // 05 - South West  // X- Y-
                                                // 06 - West        // X-
                                                // 07 - North West  // X- Y+
                Health = br.ReadByte(),         // health of the monster
                Drop = br.ReadByte(),           // index of object to be dropped
                unknown2 = br.ReadByte(),       // UNKNOWN
                Difficulty = br.ReadByte(),     // 0 - Easy, 1 - Medium, 2 - Hard
                unknown4 = br.ReadByte(),       // UNKNOWN
                unknown5 = br.ReadByte(),       // UNKNOWN
                unknown6 = br.ReadByte(),       // UNKNOWN
                unknown7 = br.ReadByte(),       // UNKNOWN
                unknown8 = br.ReadByte(),       // UNKNOWN
                Speed = br.ReadByte(),          // speed of the monster
                unknown9 = br.ReadByte(),       // only ever 0 across every level in the game. Commented as unused.
                unknown10 = br.ReadByte(),      // UNKNOWN
                unknown11 = br.ReadByte(),      // UNKNOWN
                unknown12 = br.ReadByte(),      // UNKNOWN
                unknown13 = br.ReadByte(),      // UNKNOWN
                Name = "Monster Spawn " + i,
            };
            monsters.Add(monster);
        }
        // pickup formula = number of elements multiplied by 8 - (8 bytes per pickup)
        for (int i = 0; i < pickupCount; i++)
        {
            Pickup pickup = new Pickup
            {
                X = br.ReadByte(),              // x coordinate of the pickup
                Y = br.ReadByte(),              // y coordinate of the pickup
                Type = br.ReadByte(),           // pickup type
                // Pickup Types (0x)
                // 00 - Pistol
                // 01 - Shotgun
                // 02 - Pulse Rifle
                // 03 - Flame Thrower
                // 04 - Smartgun
                // 05 - Nothing / Unused
                // 06 - Seismic Charge
                // 07 - Battery
                // 08 - Night Vision Goggles
                // 09 - Pistol Clip
                // 0A - Shotgun Cartridge
                // 0B - Pulse Rifle Clip
                // 0C - Grenades
                // 0D - Flamethrower Fuel
                // 0E - Smartgun Ammunition
                // 0F - Identity Tag
                // 10 - Auto Mapper
                // 11 - Hypo Pack
                // 12 - Acid Vest
                // 13 - Body Suit
                // 14 - Medi Kit
                // 15 - Derm Patch
                // 16 - Protective Boots
                // 17 - Adrenaline Burst
                // 18 - Derm Patch
                // 19 - Shoulder Lamp
                // 1A - Shotgun Cartridge       ( Cannot be picked up )
                // 1B - Grenades                ( Cannot be picked up )
                Amount = br.ReadByte(),         // amount of the pickup
                Multiplier = br.ReadByte(),     // multiplier for the pickup
                unknown1 = br.ReadByte(),       // only ever 0 across every level in the game - to be commented out.
                Z = br.ReadByte(),              // only ever 0 or 1 across every level in the game
                unknown2 = br.ReadByte(),       // UNKNOWN - unk2 is always the same as amount for ammunition
                Name = "Pickup " + i,
            };
            pickups.Add(pickup);
        }
        // boxes formula = number of elements multiplied by 16 - (16 bytes per box)
        for (int i = 0; i < boxCount; i++)
        {
            Crate box = new Crate
            {
                X = br.ReadByte(),
                Y = br.ReadByte(),
                Type = br.ReadByte(),
                // My Object Types (int) - indented = unused across all levels                                          Object Status       Minimum Damage  Drop
                // // // less than 20 - a box that cannot be blown up                                                   [INDESTRUCTIBLE]
                // 20 - a regular box that can be blown up ( or an egg husk if in chapter 3 )                           [DESTRUCTIBLE]      PISTOL          YES
                // 21 - destructible walls                                                                              [DESTRUCTIBLE]      GRENADE         YES
                // 22 - another small switch, the difference is at the bottom of the model ( lightning is drawn )       [INDESTRUCTIBLE]
                // 23 - barrel explodes.                                                                                [DESTRUCTIBLE]      SHOTGUN         NO
                // 24 - switch with animation ( small switch )                                                          [INDESTRUCTIBLE]
                // 25 - double stacked boxes ( two boxes on top of each other that can be blown up )                    [DESTRUCTIBLE]      PISTOL          YES
                // 26 - wide switch with zipper                                                                         [INDESTRUCTIBLE]                            Battery required
                // 27 - wide switch without zipper                                                                      [INDESTRUCTIBLE]                            Battery required
                // 28 - an empty object that can be shot                                                                [DESTRUCTIBLE]      PISTOL          NO
                // 29 - Medical locker, seen on L122LEV                                                                 [DESTRUCTIBLE]      PISTOL          YES
                // // // 30 - a regular box that can be blown up                                                        [DESTRUCTIBLE]      PISTOL          YES
                // // // 31 - a regular box that can be blown up                                                        [DESTRUCTIBLE]      PISTOL          YES
                // 32 - Strange Little Yellow Square                                                                    [INDESTRUCTIBLE]
                // 33 - Steel Coil                                                                                      [INDESTRUCTIBLE]
                // // // 34 - Strange Unused Shape                                                                      [INDESTRUCTIBLE]
                // // // 35 - Light Pylon With No Texture, Completely Red...                                            [INDESTRUCTIBLE]
                // // // 36 - Strange Tall Square ( improperly textured )                                               [INDESTRUCTIBLE]
                // // // 37 - Egg Husk Shape ( untextured )                                                             [INDESTRUCTIBLE]
                // // // greater than 37 - a regular box that can be blown up                                           [DESTRUCTIBLE]      PISTOL          YES
                Drop = br.ReadByte(),           // 0 = Pickup 2 = Enemy
                unknown1 = br.ReadByte(),       // UNKNOWN
                unknown2 = br.ReadByte(),       // UNKNOWN - only ever 0 or 10 across every level in the game
                Drop1 = br.ReadByte(),          // index of first pickup dropped
                Drop2 = br.ReadByte(),          // index of second pickup dropped
                unknown3 = br.ReadByte(),       // UNKNOWN
                unknown4 = br.ReadByte(),       // UNKNOWN
                unknown5 = br.ReadByte(),       // UNKNOWN
                unknown6 = br.ReadByte(),       // only ever 0 across every level in the game - to be commented out
                unknown7 = br.ReadByte(),       // UNKNOWN
                unknown8 = br.ReadByte(),       // only ever 0 across every level in the game - to be commented out
                Rotation = br.ReadByte(),       // Byte Direction  Facing
                                                // 00 - North   // Y+
                                                // 02 - East    // X+
                                                // 04 - South   // Y-
                                                // 06 - West    // X-
                unknown10 = br.ReadByte()       // only ever 0 across every level in the game - to be commented out
            };
            boxes.Add(box);
        }
        // doors formula = value multiplied by 8 - (8 bytes one element)
        for (int i = 0; i < doorCount; i++)
        {
            Door door = new Door
            {
                X = br.ReadByte(),              // x coordinate of the door
                Y = br.ReadByte(),              // y coordinate of the door
                unknown = br.ReadByte(),        // UNKNOWN - only ever 64 or 0 across every level in the game
                Time = br.ReadByte(),           // door open time - If zero, can be manually closed at the collider.
                LockState = br.ReadByte(),            // door tag (Lock state, 1 unlocked, 2 locked, shootable = number of shots confirmed)
                unknown2 = br.ReadByte(),       // only ever 0 across every level in the game - Probably remove from spawn script
                Rotation = br.ReadByte(),       // Byte Direction  Facing
                                                // 00 - North   // Y+
                                                // 02 - East    // X+
                                                // 04 - South   // Y-
                                                // 06 - West    // X-
                modelIndex = br.ReadByte()      // index of the door model in the BND file
            };
            doors.Add(door);
        }
        // lifts formula = value multiplied by 16 - (16 bytes one element)
        for (int i = 0; i < liftCount; i++)
        {
            Lifts lift = new Lifts
            {
                X = br.ReadByte(),              // x coordinate of the lift
                Y = br.ReadByte(),              // y coordinate of the lift
                Z = br.ReadByte(),              // z coordinate of the lift
                unknown1 = br.ReadByte(),       // this byte is always 0, 1, 2, 3, 4, 5 or 6 across every level in the game
                unknown2 = br.ReadByte(),       // this byte is always 0 across every level in the game
                unknown3 = br.ReadByte(),       // this byte is always 24, 27, 31, 32, 43, 44, 46, 47, 48, 50, 52, 56, 63, 64, 68, 79, 80, 84, 95 or 224 across every level in the game
                unknown4 = br.ReadByte(),       // this byte is always 1 across every level in the game
                unknown5 = br.ReadByte(),       // this byte is always 1, 4, 5 or 17 across every level in the game
                unknown6 = br.ReadByte(),       // this byte is always 0, 1 or 60 across every level in the game
                unknown7 = br.ReadByte(),       // this byte is always 0, 30, 50, 60, 90, 120, 150, 190, 210, 240 or 255 across every level in the game
                unknown8 = br.ReadByte(),       // this byte is always 1, 2, 3, 4, 5, 6, 7, 10, 25 or 35  across every level in the game - For shootables, number of shots. Must correspond with action byte 3.
                unknown9 = br.ReadByte(),       // this byte is always 0 across every level in the game
                unknown10 = br.ReadByte(),      // this byte is always 0, 1, 2, 3, 4, 5, 6, 7, 8 or 9 across every level in the game
                unknown11 = br.ReadByte(),      // this byte is always 0, 1, 2, 3, 4, 5, 6, 7 or 8 across every level in the game ( these three bytes always match )
                unknown12 = br.ReadByte(),      // this byte is always 0, 1, 2, 3, 4, 5, 6, 7 or 8 across every level in the game ( these three bytes always match )
                unknown13 = br.ReadByte(),      // this byte is always 0, 1, 2, 3, 4, 5, 6, 7 or 8 across every level in the game ( these three bytes always match )
            };
            lifts.Add(lift);
        }
        //br.BaseStream.Seek(4, SeekOrigin.Current); // First 4 bytes here are always 0,255,255,0, which is action group zero
        for (int i = 0; i < 64; i++) //Action Groups referred to by the "Action" byte of a collider. Max Action groups is 64.
        {
            ActionGroup rem = new()
            {
                name = "Action " + i,
                actionType = br.ReadByte(),  // Type of action? 0 = no action, 1 = Standard, 2 = Shootable (must be set with activations) 3 = Mission?
                logicStep = br.ReadByte(),  // Logic Next logic in sequence.
                byte3 = br.ReadByte(),  // Repeatable bool maybe?, possibly activations (matches number of shots for activiation of shootables)
                byte4 = br.ReadByte(),  // Seems to always be zero
            };
            if (rem.actionType != 0)
            {
                actions.Add(rem);
            }
        }
        for (int i = 0; i < 64; i++) //Logic groups following action above.
        {
            LogicGroup rem = new()
            {
                name = "Logic " + i,
                action = br.ReadByte(), // Action to be carried out?
                //0 = lighting change
                //1 = Door Unlock
                //2 = Pickup active/deactive
                //3 = Monster active/deactive
                //4 = Switch set inactive
                //5 = "Locked" Lift Operate (Byte 8 =2) (Possible a "walkable" flag)
                //6 = Door Open
                //7 = Lift operate (If Byte 8 = 2, unlock lift)
                //8 = End Level
                //9 = Path node activate
                nextStep = br.ReadByte(), // If 1 = Next step in sequence 255 stops the script
                modifier = br.ReadByte(), // Modifier to action (i.e type 3, set 1 to spawn, 255 to despawn (i.e steam)
                objectIndex = br.ReadByte(), // Object index
            };
            logics.Add(rem);
        }
        long remainingBytes = br.BaseStream.Length - br.BaseStream.Position;
        for (int i = 0; i < remainingBytes/8; i++) //parse out remainder in 8 byte chunks for testing.
        {
            RemainderBytes rem = new()
            {
                byte1 = br.ReadByte(),
                byte2 = br.ReadByte(),
                byte3 = br.ReadByte(),
                byte4 = br.ReadByte(),
                byte5 = br.ReadByte(),
                byte6 = br.ReadByte(),
                byte7 = br.ReadByte(),
                byte8 = br.ReadByte(),
            };
            remainderBytes.Add(rem);
        }
        string filePath = Application.dataPath + "/" + MapPuller.finder.levelNumber + "ExportedData.csv"; // [ush to CSV for data analysis.
        ExportToCSV(remainderBytes, filePath);
        UnityEngine.Debug.Log("CSV file exported to: " + filePath);
    }
    /*
		Build the map mesh in Unity from duplicated vertices/uvs and triangles
	*/
    private void BuildMapMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Mesh";

        mesh.vertices = meshVertices.ToArray();
        mesh.uv = meshUVs.ToArray();

        mesh.subMeshCount = uvRects.Count;

        Material[] materials = new Material[mesh.subMeshCount];
        foreach (var sub in meshTriangles)
        {
            mesh.SetTriangles(sub.Value, sub.Key);

            Material mat = new Material(baseMaterial);
            mat.name = "Mat_" + sub.Key;
            mat.color = Color.white;
            mat.mainTexture = imgData[sub.Key];
            materials[sub.Key] = mat;
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        child = new GameObject("Map");
        child.transform.parent = this.transform;
        child.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        child.transform.localScale = Vector3.one;

        var mf = child.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        var mr = child.AddComponent<MeshRenderer>();
        mr.materials = materials;

        // Un-mirror
        child.transform.localScale = new Vector3(-1f, 1f, 1f) * scalingFactor;
        //position correctly for ALT co-ord system - To add, offset
        Vector3 pos = new(-mapLength / 2, 2, mapWidth / 2);
        if (mapLength % 2 == 0)
        {
            pos.x += .5f;
        }
        if (mapWidth % 2 == 0)
        {
            pos.z -= .5f;
        }
        child.transform.localPosition = pos;
        child.AddComponent<MeshCollider>(); // ALT uses a spawning patten of Right to Left, Bottom to Top. This aspect corrects Unity spawning at (0,0,0) and allows byte values correct positioning.

        UnityEngine.Debug.Log("mesh.subMeshCount = " + mesh.subMeshCount);
    }
    // Export to CSV
    private void ExportToCSV(List<RemainderBytes> data, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            int index = 0;
            foreach (var row in data)
            {
                string line = string.Join(",", row.byte1, row.byte2, row.byte3, row.byte4, row.byte5, row.byte6, row.byte7, row.byte8);
                writer.WriteLine(line);
                index++;
            }
        }
    }
}
