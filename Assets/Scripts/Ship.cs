using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGame
{
    [RequireComponent(typeof(Rigidbody))]
    public class Ship : MonoBehaviourPunCallbacks, IPunObservable
    {
        public float moveForce;
        public Text debugLabel;
        public int centerTileType;
        public TileLookupScriptableObject tileLookup;

        [HideInInspector]
        public Rigidbody2D rb2d;
        public int teamID;

        [HideInInspector]
        public Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        List<Thruster> thrusters = new List<Thruster>();
        Vector2 moveTarget = Vector2.zero;
        bool hasMoveTarget = false;
        Vector2 rotateTarget = Vector2.zero;
        bool hasRotateTarget = false;

        const float E = 0.001f;
        const float rotationTolerance = 0.01f;

        Vector2Int[] tileDirs = new Vector2Int[] { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };

        const int DELIMITER = int.MinValue + 1;

        public bool selected;


        void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            GameManager.Instance.ships.Add(this);

        }

        void Update()
        {
            if (GameManager.Instance.teams.TryGetValue(teamID, out Team team))
            {
                foreach (Tile tile in tiles.Values)
                {

                    Color col = team.color;
                    if (!selected)
                    {
                        col *= 0.7f;
                        col.a = 1f;
                    }
                    tile.outline.color = col;
                }
            }
            //debugLabel.text = photonView.Owner.ActorNumber + " " + controller.photonView.Owner.ActorNumber;
            //debugLabel.text = photonView.Owner.NickName;
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (Vector2.Distance(transform.position, moveTarget) < 3f)
            {
                hasMoveTarget = false;
            }
            if (Vector2.Distance(transform.position, rotateTarget) < 3f)
            {
                hasRotateTarget = false;
            }

            Vector2 relativeMoveDirection = (moveTarget - (Vector2)transform.position).normalized;
            Vector2 relativeRotateVector = (rotateTarget - (Vector2)transform.position).normalized;

            //interceptVelocity(Vector2 projectileAcc, Vector2 targetAcc, Vector2 targetVelocity, Vector2 dist, double projectileSpeed)
            Vector2 relativeVelocity = -rb2d.velocity; // velocity of the target as seen from the ship: velocity of target (none) - velocity of ship
            //Vector2 chaseVector = Chase.interceptVelocity(Vector2.zero, Vector2.zero, relativeVelocity, relativeMoveDirection, 500f/*rb2d.velocity.magnitude*/);
            //chaseVector = chaseVector.normalized;

            double acc = AccelDirection(transform.up, false, false, 1, 0).Sum() / rb2d.mass; // TODO: should only call this when a tile is added
            double chaseAngle = relativeMoveDirection.y > 0 ? 1 : -1 * Chase.interceptAngle(acc, relativeVelocity, (moveTarget - (Vector2)transform.position));
            Vector2 chaseVec = new Vector2((float)Math.Cos(chaseAngle), (float)Math.Sin(chaseAngle)).normalized;

            float relativeRotation = Vector2.SignedAngle(transform.up, relativeRotateVector);
            //float relativeRotation = Vector2.SignedAngle(transform.up, chaseVec);
            relativeRotation -= rb2d.angularVelocity * 0.1f;
            float accelMult = 1;


            bool allowOrtho = hasRotateTarget;
            bool allowRotation = hasRotateTarget;
            Debug.DrawRay(transform.position, chaseVec, Color.red);
            if (hasMoveTarget || (hasRotateTarget && Mathf.Abs(relativeRotation) > 5f))
            {
                //Debug.Log(netMoveDirection + " " + netRotation + " " + allowOrtho + " " + allowRotation);
                double[] result = AccelDirection(relativeMoveDirection, allowOrtho, allowRotation, 1, relativeRotation * 10);
                //double[] result = AccelDirection(chaseVec, allowOrtho, allowRotation, 1, relativeRotation * 10);

                if (result != null)
                {
                    for (int i = 0; i < thrusters.Count; i++)
                    {
                        thrusters[i].Activation = (float)result[i] * accelMult;
                    }
                    //print("Found solution!");
                }
                else
                {
                    //print("No satisfying result");
                    for (int i = 0; i < thrusters.Count; i++)
                    {

                        thrusters[i].Activation = 0;
                    }

                }

                //AccelDirection(chaseVector, allowOrtho, allowRotation, 1, Vector2.SignedAngle(transform.up, chaseVector));
                //float netTorque = 0;
                //foreach (Thruster t in thrusters)
                //{
                //    netTorque += GetTorque(t) * t.activation;
                //}
            }
            else
            {
                foreach (Thruster t in thrusters)
                {
                    t.Activation = 0;
                }
            }
            foreach (Thruster thruster in thrusters)
            {
                ApplyThrusterForce(thruster);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(teamID);
                int syncCount = 0;
                //foreach (Tile tile in tiles.Values)
                //{
                //    if (tile.HasSyncedComponents())
                //    {
                //        syncCount++;
                //    }
                //}
                syncCount = tiles.Count;
                stream.SendNext(syncCount);

                foreach (Tile tile in tiles.Values)
                {
                    //if (tile.HasSyncedComponents())
                    //{
                        stream.SendNext(tile.pos);
                        tile.OnPhotonSerializeView(stream, info);
                        stream.SendNext(DELIMITER);
                    //}
                }

            }
            else {
                teamID = (int)stream.ReceiveNext();
                int syncCount = (int) stream.ReceiveNext();
                for (int i = 0; i < syncCount; i++) { 
                    Vector2Int pos = (Vector2Int)stream.ReceiveNext();
                    if (tiles.TryGetValue(pos, out Tile tile))
                    {
                        tile.OnPhotonSerializeView(stream, info);
                        stream.ReceiveNext(); //Receive delimiter
                    }
                    else {
                        while ((int)stream.ReceiveNext() != DELIMITER) { }
                    }
                }
            }
        }


        [PunRPC]
        void SetTileRPC(int x, int y, float rotation, int tileType)
        {
            // Create tile
            Tile tile = Instantiate(tileLookup.tilePrefabs[tileType], transform).GetComponent<Tile>();
            tile.tileType = tileType;
            AttachTile(x, y, rotation, tile);
        }

        void AttachTile(int x, int y, float rotation, Tile tile)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (tiles.TryGetValue(pos, out Tile existing))
            {
                DestroyTile(existing, true);
            }

            tile.transform.parent = transform;
            tile.transform.localPosition = (Vector2)pos;
            tile.pos = pos;
            tile.transform.localEulerAngles = new Vector3(0, 0, rotation);
            tiles[tile.pos] = tile;
            tile.ship = this;

            // Update CoM
            rb2d.centerOfMass = (rb2d.mass * rb2d.centerOfMass + (Vector2)tile.pos) / (rb2d.mass + 1);
            rb2d.mass += 1;

            // Update lists
            if (tile.TryGetComponent(out Thruster thruster))
            {
                thrusters.Add(thruster);
            }
        }

        [PunRPC]
        void DestroyTileRPC(int x, int y, bool silent)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (tiles.TryGetValue(pos, out Tile existing))
            {
                DestroyTile(existing, silent);
            }
        }

        [PunRPC]
        void MoveTilesRPC(Vector2Int[] posList, int targetID)
        {
            Ship target = PhotonNetwork.GetPhotonView(targetID).GetComponent<Ship>();
            foreach (Vector2Int pos in posList)
            {
                Tile tile = tiles[pos];
                DetachTile(tile);
                target.AttachTile(pos.x, pos.y, tile.transform.localEulerAngles.z, tile);
            }
        }

        public void SetTileNetwork(Vector2Int pos, float rotation, int tileType)
        {
            photonView.RPC("SetTileRPC", RpcTarget.All, pos.x, pos.y, rotation, tileType);
        }

        public void DestroyTileNetwork(Vector2Int pos, bool silent)
        {
            photonView.RPC("DestroyTileRPC", RpcTarget.All, pos.x, pos.y, silent);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (newPlayer == PhotonNetwork.LocalPlayer)
            {
                return;
            }
            foreach (KeyValuePair<Vector2Int, Tile> pair in tiles)
            {
                photonView.RPC("SetTileRPC", newPlayer, pair.Key.x, pair.Key.y, 0f, pair.Value.tileType);
            }
        }

        public bool TryGetTile(Vector2Int pos, out Tile tile)
        {
            return tiles.TryGetValue(pos, out tile);
        }

        public Tile GetTile(Vector2Int pos)
        {
            return tiles.TryGetValue(pos, out Tile tile) ? tile : null;
        }

        public bool TileExists(Vector2Int pos)
        {
            return tiles.TryGetValue(pos, out _);
        }


        public void DestroyTile(Tile tile, bool silent)
        {
            DetachTile(tile);
            tile.Die(silent);

            AttemptSplit();
        }

        public void DetachTile(Tile tile)
        {
            // Update CoM
            rb2d.centerOfMass = (rb2d.mass * rb2d.centerOfMass - (Vector2)tile.pos) / (rb2d.mass - 1);
            rb2d.mass -= 1;

            // Update lists
            if (tile.TryGetComponent(out Thruster thruster))
            {
                thrusters.Remove(thruster);
            }

            tiles.Remove(tile.pos);
            tile.ship = null;
        }

        void Print<T>(List<T> list)
        {
            string s = "";
            foreach (object o in list)
            {
                s += o;
            }
            Debug.Log(s);

        }

        public void AttemptSplit()
        {
            List<List<Vector2Int>> SCCs = FindSCCs();
            for (int i = 1; i < SCCs.Count; i++)
            {
                Ship newShip = GameManager.Instance.InstantiateEmptyShip(transform.position, transform.rotation);
                newShip.teamID = teamID;
                MoveTilesRPC(SCCs[i].ToArray(), newShip.photonView.ViewID);
            }

        }

        public List<List<Vector2Int>> FindSCCs()
        {
            List<List<Vector2Int>> SCCs = new List<List<Vector2Int>>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            foreach (Tile tile in tiles.Values)
            {
                if (!visited.Contains(tile.pos))
                {
                    List<Vector2Int> scc = new List<Vector2Int>();
                    Queue<Vector2Int> edge = new Queue<Vector2Int>();
                    edge.Enqueue(tile.pos);
                    visited.Add(tile.pos);
                    scc.Add(tile.pos);
                    while (edge.Count > 0)
                    {
                        Vector2Int edgePos = edge.Dequeue();
                        foreach (Vector2Int dir in tileDirs)
                        {
                            if (TileExists(edgePos + dir) && !visited.Contains(edgePos + dir))
                            {
                                edge.Enqueue(edgePos + dir);
                                visited.Add(edgePos + dir);
                                scc.Add(edgePos + dir);
                            }
                        }
                    }
                    SCCs.Add(scc);
                }
            }
            return SCCs;
        }

        public Vector2Int WorldToTilePos(Vector3 worldPos)
        {
            Vector3 localPosVec3 = transform.InverseTransformPoint(worldPos);
            Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(localPosVec3.x), Mathf.RoundToInt(localPosVec3.y));
            return tilePos;
        }

        public double[] AccelDirection(Vector2 dir, bool allowOrtho, bool allowRotate, float moveWeight, float rotateWeight)
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

            //print("---");
            for (int i = 0; i < thrusters.Count; i++)
            {
                objective[i] = 0;
                float movement = Vector2.Dot(thrusters[i].transform.up, dir) * thrusters[i].force * moveWeight;
                //Debug.Log(i + " " + movement);
                if (Mathf.Abs(movement) > 0.1f)
                {
                    objective[i] += movement;
                }
                float torque = GetTorque(thrusters[i]) * rotateWeight;
                if (Mathf.Abs(torque) > 0.1f)
                {
                    objective[i] += torque;
                }
            }

            SimplexSolver solver = new SimplexSolver(lhs, rhs, objective);
            double[] result = solver.Solve();
            return result;
        }

        float GetTorque(Thruster t)
        {
            Vector2 r = t.tile.pos - rb2d.centerOfMass;
            Vector3 localForce = transform.InverseTransformDirection(t.transform.up) * t.force;
            float torque = Vector3.Cross(r, localForce).z;

            //Debug.Log(r + " " + localForce + " " + torque);
            return torque;
        }


        float PerpDot(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public void ApplyThrusterForce(Thruster t)
        {
            rb2d.AddForceAtPosition(t.transform.up * t.force * t.Activation * Time.fixedDeltaTime, t.transform.position, ForceMode2D.Impulse);
        }

        [PunRPC]
        public void SetMoveTarget(Vector2 target)
        {
            moveTarget = target;
            hasMoveTarget = true;
        }

        public Vector2 GetMoveTarget()
        {
            return moveTarget;
        }

        [PunRPC]
        public void SetRotateTarget(Vector2 target)
        {
            rotateTarget = target;
            hasRotateTarget = true;
        }

        public Vector2 GetRotateTarget()
        {
            return moveTarget;
        }
    }

}