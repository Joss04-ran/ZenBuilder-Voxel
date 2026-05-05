using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 firstPerson = new Vector3(0, 0.6f, 0);
    public Vector3 thirdPerson = new Vector3(0, 2f, -4f);
    
    private bool isFirstPerson = true;
    void Start()
    {
        transform.localPosition = firstPerson;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isFirstPerson = !isFirstPerson;
            if (isFirstPerson == true)
            {
                transform.localPosition = firstPerson;
            }
            else
            {
                transform.localPosition = thirdPerson;
            }
        }
    }
}
