using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CascadeSymbol : GenericSymbol
{

    public ParticleSystem explosionObject;

    public void PlayExplosion()
    {
        explosionObject.Play();
    }

    public void StopExplosion()
    {
        explosionObject.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public override void StopAllAnims()
    {
        // clear explosion
        StopExplosion();
        // stop win anim
        StopTextureAnim();
    }
}
