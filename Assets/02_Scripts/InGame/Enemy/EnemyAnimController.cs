using UnityEngine;

public class EnemyAnimController : MonoBehaviour
{
    public Animator _animator;
    private void Awake()
    {
        _animator= GetComponent<Animator>();
    }
    public void ChangeAnimState() { }
}
