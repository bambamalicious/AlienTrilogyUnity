using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    //3D models to instantiate
    [Header("3D Objects")]
    public GameObject dummyObj;
    public Mesh smallCrate, largeCrate, barrel, smallSwitch;
    public Material switchMaterial;
    public Mesh pistolClip, shotgunShell, pistol, shotgun, dermPatch,autoMap, healthPack, battery;
    public Material objMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    [ContextMenu("SpawnUnknowns")]
    public void SpawnUnknowns()
    {
        int index = 1;
        foreach (UnknownObj obj in ObjDataPuller.objectPuller.unknowns)
        {
            Vector3 pos = new Vector3(-obj.x, 0 - 10, obj.y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation); RaycastHit hit;
            newObj.name = "Unknown " + index;
            obj.obj = newObj;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            index++;
        }
    }

    [ContextMenu("SpawnCrates")]
    public void SpawnCrates()
    {
        int index = 1;
        foreach (Crate crate in ObjDataPuller.objectPuller.boxes)
        {
            Vector3 pos = new Vector3(-crate.X, 0 - 10, crate.Y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation); RaycastHit hit;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            switch (crate.Type)
            {
                case 20: newObj.name = "Crate " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = smallCrate; crate.spawnedObject = newObj; break;
                case 23: newObj.name = "Barrel " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = barrel; crate.spawnedObject = newObj; break;
                case 25: newObj.name = "Large Crate " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = largeCrate; crate.spawnedObject = newObj; break;
                default:  newObj.name = "SpawnedObj " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = smallSwitch; newObj.GetComponentInChildren<MeshRenderer>().material = switchMaterial; crate.spawnedObject = newObj; break;
            }
            index++;
        }
    }

    [ContextMenu("SpawnMobs")]

    public void SpawnMobs()
    {
        int index = 1;
        foreach (Monster monster in ObjDataPuller.objectPuller.monsters)
        {
            switch (monster.Type){
                case 1: monster.Name = "FH Egg " + index; break;
                case 2: monster.Name = "Facehugger " + index; break;
                case 3: monster.Name = "ChestBurster " + index; break;
                case 6: monster.Name = "Warrior " + index; break;
                case 8: monster.Name = "Praetorian " + index; break;
                case 10: monster.Name = "Wall Body " + index; break;
                case 11: monster.Name = "Security Guard " + index; break;
                default: monster.Name = "Mob " + index; break;
            }
            index++;
            Vector3 pos = new Vector3(-monster.X, 0 , monster.Y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation); RaycastHit hit;
            newObj.name = monster.Name;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
        }
    }

    [ContextMenu("SpawnPickups")]

    public void SpawnPickups()
    {
        int index = 1;
        foreach (Pickup pickup in ObjDataPuller.objectPuller.pickups)
        {
            pickup.name = "Pickup " + index;
            index++;
            Vector3 pos = new Vector3(-pickup.x, 0, pickup.y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation); RaycastHit hit;
            newObj.GetComponentInChildren<MeshRenderer>().material = objMaterial;
            newObj.name = pickup.name;
            pickup.spawnedObject = newObj;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            switch (pickup.type)
            {
                case 0: newObj.name = "Pistol " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = pistol;  pickup.spawnedObject = newObj; break;
                case 1: newObj.name = "Shotgun " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = shotgun; pickup.spawnedObject = newObj; break;
                case 7: newObj.name = "Battery " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = battery; pickup.spawnedObject = newObj; break;
                case 9: newObj.name = "Pistol Clip " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = pistolClip; pickup.spawnedObject = newObj; break;
                case 10: newObj.name = "Shotgun Shell " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = shotgunShell; pickup.spawnedObject = newObj; break;
                case 16: newObj.name = "Auto Map " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = autoMap; pickup.spawnedObject = newObj; break;
                case 20: newObj.name = "First Aid Kit " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = healthPack; pickup.spawnedObject = newObj; break;
                case 21: newObj.name = "Dermpatch " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = dermPatch; pickup.spawnedObject = newObj; break;
                default: newObj.name = "SpawnedObj " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = smallSwitch; pickup.spawnedObject = newObj; break;
            }
            
        }
    }
}
