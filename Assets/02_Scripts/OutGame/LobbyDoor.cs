using UnityEngine;

public class LobbyDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //정해진 시간동안 범위를 벗어나지 않거나 상호작용 버튼을 누르면 인게임 맵으로 이동한다는 안내 메시지 출력하기.
        //조건 만족시 인게임 맵으로 이동.
    }

    private void OnTriggerExit(Collider other)
    {
        //타이머 초기화. 안내창 제거.
    }
}
