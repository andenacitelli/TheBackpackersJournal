using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class ImageScanner : MonoBehaviour
{
    [SerializeField] public Camera mainCam;
    //Pixels are checked as 0*Undersample, 1*Undersample, 2*Undersample
    private const int UNDERSAMPLE = 20;
    private int width;
    private int height;
    
    private List<GameObject> inView;

    // Start is called before the first frame update
    void Start()
    {
        width = mainCam.pixelWidth;
        height = mainCam.pixelHeight;     
    }

    public List<GameObject> ScanFrame()
    {
        inView = new List<GameObject>();
        for(int i = 0; i < width; i += UNDERSAMPLE)
        {
            float xPixel = (float)i;
            for(int j = 0; j < height; j += UNDERSAMPLE)
            {
                float yPixel = (float)j;
                
                Vector3 pixelPos = new Vector3(xPixel, yPixel, 0f);

                Ray ray = mainCam.ScreenPointToRay(pixelPos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    //Will need changed
                    if (hit.transform.gameObject.CompareTag("Box") && !inView.Contains(hit.transform.gameObject))
                    {
                        Debug.Log("Added to id list");
                        inView.Add(hit.transform.gameObject);
                    }
                    
                }
                       
            }
        }
        return inView;
    }
    
}
