using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;
using TMPro;

public class GalleryStorage : MonoBehaviour
{
    [Header("Params")]
    public float editExitDist;
    public float editWarnDist;

    public GameObject galleryStorageUI;
    public GameObject polaroidCamera;
    public GameObject galleryUIstart;
    public GameObject galleryUIscale;
    public GameObject framePrefab;
    public GameObject frameInfo;
    public TextMeshProUGUI frameInView;
    public JournalOptionsGUI jOptions;
    public JournalRoll journal;
    [SerializeField]
    public List<EditableObject> wallList;
    public bool isOn { get; set; }
    public bool editingStorageOn { get; set; }
    public int scaleModifier { get; set; }
    private bool distanceWarned = false;
    public CameraRoll cameraRoll;
    private CameraRollMenu cameraRollUI;

    public List<photo> gallery; 
    public photo lastPhotoPtr;
    private GalleryScaleUI rescale;
    
    private int lastIndex;
    private int frameIndex;
    private GameObject currFrame;


    private void Awake()
    {
        if(gallery == null)
        {
            gallery = new List<photo>();
        }
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

    public void LoadGRoll(Save s)
    {
        string profileName = s.playerName;
        gallery = new List<photo>();
        if (profileName != "")
        {
            string pathNoFile = Application.persistentDataPath + "/PhotoStorage/" + profileName + "/GalleryRoll/";
            DirectoryInfo info = new DirectoryInfo(pathNoFile);
            /*if (!info.Exists)
            {
                info.Create();
            }*/
            FileInfo[] fileInfo = info.GetFiles();
            int index = 0;
            photo[] loadArray = s.gallRoll;
            foreach (FileInfo f in fileInfo)
            {
                if (!f.Name.Contains("meta"))
                {
                    photo grabPhoto = loadArray[index];
                    string path = "/PhotoStorage/" + profileName + "/GalleryRoll/" + index;
                    string absolutePath = pathNoFile + f.Name;
                    Texture2D t2D = new Texture2D(Screen.width, Screen.height);
                    
                    byte[] data = File.ReadAllBytes(absolutePath);
                    ImageConversion.LoadImage(t2D, data);
                    t2D.Apply();
                    
                    photo newPhoto = new photo
                    {
                        captureData = t2D,
                        fileName = absolutePath,
                        inView = grabPhoto.inView,
                        inStorage = 1,
                        totalScore = grabPhoto.totalScore,
                        wallX = grabPhoto.wallX,
                        wallY = grabPhoto.wallY,
                        wallZ = grabPhoto.wallZ,
                        wallName = grabPhoto.wallName,
                        scale = grabPhoto.scale,
                        q = grabPhoto.q

                    };

                    gallery.Insert(index, newPhoto);

                    foreach(EditableObject wall in wallList)
                    {
                        if(wall.wallName == newPhoto.wallName)
                        {
                            //lastIndex = index;
                            Vector3 frameScale = newPhoto.scale;
                            Vector3 framePos = new Vector3(newPhoto.wallX, newPhoto.wallY, newPhoto.wallZ);
                            Quaternion q = grabPhoto.q;
                            StartCoroutine(LoadFrame(wall, framePos, frameScale, t2D, q, index));
                            break;
                        }
                    }
                    
                    index++;
                }


            }
        }
    }

    public IEnumerator LoadFrame(EditableObject wall, Vector3 framePos, Vector3 frameScale, Texture2D pic, Quaternion q, int index)
    {
        GameObject newFrame = Instantiate(framePrefab, wall.transform);
        newFrame.name = "frame" + index;
        yield return new WaitForEndOfFrame();
        Image display = newFrame.GetComponentInChildren<Image>();
        yield return new WaitForEndOfFrame();
        //print(newPhoto.captureData.width);
        Sprite newS = Sprite.Create(pic, new Rect(0.0f, 0.0f, pic.width, pic.height), new Vector2(0.0f, 0.0f), display.pixelsPerUnit);
        yield return new WaitForEndOfFrame();
        display.sprite = newS;
        Color holdColor = new Color(255f, 255f, 255f);
        display.color = holdColor;

        newFrame.transform.localScale = frameScale;
        newFrame.transform.rotation = q;
        wall.LoadFrame(newFrame, framePos);
        yield return new WaitForEndOfFrame();
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

    public void FrameDetailsOpen(GameObject selectedFrame)
    {
        isOn = true;
        string frameName = selectedFrame.transform.parent.name;
        char charIndex = frameName[frameName.Length - 1];
        int index = charIndex - '0';
        print("frame index set as: " + index);
        frameIndex = index;
        currFrame = selectedFrame;
        photo grabPhoto = gallery[index];

        // Update in-view state
        string[] inPic = grabPhoto.inView;
        string inViewInfo = "";
        for(int i = 0; i < inPic.Length; i++)
        {
            print(inPic[i]);
            if(i != inPic.Length - 1)
            {
                inViewInfo += inPic[i] + ", ";
            } else
            {
                inViewInfo += inPic[i];
            }
            
        }
        frameInView.text = inViewInfo;

        galleryUIstart.SetActive(false);
        frameInfo.SetActive(true);
        galleryStorageUI.SetActive(true);
    }


    public void ForwardToJournal(string pageChoice)
    {
        
        photo grabPhoto = gallery[frameIndex];
        journal.RecievePhoto(grabPhoto, pageChoice);
        //FrameJournalSelectExit();
    }

    public void FrameJournalSelectExit()
    {
        if (journal.overwriteFlag)
        {
            journal.overwriteFlag = false;
            StartCoroutine(OverwriteExit());
        }
        else
        {
            isOn = false;
            jOptions.gameObject.SetActive(false);

            galleryStorageUI.SetActive(false);
            frameInfo.SetActive(false);
            galleryUIstart.SetActive(true);
        }
        
    }

    private IEnumerator OverwriteExit()
    {
        
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        print("Frames waited, exiting menu to trigger autosave");
        isOn = false;

        jOptions.gameObject.SetActive(false);
        galleryStorageUI.SetActive(false);
        frameInfo.SetActive(false);
        galleryUIstart.SetActive(true);
    }

    public void FrameDetailToJournal()
    {
        photo grabPhoto = gallery[frameIndex];

        //Selection screen will use top 3 entries within inView
        // This is with the assumption that the ImageScanner will
        // order inView based on pixel prominence (amount of hits)
        jOptions.PreloadOptions(grabPhoto.inView);
        jOptions.gameObject.SetActive(true);
        
        /*if (grabPhoto.inView.Length == 0)
        {
          
        }
        else
        {
            // activate selection gui

            
        }*/


    }

    public void FrameDetailDelete()
    {
        
        gallery.RemoveAt(frameIndex);

        foreach(EditableObject wall in wallList)
        {
            bool result = wall.DictChange(frameIndex);
            if (result)
            {
                print("removed frame from wall");
                break;
            } 
        }
        Destroy(currFrame.transform.parent.gameObject);
        StartCoroutine(GalleryDelete());
        
        FrameDetailsClose();

        //Clean-up
        StartCoroutine(ReformatGR(frameIndex));
    }

    private IEnumerator GalleryDelete()
    {
        string filePath = Application.persistentDataPath + "/PhotoStorage/" + cameraRoll.profileName + "/GalleryRoll/" + frameIndex + ".png";
        File.Delete(filePath);
        yield return new WaitForEndOfFrame();

        
    }

    private IEnumerator ReformatGR(int removedIndex)
    {
        print("REMOVED GALLERY INDEX: " + removedIndex);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        string pathNoFile = Application.persistentDataPath + "/PhotoStorage/" + cameraRoll.profileName + "/GalleryRoll/";
        DirectoryInfo info = new DirectoryInfo(pathNoFile);
        FileInfo[] fileInfo = info.GetFiles();
        int index = 0;
        if (gallery.Count != 0)
        {
            int numFiles = fileInfo.Length;
            print("Files Detected: " + numFiles);
            for (int i = 0; i < numFiles; i++)
            {
                FileInfo f = fileInfo[i];
                if (!f.Name.Contains("meta"))
                {

                    char oldName = f.Name[0];
                    if (index >= removedIndex)
                    {
                        print("old name: " + oldName + ".png");
                        print("new name: " + index + ".png");
                        string oldFPath = pathNoFile + oldName + ".png";
                        string newFPath = pathNoFile + index + ".png";
                        //FileUtil.CopyFileOrDirectory(oldFPath, newFPath);
                        File.Copy(oldFPath, newFPath);
                        yield return new WaitForEndOfFrame();
                        photo grab = gallery[index];
                        photo newPhoto = new photo
                        {
                            fileName = newFPath,
                            inView = grab.inView,
                            captureData = grab.captureData,
                            inStorage = grab.inStorage,
                            totalScore = grab.totalScore,
                            scale = grab.scale,
                            wallName = grab.wallName,
                            q = grab.q,
                            wallX = grab.wallX,
                            wallY = grab.wallY,
                            wallZ = grab.wallZ
                        };
                        gallery.RemoveAt(index);
                        gallery.Insert(index, newPhoto);

                        //Physical frames need names changed too
                        string oldFrame = "frame" + oldName;
                        print("Old Frame name: " + oldFrame);
                        string newFrame = "frame" + index;
                        print("New Frame name: " + newFrame);
                        GameObject frame = GameObject.Find(oldFrame);
                        yield return new WaitForEndOfFrame();
                        frame.name = newFrame;
                        /*
                        foreach (EditableObject wall in wallList)
                        {
                            if(wall.wallName == newPhoto.wallName)
                            {
                                
                                break;
                            }
                        }
                        */
                        grab.fileName = newFPath;
                        yield return new WaitForEndOfFrame();

                        print("Deleting old file: " + oldFPath);
                        //bool result = FileUtil.DeleteFileOrDirectory(oldFPath);
                        File.Delete(oldFPath);
                        /*if (result)
                        {
                            print("Deleted file!");
                        } else
                        {
                            print("Failed to delete file. Trying again after wait.");
                            yield return new WaitForEndOfFrame();
                            FileUtil.DeleteFileOrDirectory(oldFPath);
                        }*/
                        yield return new WaitForEndOfFrame();
                        //AssetDatabase.Refresh();
                    }
                    index++;
                }
            }
            print("Files Used: " + index);
        }

        yield return new WaitForEndOfFrame();

    }

    public void FrameDetailsClose()
    {
        isOn = false;
        galleryUIstart.SetActive(true);
        frameInfo.SetActive(false);
        galleryStorageUI.SetActive(false);
    }

    public photo ReceivePhoto(int crIndex, photo grab)
    {
        //crIndex is the moved camera roll index - in case it's needed
        

        lastIndex = gallery.Count;
        Debug.Log("Recieved Photo in gallery storage - current count: " + lastIndex);
        if (lastIndex < 0)
        {
            // first gallery placement
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
            totalScore = grab.totalScore,
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
        rescale.ResetRect();
        scaleModifier = chosenScale;
        editingStorageOn = true;
        StartCoroutine(storageEditingMode());
        ExitGalleryStorage();
    }

    public void ManageGallery()
    {
        print("Manage Gallery started");
    }

    public void FinishStoragePlace(GameObject frame, Vector3 storePoint)
    {
        Image display = frame.GetComponentInChildren<Image>();
        frame.name = "frame" + lastIndex;
        string newWallName = frame.transform.parent.gameObject.name;
        photo grabP = gallery[lastIndex];
        gallery.Remove(grabP);
        Vector3 newScale = frame.transform.localScale;
        Vector3 localStore = frame.transform.localPosition;
        Quaternion newQ = frame.transform.rotation;
        Vector3 euler = frame.transform.localEulerAngles;
        //Create new photo bc I cant write to existing structs
        photo newPhoto = new photo
        {
            fileName = grabP.fileName,
            captureData = grabP.captureData,
            inView = grabP.inView,
            inStorage = 1,
            totalScore = grabP.totalScore,
            wallX = localStore.x,
            wallY = localStore.y,
            wallZ = localStore.z,
            wallName = newWallName,
            scale = newScale,
            q = newQ
        };
        
        //replace where the grabbed photo was
        gallery.Insert(lastIndex, newPhoto);
        Sprite newS = Sprite.Create(grabP.captureData, new Rect(0.0f, 0.0f, grabP.captureData.width, grabP.captureData.height), new Vector2(0.0f, 0.0f), display.pixelsPerUnit);
        display.sprite = newS;
        Color holdColor = new Color(255f, 255f, 255f);
        display.color = holdColor;
        editingStorageOn = false;
        isOn = false;
        scaleModifier = 3;
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

        
        using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fs.Seek(0, SeekOrigin.End);
            await fs.WriteAsync(rawData, 0, rawData.Length);
        }
        
        

        print("File Written.");

    }
}
