using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace SpaceGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;
        public SpacePlayer localPlayer;
        public List<Ship> ships;
        public Ship shipPrefab;
        public SpacePlayer playerPrefab;

        public void Awake()
        {
            Instance = this;
            Vector2 pos = UnityEngine.Random.insideUnitCircle * 10;
            Ship ship = PhotonNetwork.Instantiate(shipPrefab.name, pos, Quaternion.identity).GetComponent<Ship>();
            localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity).GetComponent<SpacePlayer>();
            ship.photonView.TransferOwnership(PhotonNetwork.MasterClient);

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