

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
        public Dictionary<int, Team> teams = new Dictionary<int, Team>();
        public int localTeamID;

        public void Awake()
        {
            Instance = this;

            localTeamID = Random.Range(int.MinValue, int.MaxValue);

            Vector2 pos = Random.insideUnitCircle * 10;
            Ship localShip = InstantiateEmptyShip(pos, Quaternion.identity);
            localShip.teamID = localTeamID;
            localShip.SetTileNetwork(Vector2Int.zero, 0, localShip.centerTileType);
            FindObjectOfType<Controller>().SelectShip(localShip);

            Team team = new Team(localTeamID);
            AddTeam(team);
            photonView.RPC("AddTeam", RpcTarget.Others, team);
        }

        public Ship InstantiateEmptyShip(Vector2 pos, Quaternion rotation) {
            Ship ship = PhotonNetwork.Instantiate(shipPrefab.name, pos, rotation).GetComponent<Ship>();
            ship.photonView.TransferOwnership(PhotonNetwork.MasterClient);
            return ship;
        }

        [PunRPC]
        public void AddTeam(Team team) {
            if (teams.ContainsKey(team.id)) {
                Debug.LogWarning("Already have team " + team.id);
                return;
            }
            teams.Add(team.id, team);
            Debug.Log("Added team " + team.id + " / " + teams.Count);
        }

        [PunRPC]
        public void AddTeams(Team[] teamArr)
        {
            foreach (Team t in teamArr) {
                if (teams.ContainsKey(t.id))
                {
                    Debug.LogWarning("Already have team[s] " + t.id);
                    continue;
                }
                teams.Add(t.id, t);
                Debug.Log("Added team[s] " + t.id + " / " + teams.Count);
            }
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
                Team[] teamArr = new Team[teams.Values.Count];
                teams.Values.CopyTo(teamArr, 0);
                photonView.RPC("AddTeams", other, teamArr);

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