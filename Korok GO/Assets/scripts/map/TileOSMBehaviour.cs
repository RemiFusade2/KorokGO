using Assets.scripts.Map.OpenStreetMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileOSMBehaviour : MonoBehaviour
{
    public int zoom;
    public int tileX;
    public int tileY;

    private Texture2D tileTexture;

    public Renderer tileRenderer;

    private bool textureDownloadHasBeenRequested;
    public bool TextureDownloadHasBeenRequested()
    {
        return textureDownloadHasBeenRequested;
    }
    
    public bool TextureDownloadHasBeenFinished()
    {
        Texture2D tileTexture = TileLoader.instance.GetTextureFromZoomAndCoords(zoom, tileX, tileY);
        if (tileTexture != null)
        {
            //Debug.Log("tileTexture : " + tileTexture.name);
        }
        else
        {
            // Debug.Log("tileTexture = null");
        }
        return tileTexture != null;
    }

    public bool TextureHasBeenSet()
    {
        Texture2D tileTexture = TileLoader.instance.GetTextureFromZoomAndCoords(zoom, tileX, tileY);
        return tileTexture != null && tileTexture.Equals(tileRenderer.material.mainTexture);
    }

    public float GetTileSideSize()
    {
        return this.transform.localScale.x * 10;
    }

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public bool TileCoordinatesAreEqual(Vector2 tileCoords)
    {
        return (this.tileX == Mathf.FloorToInt(tileCoords.x)) && (this.tileY == Mathf.FloorToInt(tileCoords.y));
    }

    public void SetData(int tileZoom, int x, int y)
    {
        zoom = tileZoom;
        tileX = x;
        tileY = y;
        textureDownloadHasBeenRequested = false;
    }

    public bool LoadTexture()
    {
        bool loadDidHappen = TileLoader.instance.LoadTileImageFromZoomAndCoords(zoom, tileX, tileY);
        textureDownloadHasBeenRequested = loadDidHappen;
        return loadDidHappen;
    }

    static Color colorIsTransparent = new Color(0, 22 / 255.0f, 1 / 255.0f, 1);

    public void SetTexture()
    {
        Texture2D tileTexture = TileLoader.instance.GetTextureFromZoomAndCoords(zoom, tileX, tileY);

        Debug.Log("SetTexture()");

        // treat my texture
        Color[] allPixels = tileTexture.GetPixels();
        Color transparence = new Color(0, 0, 0, 0);
        for (int i = 0; i < allPixels.Length; i++)
        {
            if (allPixels[i].Equals(colorIsTransparent))
            {
                allPixels[i] = transparence;
            }
            else
            {
                allPixels[i].a = 0.4f;
            }
        }
        tileTexture.SetPixels(allPixels);
        tileTexture.Apply();
        

        float epsilon = 0.002f;
        tileRenderer.material.mainTextureScale = new Vector2(1 - 2 * epsilon, 1 - 2 * epsilon);
        tileRenderer.material.mainTextureOffset = new Vector2(epsilon, epsilon);
        tileRenderer.material.mainTexture = tileTexture;
    }
}
