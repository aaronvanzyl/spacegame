using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace SpaceGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Animator))]
    public class SpacePlayer : MonoBehaviourPunCallbacks
    {
        public float groundedMoveSpeed;
        public float ungroundedMoveForce;

        Collider2D col;
        Animator animator;
        Rigidbody2D rb2d;
        Ship attached;
        ContactFilter2D tileContactFilter;

        Tile occupying;

        void Start()
        {
            if (!GetComponent<PhotonView>().IsMine)
            {
                enabled = false;
            }
            rb2d = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            col = GetComponent<Collider2D>();

            tileContactFilter = new ContactFilter2D().NoFilter();
        }

        private void Update()
        {
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

            Vector2 input;
            if (occupying == null)
            {
                input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                animator.SetBool("InTile", false);
                animator.SetBool("Moving", input.magnitude > 0.1f);

                if (input.magnitude > 0.1f)
                {
                    transform.eulerAngles = input.x > 0 ? Vector3.zero : new Vector3(0, 180, 0);
                }

                if (attached != null)
                {
                    rb2d.velocity = attached.rb2d.velocity;
                    if (input.magnitude > 0.1f)
                    {
                        rb2d.AddForce(rb2d.mass * input * groundedMoveSpeed, ForceMode2D.Impulse);
                    }
                }
                else
                {
                    rb2d.AddForce(input * ungroundedMoveForce);
                }
            }
            else
            {
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
                    attached = tile.ship;
                    return;
                }
            }
            attached = null;
        }
    }
}