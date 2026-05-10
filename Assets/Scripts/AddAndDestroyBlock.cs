using UnityEngine;

public class AddAndDestroyBlock : MonoBehaviour
{
    public float buildDistance = 5f;
    public Transform cameraTransform;
    private int currentBlockID = 1;
    public GhostBlockController ghostBlock;

    void Update()
    {
        // Choose A Block
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentBlockID = 1; 
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentBlockID = 6; 
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentBlockID = 16; 

        if (Input.GetMouseButtonDown(0))
        {
            // Block placement only proceeds when ghost confirms valid position.
            if (ghostBlock != null && !ghostBlock.CanPlace()) return;
            ModifyChunk(currentBlockID);
            AudioManager.Instance?.PlaySFX("Placeblock", 0.7f);
        }

        if (Input.GetMouseButtonDown(1))
        {
            ModifyChunk(0);
        }
    }

    void ModifyChunk(int blockID)
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, buildDistance))
        {
            Chunk chunk = hit.collider.GetComponent<Chunk>();

            if (chunk != null)
            {
                Vector3 targetBlock;

                if (blockID == 0)
                    targetBlock = hit.point - (hit.normal * 0.1f);
                else
                    targetBlock = hit.point + (hit.normal * 0.1f);

                Vector3 localPos = chunk.transform.InverseTransformPoint(targetBlock);
                chunk.EditVoxel(localPos, blockID);
            }
        }
    }
}