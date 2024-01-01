using UnityEngine;
using System;

public class fluvialErroder
{
    public geoTerrain mTerrain;
    private int numDrops = 1000;
    private int numSteps = 64;
    public int radius = 8;
    public fluvialErroder(ref geoTerrain terrain)
    {
        mTerrain = terrain;
    }
    
    public void Errode()
    {
        float vel;
        float water;
        float sediment;
        float carryCapacity;

        float height;
        float heightNew;

        // Parameters
        float Pintertia = 0.3f; // Inertia
        float Pminslope = 0.01f; // Allows errosion of flat regions
        float Pcapacity = 8f; // Carry capacity modifier
        float Pdeposition = 0.002f; // Fraction of eccess sediment that is dropped by deposition
        float Perrosion = 0.7f; // Fraction of remaining carry capacity that is taken up by errosion
        float Pgravity = 10f; // Controlls water speedup
        float Pevaporation = 0.02f; // Evaporation removes water from drop

        Vector2 pos = new Vector2();
        Vector2 posNew = new Vector2();
        Vector2 dir = new Vector2();
        Vector2 grad = new Vector2();

        Color[] colors = new Color[mTerrain.mLinearRes*mTerrain.mLinearRes];
        Array.Fill(colors, Color.black);

        for (int dropIndex=0; dropIndex<numDrops; dropIndex++)
        {
            vel = 0;
            water = 1;
            sediment = 0;
            dir.x = 0;
            dir.y = 0;

            // init drop start pos
            int xInd = (int)(mTerrain.mLinearRes*UnityEngine.Random.Range(0f, 1f));
            int zInd = (int)(mTerrain.mLinearRes*UnityEngine.Random.Range(0f, 1f));
            pos.x = xInd;
            pos.y = zInd;


            for (int stepIndex=0; stepIndex<numSteps; stepIndex++)
            {
                if (!CheckPos(pos))
                {
                    break;
                }

                height = getInterpH(pos);
                grad = getInterpGrad(pos).normalized;


                // Compute new direction of motion taking inertia into account
                dir = (dir*Pintertia - grad*(1-Pintertia)).normalized;
                posNew = pos + dir;

            
                // Compute new interpolated height
                heightNew = getInterpH(posNew);
                float deltaHeight = heightNew - height;


                // If enters pit deposit to overcome
                if (deltaHeight > Pminslope)
                {
                    if (sediment < deltaHeight)
                    {
                        // Drop all sediment
                        DepositeLocal(pos, sediment);
                        break;
                    }

                    // Deposite deltaHeight 
                    DepositeLocal(pos, deltaHeight);
                    sediment -= deltaHeight;
                } else {
                    // else compute new carry capacity
                    carryCapacity = Mathf.Max(-deltaHeight, Pminslope)*vel*water*Pcapacity;

                    if (sediment > carryCapacity)
                    {
                        // if sediment > carry capacity deposite some fraction
                        float toDeposite = (sediment - carryCapacity)*Pdeposition;
                        DepositeLocal(pos, toDeposite);
                        sediment -= toDeposite;
                    } else {
                        // if sediment < carry capacity take up some fraction
                        // Never take up more sediment than height difference
                        float toErrode = Mathf.Min((carryCapacity-sediment)*Perrosion, -deltaHeight);
                        DepositHeight(pos, -toErrode);
                        sediment += toErrode;
                    }
                }

                // Update speed of motion
                vel = Mathf.Sqrt(vel*vel - deltaHeight*Pgravity);
                // Evaporate water from drop
                water *= 1-Pevaporation;
                pos = posNew;


                // int drawWidth = 1;
                // for (int i=0; i<drawWidth; i++)
                // {
                //     for (int j=0; j<drawWidth; j++) {
                //         int ix = (int)pos.x+i;
                //         int iz = (int)pos.y+j;
                //         if (ix >=0 && ix < mTerrain.mLinearRes && iz >=0 && iz < mTerrain.mLinearRes)
                //         {
                //             colors[ix*mTerrain.mLinearRes+iz].r += 1f/(float)numSteps;
                //         }
                //     }
                // }
            }
        } 
        mTerrain.genMeshFromHeight();
        mTerrain.mesh.colors = colors;
    }

    private bool CheckPos(Vector2 pos)
    {
        bool safe = true;
        if (pos.x < 0 || pos.x >= mTerrain.mLinearRes || pos.y < 0 || pos.y >= mTerrain.mLinearRes)
        {
            safe = false;
        }
        return safe;
    }

    private void DepositeLocal(Vector2 pos, float deltaHeight)
    {
        // Get closest grid point
        int ix = Mathf.FloorToInt(pos.x);
        int iy = Mathf.FloorToInt(pos.y);

        // Offsets to actual position
        float u = pos.x - ix;
        float v = pos.y - iy;

        // Extrapolate deposition
        DepositAt(new Vector2(ix, iy), deltaHeight*(1-u)*(1-v));
        DepositAt(new Vector2(ix+1, iy), deltaHeight*(u)*(1-v));
        DepositAt(new Vector2(ix, iy+1), deltaHeight*(1-u)*(v));
        DepositAt(new Vector2(ix+1, iy+1), deltaHeight*(u)*(v));
    }

    private void DepositHeight(Vector2 pos, float deltaHeight)
    {
        // Get closest grid point
        int ix = Mathf.FloorToInt(pos.x);
        int iy = Mathf.FloorToInt(pos.y);

        float[,] weights = new float[radius, radius];
        float weightSum = 0;

        int halfRad = Mathf.FloorToInt(radius);

        for (int i=0; i<radius; i++)
        {
            for (int j=0; j<radius; j++)
            {
                Vector2 position = new Vector2(ix+i, iy+j);
                float distance = (position-pos).magnitude;
                weights[i, j] = Mathf.Max(0, radius-distance);
                weightSum += weights[i, j];
            }
        }
        // Debug.Log("WeightSum: "+weightSum);
        if (weightSum > 0) {
            for (int i=0; i<radius; i++)
            {
                for (int j=0; j<radius; j++)
                {
                    Vector2 position = new Vector2(ix+i, iy+j);
                    weights[i, j] /= weightSum;
                    // Debug.Log("weight: "+weights[i, j]);
                    DepositAt(position, deltaHeight*weights[i, j]);
                }
            }
        }
    }

    private void DepositAt(Vector2 pos, float deltaHeight)
    {
        int ix = (int)pos.x;
        int iy = (int)pos.y;

        if (CheckPos(pos))
        {
            mTerrain.heightMap[ix, iy] += deltaHeight;
        }
    }

    private float getInterpH(Vector2 pos)
    {
        // Get closest grid point
        int ix = Mathf.FloorToInt(pos.x);
        int iy = Mathf.FloorToInt(pos.y);

        // Offsets to actual position
        float u = pos.x - ix;
        float v = pos.y - iy;

        // Current heights
        float h00=getH(ix, iy);
        float h10=getH(ix+1, iy);
        float h01=getH(ix, iy+1);
        float h11=getH(ix+1, iy+1);

        // Bilinear interpolated height
        float height = (1-v)*((1-u)*h00 + u*h10) + v*((1-u)*h01 + u*h11);

        return height;
    }

    private Vector2 getInterpGrad(Vector2 pos)
    {
        Vector2 grad = new Vector2();

        // Get closest grid point
        int ix = Mathf.FloorToInt(pos.x);
        int iy = Mathf.FloorToInt(pos.y);

        // Offsets to actual position
        float u = pos.x - ix;
        float v = pos.y - iy;

        // Current heights
        float h00=getH(ix, iy);
        float h10=getH(ix+1, iy);
        float h01=getH(ix, iy+1);
        float h11=getH(ix+1, iy+1);

        // Interpolated gradient
        grad.x = (h10-h00)*(1-v) + (h11-h01)*v;
        grad.y = (h01-h00)*(1-u) + (h11-h10)*u;

        return grad;
    }

    private float getH(int xCoord, int yCoord)
    {
        float height = 1;
        if (xCoord >=0 && xCoord < mTerrain.mLinearRes && yCoord >=0 && yCoord < mTerrain.mLinearRes)
        {
            height = mTerrain.heightMap[xCoord, yCoord];
        }
        return height;
    }

    private void DepositArround(int i, int j, float sediment)
    {
        DepositAt(i, j, sediment*0.25F);
        DepositAt(i+1, j, sediment*0.25F);
        DepositAt(i, j+1, sediment*0.25F);
        DepositAt(i+1, j+1, sediment*0.25F);
    }

    private void DepositAt(int i, int j, float dh)
    {
        mTerrain.heightMap[i, j] += dh;
    }
}