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

    [Header("Water Detection")]
    [Tooltip("Y offset from transform.position to check for water (chest height)")]
    public float waterCheckOffset = 0.8f;

    private int jumpCount = 0;
    private bool isGround = false;
    private bool isInWater = false;

    public Rigidbody rb;

    private PlayerAudioController _audio;
    private float _waterCheckTimer;

    void Start()
    {
        _audio = GetComponent<PlayerAudioController>();
    }

    void Update()
    {
        _waterCheckTimer += Time.deltaTime;
        if (_waterCheckTimer >= 0.1f)
        {
            _waterCheckTimer = 0f;
            UpdateWaterState();
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (isInWater)
        {
            Vector3 swimDir = (transform.right * moveX + transform.forward * moveY).normalized;
            rb.linearVelocity = new Vector3(
                swimDir.x * swimSpeed,
                rb.linearVelocity.y,
                swimDir.z * swimSpeed);

            if (Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    Mathf.Lerp(rb.linearVelocity.y, swimSpeed, Time.deltaTime * 8f),
                    rb.linearVelocity.z);
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift))
            {
                // Sink downward actively.
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    Mathf.Lerp(rb.linearVelocity.y, -swimSpeed, Time.deltaTime * 8f),
                    rb.linearVelocity.z);
            }
            else
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    Mathf.Lerp(rb.linearVelocity.y, 1f, Time.deltaTime * 2f),
                    rb.linearVelocity.z);
            }

            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, swimSpeed * 1.5f);
        }
        else
        {
            Vector3 dir = (transform.right * moveX + transform.forward * moveY).normalized;
            rb.linearVelocity = new Vector3(
                dir.x * moveSpeed,
                rb.linearVelocity.y,
                dir.z * moveSpeed);

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
    private void UpdateWaterState()
    {
        bool inWaterNow = false;

        if (WorldManager.Instance != null)
        {
            float[] offsets = { 0.12f, 0.5f, 1.12f, 1,5f };
            foreach (float offset in offsets)
            {
                Vector3 pos = transform.position + Vector3.up * offset;
                if (WorldManager.Instance.GetWaterLevelAt(pos) > 0)
                {
                    inWaterNow = true;
                    break;
                }
            }
        }

        if (inWaterNow == isInWater) return;

        isInWater = inWaterNow;

        if (isInWater)
        {
            rb.linearDamping = waterDrag;
            rb.useGravity = false;
            _audio?.OnEnterWater();
            Debug.Log("[Movement] Entered water");
        }
        else
        {
            rb.linearDamping = normalDrag;
            rb.useGravity = true;
            Debug.Log("[Movement] Left water");
        }
    }

    private void OnCollisionEnter(Collision col) => CheckGround(col);
    private void OnCollisionStay(Collision col) => CheckGround(col);
    private void OnCollisionExit(Collision col) { isGround = false; }

    private void CheckGround(Collision col)
    {
        foreach (ContactPoint c in col.contacts)
        {
            if (c.normal.y > 0.5f && rb.linearVelocity.y <= 0.5f)
            {
                isGround = true;
                jumpCount = 0;
                return;
            }
        }
    }
}