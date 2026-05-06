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
    private bool isGround = true;
    private bool isInWater = false;  

    public Rigidbody rb;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        float currentSpeed = isInWater ? swimSpeed : moveSpeed;
        transform.Translate(moveX * currentSpeed * Time.deltaTime,
                            0,
                            moveY * currentSpeed * Time.deltaTime);

        if (isInWater)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                rb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpCount++;
                isGround = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WaterVolume>() != null)
        {
            isInWater = true;
            rb.linearDamping = waterDrag;           
            rb.useGravity = false;         
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
        isGround = true;
        jumpCount = 0;
    }
}