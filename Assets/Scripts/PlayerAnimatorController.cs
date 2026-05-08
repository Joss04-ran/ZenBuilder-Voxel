using UnityEngine;
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private float walkThreshold = 0.1f;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogError("Animator doesn't found in " + gameObject.name);
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float speed = new Vector2(horizontal, vertical).magnitude;

        animator.SetFloat("Speed", speed);
    }
}