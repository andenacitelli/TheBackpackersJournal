using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.IO;
using System.Threading;
using System.Collections;
using System;

public struct photo
{
    public Texture2D captureData;
    public string fileName;
    public List<GameObject> inView;
    // will need more things here - for sure
}
//UI Code here is temporary & just for testing
public class CameraRoll : MonoBehaviour
{
    //Make an option in the future
    private static bool autoPlace = true;
    private bool bufferFilled = false;
    public int capacity = 9;
    private List<GameObject> currView;
    private ImageScanner imageScan;
    private CRUIGallery galleryUI;
    private int internalIndex = 0;
    [Header("UI")]
    public CameraRollMenu crUI;
    [Header("Storage")]
    public GameObject galleryStorageGO;
   
    public List<photo> cRollStorage;
    private GalleryStorage galleryStorage;
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
        galleryStorage = galleryStorageGO.GetComponent<GalleryStorage>();
        
        LoadCRoll();
        
    }

    private void Update()
    {
        if(bufferFilled && counter > 120)
        {
            counter = 0;
            bufferFilled = false;
            ProcessRecievedPhoto(buffer.captureData);
        }
        counter++;
    }

    private void LoadCRoll() 
    {
        cRollStorage = new List<photo>();
        string pathNoFile = Application.persistentDataPath + "/PhotoStorage/";
        DirectoryInfo info = new DirectoryInfo(pathNoFile);
        if (!info.Exists)
        {
            info.Create();
        }
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

    public void ForwardPhotoToStorage(int indexChosen)
    {
        photo grabP = cRollStorage[indexChosen];
        Texture2D tex = grabP.captureData;
        //crUI.UpdateCR(indexChosen, null);
        galleryStorage.ReceivePhoto(indexChosen);

        //TODO: HANDLE FILES
        //cRollStorage.RemoveAt(indexChosen);
    }

    public void RecievePhoto(byte[] rawData)
    {
        Debug.Log("Recieved Photo");
        currView = imageScan.ScanFrame();
        Texture2D newTex = new Texture2D(Screen.width, Screen.height);
        newTex.LoadRawTextureData(rawData);
        newTex.Apply();
        buffer = new photo
        {
            inView = currView,
            captureData = newTex
        };
        bufferFilled = true;
    }

    public void ProcessRecievedPhoto(Texture2D screenTex)
    {
        //screenTex.Apply();
        Debug.Log("Processing Recieved Photo");
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
        
        /*
        #region debugScreenCapture
        string objectsInImage = "On Display: ";
        if(currView.Count > 0)
        {
            foreach (GameObject scannedObj in currView)
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
        */
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
        string fileName = Application.persistentDataPath + "/PhotoStorage/" + crIndex + ".png";

        buffPass.fileName = fileName;
        crUI.UpdateCR(crIndex, buffPass.captureData);
        cRollStorage.Insert(crIndex, buffPass);


        if (!isLoad)
        {
            //WriteFile(fileName);
        }
        
    }

    public async void WriteFile(string fileName, Texture2D data)
    {
        byte[] rawData = data.EncodeToPNG();
        using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fs.Seek(0, SeekOrigin.End);
            await fs.WriteAsync(rawData, 0, rawData.Length);
        }
        
        //yield return new WaitForSeconds(.1f);
        /*new Thread(() =>
        {
            Thread.Sleep(0);
            File.WriteAllBytes(fileName, rawData);

        }).Start();*/
        print("File Written.");
        //yield return new WaitForSeconds(.5f);

        //yield break;
    }

    /* Efficient way to off-load picture saving:
     * 
     * 
        
        
     * where: rawData - byte[] & fileName - str
     */

}
