////using UnityEngine;

////public static class TilePhysics
////{
////    public static void AlignRoom(TileController room, TileSocket socketA, TileSocket socketB)
////    {
////        if (room == null || socketA == null || socketB == null)
////        {
////            Debug.LogError("[위험] AlignRoom 연산 도중 필수 참조(Room/Socket)가 null로 전달되었습니다.");
////            return;
////        }

////        // 방향(회전) 맞추기 계산
////        Quaternion targetRotation = Quaternion.LookRotation(-socketA.transform.forward, socketA.transform.up);
////        Quaternion differenceRotation = targetRotation * Quaternion.Inverse(socketB.transform.rotation);
////        room.transform.rotation = differenceRotation * room.transform.rotation;

////        // 위치 맞추기 계산
////        Vector3 offset = socketA.transform.position - socketB.transform.position;
////        room.transform.position += offset;
////    }

////    public static bool IsRoomOverlapping(TileController room, LayerMask tileMask)
////    {
////        if (room == null || room.TileCollider == null)
////        {
////            Debug.LogError("[위험] IsRoomOverlapping 검사 대상 방 혹은 콜리더가 존재하지 않습니다.");
////            return false;
////        }

////        // BoxCollider 정보 기반의 CheckBox 연산 수행
////        return Physics.CheckBox(
////            room.transform.position + room.TileCollider.center,
////            room.TileCollider.size / 2f,
////            room.transform.rotation,
////            tileMask
////        );
////    }
////}


//using UnityEngine;

//public static class TilePhysics
//{
//    public static void AlignRoom(TileController room, TileSocket socketA, TileSocket socketB)
//    {
//        // [위험도: 상] 필수 매개변수 Null 체크 방어 코드
//        if (room == null || socketA == null || socketB == null)
//        {
//            Debug.LogError("[ERROR] AlignRoom: 연산 도중 필수 참조(Room/Socket)가 null로 전달되었습니다. 정렬을 중단합니다.");
//            return;
//        }

//        // 방향(회전) 맞추기 계산
//        Quaternion targetRotation = Quaternion.LookRotation(-socketA.transform.forward, socketA.transform.up);
//        Quaternion differenceRotation = targetRotation * Quaternion.Inverse(socketB.transform.rotation);
//        room.transform.rotation = differenceRotation * room.transform.rotation;

//        // 위치 맞추기 계산
//        Vector3 offset = socketA.transform.position - socketB.transform.position;
//        room.transform.position += offset;
//    }

//    public static bool IsRoomOverlapping(TileController room, LayerMask tileMask)
//    {
//        // [위험도: 상] 검사 대상 컴포넌트 유효성 검증 방어 코드
//        if (room == null)
//        {
//            Debug.LogError("[ERROR] IsRoomOverlapping: 검증할 room(TileController) 대상이 null입니다.");
//            return false;
//        }

//        if (room.TileCollider == null)
//        {
//            Debug.LogError($"[ERROR] IsRoomOverlapping: '{room.gameObject.name}' 오브젝트 내부에 TileCollider(BoxCollider) 정보가 존재하지 않아 충돌 연산을 수행할 수 없습니다.");
//            return false;
//        }

//        // BoxCollider 정보 기반의 CheckBox 연산 수행
//        return Physics.CheckBox(
//            room.transform.position + room.TileCollider.center,
//            room.TileCollider.size / 2f,
//            room.transform.rotation,
//            tileMask
//        );
//    }
//}

using UnityEngine;

public static class TilePhysics
{
    public static void AlignRoom(TileController room, TileSocket socketA, TileSocket socketB)
    {
        if (room == null || socketA == null || socketB == null) return;

        // 🚨 [핵심] 스크립트가 붙은 껍데기가 아니라, 실제 위치/회전값을 가진 알맹이를 가져옵니다.
        Transform pointA = socketA.ConnectionPoint;
        Transform pointB = socketB.ConnectionPoint;

        // 알맹이(pointA, pointB)의 방향과 위치를 기준으로 연산합니다.
        Quaternion targetRotation = Quaternion.LookRotation(-pointA.forward, pointA.up);
        Quaternion differenceRotation = targetRotation * Quaternion.Inverse(pointB.rotation);
        room.transform.rotation = differenceRotation * room.transform.rotation;

        // 위치 맞추기 계산
        Vector3 offset = pointA.position - pointB.position;
        room.transform.position += offset;
    }

    public static bool IsRoomOverlapping(TileController room, LayerMask tileMask)
    {
        if (room == null || room.TileCollider == null) return false;

        room.TileCollider.enabled = false;
        Physics.SyncTransforms();

        Vector3 worldCenter = room.transform.TransformPoint(room.TileCollider.center);

        // [핵심] 스케일 오차 방지 및 '절대 수치(0.5f)'로 박스를 깎아서 문 맞닿음 오차 완전 무시
        Vector3 worldExtents = Vector3.Scale(room.TileCollider.size, room.transform.lossyScale) / 2f;
        float shrinkMargin = 0.5f;

        Vector3 shrunkExtents = new Vector3(
            Mathf.Max(0.1f, worldExtents.x - shrinkMargin),
            Mathf.Max(0.1f, worldExtents.y - shrinkMargin),
            Mathf.Max(0.1f, worldExtents.z - shrinkMargin)
        );

        Collider[] hitColliders = Physics.OverlapBox(worldCenter, shrunkExtents, room.transform.rotation, tileMask);

        room.TileCollider.enabled = true;

        if (hitColliders.Length > 0)
        {
            Debug.LogWarning($"[물리 겹침 반려] '{room.gameObject.name}' 방이 '{hitColliders[0].gameObject.name}' 오브젝트와 겹쳐서 생성을 취소합니다!");
            return true;
        }

        return false;
    }
}