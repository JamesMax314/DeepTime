using UnityEngine;
using System;

public class fluvialErroder
{
    public geoTerrain mTerrain;
    private int numDrops = 100;
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
        float Pdeposition = 0.2f; // Fraction of eccess sediment that is dropped by deposition
        float Perrosion = 0.7f; // Fraction of remaining carry capacity that is taken up by errosion
        float Pgravity = 10f; // Controlls water speedup
        float Pevaporation = 0.02f; // Evaporation removes water from drop

        Vector2 pos = new Vector2();
        Vector2 flPos = new Vector2();
        Vector2 posNew = new Vector2();
        Vector2 flooredPosNew = new Vector2();
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
                // Debug.Log(grad.x + ", " + grad.y);


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
                        // Debug.Log("Errode: "+ toErrode);
                        sediment += toErrode;
                    }
                }

                // Update speed of motion
                vel = Mathf.Sqrt(vel*vel - deltaHeight*Pgravity);
                // Evaporate water from drop
                water *= 1-Pevaporation;
                pos = posNew;

                // Debug.Log(sediment);

                int drawWidth = 1;
                for (int i=0; i<drawWidth; i++)
                {
                    for (int j=0; j<drawWidth; j++) {
                        int ix = (int)pos.x+i;
                        int iz = (int)pos.y+j;
                        if (ix >=0 && ix < mTerrain.mLinearRes && iz >=0 && iz < mTerrain.mLinearRes)
                        {
                            colors[ix*mTerrain.mLinearRes+iz].r += 1f/(float)numSteps;
                        }
                    }
                }
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

    public void errode()
    {
        Color[] colors = new Color[mTerrain.mLinearRes*mTerrain.mLinearRes];
        Array.Fill(colors, Color.black);
        for (int dropIndex=0; dropIndex<numDrops; dropIndex++)
        {
            // Random start location
            int xInd = (int)(mTerrain.mLinearRes*UnityEngine.Random.Range(0f, 1f));
            xInd = Mathf.RoundToInt(Mathf.Max(Mathf.Min(xInd, mTerrain.mLinearRes-2), 0));
            int zInd = (int)(mTerrain.mLinearRes*UnityEngine.Random.Range(0f, 1f));
            zInd = Mathf.RoundToInt(Mathf.Max(Mathf.Min(zInd, mTerrain.mLinearRes-2), 0));

            float xPos = xInd;
            float zPos = zInd;

            float ds;

            float dx=0, dz=0, dlMin=0.1f;
            float sediment = 0;
            float v=0, w=1, Kq=10, Kd=0.02F, Kr=0.9F, Kg=10;
            float minSlope = 0F;
            float inertia = 0.1F;

            // Current heights
            float h00=mTerrain.heightMap[xInd, zInd];
            float h10=mTerrain.heightMap[xInd+1, zInd];
            float h01=mTerrain.heightMap[xInd  , zInd+1];
            float h11=mTerrain.heightMap[xInd+1, zInd+1];

            for (int stepIndex=0; stepIndex<numSteps; stepIndex++)
            {

                float h=h00;

                // calc gradient
                float gx=h00+h01-h10-h11;
                float gz=h00+h10-h01-h11;

                // calc next pos
                dx=(dx-gx)*inertia+gx;
                dz=(dx-gx)*inertia+gz;

                float dl = Mathf.Sqrt(dx*dx + dz*dz);

                // if (dl<=dlMin)
                // {
                //     // pick random dir
                //     float a=UnityEngine.Random.Range(0f, 2*Mathf.PI);
                //     dx=Mathf.Cos(a);
                //     dz=Mathf.Sin(a);
                // }
                // else
                // {
                dx/=dl;
                dz/=dl;
                // }

                float nxPos=xPos+dx;
                float nzPos=zPos+dz;

                int nxInd = Mathf.RoundToInt(Mathf.Max(Mathf.Min(nxPos, mTerrain.mLinearRes-2), 0));
                int nzInd = Mathf.RoundToInt(Mathf.Max(Mathf.Min(nzPos, mTerrain.mLinearRes-2), 0));

                if (nxInd == 0 || nxInd == mTerrain.mLinearRes-2 || nzInd == 0 || nzInd == mTerrain.mLinearRes-2)
                {
                    break;
                }

                // Difference between actual point and index
                float nxFloat = nxPos-nxInd;
                float nzFloat = nzPos-nzInd;


                // Next heights
                float nh00=mTerrain.heightMap[nxInd, nzInd];
                float nh10=mTerrain.heightMap[nxInd+1, nzInd];
                float nh01=mTerrain.heightMap[nxInd  , nzInd+1];
                float nh11=mTerrain.heightMap[nxInd+1, nzInd+1];

                // Interpolate height from surrounding heights
                float nh=(nh00*(1-nxFloat)+nh10*nxFloat)*(1-nzFloat)+(nh01*(1-nxFloat)+nh11*nxFloat)*nzFloat;

                // if higher than current, try to deposit sediment up to neighbour height
                if (nh>=h)
                {
                    // Height required to overcome obstacle
                    ds=(nh-h);

                    // If can't make it over
                    if (ds>=sediment)
                    {
                        // deposit all sediment and stop
                        ds=sediment;
                        DepositArround(xInd, zInd, ds);
                        sediment=0;
                        break;
                    }

                    DepositArround(xInd, zInd, ds);
                    sediment-=ds;
                }

                // compute transport capacity
                float dh=h-nh;
                float slope=dh;

                float q=Mathf.Max(slope, minSlope)*v;

                // deposit/erode (don't erode more than dh)
                ds=sediment-q;
                if (ds>=0)
                {
                    // deposit
                    ds*=Kd*0.001F;
                    //ds=minval(ds, 1.0f);
                    if (sediment-ds>0)
                    {
                        DepositArround(xInd, zInd, ds);
                    }
                } else {
                    // erode
                    ds*=Kr*0.001F;
                    // ds=Mathf.Max(ds, -dh*0.99f);
                    // if (ds > -dh)
                    {
                        DepositArround(xInd, zInd, ds);
                    }
                }

                dh-=ds;
                sediment -= ds;
                sediment = Mathf.Max(sediment, 0);

                // v^2 = u^2 + 2as
                v=Mathf.Sqrt(v*v+2*Kg*dh);


                int drawWidth = 2;
                for (int i=0; i<drawWidth; i++)
                {
                    for (int j=0; j<drawWidth; j++) {
                        int ix = xInd+i;
                        int iz = zInd+j;
                        if (ix >=0 && ix < mTerrain.mLinearRes && iz >=0 && iz < mTerrain.mLinearRes)
                        {
                            colors[ix*mTerrain.mLinearRes+iz] = new Color(sediment/50, 0, 0, 1f);
                        }
                    }
                }

                xPos = nxPos;
                zPos = nzPos;
                xInd = nxInd;
                zInd = nzInd;

                h00 = nh00;
                h10 = nh10;
                h01 = nh01;
                h11 = nh11;
            }
        }
        mTerrain.genMeshFromHeight();
        mTerrain.mesh.colors = colors;
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