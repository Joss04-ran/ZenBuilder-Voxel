using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour
{
    public GameObject blockPrefab; 
    public int poolSize = 100;     
    private List<GameObject> pool = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(blockPrefab);
            obj.SetActive(false); 
            pool.Add(obj);        
        }
    }

    public GameObject GetBlock(Vector3 position)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.position = position; 
                obj.SetActive(true);               
                return obj;
            }
        }
        Debug.LogWarning("The Block Storage is Empty!");
        return null;
    }
}