using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static Vector3 objectPoolPosition = new Vector3(0, 50, -20);

    public static void DestroyChildElements(GameObject parentObject)
    {
        foreach (Transform child in parentObject.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public static float GetAnimationClipDurationByName(Animator animator, string clipName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == clipName) return clip.length;
        }
        return 0f;
    }
    public static float GetAnimationClipDurationByAction(Animator animator, string actionName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains(actionName)) return clip.length;
        }
        return 0f;
    }
    public static void RepositionToObjectPool(GameObject repositionObject)
    {
        repositionObject.transform.position = objectPoolPosition;
    }

}
