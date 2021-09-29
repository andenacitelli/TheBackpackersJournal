using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System;

public struct photo
{
    public Texture2D captureData;
    public string fileName;
    public byte[] rawDat;
    // will need more things here - for sure
}
//UI Code here is temporary & just for testing
public class CameraRoll : MonoBehaviour
{
    //Make an option in the future
    private static bool autoPlace = true;
    private bool bufferFilled = false;
    public int capacity = 9;
    private List<GameObject> inView;
    private ImageScanner imageScan;
    private CRUIGallery galleryUI;
    private int internalIndex = 0;
    [Header("UI")]
    public CameraRollMenu crUI;

    public List<photo> cRollStorage;
    private static photo buffer;
    private int counter = 0;

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
        
        LoadCRoll();
        
    }

    private void Update()
    {
        if(bufferFilled && counter > 120)
        {
            Debug.Log("Hopped in here!");
            counter = 0;
            bufferFilled = false;
            ProcessRecievedPhoto(buffer.captureData);
        }
        counter++;
    }

    private void LoadCRoll() 
    {
        cRollStorage = new List<photo>();
        string pathNoFile = Application.dataPath + "/PhotoStorage/";
        DirectoryInfo info = new DirectoryInfo(pathNoFile);
        FileInfo[] fileInfo = info.GetFiles();
        int index = 0;
        foreach (FileInfo f in fileInfo)
        {
            if (!f.Name.Contains("meta"))
            {
                
                string path = "/PhotoStorage/" + index;
                string absolutePath = pathNoFile + f.Name;
                Texture2D t2D = new Texture2D(Screen.width, Screen.height);

                byte[] data = File.ReadAllBytes(absolutePath);
                ImageConversion.LoadImage(t2D, data);
                t2D.Apply();
                
                //Texture2D newTex = new Texture2D(Screen.width, Screen.height);
                //newTex.LoadRawTextureData(grab);
                //newTex.Apply();
                photo grabPhoto = new photo
                {
                    captureData = t2D,
                    fileName = absolutePath
                };
                SaveBuffer(index, grabPhoto, true);
                index++;
            }
            

        }

    }

    public void RecievePhoto(byte[] rawData)
    {
        Texture2D newTex = new Texture2D(Screen.width, Screen.height);
        newTex.LoadRawTextureData(rawData);
        newTex.Apply();
        buffer = new photo
        {
            rawDat = rawData,
            captureData = newTex
        };
        bufferFilled = true;
    }

    public void ProcessRecievedPhoto(Texture2D screenTex)
    {
        //screenTex.Apply();
        Debug.Log("Recieved Photo");
        internalIndex = cRollStorage.Count;
        //create struct
        
        

        if (!IsStorageFull() && autoPlace)
        {
            SaveBuffer(internalIndex, buffer, false);
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
        SaveBuffer(indexToReplace, buffer, false);
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
    private void SaveBuffer(int crIndex, photo buffPass, bool isLoad)
    {
        string fileName = Application.dataPath + "/PhotoStorage/" + crIndex + ".png";

        buffPass.fileName = fileName;
        crUI.UpdateCR(crIndex, buffPass.captureData);
        cRollStorage.Insert(crIndex, buffPass);


        if (!isLoad)
        {
            StartCoroutine(WriteFile(fileName));
        }
        
    }

    private IEnumerator WriteFile(string fileName)
    {
        byte[] rawData = buffer.captureData.EncodeToPNG();
        yield return new WaitForSeconds(.1f);
        new System.Threading.Thread(() =>
        {
            //System.Threading.Thread.Sleep(100);
            File.WriteAllBytes(fileName, rawData);
        }).Start();

        yield return new WaitForSeconds(.5f);

        yield break;
    }

    /* Efficient way to off-load picture saving:
     * 
     * 
        
        
     * where: rawData - byte[] & fileName - str
     */

}
