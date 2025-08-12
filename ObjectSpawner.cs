using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Diagnostics.Contracts;

public class BndSection
{
    public string Name { get; set; } = "";
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

[System.Serializable] // Makes the class visible in the Inspector
public class CollisionNode
{
    public string name;
    public GameObject obj;
    public int unknown1, unknown2, unknown3, unknown4, unknown5, unknown6, unknown7, unknown8, unknown9, unknown10, unknown11, unknown12, unknown13, unknown14, unknown15, unknown16;
}

[System.Serializable] // Makes the class visible in the Inspector
public class PathNode
{
    public string name;
    public GameObject obj;
    public int x, y, unknown1, unknown2, nodeA, nodeB, nodeC, nodeD; 
}

    [System.Serializable] // Makes the class visible in the Inspector
public class Monster
{
    public string Name;
    public GameObject spawnedObj;
    public int Type;   //1= FH Egg 2 = Facehugger, 3 = chestburster 6 = Warrior 8= Praetorian, 10 = body 11 = WY Guard
    public int X;     // correct
    public int Y;     // also correct
    public int Z;     // 255 = floor item
    public int rotation; //rotation. Confirmed
    public int Health; // Unknown what this actually is
    public int Drop;  // - This is the object health
    public int unknown3;
    public int difficulty; // Difficulty level 0 = easy, 1 = normal 2= hard Confirmed
    public int unknown4;
    public int unknown5;
    public int unknown6;  
    public int unknown7;
    public int unknown8;
    public int Speed;    //100 is full speed
    public int unknown9; // unused
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
    public int Drop;  //2 = enemy spawn, 0 = pickups
	public int unknown1;
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

[System.Serializable]

public class Door
{
    public GameObject spawnedObject;
    public int x, y, unknown, time, tag, unknown2, rotation, index;
    public string name;
}

[System.Serializable] // Makes the class visible in the Inspector
public class Lifts
{
    public string name;
    public GameObject spawnedObject;
    public byte x, y, z, unknown1, unknown2, unknown3, unknown4, unknown5, unknown6, unknown7, unknown8, unknown9, unknown10, unknown11, unknown12, unknown13;          // x coordinate of the lift

}

public class ObjDataPuller : MonoBehaviour
{

    [Header("Object Lists")]
    public List<CollisionNode> collisions = new();
    public List<Monster> monsters = new();
    public List<Crate> boxes = new();
    public List<Pickup> pickups = new();
    public List<Door> doors = new();
    public List<Lifts> lifts = new();

    public List<PathNode> pathNodes = new();

    //[Header("paths")]
    private string levelPath = ""; // path to the .MAP file
    private string texturePath = ""; // path to the .B16 file

    [Header("Map Strings")]
    public string unknownMapBytes1;
    public string unknownMapBytes2;
    public string unknownMapBytes3;
    public string unknownMapBytes4;
    public string random1;
    public string random2;
    public string random3;
    public string random4;
    public string random5;
    public string random6;
    //Map strings
    public string mapLengthString, mapWidthString, playerStartXString, playerStartYString, monsterCountString, pickupCountString, boxCountString, doorCountString, playerStartAngleString, liftCountString;


    public Byte[] remainderBytes;

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
        List<BndSection> levelSections = LoadSection(data, "MAP0");
        using var ms = new MemoryStream(levelSections[0].Data);
        using var map0br = new BinaryReader(ms);
        // Read number of vertices
        ushort vertCount = map0br.ReadUInt16();
        Debug.Log("Number of vertices: " + vertCount);
        ushort quadCount = map0br.ReadUInt16();         // Read number of quads
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
        ushort doorCount = map0br.ReadUInt16();             // door count
        doorCountString = doorCount.ToString();          // display door count
        ushort liftCount = map0br.ReadUInt16();             // lift count
        liftCountString = liftCount.ToString();          // display lift count
        ushort playerStart = map0br.ReadUInt16();      // player start angle
        playerStartAngleString = playerStart.ToString();   // display player start angle
        // unknown bytes
        ushort unknown1 = map0br.ReadUInt16();                                 // unknown 1
        unknownMapBytes1 = unknown1.ToString();
        ushort unknownmap2 = map0br.ReadByte();
        unknownMapBytes2 = unknownmap2.ToString();
        ushort unknownmap3 = map0br.ReadByte();
        unknownMapBytes3 = unknownmap3.ToString();

        ushort random1string = map0br.ReadByte();
        random1 = random1string.ToString();
        ushort random2string = map0br.ReadByte();
        random2 = random2string.ToString();
        ushort random3string = map0br.ReadByte();
        random3 = random3string.ToString();
        ushort random4string = map0br.ReadByte();
        random4 = random4string.ToString();
        ushort random5string = map0br.ReadByte();
        random5 = random5string.ToString();
        ushort random6string = map0br.ReadByte();
        random6 = random6string.ToString();
        // unknown 3 & 4
        // vertice formula - multiply the value of these two bytes by 8 - (6 bytes for 3 points + 2 bytes zeros)
        map0br.BaseStream.Seek(vertCount * 8, SeekOrigin.Current);
        // quad formula - the value of these 2 bytes multiply by 20 - (16 bytes dot indices and 4 bytes info)
        map0br.BaseStream.Seek(quadCount * 20, SeekOrigin.Current);
        //MessageBox.Show($"{ms.Position}"); // 323148 + 20 = 323168 ( L111LEV.MAP )
        // size formula - for these bytes = multiply length by width and multiply the resulting value by 16 - (16 bytes describe one cell.)
        // collision 16
        //4//2//2//1//1//1//1//2//1//1
        //map0br.BaseStream.Seek((mapLength * mapWidth * 16), SeekOrigin.Current); //skip collision nodes
        int colSections = mapLength * mapWidth;
        for (int i = 0; i < colSections; i++)
        {
            CollisionNode node = new CollisionNode
            {
                unknown1 = map0br.ReadInt32(),
                //unknown2 = map0br.ReadByte(),
                //unknown3 = map0br.ReadByte(),
                //unknown4 = map0br.ReadByte(),
                unknown5 = map0br.ReadByte(),
                unknown6 = map0br.ReadByte(),
                unknown7 = map0br.ReadByte(),
                unknown8 = map0br.ReadByte(),
                unknown9 = map0br.ReadByte(),
                unknown10 = map0br.ReadByte(),
                unknown11 = map0br.ReadByte(),
                unknown12 = map0br.ReadByte(),
                unknown13 = map0br.ReadByte(),
                unknown14 = map0br.ReadByte(),
                unknown15 = map0br.ReadByte(),
                unknown16 = map0br.ReadByte(),
                name = "Collision Node " + i
            };
            collisions.Add(node);
        }

        for (int i = 0; i < unknown; i++)
        {
            PathNode obj = new PathNode
            {
                x = map0br.ReadByte(),
                y = map0br.ReadByte(),
                unknown1 = map0br.ReadByte(),
                unknown2 = map0br.ReadByte(),
                nodeA = map0br.ReadByte(),
                nodeB = map0br.ReadByte(),                    
                nodeC = map0br.ReadByte(),
                nodeD = map0br.ReadByte(),
            };
            pathNodes.Add(obj);
        }
        //map0br.BaseStream.Seek(unknown * 8, SeekOrigin.Current);

        for (int i = 0; i < monsterCount; i++) // 28
        {
            Monster monster = new Monster
            {
                Name = "Monster Spawn " + i,
                Type = map0br.ReadByte(),
                X = map0br.ReadByte(),
                Y = map0br.ReadByte(),
                Z = map0br.ReadByte(),
                rotation = map0br.ReadByte(),
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
                Drop = map0br.ReadByte(),
                unknown1 = map0br.ReadByte(),
                unknown2 = map0br.ReadByte(),
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
            boxes.Add(box);
        }
         for (int i = 0; i < doorCount; i++) // 6 -> 6 doors in L111LEV.MAP
        {

            Door door = new Door
            {
                x = map0br.ReadByte(),
                y = map0br.ReadByte(),
                unknown = map0br.ReadByte(),
                time = map0br.ReadByte(),
                tag = map0br.ReadByte(),
                unknown2 = map0br.ReadByte(),
                rotation = map0br.ReadByte(),
                index = map0br.ReadByte()
            };
            doors.Add(door);
        }

        for (int i = 0; i < liftCount; i++) // 16 doors in L141LEV.MAP
		{
            Lifts lift = new Lifts
            {
                x = map0br.ReadByte(),
                y = map0br.ReadByte(),
                z = map0br.ReadByte(),
                unknown1 = map0br.ReadByte(),
                unknown2 = map0br.ReadByte(),
                unknown3 = map0br.ReadByte(),   
                unknown4 = map0br.ReadByte(),
                unknown5 = map0br.ReadByte(),
                unknown6 = map0br.ReadByte(),
                unknown7 = map0br.ReadByte(),
                unknown8 = map0br.ReadByte(),
                unknown9 = map0br.ReadByte(),
                unknown10 = map0br.ReadByte(),
                unknown11 = map0br.ReadByte(),
                unknown12 = map0br.ReadByte(),
                unknown13 = map0br.ReadByte(),
            };
            lifts.Add(lift);
		}
        long remainingBytes = map0br.BaseStream.Length - map0br.BaseStream.Position;
        remainderBytes = map0br.ReadBytes((int)remainingBytes);
    }

    private List<BndSection> LoadSection(byte[] bnd, string section)
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

    private void ExportToCSV(List<CollisionNode> data, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var row in data)
            {
                string line = string.Join(",", row.name,row.unknown1,row.unknown5,row.unknown6, row.unknown7, row.unknown8, row.unknown9, row.unknown10, row.unknown11, row.unknown12, row.unknown13, row.unknown14, row.unknown15, row.unknown16);
                writer.WriteLine(line);
            }
        }
    }
}
