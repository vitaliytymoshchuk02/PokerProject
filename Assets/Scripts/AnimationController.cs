using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"Animator component not found on {gameObject.name}.");
        }
    }
    public void PlayAnimation(string name)
    {
        if (animator != null)
        {
            animator.Play("");
        }
        else Debug.Log($"Animator component not found on {gameObject.name}.");
    }
    public void SetTrigger(string trigger)
    {
        if (animator == null) { Debug.Log("Animator is null"); }
        animator.SetTrigger(trigger);
    }
    public bool IsAnimationFinished(string animationName)
    {
        var animState = animator.GetCurrentAnimatorStateInfo(0);
        return animState.IsName(animationName) && animState.normalizedTime >= 1.0f;
    }
}
