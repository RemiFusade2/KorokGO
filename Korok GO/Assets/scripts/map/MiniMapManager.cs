using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapManager : MonoBehaviour
{
    public GameObject seedPrefab;

    public Canvas canvas;
    public Image mapImage;

    public Transform koroksParent;
    public Transform mapSeedParent;

    public long currentNumberOfKoroks;
    public long totalNumberOfKoroks;

    public Text completionText;
    public Text completionText2;

    // Use this for initialization
    void Start ()
    {
        /*
		foreach (Transform korok in koroksParent)
        {
            if (korok.GetComponent(typeof(KorokBehaviour)) != null && korok.GetComponent<KorokBehaviour>().isFound)
            {
                DisplayKorok(korok.GetComponent<KorokBehaviour>().xID, korok.GetComponent<KorokBehaviour>().yID);
            }
        }
        UpdateCompletion();*/
    }

    public void DisplayKorok(int tilex, int tiley)
    {
        float xF = tilex / Mathf.Pow(2, MapGeneratorOSM.instance.zoomLevel);
        float yF = tiley / Mathf.Pow(2, MapGeneratorOSM.instance.zoomLevel);

        Vector2 sizeDelta = mapImage.GetComponent<RectTransform>().sizeDelta;
        Vector2 canvasScale = new Vector2(canvas.transform.localScale.x, canvas.transform.localScale.y);
        Vector2 imageSize = new Vector2(sizeDelta.x * canvasScale.x, sizeDelta.y * canvasScale.y);

        float positionX = xF * imageSize.x;
        float positionY = yF * imageSize.y;

        GameObject seed = Instantiate(seedPrefab, Vector3.zero, Quaternion.identity);
        seed.transform.SetParent(mapSeedParent, false);
    }

    public void UpdateCompletion()
    {
        float completion = currentNumberOfKoroks / totalNumberOfKoroks;
        string completionString = completion.ToString() + "/100%";
        completionText.text = completionString;
        completionText2.text = completionString;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
