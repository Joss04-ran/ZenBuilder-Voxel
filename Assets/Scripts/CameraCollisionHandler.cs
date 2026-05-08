using UnityEngine;
public class CameraCollisionHandler : MonoBehaviour
{
    [Header("References")]
    public Transform playerPivot; 

    [Header("Collision Settings")]
    public float collisionRadius = 0.2f;
    public float minDistance = 0.3f;
    public LayerMask collisionMask = ~0;

    private Vector3 _desiredLocalPos;
    void Start()
    {
        _desiredLocalPos = transform.localPosition;
    }
    void LateUpdate()
    {
        _desiredLocalPos = transform.localPosition;
        CheckCollision();
    }

    private void CheckCollision()
    {
        if (playerPivot == null) return;

        Vector3 desiredWorldPos = playerPivot.TransformPoint(_desiredLocalPos);
        Vector3 direction = desiredWorldPos - playerPivot.position;
        float distance = direction.magnitude;
        if (Physics.SphereCast(playerPivot.position, collisionRadius,
            direction.normalized, out RaycastHit hit, distance, collisionMask))
        {
            float safeDistance = Mathf.Max(hit.distance - collisionRadius, minDistance);
            Vector3 safeWorldPos = playerPivot.position + direction.normalized * safeDistance;
            transform.position = safeWorldPos;
        }
        else
        {
            transform.position = desiredWorldPos;
        }
    }
}