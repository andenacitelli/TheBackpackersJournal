using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableGUI : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject cursorGO;
    public GameObject interactableGO;
    public GameObject editableGO;
    public GameObject framePrefab;
    public GameObject gallery;

    private InteractableObject obj;
    private EditableObject obj2;
    private Image cursor;
    private Vector3 middleV = new Vector3(0.5f, 0.5f, 0);
    private bool halfVisible;
    private bool fullVisible;
    private bool buttonPressed;
    private bool showSampleCanvas;
    private bool isFrame;
    private GameObject placeObject;
    private Image display;
    private GalleryStorage storage;
    public bool cursorOn { get; set; }
    public void Start()
    {
        cursorOn = false;
        halfVisible = false;
        fullVisible = false;
        buttonPressed = false;
        cursor = cursorGO.GetComponent<Image>();
        obj = interactableGO.GetComponent<InteractableObject>();
        obj2 = editableGO.GetComponent<EditableObject>();
        storage = gallery.GetComponent<GalleryStorage>();
    }

    public void ToggleCursor(bool pass, bool editMode, int scaleVal)
    {
        if(cursorOn != pass)
        {
            if (pass)
            {
                cursorOn = true;
                if (editMode)
                {
                    StopCoroutine(cursorOpacity());
                    placeObject = Instantiate(framePrefab);
                    //print("OptionalScale " + scaleVal);
                    Vector3 scale = DetermineScale(scaleVal);
                    placeObject.transform.localScale = scale;
                    display = placeObject.GetComponentInChildren<Image>();
                    StartCoroutine(editCursor());
                } else
                {
                    StartCoroutine(cursorOpacity());
                }
                
            } else
            {
                cursorOn = false;
                StopAllCoroutines();
                
            }
        }
    }

    //When user engages Interact action
    public void OnUserInteract(float val)
    {
        if (fullVisible && (isFrame || !buttonPressed))
        {
            if(isFrame)
            {
                storage.FrameDetailsOpen(placeObject);
            } else if (!buttonPressed)
            {
                var tempColor = cursor.color;
                tempColor.a = 0f;
                cursor.color = tempColor;
                obj.CallUIEvent();
                buttonPressed = true;
            }
            
        } else if (showSampleCanvas)
        {
            StopCoroutine(editCursor());
            obj2.StoreOnWall(placeObject);
            showSampleCanvas = false;
            print("Placed Canvas");
        }
    }

    IEnumerator editCursor()
    {
        while (true)
        {
            Ray screenMiddle = Camera.main.ViewportPointToRay(middleV);
            RaycastHit hit;
            

            if (Physics.Raycast(screenMiddle, out hit, 8f))
            {
                GameObject grab = hit.transform.gameObject;
                if (grab.CompareTag("EditableObject"))
                {
                    EditableObject wall = grab.GetComponent<EditableObject>();
                    //for finding new walls to edit on:
                    if (grab != editableGO)
                    {
                        Debug.Log("Switched obj in EditableGUI");
                        editableGO = grab;
                        obj2 = wall;
                    }

                    placeObject.transform.position = hit.point;
                    Vector3 normal = hit.point + hit.normal.normalized;

                    placeObject.transform.LookAt(normal);
                    
                    
                    if (!showSampleCanvas)
                    {
                        placeObject.SetActive(true);
                        var tempColor = cursor.color;
                        tempColor.a = 1f;
                        cursor.color = tempColor;
                        showSampleCanvas = true;
                    }
                    showSampleCanvas = wall.CanPlaceHere(placeObject);
                    SetPlaceColor(showSampleCanvas);


                } else if (grab.CompareTag("PlaceableObject"))
                {
                    //For picking up picture frames? - need to add editing buffer & check if it's not full

                }
                else if (showSampleCanvas)
                {
                    CleanUpHelperEdit();
                }
            }
            else if (showSampleCanvas)
            {
                CleanUpHelperEdit();
            }
            yield return new WaitForSeconds(.25f);
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
                    if (grab != interactableGO)
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
                    else if (!halfVisible)
                    {
                        var tempColor = cursor.color;
                        tempColor.a = .5f;
                        cursor.color = tempColor;
                        halfVisible = true;
                    }

                } else if (grab.CompareTag("PlaceableObject")) 
                {
                    
                    if (!fullVisible)
                    {
                        
                        var tempColor = cursor.color;
                        tempColor.a = 1f;
                        cursor.color = tempColor;
                        fullVisible = true;
                    }
                    placeObject = grab;
                    isFrame = true;


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
        isFrame = false;
        var tempColor = cursor.color;
        tempColor.a = 0f;
        cursor.color = tempColor;
    }

    private void CleanUpHelperEdit()
    {
        showSampleCanvas = false;
        placeObject.SetActive(false);
        var tempColor = cursor.color;
        tempColor.a = 0f;
        cursor.color = tempColor;
    }

    private void SetPlaceColor(bool canPlace)
    {
        Color newC;
        if (canPlace)
        {
            newC = new Color(0f, 255f, 0f);
        }
        else
        {
            newC = new Color(255f, 0f, 0f);
        }
        
        display.color = newC;
    }

    //Scale is an int where 0 = 40%, 1 = 50%, 2 = 80%, 3 = 100%, 4 = 120%, 5 = 150% (of original size)
    private Vector3 DetermineScale(int scaleCode)
    {
        float x, y;
        switch (scaleCode)
        {
            case 0:
                x = .4f;
                break;
            case 1:
                x = .5f;
                break;
            case 2:
                x = .8f;
                break;
            case 3:
                x = 1f;
                break;
            case 4:
                x = 1.2f;
                break;
            case 5:
                x = 1.5f;
                break;
            default:
                print("Couldn't determine scale, see InteractableGUI");
                x = 0;
                break;
        }
        y = x * .8f;
        Debug.Log("Local Scale - x: " + x + " y: " + y);
        return new Vector3(x, y, .05f);

        
    }
}
