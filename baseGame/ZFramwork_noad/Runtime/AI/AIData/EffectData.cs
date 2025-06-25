using System.Collections;
using UnityEngine;

public class EffectData
{
    public Vector3 pos;
    public string path;
    public GameObject effectObj;
    public bool isForce;

    public EffectData(Vector3 pos,string path,bool force = true)
    {
        this.isForce = force;
        this.pos = pos;
        this.path = "__effect/fllow_agent/" + path;
    }
    

    public bool HaveEffect()
    {
        return !string.IsNullOrEmpty(path);
    }
}