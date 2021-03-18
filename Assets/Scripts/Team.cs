using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Team
{
    public int id;
    public Color color;

    public Team(int id) {
        this.id = id;
        this.color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), 0.6f, 1);
    }

    public static object DeserializeTeam(byte[] data)
    {
        Team result = new Team();
        MemoryStream stream = new MemoryStream(data);
        int pos = 0;
        result.id = BitConverter.ToInt32(data, pos);
        pos += 4;
        Color color = Color.white;
        color.r = BitConverter.ToSingle(data, pos);
        pos += 4;
        color.g = BitConverter.ToSingle(data, pos);
        pos += 4;
        color.b = BitConverter.ToSingle(data, pos);
        pos += 4;
        result.color = color;
        return result;
    }

    public static byte[] SerializeTeam(object customType)
    {
        Team team = (Team)customType;
        MemoryStream stream = new MemoryStream();
        byte[] id = BitConverter.GetBytes(team.id);
        stream.Write(id, 0, id.Length);
        byte[] r = BitConverter.GetBytes(team.color.r);
        stream.Write(r, 0, r.Length);
        byte[] g = BitConverter.GetBytes(team.color.g);
        stream.Write(g, 0, g.Length);
        byte[] b = BitConverter.GetBytes(team.color.b);
        stream.Write(b, 0, b.Length);


        return stream.ToArray();
    }
}
