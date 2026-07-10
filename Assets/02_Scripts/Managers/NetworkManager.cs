using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Inst { get; set; }

    public NetworkShopService ShopService { get; private set; }
    //public NetworkStashService StashService { get; private set; }


    private void Awake()
    {
        Inst = this;
        InitNetworkService();
    }

    private void InitNetworkService()
    {
        ShopService = new NetworkShopService();
        //StashService = new NetworkStashService();
    }
}
