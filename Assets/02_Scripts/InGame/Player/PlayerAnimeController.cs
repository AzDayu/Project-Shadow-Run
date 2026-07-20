using UnityEngine;

public enum  WeaponType
{
    None,
    Rifle,
    Pistol
}

public class PlayerAnimeController : MonoBehaviour
{
    [SerializeField] private Animator _playerAnimator;

    public void SetRun(bool isRunning)
    {
        _playerAnimator.SetBool("isRun", isRunning);
    }

    public void SwapWeaponPosture()
    {
        WeaponType weaponType = InventoryManager.Instance.ReturnWeaponTypeFromQuickSlotID();

        switch(weaponType)
        {
            case WeaponType.Rifle:
                _playerAnimator.SetBool("isRifle", true);
                _playerAnimator.SetBool("isPistol", false);
                break;
            case WeaponType.Pistol:
                _playerAnimator.SetBool("isRifle", false);
                _playerAnimator.SetBool("isPistol", true);
                break;
            default:
                _playerAnimator.SetBool("isRifle", false);
                _playerAnimator.SetBool("isPistol", false);
                break;
        }
    }
}
