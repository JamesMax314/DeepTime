using UnityEngine;

public class QuadCreator : MonoBehaviour
{
    public int linearRes = 50;
    public float xLen = 1;
    public float zLen = 1;
    public float peakHeight = 1;
    public Vector3 position = new Vector3(0, 0, 0);

    public void Start()
    {
        // MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        // meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshCollider meshCollider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;

        geoTerrain terrain = new geoTerrain();
        fluvialErroder fluvial = new fluvialErroder(ref terrain);
        // fluvial.errode();
        Mesh mesh = terrain.genMeshFromHeight();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void Update()
    {

    }
}
