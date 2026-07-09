using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GridSnapper : EditorWindow
{
    private float gridSizeXZ = 2.0f;
    private float gridSizeY = 3.0f;

    private bool useMagneticSnap = false;
    private float snapRadius = 1.5f;
    private float colliderScale = 0.9f;

    [MenuItem("Tools/모듈러 스냅 툴 (Grid Snapper)")]
    public static void ShowWindow()
    {
        GetWindow<GridSnapper>("스냅 툴");
    }

    [MenuItem("Tools/선택한 오브젝트 격자 스냅하기 %&s")]
    public static void SnapObjectsShortcut()
    {
        GridSnapper window = GetWindow<GridSnapper>("스냅 툴");
        window.SnapObjects();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        GUILayout.Label("모듈러 에셋 조립 도우미", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUILayout.Label("1. 격자(Grid) 스냅 기능", EditorStyles.miniBoldLabel);
        gridSizeXZ = EditorGUILayout.FloatField("평면 크기 (X, Z축)", gridSizeXZ);
        gridSizeY = EditorGUILayout.FloatField("높이 크기 (Y축)", gridSizeY);

        if (GUILayout.Button("선택한 오브젝트 격자 스냅하기 (Ctrl+Alt+S)", GUILayout.Height(25)))
        {
            SnapObjects();
        }

        EditorGUILayout.Space();

        GUILayout.Label("2. 워프레임식 자석 스냅 (드래그 후 놓을 때 작동)", EditorStyles.miniBoldLabel);
        useMagneticSnap = EditorGUILayout.Toggle("자석 스냅 활성화", useMagneticSnap);
        snapRadius = EditorGUILayout.FloatField("자석 감지 반경 (m)", snapRadius);

        EditorGUILayout.Space();

        GUILayout.Label("3. 유틸리티", EditorStyles.miniBoldLabel);
        colliderScale = EditorGUILayout.Slider("콜리더 크기 비율", colliderScale, 0.5f, 1.0f);

        if (GUILayout.Button("선택한 오브젝트 BoxCollider 자동 맞춤", GUILayout.Height(25)))
        {
            FitColliderToChildren();
        }

        if (GUILayout.Button("선택한 오브젝트 이름 변경 (X, Y, Z 좌표 기준)", GUILayout.Height(25)))
        {
            RenameObjectsByPosition();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!useMagneticSnap || Selection.activeGameObject == null) return;

        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
        {
            GameObject activeObj = Selection.activeGameObject;

            List<Transform> mySockets = FindAllSocketsInObject(activeObj.transform);
            if (mySockets.Count == 0) return;

            List<Transform> targetSockets = FindAllSocketsInScene(activeObj);

            foreach (Transform mySock in mySockets)
            {
                foreach (Transform targetSock in targetSockets)
                {
                    float distance = Vector3.Distance(mySock.position, targetSock.position);

                    if (distance <= snapRadius)
                    {
                        Undo.RecordObject(activeObj.transform, "Magnetic Snap");

                        // 🌟 [수정 2] 회전 꼬임 및 좌표 어긋남 완벽 해결
                        // LookRotation을 사용해 단 한 번의 연산으로 정면과 하늘 방향을 동시에 정확하게 맞춥니다.
                        // 💡 (만약 에셋 특성상 같은 방향으로 겹친다면 '-targetSock.forward'에서 마이너스(-)를 지우세요)
                        Quaternion targetRotation = Quaternion.LookRotation(-targetSock.forward, targetSock.up);

                        // 현재 타일(부모)과 소켓 간의 로컬 회전 차이를 역산하여 부모를 강제로 돌립니다.
                        Quaternion rootToSocketRot = Quaternion.Inverse(activeObj.transform.rotation) * mySock.rotation;
                        activeObj.transform.rotation = targetRotation * Quaternion.Inverse(rootToSocketRot);

                        // 회전이 완벽하게 끝난 상태의 좌표를 다시 가져와서 오차 없이 정확히 1:1로 겹치게 평행이동합니다.
                        Vector3 offset = mySock.position - activeObj.transform.position;
                        activeObj.transform.position = targetSock.position - offset;

                        sceneView.Repaint();
                        return;
                    }
                }
            }
        }
    }

    // 내 오브젝트 소켓 찾기
    private List<Transform> FindAllSocketsInObject(Transform root)
    {
        List<Transform> sockets = new List<Transform>();
        // 🌟 [수정 1] false를 넣어 원초적으로 꺼진 오브젝트 탐색 방지
        Transform[] allChildren = root.GetComponentsInChildren<Transform>(false);

        foreach (Transform child in allChildren)
        {
            if (child.parent != null && child.parent.name == "MatchPoint")
            {
                // 부모(MatchPoint)와 자식(01, 02)이 모두 진짜 켜져 있는지 이중으로 확실하게 체크합니다.
                if (child.gameObject.activeInHierarchy && child.parent.gameObject.activeInHierarchy)
                {
                    sockets.Add(child);
                }
            }
        }
        return sockets;
    }

    // 씬의 다른 소켓 찾기
    private List<Transform> FindAllSocketsInScene(GameObject excludeObj)
    {
        List<Transform> sceneSockets = new List<Transform>();
        Transform[] allTransforms = FindObjectsOfType<Transform>();

        foreach (Transform t in allTransforms)
        {
            if (!t.IsChildOf(excludeObj.transform) && t.parent != null && t.parent.name == "MatchPoint")
            {
                // 여기도 마찬가지로 완벽한 이중 활성화 체크를 적용합니다.
                if (t.gameObject.activeInHierarchy && t.parent.gameObject.activeInHierarchy)
                {
                    sceneSockets.Add(t);
                }
            }
        }
        return sceneSockets;
    }

    public void SnapObjects()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Snap Object");
            Vector3 pos = go.transform.position;
            pos.x = Mathf.Round(pos.x / gridSizeXZ) * gridSizeXZ;
            pos.y = Mathf.Round(pos.y / gridSizeY) * gridSizeY;
            pos.z = Mathf.Round(pos.z / gridSizeXZ) * gridSizeXZ;
            go.transform.position = pos;
        }
    }

    private void FitColliderToChildren()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) continue;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            Undo.RecordObject(obj, "Fit BoxCollider");
            BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = Undo.AddComponent<BoxCollider>(obj);
            }

            boxCollider.center = obj.transform.InverseTransformPoint(bounds.center);
            boxCollider.size = bounds.size * colliderScale;
        }
        Debug.Log($"<color=green>선택한 오브젝트들에 BoxCollider가 {colliderScale * 100:F0}% 크기로 맞춰졌습니다!</color>");
    }

    private void RenameObjectsByPosition()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0) return;

        foreach (GameObject go in selectedObjects)
        {
            Undo.RecordObject(go, "Rename Object By Position");
            Vector3 pos = go.transform.position;
            int roundedX = Mathf.RoundToInt(pos.x);
            int roundedY = Mathf.RoundToInt(pos.y);
            int roundedZ = Mathf.RoundToInt(pos.z);
            go.name = $"({roundedX}, {roundedY}, {roundedZ})";
        }
    }
}