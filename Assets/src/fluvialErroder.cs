using UnityEngine;
using System;

public class fluvialErroder
{
    private geoTerrain mTerrain;
    private int numDrops = 1000;
    private int numSteps = 100;
    public fluvialErroder(ref geoTerrain terrain)
    {
        mTerrain = terrain;
    }

    public void errode()
    {
        for (int dropIndex=0; dropIndex<numDrops; dropIndex++)
        {
            // Random start location
            int xInd = (int)(mTerrain.mLinearRes*UnityEngine.Random.Range(0f, 1f));
            int yInd = (int)(mTerrain.mLinearRes*UnityEngine.Random.Range(0f, 1f));

            float xPos = xInd;
            float yPos = yInd;

            float dx=0, dy=0;

            for (int stepIndex=0; stepIndex<numSteps; stepIndex++)
            {
                float[] grad = ComputeGradientVector(xInd, yInd);
                dx = grad[0];
                dy = grad[1];

                float dl = Mathf.Sqrt(dx*dx + dy*dy);
                dx /= dl;
                dy /= dl;

                float nXPos = xPos+dx;
                float nYPos = yPos+dy;

                int nXInd = (int)Mathf.Floor(xPos);
                int nYInd = (int)Mathf.Floor(yPos);

                float deltaActualNX = nXPos-nXInd;
                float deltaActualNY = nYPos-nYInd;

                float nextHeight = mTerrain.heightMap[nXInd, nYInd];
                float currentHeight = mTerrain.heightMap[xInd, yInd];
            }

        }

    }

    private float[] ComputeGradientVector(int x, int y)
    {
        int xLen = mTerrain.mLinearRes;
        int yLen = mTerrain.mLinearRes;

        float[] gradient = new float[2];

        // Compute gradient in the x direction
        float dx = (x < xLen - 1) ? mTerrain.heightMap[x, y + 1] - mTerrain.heightMap[x, y] : 0;
        gradient[0] = dx;

        // Compute gradient in the y direction
        float dy = (y < yLen - 1) ? mTerrain.heightMap[x + 1, y] - mTerrain.heightMap[x, y] : 0;
        gradient[1] = dy;
        return gradient;
    }
}