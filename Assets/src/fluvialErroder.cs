using UnityEngine;
using System;

public class fluvialErroder
{
    public geoTerrain mTerrain;
    private int numDrops = 1000;
    private int numSteps = 1000;
    public fluvialErroder(ref geoTerrain terrain)
    {
        mTerrain = terrain;
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
        // mTerrain.mesh.colors = colors;
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