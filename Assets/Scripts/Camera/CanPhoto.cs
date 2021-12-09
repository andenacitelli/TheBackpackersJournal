using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICanPhoto{
    GameObject ReportIsVisible();

}
public class CanPhoto : MonoBehaviour, ICanPhoto
{
    // This class can hold any attribute values we want
    // to attach to the attributes

    private Renderer rend;
    private Animator anim;
    private int hashIdle;
    private int hashWalk;
    private int hashRun;
    private int hashDeath;
    private int hashAttackBite;
    private int hashAttackBiteL;
    private int hashAttackBiteR;
    private int hashAttackPaw;
    private int hashAttackPaws;
    private int hashAttackPawL;
    private int hashAttackPawR;
    //private int hashCrawling;
    //private int hashDigging;
    private int hashDrinking;
    private int hashEating;
    //private int hashFlexing;
    //private int hashFlexingScratch;
    private int hashHowling;
    private int hashLaying;
    private int hashRoaring;
    //private int hashScratching;
    private int hashSitting;
    private int hashSleeping;
    private int hashSleepLay;
    //private int hashSmelling;
    //private int hashSneaking;
    //private int hashStanding;
    private int hashStandRoar;
    //private int hashYawning;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        anim = GetComponent<Animator>();

        // unused behaviors included but commented out, in case these are expanded on
        #region(Hashes)
        hashIdle = Animator.StringToHash("Idle");
        hashWalk = Animator.StringToHash("Walk");
        hashRun = Animator.StringToHash("Run");
        hashDeath = Animator.StringToHash("Death");
        hashAttackBite = Animator.StringToHash("Attack Bite");
        hashAttackBiteL = Animator.StringToHash("Attack Bite Left");
        hashAttackBiteR = Animator.StringToHash("Attack Bite Right");
        hashAttackPaw = Animator.StringToHash("Attack Paw");
        hashAttackPaws = Animator.StringToHash("Attack Paws");
        hashAttackPawL = Animator.StringToHash("Attack Paw Left");
        hashAttackPawR = Animator.StringToHash("Attack Paw Right");
        //hashCrawling = Animator.StringToHash("Crawling");
        //hashDigging = Animator.StringToHash("Digging");
        hashDrinking = Animator.StringToHash("Drinking");
        hashEating = Animator.StringToHash("Eating");
        //hashFlexing = Animator.StringToHash("Flexing");
        //hashFlexingScratch = Animator.StringToHash("Flexing Scratch");
        hashHowling = Animator.StringToHash("Howling");
        hashLaying = Animator.StringToHash("Laying");
        hashRoaring = Animator.StringToHash("Roaring");
        //hashScratching = Animator.StringToHash("Scratching");
        hashSitting = Animator.StringToHash("Sitting");
        hashSleeping = Animator.StringToHash("Sleeping");
        hashSleepLay = Animator.StringToHash("Sleep Lay");
        //hashSmelling = Animator.StringToHash("Smelling");
        //hashSneaking = Animator.StringToHash("Sneaking");
        //hashStanding = Animator.StringToHash("Standing");
        hashStandRoar = Animator.StringToHash("Stand Roar");
        //hashYawning = Animator.StringToHash("Yawning");
        #endregion


    }

    public GameObject ReportIsVisible()
    {
        GameObject hold = new GameObject();
        if (rend.isVisible)
        {
            hold = gameObject;
        }
        
        return hold;
    }

    public string ReportObjectName()
    {
        return gameObject.name;
    }

    public string ReportAction()
    {
        // Get animator or whatever here. will ret a string
        int currHash = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
        string actionName = "Walk";
        if(currHash != hashWalk || currHash != hashIdle)
        {
            if (currHash == hashRun)
            {
                actionName = "Run";
            }
            else if (currHash == hashDeath)
            {
                actionName = "Death";
            }
            else if (currHash == hashDrinking || currHash == hashEating)
            {
                actionName = "Eating";
            }
            else if (currHash == hashHowling)
            {
                actionName = "Howl";
            }
            else if (currHash == hashRoaring || currHash == hashStandRoar)
            {
                actionName = "Roar";
            }
            else if (currHash == hashSitting)
            {
                actionName = "Sit";
            }
            else if (currHash == hashLaying || currHash == hashSleeping || currHash == hashSleepLay)
            {
                actionName = "LayingDown";
            }
            else if (currHash == hashAttackBite || currHash == hashAttackBiteL || currHash == hashAttackBiteR || currHash == hashAttackPaw || currHash == hashAttackPaws || currHash == hashAttackPawL || currHash == hashAttackPawR)
            {
                actionName = "Attack";
            }
        }
        

        return actionName;
    }

    public string ReportToken()
    {
        string token = ReportObjectName() + "_" + ReportAction();
        Debug.Log("Created new token: " + token);
        return token;
    }

}
