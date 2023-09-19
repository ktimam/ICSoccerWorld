using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    public float[] h;
    public string id;
    public float[] p;
}

[System.Serializable]
public class ScreenSnap
{
    public PlayerInfo[] scrn;
}

[System.Serializable]
public class ScreenSnaps
{
    public ScreenSnap[] scrnsnaps;

    public static ScreenSnaps CreateFromJSON(string jsonString)
    {
        return JsonConvert.DeserializeObject<ScreenSnaps>(jsonString);
    }
}

[System.Serializable]
public class JSONInfo
{
    public int score1;
    public int score2;
    public string snapshot;

    public static JSONInfo CreateFromJSON(string jsonString)
    {
        return JsonConvert.DeserializeObject<JSONInfo>(jsonString);
    }
}

