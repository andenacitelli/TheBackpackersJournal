using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableGUI : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject cursorGO;
    public GameObject interactableGO;

    private InteractableObject obj;
    private Image cursor;
    private Vector3 middleV = new Vector3(0.5f, 0.5f, 0);
    private bool halfVisible;
    private bool fullVisible;
    private bool buttonPressed;
    public bool cursorOn { get; set; }
    public void Start()
    {
        cursorOn = false;
        halfVisible = false;
        fullVisible = false;
        buttonPressed = false;
        cursor = cursorGO.GetComponent<Image>();
        obj = interactableGO.GetComponent<InteractableObject>();
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
            } else
            {            
                cursorOn = false;
                StopCoroutine(cursorOpacity());
            }
        }
    }

    //When user engages Interact action
    public void OnUserInteract(float val)
    {
        if (fullVisible && !buttonPressed)
        {
            obj.CallUIEvent();
            print("BUTTON PRESSSS: " + val);
            buttonPressed = true;
        }
    }

    IEnumerator cursorOpacity()
    {
        while (true)
        {
            Ray screenMiddle = Camera.main.ViewportPointToRay(middleV);
            RaycastHit hit;
            
            if (Physics.Raycast(screenMiddle, out hit, 6f))
            {
                GameObject grab = hit.transform.gameObject;
                if (grab.CompareTag("InteractableObject"))
                {
                    if(grab != interactableGO)
                    {
                        Debug.Log("Switched obj in InteractableGUI");
                        interactableGO = grab;
                        obj = interactableGO.GetComponent<InteractableObject>();
                    }
                    
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
            yield return new WaitForSeconds(.25f);
        }
    }

    private void CleanUpHelper()
    {
        buttonPressed = false;
        halfVisible = false;
        fullVisible = false;
        var tempColor = cursor.color;
        tempColor.a = 0f;
        cursor.color = tempColor;
    }
}
