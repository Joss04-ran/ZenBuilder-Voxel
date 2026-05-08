using UnityEngine;

public class MenuCameraRotate : MonoBehaviour
{
    public float rotationSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
