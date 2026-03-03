using UnityEngine;

public class ShootController : MonoBehaviour
{
    public enum FireMode { SemiAuto, Auto }

    [Header("References")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Settings")]
    [SerializeField] private float shootCooldown = 0.2f;
    [SerializeField] private FireMode fireMode = FireMode.SemiAuto;

    private float lastShootTime;
    private bool wasPressedLastFrame;
    private AudioSource audioSource;
    private AudioClip shootClip;

    public FireMode CurrentFireMode => fireMode;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = true;
        shootClip = SoundGenerator.GenerateShootSound();
    }

    private void Update()
    {
        bool isPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            ToggleFireMode();

        bool canShoot = Time.time >= lastShootTime + shootCooldown;

        bool shouldShoot = fireMode == FireMode.Auto
            ? isPressed && canShoot
            : isPressed && !wasPressedLastFrame && canShoot;

        if (shouldShoot)
        {
            lastShootTime = Time.time;
            Instantiate(bulletPrefab, shootOrigin.position, shootOrigin.rotation);
            audioSource.PlayOneShot(shootClip);
            OVRInput.SetControllerVibration(0.5f, 0.7f, OVRInput.Controller.RTouch);
        }

        wasPressedLastFrame = isPressed;
    }

    public void ToggleFireMode()
    {
        fireMode = fireMode == FireMode.SemiAuto ? FireMode.Auto : FireMode.SemiAuto;
    }
}
