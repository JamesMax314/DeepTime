using UnityEngine;

public class QuadCreator : MonoBehaviour
{
    public int linearRes = 50;
    public float xLen = 1;
    public float zLen = 1;
    public float peakHeight = 1;
    public Vector3 position = new Vector3(0, 0, 0);
    private Mesh mesh;
    private fluvialErroder fluvial;
    private MeshFilter meshFilter;
    // private MeshCollider meshCollider;
    private geoTerrain terrain;

    public void Start()
    {
        // MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        // meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshFilter = gameObject.AddComponent<MeshFilter>();

        // meshCollider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;

        terrain = new geoTerrain();
        fluvial = new fluvialErroder(ref terrain);
        // Mesh mesh = terrain.genMeshFromHeight();
        fluvial.Errode();
        mesh = fluvial.mTerrain.mesh;
        meshFilter.mesh = mesh;
        // meshCollider.sharedMesh = mesh;
    }

    public void Update()
    {
        // fluvial.Errode();
        // mesh = terrain.mesh;
        // meshFilter.mesh = mesh;
        // meshCollider.sharedMesh = mesh;
    }
}
