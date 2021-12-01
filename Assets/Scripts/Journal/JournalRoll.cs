using BookCurlPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JournalRoll : MonoBehaviour
{
    private BookPro book;
    public List<photo> photos;
    public GalleryStorage gallery;
    List<PageController> pages;

    public int pageNum;

    private void Awake()
    {
        print("JournalRoll-Awake");
        if(pages == null)
        {
            pages = new List<PageController>();
        }
        
        book = GetComponent<BookPro>();

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
    }
    // Start is called before the first frame update
    void Start()
    {
        
        
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
        print("ProfileNAME: " + s.playerName);
        string profileName = s.playerName;
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

                    photo grabPhoto = new photo
                    {
                        captureData = t2D,
                        fileName = absolutePath,
                        inView = loadArray[index].inView,
                        totalScore = loadArray[index].totalScore
                    };
                    RecievePhoto(loadArray[index], fName);
                    index++;
                }


            }
            pageNum = index;
        }


    }


    public void RecievePhoto(photo grabPhoto, string pageChoice, int loadOption=0)
    {
        // Not loading
        if(loadOption == 0)
        {
            
            PageController pCont = QueryPage(pageChoice);
            print("Applying to journal page: " + pCont);
            string fileName = Application.persistentDataPath + "/PhotoStorage/" + gallery.cameraRoll.profileName + "/JournalRoll/" + pageChoice + ".png";
            SavePhotoToPageList(grabPhoto, fileName, pageChoice, pCont);
        }
        else
        {

        }
        
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
            pageName = pChoice
        };

        photos.Add(newPhoto);

        pCont.UpdateImage(grabPhoto.captureData);
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
