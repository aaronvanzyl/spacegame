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
        [HideInInspector]
        public Ship selectedShip;
        int selectedTileIndex = -1;
        Tile selectedTile;
        bool selectedTileIsSolid;
        public TileLookupScriptableObject tileLookup;
        public SpriteRenderer ghost;
        public Sprite deleteSprite;
        public Color deleteColor;
        Sprite selectedTileSprite;
        public Color ghostInvalidColor;
        public Color ghostValidColor;
        public Color ghostOutOfRangeColor;
        public SelectionGroup tileSelectionGroup;
        public float maxPlacementDist;
        float rotation = 0;
        bool hasRotated;

        void Start()
        {
            foreach (Tile tile in tileLookup.tilePrefabs)
            {
                tileSelectionGroup.AddSelectOption(tile.GetComponent<SpriteRenderer>().sprite);
            }
            tileSelectionGroup.OnToggle(0);
            ToggleOn(false);
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
                // Need to rewrite to use input.getbuttondown
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
                Tile underCursor = null;
                Vector2Int tilePos = Vector2Int.zero;

                // Determine if tile is support and empty
                foreach (Ship ship in GameManager.Instance.ships)
                {
                    tilePos = ship.WorldToTilePos(cursorPos);
                    if (ship.TryGetTile(tilePos, out Tile tile))
                    {
                        attachedShip = ship;
                        underCursor = tile;
                        tileEmpty = false;
                        break;
                    }
                    else
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
                }

                // Set position of ghost and check for partial overlap
                if (!tileSupported && underCursor == null)
                {
                    ghost.transform.position = cursorPos;
                    ghost.transform.up = Vector2.up;
                    if (selectedTile.canRotate)
                    {
                        ghost.transform.eulerAngles += new Vector3(0, 0, rotation);
                    }
                }
                else if (underCursor != null) {
                    ghost.transform.position = underCursor.transform.position;
                    ghost.transform.up = underCursor.transform.up;
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
                    foreach (Collider2D col in overlapping)
                    {
                        if (col.TryGetComponent<Tile>(out _) || (selectedTileIsSolid && !col.isTrigger))
                        {
                            tileEmpty = false;
                            break;
                        }
                    }
                    
                }

                // Set color and sprite of ghost
                if (underCursor != null)
                {
                    ghost.sprite = deleteSprite;
                    ghost.material.color = deleteColor;
                    
                }
                else
                {
                    ghost.sprite = selectedTileSprite;
                    ghost.material.color = (tileSupported && tileEmpty && attachedShip == selectedShip) ? ghostValidColor : ghostInvalidColor;
                }

                // Check for create/delete
                if (tileEmpty && tileSupported && attachedShip == selectedShip && Input.GetButtonDown("Fire1"))
                {
                    attachedShip.SetTileNetwork(tilePos, selectedTile.canRotate ? rotation : 0, selectedTileIndex);
                }
                else if (underCursor != null && attachedShip == selectedShip && Input.GetButtonDown("Fire2"))
                {
                    attachedShip.DestroyTileNetwork(tilePos, true);
                }
            }
        }

        void SelectTile(int tileIndex)
        {
            selectedTileIndex = tileIndex;
            if (selectedTileIndex != -1)
            {
                selectedTile = tileLookup.tilePrefabs[selectedTileIndex];
                selectedTileIsSolid = !selectedTile.GetComponent<Collider2D>().isTrigger;
                ghost.sprite = selectedTileSprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
            }
        }

        public void ToggleOn(bool isOn)
        {
            gameObject.SetActive(isOn);
            ghost.gameObject.SetActive(isOn);
            tileSelectionGroup.gameObject.SetActive(isOn);
        }
    }
}