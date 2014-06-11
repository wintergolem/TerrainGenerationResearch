using UnityEngine;
using System.Collections;

public class SimplexNoiseTest : MonoBehaviour {

    public Terrain terrain;
    public MeshRenderer mesh;
    public Texture2D texture;
    public float floodLevel, mountLevel;

    TerrainData td;
    float[,] map;
   

    /*****************************SIZE DEFINITIONS**/
    public int hGrid = 500;
    public int vGrid = 500;
	// Use this for initialization
	void Start () 
    {
        
        td = terrain.terrainData;
        texture = new Texture2D(td.heightmapWidth, td.heightmapHeight);
        mesh.renderer.material.mainTexture = texture;
	    //seed randow number generator

        hGrid = td.heightmapHeight;
        vGrid = td.heightmapWidth;
    }
	
	// Update is called once per frame
	void Update () 
    {
	    if( Input.GetKeyDown( KeyCode.S))
        {
             map = new float[hGrid , vGrid];

            float min=0 , max=0;

            FillMap( map , ref min ,ref max);

            //assign map to heightmap
            printMap(map, min, max);
        }

	}

    void FillMap( float[,] map , ref float min , ref float max)
    {
        //set up some variables
        int i,j,k,
            octaves = 16;
        // fBm Variables
        float pixel_value, amplitude, frequency, 
            gain = 0.65f, lacuarity = 2.0f;
        //noise variables
        Vector2 disBot;
        Vector2 disMid;
        Vector2 disTop;
        Vector3 noise; // x = b, y = m , z = t / three corners noise contributions
        float tempDis , x, y,
            skew_value , unskew_value,
            general_skew = (Mathf.Sqrt(3.0f) - 1.0f) * 0.5f,
            general_unskew = ( 3.0f - Mathf.Sqrt(3.0f))/6.0f;

        Vector2 cornerBot;
        Vector2 cornerMid;
        Vector2 cornerTop;
        Vector3 grad;

        min = 100000.0f;
	    max = -100000.0f; 

        //set up the gradient table with 8 equally distributed angles around the unit circle
        float[,] gradients = new float[8,2];
        for(i=0; i<8; i++)
        {
            gradients[i,0] = Mathf.Cos( 0.785398163f * (float)i );//0.78.... is PI/4
            gradients[i,1] = Mathf.Sin( 0.785398163f * (float)i );
        }

        //set up random numbers table
        int indexMax = td.heightmapHeight > td.heightmapWidth ? td.heightmapHeight : td.heightmapWidth;
        print(indexMax.ToString());
        int[] permutations = new int[indexMax]; //make it as long as the largest dimension
        for( i = 0; i < indexMax; i++ )
            permutations[i] = i;    //each number will show once
        //randomize the table
        for( i = 0; i < indexMax; i++)
        {
            j = (int)RandomS(indexMax);
            k = permutations [i];
            permutations[i] = permutations[j];
            permutations[j] = k;
        }

        //loop through all the pixels
        for( i = 0; i < vGrid; i++)
            for( j = 0; j < hGrid; j++ )
            {
                //get value for pixel by adding successive layers
                amplitude = 1.0f;
                frequency = 1.0f / (float)hGrid;
                pixel_value = 0.0f;

                for( k = 0; k < octaves; k++ )
                {
                    //get x and y values, they are in simplex space
                    x = (float)j * frequency;
                    y = (float)i * frequency;

                    //get the bottom-left corner of the simplex in skewed space
                    skew_value = (x + y) * general_skew;
                    cornerBot.x = MyFloor( x + skew_value);
                    cornerBot.y = MyFloor( y + skew_value);

                    //get distance from bottom corner in simplex space
                    unskew_value = (float)(cornerBot.x + cornerBot.y) * general_unskew;
                    disBot.x = x - (float)cornerBot.x + unskew_value;
                    disBot.y = y - (float)cornerBot.y + unskew_value;

                    //get the middle corner in skew space
                    if( disBot.x > disBot.y)
                    {
                        cornerMid.x = 1 + cornerBot.x;//lower triangle
                        cornerMid.y = cornerBot.y;
                    }
                    else
                    {
                        cornerMid.x = cornerBot.x;
                        cornerMid.y = 1 + cornerBot.y;
                    }

                    //get the top corner in skew space
                    cornerTop.x = 1 + cornerBot.x;
                    cornerTop.y = 1 + cornerBot.y;

                    //get the distance from the other two corners
                    disMid.x = (float)(cornerMid.x - cornerBot.x) + general_unskew;
                    disMid.y = (float)( cornerMid.y - cornerBot.y) + general_unskew;

                    disTop.x = disBot.x - 1.0f + general_unskew + general_unskew;
                    disTop.y = disBot.y - 1.0f + general_unskew + general_unskew;

                    //get the gradients indices
                    grad.x = permutations[(int)(cornerBot.x + permutations[(int)(cornerBot.y) & 255]) & 255] & 7;
                    grad.y = permutations[(int)(cornerMid.x + permutations[(int)(cornerMid.y) & 255]) & 255] & 7;
                    grad.z = permutations[(int)(cornerTop.x + permutations[(int)(cornerTop.y) & 255]) & 255] & 7;

                    //get the noise from each corner using an attenuation function
                    //  bottom corner
                    tempDis = 0.5f - disBot.x * disBot.x - disBot.y * disBot.y;
                    if( tempDis < 0.0f)
                        noise.x = 0.0f;
                    else
                        noise.x = Mathf.Pow( tempDis , 4) * DotProduct(gradients, (int)grad.x , disBot.x, disBot.y);

                    
				    //middle corner
				    tempDis = 0.5f - disMid.x * disMid.x - disMid.y * disMid.y;
				    if (tempDis < 0.0f)
					    noise.y = 0.0f;
				    else
					    noise.y = Mathf.Pow(tempDis,4.0f) * DotProduct(gradients , (int)grad.y,disMid.x,disMid.y);

				    //	last the top corner
				    tempDis = 0.5f - disTop.x * disTop.x - disTop.y * disTop.y;
				    if (tempDis < 0.0f)
					    noise.z = 0.0f;
				    else
					    noise.z = Mathf.Pow(tempDis,4.0f) * DotProduct(gradients , (int) grad.z,disTop.x,disTop.y);
				
                    //add in and adjust for next layer
                    pixel_value += (noise.x + noise.y + noise.z) * amplitude;

                    amplitude *= gain;
                    frequency *= lacuarity;
                }

                //put it in map
                map[j,i] = pixel_value;

                //do some quick checks
                if( pixel_value < min)
                    min = pixel_value ;

                else if( pixel_value > max)
                    max = pixel_value ;
            }
    }

    void printMap( float[,] map, float min, float max)
    {
        //heightmap
        float diff = max - min;

        float flood = floodLevel * diff;
        float mount = mountLevel * diff;

        int i, j, k; // used for looping
        

        //texture

        Color landlow = new Color(0, 64 , 0),
          landhigh = new Color(116, 182 , 133 ),
          waterlow = new Color(55 , 0, 0),
          waterhigh = new Color(106 , 53 , 0),
          mountlow = new Color(147 , 157 , 167 ),
          mounthigh = new Color(226 , 223 , 216 );

        Color newColor = new Color(1, 1, 1);
        Color[] colors = new Color[ td.heightmapWidth * td.heightmapHeight ];
        for (i = 0; i < vGrid; i++)
            {//bitmaps start with the bottom row, and work their way up...
		    for ( j=0; j<hGrid; j++ )
                {//...but still go left to right
                    map[j, i] -=min; //dont know why - but is needed
			    //if this point is below the floodline...
                if (map[j, i] < flood)
                    newColor = lerp(waterlow,waterhigh,map[j,i]/flood);

                //if this is above the mountain line...
                else if (map[j, i] > mount)
                    newColor = lerp(mountlow, mounthigh, (map[j, i] - mount) / (diff - mount));

                //if this is regular land
                else
                    newColor = lerp(landlow, landhigh, (map[j, i] - flood) / (mount - flood));

                colors[i*vGrid + j] = newColor;
                //out.put(char(newcolor.v[0]));//blue
                //out.put(char(newcolor.v[1]));//green
                //out.put(char(newcolor.v[2]));//red
		    }
		
	    }
       texture.SetPixels(colors);
       texture.Apply();

       float[,] hm = new float[td.heightmapWidth, td.heightmapHeight];
       for (i = 0; i < hGrid; i++)
           for (j = 0; j < vGrid; j++)
           {
               hm[i, j] = map[i, j] / max;
           }

       td.SetHeights(0, 0, hm);
    }


    Color lerp(Color c1, Color c2, float value)
    {
	    Color tcolor = new Color(0,0,0);
	
	    for (int g=0;g<3;g++){
		    if (c1[g]>c2[g])
			    tcolor[g]=c2[g]+(char)((float)(c1[g]-c2[g])*value);

		    else
			    tcolor[g]=c1[g]+(char)((float)(c2[g]-c1[g])*value);
	}
	
	return (tcolor);
}

    float RandomS(float max)
    {
        int r;
        float s;

        r = Random.Range(0,1000000); // converted from c++'s rand();
        s = (float)(r & 0x7fff)/(float)0x7fff;

        return (s* max);
    }

    int MyFloor(float value)
    {
        return (value >= 0 ? (int)value : (int)value-1);
    }

    float DotProduct( float[,] grad , int index, float x , float y )
    {
        return (grad[index,0] * x + grad[index,1] * y);
    }

    void HeightToTexture()
    {
        //texture = new Texture2D(td.heightmapWidth, td.heightmapHeight);
        Color[] colors = new Color[td.heightmapHeight * td.heightmapWidth];
        float[,] hm = td.GetHeights(0, 0, td.heightmapWidth, td.heightmapHeight);
        for (int x = 0; x < td.heightmapWidth; x++)
        {
            for (int y = 0; y < td.heightmapHeight; y++)
            {
                colors[x * td.heightmapWidth + y] = /*new Color(0, 0, 0, 1);//*/new Color(hm[x, y], hm[x, y], hm[x, y]);
                //print(x.ToString() + "  " + y.ToString());
            }
        }
        //texture.SetPixels(
        texture.SetPixels(colors);
        texture.Apply(false);

    }
}
