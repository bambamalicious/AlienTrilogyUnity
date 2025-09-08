using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static UnityEngine.Rendering.DebugUI.MessageBox;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

public class ObjectSpawner : MonoBehaviour
{
    //3D models to instantiate
    //[Header("3D Objects")]
    //OBJ3D Meshes
    //0 - Explosive Barrel
    //1 - Single Crate
    //2 - Double Crate
    //3 - Switch Red Left Light
    //4 - Switch Red Right Light
    //5 - Switch Both Lights Off
    //6 - Switch Both Lights Yellow
    //7 - Large Switch Red Left Light
    //8 - Large Switch Red Right Light
    //9 - Large Switch Both Lights Off
    //10 - Large Switch Both Lights Yellow
    //11 - Switch Battery Red Left Light
    //12 - Switch Battery Red Right Light
    //13 - Switch Battery Both Lights Off
    //14 - Switch Battery Both Lights Yellow
    //15 - Large Switch Battery Red Left Light
    //16 - Large Switch Battery Red Right Light
    //17 - Large Switch Battery Both Lights Off
    //18 - Large Switch Battery Both Lights Yellow
    //19 - Boneship Switch Red Left Light
    //20 - Boneship Switch Red Right Light
    //21 - Boneship Switch Both Lights Off
    //22 - Boneship Switch Both Lights Yellow
    //23 - Boneship Switch Red Left Light
    //24 - Boneship Switch Red Right Light
    //25 - Boneship Switch Both Lights Off
    //26 - Boneship Switch Both Lights Yellow
    //27 - Boneship Switch Red Left Light
    //28 - Boneship Switch Red Right Light
    //29 - Boneship Switch Both Lights Off
    //30 - Boneship Switch Both Lights Yellow
    //31 - Boneship Switch Red Left Light
    //32 - Boneship Switch Red Right Light
    //33 - Boneship Switch Both Lights Off
    //34 - Boneship Switch Both Lights Yellow
    //35 - Steel Coil
    //36 - Unused Shape
    //37 - Pylon(Unused )
    //38 - Computer(Unused? )
    //39 - Egg Husk Shape Untextured
    //40 - Stasis Pod Cover
    //41 - Egg Husk
    //PICKMOD Meshes
    //0 - Pistol
    //1 - Shotgun
    //2 - Pulse Rifle
    //3 - Flamethrower
    //4 - Smart Gun
    //5 - Seismic Charge
    //6 - Battery
    //7 - Night Vision Goggles
    //8 - Pistol Clip
    //9 - Shotgun Shell
    //10 - Pulse Rifle Clip
    //11 - Pulse Rifle Grenade
    //12 - Flamethrower Fuel
    //13 - Smart Gun Ammunition
    //14 - ID Badge
    //15 - Auto Mapper
    //16 - Hypo Pack
    //17 - Acid Vest
    //18 - Body Suit
    //19 - Medi Kit
    //20 - Dermpatch
    //21 - Boots
    //22 - Adrenaline Burst
    //23 - Shoulder Lamp
    //24 - Shotgun Ammunition
    //25 - Pistol Shell
    //public Mesh menuJoystick, menuCamera;
    //OPTOBJ Meshes
    //0 - Joystick
    //1 - Camera
    //2 - Gamepad
    //3 - Multitap?
    //4 - Hard Drive Saving<-
    //5 - Hard Drive Loading ->
    //6 - Camera Crossed Out
    //7 - Keyboard
    //8 - Mouse
    //9 - Computer, Monitor and Keyboard
    //10 - Two Linked Computers, Monitors and Keyboards
    //11 - Speaker(Disc )
    //12 - Speaker(Music )
    //13 - Headphones


    public static ObjectSpawner spawner;
    private GameObject colFrame, pathCover, crateCover, mobCover, pickupCover, liftCover, doorCover;
    public GameObject colObj;
    GameObject newObj;

    void Start()
    {
        if (spawner == null)
        {
            spawner = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("SpawnAll")]
    public void SpawnAll()
    {
        SpawnPaths();
        SpawnObjects();
        SpawnMobs();
        SpawnPickups();
        SpawnLifts();
        SpawnDoors();
    }

    [ContextMenu("ClearAll")]
    public void ClearAll()
    {
        AlienTrilogyMapLoader.loader.collisions.Clear();
        AlienTrilogyMapLoader.loader.pathNodes.Clear();
        AlienTrilogyMapLoader.loader.boxes.Clear();
        AlienTrilogyMapLoader.loader.monsters.Clear();
        AlienTrilogyMapLoader.loader.pickups.Clear();
        AlienTrilogyMapLoader.loader.lifts.Clear();
        AlienTrilogyMapLoader.loader.doors.Clear();
        Destroy(AlienTrilogyMapLoader.loader.child);
        Destroy(colFrame);
        Destroy(pathCover);
        Destroy(crateCover);
        Destroy(mobCover);
        Destroy(pickupCover);
        Destroy(doorCover);
        Destroy(liftCover);
    }

    [ContextMenu("Spawn Collisions")]
    public void SpawnCollisions()
    {
        colFrame = new GameObject();
        colFrame.transform.position = new(0, 0, 0);
        colFrame.transform.name = "Collision Nodes";
        int index = 0;
        int xCount = 0;
        int yCount = 0;
        foreach (CollisionNode col in AlienTrilogyMapLoader.loader.collisions)
        {
            if (/*col.unknown1 == 255 ||*/ col.scriptAction == 0)
            {
                count();
                continue;
            }
            Vector3 pos = new(xCount, -10, yCount);
            newObj = Instantiate(colObj, pos, transform.rotation, colFrame.transform);
            newObj.transform.localPosition = pos;
            newObj.name = "Collision Node " + index;
            col.obj = newObj;
            CollisionObj script = newObj.GetComponent<CollisionObj>();
            script.name = newObj.name;
            script.unknown1 = col.unknown1;
            script.unknown2 = col.unknown2;
            script.unknown3 = col.unknown3;
            script.unknown4 = col.unknown4;
            //script.unknown5 = col.unknown5;
            script.unknown6 = col.unknown6;
            script.unknown7 = col.unknown7;
            //script.unknown8 = col.unknown8;
            script.ceilingFog = col.ceilingFog;
            script.floorFog = col.floorFog;
            script.ceilingHeight = col.ceilingHeight;
            script.floorHeight = col.floorHeight;
            script.unknown13 = col.unknown13;
            script.Lighting = col.Lighting;
            script.Action = col.scriptAction;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up * 15, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            count();
        }
        void count()
        {
            xCount--;
            if (xCount <= -AlienTrilogyMapLoader.loader.mapLength)
            {
                yCount++;
                xCount = 0;
            }
            index++;
        }
    }

    [ContextMenu("Spawn Paths")]
    public void SpawnPaths()
    {
        pathCover = new GameObject();
        pathCover.transform.position = new(0, 0, 0); 
        pathCover.transform.name = "Path Nodes";
        int index = 0;
        Material spawnMaterial = new(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.red
        };
        foreach (PathNode obj in AlienTrilogyMapLoader.loader.pathNodes)
        {
            Vector3 pos = new(-obj.X, 0 - 10, obj.Y);
            newObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newObj.transform.parent = pathCover.transform;
            newObj.transform.localPosition = pos;
            newObj.GetComponent<MeshRenderer>().material = spawnMaterial;
            newObj.name = "PathNode " + index;
            obj.obj = newObj;
            pathObj script = newObj.transform.AddComponent<pathObj>();
            script.name = newObj.name;
            script.X = obj.X;
            script.Y = obj.Y;
            script.nodeState = obj.nodeState;
            script.NodeA = obj.NodeA;
            script.NodeB = obj.NodeB;
            script.NodeC = obj.NodeC;
            script.NodeD = obj.NodeD;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            index++;
        }
    }

    [ContextMenu("Spawn Objects")]
    public void SpawnObjects()
    {
        crateCover = new GameObject();
        crateCover.transform.position = new(0, 0, 0);
        crateCover.transform.name = "Objects";
        int index = 0;
        Material crateMaterial = new(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.cyan
        };
        foreach (Crate crate in AlienTrilogyMapLoader.loader.boxes)
        {
            switch (crate.Type)
            {
                case 20: crate.Name = "Crate " + index; break;
                case 21: crate.Name = "Destructible Wall " + index; break;
                case 22: crate.Name = "Small Switch " + index; break;
                case 23: crate.Name = "Explosive Barrel " + index; break;
                case 24: crate.Name = "Animated Switch " + index; break;
                case 25: crate.Name = "Double Crates " + index; break;
                case 26: crate.Name = "Wide Switch " + index; break;
                case 27: crate.Name = "Wide Battery Switch " + index; break;
                case 29: crate.Name = "Medical Locker" + index; break;
                //
                case 32: crate.Name = "Strange Little Yellow Square   " + index; break;
                case 33: crate.Name = "Steel Coil       " + index; break;
            }
            Vector3 pos = new Vector3(-crate.X, 0 - 10, crate.Y);
            newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObj.transform.parent = crateCover.transform;
            newObj.GetComponent<MeshRenderer>().material = crateMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = crate.Name;            
            crate.spawnedObject = newObj;
            boxObj script = newObj.transform.AddComponent<boxObj>();
            script.name = crate.Name;
            script.X = crate.X;
            script.Y = crate.Y;
            script.Type = crate.Type;
            script.Drop = crate.Drop;
            script.unknown1 = crate.unknown1;
            script.unknown2 = crate.unknown2;
            script.Drop1 = crate.Drop1;
            script.Drop2 = crate.Drop2;
            script.unknown3 = crate.unknown3;
            script.unknown4 = crate.unknown4;
            script.unknown5 = crate.unknown5;
            script.unknown6 = crate.unknown6;
            script.unknown7 = crate.unknown7;
            script.unknown8 = crate.unknown8;
            script.Rotation = crate.Rotation;
            script.unknown10 = crate.unknown10;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            index++;
        }
    }

    [ContextMenu("Spawn Mobs")]
    public void SpawnMobs()
    {
        mobCover = new GameObject();
        mobCover.transform.position = new(0, 0, 0);
        mobCover.transform.name = "Monsters";
        int index = 0;
        Material mobMaterial = new(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.yellow
        };
        foreach (Monster monster in AlienTrilogyMapLoader.loader.monsters)
        {
            switch (monster.Type)
            {
                case 1: monster.Name = "Egg " + index; break;
                case 2: monster.Name = "Facehugger " + index; break;
                case 3: monster.Name = "ChestBurster " + index; break;
                case 4: monster.Name = "Dog Alien " + index; break;
                case 5: monster.Name = "ChestBurster " + index; break;
                case 6: monster.Name = "Warrior Drone " + index; break;
                case 7: monster.Name = "Queen " + index; break;
                case 8: monster.Name = "Ceiling Warrior Drone " + index; break;
                case 9: monster.Name = "Ceiling Dog Alien " + index; break;
                case 10: monster.Name = "Colonist " + index; break;
                case 11: monster.Name = "Security Guard " + index; break;
                case 12: monster.Name = "Soldier " + index; break;
                case 13: monster.Name = "Synthetic " + index; break;
                case 14: monster.Name = "Handler " + index; break;
                //case 15: monster.Name = "Player " + index; break;                 //unused possibly player?
                case 16: monster.Name = "Horizontal Steam Vent " + index; break;
                case 17: monster.Name = "Horizontal Flame Vent " + index; break;
                case 18: monster.Name = "Vertical Steam Vent " + index; break;
                case 19: monster.Name = "Vertical Flame Vent " + index; break;
            }
            Vector3 pos = new(-monster.X, 0, monster.Y);
            newObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            newObj.transform.parent = mobCover.transform;
            newObj.GetComponent<MeshRenderer>().material = mobMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = monster.Name;
            monster.spawnedObj = newObj;
            monsterObj script = newObj.transform.AddComponent<monsterObj>();
            script.name = monster.Name;
            script.Type = monster.Type;
            script.X = monster.X;
            script.Y = monster.Y;
            script.Z = monster.Z;
            script.Rotation = monster.Rotation;
            script.Health = monster.Health;
            script.Drop = monster.Drop;
            script.unknown2 = monster.unknown2;
            script.Difficulty = monster.Difficulty;
            script.unknown4 = monster.unknown4;
            script.unknown5 = monster.unknown5;
            script.unknown6 = monster.unknown6;
            script.unknown7 = monster.unknown7;
            script.unknown8 = monster.unknown8;
            script.Speed = monster.Speed;
            script.unknown8 = monster.unknown8;
            //script.unknown9 = monster.unknown9;
            script.unknown10 = monster.unknown10;
            script.unknown11 = monster.unknown11;
            script.unknown12 = monster.unknown12;
            script.unknown13 = monster.unknown13;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            index++;
        }
    }

    [ContextMenu("Spawn Pickups")]
    public void SpawnPickups()
    {
        pickupCover = new GameObject();
        pickupCover.transform.position = new(0, 0, 0);
        pickupCover.transform.name = "Pickups";
        int index = 0;
        Material pickupMaterial = new(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.blue
        };
        foreach (Pickup pickup in AlienTrilogyMapLoader.loader.pickups)
        {
            switch (pickup.Type)
            {
                case 0: pickup.Name = "Pistol " + index; break;
                case 1: pickup.Name = "Shotgun " + index; break;
                case 2: pickup.Name = "Pulse Rifle " + index; break;
                case 3: pickup.Name = "Flame Thrower " + index; break;
                case 4: pickup.Name = "Smartgun " + index; break;
                //case 5: pickup.Name = "Unused " + index; break;
                case 6: pickup.Name = "Seismic Charge " + index; break;
                case 7: pickup.Name = "Battery " + index; break;
                case 8: pickup.Name = "Night Vision Goggles " + index; break;
                case 9: pickup.Name = "Pistol Clip " + index; break;
                case 10: pickup.Name = "Shotgun Cartridge " + index; break;
                case 11: pickup.Name = "Pulse Rifle Clip " + index; break;
                case 12: pickup.Name = "Grenades " + index; break;
                case 13: pickup.Name = "Flamethrower Fuel " + index; break;
                case 14: pickup.Name = "Smartgun Ammunition " + index; break;
                case 15: pickup.Name = "Identity Tag " + index; break;
                case 16: pickup.Name = "Auto Mapper " + index; break;
                case 17: pickup.Name = "Hypo Pack " + index; break;
                case 18: pickup.Name = "Acid Vest " + index; break;
                case 19: pickup.Name = "Body Suit " + index; break;
                case 20: pickup.Name = "Medi Kit " + index; break;
                case 21: pickup.Name = "Derm Patch " + index; break;
                case 22: pickup.Name = "Protective Boots " + index; break;
                case 23: pickup.Name = "Adrenaline Burst " + index; break;
                case 24: pickup.Name = "Derm Patch " + index; break;
                case 25: pickup.Name = "Shoulder Lamp " + index; break;
            }
            Vector3 pos = new(-pickup.X, 0, pickup.Y);
            newObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            newObj.transform.parent = pickupCover.transform;
            newObj.GetComponent<MeshRenderer>().material = pickupMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = pickup.Name;
            pickup.spawnedObject = newObj;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            index++;
        }
    }

    [ContextMenu("Spawn Lifts")]
    public void SpawnLifts()
    {
        liftCover = new GameObject();
        liftCover.transform.position = new(0, 0, 0);
        liftCover.name = "Lifts";
        Material liftMaterial = new(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.yellow
        };
        int index = 0;

        foreach (Lifts obj in AlienTrilogyMapLoader.loader.lifts)
        {
            Vector3 pos = new Vector3(-obj.X, 0 - 10, obj.Y);
            newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObj.transform.parent = liftCover.transform;
            newObj.GetComponent<MeshRenderer>().material = liftMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = "Lift " + index;
            obj.spawnedObject = newObj;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            index++;
        }
    }

    [ContextMenu("Spawn Doors")]
    public void SpawnDoors()
    {
        doorCover = new GameObject();
        doorCover.transform.position = new(0, 0, 0);
        doorCover.name = "Doors";
        Material doorMaterial = new(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.white
        };
        int index = 0;
        foreach (Door obj in AlienTrilogyMapLoader.loader.doors)
        {
            Vector3 pos = new Vector3(-obj.X, 0 - 10, obj.Y);
            newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObj.transform.parent = doorCover.transform;
            newObj.GetComponent<MeshRenderer>().material = doorMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = "Door " + index;
            obj.spawnedObject = newObj;
            DoorObj script = newObj.AddComponent<DoorObj>();
            script.name = obj.name;
            script.X = obj.X;
            script.Y = obj.Y;
            script.unknown = obj.unknown;
            script.Time = obj.Time;
            script.lockState = obj.LockState;
            //script.unknown2 = obj.unknown2; - commented as 0 on all doors so not required.
            script.Rotation = obj.Rotation;
            script.modelIndex = obj.modelIndex;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            index++;
        }
    }
}
