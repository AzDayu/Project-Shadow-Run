using UnityEngine;

public class PlayerAnimeController : MonoBehaviour
{
    [SerializeField] private Animator _playerAnimator;

    public void SetRun(bool isRunning)
    {
        _playerAnimator.SetBool("isRun", isRunning);
    }


}
