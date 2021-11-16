using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;

public class GalleryStorage : MonoBehaviour
{
    [Header("Params")]
    public float editExitDist;
    public float editWarnDist;

    public GameObject galleryStorageUI;
    public GameObject polaroidCamera;
    public GameObject galleryUIstart;
    public GameObject galleryUIscale;
    public bool isOn { get; set; }
    public bool editingStorageOn { get; set; }
    public int scaleModifier { get; set; }
    private bool distanceWarned = false;
    private CameraRoll cameraRoll;
    private CameraRollMenu cameraRollUI;

    public List<photo> gallery; 
    public photo lastPhotoPtr;
    private GalleryScaleUI rescale;
    private int lastIndex;


    private void Awake()
    {
        gallery = new List<photo>();
    }
    private void Start()
    {
        rescale = galleryUIscale.GetComponent<GalleryScaleUI>();
        cameraRoll = polaroidCamera.GetComponent<CameraRoll>();
        cameraRollUI = cameraRoll.crUI;
        editingStorageOn = false;
        isOn = false;
        scaleModifier = -1;
    }
    public void StartGalleryStorage()
    {
        galleryStorageUI.SetActive(true);
        isOn = true;
    }

    public void StartCRtoStorageConvert()
    {
        cameraRollUI.OpenCRStorage();
    }

    public photo ReceivePhoto(int crIndex, photo grab)
    {
        //crIndex is the moved camera roll index - in case it's needed
        Debug.Log("Recieved Photo in gallery storage");

        lastIndex = gallery.Count;
        if(lastIndex < 0)
        {
            lastIndex = 0;
        }
        galleryUIstart.SetActive(false);
        
        //calculate score?
        rescale.PreloadPhoto(grab.captureData);
        galleryUIscale.SetActive(true);

        string newFName = Application.persistentDataPath + "/PhotoStorage/" + cameraRoll.profileName + "/GalleryRoll/" + lastIndex + ".png";

        photo newPhoto = new photo
        {
            captureData = grab.captureData,
            inView = grab.inView,
            fileName = newFName
        };
        gallery.Insert(lastIndex, newPhoto);
        newPhoto.inStorage = 1;
        return newPhoto;
    }

    public void IncreaseScale()
    {
        rescale.IncreaseScale();
    }

    public void DecreaseScale()
    {
        rescale.DecreaseScale();
    }
    public void ScaleSelectConfirm()
    {
        //Scale is an int where 0 = 25%, 1 = 50%, 2 = 80%, 3 = 100%, 4 = 120%, 5 = 150% (of original size)
        int chosenScale = rescale.currentSizeIndex;
        scaleModifier = chosenScale;
        editingStorageOn = true;
        StartCoroutine(storageEditingMode());
        ExitGalleryStorage();
    }

    public void FinishStoragePlace(GameObject frame)
    {
        Image display = frame.GetComponentInChildren<Image>();
        Vector3 framePos = frame.transform.parent.position;
        //get photo from gallerystorage here
        photo grabP = gallery[lastIndex];
        grabP.wallX = framePos.x;
        grabP.wallY = framePos.y;
        grabP.wallZ = framePos.z;
        Sprite newS = Sprite.Create(grabP.captureData, new Rect(0.0f, 0.0f, grabP.captureData.width, grabP.captureData.height), new Vector2(0.0f, 0.0f), display.pixelsPerUnit);
        display.sprite = newS;
        Color holdColor = new Color(255f, 255f, 255f);
        display.color = holdColor;
        editingStorageOn = false;
    }

    public void ExitGalleryStorage()
    {
        galleryStorageUI.SetActive(false);
        galleryUIscale.SetActive(false);
        galleryUIstart.SetActive(true);
    }

    private void CheckDistanceWarning(float dist)
    {
        if (dist >= editWarnDist && !distanceWarned)
        {
            //TODO: Add GUI Alert here
            Debug.Log("Caution: Leaving storage edit range- start");
            distanceWarned = true;
        }
        else if (dist < editWarnDist && distanceWarned)
        {
            //TODO: turn off gui alert here
            Debug.Log("Caution: Leaving storage edit range- end");
            distanceWarned = false;
        }
    }

    private IEnumerator storageEditingMode()
    {
        float dist = Vector3.Distance(transform.position, polaroidCamera.transform.position);
        while ( dist <= editExitDist && editingStorageOn)
        {
            CheckDistanceWarning(dist);
            dist = Vector3.Distance(transform.position, polaroidCamera.transform.position);
            yield return new WaitForSeconds(.25f);
        }
        Debug.Log("Exited editing mode");
        editingStorageOn = false;
        isOn = false;
        yield break;
    }

    public async void WriteFile(string fileName, Texture2D data)
    {
        byte[] rawData = data.EncodeToPNG();
        string pName = PlayerPrefs.GetString("profileName");

        
        using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fs.Seek(0, SeekOrigin.End);
            await fs.WriteAsync(rawData, 0, rawData.Length);
        }
        
        

        print("File Written.");

    }
}
