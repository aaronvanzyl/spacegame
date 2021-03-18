using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomSerialize : MonoBehaviour
{
    static bool initialized;
    void Awake()
    {
        if (initialized) {
            return;
        }
        initialized = true;
        byte code = 0;
        PhotonPeer.RegisterType(typeof(Vector2Int), code, SerializeVector2Int, DeserializeVector2Int);
        code += 1;
        PhotonPeer.RegisterType(typeof(Team), code, Team.SerializeTeam, Team.DeserializeTeam);
        code += 1;
    }

    public static object DeserializeVector2Int(byte[] data)
    {
        Vector2Int result = new Vector2Int();
        MemoryStream stream = new MemoryStream(data);
        int pos = 0;
        result.x = BitConverter.ToInt32(data, pos);
        pos += 4;
        result.y = BitConverter.ToInt32(data, pos);
        pos += 4;
        return result;
    }

    public static byte[] SerializeVector2Int(object customType)
    {
        Vector2Int vec = (Vector2Int)customType;
        MemoryStream stream = new MemoryStream();
        byte[] x = BitConverter.GetBytes(vec.x);
        stream.Write(x, 0, x.Length);
        byte[] y = BitConverter.GetBytes(vec.y);
        stream.Write(y, 0, y.Length);
        return stream.ToArray();
    }
}
