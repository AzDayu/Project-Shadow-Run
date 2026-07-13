using System.Collections;
using UnityEngine;

public class LobbyDoor : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("일정 시간 후 이동시킬 목적지(빈 오브젝트)")]
    [SerializeField] private Transform _targetDestination;

    [Tooltip("트리거 진입 후 대기해야 하는 시간 (초)")]
    [SerializeField] private float _waitTime = 3.0f;

    [Tooltip("트리거에 반응할 대상의 태그 (예: Player)")]
    [SerializeField] private string _targetTag = "Player";

    private Coroutine _teleportCoroutine;

    private void Awake()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_targetTag))
        {
            if (_teleportCoroutine != null)
            {
                StopCoroutine(_teleportCoroutine);
            }

            _teleportCoroutine = StartCoroutine(TeleportRoutine(other.gameObject));
            Debug.Log($"[{other.name}] 트리거 진입! {_waitTime}초 대기 타이머를 시작합니다.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_targetTag))
        {
            if (_teleportCoroutine != null)
            {
                StopCoroutine(_teleportCoroutine);
                _teleportCoroutine = null;
                Debug.Log($"[{other.name}] 범위를 이탈하여 타이머가 초기화되었습니다.");
            }
        }
    }

    private IEnumerator TeleportRoutine(GameObject targetObject)
    {
        yield return new WaitForSeconds(_waitTime);

        if (_targetDestination != null)
        {
            CharacterController controller = targetObject.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
                targetObject.transform.position = _targetDestination.position;
                controller.enabled = true;
            }
            else
            {
                targetObject.transform.position = _targetDestination.position;
            }

            Debug.Log($"[{targetObject.name}] 대기 완료! {_targetDestination.name} 위치로 순간이동했습니다.");
        }
        else
        {
            Debug.LogWarning("이동할 목적지(Target Destination)가 설정되지 않았습니다! 인스펙터를 확인해주세요.");
        }

        _teleportCoroutine = null;
    }
}
