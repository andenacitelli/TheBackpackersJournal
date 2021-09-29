using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public delegate void SendViewInput(int change);
public class CRPhotoView : MonoBehaviour
{
    public Image uiPhoto;
    public CameraRollMenu crUI;
    public int currIndex { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        currIndex = -1;
    }

    public void NextButtonPress()
    {
        SendViewInput change = crUI.PhotoViewResponse;
        change(1);
    }

    public void LastButtonPress()
    {
        SendViewInput change = crUI.PhotoViewResponse;
        change(-1);
    }

    public void ReturnPress()
    {
        SendViewInput change = crUI.PhotoViewResponse;
        change(0);
    }

    public void AssignPicture(Texture2D photo, int index)
    {
        currIndex = index;
        uiPhoto.sprite = Sprite.Create(photo, new Rect(0.0f, 0.0f, Screen.width, Screen.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
