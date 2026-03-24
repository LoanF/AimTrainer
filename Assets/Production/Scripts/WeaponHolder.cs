using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShootController shootController;
    [SerializeField] private Transform leftHandTransform;
    [Tooltip("Tous les GameObjects visuels de la main droite à masquer quand une arme est équipée (ex : modèle manette + modèle main)")]
    [SerializeField] private GameObject[] controllerVisualRoots;

    [Header("Settings")]
    [SerializeField] private float pickupRadius      = 0.25f;
    [SerializeField] private float secondaryGripRadius = 0.15f;

    private WeaponPickup equippedWeapon;
    private Transform    gunSocket;
    private bool         isTwoHanded;
    private bool         isSecondaryGripped;

    private void Awake()
    {
        gunSocket = new GameObject("GunSocket").transform;
        gunSocket.SetParent(transform);
        gunSocket.localPosition = Vector3.zero;
        gunSocket.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        // Main droite — ramasser / poser
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
        {
            if (equippedWeapon == null) TryPickup();
            else                        Drop();
        }

        // Main gauche — saisir / relâcher le foregrip
        if (isTwoHanded && equippedWeapon != null
            && equippedWeapon.secondaryGripPoint != null
            && leftHandTransform != null)
        {
            bool wasGripped = isSecondaryGripped;

            if (!isSecondaryGripped
                && OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
            {
                float dist = Vector3.Distance(leftHandTransform.position,
                                              equippedWeapon.secondaryGripPoint.position);
                if (dist <= secondaryGripRadius)
                    isSecondaryGripped = true;
            }
            else if (isSecondaryGripped
                     && OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
            {
                isSecondaryGripped = false;
            }

            if (isSecondaryGripped != wasGripped)
                shootController.SetEquipped(isSecondaryGripped);
        }
    }

    private void LateUpdate()
    {
        if (!isSecondaryGripped || equippedWeapon == null || leftHandTransform == null)
            return;

        Vector3 dir = (leftHandTransform.position - gunSocket.position).normalized;
        if (dir == Vector3.zero) return;
        gunSocket.rotation = Quaternion.LookRotation(dir, transform.up);
    }

    private void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius,
                                                ~0, QueryTriggerInteraction.Collide);
        WeaponPickup best     = null;
        float        bestDist = pickupRadius;

        foreach (var hit in hits)
        {
            var w = hit.GetComponent<WeaponPickup>();
            if (w == null) continue;
            float d = Vector3.Distance(transform.position, w.transform.position);
            if (d < bestDist) { bestDist = d; best = w; }
        }

        if (best == null) return;

        Equip(best);
    }

    private void Drop()
    {
        if (equippedWeapon == null) return;

        isSecondaryGripped = false;
        isTwoHanded        = false;
        gunSocket.localRotation = Quaternion.identity;

        Vector3 dropPos = transform.position + transform.forward * 0.3f + Vector3.down * 0.2f;
        equippedWeapon.OnDrop(dropPos);
        equippedWeapon = null;

        shootController.SetEquipped(false);
        shootController.RestoreDefaultShootOrigin();
        shootController.SetAimTransform(null);
        SetControllerVisible(true);
    }

    /// <summary>Équipe directement une arme (utilisé par WeaponSelectionMenu).</summary>
    public void ForceEquip(WeaponPickup weapon)
    {
        if (weapon == null) return;
        if (equippedWeapon != null) Drop();
        Equip(weapon);
    }

    private void Equip(WeaponPickup weapon)
    {
        equippedWeapon = weapon;
        equippedWeapon.OnPickup(gunSocket); // parenté à gunSocket → suit la main

        if (shootController == null)
        {
            Debug.LogError("[WeaponHolder] shootController non assigné dans l'Inspector !");
            SetControllerVisible(false);
            return;
        }

        if (equippedWeapon.muzzlePoint != null)
            shootController.SetShootOrigin(equippedWeapon.muzzlePoint);

        shootController.SetAimTransform(gunSocket);

        if (equippedWeapon.weaponData != null)
        {
            shootController.SetWeaponData(equippedWeapon.weaponData);
            isTwoHanded = equippedWeapon.weaponData.isTwoHanded;
        }

        // Arme deux mains : le tir reste désactivé tant que la main gauche ne tient pas le foregrip
        shootController.SetEquipped(!isTwoHanded);
        SetControllerVisible(false);
    }

    private void SetControllerVisible(bool visible)
    {
        if (controllerVisualRoots == null) return;
        foreach (var go in controllerVisualRoots)
            if (go != null) go.SetActive(visible);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);

        if (isSecondaryGripped && equippedWeapon?.secondaryGripPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(equippedWeapon.secondaryGripPoint.position, secondaryGripRadius);
        }
    }
}
