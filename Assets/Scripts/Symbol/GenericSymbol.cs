using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Generic symbol, handles animation switching and symbol IDs. 
// Extend this to implement animations and game-specific interactions.
public class GenericSymbol : MonoBehaviour
{
    // the symbol ID e.g. A, K, Q, J
    string currentID = "IDK";

    // the symbol main object
    public GameObject symbolObject;

    // the quad for the texture
    public GameObject textureObject;

    // current set of sprite animations
    public Sprite[] currentTextures;

    // TODO: move this to subclass
    public ParticleSystem explosionObject;

    public void Start()
    {   
        // random default symbol
        ChangeToID(0);
    }

    public string GetCurrentID()
    {
        return currentID;
    }

    public void SetCurrentID(string newID)
    {
        currentID = newID;
    }

    public void moveAroundDeleteMe()
    {
        symbolObject.transform.position = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 10);
    }

    public void ChangeToID(int value)
    {
        Animator animator = textureObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("symbolTextureID", value);
        }
        else
        {
            throw new System.Exception("No animator found on textureObject");
        }
    }

    public void PlayExplosion()
    {
        explosionObject.Play();
    }

    public void StopAllAnims()
    {
        // clear explosion
        explosionObject.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // reset Z-pos

        // stop win anim
        stopTextureAnim();
    }

    public void stopTextureAnim()
    {
        Animator animator = textureObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0);
            animator.speed = 0;
        }
        else
        {
            throw new System.Exception("No animator found on textureObject");
        }
    }

    public void startTextureAnim()
    {
        Animator animator = textureObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 1;
            // animator.SetFloat("PlaybackTime", 0);
        }
        else
        {
            throw new System.Exception("No animator found on textureObject");
        }

    }
}
