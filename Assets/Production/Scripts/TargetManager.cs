using UnityEngine;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    public GameObject targetPrefab; 
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

        currentTarget = Instantiate(targetPrefab, newPosition, Quaternion.identity);
    
        currentTarget.GetComponent<TargetObstacle>().manager = this;
    }
}