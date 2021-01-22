using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace SpaceGame {
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Animator))]
    public class SpacePlayer : MonoBehaviourPunCallbacks
    {
        public ContactFilter2D cf;
        public float groundedMoveSpeed;
        public float ungroundedMoveForce;

        Collider2D col;
        Animator animator;
        Rigidbody2D rb2d;
        Ship attached;
        ContactFilter2D tileContactFilter;

        // Start is called before the first frame update
        void Start()
        {
            if (!GetComponent<PhotonView>().IsMine) {
                enabled = false;
            }
            rb2d = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            col = GetComponent<Collider2D>();

            tileContactFilter = new ContactFilter2D().NoFilter();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            CheckGrounded();

            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            animator.SetBool("Moving", input.magnitude > 0.1f);
            if (input.magnitude > 0.1f) {
                transform.eulerAngles = input.x > 0 ? Vector3.zero : new Vector3(0, 180f, 0);
            }

            if (attached != null)
            {
                //rb2d.velocity = 
                //rb2d.AddForce(input);
                //if (rb2d.velocity.magnitude > groundedSpeed) {
                //    rb2d.velocity = input * groundedSpeed;
                //}
                //rb2d.velocity = attached.rb2d.velocity + input * groundedSpeed;
                if (input.magnitude < 0.1f)
                {
                    rb2d.AddForce(-rb2d.mass * rb2d.velocity, ForceMode2D.Impulse);
                }
                else
                {
                    //Vector2 forceDir = (input * groundedMoveSpeed - rb2d.velocity).normalized;
                    rb2d.velocity = Vector2.zero;
                    rb2d.AddForce(rb2d.mass * input * groundedMoveSpeed, ForceMode2D.Impulse);
                }
                //attached.rb2d.AddForceAtPosition(-forceDir * ungroundedMoveForce, transform.position);
                //transform.position += (Vector3)input * groundedSpeed * Time.deltaTime;
            }
            else {
                rb2d.AddForce(input * ungroundedMoveForce);
            }
        }

        void CheckGrounded()
        {
            List<Collider2D> overlapping = new List<Collider2D>();
            col.OverlapCollider(tileContactFilter, overlapping);
            foreach (Collider2D other in overlapping) {
                if (other.isTrigger && other.TryGetComponent(out Tile tile)) {
                    attached = tile.ship;
                    //transform.SetParent(attached.transform);
                    //rb2d.isKinematic = true;
                    return;
                }
            }
            //transform.SetParent(null);
            //rb2d.isKinematic = false;
            attached = null;
        }
    }
}