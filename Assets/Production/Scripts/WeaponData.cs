using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "AimTrainer/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "Weapon";
    public float shootCooldown = 0.2f;
    public ShootController.FireMode fireMode = ShootController.FireMode.SemiAuto;
    public bool isTwoHanded = false;
}
