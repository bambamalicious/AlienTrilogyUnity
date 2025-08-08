using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Diagnostics.Contracts;



[System.Serializable] // Makes the class visible in the Inspector
public class Monster
{
    public string Name;
    public int Type;   //1= FH Egg 2 = Facehugger, 3 = chestburster 6 = Warrior 8= Praetorian, 10 = body 11 = WY Guard
    public int X;     // correct
    public int Y;     // also correct
    public int Z;     // 255 = floor item
    public int unknown1;
    public int Health; // Unknown what this actually is
    public int Drop;  // - This is the object health
    public int unknown3;
    public int difficulty; // Difficulty level 0 = easy, 1 = normal 2= hard
    public int unknown4;
    public int unknown5;
    public int unknown6;  //Rotation the monster is facing
    public int unknown7;
    public int unknown8;
    public int Speed;    //100 is full speed
    public int unknown9;
    public int unknown10;
    public int unknown11;
    public int unknown12;
    public int unknown13;  //Invulnerability from spawn / release
}

[System.Serializable] // Makes the class visible in the Inspector
public class Crate
{
    public string name;
    public GameObject spawnedObject;
    public int X;
    public int Y;
    public int Type;
    public int Drop;  //if crate, health
    public int unknown1; //2 = enemy spawn, 0 = pickups
    public int unknown2;
    public int Drop1; // - Index of pickup (when correctly calculated.
    public int Drop2;  // - index of second pickup (255 is no pickup)
    public int unknown3;
    public int unknown4;
    public int unknown5;
    public int unknown6;
    public int unknown7;
    public int unknown8;
    public int rotation;  //Rotation of object, seems to be 0-3
    public int unknown10;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Pickup
{
    public string name;
    public GameObject spawnedObject;
    public int x;
    public int y;
    public int type;
    public int amount;
    public int multiplier;
    public int unknown1;
    public int z;
    public int unknown2;
}

public class ObjDataPuller : MonoBehaviour
{

    [Header("Object Lists")]
    public List<Monster> monsters = new();
    public List<Crate> boxes = new();
    public List<Pickup> pickups = new();

    [Header("paths")]
    public string levelPath = ""; // path to the .MAP file
    public string texturePath = "C:\\Program Files (x86)\\Collection Chamber\\Alien Trilogy\\HDD\\TRILOGY\\CD\\L111LEV.B16"; // path to the .B16 file

    [Header("Map Strings")]
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


    public static ObjDataPuller objectPuller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(objectPuller == null)
        {
            objectPuller = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("Pull Object Data")]
    // Update is called once per frame
    public void Initiate(string levelToLoad, string texturesToLoad)
    {
        levelPath = levelToLoad;
        texturePath = texturesToLoad;
        byte[] mapFileData = File.ReadAllBytes(levelPath);
        Debug.Log("Map geometry bytes: " + mapFileData.Length);
        GetData(mapFileData);
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

            map0br.BaseStream.Seek(unknown * 8, SeekOrigin.Current);

            for (int i = 0; i < monsterCount; i++) // 28
            {
                Monster monster = new Monster
                {
                    Name = "Monster Spawn " + i,
                    Type = map0br.ReadByte(),
                    X = map0br.ReadByte(),
                    Y = map0br.ReadByte(),
                    Z = map0br.ReadByte(),
                    unknown1 = map0br.ReadByte(),
                    Health = map0br.ReadByte(),
                    Drop = map0br.ReadByte(),                    
                    unknown3 = map0br.ReadByte(),
                    difficulty = map0br.ReadByte(),
                    unknown4 = map0br.ReadByte(),
                    unknown5 = map0br.ReadByte(),
                    unknown6 = map0br.ReadByte(),
                    unknown7 = map0br.ReadByte(),
                    unknown8 = map0br.ReadByte(),
                    Speed = map0br.ReadByte(),
                    unknown9 = map0br.ReadByte(),
                    unknown10 = map0br.ReadByte(),
                    unknown11 = map0br.ReadByte(),
                    unknown12 = map0br.ReadByte(),
                    unknown13 = map0br.ReadByte(),
                };
                monsters.Add(monster);
            }

            //MessageBox.Show($"Pickups : {ms.Position}"); // 478268 + 20 = 478288 ( L111LEV.MAP )
            // pickup formula = number of elements multiplied by 8 - (8 bytes per pickup)
            for (int i = 0; i < pickupCount; i++) // 28
            {
                Pickup pickup = new Pickup
                {
                    name = "Pickup " + i,
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
                    rotation = map0br.ReadByte(),
                    unknown10 = map0br.ReadByte()
                };
                if (box.Type != 20 && box.Type != 23 && box.Type != 25)
                {
                    boxes.Add(box);
                }
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
}
