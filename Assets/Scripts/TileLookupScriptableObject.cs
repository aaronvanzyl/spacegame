using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace SpaceGame {
    [CreateAssetMenu(fileName = "TileLookup", menuName = "ScriptableObjects/TileLookupScriptableObject", order = 1)]
    public class TileLookupScriptableObject : ScriptableObject
    {
        public List<Tile> tilePrefabs;
    }
}
