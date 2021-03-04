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
        public float moveForce;
        public Text debugLabel;
        public int centerTileType;
        public TileLookupScriptableObject tileLookup;

        [HideInInspector]
        public Rigidbody2D rb2d;
        [HideInInspector]
        public int team;

        Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        List<Thruster> thrusters = new List<Thruster>();
        Vector2 moveTarget = Vector2.zero;
        bool hasMoveTarget = false;
        Vector2 rotateTarget = Vector2.zero;
        bool hasRotateTarget = false;

        const float E = 0.001f;
        const float rotationTolerance = 0.01f;

        

        void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            team = photonView.ViewID;
            rb2d.centerOfMass = Vector2.zero;
            if (photonView.IsMine)
            {
                SetTileNetwork(Vector2Int.zero, 0, centerTileType);
            }
            GameManager.Instance.ships.Add(this);
        }

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

            if (Vector2.Distance(transform.position, moveTarget) < 3f) {
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
            Vector2 chaseVector = Chase.interceptVelocity(Vector2.zero, Vector2.zero, relativeVelocity, relativeMoveDirection, 500f/*rb2d.velocity.magnitude*/);
            chaseVector = chaseVector.normalized;

            float relativeRotation = Vector2.SignedAngle(transform.up, relativeRotateVector);
            relativeRotation -= rb2d.angularVelocity * 0.1f;

            bool allowOrtho = hasRotateTarget;
            bool allowRotation = hasRotateTarget;
            if (hasMoveTarget || (hasRotateTarget && Mathf.Abs(relativeRotation) > 5f))
            {
                //Debug.Log(netMoveDirection + " " + netRotation + " " + allowOrtho + " " + allowRotation);
                AccelDirection(relativeMoveDirection, allowOrtho, allowRotation, 1, relativeRotation * 10);
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

            foreach (Tile tile in tiles.Values) {
                if (tile.HasSyncedComponents()) {
                    tile.OnPhotonSerializeView(stream, info);
                }
            }
        }


        [PunRPC]
        void SetTileRPC(int x, int y, float rotation, int tileType)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (tiles.TryGetValue(pos, out Tile existing))
            {
                DestroyTile(existing);
            }

            // Create tile
            Tile tile = Instantiate(tileLookup.tilePrefabs[tileType], transform).GetComponent<Tile>();
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
        void DestroyTileRPC(int x, int y)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (tiles.TryGetValue(pos, out Tile existing))
            {
                DestroyTile(existing);
            }
        }

        public void SetTileNetwork(Vector2Int pos, float rotation, int tileType)
        {
            photonView.RPC("SetTileRPC", RpcTarget.All, pos.x, pos.y, rotation, tileType);
        }

        public void DestroyTileNetwork(Vector2Int pos)
        {
            photonView.RPC("DestroyTileRPC", RpcTarget.All, pos.x, pos.y);
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


        public void DestroyTile(Tile tile)
        {
            // Update CoM
            rb2d.centerOfMass = (rb2d.mass * rb2d.centerOfMass - (Vector2)tile.pos) / (rb2d.mass - 1);
            rb2d.mass -= 1;

            // Update lists
            if (tile.TryGetComponent(out Thruster thruster))
            {
                thrusters.Remove(thruster);
            }

            // Destroy tile
            tiles.Remove(tile.pos);
            Destroy(tile.gameObject);
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