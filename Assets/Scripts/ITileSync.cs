using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public interface ITileSync
    {
        Tile Parent
        {
            get;
            set;
        }
        bool NeedSync
        {
            get;
            set;
        }
        void Serialize(PhotonStream stream);

        void Deserialize(PhotonStream stream);
    }
}