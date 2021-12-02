using BookCurlPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JournalRoll : MonoBehaviour
{
    private BookPro book;
    public List<photo> photos;
    public GalleryStorage gallery;
    List<PageController> pages;

    public int pageNum;
    public Save saveFromGM;
    private string profileName;
    public bool overwriteFlag;

    private void Awake()
    {
        
        if(pages == null)
        {
            print("JournalRoll- create pages");
            pages = new List<PageController>();
            
        }
        if(photos == null)
        {
            photos = new List<photo>();
        }
        
        book = GetComponent<BookPro>();

       
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (Paper page in book.papers)
        {
            if (page.Back.TryGetComponent(out PageController pageCont))
            {
                pages.Add(pageCont);
            }

            if (page.Front.TryGetComponent(out PageController pageCont2))
            {
                pages.Add(pageCont2);
            }
        }
        LoadJRoll(saveFromGM);
        //prolly load here
        /* Debug print for pages
        foreach(PageController pCont in pages)
        {
            print(pCont.pageTitle + " page successfully retrieved");
        }
        -- The pages list is unordered, but can be queried using the pageTitle
        */
    }

    public void LoadJRoll(Save s)
    {
        //print("ProfileNAME: " + s.playerName);
        profileName = s.playerName;
        photos = new List<photo>();
        if (profileName != "")
        {
            string pathNoFile = Application.persistentDataPath + "/PhotoStorage/" + profileName + "/JournalRoll/";
            DirectoryInfo info = new DirectoryInfo(pathNoFile);
            /*if (!info.Exists)
            {
                info.Create();
            }*/
            FileInfo[] fileInfo = info.GetFiles();
            int index = 0;
            photo[] loadArray = s.jRoll;
            foreach (FileInfo f in fileInfo)
            {
                if (!f.Name.Contains("meta"))
                {
                    int cutOff = f.Name.IndexOf('.');
                    string fName = f.Name.Substring(0, cutOff + 1);
                    string path = "/PhotoStorage/" + profileName + "/JournalRoll/" + fName;
                    string absolutePath = pathNoFile + f.Name;
                    Texture2D t2D = new Texture2D(Screen.width, Screen.height);

                    byte[] data = File.ReadAllBytes(absolutePath);
                    ImageConversion.LoadImage(t2D, data);
                    t2D.Apply();
                    //yield return new WaitForEndOfFrame();

                    photo grabPhoto = new photo
                    {
                        captureData = t2D,
                        fileName = absolutePath,
                        inView = loadArray[index].inView,
                        totalScore = loadArray[index].totalScore
                    };
                    RecievePhoto(grabPhoto, fName, 1);
                    index++;
                }


            }
            pageNum = index;
        }


    }


    public void RecievePhoto(photo grabPhoto, string pageChoice, int loadOption=0)
    {


        PageController pCont = QueryPage(pageChoice);
        
        //print("Applying to journal page: " + pCont);
        string fileName = Application.persistentDataPath + "/PhotoStorage/" + gallery.cameraRoll.profileName + "/JournalRoll/" + pCont.pageTitle + ".png";
       

        if (loadOption == 0)
        {
            //ISSUE HERE
            
            //overwrite check, we aren't loading
            if (File.Exists(fileName))
            {
                //remove photo from photo list
                for (int i = 0; i < photos.Count; i++)
                {
                    if (photos[i].fileName.Equals(fileName))
                    {
                        photos.RemoveAt(i);
                        break;
                    }
                }

                overwriteFlag = true;
                print("Overwriting...");
                File.Delete(fileName);
            }
        }

        SavePhotoToPageList(grabPhoto, fileName, pageChoice, pCont);
        gallery.FrameJournalSelectExit();
        
        
    }

    private void SavePhotoToPageList(photo grabPhoto, string newFName, string pChoice, PageController pCont)
    {
        

        photo newPhoto = new photo
        {
            captureData = grabPhoto.captureData,
            fileName = newFName,
            inView = grabPhoto.inView,
            inStorage = 0,
            totalScore = grabPhoto.totalScore,
            pageName = pCont.pageTitle
        };

        photos.Add(newPhoto);

        pCont.UpdateImage(grabPhoto.captureData);
        //update scoring info
    }


    public PageController QueryPage(string pageChoice)
    {
        PageController retPage = null;
        foreach(PageController page in pages)
        {
            if (pageChoice.Contains(page.pageTitle))
            {
                print("Query matched " + pageChoice + " choice with exisiting page: " + page.pageTitle);
                retPage = page;
                break;
            }
        }
        return retPage;
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
