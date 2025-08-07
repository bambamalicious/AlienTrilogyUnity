using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    //3D models to instantiate
    [Header("3D Objects")]
    public GameObject dummyObj;
    public Mesh smallCrate;
    public Mesh largeCrate;
    public Mesh barrel;
    public Mesh smallSwitch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    [ContextMenu("SpawnCrates")]
    public void SpawnCrates()
    {
        int index = 1;
        foreach (Crate crate in ObjDataPuller.objectPuller.boxes)
        {
            crate.name = "Object " + index;
            index++;
            Vector3 pos = new Vector3(-crate.X, 0 - 10, crate.Y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation); RaycastHit hit;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            if (crate.Type == 20)
            {
                newObj.GetComponentInChildren<MeshFilter>().mesh = smallCrate;
                crate.spawnedObject = newObj;
                newObj.transform.name = "Crate " + index;
            }
            else if (crate.Type == 25)
            {
                newObj.GetComponentInChildren<MeshFilter>().mesh = largeCrate;
                crate.spawnedObject = newObj;
                newObj.transform.name = "Large Crate " + index;
            }
            else if (crate.Type == 23)
            {
                newObj.GetComponentInChildren<MeshFilter>().mesh = barrel;
                crate.spawnedObject = newObj;
                newObj.transform.name = "Barrel " + index;
            }
            else
            {
                newObj.GetComponentInChildren<MeshFilter>().mesh = smallSwitch;
                crate.spawnedObject = newObj;
                newObj.transform.name = "Switch " + index;
            }
        }
    }

    [ContextMenu("SpawnMobs")]

    public void SpawnMobs()
    {
        int index = 1;
        foreach (Monster monster in ObjDataPuller.objectPuller.monsters)
        {
            monster.Name = "Mob " + index;
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
            newObj.name = pickup.name;
            pickup.spawnedObject = newObj;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
        }
    }
}
