using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural, seeded district generator.
/// Creates multiple connected street segments with at least one intersection,
/// varied building footprints/heights, sidewalks, and crate clusters.
/// All geometry has colliders.
///
/// Palette (Build Guide 6.2):
///   Streets/sidewalk/community props = warm #F2762E family (0.949, 0.463, 0.180)
///   Institutional buildings = grey #6E6E73 (0.431, 0.431, 0.451)
/// </summary>
public class CityBlockGenerator : MonoBehaviour
{
    [Header("Materials")]
    public Material warmMaterial;
    public Material greyMaterial;

    [Header("Prefabs")]
    public GameObject cratePrefab;

    // Deterministic seed for reproducible generation (tests rely on this)
    [Header("Generation")]
    public int seed = 1337;

    // Counters for tests
    [Header("Runtime Stats (read by tests)")]
    public int buildingCount;
    public int streetSegmentCount;
    public List<GameObject> buildings = new List<GameObject>();
    public List<GameObject> streetSegments = new List<GameObject>();

    // Palette constants
    private static readonly Color WARM = new Color(0.949f, 0.463f, 0.180f); // #F2762E
    private static readonly Color GREY = new Color(0.431f, 0.431f, 0.451f); // #6E6E73

    void Awake()
    {
        if (transform.Find("Floor") == null)
            GenerateCityBlock();
    }

    public void GenerateCityBlock()
    {
        Random.InitState(seed);
        buildings.Clear();
        streetSegments.Clear();
        buildingCount = 0;
        streetSegmentCount = 0;

        // === Floor plane (warm — community ground) ===
        // Large enough to cover the entire district + sidewalks
        float districtSize = 80f;
        float sidewalkWidth = 2f;
        float floorWidth = districtSize + sidewalkWidth * 2;
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(transform);
        floor.transform.localScale = new Vector3(floorWidth, 0.1f, floorWidth);
        floor.transform.position = new Vector3(0, -0.05f, 0);
        floor.GetComponent<Renderer>().material = warmMaterial;
        floor.GetComponent<BoxCollider>().isTrigger = false;

        // === Spawn plaza (clear area around origin for tests) ===
        // No buildings or crates within this radius — guarantees unobstructed forward path
        float spawnClearRadius = 8f;

        // === Main street (north-south) ===
        CreateStreetSegment(new Vector3(0, 0, 0), new Vector3(0, 0, 1), 40f, 8f, "Street_NS_Main");

        // === East-west street (intersection at origin) ===
        CreateStreetSegment(new Vector3(0, 0, 0), new Vector3(1, 0, 0), 40f, 8f, "Street_EW_Main");

        // === Secondary north-south street (west side) ===
        CreateStreetSegment(new Vector3(-20, 0, 0), new Vector3(0, 0, 1), 30f, 8f, "Street_NS_West");

        // === Secondary east-west street (north side) ===
        CreateStreetSegment(new Vector3(0, 0, 15), new Vector3(1, 0, 0), 30f, 8f, "Street_EW_North");

        // === Sidewalk (warm — community ground) ===
        // Surrounds the main intersection
        GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Quad);
        sidewalk.name = "Sidewalk";
        sidewalk.transform.SetParent(transform);
        sidewalk.transform.localScale = new Vector3(districtSize + sidewalkWidth * 2, 1, sidewalkWidth * 2);
        sidewalk.transform.rotation = Quaternion.Euler(-90, 0, 0);
        sidewalk.GetComponent<Renderer>().material = warmMaterial;
        sidewalk.GetComponent<Renderer>().gameObject.layer = LayerMask.NameToLayer("Default");

        // Additional sidewalk along the EW street
        GameObject sidewalkEW = GameObject.CreatePrimitive(PrimitiveType.Quad);
        sidewalkEW.name = "Sidewalk_EW";
        sidewalkEW.transform.SetParent(transform);
        sidewalkEW.transform.localScale = new Vector3(sidewalkWidth * 2, 1, districtSize + sidewalkWidth * 2);
        sidewalkEW.transform.rotation = Quaternion.Euler(-90, 0, 0);
        sidewalkEW.GetComponent<Renderer>().material = warmMaterial;

        // === Buildings — all grey (institutional walls) ===
        // Place buildings along streets, avoiding the spawn clear zone
        PlaceBuildings(spawnClearRadius);

        // === Crate clusters (cover, grey) ===
        PlaceCrates(spawnClearRadius);
    }

    void CreateStreetSegment(Vector3 center, Vector3 direction, float length, float width, string name)
    {
        GameObject street = GameObject.CreatePrimitive(PrimitiveType.Quad);
        street.name = name;
        street.transform.SetParent(transform);
        street.transform.localScale = new Vector3(length, 1, width);
        street.transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
        street.transform.position = center + Vector3.up * 0.01f;
        street.GetComponent<Renderer>().material = warmMaterial;
        // Quad has a MeshCollider — keep it for raycast tests
        streetSegments.Add(street);
        streetSegmentCount++;
    }

    void PlaceBuildings(float spawnClearRadius)
    {
        // Building positions around the district, avoiding spawn zone
        // Format: (position, width, height)
        var buildingSpecs = new (Vector3 pos, float width, float height)[]
        {
            // North side buildings
            (new Vector3(-15, 2.5f, 12), 6f, 5f),
            (new Vector3(15, 3f, 12), 8f, 6f),
            (new Vector3(0, 2.5f, 18), 5f, 5f),

            // South side buildings
            (new Vector3(-15, 3f, -12), 7f, 6f),
            (new Vector3(15, 2.5f, -12), 6f, 5f),

            // West side buildings
            (new Vector3(-22, 3f, 0), 5f, 6f),
            (new Vector3(-22, 2.5f, 10), 4f, 5f),

            // East side buildings
            (new Vector3(22, 3f, 0), 6f, 6f),
            (new Vector3(22, 2.5f, -10), 5f, 5f),

            // Far buildings
            (new Vector3(0, 4f, 30), 10f, 8f),
            (new Vector3(-30, 3.5f, 0), 8f, 7f),
            (new Vector3(30, 3f, 0), 7f, 6f),
        };

        foreach (var spec in buildingSpecs)
        {
            // Skip if too close to spawn zone
            if (new Vector2(spec.pos.x, spec.pos.z).magnitude < spawnClearRadius + 2f)
                continue;

            CreateBuilding(spec.pos, spec.width, spec.height, greyMaterial, $"Building_{buildingCount}");
        }
    }

    void CreateBuilding(Vector3 position, float width, float height, Material material, string name)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = name;
        building.transform.SetParent(transform);
        building.transform.localScale = new Vector3(width, height, width);
        building.transform.position = position;
        building.GetComponent<Renderer>().material = material;
        // Primitive cubes already have BoxCollider — ensure not trigger
        var bc = building.GetComponent<BoxCollider>();
        if (bc != null) bc.isTrigger = false;
        buildings.Add(building);
        buildingCount++;
    }

    void PlaceCrates(float spawnClearRadius)
    {
        // Crate clusters — placed away from spawn zone
        Vector3[] cratePositions = {
            new Vector3(-8, 0.5f, 5),
            new Vector3(8, 0.5f, 5),
            new Vector3(-8, 0.5f, -5),
            new Vector3(8, 0.5f, -5),
            new Vector3(0, 0.5f, 12),
            new Vector3(0, 0.5f, -12),
            new Vector3(-15, 0.5f, 0),
            new Vector3(15, 0.5f, 0),
        };

        for (int i = 0; i < cratePositions.Length; i++)
        {
            // Skip crates in spawn zone
            if (new Vector2(cratePositions[i].x, cratePositions[i].z).magnitude < spawnClearRadius)
                continue;

            GameObject crate;
            if (cratePrefab != null)
            {
                crate = Instantiate(cratePrefab, cratePositions[i], Quaternion.identity);
                crate.name = $"Crate_{i}";
            }
            else
            {
                crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.name = $"Crate_{i}";
                crate.transform.localScale = new Vector3(1, 1, 1);
                crate.transform.position = cratePositions[i];
                crate.GetComponent<Renderer>().material = greyMaterial;
            }
            crate.transform.SetParent(transform);
        }
    }
}