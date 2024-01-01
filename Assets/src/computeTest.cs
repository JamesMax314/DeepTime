using UnityEngine;

public class TextureGenerator : MonoBehaviour
{

    public ComputeShader TextureShader;
    private RenderTexture _rTexture;
    private ComputeBuffer inputDataBuffer; // Your ComputeBuffer

    public int linearRes = 50;
    public float xLen = 1;
    public float zLen = 1;
    public float peakHeight = 1;
    public int erosionSteps = 100;
    private int step = 0;
    public Vector3 position = new Vector3(0, 0, 0);
    private Mesh mesh;
    private fluvialErroder fluvial;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private geoTerrain terrain;

    public void Start()
    {
        // MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        // meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshFilter = gameObject.AddComponent<MeshFilter>();

        meshCollider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;

        terrain = new geoTerrain();
        // fluvial = new fluvialErroder(ref terrain);
        Mesh mesh = terrain.genMeshFromHeight();
        // fluvial.Errode();
        // mesh = fluvial.mTerrain.mesh;
        // meshFilter.mesh = mesh;
        // meshCollider.sharedMesh = mesh;

        inputDataBuffer = new ComputeBuffer(terrain.mLinearRes * terrain.mLinearRes, sizeof(float));
        inputDataBuffer.SetData(terrain.heightMap);
    }

    void OnDestroy()
    {
        inputDataBuffer.Release();
    }

    public void Update()
    {
        // if (step < erosionSteps)
        // {
        //     fluvial.Errode();
        //     mesh = terrain.mesh;
        //     meshFilter.mesh = mesh;
        // }
        // if (step == erosionSteps)
        // {
        //     meshCollider.sharedMesh = mesh;
        // }
        // step ++;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (_rTexture == null) { 
            _rTexture = new RenderTexture( terrain.mLinearRes, terrain.mLinearRes, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _rTexture.enableRandomWrite = true;
            _rTexture.Create();
        }
        int kernel = TextureShader.FindKernel("CSMain");
        TextureShader.SetTexture(kernel, "Result", _rTexture);
        TextureShader.SetInt("linearRes", 512);
        TextureShader.SetInt("radius", 4);
        // Formula = Ceil(Screen dimension size / threads per group)
        int workgroupsX = Mathf.CeilToInt(10 / 8.0f);
        int workgroupsY = Mathf.CeilToInt(10 / 8.0f);
        TextureShader.SetBuffer(kernel, "meshHeight", inputDataBuffer);

        TextureShader.Dispatch(kernel, workgroupsX, workgroupsY, 1);
        Graphics.Blit(_rTexture, destination);
    }

}