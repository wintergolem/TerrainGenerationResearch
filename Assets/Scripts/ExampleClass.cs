using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour {
	public int pixWidth;
	public int pixHeight;
	public float xOrg;
	public float yOrg;
	public float scale = 1.0F;
	private Texture2D noiseTex;
	private Color[] pix;
	void Start() {
		noiseTex = new Texture2D(pixWidth, pixHeight);
		pix = new Color[noiseTex.width * noiseTex.height];
		renderer.material.mainTexture = noiseTex;
	}
	void CalcNoise() {
		float y = 0.0F;
		while (y < noiseTex.height) {
			float x = 0.0F;
			while (x < noiseTex.width) {
				float xCoord = xOrg + x / noiseTex.width * scale;
				float yCoord = yOrg + y / noiseTex.height * scale;
				float sample = Mathf.PerlinNoise(xCoord, yCoord);
				//pix[y * noiseTex.width + x] = new Color(sample, sample, sample);
				x++;
			}
			y++;
		}
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
	}
	void Update() {
		CalcNoise();
	}
}