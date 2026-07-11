using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInputHandler InputHandler;

    [Header("Weapon")]
    [SerializeField] private Transform WeaponSocket;
    [SerializeField] private Camera PlayerCamera;

    private WeaponBase _equippedWeapon;
    private WeaponData _equippedWeaponData;

    public WeaponBase EquippedWeapon => _equippedWeapon;

    private void OnEnable()
    {
        if (InputHandler != null)
            InputHandler.FirePerformed += Fire;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance
                .OnSelectedQuickSlotChanged +=
                EquipSelectedQuickSlot;
        }
    }

    private void Start()
    {
        EquipSelectedQuickSlot();
    }

    private void OnDisable()
    {
        if (InputHandler != null)
            InputHandler.FirePerformed -= Fire;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance
                .OnSelectedQuickSlotChanged -=
                EquipSelectedQuickSlot;
        }
    }

    private void EquipSelectedQuickSlot()
    {
        if (InventoryManager.Instance == null)
            return;

        ItemStack stack =
            InventoryManager.Instance
                .GetSelectedQuickSlotStack();

        if (stack?.Item is not WeaponData weaponData)
        {
            UnequipWeapon();
            return;
        }

        EquipWeapon(weaponData);
    }

    private void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
            return;

        if (_equippedWeaponData == weaponData &&
            _equippedWeapon != null)
        {
            return;
        }

        UnequipWeapon();

        if (string.IsNullOrEmpty(weaponData.PrefabPath))
        {
            Debug.LogWarning(
                $"무기 PrefabPath가 비어 있습니다. " +
                $"Item: {weaponData.ItemName}"
            );

            return;
        }

        GameObject weaponPrefab =
            Resources.Load<GameObject>(
                weaponData.PrefabPath
            );

        if (weaponPrefab == null)
        {
            Debug.LogWarning(
                $"무기 프리팹 로드 실패: " +
                $"{weaponData.PrefabPath}"
            );

            return;
        }

        GameObject weaponObject =
            Instantiate(
                weaponPrefab,
                WeaponSocket
            );

        weaponObject.transform
            .SetLocalPositionAndRotation(
                Vector3.zero,
                Quaternion.identity
            );

        if (!weaponObject.TryGetComponent(
            out WeaponBase weapon))
        {
            Debug.LogWarning(
                $"무기 프리팹에 " +
                $"{nameof(TestWeaponBase)}가 없습니다."
            );

            Destroy(weaponObject);
            return;
        }

        weapon.Initialize(weaponData);

        _equippedWeapon = weapon;
        _equippedWeaponData = weaponData;

        Debug.Log(
            $"무기 장착: {weaponData.ItemName}"
        );
    }

    private void UnequipWeapon()
    {
        if (_equippedWeapon != null)
            Destroy(_equippedWeapon.gameObject);

        _equippedWeapon = null;
        _equippedWeaponData = null;
    }

    private void Fire()
    {
        if (_equippedWeapon == null)
            return;

        if (PlayerCamera == null)
            return;

        Transform firePoint = _equippedWeapon.FirePoint;

        if (firePoint == null)
        {
            Debug.LogWarning(
                "무기에 FirePoint가 할당되지 않았습니다."
            );

            return;
        }

        _equippedWeapon.Fire(
            firePoint.position,
            PlayerCamera.transform.forward
        );
    }
}
