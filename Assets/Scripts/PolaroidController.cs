using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolaroidController : MonoBehaviour
{
    [Header("GUI Components")]
    public RectTransform shutter;
    public GameObject overlay;
    [Header("Parent Camera")]
    [SerializeField] GameObject polaroidPrefab;
    
    private const int WORLD_CURRENT = 0;
    private const int WORLD_END = 1;
    private const int WORLD_START = 2;
    private Vector3 LOCAL_START = new Vector3(0f, -1f, 1.3f);
    private Vector3 LOCAL_END = new Vector3(0f, 0f, .55f);
    private bool aimEngaged, aimDisengaged, aimRunning;
    private bool shutterOn, shutterHalved, shutterStopped;
    private bool photoActive;
    private float shutterStop, shutterWidth;
    private float raiseHalfDist, shutterT;
    

    private void Start()
    {
        aimRunning = false;
        photoActive = false;
        shutterStop = (float)Screen.height;
        shutterT = shutterStop / 10;
        shutterWidth = shutter.sizeDelta.x;
        Vector3[] worldPos = ConvertToWorldPoints();
        raiseHalfDist = Vector3.Distance(worldPos[WORLD_START], worldPos[WORLD_END])/2;
        
    }

    public void OnRaiseInput(float secondary)
    {
        Debug.Log($"On Raise Start: {secondary}");
        if (!aimRunning && secondary == 1)
        {  
            StartCoroutine("EngagedAim");
        } else if(secondary == 0)
        {
            //aimEngaged = false;
            aimDisengaged = true;
        }
        
    }

    public void OnPrimaryInput(float primary)
    {
        
        if (photoActive )
        {
            Debug.Log($"Took a picture with: {primary}");
        } else
        {
            Debug.Log("No Aim. No picture.");
        }
    }

    private IEnumerator EngagedAim()
    {
        aimRunning = true;
        //Debug.Log("EngagedAim started");
        aimEngaged = true;
        aimDisengaged = false;
        shutterOn = false;
        shutterHalved = false;
        shutterStopped = false;

        Vector3[] worldPos = ConvertToWorldPoints();
        float currentShutter= 0;
        
        while (aimEngaged && (!shutterStopped && Vector3.Distance(worldPos[WORLD_CURRENT], worldPos[WORLD_END]) >= .005))
        {
            worldPos = ConvertToWorldPoints();
            transform.position = Vector3.Lerp(worldPos[WORLD_CURRENT], worldPos[WORLD_END], .1f);
            if(!shutterOn && Vector3.Distance(worldPos[WORLD_CURRENT], worldPos[WORLD_END]) > raiseHalfDist )
            {
                shutterOn = true;
            } else if (shutterOn && !shutterHalved)
            {
                if(!shutterHalved && currentShutter < shutterStop)
                {
                    currentShutter += shutterT;
                    shutter.sizeDelta = new Vector2(shutterWidth, currentShutter);
                } else {
                    shutterHalved = true;
                    polaroidPrefab.SetActive(false);
                    overlay.SetActive(true);
                }
                
            } else if (shutterOn && shutterHalved) 
            {
                if (!shutterStopped && currentShutter > 0)
                {
                    currentShutter -= shutterT;
                    shutter.sizeDelta = new Vector2(shutterWidth, currentShutter);
                }
                else
                {
                    shutterStopped = true;
                    shutterHalved = false;
                    shutterOn = false;
                }
                
            }
            yield return new WaitForSeconds(.01f);   
        }

        while (!aimDisengaged)
        {
            if (!photoActive)
            {
                photoActive = true;
            }
            yield return new WaitForSeconds(.01f);
        }

        //Player Released Aim
        photoActive = false;
        aimEngaged = false;
        shutterStopped = false;
        
        while (Vector3.Distance(worldPos[WORLD_CURRENT], worldPos[WORLD_START]) >= .005)
        {
            if (!shutterOn && !shutterStopped)
            {
                shutterOn = true;
            }
            else if (shutterOn && !shutterHalved && !shutterStopped)
            {
                if (!shutterHalved && currentShutter < shutterStop)
                {
                    currentShutter += shutterT;
                    shutter.sizeDelta = new Vector2(shutterWidth, currentShutter);
                }
                else
                {
                    shutterHalved = true;
                    polaroidPrefab.SetActive(true);
                    overlay.SetActive(false);
                }

            }
            else if (shutterOn && shutterHalved && !shutterStopped)
            {
                if (!shutterStopped && currentShutter > 0)
                {
                    currentShutter -= shutterT;
                    shutter.sizeDelta = new Vector2(shutterWidth, currentShutter);
                }
                else
                {
                    shutterStopped = true;
                    shutterHalved = false;
                    shutterOn = false;
                }

            }

            worldPos = ConvertToWorldPoints();
            transform.position = Vector3.Lerp(worldPos[WORLD_CURRENT], worldPos[WORLD_START], .08f);
            
            yield return new WaitForSeconds(.01f);
        }

        if (!aimEngaged && aimDisengaged)
        {
            //Debug.Log("EngagedAim ended");
            aimRunning = false;
            yield break;
        }
    }

    private Vector3[] ConvertToWorldPoints()
    {
        Vector3[] retArr = new Vector3[3];
        retArr[WORLD_CURRENT] = transform.position;
        
        retArr[WORLD_END] = gameObject.transform.parent.TransformPoint(LOCAL_END);
        
        retArr[WORLD_START] = gameObject.transform.parent.TransformPoint(LOCAL_START);
        return retArr;
    }
}
