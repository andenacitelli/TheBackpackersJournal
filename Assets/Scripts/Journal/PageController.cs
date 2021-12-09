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
    //We can calc. silver(+2), gold(+1), & plat(+2) from this value
    private int bronzeScore;
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
            } else if (childName.Contains("score"))
            {
                scoringText = childObj.GetComponent<TextMeshProUGUI>();
            }
        }

        pageTitle = titleText.text;
        DetermineBronzeScore();
        //print(pageTitle + " page was initialized correctly");
    }
    

    private void DetermineBronzeScore()
    {
        switch (pageTitle)
        {
            case "Bear":
                bronzeScore = 4;
                break;
            case "Boar":
                bronzeScore = 4;
                break;
            case "Cougar":
                bronzeScore = 5;
                break;
            case "Cow":
                bronzeScore = 3;
                break;
            case "Deer":
                bronzeScore = 3;
                break;
            case "Fox":
                bronzeScore = 5;
                break;
            case "Horse":
                bronzeScore = 4;
                break;
            case "Moose":
                bronzeScore = 4;
                break;
            case "Rabbit":
                bronzeScore = 3;
                break;
            case "Raccoon":
                bronzeScore = 3;
                break;
            case "Tiger":
                bronzeScore = 5;
                break;
            case "Wolf":
                bronzeScore = 5;
                break;
            default:
                break;
        }
    }

    public void UpdateImage(Texture2D t2d)
    {
        pagePhoto.color = new Color(pagePhoto.color.r, pagePhoto.color.g, pagePhoto.color.b, 1);
        pagePhoto.sprite = Sprite.Create(t2d, new Rect(0.0f, 0.0f, t2d.width, t2d.height), new Vector2(0.0f, 0.0f), pagePhoto.pixelsPerUnit);
    }

    public void UpdateText(int totalScore)
    {
        // issue here with only deer? showing as unranked - idk why tho, its been a night
        string text;
        Debug.Log(pageTitle + ": brone value: " + bronzeScore);
        Debug.Log(totalScore);
        Debug.Log("-------------");
        if(totalScore == bronzeScore || totalScore == (bronzeScore +1))
        {
            text = "Bronze (" + totalScore + ")";
        } else if (totalScore == (bronzeScore + 2))
        {
            text = "Silver (" + totalScore + ")";
        } else if (totalScore == (bronzeScore + 3))
        {
            text = "Gold (" + totalScore + ")";
        } else if(totalScore >= (bronzeScore + 5))
        {
            text = "Plat (" + totalScore + ")";
        } else
        {
            text = "Unranked (" + totalScore + ")";
        }

        scoringText.text = text;
    }
}
