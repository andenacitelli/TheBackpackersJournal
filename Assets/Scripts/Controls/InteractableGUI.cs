using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableGUI : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject cursorGO;

    private Image cursor;
    private Vector3 middleV = new Vector3(0.5f, 0.5f, 0);
    private bool halfVisible;
    private bool fullVisible;
    public bool cursorOn { get; set; }
    public void Start()
    {
        cursorOn = false;
        halfVisible = false;
        fullVisible = false;
        cursor = cursorGO.GetComponent<Image>();
    }

    public void ToggleCursor(bool pass)
    {
        if(cursorOn != pass)
        {
            if (pass)
            {
                
                StopCoroutine(cursorOpacity());
                cursorOn = true;
                StartCoroutine(cursorOpacity());
                print("Started coroutine");
            } else
            {
                
                cursorOn = false;
                StopCoroutine(cursorOpacity());
                print("stopped coroutine");
            }
        }
    }

    IEnumerator cursorOpacity()
    {
        while (true)
        {
            Ray screenMiddle = Camera.main.ViewportPointToRay(middleV);
            RaycastHit hit;

            if (Physics.Raycast(screenMiddle, out hit, 10f))
            {
                if (hit.transform.gameObject.CompareTag("InteractableObject"))
                {
                    Debug.Log("Hit!");
                    if (hit.collider.isTrigger && !fullVisible)
                    {
                        var tempColor = cursor.color;
                        tempColor.a = 1f;
                        cursor.color = tempColor;
                        fullVisible = true;
                    }
                    else if(!halfVisible)
                    {
                        var tempColor = cursor.color;
                        tempColor.a = .5f;
                        cursor.color = tempColor;
                        halfVisible = true;
                    }
                    
                } else if (halfVisible || fullVisible)
                {
                    CleanUpHelper();
                }
            } else if (halfVisible || fullVisible)
            {
                CleanUpHelper();
            }
            yield return new WaitForSeconds(.5f);
        }
    }

    private void CleanUpHelper()
    {
        halfVisible = false;
        fullVisible = false;
        var tempColor = cursor.color;
        tempColor.a = 0f;
        cursor.color = tempColor;
    }
}
