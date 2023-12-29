using UnityEngine;
using System;

public class MeshGen : MonoBehaviour
{
    private static float FractalNoise(float x, float y, float lenX, float lenY){
        int numFreq = 30;
        float result = 0;
        float weightSum = 0;
        for (int i=1; i<numFreq; i++)
        {
            result += Mathf.PerlinNoise(x*i+lenX*i, y*i+lenY*i)*1/i;
            weightSum += 1/i;
        }

        return result/weightSum - weightSum;
    }

    public static Mesh create(int linearRes, float xLen, float zLen, Vector3 position, float peakHeight)
    {
        Mesh mesh = new Mesh();

        float xStep = xLen / linearRes;
        float zStep = zLen / linearRes;
        Vector3[] vertices = new Vector3[linearRes*linearRes];

        for (int i=0; i<linearRes; i++)
        {
            float xPos = position[0]+i*xStep;
            for (int j=0; j<linearRes; j++) {
                float zPos = position[2]+j*zStep;
                float height = FractalNoise(xPos/xLen, zPos/zLen, xLen, zLen)*peakHeight;
                vertices[i*linearRes+j] = new Vector3(xPos, position[1]+height, zPos);
            }
        }

        int numIndices = (linearRes-1)*(linearRes-1)*2*3;
        int[] indices = new int[numIndices];
        int count =0 ;
        for (int i=0; i<linearRes-1; i++) {
            for (int j=0; j<linearRes-1; j++) {
                indices[count] = i*linearRes + j;
                indices[count+1] = i*linearRes + j+1;
                indices[count+2] = (i+1)*linearRes+j;            
                
                indices[count+3] = linearRes*linearRes - (i*linearRes + j) - 1;
                indices[count+4] = linearRes*linearRes - (i*linearRes + j+1) - 1;
                indices[count+5] = linearRes*linearRes - ((i+1)*linearRes+j) - 1;
                count += 6;
            }
        }

        // Vector3[] normals = computeNormals(vertices, indices);
        // Array.Fill(normals, -Vector3.forward);

        mesh.vertices = vertices;
        mesh.triangles = indices;
        // mesh.normals = normals;

        return mesh;
    }

    public static Vector3[] computeNormals(Vector3[] vertices, int[] indices)
    {
        Vector3[] normals = new Vector3[vertices.Length];
        // printf("indices size: %u\n", indices.size());

        for (int i=0; i<(int)(indices.Length/3); i++) {
            Vector3 AB = vertices[indices[3*i+1]] - vertices[indices[3*i]];
            Vector3 AC = vertices[indices[3*i+2]] - vertices[indices[3*i]];
            Vector3 norm = Vector3.Cross(AB, AC).normalized;
            for (int j=0; j<3; j++) {
                normals[indices[3*i+j]] = norm;
            }

        }

        return normals;
    }
}