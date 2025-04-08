using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Manager
{
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new();
    private Dictionary<string, Pool> _poolSettings = new();

    public Dictionary<string, Queue<GameObject>> PoolDictionary => _poolDictionary;
    public Dictionary<string, Pool> PoolSettings => _poolSettings;

    public override void Init()
    {
        
    }

    public override void Clear()
    {
        _poolDictionary.Clear();
        _poolSettings.Clear();
    }

    public void CreatePool(string tag, GameObject prefab, int poolCount)
    {
        if (!_poolSettings.ContainsKey(tag))
        {
            GameObject poolObject = new GameObject($"Root_{tag}");
            poolObject.AddComponent<Pool>();
            poolObject.GetComponent<Pool>().Initialize(tag, prefab, poolCount);

            _poolSettings.Add(tag, poolObject.GetComponent<Pool>());
            
            _poolDictionary.Add(tag, new Queue<GameObject>());

            for (int i = 0; i < poolCount; i++)
            {
                GameObject obj = Object.Instantiate(prefab, poolObject.transform);
                obj.SetActive(false);
                _poolDictionary[tag].Enqueue(obj);
            }
        }
    }

    public GameObject Pop(string tag)
    {
        if (!_poolDictionary.ContainsKey(tag))
            return null;

        Queue<GameObject> objectPool = _poolDictionary[tag];
        
        if (objectPool.Count == 0)
        {
            Pool settings = _poolSettings[tag].GetComponent<Pool>();
            GameObject obj = Object.Instantiate(settings.Prefab);
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        GameObject pooledObject = objectPool.Dequeue();
        pooledObject.SetActive(true);
        pooledObject.transform.localScale = Vector3.one;
        return pooledObject;
    }

    public void Push(string tag, GameObject obj)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.Log($"Pool with tag {tag} not found");
            return;
        }

        obj.SetActive(false);
        Debug.Log($"Pushing object to pool with tag {tag} in {_poolSettings[tag].transform.name}");
        obj.transform.SetParent(_poolSettings[tag].transform);
        _poolDictionary[tag].Enqueue(obj);
    }
}
