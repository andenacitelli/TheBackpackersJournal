using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

//UI Code here is temporary & just for testing
public class CameraRoll : MonoBehaviour
{
    //Make an option in the future
    private static bool autoPlace = true;
    public int capacity = 9;
    private List<GameObject> inView;
    private ImageScanner imageScan;
    private CRUIGallery galleryUI;
    private int internalIndex = 0;
    [Header("UI")]
    public CameraRollMenu crUI;

    public struct photo {
        public Texture2D captureData;
        public string fileName;
        // will need more things here - for sure
    }

    public List<photo> cRollStorage;
    private static photo buffer;

    #region UItest
    [Header("TestConnections")]
    public GameObject uiTestImage;
    public GameObject uiTestData;

    private Image uiImage;
    private TextMeshProUGUI uiText;
    #endregion
    private void Awake()
    {
        imageScan = GetComponent<ImageScanner>();
        uiImage = uiTestImage.GetComponent<Image>();
        uiText = uiTestData.GetComponent<TextMeshProUGUI>();
        
        cRollStorage = new List<photo>();
        
    }
    public void RecievePhoto(Texture2D screenTex)
    {
        //screenTex.Apply();
        Debug.Log("Recieved Photo");
        internalIndex = cRollStorage.Count;
        //create struct
        photo grab = new photo { 
            captureData = screenTex
        };
        

        if (!IsStorageFull() && autoPlace)
        {
            SaveBuffer(internalIndex, grab);
        } else
        {
            crUI.UpdatePopUp(screenTex);
            CapturePopUp();
        }

        //save pngData with pngData.LoadRawTextureData(screenText.GetRawTextureData())
        // be sure to pngData.Apply() - this commits it to memory

        //Creates texture and loads byte array data to create image
        //Texture2D tex = new Texture2D(2, 2);

        //empty buffer struct & fill with new info

        //tex.LoadImage(pngData);

        //Creates a new Sprite based on the Texture2D
        inView = imageScan.ScanFrame();

        #region debugScreenCapture
        string objectsInImage = "On Display: ";
        if(inView.Count > 0)
        {
            foreach (GameObject scannedObj in inView)
            {
                objectsInImage += scannedObj.name + " - ";
            }
        } else
        {
            objectsInImage += "None";
        }
        
        Sprite fromTex = Sprite.Create(screenTex, new Rect(0.0f, 0.0f, screenTex.width, screenTex.height), new Vector2(0.5f, 0.5f), 100.0f);
        uiImage.sprite = fromTex;
        uiText.text = objectsInImage;
        #endregion
    }

    public void CapturePopUp()
    {
        crUI.OpenPopUp();
    }

    public void KeepCapture()
    {

    }

    public void DiscardCapture()
    {

    }

    public bool IsStorageFull()
    {
        bool isFull = false;

        if(cRollStorage.Count == capacity)
        {
            isFull = true;
        }

        return isFull;
    }
    private void SaveBuffer(int crIndex, photo buffPass)
    {
        string fileName = Application.dataPath + "/PhotoStorage/" + crIndex + ".png";


        crUI.UpdateCR(crIndex, buffPass.captureData);
        cRollStorage.Insert(crIndex, buffPass);
    }

    /* Efficient way to off-load picture saving:
     * 
     * 
        new System.Threading.Thread(() =>
        {
            System.Threading.Thread.Sleep(100);
            File.WriteAllBytes(fileName, rawData);
        }).Start();
        
     * where: rawData - byte[] & fileName - str
     */

}
