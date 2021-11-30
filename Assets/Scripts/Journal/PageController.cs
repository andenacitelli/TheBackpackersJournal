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
    void Start()
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
    
}
