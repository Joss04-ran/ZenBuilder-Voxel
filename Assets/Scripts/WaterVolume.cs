using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterVolume : MonoBehaviour
{
    private BoxCollider waterCollider;

    void Start()
    {
        waterCollider = GetComponent<BoxCollider>();
        waterCollider.isTrigger = true; 
    }
}