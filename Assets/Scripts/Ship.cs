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
        public Rigidbody2D rb2d;
        Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        List<ShipController> controllers = new List<ShipController>();
        List<Thruster> thrusters = new List<Thruster>();
        public List<SpacePlayer> attachedPlayers = new List<SpacePlayer>();
        const float E = 0.001f;
        const float rotationTolerance = 0.1f;

        void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        // Start is called before the first frame update
        void Start()
        {

            rb2d.centerOfMass = Vector2.zero;
            if (TryGetComponent(out CameraController follow))
            {
                follow.enabled = photonView.IsMine;
            }
            if (photonView.IsMine)
            {
                SetTile(Vector2Int.zero, 0, centerTile.name);
            }
            GameManager.Instance.ships.Add(this);
            //photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }

        // Update is called once per frame
        void Update()
        {
            //debugLabel.text = photonView.Owner.ActorNumber + " " + controller.photonView.Owner.ActorNumber;
            //debugLabel.text = photonView.Owner.NickName;
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            float netRotation = 0;
            Vector2 netMoveDirection = Vector2.zero;
            int numActiveControllers = 0;
            foreach (ShipController controller in controllers)
            {
                if (controller.moveDirection.magnitude > 0 || Mathf.Abs(controller.rotation) > 0)
                {
                    netRotation += controller.rotation;
                    netMoveDirection += controller.moveDirection;
                    numActiveControllers++;
                }
            }
            if (numActiveControllers > 0)
            {
                netMoveDirection /= (float)numActiveControllers;
                netRotation /= (float)numActiveControllers;
                bool allowOrtho = netMoveDirection.magnitude < E;
                bool allowRotation = Mathf.Abs(netRotation) > E;
                Debug.Log(netMoveDirection + " " + allowOrtho + " " + allowRotation);
                AccelDirection(netMoveDirection, allowOrtho, allowRotation, netMoveDirection.magnitude, netRotation * 50);
                //rb2d.AddForce(netMoveDirection * moveForce);
                //foreach (Thruster thruster in thrusters) {
                //    thruster.activation = netMoveDirection.y;
                //}
            }
            //print("pre velocity" + rb2d.velocity + " " + rb2d.angularVelocity);
            foreach (Thruster thruster in thrusters) {
                thruster.ApplyForce();
            }
            //print("post velocity " + rb2d.velocity + " " + rb2d.angularVelocity);
            foreach (SpacePlayer player in attachedPlayers) {
                player.PostShipFixedUpdate();
            }
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


        public void SetTile(Vector2Int pos, float rotation, string prefabName)
        {
            if (tiles.TryGetValue(pos, out Tile existing))
            {
                PhotonNetwork.Destroy(existing.photonView);
            }

            object[] data = new object[4];
            data[0] = pos.x;
            data[1] = pos.y;
            data[2] = photonView.ViewID;
            data[3] = rotation;
            PhotonNetwork.Instantiate("Tiles/" + prefabName, Vector3.zero, Quaternion.identity, data: data);
        }

        public Tile GetTile(Vector2Int pos)
        {
            return tiles.TryGetValue(pos, out Tile tile) ? tile : null;
        }

        public bool TileExists(Vector2Int pos)
        {
            return tiles.TryGetValue(pos, out _);
        }

        public void OnTileAdded(Tile tile)
        {
            if (photonView.IsMine)
            {
                tile.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
            if (!tiles.TryGetValue(tile.pos, out _))
            {
                rb2d.centerOfMass = (rb2d.mass * rb2d.centerOfMass + (Vector2)tile.pos) / (rb2d.mass + 1);
                rb2d.mass += 1;
            }
            tiles[tile.pos] = tile;
            if (tile is ShipController controller)
            {
                controllers.Add(controller);
            }
            if (tile is Thruster thruster)
            {
                thrusters.Add(thruster);
            }
        }

        public void OnTileDestroyed(Tile tile)
        {
            if (tiles[tile.pos] == tile)
            {
                rb2d.centerOfMass = (rb2d.mass * rb2d.centerOfMass - (Vector2)tile.pos) / (rb2d.mass - 1);
                rb2d.mass -= 1;
                tiles.Remove(tile.pos);
            }
            if (tile is ShipController controller)
            {
                controllers.Remove(controller);
            }
            if (tile is Thruster thruster)
            {
                thrusters.Remove(thruster);
            }
        }

        public Vector2Int WorldToTilePos(Vector3 worldPos)
        {
            Vector3 localPosVec3 = transform.InverseTransformPoint(worldPos);
            Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(localPosVec3.x), Mathf.RoundToInt(localPosVec3.y));
            return tilePos;
        }

        public void AccelDirection(Vector2 dir, bool allowOrtho, bool allowRotate, float moveWeight, float rotateWeight, float accelMult = 1)
        {
            dir = dir.normalized;
            int numVars = thrusters.Count;
            int numEqs = thrusters.Count; // each thruster < 1
            if (!allowOrtho) numEqs += 2; // 0 < l,r < 0
            if (!allowRotate) numEqs += 2; // 0 < torque l,r < 0
            float[,] lhs = new float[numEqs, numVars];
            float[] rhs = new float[numEqs];
            float[] objective = new float[numVars];

            int eq = 0;

            // Each thruster activation is below 1
            for (int i = 0; i < thrusters.Count; i++)
            {
                lhs[eq, i] = 1;
                rhs[eq] = 1;
                eq += 1;
            }

            if (!allowOrtho)
            {
                // Orthogonal force is 0
                for (int i = 0; i < thrusters.Count; i++)
                {
                    lhs[eq, i] = PerpDot(thrusters[i].transform.up, dir) * thrusters[i].force; // r < 0
                    lhs[eq + 1, i] = -lhs[eq, i]; // l < 0
                }
                rhs[eq] = E;
                rhs[eq + 1] = E;
                eq += 2;
            }

            if (!allowRotate)
            {
                // Torque is 0
                for (int i = 0; i < thrusters.Count; i++)
                {
                    lhs[eq, i] = GetTorque(thrusters[i]); // r < 0
                    lhs[eq + 1, i] = -lhs[eq, i]; // l < 0
                }
                rhs[eq] = rotationTolerance;
                rhs[eq + 1] = rotationTolerance;
                eq += 2;
            }

            // Set objective (maximum parallel force)

            print("---");
            for (int i = 0; i < thrusters.Count; i++)
            {
                objective[i] = 0;
                objective[i] += Vector2.Dot(thrusters[i].transform.up, dir) * thrusters[i].force * moveWeight;
                float torque = GetTorque(thrusters[i]);
                if (Mathf.Abs(torque) > 0.1f)
                {
                    objective[i] += torque * rotateWeight;
                }
            }

            SimplexSolver solver = new SimplexSolver(lhs, rhs, objective);
            double[] result = solver.Solve();
            if (result != null)
            {
                for (int i = 0; i < thrusters.Count; i++)
                {
                    thrusters[i].activation = (float)result[i] * accelMult;
                }
                //print("Found solution!");
            }
            else
            {
                //print("No satisfying result");
                for (int i = 0; i < thrusters.Count; i++)
                {

                    thrusters[i].activation = 0;
                }

            }
        }

        float GetTorque(Thruster t)
        {
            //Vector2 r = (Vector2)t.transform.position - ((Vector2)transform.position + rb2d.centerOfMass);
            Vector2 r = t.pos - rb2d.centerOfMass;
            Vector3 localUp = t.transform.InverseTransformDirection(t.transform.up);
            float angleRad = Vector2.SignedAngle(r, localUp) * Mathf.Deg2Rad;
            float torqueDeg = r.magnitude * Mathf.Sin(angleRad) * Mathf.Rad2Deg * t.force;
            Debug.Log(r + " " + angleRad + " " + torqueDeg);
            return torqueDeg;
        }

        float PerpDot(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
    }

}