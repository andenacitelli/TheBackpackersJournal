using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CRPopUp : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject fullAlert;
    public Image uiPhoto;

    void Start()
    {
        
    }

    public void AssignPicture(Texture2D photo)
    {
        uiPhoto.sprite = Sprite.Create(photo, new Rect(0.0f, 0.0f, Screen.width, Screen.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
