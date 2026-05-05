using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float jumpForce = 5.0f;
    public int maxJumps = 2;
    private int jumpCount = 0;
    public Rigidbody rb;
    private bool isGround = true;
    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        transform.Translate(moveX * moveSpeed * Time.deltaTime, 
            0, moveY * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps )
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
            isGround = false;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        isGround = true;
        jumpCount = 0;
    }
}
