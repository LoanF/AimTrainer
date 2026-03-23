using UnityEngine;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    public List<GameObject> targetPrefabs = new List<GameObject>(); 
    public float spawnDistance = 3.0f;
    private GameObject currentTarget;

    void Start()
    {
        RelocateTarget(transform.position);
    }

    public void RelocateTarget(Vector3 oldPosition)
    {
        if (currentTarget != null) 
        {
            Destroy(currentTarget); 
        }

        Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(1f, 2f), spawnDistance);
        Vector3 newPosition = transform.position + offset;

        List<GameObject> validPrefabs = new List<GameObject>();
        if (targetPrefabs != null)
        {
            foreach (var p in targetPrefabs)
            {
                if (p != null) validPrefabs.Add(p);
            }
        }

        if (validPrefabs.Count > 0)
        {
            GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];
            currentTarget = Instantiate(prefabToSpawn, newPosition, Quaternion.identity);
        
            currentTarget.GetComponent<TargetObstacle>().manager = this;
        }
        else
        {
            Debug.LogWarning("No target prefabs assigned to TargetManager!");
        }
    }
}