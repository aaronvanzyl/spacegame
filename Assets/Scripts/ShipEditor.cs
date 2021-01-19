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
        int selectedTile = -1;
        public Tile[] tilePrefabs;
        public SpriteRenderer ghost;
        public Material ghostInvalidMaterial;
        public Material ghostValidMaterial;
        public SelectionGroup tileSelectionGroup;



        // Start is called before the first frame update
        void Start()
        {
            SelectTile(0);
            foreach (Tile tile in tilePrefabs)
            {
                tileSelectionGroup.AddSelectOption(tile.GetComponent<SpriteRenderer>().sprite);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (tileSelectionGroup.currentlySelected != selectedTile)
            {
                SelectTile(tileSelectionGroup.currentlySelected);
            }
            if (selectedTile == -1 || EventSystem.current.IsPointerOverGameObject())
            {
                ghost.gameObject.SetActive(false);
            }
            else
            {
                ghost.gameObject.SetActive(true);
                Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                bool tileEmpty = true;
                bool tileSupported = false;
                Ship attachedShip = null;
                Vector2Int tilePos = Vector2Int.zero;
                foreach (Ship ship in GameManager.Instance.ships)
                {
                    tilePos = ship.GetTileFromWorldPos(cursorPos);
                    if (ship.GetTile(tilePos) == null)
                    {
                        if (ship.SolidTile(tilePos - Vector2Int.left)
                            || ship.SolidTile(tilePos - Vector2Int.up)
                            || ship.SolidTile(tilePos - Vector2Int.right)
                            || ship.SolidTile(tilePos - Vector2Int.down))
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


                Debug.Log(tileEmpty + " " + tileSupported);
                if (!tileSupported)
                {
                    ghost.transform.position = cursorPos;
                    ghost.transform.up = Vector2.up;
                }
                else
                {
                    ghost.transform.position = attachedShip.transform.TransformPoint((Vector2)tilePos);
                    ghost.transform.up = attachedShip.transform.up;

                    if (Physics2D.OverlapBox(ghost.transform.position, Vector2.one * 0.95f, ghost.transform.eulerAngles.z))
                    {
                        tileEmpty = false;
                    }
                    if (tileEmpty && Input.GetButtonDown("Fire1"))
                    {
                        attachedShip.SetTile(tilePos, tilePrefabs[selectedTile].name);
                    }
                }

                ghost.material = (tileSupported && tileEmpty) ? ghostValidMaterial : ghostInvalidMaterial;
            }
        }

        void SelectTile(int tileIndex)
        {
            selectedTile = tileIndex;
            if (selectedTile != -1)
            {
                ghost.sprite = tilePrefabs[selectedTile].GetComponent<SpriteRenderer>().sprite;
            }
        }
    }
}