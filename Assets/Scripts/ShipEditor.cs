using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceGame
{
    public class ShipEditor : MonoBehaviour
    {
        int selectedTileIndex = -1;
        Tile selectedTile;
        bool selectedTileIsSolid;
        public Tile[] tilePrefabs;
        public SpriteRenderer ghost;
        public Color ghostInvalidColor;
        public Color ghostValidColor;
        public Color ghostOutOfRangeColor;
        public SelectionGroup tileSelectionGroup;
        public float maxPlacementDist;
        SpacePlayer player;
        float rotation = 0;
        bool hasRotated;

        void Start()
        {
            player = GameManager.Instance.localPlayer;
            SelectTile(0);
            foreach (Tile tile in tilePrefabs)
            {
                tileSelectionGroup.AddSelectOption(tile.GetComponent<SpriteRenderer>().sprite);
            }
        }

        void Update()
        {
            if (player.occupying != null)
            {
                ghost.gameObject.SetActive(false);
                return;
            }

            if (tileSelectionGroup.currentlySelected != selectedTileIndex)
            {
                SelectTile(tileSelectionGroup.currentlySelected);
            }
            if (selectedTileIndex == -1 || EventSystem.current.IsPointerOverGameObject())
            {
                ghost.gameObject.SetActive(false);
            }
            else
            {
                if (!hasRotated && Mathf.Abs(Input.GetAxis("Rotate")) > 0)
                {
                    hasRotated = true;
                    rotation = (rotation + 90 * Mathf.Sign(Input.GetAxis("Rotate"))) % 360;
                }
                else if (Input.GetAxis("Rotate") == 0)
                {
                    hasRotated = false;
                }

                ghost.gameObject.SetActive(true);
                Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                bool tileEmpty = true;
                bool tileSupported = false;
                bool inRange = false;
                Ship attachedShip = null;
                Vector2Int tilePos = Vector2Int.zero;
                foreach (Ship ship in GameManager.Instance.ships)
                {
                    tilePos = ship.WorldToTilePos(cursorPos);
                    if (ship.GetTile(tilePos) == null)
                    {
                        if (ship.TileExists(tilePos - Vector2Int.left)
                            || ship.TileExists(tilePos - Vector2Int.up)
                            || ship.TileExists(tilePos - Vector2Int.right)
                            || ship.TileExists(tilePos - Vector2Int.down))
                        {
                            attachedShip = ship;
                            tileSupported = true;
                            break;
                        }
                    }
                    else
                    {
                        tileEmpty = false;
                        break;
                    }
                }

                if (!tileSupported)
                {
                    ghost.transform.position = cursorPos;
                    //ghost.transform.up = GameManager.Instance.localPlayer.transform.up;
                    ghost.transform.up = Vector2.up;
                    if (selectedTile.canRotate)
                    {
                        ghost.transform.eulerAngles += new Vector3(0, 0, rotation);
                    }
                    inRange = Vector2.Distance(player.transform.position, ghost.transform.position) <= maxPlacementDist;
                }
                else
                {
                    ghost.transform.position = attachedShip.transform.TransformPoint((Vector2)tilePos);
                    ghost.transform.up = attachedShip.transform.up;
                    if (selectedTile.canRotate)
                    {
                        ghost.transform.eulerAngles += new Vector3(0, 0, rotation);
                    }
                    inRange = Vector2.Distance(player.transform.position, ghost.transform.position) <= maxPlacementDist;
                    Collider2D[] overlapping = Physics2D.OverlapBoxAll(ghost.transform.position, Vector2.one * 0.95f, ghost.transform.eulerAngles.z);
                    foreach (Collider2D col in overlapping) {
                        if (col.TryGetComponent<Tile>(out _) || (selectedTileIsSolid && !col.isTrigger)) {
                            tileEmpty = false;
                            break;
                        }
                    }
                    if (tileEmpty && inRange && Input.GetButtonDown("Fire1"))
                    {
                        attachedShip.SetTile(tilePos, selectedTile.canRotate ? rotation : 0, selectedTile.name);
                    }
                }

                ghost.material.color = (tileSupported && tileEmpty && inRange) ? ghostValidColor : ghostInvalidColor;
                if (!inRange)
                {
                    ghost.material.color *= ghostOutOfRangeColor;
                }
            }
        }

        void SelectTile(int tileIndex)
        {
            selectedTileIndex = tileIndex;
            if (selectedTileIndex != -1)
            {
                selectedTile = tilePrefabs[selectedTileIndex];
                selectedTileIsSolid = !selectedTile.GetComponent<Collider2D>().isTrigger;
                ghost.sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
            }
        }
    }
}