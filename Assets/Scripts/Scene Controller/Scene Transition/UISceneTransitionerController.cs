using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISceneTransitionerController : MonoBehaviour
{
    public static UISceneTransitionerController Instance { get; private set; }

    public Animator animatorFade;
    public Image solid;

    protected Animator selectedAnimator;

    private void Awake()
    {
        Instance = this;
        selectedAnimator = animatorFade;
    }

    public IEnumerator TransitionIn (float speed, Color color)
    {
        solid.color = color;
        selectedAnimator.SetFloat("Speed", speed);
        selectedAnimator.Play("In", 0, 0.0f);

        // Wait for the next frame to ensure the Animator has started playing the animation
        yield return null;
        // Wait while the Animator is playing the specified animation
        while (selectedAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;
    }

    public IEnumerator TransitionOut(float speed, Color color)
    {
        solid.color = color;
        selectedAnimator.SetFloat("Speed", speed);
        selectedAnimator.Play("Out", 0, 0.0f);

        // Wait for the next frame to ensure the Animator has started playing the animation
        yield return null;
        // Wait while the Animator is playing the specified animation
        while (selectedAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;
    }
}
