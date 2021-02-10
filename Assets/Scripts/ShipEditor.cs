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
        public TileLookupScriptableObject tileLookup;
        public SpriteRenderer ghost;
        public Color ghostInvalidColor;
        public Color ghostValidColor;
        public Color ghostOutOfRangeColor;
        public SelectionGroup tileSelectionGroup;
        public float maxPlacementDist;
        float rotation = 0;
        bool hasRotated;

        void Start()
        {
            SelectTile(0);
            foreach (Tile tile in tileLookup.tilePrefabs)
            {
                tileSelectionGroup.AddSelectOption(tile.GetComponent<SpriteRenderer>().sprite);
            }
        }

        void Update()
        {

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
                }
                else
                {
                    ghost.transform.position = attachedShip.transform.TransformPoint((Vector2)tilePos);
                    ghost.transform.up = attachedShip.transform.up;
                    if (selectedTile.canRotate)
                    {
                        ghost.transform.eulerAngles += new Vector3(0, 0, rotation);
                    }
                    Collider2D[] overlapping = Physics2D.OverlapBoxAll(ghost.transform.position, Vector2.one * 0.95f, ghost.transform.eulerAngles.z);
                    foreach (Collider2D col in overlapping) {
                        if (col.TryGetComponent<Tile>(out _) || (selectedTileIsSolid && !col.isTrigger)) {
                            tileEmpty = false;
                            break;
                        }
                    }
                    if (tileEmpty && attachedShip == GameManager.Instance.localShip && Input.GetButtonDown("Fire1"))
                    {
                        attachedShip.SetTileNetwork(tilePos, selectedTile.canRotate ? rotation : 0, selectedTileIndex);
                    }
                }

                ghost.material.color = (tileSupported && tileEmpty && attachedShip == GameManager.Instance.localShip) ? ghostValidColor : ghostInvalidColor;
            }
        }

        void SelectTile(int tileIndex)
        {
            selectedTileIndex = tileIndex;
            if (selectedTileIndex != -1)
            {
                selectedTile = tileLookup.tilePrefabs[selectedTileIndex];
                selectedTileIsSolid = !selectedTile.GetComponent<Collider2D>().isTrigger;
                ghost.sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
            }
        }
    }
}