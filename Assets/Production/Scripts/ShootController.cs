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

    [Header("Aim Down Sights")]
    [SerializeField] private float aimSmoothing = 0.3f;

    private float lastShootTime;
    private bool wasPressedLastFrame;
    private AudioSource audioSource;
    private AudioClip shootClip;
    private Quaternion smoothedRotation;
    private bool isAiming;

    public FireMode CurrentFireMode => fireMode;
    public bool IsAiming => isAiming;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = true;
        shootClip = SoundGenerator.GenerateShootSound();
        smoothedRotation = transform.rotation;
    }

    private void Update()
    {
        isAiming = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);

        if (isAiming)
        {
            smoothedRotation = Quaternion.Slerp(smoothedRotation, shootOrigin.rotation, aimSmoothing);
            shootOrigin.rotation = smoothedRotation;
        }
        else
        {
            smoothedRotation = shootOrigin.rotation;
        }

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
            OVRInput.SetControllerVibration(0.15f, 0.3f, OVRInput.Controller.RTouch);
        }

        wasPressedLastFrame = isPressed;
    }

    public void ToggleFireMode()
    {
        fireMode = fireMode == FireMode.SemiAuto ? FireMode.Auto : FireMode.SemiAuto;
    }
}
