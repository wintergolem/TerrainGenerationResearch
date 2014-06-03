using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestTerrainData : MonoBehaviour {

    //Dictionary<string, float> thresholds;

    public Terrain terrain;
    TerrainData td;




    float[,] hm;

    public float fRandMin = 0.2f;
    public float fRandMax = 0.8f;
    public int iTimesThroughTheLoop = 3;
    public float smooth;
    public float decay;
    public float iFlat = 0.2f;
    public float grassThreshold = 0.5f;

	// Use this for initialization
	void Start () 
    {
        td = terrain.terrainData;
        td.
	}
	
	// Update is called once per frame
	void Update () {
	
        if(Input.GetKeyDown(KeyCode.R))
        {
            float f = td.GetHeight(0,0);
            print(f.ToString());
           
        }

        if( Input.GetKeyDown(KeyCode.E))
        {
            hm = new float[td.heightmapWidth, td.heightmapHeight];
            for (int x = 0; x < td.heightmapWidth; x++)
                for (int y = 0; y < td.heightmapHeight; y++)
                {
                    hm[x, y] = iFlat;
                }
            td.SetHeights(0, 0, hm);
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            BottomLine();
        }

        if( Input.GetKeyDown(KeyCode.C))
        {

        }
	}

    void BottomLine()
    {
        hm = new float[td.heightmapWidth, td.heightmapHeight];
        for (int x = 0; x < td.heightmapWidth; x++)
            for (int y = 0; y < td.heightmapHeight; y++)
            {
                hm[x, y] = Random.Range(fRandMin, fRandMax);
            }
        int temp = 0;
        float tempSmooth = smooth;
        print("Starting");
        while (temp < iTimesThroughTheLoop)
        {
            temp++;
            for (int x = 0; x < td.heightmapWidth; x++)
                for (int y = 0; y < td.heightmapHeight; y++)
                {
                    float h = hm[x, y];
                    int tx = x + 1;
                    int ty = y + 1;
                    if (tx >= td.heightmapWidth)
                        tx -= td.heightmapHeight;
                    if (ty >= td.heightmapHeight)
                        ty -= td.heightmapHeight;

                    //adjust across
                    if (hm[tx, y] < h)
                    {
                        h -= tempSmooth;
                    }
                    else
                    {
                        h += tempSmooth;
                    }

                    //adjust down
                    if (hm[x, ty] < h)
                        h -= tempSmooth;
                    else
                        h += tempSmooth;

                    //normalize
                    if (h >= 1)
                        h = 0.9f;
                    else if (h <= 0)
                        h = 0.1f;
                    hm[x, y] = h;
                }
            tempSmooth -= decay;
            if (tempSmooth <= 0)
                break;
        }
        td.SetHeights(0, 0, hm);
        print("done");
    }

    void Texture()
    {
        hm = new float[td.heightmapWidth, td.heightmapHeight];
        for (int x = 0; x < td.heightmapWidth; x++)
            for (int y = 0; y < td.heightmapHeight; y++)
            {
                
            }
    }
}
