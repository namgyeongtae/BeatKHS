using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensionMethods
{
    public static Transform FindChildRecursively(this Transform tr, string childName)
    {
        foreach (Transform child in tr)
        {
            if (child.name == childName)
                return child;
            
            Transform finding = FindChildRecursively(child, childName);

            if (finding != null)
                return finding;
        }

        return null;
    }

    public static void CachedChildTransform(this Transform tr,  Dictionary<string, Transform> childTransform)
    {
        foreach (Transform child in tr)
        {
            if (childTransform.ContainsKey(child.name) == false)
                childTransform.Add(child.name, child);

            if (child.childCount > 0)
                CachedChildTransform(child, childTransform);
        }
    }
}

