using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Generic symbol, handles animation switching and symbol IDs. 
// Extend this to implement animations and game-specific interactions.
public class GenericSymbol : MonoBehaviour
{
    // the symbol main object
    public GameObject symbolObject;

    // the quad for the texture
    public GameObject textureObject;

    public void HideFromView()
    {
        symbolObject.transform.position = new Vector3(0, 0, 999);
    }

    public void SetTextureID(int value)
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

    

    public virtual void StopAllAnims()
    {
        // stop win anim
        StopTextureAnim();
    }

    public void StopTextureAnim()
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

    public void StartTextureAnim()
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
