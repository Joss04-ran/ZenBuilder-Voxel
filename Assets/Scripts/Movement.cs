using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float jumpForce = 5.0f;
    public int maxJumps = 2;

    public float swimSpeed = 5f;
    public float floatForce = 6f;
    public float waterDrag = 3f;
    public float normalDrag = 0f;

    private int jumpCount = 0;
    private bool isGround = false;
    private bool isInWater = false;

    public Rigidbody rb;
    private PlayerAudioController _audio;

    void Start()
    {
        _audio = GetComponent<PlayerAudioController>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        float currentSpeed = isInWater ? swimSpeed : moveSpeed;
        Vector3 moveDirection = (transform.right * moveX + transform.forward * moveY).normalized;

        rb.linearVelocity = new Vector3(
            moveDirection.x * currentSpeed,
            rb.linearVelocity.y,
            moveDirection.z * currentSpeed);

        if (isInWater)
        {
            if (Input.GetKey(KeyCode.Space))
                rb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpCount++;
                isGround = false;
                _audio?.OnJump();
            }
        }

        if (transform.position.y < -25f)
        {
            transform.position = new Vector3(transform.position.x, 60f, transform.position.z);
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WaterVolume>() != null)
        {
            isInWater = true;
            rb.linearDamping = waterDrag;
            rb.useGravity = false;
            _audio?.OnEnterWater();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<WaterVolume>() != null)
        {
            isInWater = false;
            rb.linearDamping = normalDrag;
            rb.useGravity = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckGround(collision);
    }
    private void OnCollisionStay(Collision collision)
    {
        CheckGround(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        isGround = false;
    }
    private void CheckGround(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            bool isFlatSurface = contact.normal.y > 0.5f;
            bool isMovingDownward = rb.linearVelocity.y <= 0.5f;

            if (isFlatSurface && isMovingDownward)
            {
                isGround = true;
                jumpCount = 0;
                return;
            }
        }
    }
}