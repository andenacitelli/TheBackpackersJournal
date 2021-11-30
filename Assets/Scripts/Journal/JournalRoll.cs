using BookCurlPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalRoll : MonoBehaviour
{
    private BookPro book;
    List<photo> photos;
    List<PageController> pages;

    private void Awake()
    {
        pages = new List<PageController>();
        book = GetComponent<BookPro>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
        foreach(Paper page in book.papers)
        {
            if(page.Back.TryGetComponent(out PageController pageCont))
            {
                pages.Add(pageCont);
            }

            if (page.Front.TryGetComponent(out PageController pageCont2))
            {
                pages.Add(pageCont2);
            }
        }

        /* Debug print for pages
        foreach(PageController pCont in pages)
        {
            print(pCont.pageTitle + " page successfully retrieved");
        }
        -- The pages list is unordered, but can be queried using the pageTitle
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
