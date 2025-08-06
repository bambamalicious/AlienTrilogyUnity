using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Diagnostics.Contracts;

/*
	Alien Trilogy Data Loader
	Load data directly from original Alien Trilogy files to use it in Unity
*/

[System.Serializable] // Makes the class visible in the Inspector
public class Monster
{
    public string Type;
    public string X;
    public string Y;
    public string Z;
    public string Health;
    public string Drop;
    public string Speed;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Crate
{
    public string name;
    public GameObject spawnedObject;
    public int X;
    public int Y;
    public int Type;
    public int Drop;
    public int unknown1;
    public int unknown2;
    public int Drop1;
    public int Drop2;
    public int unknown3;
    public int unknown4;
    public int unknown5;
    public int unknown6;
    public int unknown7;
    public int unknown8;
    public int unknown9;
    public int unknown10;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Pickup
{
    public int x;
    public int y;
    public int type;
    public int amount;
    public int multiplier;
    public int unknown1;
    public int z;
    public int unknown2;
}
    public class AlienTrilogyMapLoader : MonoBehaviour
{
    [Header("PATHS")]
    public string levelPath = ""; // path to the .MAP file
    public string texturePath = "C:\\Program Files (x86)\\Collection Chamber\\Alien Trilogy\\HDD\\TRILOGY\\CD\\L111LEV.B16"; // path to the .B16 file

    [Header("SETTINGS")]
    public int textureSize = 256; // pixel dimensions
    public float scalingFactor = 1/512f; // scaling corrections
    public Material baseMaterial;

    [Header("MAP DETAILS")]
    //Map strings
    public string mapLengthString;
    public string mapWidthString;
    public string playerStartXString;
    public string playerStartYString;                               // unknown 1
    public string monsterCountString;
    public string pickupCountString;
    public string boxCountString;
    public string doorCountString;
    public string playerStartAngleString;
    public string unknownMapBytes1;
    public string unknownMapBytes2;

    [Header("Object Lists")]
    public List<Monster> monsters = new();
    public List<Crate> boxes = new();
    public List<Pickup> pickups = new();

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
    public void Initiate(string levelToLoad)
    {
        levelPath = levelToLoad + "L111LEV.MAP";
        byte[] mapFileData = File.ReadAllBytes(levelPath);
        Debug.Log("Map geometry bytes: " + mapFileData.Length);
        GetData(mapFileData);

        // Build map textures
        BuildMapTextures();

        // Build map geometry
        BuildMapGeometry();

        // Build map mesh
        BuildMapMesh();
    }

    /*
		Load a specific byte-section from map data
	*/
    private List<BinaryReader> LoadSection(byte[] data, string sectionName)
    {
        MemoryStream ms = new MemoryStream(data);
        BinaryReader br = new BinaryReader(ms);
        List<BinaryReader> brList = new();

        string formTag = Encoding.ASCII.GetString(br.ReadBytes(4));
        int formSize = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
        string platform = Encoding.ASCII.GetString(br.ReadBytes(4));
        while (br.BaseStream.Position + 8 <= br.BaseStream.Length) // Parse chunks
        {
            string chunkName = Encoding.ASCII.GetString(br.ReadBytes(4));
            int chunkSize = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
            if (br.BaseStream.Position + chunkSize > br.BaseStream.Length) { break; }
            byte[] chunkData = br.ReadBytes(chunkSize);
            if ((chunkSize % 2) != 0) { br.BaseStream.Seek(1, SeekOrigin.Current); } // IFF padding to 2-byte alignment
            if (chunkName.StartsWith(sectionName))
            {
                Debug.Log(sectionName + " > " + chunkName + " > bytes: " + chunkData.Length);
                MemoryStream msChunkData = new MemoryStream(chunkData);
                BinaryReader brChunkData = new BinaryReader(msChunkData);
                brList.Add(brChunkData);
            }
        }

        return brList;
    }

    /*
		Build the map textures
	*/
    private void BuildMapTextures()
    {
        byte[] textureData = File.ReadAllBytes(texturePath);
        Debug.Log("Map texture bytes: " + textureData.Length);

        int imageId = ExtractIdFromGfxFilename(texturePath);

        // Read BX sections
        List<BinaryReader> brList = LoadSection(textureData, "BX");
        uvRects.Clear();

        // Read UV from texture groups BX00-BX04
        foreach (BinaryReader bxbr in brList)
        {
            List<(int X, int Y, int Width, int Height)> rects = new();
            int rectCount = bxbr.ReadInt16();
            for (int j = 0; j < rectCount; j++)
            {
                byte x = bxbr.ReadByte();
                byte y = bxbr.ReadByte();
                byte width = bxbr.ReadByte();
                byte height = bxbr.ReadByte();
                bxbr.ReadBytes(2); // unknown bytes
                rects.Add((x, y, width + 1, height + 1));
            }
            uvRects.Add(rects);
        }

        // Read TP sections
        List<BinaryReader> brImgList = LoadSection(textureData, "TP");
        List<BinaryReader> brPalList = LoadSection(textureData, "CL");
        Debug.Log("brPalList.Count = " + brPalList.Count);

        int imageIndex = 0;
        imgData.Clear();

        // Read texture image data from TP00-TP04
        foreach (BinaryReader tpbr in brImgList)
        {
            byte[] imageBytes = tpbr.ReadBytes((int)tpbr.BaseStream.Length);
            byte[] paletteData = brPalList[imageIndex].ReadBytes((int)brPalList[imageIndex].BaseStream.Length); // Embedded palette

            List<byte> palD = new();
            for (int p = 0; p < paletteData.Length; p++)
            {
                if (p >= 4)
                {
                    palD.Add(paletteData[p]);
                }
            }
            paletteData = Convert16BitPaletteToRGB(palD.ToArray());

            Texture2D texture = RenderRaw8bppImageUnity(imageBytes, paletteData, textureSize, textureSize, imageId, imageIndex);
            texture.name = "Tex_" + imageIndex.ToString("D2");
            imgData.Add(texture);
            Debug.Log($"Loaded texture: {texture.width}x{texture.height}");
            imageIndex++;
        }
    }

    /*
		Extract map id
	*/
    public int ExtractIdFromGfxFilename(string fullPath)
    {
        string fileName = Path.GetFileName(fullPath); // e.g. "111GFX.B16"

        // Match exactly 3 digits at the start of the filename
        var match = Regex.Match(fileName, @"^\d{3}");

        if (match.Success && int.TryParse(match.Value, out int id))
        {
            return id;
        }

        return 0; // Fallback if no match or parse fails
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
        int width,
        int height,
        int mapId,
        int imageIndex)
    {
        // Get transparency for this image
        int[] transparentValues = GetTransparencyValues(mapId, imageIndex);

        // Number of colors in palette
        int numColors = rgbPalette.Length / 3;

        // Create color array
        Color32[] pixels = new Color32[width * height];

        // Create texture object
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Write pixels to texture
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcIndex = y * width + x;
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
                int flippedY = height - 1 - y;
                int dstIndex = flippedY * width + x;
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
        byte[] mapData = File.ReadAllBytes(levelPath);
        Debug.Log("Map geometry bytes: " + mapData.Length);

        // Read MAP0 section
        List<BinaryReader> map0brList = LoadSection(mapData, "MAP0");

        // Clear all lists to avoid artefacts
        meshVertices.Clear();
        meshUVs.Clear();
        meshTriangles.Clear();
        originalVertices.Clear();

        // Build mesh data
        foreach (BinaryReader map0br in map0brList)
        {
            // Read number of vertices
            ushort vertCount = map0br.ReadUInt16();
            Debug.Log("Number of vertices: " + vertCount);

            // Read number of quads
            ushort quadCount = map0br.ReadUInt16();
            Debug.Log("Number of quads: " + quadCount);

            // Skip unused bytes
            map0br.BaseStream.Seek(32, SeekOrigin.Current);

            // Read original vertices
            originalVertices.Clear();
            for (int i = 0; i < vertCount; i++)
            {
                try
                {
                    short x = map0br.ReadInt16();
                    short y = map0br.ReadInt16();
                    short z = map0br.ReadInt16();
                    map0br.ReadBytes(2); // unknown bytes
                    originalVertices.Add(new Vector3(x, y, z));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed at vertex index {i}: {e}");
                    break;
                }
            }

            Debug.Log("originalVertices.Count = " + originalVertices.Count);

            // Read and process quads, duplicating vertices & uvs per quad
            for (int i = 0; i < quadCount; i++)
            {
                int a = 0;
                int b = 0;
                int c = 0;
                int d = 0;
                try
                {
                    a = map0br.ReadInt32();
                    b = map0br.ReadInt32();
                    c = map0br.ReadInt32();
                    d = map0br.ReadInt32();
                    ushort texIndex = map0br.ReadUInt16();
                    byte flags = map0br.ReadByte();
                    byte other = map0br.ReadByte();

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
                        Debug.LogWarning($"Invalid quad indices at quad {i}: {a}, {b}, {c}, {d}, {flags}");
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
                    Debug.LogWarning($"Failed at quad index {i}:  {a}, {b}, {c}, {d} - {e}");
                    break;
                }
            }
        }
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

        GameObject child = new GameObject("Map");
        child.transform.parent = this.transform;
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        var mf = child.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        var mr = child.AddComponent<MeshRenderer>();
        mr.materials = materials;

        // Un-mirror
        child.transform.localScale = new Vector3(-1f, 1f, 1f) * scalingFactor;
        child.transform.localPosition = new Vector3(-int.Parse(mapLengthString) / 2, 0, int.Parse(mapWidthString) / 2);
        child.AddComponent<MeshCollider>();

        Debug.Log("mesh.subMeshCount = " + mesh.subMeshCount);
    }
    public void GetData(byte[] data)
    {

        // Read MAP0 section
        List<BinaryReader> map0brList = LoadSection(data, "MAP0");

        foreach (BinaryReader map0br in map0brList)
        {
            // Read number of vertices
            ushort vertCount = map0br.ReadUInt16();
            Debug.Log("Number of vertices: " + vertCount);

            // Read number of quads
            ushort quadCount = map0br.ReadUInt16();
            Debug.Log("Number of quads: " + quadCount);
            ushort mapLength = map0br.ReadUInt16();
            mapLengthString = mapLength.ToString();
            ushort mapWidth = map0br.ReadUInt16();
            mapWidthString = mapWidth.ToString();            // display map width
            ushort playerStartX = map0br.ReadUInt16();
            playerStartXString = playerStartX.ToString();        // display player start X coordinate
            ushort playerStartY = map0br.ReadUInt16();
            playerStartYString = playerStartY.ToString();    // display player start Y coordinate
            byte unknown = map0br.ReadByte();                   // unknown 1
            map0br.ReadByte();                                 // unknown 2                                                          
            ushort monsterCount = map0br.ReadUInt16();
            monsterCountString = monsterCount.ToString();        // display monster count
            ushort pickupCount = map0br.ReadUInt16();
            pickupCountString = pickupCount.ToString();         // display pickup count
            ushort boxCount = map0br.ReadUInt16();
            boxCountString = boxCount.ToString();           // display box count
            ushort doorCount = map0br.ReadUInt16();
            doorCountString = doorCount.ToString();          // display door count
            ushort unknownmap1 = map0br.ReadByte();                                // unknown 1
            unknownMapBytes1 = unknownmap1.ToString();
            ushort unknownmap2 = map0br.ReadByte();                                // unknown 1
            unknownMapBytes2 = unknownmap1.ToString();                            // unknown 2
            ushort playerStartAngle = map0br.ReadUInt16();
            playerStartAngleString = playerStartAngle.ToString();   // display player start angle

            map0br.ReadBytes(10);                               // unknown 3 & 4
            // vertice formula - multiply the value of these two bytes by 8 - (6 bytes for 3 points + 2 bytes zeros)
            map0br.BaseStream.Seek(vertCount * 8, SeekOrigin.Current);
            // quad formula - the value of these 2 bytes multiply by 20 - (16 bytes dot indices and 4 bytes info)
            map0br.BaseStream.Seek(quadCount * 20, SeekOrigin.Current);
            //MessageBox.Show($"{ms.Position}"); // 323148 + 20 = 323168 ( L111LEV.MAP )
            // size formula - for these bytes = multiply length by width and multiply the resulting value by 16 - (16 bytes describe one cell.)
            // collision 16
            //4//2//2//1//1//1//1//2//1//1
            map0br.BaseStream.Seek((mapLength * mapWidth * 16), SeekOrigin.Current);

            for (int i = 0; i < monsterCount; i++) // 28
            {
                byte type = map0br.ReadByte();
                byte x = map0br.ReadByte();
                byte y = map0br.ReadByte();
                byte z = map0br.ReadByte();
                short health = map0br.ReadInt16();
                byte drop = map0br.ReadByte();
                map0br.ReadBytes(7); // unknown bytes
                short speed = map0br.ReadInt16();
                map0br.ReadBytes(4); // unknown bytes
                Monster monster = new Monster
                {
                    Type = type.ToString(),
                    X = x.ToString(),
                    Y = y.ToString(),
                    Z = z.ToString(),
                    Health = health.ToString(),
                    Drop = drop.ToString(),
                    Speed = speed.ToString()
                };
                monsters.Add(monster);
            }
            //MessageBox.Show($"Pickups : {ms.Position}"); // 478268 + 20 = 478288 ( L111LEV.MAP )
            // pickup formula = number of elements multiplied by 8 - (8 bytes per pickup)
            for (int i = 0; i < pickupCount; i++) // 28
            {
                Pickup pickup = new Pickup 
                {
                x = map0br.ReadByte(),
                y = map0br.ReadByte(),
                type = map0br.ReadByte(),
                amount = map0br.ReadByte(),
                multiplier = map0br.ReadByte(),
                unknown1 = map0br.ReadByte(),// unk1
                z = map0br.ReadByte(),
                unknown2 = map0br.ReadByte(), // unk2
                };
                pickups.Add(pickup);
            }
            map0br.BaseStream.Seek(unknown * 8, SeekOrigin.Current);
            //MessageBox.Show($"Boxes : {ms.Position}"); // 478492 + 20 = 478512 ( L111LEV.MAP )
            // boxes formula = number of elements multiplied by 16 - (16 bytes per box)
            for (int i = 0; i < boxCount; i++) // 44 -> 44 objects in L111LEV.MAP ( Barrels, Boxes, Switches )
            {
                Crate box = new Crate
                {
                    X = map0br.ReadByte(),
                    Y = map0br.ReadByte(),
                    Type = map0br.ReadByte(),
                    unknown1 = map0br.ReadByte(),
                    unknown2 = map0br.ReadByte(),
                    Drop = map0br.ReadByte(),
                    Drop1 = map0br.ReadByte(),
                    Drop2 = map0br.ReadByte(),
                    unknown3 = map0br.ReadByte(),
                    unknown4 = map0br.ReadByte(),
                    unknown5 = map0br.ReadByte(),
                    unknown6 = map0br.ReadByte(),
                    unknown7 = map0br.ReadByte(),
                    unknown8 = map0br.ReadByte(),
                    unknown9 = map0br.ReadByte(),
                    unknown10 = map0br.ReadByte()
                };
                boxes.Add(box);
            }
            //MessageBox.Show($"Doors : {ms.Position}"); // 479196 + 20 = 479216 ( L111LEV.MAP )
            // doors formula = value multiplied by 8 - (8 bytes one element)
            for (int i = 0; i < doorCount; i++) // 6 -> 6 doors in L111LEV.MAP
            {
                byte x = map0br.ReadByte();
                byte y = map0br.ReadByte();
                map0br.ReadByte(); // unk1
                byte time = map0br.ReadByte();
                byte tag = map0br.ReadByte();
                map0br.ReadByte(); // unk2
                byte rotation = map0br.ReadByte();
                byte index = map0br.ReadByte();
                //doors.Add((x, y, time, tag, rotation, index));
            }
        }
    }
}
