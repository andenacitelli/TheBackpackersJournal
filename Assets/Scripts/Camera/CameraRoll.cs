using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.IO;
using System.Xml.Serialization;
using System.Collections;

public struct photo
{
    public Texture2D captureData;
    public string fileName;
    // will need more things here - for sure
}
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
        
        cRollStorage = LoadCRoll();
        
    }

    private List<photo> LoadCRoll() 
    {
        List<photo> retList = new List<photo>();
        string pathNoFile = Application.dataPath + "/PhotoStorage/";
        DirectoryInfo info = new DirectoryInfo(pathNoFile);
        FileInfo[] fileInfo = info.GetFiles();
        int index = 0;
        foreach (FileInfo f in fileInfo)
        {
            if (!f.Name.Contains("meta"))
            {
                //FileStream fs = f.OpenRead();
                
                //byte[] grab = new byte[fs.Length];

                string absolutePath = pathNoFile + f.Name;
                byte[] grab = File.ReadAllBytes(absolutePath);
            
                
                Texture2D newTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
                newTex.LoadRawTextureData(grab);
                newTex.Apply();
                photo grabPhoto = new photo
                {
                    captureData = newTex,
                    fileName = absolutePath
                };
                SaveBuffer(index, grabPhoto);
                index++;
            }
            

        }

        return retList;

    }

    public void RecievePhoto(Texture2D screenTex)
    {
        //screenTex.Apply();
        Debug.Log("Recieved Photo");
        internalIndex = cRollStorage.Count;
        //create struct
        buffer = new photo { 
            captureData = screenTex
        };
        

        if (!IsStorageFull() && autoPlace)
        {
            SaveBuffer(internalIndex, buffer);
        } else
        {
            crUI.UpdatePopUp(buffer.captureData);
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

    public void KeepCapture(int indexToReplace)
    {
        cRollStorage.RemoveAt(indexToReplace);
        SaveBuffer(indexToReplace, buffer);
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

        buffPass.fileName = fileName;
        crUI.UpdateCR(crIndex, buffPass.captureData);
        cRollStorage.Insert(crIndex, buffPass);

        StartCoroutine(WriteFile(fileName));

    }

    private IEnumerator WriteFile(string fileName)
    {
        byte[] rawData = buffer.captureData.EncodeToPNG();
        new System.Threading.Thread(() =>
        {
            System.Threading.Thread.Sleep(100);
            File.WriteAllBytes(fileName, rawData);
        }).Start();

        yield return new WaitForSeconds(.1f);
        yield break;
    }

    /* Efficient way to off-load picture saving:
     * 
     * 
        
        
     * where: rawData - byte[] & fileName - str
     */

}
