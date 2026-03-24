using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "AimTrainer/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "Weapon";
    public float shootCooldown = 0.2f;
    public ShootController.FireMode fireMode = ShootController.FireMode.SemiAuto;
    public bool isTwoHanded = false;

    [Header("Sound")]
    public WeaponSoundProfile soundProfile;

    [Header("Bullet Spread")]
    [Tooltip("Angle de dispersion en degrés (0 = précis)")]
    public float spreadAngle = 0f;
    [Tooltip("Nombre de balles par tir (>1 pour shotgun)")]
    public int bulletsPerShot = 1;
}
