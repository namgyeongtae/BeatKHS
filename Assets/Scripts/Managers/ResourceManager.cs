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

    public GameObject Instantiate(string path, Transform parent = null, int poolCount = 5)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        /* if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original, parent, poolCount).gameObject; */

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    public GameObject Instantiate(GameObject prefab, int poolCount = 5, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {prefab}");
            return null;
        }

        /* if (prefab.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(prefab, parent, poolCount).gameObject; */

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        /* Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        } */

        Object.Destroy(go);
    }

    public IEnumerator Destroy(GameObject go, float delay)
    {
        if (go == null)
            yield break;

        /* Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            yield return new WaitForSeconds(delay);
            Managers.Pool.Push(poolable);
            yield break;
        } */

        Object.Destroy(go, delay);
    }
}
