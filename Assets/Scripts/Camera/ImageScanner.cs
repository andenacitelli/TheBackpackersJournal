using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;

public class ImageScanner : MonoBehaviour
{
    [SerializeField] public Camera mainCam;
    //Pixels are checked as 0*Undersample, 1*Undersample, 2*Undersample
    private const int UNDERSAMPLE = 20;
    private int width;
    private int height;
    
    private List<string> inView;
    private Dictionary<string, int> inViewDict;

    // Start is called before the first frame update
    void Start()
    {
        width = mainCam.pixelWidth;
        height = mainCam.pixelHeight;     
    }

    public List<string> ScanFrame()
    {
        
        inViewDict = new Dictionary<string, int>();
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
                    
                    CanPhoto canP;
                    if (hit.transform.gameObject.TryGetComponent<CanPhoto>(out canP))
                    {
                        string token = canP.ReportToken();
                        if (!inViewDict.ContainsKey(token))
                        {
                            inViewDict.Add(token, 1);
                        } else
                        {
                            int grabCount = inViewDict[token];
                            grabCount++;
                            inViewDict[token] = grabCount;
                        }
                    }
                    /*Old test code:
                     * string objName = hit.transform.gameObject.name;
                    if (hit.transform.gameObject.CompareTag("Box") && !inView.Contains(hit.transform.gameObject.name))
                    {
                        inView.Add(hit.transform.gameObject.name);
                    }*/

                }

            }
            
        }
        // sort by highest value
        var sortedDict = from entry in inViewDict orderby entry.Value descending select entry;
        inViewDict = sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
        inView = new List<string>(inViewDict.Keys);
        return inView;
    }

    public int ScoreView(List<string> currView)
    {
        int totalScore = 0;
        
        foreach(string token in currView)
        {
            int splitIndex = token.IndexOf('_');
            string animalName = token.Substring(0, splitIndex);
            string animalAction = token.Substring(splitIndex + 1);
            // kinda sloppy but makes it easiest to account for male/female
            if (animalName.Contains("Deer"))
            {
                animalName = "Deer";   
            }
            // Animal Score
            switch (animalName)
            {
                case "Bear":
                    totalScore += 2;
                    break;
                case "Boar":
                    totalScore += 2;
                    break;
                case "Cougar":
                    totalScore += 3;
                    break;
                case "Cow":
                    totalScore += 1;
                    break;
                case "Deer":
                    totalScore += 1;
                    break;
                case "Fox":
                    totalScore += 3;
                    break;
                case "Horse":
                    totalScore += 2;
                    break;
                case "Moose":
                    totalScore += 2;
                    break;
                case "Rabbit":
                    totalScore += 1;
                    break;
                case "Raccoon":
                    totalScore += 1;
                    break;
                case "Tiger":
                    totalScore += 3;
                    break;
                case "Wolf":
                    totalScore += 3;
                    break;
                default:
                    break;
            }

            //behavior score
            switch (animalAction)
            {
                case "Run":
                    totalScore += 1;
                    break;
                case "Death":
                    totalScore += 3;
                    break;
                case "Attack":
                    totalScore += 4;
                    break;
                case "Eating":
                    totalScore += 2;
                    break;
                case "Howl":
                    totalScore += 3;
                    break;
                case "Roar":
                    totalScore += 3;
                    break;
                case "Sit":
                    totalScore += 2;
                    break;
                case "LayingDown":
                    totalScore += 2;
                    break;
                default:
                    break;
            }
        }
        return totalScore;
    }
    
}
