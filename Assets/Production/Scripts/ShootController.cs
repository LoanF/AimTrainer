using UnityEngine;

public class ShootController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Settings")]
    [SerializeField] private float shootCooldown = 0.2f;

    private float lastShootTime;
    private bool wasPressedLastFrame;

    private void Update()
    {
        bool isPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (isPressed && !wasPressedLastFrame && Time.time >= lastShootTime + shootCooldown)
        {
            lastShootTime = Time.time;
            Instantiate(bulletPrefab, shootOrigin.position, shootOrigin.rotation);
        }

        wasPressedLastFrame = isPressed;
    }
}
