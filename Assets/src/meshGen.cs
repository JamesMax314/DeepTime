using UnityEngine;
using System;

public class geoTerrain
{
    public int mLinearRes;
    public float mXLen;
    public float mZLen;
    private float mPeakHeight;
    public float[,] heightMap;
    public Mesh mesh;

    public geoTerrain()
    {
        mLinearRes = 512;
        mXLen = (float)1e4;
        mZLen = (float)1e4;
        mPeakHeight = (float)4e3;
        mesh = new Mesh();
        genHeights();
    }

    public geoTerrain(int linearRes, float xLen, float zLen, float peakHeight)
    {
        mLinearRes = linearRes;
        mXLen = xLen;
        mZLen = zLen;
        mPeakHeight = peakHeight;
        mesh = new Mesh();
        genHeights();
    }

    public Mesh genMeshFromHeight()
    {
        if (mLinearRes > 255) 
        {
            // Set to 32-bit indices to support high resolution meshes
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        float xStep = mXLen / mLinearRes;
        float zStep = mZLen / mLinearRes;
        Vector3[] vertices = new Vector3[mLinearRes*mLinearRes];
        Vector2[] uvs = new Vector2[mLinearRes*mLinearRes];
        Color[] colors = new Color[mLinearRes*mLinearRes];

        for (int i=0; i<mLinearRes; i++)
        {
            float xPos = -mXLen/2+i*xStep;
            for (int j=0; j<mLinearRes; j++) {
                float zPos = -mZLen/2+j*zStep;
                float yPos = Mathf.Max(0, Mathf.Min(1, heightMap[i, j]))*mPeakHeight;
                vertices[i*mLinearRes+j] = new Vector3(xPos, yPos, zPos);
                uvs[i*mLinearRes+j] = new Vector2((float)i/mLinearRes, (float)j/mLinearRes);
                colors[i*mLinearRes+j] = Color.black;
            }
        }

        int numIndices = (mLinearRes-1)*(mLinearRes-1)*2*3;
        int[] indices = new int[numIndices];
        int count = 0;
        for (int i=0; i<mLinearRes-1; i++) {
            for (int j=0; j<mLinearRes-1; j++) {
                indices[count] = i*mLinearRes + j;
                indices[count+1] = i*mLinearRes + j+1;
                indices[count+2] = (i+1)*mLinearRes+j;            
                
                indices[count+3] = mLinearRes*mLinearRes - (i*mLinearRes + j) - 1;
                indices[count+4] = mLinearRes*mLinearRes - (i*mLinearRes + j+1) - 1;
                indices[count+5] = mLinearRes*mLinearRes - ((i+1)*mLinearRes+j) - 1;
                count += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void genHeights()
    {
        heightMap = new float[mLinearRes, mLinearRes];
        float xStep = mXLen / mLinearRes;
        float zStep = mZLen / mLinearRes;

        float fractalOffset = mLinearRes*UnityEngine.Random.Range(0f, 1f);

        for (int i=0; i<mLinearRes; i++)
        {
            float xPos = mXLen/2+i*xStep;
            for (int j=0; j<mLinearRes; j++) {
                float zPos = mZLen/2+j*zStep;
                float height = FractalNoise(xPos/mXLen+fractalOffset, zPos/mZLen+fractalOffset);
                heightMap[i, j] = height;
            }
        }

        Debug.Log(GetMaxValue(heightMap));
        Debug.Log(GetMinValue(heightMap));
    }

    private float FractalNoise(float x, float y){
        int numFreq = 4;
        float result = 0;
        float weightSum = 0;
        for (int i=0; i<numFreq; i++)
        {
            float freq = Mathf.Pow(2, i+1);
            result += Mathf.PerlinNoise((x+mXLen)*freq, (y+mZLen)*freq)/freq;
            weightSum += 1/freq;
        }

        return result/weightSum;
    }

    float GetMaxValue(float[,] array)
    {
        // Initialize max with the minimum possible integer value
        float max = float.MinValue;

        // Iterate through the 2D array
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                // Compare the current element with the current max
                if (array[i, j] > max)
                {
                    // Update max if the current element is greater
                    max = array[i, j];
                }
            }
        }

        return max;
    }

    float GetMinValue(float[,] array)
    {
        // Initialize max with the minimum possible integer value
        float max = float.MaxValue;

        // Iterate through the 2D array
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                // Compare the current element with the current max
                if (array[i, j] < max)
                {
                    // Update max if the current element is greater
                    max = array[i, j];
                }
            }
        }

        return max;
    }
}