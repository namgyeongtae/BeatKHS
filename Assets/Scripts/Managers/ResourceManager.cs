using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            /* GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T; */
        }

        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Transform parent = null, bool usePool = false, int poolCount = 5)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        if (usePool)
        {
            string name = original.name;
            if (!Managers.Pool.PoolDictionary.ContainsKey(name))
                Managers.Pool.CreatePool(name, original, poolCount);
            
            return Managers.Pool.Pop(name);
        }

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    public GameObject Instantiate(GameObject prefab, Transform parent = null, bool usePool = false, int poolCount = 5)
    {
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {prefab}");
            return null;
        }

        GameObject go = null;

        if (usePool)
        {
            string name = prefab.name;
            if (!Managers.Pool.PoolDictionary.ContainsKey(name))
                Managers.Pool.CreatePool(name, prefab, poolCount);
            
            go = Managers.Pool.Pop(name);
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
            go.name = prefab.name;

            return go;
        }

        go = Object.Instantiate(prefab, parent);
        go.transform.localScale = Vector3.one;
        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go, bool usePool = false)
    {
        if (go == null)
            return;

        if (usePool)
        {
            Managers.Pool.Push(go.name, go);
            return;
        }

        Object.Destroy(go);
    }

    public IEnumerator Destroy(GameObject go, float delay, bool usePool = false)
    {
        if (go == null)
            yield break;

        yield return new WaitForSeconds(delay);

        if (usePool)
        {
            Managers.Pool.Push(go.name, go);
            yield break;
        }

        Object.Destroy(go);
    }
}
