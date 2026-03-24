using System.Collections;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    public enum FireMode { SemiAuto, Auto }

    [Header("References")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private AudioClip shootSoundOverride;

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
    private bool isEquipped;
    private Transform defaultShootOrigin;
    private Transform aimTransform;

    public FireMode CurrentFireMode => fireMode;
    public bool IsAiming => isAiming;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = true;
        shootClip = shootSoundOverride != null ? shootSoundOverride : SoundGenerator.GenerateShootSound();
        smoothedRotation = transform.rotation;
        defaultShootOrigin = shootOrigin;
    }

    public void SetEquipped(bool equipped) => isEquipped = equipped;

    public void SetShootOrigin(Transform origin) => shootOrigin = origin;

    public void SetAimTransform(Transform aim) => aimTransform = aim;

    public void RestoreDefaultShootOrigin() => shootOrigin = defaultShootOrigin;

    public void SetWeaponData(WeaponData data)
    {
        shootCooldown = data.shootCooldown;
        fireMode = data.fireMode;
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

        bool canShoot = isEquipped && Time.time >= lastShootTime + shootCooldown;

        bool shouldShoot = fireMode == FireMode.Auto
            ? isPressed && canShoot
            : isPressed && !wasPressedLastFrame && canShoot;

        if (shouldShoot)
        {
            lastShootTime = Time.time;
            Quaternion bulletRot = aimTransform != null
                ? Quaternion.LookRotation(aimTransform.forward, aimTransform.up)
                : shootOrigin.rotation;
            Instantiate(bulletPrefab, shootOrigin.position, bulletRot);
            audioSource.PlayOneShot(shootClip);
            StartCoroutine(HapticPulse());
            StartCoroutine(MuzzleFlash());
        }

        wasPressedLastFrame = isPressed;
    }

    private IEnumerator HapticPulse()
    {
        OVRInput.SetControllerVibration(0.15f, 0.3f, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(0.05f);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
    }

    private IEnumerator MuzzleFlash()
    {
        // Crée une lumière ponctuelle orange/blanche au niveau de la bouche
        var flashGo = new GameObject("MuzzleFlash");
        flashGo.transform.position = shootOrigin.position;

        var light = flashGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.75f, 0.3f);
        light.intensity = 8f;
        light.range = 1.5f;

        yield return new WaitForSeconds(0.04f);
        Destroy(flashGo);
    }

    public void ToggleFireMode()
    {
        fireMode = fireMode == FireMode.SemiAuto ? FireMode.Auto : FireMode.SemiAuto;
    }
}
