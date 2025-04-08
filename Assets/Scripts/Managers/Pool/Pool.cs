using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public string Tag;
    public GameObject Prefab;
    public int PoolCount;

    public void Initialize(string tag, GameObject prefab, int poolCount)
    {
        Tag = tag;
        Prefab = prefab;
        PoolCount = poolCount;
    }
}
