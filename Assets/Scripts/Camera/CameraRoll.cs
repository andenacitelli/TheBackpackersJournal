using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

//UI Code here is temporary & just for testing
public class CameraRoll : MonoBehaviour
{
    private UnityEvent findInView;
    private List<GameObject> inView;
    private ImageScanner imageScan;
    
    struct photo {
        public byte[] pngBytes;
        public string name;
        // will need more things here - for sure
    }

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
        

    }
    public void RecievePhoto(Texture2D screenTex)
    {

        //create struct
        //save pngData with pngData.LoadRawTextureData(screenText.GetRawTextureData())
        // be sure to pngData.Apply() - this commits it to memory

        //Creates texture and loads byte array data to create image
        //Texture2D tex = new Texture2D(2, 2);

        //empty buffer struct & fill with new info

        //tex.LoadImage(pngData);

        //Creates a new Sprite based on the Texture2D
        inView = imageScan.ScanFrame();
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
    }


    void FoundInView()
    {
        //inView.Add(found);
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
