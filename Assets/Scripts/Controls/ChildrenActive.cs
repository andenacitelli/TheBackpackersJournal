using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildrenActive : MonoBehaviour
{
    public bool noChildrenActive { get; set; }
    private GameObject[] children;
    public void Start()
    {
        noChildrenActive = true;
        int childCount = gameObject.transform.childCount;
        children = new GameObject[childCount];
        for(int i = 0; i < childCount; i++)
        {
            children[i] = gameObject.transform.GetChild(i).gameObject;
        }
    }
    public string ActiveChildren()
    {
        //returns the name of the active child, otherwise empty string
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].activeInHierarchy)
            {
                noChildrenActive = false;
                return children[i].name;
            }
        }
        noChildrenActive = true;
        return "";
    }

    public GameObject GetChildWithName(string pass)
    {
        // Returns gameobject if name is found, NULL otherwise
        for(int i = 0; i < children.Length; i++)
        {
            if (children[i].name.Contains(pass))
            {
                return children[i];
            }
        }
        return null;
    }
}
