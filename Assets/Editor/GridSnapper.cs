using UnityEngine;
using UnityEditor;

public class GridSnapper : EditorWindow
{
    // X, Z축과 Y축의 그리드 사이즈를 따로 관리
    private float gridSizeXZ = 2.0f; // 바닥 타일 기준[cite: 1]
    private float gridSizeY = 3.0f;  // 벽/천장 높이 기준[cite: 1]

    [MenuItem("Tools/모듈러 스냅 툴 (Grid Snapper)")] //[cite: 1]
    public static void ShowWindow() //[cite: 1]
    {
        GetWindow<GridSnapper>("스냅 툴"); //[cite: 1]
    }

    void OnGUI() //[cite: 1]
    {
        GUILayout.Label("모듈러 에셋 조립 도우미 (수평/수직 분리형)", EditorStyles.boldLabel); //[cite: 1]
        EditorGUILayout.Space(); //[cite: 1]

        // 입력 필드를 두 개로 분리하여 인스펙터에 표시[cite: 1]
        gridSizeXZ = EditorGUILayout.FloatField("평면 크기 (X, Z축)", gridSizeXZ); //[cite: 1]
        gridSizeY = EditorGUILayout.FloatField("높이 크기 (Y축)", gridSizeY); //[cite: 1]

        EditorGUILayout.Space(); //[cite: 1]

        // 기존 스냅 버튼[cite: 1]
        if (GUILayout.Button("선택한 오브젝트 스냅하기", GUILayout.Height(30))) //[cite: 1]
        {
            SnapObjects(); //[cite: 1]
        }

        EditorGUILayout.Space();

        // 새로 추가된 콜라이더 자동 맞춤 버튼
        if (GUILayout.Button("선택한 오브젝트 BoxCollider 자동 맞춤", GUILayout.Height(30)))
        {
            FitColliderToChildren();
        }

        EditorGUILayout.Space();
        GUILayout.Label("💡 팁: 에셋을 선택하고 Ctrl+Alt+S를 눌러도 자동 스냅됩니다."); //[cite: 1]
    }

    [MenuItem("Tools/선택한 오브젝트 스냅 실행 %&s")] //[cite: 1]
    private static void SnapObjectsShortcut() //[cite: 1]
    {
        GridSnapper window = GetWindow<GridSnapper>(); //[cite: 1]
        window.SnapObjects(); //[cite: 1]
    }

    public void SnapObjects() //[cite: 1]
    {
        foreach (GameObject go in Selection.gameObjects) //[cite: 1]
        {
            // 실행 취소(Ctrl+Z) 기록[cite: 1]
            Undo.RecordObject(go.transform, "Snap Objects"); //[cite: 1]

            Vector3 position = go.transform.position; //[cite: 1]

            // X와 Z는 gridSizeXZ를 기준으로, Y는 gridSizeY를 기준으로 계산[cite: 1]
            position.x = Mathf.Round(position.x / gridSizeXZ) * gridSizeXZ; //[cite: 1]
            position.z = Mathf.Round(position.z / gridSizeXZ) * gridSizeXZ; //[cite: 1]
            position.y = Mathf.Round(position.y / gridSizeY) * gridSizeY; //[cite: 1]

            go.transform.position = position; //[cite: 1]
        }
    }

    // 새롭게 병합된 콜라이더 자동 조절 메서드
    private void FitColliderToChildren()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("적용할 오브젝트를 먼저 선택해주세요.");
            return;
        }

        foreach (GameObject obj in selectedObjects)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                Debug.LogWarning($"{obj.name} 및 하위 오브젝트에 렌더러(Renderer)가 없습니다.");
                continue;
            }

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
            boxCollider.size = bounds.size;

            Debug.Log($"<color=green>{obj.name}에 BoxCollider가 완벽하게 맞춰졌습니다!</color>");
        }
    }
}