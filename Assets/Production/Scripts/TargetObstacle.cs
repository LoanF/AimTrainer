using UnityEngine;

public class TargetObstacle : MonoBehaviour
{
    [HideInInspector] 
    public TargetManager manager;

    [ContextMenu("Force WasShot")]
    public void WasShot()
    {
        if (manager != null)
        {
            manager.RelocateTarget(this.transform.position);
            
            Destroy(gameObject);
            
            Debug.Log("Target Hit! Manager is spawning the next one.");
        }
        else 
        {
            Debug.LogWarning("This Target has no Manager assigned! Check your Spawner logic.");
        }
    }
}