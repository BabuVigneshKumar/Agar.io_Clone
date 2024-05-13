
//using Mirror;
//using UnityEngine;
//public class AgarNetworkManager : NetworkManager
//{
//    public GameObject AgarObj;
//    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
//    {
//        base.OnServerAddPlayer(conn);

//        Debug.Log("<color=fusia>Attempting to instantiate player object.</color>");

      
//        if (AgarObj == null)
//        {
//            Debug.LogError("<color=red>AgarObj prefab is not set.</color>");
//            return;
//        }

       
//        GameObject player = Instantiate(AgarObj, GetStartPosition().position, Quaternion.identity);
//        if (player == null)
//        {
//            Debug.LogError("<color=red>Failed to instantiate player object.</color>");
//            return;            
//        }

//        Debug.Log("<color=green>Player object instantiated successfully.</color>");

       
//        NetworkIdentity playerNetIdentity = player.GetComponent<NetworkIdentity>();
//        if (playerNetIdentity == null)
//        {
//            Debug.LogError("<color=red>No NetworkIdentity component found on player object.</color>");
//            return;
//        }

//        Debug.Log("<color=green>NetworkIdentity component found on player object.</color>");

       
//        NetworkServer.Spawn(player, conn);

        
//        playerNetIdentity.AssignClientAuthority(conn);
//        Debug.Log($"<color=green>Client authority assigned to player object.</color> {conn}  --> Connection Id {conn.connectionId}  Connection Address -->  {conn.address}");


//    }
//}
