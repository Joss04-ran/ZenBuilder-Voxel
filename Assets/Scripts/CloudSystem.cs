using System.Collections.Generic;
using UnityEngine;

public class CloudSystemOpenWorld : MonoBehaviour
{
    public Transform player;

    public int cloudCount = 40; 
    public float cloudHeight = 40f;
    public Vector2 areaSize = new Vector2(150f, 150f);

    public float cloudSpeed = 2f;
    public Vector2 widthRange = new Vector2(8f, 20f);
    public Vector2 lengthRange = new Vector2(8f, 20f);
    public float thickness = 1.5f;

    public Material cloudMaterial;

    private List<GameObject> clouds = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player Is Not Registered In Cloud System");
            return;
        }

        for (int i = 0; i < cloudCount; i++)
        {
            SpawnCloud();
        }
    }

    void Update()
    {
        foreach (GameObject cloud in clouds)
        {
            cloud.transform.Translate(Vector3.right * cloudSpeed * Time.deltaTime);

            Vector3 pos = cloud.transform.position;

            if (pos.x > player.position.x + areaSize.x / 2f)
                pos.x -= areaSize.x;
            else if (pos.x < player.position.x - areaSize.x / 2f)
                pos.x += areaSize.x;

            if (pos.z > player.position.z + areaSize.y / 2f)
                pos.z -= areaSize.y;
            else if (pos.z < player.position.z - areaSize.y / 2f)
                pos.z += areaSize.y;

            cloud.transform.position = pos;
        }
    }

    void SpawnCloud()
    {
        GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cloud.name = "Cloud";
        cloud.transform.parent = transform;

        float w = Random.Range(widthRange.x, widthRange.y);
        float l = Random.Range(lengthRange.x, lengthRange.y);
        cloud.transform.localScale = new Vector3(w, thickness, l);

        if (cloudMaterial != null)
            cloud.GetComponent<Renderer>().material = cloudMaterial;

        Destroy(cloud.GetComponent<BoxCollider>());

        float x = Random.Range(player.position.x - areaSize.x / 2f, player.position.x + areaSize.x / 2f);
        float z = Random.Range(player.position.z - areaSize.y / 2f, player.position.z + areaSize.y / 2f);

        cloud.transform.position = new Vector3(x, cloudHeight, z);
        clouds.Add(cloud);
    }
}