using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public delegate void SendGallerySelect(int val, bool replace);

public class CRUIGallery : MonoBehaviour
{
    // If treated as a page --> should have an offset +9 when index is referenced.`
    public GameObject p0;
    public GameObject p1;
    public GameObject p2;
    public GameObject p3;
    public GameObject p4;
    public GameObject p5;
    public GameObject p6;
    public GameObject p7;
    public GameObject p8;
    public CameraRollMenu crUI;

    //If isReplaceMode, gallery was opened with intention to replace photo.
    public bool isReplaceMode { get; set; }


    public void Awake()
    {
        isReplaceMode = false;
    }

    public void AssignPicture(int index, Texture2D photo)
    {
        GameObject pRef = DeterminePhoto(index);
        if(pRef != null)
        {
            Image uiPhoto = pRef.GetComponent<Image>();
            uiPhoto.sprite = Sprite.Create(photo, new Rect(0.0f, 0.0f, Screen.width, Screen.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }

    public void P0Click()
    {
        ClickResponse(0);
    }

    public void P1Click()
    {
        ClickResponse(1);
    }

    public void P2Click()
    {
        ClickResponse(2);
    }
    public void P3Click()
    {
        ClickResponse(3);
    }
    public void P4Click()
    {
        ClickResponse(4);
    }
    public void P5Click()
    {
        ClickResponse(5);
    }
    public void P6Click()
    {
        ClickResponse(6);
    }
    public void P7Click()
    {
        ClickResponse(7);
    }
    public void P8Click()
    {
        ClickResponse(8);
    }

    private void ClickResponse(int index)
    {
        SendGallerySelect send = crUI.RecieveGallerySelect;
        send(index, isReplaceMode);
    }

    private GameObject DeterminePhoto(int index)
    {
        GameObject photoPointer;
        #region parseIndex
        switch (index)
        {
            case 0:
                photoPointer = p0;
                break;
            case 1:
                photoPointer = p1;
                break;
            case 2:
                photoPointer = p2;
                break;
            case 3:
                photoPointer = p3;
                break;
            case 4:
                photoPointer = p4;
                break;
            case 5:
                photoPointer = p5;
                break;
            case 6:
                photoPointer = p6;
                break;
            case 7:
                photoPointer = p7;
                break;
            case 8:
                photoPointer = p8;
                break;
            default:
                photoPointer = null;
                break;
        }
        #endregion
        return photoPointer;
    }
}
