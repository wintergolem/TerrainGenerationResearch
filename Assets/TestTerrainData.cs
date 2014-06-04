using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TestTerrainData : MonoBehaviour {

    //Dictionary<string, float> thresholds;

    public Terrain terrain;
    public MeshRenderer mesh;
    TerrainData td;




    float[,] hm;

    public float fRandMin = 0.2f;
    public float fRandMax = 0.8f;
    public int iTimesThroughTheLoop = 3;
    public float smooth;
    public float decay;
    public float iFlat = 0.2f;
    public float fGrassThreshold = 0.5f;
    public float tileSizeBase = 1;
    public float tileLowerBound = -2;
    public float tileUpperBound = 10;

	// Use this for initialization
	void Start () 
    {
        td = terrain.terrainData;
        
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

        if (Input.GetKey(KeyCode.S))
            Smooth();
        if( Input.GetKeyDown( KeyCode.P))
        {
            Perlin();
        }

        if( Input.GetKeyDown(KeyCode.C))
        {
            Texturize();
        }
        if (Input.GetKeyDown(KeyCode.D))
            HeightToTexture();
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

    void Smooth()
    {
        hm = new float[td.heightmapWidth, td.heightmapHeight];
        hm = td.GetHeights(0, 0, td.heightmapWidth, td.heightmapHeight);
        int temp = 0;
        bool btemp = false;
        float tempSmooth = smooth;
        float tempDif = 0;
        while (temp < iTimesThroughTheLoop)
        {
            temp++;
            for (int x = 0; x < td.heightmapWidth; x++)
                for (int y = 0; y < td.heightmapHeight; y++)
                {
                    float h = hm[x, y];

                    //if (h < 0 || h > 1)
                        //print(h.ToString() );

                    int tx = x + 1;
                    int ty = y + 1;
                    if (tx >= td.heightmapWidth)
                        tx -= td.heightmapHeight;
                    if (ty >= td.heightmapHeight)
                        ty -= td.heightmapHeight;
                    
                   // if really close, make the same
                    //print(Mathf.Abs(hm[tx, y] - h).ToString() + "  " + h.ToString() + "   " + hm[tx, y].ToString());
                    if (Mathf.Abs(hm[tx, y] - h) <= fGrassThreshold && hm[tx, y] != h)
                    {
                        h = hm[tx, y];
                        btemp = true;
                    }
                    if (Mathf.Abs(hm[x, ty] - h) <= fGrassThreshold && hm[x, ty] != h)
                    {
                        h = hm[x, ty];
                        btemp = true;
                    }
                    if(!btemp)
                    {
                        //adjust across
                        if (hm[tx, y] < h)
                        {
                            h -= tempSmooth;
                        }
                        else if( hm[tx , y] < h)
                        {
                            h += tempSmooth;
                        }

                        //adjust down
                        if (hm[x, ty] < h)
                            h -= tempSmooth;
                        else if (hm[x, ty] > h)
                            h += tempSmooth;
                    }
                    btemp = false;
                    ////normalize
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
        //print("done smooh");
        td.SetHeights(0, 0, hm);
    }

    void Perlin()
    {
        float[,] heights = new float[td.heightmapWidth, td.heightmapHeight];

        float[] top = new float[td.heightmapWidth];
        float[] left = new float[td.heightmapHeight];
        float tempTile = 0;
        for (int x = 0; x < td.heightmapWidth; x++)
        {
            for (int y = 0; y < td.heightmapHeight; y++)
            {
                //if not bottom or right edge grab random tile
                if (x != td.heightmapWidth && y != td.heightmapHeight)
                {
                    tempTile = tileSizeBase + Random.Range(tileLowerBound, tileUpperBound);
                }
                //right edge, grab left edge
                else if (x == td.heightmapWidth)
                {
                    tempTile = left[x];
                }
                else if (y == td.heightmapHeight)
                {
                    tempTile = top[y];
                }
                else
                    print("custom error 1");

                //left edge, save for later
                if( x == 0)
                {
                    left[y] = tempTile;
                }
                    //top edge, save for later
                else if( y == 0)
                {
                    top[x] = tempTile;
                }

                heights[x, y] = Mathf.PerlinNoise(((float)x / (float)td.heightmapWidth) * tempTile, ((float)y / (float)td.heightmapHeight) * tempTile) / 10.0f;
            }
        }

        td.SetHeights(0, 0, heights);
    }

    void Texturize()
    {
        float[, ,] splatMapData = new float[td.alphamapWidth, td.alphamapHeight, td.alphamapLayers];

        for( int y = 0; y < td.alphamapHeight; y++)
            for( int x = 0; x < td.alphamapWidth ; x++ )
            {
                //normalize coordinates to range between 0-1
                float x2 = (float)x / (float)td.alphamapWidth;
                float y2 = (float)y / (float)td.alphamapHeight;

                //get height of this location
                float height = td.GetHeight(Mathf.RoundToInt(y2 * td.heightmapHeight), Mathf.RoundToInt(x2 * td.heightmapWidth));

                //get normal of this location
                Vector3 normal = td.GetInterpolatedNormal(y, x);

                //get steepness
                float steepness = td.GetSteepness(y, x);

                float[] splatWeights = new float[td.alphamapLayers];

// CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

                // Texture[0] has constant influence
                splatWeights[1] = 0.5f;

                // Texture[1] is stronger at lower altitudes
                splatWeights[3] = Mathf.Clamp01((td.heightmapHeight - height));

                // Texture[2] stronger on flatter terrain
                // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                // Subtract result from 1.0 to give greater weighting to flat surfaces
                splatWeights[0] = 1.0f - Mathf.Clamp01(steepness * steepness / (td.heightmapHeight / 5.0f));

                // Texture[3] increases with height but only on surfaces facing positive Z axis 
                splatWeights[2] = height * Mathf.Clamp01(normal.z);

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < td.alphamapLayers; i++)
                {

                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;

                    // Assign this point to the splatmap array
                    splatMapData[x, y, i] = splatWeights[i];
                }
            }
        

        // Finally assign the new splatmap to the terrainData:
        td.SetAlphamaps(0, 0, splatMapData);
    }

    void HeightToTexture()
    {
        Texture2D texture = new Texture2D(td.heightmapWidth, td.heightmapHeight);
        Color[] colors = new Color[td.heightmapHeight * td.heightmapWidth];
        hm = td.GetHeights(0, 0, td.heightmapWidth, td.heightmapHeight);
        for( int x = 0; x < td.heightmapWidth ; x ++)
        {
            for( int y = 0; y < td.heightmapHeight ; y ++)
            {
                colors[x * td.heightmapWidth + y] = new Color(0, 0, 0);//new Color(hm[x, y], hm[x, y], hm[x, y]);
                //print(x.ToString() + "  " + y.ToString());
            }
        }

        texture.SetPixels(colors);

        mesh.material.SetTexture(0, texture);
    }
}


//// CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

//                // Texture[0] has constant influence
//                splatWeights[0] = 0.5f;

//                // Texture[1] is stronger at lower altitudes
//                splatWeights[1] = Mathf.Clamp01((td.heightmapHeight - height));

//                // Texture[2] stronger on flatter terrain
//                // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
//                // Subtract result from 1.0 to give greater weighting to flat surfaces
//                splatWeights[2] = 1.0f - Mathf.Clamp01(steepness * steepness / (td.heightmapHeight / 5.0f));

//                // Texture[3] increases with height but only on surfaces facing positive Z axis 
//                splatWeights[3] = height * Mathf.Clamp01(normal.z);

//                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
//                float z = splatWeights.Sum();

//                // Loop through each terrain texture
//                for (int i = 0; i < td.alphamapLayers; i++)
//                {

//                    // Normalize so that sum of all texture weights = 1
//                    splatWeights[i] /= z;

//                    // Assign this point to the splatmap array
//                    splatMapData[x, y, i] = splatWeights[i];