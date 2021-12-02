using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryScaleUI : MonoBehaviour
{
    // Needs to be the image & not the image from getcomponent >> causes weird ass bug
    [Header("GUI")]
    public Image image;
    public RectTransform imageGUIObj;
    public TextMeshProUGUI text;

    private string[] sizeList = { "XXSmall", "XSmall", "Small", "Medium", "Large", "XLarge" };
    private Vector2[] sizeValues = new Vector2[6];
    public Vector2 resetSize;

    public int currentSizeIndex;


    private void Start()
    {
        currentSizeIndex = 3;
        resetSize = imageGUIObj.sizeDelta;
    }

    public void PreloadPhoto(Texture2D picData)
    {
        image.sprite = Sprite.Create(picData, new Rect(0.0f, 0.0f, picData.width, picData.height), new Vector2(0.0f, 0.0f), image.pixelsPerUnit);
        Vector2 sizeV2 = imageGUIObj.sizeDelta;
        sizeValues[0] = RatioVector2(sizeV2, -.75f);
        sizeValues[1] = RatioVector2(sizeV2, -.50f);
        sizeValues[2] = RatioVector2(sizeV2, -.20f);
        sizeValues[3] = RatioVector2(sizeV2, 0f);
        sizeValues[4] = RatioVector2(sizeV2, .20f);
        sizeValues[5] = RatioVector2(sizeV2, .50f);
    }

    public void IncreaseScale()
    {
        if (!OverflowStopper(1))
        {
            float newHorSize = sizeValues[currentSizeIndex].x;
            float newVertSize = sizeValues[currentSizeIndex].y;
            imageGUIObj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newVertSize);
            imageGUIObj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newHorSize);
            text.text = sizeList[currentSizeIndex];

        }
        
    }

    public void DecreaseScale()
    {
        if (!OverflowStopper(-1))
        {
            float newHorSize = sizeValues[currentSizeIndex].x;
            float newVertSize = sizeValues[currentSizeIndex].y;
            imageGUIObj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newVertSize);
            imageGUIObj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newHorSize);
            text.text = sizeList[currentSizeIndex];
        }
        
    }

    public bool OverflowStopper(int change)
    {
        // returns true if overflow prevented advancing list
        bool didOverFlow = false;
        int newIndex = currentSizeIndex + change;
        //print("Prospective new index: " + newIndex);
        if (newIndex == sizeList.Length)
        {
            newIndex--;
            didOverFlow = true;
        } else if(newIndex == -1)
        {
            newIndex++;
            didOverFlow = true;
        }
        //print("Actual new index: " + newIndex);

        currentSizeIndex = newIndex;

        return didOverFlow;
    }

    private Vector2 RatioVector2(Vector2 org, float ratio)
    {
        float xVal = org.x;
        float yVal = org.y;

        float newX;
        float newY;

        if(ratio > 0)
        {
            //Positive, so add ratio adds
            newX = org.x + (org.x * ratio);
            newY = org.y + (org.y * ratio);
            
        } else
        {
            //negative, so ratio subs
            newX = org.x - (org.x * Mathf.Abs(ratio));
            newY = org.y - (org.y * Mathf.Abs(ratio));
        }


        return new Vector2(newX, newY);
    }

    public void ResetRect()
    {
        imageGUIObj.sizeDelta = resetSize;
        currentSizeIndex = 3;
    }
}
