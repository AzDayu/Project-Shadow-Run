using UnityEngine;
[System.Serializable]
public class BaseData 
{
    public string Id;

}
[System.Serializable]
public class PreloadData : BaseData
{
    public string Address;
    public AssetType AssetType;

}
[System.Serializable]
public class ObjectPoolData : BaseData
{
    public string Address;
    public int PoolSize;
}
[System.Serializable]
public class EnemyData : BaseData 
{
    public string PrefabAddress;
}