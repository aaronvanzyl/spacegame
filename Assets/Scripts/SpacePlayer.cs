using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace SpaceGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpacePlayer : MonoBehaviourPunCallbacks
    {
        public Text nameTag;
        public float groundedMoveSpeed;
        public float ungroundedMoveForce;

        Collider2D col;
        Animator animator;
        Rigidbody2D rb2d;
        Ship attached;
        ContactFilter2D tileContactFilter;
        SpriteRenderer spriteRenderer;

        public Tile occupying;

        Vector2 relativeInput;

        float baseDrag;
        float baseAngularDrag;

        void Start()
        {
            nameTag.text = photonView.Owner.NickName;
            rb2d = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            tileContactFilter = new ContactFilter2D().NoFilter();
            baseDrag = rb2d.drag;
            baseAngularDrag = rb2d.angularDrag;

            if (!GetComponent<PhotonView>().IsMine)
            {
                enabled = false;
            }
        }

        private void Update()
        {
            //relativeInput = transform.right * Input.GetAxis("Horizontal") + transform.up * Input.GetAxis("Vertical");
            relativeInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            animator.SetBool("Moving", relativeInput.magnitude > 0);
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
            {
                spriteRenderer.flipX = Input.GetAxis("Horizontal") < 0;
            }

            if (Input.GetButtonDown("Interact"))
            {
                Debug.Log("interacting");
                if (occupying != null)
                {
                    occupying.isOccupied = false;
                    occupying = null;
                }
                else if (attached != null)
                {
                    Tile tile = attached.GetTile(attached.WorldToTilePos(transform.position));

                    Debug.Log("getting a tile: " + tile.canOccupy + " " + tile.isOccupied);
                    if (tile != null && tile.canOccupy && !tile.isOccupied)
                    {
                        Debug.Log("occupying tile");
                        occupying = tile;
                        tile.isOccupied = true;
                        tile.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                        animator.SetBool("Moving", false);
                        animator.SetBool("InTile", true);
                    }
                }
            }
        }

        void FixedUpdate()
        {
            CheckGrounded();
            if (attached == null)
            {
                rb2d.AddForce(relativeInput * ungroundedMoveForce);
            }
            PostShipFixedUpdate();
        }

        public void PostShipFixedUpdate()
        {
            if (attached == null)
            {
                return;
            }
            if (occupying == null)
            {
                //Vector3 eulerAngles = transform.eulerAngles;
                //eulerAngles.z = attached.transform.eulerAngles.z;
                //transform.eulerAngles = eulerAngles;

                //rb2d.angularVelocity = attached.rb2d.angularVelocity;
                rb2d.velocity = attached.rb2d.GetPointVelocity(transform.position);
                rb2d.AddForce(rb2d.mass * relativeInput * groundedMoveSpeed, ForceMode2D.Impulse);
            }
            else
            {
                //transform.rotation = attached.transform.rotation;
                //rb2d.angularVelocity = attached.rb2d.angularVelocity;
                rb2d.transform.position = occupying.transform.position;
                rb2d.velocity = attached.rb2d.velocity;
            }
        }

        void CheckGrounded()
        {
            List<Collider2D> overlapping = new List<Collider2D>();
            col.OverlapCollider(tileContactFilter, overlapping);
            foreach (Collider2D other in overlapping)
            {
                if (other.isTrigger && other.TryGetComponent(out Tile tile))
                {
                    if (tile.ship != attached)
                    {
                        if (attached != null)
                        {
                            attached.attachedPlayers.Remove(this);
                        }
                        attached = tile.ship;
                        tile.ship.attachedPlayers.Add(this);

                        rb2d.drag = attached.rb2d.drag;
                        rb2d.angularDrag = attached.rb2d.angularDrag;

                    }
                    return;
                }
            }
            if (attached != null)
            {
                attached.attachedPlayers.Remove(this);
                rb2d.drag = baseDrag;
                rb2d.angularDrag = baseAngularDrag;
                attached = null;
            }
        }
    }
}