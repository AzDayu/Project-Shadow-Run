using UnityEngine;

public class PlayerShootTest : MonoBehaviour
{
    [Header("테스트 세팅")]
    [Tooltip("크로스헤어(화면 중앙) 기준이 될 메인 카메라")]
    public Camera mainCamera;

    [Tooltip("팀 프로젝트의 중앙 입력 핸들러")]
    public PlayerInputHandler inputHandler;

    [Tooltip("사거리 (테스트용)")]
    public float range = 100f;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("<color=red>[에러]</color> 메인 카메라를 찾을 수 없습니다! 씬의 카메라에 'MainCamera' 태그가 있는지 확인해주세요.");
            }
        }

        if (inputHandler == null)
        {
            inputHandler = GetComponentInParent<PlayerInputHandler>();
        }

        if (inputHandler != null)
        {
            inputHandler.FirePerformed += TestShoot;
        }
        else
        {
            Debug.LogError("<color=red>[에러]</color> PlayerInputHandler를 찾을 수 없습니다!");
        }
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.FirePerformed -= TestShoot;
        }
    }

    private void TestShoot()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log($"<color=green>[크로스헤어 적중]</color> 맞은 오브젝트: {hit.transform.name}");
            Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * range, Color.blue, 2f);
        }
    }
}