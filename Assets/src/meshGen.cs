using UnityEngine;
using System;

public class MeshGen : MonoBehaviour
{
    public static Mesh create(int linearRes, float xLen, float zLen, Vector3 position)
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
                vertices[i*linearRes+j] = new Vector3(xPos, position[1], zPos);
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

        Vector3[] normals = new Vector3[linearRes*linearRes];
        Array.Fill(normals, -Vector3.forward);

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.normals = normals;

        return mesh;
    }
}