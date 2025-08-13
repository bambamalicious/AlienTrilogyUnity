using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    //3D models to instantiate
    //[Header("3D Objects")]
    private GameObject dummyObj;
    public GameObject colObj;
    //public Mesh smallCrate, largeCrate, barrel, smallSwitch;
    //public Material switchMaterial;
    //public Mesh pistolClip, shotgunShell, pistol, shotgun, dermPatch,autoMap, healthPack, battery;
    private Material objMaterial;
    public static ObjectSpawner spawner;
    private GameObject colFrame, pathCover, crateCover, mobCover, pickupCover, liftCover, doorCover;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        dummyObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        objMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

    }
	
    [ContextMenu("SpawnAll")]
    public void SpawnAll()
    {
        SpawnPaths();
		SpawnCrates();
		SpawnMobs();
		SpawnPickups();
		SpawnLifts();
        SpawnDoors();
        ObjDataPuller.objectPuller.pathNodes.Clear();
        ObjDataPuller.objectPuller.boxes.Clear();
        ObjDataPuller.objectPuller.monsters.Clear();
        ObjDataPuller.objectPuller.pickups.Clear();
        ObjDataPuller.objectPuller.lifts.Clear();
        ObjDataPuller.objectPuller.doors.Clear();
    }

    [ContextMenu ("ClearAll")]

    public void ClearAll()
    {
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
        GameObject colCover = Instantiate(new GameObject(), new Vector3(0,0,0), transform.rotation);
        colCover.transform.name = "Collision Nodes";
        int index = 0;
        int xCount = 1;
        int yCount = 0;
        foreach (CollisionNode col in ObjDataPuller.objectPuller.collisions)
        {
            Vector3 pos = new Vector3(xCount+.5f, -10, yCount);
            GameObject newObj = Instantiate(colObj, pos, transform.rotation, colCover.transform);
            newObj.transform.localPosition = pos;
            RaycastHit hit;
            newObj.name = "PathNode " + index;
            col.obj= newObj;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up * 15, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            index++;
            xCount--;
            if (xCount < -int.Parse(ObjDataPuller.objectPuller.mapLengthString)+1)
            {
                yCount++;
                xCount = 0;
            }
        }
    }

    [ContextMenu("Spawn Paths")]
    public void SpawnPaths()
    {
        pathCover = Instantiate(new GameObject(), new Vector3(0,0,0), transform.rotation);
        pathCover.transform.name = "Path Nodes";
        int index = 0;
        dummyObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Material spawnMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        spawnMaterial.color = Color.red;
        foreach (PathNode obj in ObjDataPuller.objectPuller.pathNodes)
        {
            Vector3 pos = new Vector3(-obj.x, 0 - 10, obj.y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation, pathCover.transform); 
            newObj.transform.localPosition = pos;
            newObj.GetComponent<MeshRenderer>().material = spawnMaterial;
            RaycastHit hit;
            newObj.name = "PathNode " + index;
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

    [ContextMenu("Spawn Crates")]
    public void SpawnCrates()
    {
        crateCover = Instantiate(new GameObject(), new Vector3(0,0,0), transform.rotation);
        crateCover.transform.name = "Box Objects";
        int index = 0;
        dummyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Material crateMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        crateMaterial.color = Color.cyan;
        foreach (Crate crate in ObjDataPuller.objectPuller.boxes)
        {
            Vector3 pos = new Vector3(-crate.X-.5f, 0 - 10, crate.Y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation, crateCover.transform);
            newObj.GetComponent<MeshRenderer>().material = crateMaterial;
            newObj.transform.localPosition = pos;
            RaycastHit hit;
            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            switch (crate.Type)
            {
                //case 20: newObj.name = "Crate " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = smallCrate; crate.spawnedObject = newObj; break;
                //case 23: newObj.name = "Barrel " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = barrel; crate.spawnedObject = newObj; break;
                //case 25: newObj.name = "Large Crate " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = largeCrate; crate.spawnedObject = newObj; break;
                default:  newObj.name = "SpawnedObj " + index; crate.spawnedObject = newObj; break;
            }
            index++;
        }
    }

    [ContextMenu("Spawn Mobs")]
    public void SpawnMobs()
    {
        mobCover = Instantiate(new GameObject(), new Vector3(0, 0, 0), transform.rotation);
        mobCover.transform.name = "Actor Spawns";
        int index = 0;
        dummyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Material mobMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mobMaterial.color = Color.yellow;
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
            Vector3 pos = new Vector3(-monster.X-.5f, 0 , monster.Y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation, mobCover.transform);
            newObj.GetComponent<MeshRenderer>().material = mobMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = monster.Name;
            monster.spawnedObj = newObj;
            RaycastHit hit;

            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
        }
    }

    [ContextMenu("Spawn Pickups")]
    public void SpawnPickups()
    {
        pickupCover = Instantiate(new GameObject(), new Vector3(0, 0, 0), transform.rotation);
        pickupCover.transform.name = "Pickup Spawns";
        int index = 0;
        dummyObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Material pickupMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        pickupMaterial.color = Color.blue;
        foreach (Pickup pickup in ObjDataPuller.objectPuller.pickups)
        {
            pickup.name = "Pickup " + index;
            index++;
            Vector3 pos = new Vector3(-pickup.x, 0, pickup.y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation, pickupCover.transform);
            newObj.GetComponent<MeshRenderer>().material = pickupMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = pickup.name;
            pickup.spawnedObject = newObj;
            RaycastHit hit;

            if (Physics.Raycast(newObj.transform.position, newObj.transform.up, out hit))
            {
                if (hit.collider != null)
                {
                    newObj.transform.position = hit.point;
                }
            }
            switch (pickup.type)
            {
                //case 0: newObj.name = "Pistol " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = pistol;  pickup.spawnedObject = newObj; break;
                //case 1: newObj.name = "Shotgun " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = shotgun; pickup.spawnedObject = newObj; break;
                //case 7: newObj.name = "Battery " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = battery; pickup.spawnedObject = newObj; break;
                //case 9: newObj.name = "Pistol Clip " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = pistolClip; pickup.spawnedObject = newObj; break;
                //case 10: newObj.name = "Shotgun Shell " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = shotgunShell; pickup.spawnedObject = newObj; break;
                //case 16: newObj.name = "Auto Map " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = autoMap; pickup.spawnedObject = newObj; break;
                //case 20: newObj.name = "First Aid Kit " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = healthPack; pickup.spawnedObject = newObj; break;
                //case 21: newObj.name = "Dermpatch " + index; newObj.GetComponentInChildren<MeshFilter>().mesh = dermPatch; pickup.spawnedObject = newObj; break;
                default: newObj.name = "SpawnedObj " + index; pickup.spawnedObject = newObj; break;
            }
            
        }
    }

    [ContextMenu("Spawn Lifts")]

    public void SpawnLifts()
    {
        liftCover = Instantiate(new GameObject(), new Vector3(0, 0, 0), transform.rotation);
        liftCover.name = "Lift Objects";
        dummyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Material liftMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        liftMaterial.color = Color.yellow;
        int index = 0;

        foreach (Lifts obj in ObjDataPuller.objectPuller.lifts)
        {
            Vector3 pos = new Vector3(-obj.x, 0 - 10, obj.y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation, liftCover.transform);
            newObj.GetComponent<MeshRenderer>().material = liftMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = "Lift " + index;
            obj.spawnedObject = newObj;
            RaycastHit hit;

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

    [ContextMenu("Spawn Doors")]
    public void SpawnDoors()
    {
        doorCover = Instantiate(new GameObject(), new Vector3(0, 0, 0), transform.rotation);
        doorCover.name = "Doors";
        dummyObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Material doorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        doorMaterial.color = Color.white;
        int index = 0;
        foreach (Door obj in ObjDataPuller.objectPuller.doors)
        {
            Vector3 pos = new Vector3(-obj.x, 0 - 10, obj.y);
            GameObject newObj = GameObject.Instantiate(dummyObj, pos, transform.rotation, doorCover.transform);
            newObj.GetComponent<MeshRenderer>().material = doorMaterial;
            newObj.transform.localPosition = pos;
            newObj.name = "Door " + index;
            obj.spawnedObject = newObj;
            RaycastHit hit;

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
}
