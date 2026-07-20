using UnityEngine;
public class CoverWallInfo
{
    public CoverWall Wall;
    public CoverHidePoint SelectedHidePoint;
    public Transform PeekPoint;
}
[System.Serializable]
public class CoverHidePoint
{
    public Transform HidePoint;
    public Transform PeekLeft;
    public Transform PeekRight;
}
public class CoverWall : MonoBehaviour
{
    [SerializeField] public CoverHidePoint _coverHidePoint1;
    [SerializeField] public CoverHidePoint _coverHidePoint2;
}
