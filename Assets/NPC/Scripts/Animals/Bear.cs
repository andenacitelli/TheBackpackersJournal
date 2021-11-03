using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bear : PredatorController
{
    protected override void Initialize()
    {
        base.Initialize();
        audioManager.Assign3DSource(audioSource, attackSoundName);
    }

    protected override void PlayAttackSound() 
    {
        audioManager.Play(attackSoundName);
    }
}
