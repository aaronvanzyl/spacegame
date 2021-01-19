using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGame
{
    [RequireComponent(typeof(Rigidbody))]
    public class Ship : MonoBehaviourPunCallbacks, IPunObservable
    {
        public Tile centerTile;
        public float moveForce;
        public Text debugLabel;
        ShipController controller;
        Rigidbody2D rb2d;
        Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        public List<Tile> tileUpdateList = new List<Tile>();

        void Awake()
        {
            controller = GetComponentInChildren<ShipController>();
            rb2d = GetComponent<Rigidbody2D>();
        }

        // Start is called before the first frame update
        void Start()
        {

            if (photonView.IsMine)
            {
                GameManager.Instance.localPlayer = this;
            }
            if (TryGetComponent<CameraFollow>(out CameraFollow follow))
            {
                follow.enabled = photonView.IsMine;
            }
            if (photonView.IsMine)
            {
                SetTile(Vector2Int.zero, centerTile.name);
            }
            GameManager.Instance.ships.Add(this);
            //photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }

        // Update is called once per frame
        void Update()
        {
            //debugLabel.text = photonView.Owner.ActorNumber + " " + controller.photonView.Owner.ActorNumber;
            debugLabel.text = photonView.Owner.NickName;
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            rb2d.AddForce(moveForce * controller.direction);
        }

        //public override void OnPlayerEnteredRoom(Player player) { 
        //    PhotonNetwork.Instantiate()
        //}

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //if (stream.IsWriting)
            //{
            //    stream.SendNext(tileUpdateList.Count);
            //    foreach (Tile tile in tileUpdateList) {
            //        stream.SendNext(tile.pos);
            //        tile.Serialize(stream);
            //        tile.awaitingNetworkUpdate = false;
            //    }
            //    tileUpdateList.Clear();
            //}
            //else
            //{
            //    int changedTileCount = (int)stream.ReceiveNext();
            //    for (int i = 0; i < changedTileCount; i++) {
            //        Vector2Int pos = (Vector2Int)stream.ReceiveNext();
            //        tiles[pos].Deserialize(stream);
            //    }
            //}
        }


        public void SetTile(Vector2Int pos, string prefabName)
        {
            if (tiles.TryGetValue(pos, out Tile existing))
            {
                PhotonNetwork.Destroy(existing.photonView);
            }

            object[] data = new object[3];
            data[0] = pos.x;
            data[1] = pos.y;
            data[2] = photonView.ViewID;
            PhotonNetwork.Instantiate("Tiles/" + prefabName, Vector3.zero, Quaternion.identity, data: data);
        }

        public Tile GetTile(Vector2Int pos)
        {
            return tiles.TryGetValue(pos, out Tile tile) ? tile : null;
        }

        public bool SolidTile(Vector2Int pos)
        {
            return tiles.TryGetValue(pos, out _);
        }

        public void OnTileAdded(Tile tile)
        {
            if (!tiles.TryGetValue(tile.pos, out _))
            {
                rb2d.mass += 1;
            }
            tiles[tile.pos] = tile;
        }

        public void OnTileDestroyed(Tile tile)
        {
            if (tiles[tile.pos] == tile)
            {
                rb2d.mass -= 1;
                tiles.Remove(tile.pos);
            }
        }

        public Vector2Int GetTileFromWorldPos(Vector3 worldPos)
        {
            Vector3 localPosVec3 = transform.InverseTransformPoint(worldPos);
            Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(localPosVec3.x), Mathf.RoundToInt(localPosVec3.y));
            return tilePos;
        }
    }
}