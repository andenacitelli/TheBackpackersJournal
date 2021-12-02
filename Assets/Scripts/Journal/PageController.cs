using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageController : MonoBehaviour
{
    public string pageTitle;
    Image pagePhoto;
    TextMeshProUGUI titleText;
    TextMeshProUGUI scoringText;
    // Start is called before the first frame update
    void Awake()
    {
        int pageChildCount = transform.childCount;
        for(int i = 0; i < pageChildCount; i++)
        {
            GameObject childObj = transform.GetChild(i).gameObject;
            string childName = childObj.name;
            if (childName.Contains("photo"))
            {
                //The photo object child
                pagePhoto = childObj.GetComponent<Image>();
            } else if (childName.Contains("animal"))
            {
                //The Page title object
                titleText = childObj.GetComponent<TextMeshProUGUI>();
            } else if (childName.Contains("scoring"))
            {
                scoringText = childObj.GetComponent<TextMeshProUGUI>();
            }
        }

        pageTitle = titleText.text;
        //print(pageTitle + " page was initialized correctly");
    }
    

    public void UpdateImage(Texture2D t2d)
    {
        pagePhoto.color = new Color(pagePhoto.color.r, pagePhoto.color.g, pagePhoto.color.b, 1);
        pagePhoto.sprite = Sprite.Create(t2d, new Rect(0.0f, 0.0f, t2d.width, t2d.height), new Vector2(0.0f, 0.0f), pagePhoto.pixelsPerUnit);
    }
}
