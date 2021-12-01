using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JournalOptionsGUI : MonoBehaviour
{

    public TextMeshProUGUI option1Text;
    public TextMeshProUGUI option2Text;
    public TextMeshProUGUI option3Text;
    public GameObject noOptionText;

    public GalleryStorage gallery;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void PreloadOptions(string[] options)
    {
        // This will apply the top 3 "inView" entries
        // options argument expected to have 3 or less elements
        int optionCount = options.Length;

        if(optionCount >= 3)
        {
            option1Text.text = options[0];
            option2Text.text = options[1];
            option3Text.text = options[2];
        } else if(optionCount == 2)
        {
            option1Text.text = options[0];
            option2Text.text = options[1];
            option3Text.text = "No Option";
        } else if(optionCount == 1)
        {
            option1Text.text = options[0];
            option2Text.text = "No Option";
            option3Text.text = "No Option";
        } else
        {
            noOptionText.SetActive(true);
        }
    }

    public void Option1Press()
    {
        string chosenText = option1Text.text;
        if (!chosenText.Contains("No Option"))
        {
            gallery.ForwardToJournal(chosenText);
            //print("UserChoice: " + chosenText);
            CleanupText();
        }
    }

    public void Option2Press()
    {
        string chosenText = option2Text.text;
        if (!chosenText.Contains("No Option"))
        {
            gallery.ForwardToJournal(chosenText);
            //print("UserChoice: " + chosenText);
            CleanupText();
        }
        
    }

    public void Option3Press()
    {
        string chosenText = option3Text.text;
        if (!chosenText.Contains("No Option"))
        {
            gallery.ForwardToJournal(chosenText);
            //print("UserChoice: " + chosenText);
            CleanupText();
            
        }
    }

    public void ExitButton()
    {
        CleanupText();
        gallery.FrameJournalSelectExit();
    }

    private void CleanupText()
    {
        noOptionText.SetActive(false);
        option1Text.text = "No Option";
        option2Text.text = "No Option";
        option3Text.text = "No Option";
    }
}
