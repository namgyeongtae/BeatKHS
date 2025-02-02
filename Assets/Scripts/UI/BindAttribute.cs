using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Bind : Attribute
{
    public string ObjectName = string.Empty;

    public Bind(string InObjectName)
    {
        ObjectName = InObjectName;
    }
}

public partial class BindAttribute
{
    private class BindInfo
    {
        public readonly FieldInfo FieldInfo;
        public readonly Bind Attribute;

        public BindInfo(FieldInfo InFieldInfo, Bind InAttribute)
        {
            FieldInfo = InFieldInfo;
            Attribute = InAttribute;
        }
    }

    private static BindInfo[] container = null;

    public static void InstallBindings(MonoBehaviour target)
    {
        container = target.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.IsDefined(typeof(Bind), false))
                    .Select(p => new BindInfo(p, GetBindAttributes(p)))
                    .ToArray();

        if (container == null)
            return;

        Dictionary<string, Transform> cachedChildTransform = new Dictionary<string, Transform>();

        target.transform.CachedChildTransform(cachedChildTransform);

        foreach (var item in container)
        {
            if (cachedChildTransform.TryGetValue(item.Attribute.ObjectName, out var outTransform) == true)
            {
                if (item.FieldInfo.FieldType == typeof(GameObject))
                {
                    item.FieldInfo.SetValue(target, outTransform.gameObject);
                }
                else
                {
                    if (item.FieldInfo.FieldType.IsArray)
                    {
                        Type type = item.FieldInfo.FieldType.GetElementType();
                        Component[] components = outTransform.GetComponentsInChildren(type, true);
                        Array filledArray = Array.CreateInstance(type, components.Length);
                        Array.Copy(components, filledArray, components.Length);
                        item.FieldInfo.SetValue(target, filledArray);
                    }
                    else
                    {
                        var component = outTransform.GetComponent(item.FieldInfo.FieldType);
                        if (component == null)
                        {
                            continue;
                        }
                        item.FieldInfo.SetValue(target, component);
                    }
                }
                if (outTransform.TryGetComponent<UIBindBase>(out var bindBase))
                {
                    // BindBase 오브젝트 하위에 있는 또 다른 BindBase 먼저 Binding해줘!
                    bindBase.InstallBindings();
                }
            }
        }
        cachedChildTransform.Clear();
    }

    private static Bind GetBindAttributes(FieldInfo fieldInfo)
    {
        return fieldInfo.GetCustomAttributes(typeof(Bind), false).FirstOrDefault() as Bind;
    }
}
