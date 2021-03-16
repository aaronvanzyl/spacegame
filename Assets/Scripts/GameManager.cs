

using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;
        [HideInInspector]
        public List<Ship> ships;
        public Ship shipPrefab;

        public void Awake()
        {
            Instance = this;
            Vector2 pos = Random.insideUnitCircle * 10;
            Ship localShip = InstantiateEmptyShip(pos, Quaternion.identity);
            localShip.SetTileNetwork(Vector2Int.zero, 0, localShip.centerTileType);
            FindObjectOfType<Controller>().SelectShip(localShip);

        }

        public Ship InstantiateEmptyShip(Vector2 pos, Quaternion rotation) {
            Ship ship = PhotonNetwork.Instantiate(shipPrefab.name, pos, rotation).GetComponent<Ship>();
            ship.photonView.TransferOwnership(PhotonNetwork.MasterClient);
            return ship;
        }

        #region Photon Callbacks


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


                //LoadArena();
            }
        }


        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


                //LoadArena();
            }
        }


        #endregion


        #region Public Methods


        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }


        #endregion
    }
}