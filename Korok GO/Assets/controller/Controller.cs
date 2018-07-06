using Assets.scripts.Location;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{

    public static Controller instance;

    public Transform player;

    public GameObject titleScreenPanel;
    public GameObject GPSErrorPanel;
    
    public Image titleScreenBkg;
    public Text titleScreenCopyrightText;
    public Image titleScreenLogo;

    public int seeds;
    public Text UISeedsText;

    public float splashScreenTime;

    [Header("Korok Found cutscene")]
    public Camera gameCamera;
    public GameObject appearingKorokPrefab;
    public GameObject winSeedUIPanel;

    [Header("Shop")]
    public GameObject shopPanel;
    public GameObject shopButton;
    public GameObject backButton;
    public GameObject shopWildHaircutDetailPanel;
    public GameObject shopKorokMaskDetailPanel;
    public GameObject shopCappyDetailPanel;
    public GameObject shopBaldnessDetailPanel;
    public List<GameObject>masksObjectsToHide;
    public Text korokMaskInventoryValueText;
    public Text cappyInventoryValueText;
    public Text baldnessInventoryValueText;

    [Header("Baldness")]
    public List<GameObject> baldnessObjectsToShow;

    [Header("Haircut of the wild")]
    public List<GameObject> haircutOfTheWildObjectsToShow;

    [Header("Korok mask")]
    public List<GameObject> korokMaskObjectsToShow;

    [Header("Cappy")]
    public List<GameObject> cappyObjectsToShow;



    private bool wildhaircutDetailVisible;
    private bool korokMaskDetailVisible;
    private bool cappyDetailVisible;
    private bool baldnessDetailVisible;

    public void ShowShop(bool show)
    {
        shopPanel.SetActive(show);
        shopButton.SetActive(!show);
        backButton.SetActive(show);
        wildhaircutDetailVisible = false;
        korokMaskDetailVisible = false;
        cappyDetailVisible = false;
        baldnessDetailVisible = false;
        UpdateDetailPanels();
    }

    private void UpdateDetailPanels()
    {
        int korokMaskInInventory = PlayerPrefs.GetInt("KorokGO_KorokMask", 0);
        korokMaskInventoryValueText.text = "x" + korokMaskInInventory.ToString();
        int cappyInInventory = PlayerPrefs.GetInt("KorokGO_Cappy", 0);
        cappyInventoryValueText.text = "x" + cappyInInventory.ToString();
        int baldnessInInventory = PlayerPrefs.GetInt("KorokGO_Baldness", 0);
        baldnessInventoryValueText.text = "x" + baldnessInInventory.ToString();

        shopWildHaircutDetailPanel.SetActive(wildhaircutDetailVisible);
        shopKorokMaskDetailPanel.SetActive(korokMaskDetailVisible);
        shopCappyDetailPanel.SetActive(cappyDetailVisible);
        shopBaldnessDetailPanel.SetActive(baldnessDetailVisible);
    }

    public void ToggleWildHaircutDetails()
    {
        wildhaircutDetailVisible = !wildhaircutDetailVisible;
        korokMaskDetailVisible = false;
        cappyDetailVisible = false;
        baldnessDetailVisible = false;
        UpdateDetailPanels();
    }

    public void ToggleKorokMaskDetails()
    {
        wildhaircutDetailVisible = false;
        korokMaskDetailVisible = !korokMaskDetailVisible;
        cappyDetailVisible = false;
        baldnessDetailVisible = false;
        UpdateDetailPanels();
    }

    public void ToggleCappyDetails()
    {
        wildhaircutDetailVisible = false;
        korokMaskDetailVisible = false;
        cappyDetailVisible = !cappyDetailVisible;
        baldnessDetailVisible = false;
        UpdateDetailPanels();
    }

    public void ToggleBaldnessDetails()
    {
        wildhaircutDetailVisible = false;
        korokMaskDetailVisible = false;
        cappyDetailVisible = false;
        baldnessDetailVisible = !baldnessDetailVisible;
        UpdateDetailPanels();
    }

    private IEnumerator WaitAndResetUISeedsTextColor(float delay)
    {
        yield return new WaitForSeconds(delay);
        UISeedsText.color = Color.white;
    }

    private void EquipMask(string playerPrefsMaskKey, int price, List<GameObject> toHide, List<GameObject> toShow)
    {
        int maskInInventory = PlayerPrefs.GetInt(playerPrefsMaskKey, 0);

        if (maskInInventory == 0)
        {
            if (seeds >= price)
            {
                // buy mask
                maskInInventory += 1;
                PlayerPrefs.SetInt(playerPrefsMaskKey, maskInInventory);
                /*seeds -= price;
                PlayerPrefs.SetInt("KorokGO_Seeds", seeds);
                UISeedsText.text = seeds.ToString();*/

                int korokMaskInInventory = PlayerPrefs.GetInt("KorokGO_KorokMask", 0);
                korokMaskInventoryValueText.text = "x" + korokMaskInInventory.ToString();
                int cappyInInventory = PlayerPrefs.GetInt("KorokGO_Cappy", 0);
                cappyInventoryValueText.text = "x" + cappyInInventory.ToString();
                int baldnessInInventory = PlayerPrefs.GetInt("KorokGO_Baldness", 0);
                baldnessInventoryValueText.text = "x" + baldnessInInventory.ToString();
            }
            else
            {
                // can't buy
                UISeedsText.color = Color.red;
                StartCoroutine(WaitAndResetUISeedsTextColor(0.5f));
            }
        }

        if (maskInInventory > 0)
        {
            // equip mask
            foreach (GameObject obj in toHide)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in toShow)
            {
                obj.SetActive(true);
            }
        }
    }

    public void EquipBaldness()
    {
        EquipMask("KorokGO_Baldness", 900, masksObjectsToHide, baldnessObjectsToShow);
    }

    public void EquipHaircutOfTheWild()
    {
        EquipMask("KorokGO_HaircutOfTheWild", 0, masksObjectsToHide, haircutOfTheWildObjectsToShow);
    }

    public void EquipKorokMask()
    {
        EquipMask("KorokGO_KorokMask", 50, masksObjectsToHide, korokMaskObjectsToShow);
    }

    public void EquipCappy()
    {
        EquipMask("KorokGO_Cappy", 100, masksObjectsToHide, cappyObjectsToShow);
    }

    void Awake()
    {
        korokMaskDetailVisible = false;
        cappyDetailVisible = false;
        Controller.instance = this;
    }

    void Start()
    {
        ShowShop(false);
        seeds = PlayerPrefs.GetInt("KorokGO_Seeds", 0);
        UISeedsText.text = seeds.ToString();
        StartCoroutine(WaitAndStartGame(0));
    }

    public void FindKorok(Vector3 korokPosition, int xID, int yID)
    {
        GameObject korokTile = MapGeneratorOSM.instance.GetTileFromCoords(xID, yID);
        Vector3 korokPositionRelativeToTile = korokPosition - korokTile.transform.position;

        string korokPositionStr = korokPositionRelativeToTile.x.ToString() + ",3," + korokPositionRelativeToTile.z.ToString();

        PlayerPrefs.SetString(KorokBehaviour.GetHashCodeForIDs(xID, yID), korokPositionStr + ",Found");

        StartCoroutine(WaitAndPlayKorokFoundCutscene(0, korokPosition));
    }


    IEnumerator WaitAndPlayKorokFoundCutscene(float delay, Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        gameCamera.GetComponent<CameraMapOSM>().cameraFollowsPlayer = false;
        gameCamera.transform.position = position + 3 * Vector3.up - 2 * Vector3.forward;
        AudioManager.instance.PlayKorokYahaha();
        StartCoroutine(WaitAndSpawnKorok(0, position));
    }

    IEnumerator WaitAndSpawnKorok(float delay, Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        GameObject korok = Instantiate(appearingKorokPrefab, new Vector3(position.x, 3, position.z), Quaternion.identity);
        korok.transform.LookAt(gameCamera.transform, Vector3.up);
        gameCamera.transform.LookAt(korok.transform, Vector3.up);
        StartCoroutine(WaitAndShowSeedUI(3.5f));
    }

    IEnumerator WaitAndShowSeedUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        winSeedUIPanel.SetActive(true);
        AudioManager.instance.PlaySeedWonUI();
        StartCoroutine(WaitAndIncrementSeeds(1.8f));
    }

    IEnumerator WaitAndIncrementSeeds(float delay)
    {
        yield return new WaitForSeconds(delay);
        seeds++;
        UISeedsText.text = seeds.ToString();
        AudioManager.instance.PlayPlicUI();
        PlayerPrefs.SetInt("KorokGO_Seeds", seeds);
        StartCoroutine(WaitAndHideSeedUI(0.8f));
    }

    IEnumerator WaitAndHideSeedUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        winSeedUIPanel.SetActive(false);
        AudioManager.instance.PlayKorokByeBye();
        StartCoroutine(WaitAndGoBackToInGameCamera(1.5f));
    }

    IEnumerator WaitAndGoBackToInGameCamera(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameCamera.GetComponent<CameraMapOSM>().cameraFollowsPlayer = true;
    }

    IEnumerator WaitAndStartGame(float delay)
    {
        yield return new WaitForSeconds(delay);
        Connect();
    }

    IEnumerator WaitAndRemoveUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(WaitAndMakeUIMoreTransparent(0));
    }

    IEnumerator WaitAndMakeUIMoreTransparent(float t)
    {
        yield return new WaitForEndOfFrame();
        float delta = Time.deltaTime;
        float bkgalpha = (1 - t) < 0 ? 0 : (1 - t);
        float logoalpha = (1.5f - t) < 0 ? 0 : ((1.5f - t) > 1 ? 1 : (1.5f - t));
        titleScreenBkg.color = new Color(titleScreenBkg.color.r, titleScreenBkg.color.g, titleScreenBkg.color.b, bkgalpha);
        titleScreenCopyrightText.color = new Color(titleScreenCopyrightText.color.r, titleScreenCopyrightText.color.g, titleScreenCopyrightText.color.b, bkgalpha);
        titleScreenLogo.color = new Color(titleScreenLogo.color.r, titleScreenLogo.color.g, titleScreenLogo.color.b, logoalpha);

        if (t >= 1.5f)
        {
            titleScreenPanel.SetActive(false);
        }
        else
        {
            StartCoroutine(WaitAndMakeUIMoreTransparent(t + delta));
        }
    }

    public void Connect()
    {
        // Immediately start to initialize DataModel
        StartCoroutine(WaitAndInitializeLocationService());
    }

    private IEnumerator WaitAndInitializeLocationService()
    {
        Debug.Log( "Starting Location Service..." );
        yield return StartCoroutine(DataModel.instance.InitializeLocationService());

        if (MyLocationService.instance.locationServiceIsRunning)
        {
            // location service is ready
            Debug.Log("Location Service Operational!");
            // draw map
            MapGeneratorOSM.instance.GenerateOSMMap();
            StartCoroutine(WaitAndRemoveUI(splashScreenTime));
        }
        else
        {
            // location service failed to start
            Debug.LogError("Failed to start location service. Please verify that your GPS is active.");
            StartCoroutine(WaitAndDisplayGPSError());
        }
    }

    IEnumerator WaitAndDisplayGPSError()
    {
        yield return new WaitForEndOfFrame();

        GPSErrorPanel.SetActive(true);
    }
}
