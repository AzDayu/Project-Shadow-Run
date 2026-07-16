using UnityEngine;

public class Dev_CheatManager : MonoBehaviour
{
    void Update()
    {
        // 'L' 키를 누르면 로비로 이동
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("치트: 로비로 순간이동합니다.");

            // 씬 전환 대신 GameManager의 메서드 호출
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToOutGame();
            }
            else
            {
                Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다.");
            }
        }
    }
}
