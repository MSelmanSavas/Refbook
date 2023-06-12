using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SingletonMonoBehaviour;
using System;

public class RefBook : SingletonMonoBehaviour<RefBook>
{
    protected override bool dontDestroyOnLoad => true;

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ShowInInspector]
#endif
    Dictionary<System.Type, List<object>> References = new Dictionary<System.Type, List<object>>();

    System.Type _lastAddedType;
    System.Type _lastAddTryType;
    System.Type _lastAccessedType;
    System.Type _lastRemovedType;

    public static bool AddWithInterfaces(object obj)
    {
        bool isAllReferencesAdded = true;

        try
        {
            AddOnlyInterfaces(obj);
            Instance.AddInternal(obj);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while adding {obj} as type {Instance._lastAddTryType}. Error : {e}");
            isAllReferencesAdded = false;
        }

        return isAllReferencesAdded;
    }

    public static bool AddOnlyInterfaces(object obj)
    {
        bool isAllReferencesAdded = true;

        try
        {
            Type[] interfaces = obj.GetType().GetInterfaces();

            foreach (var interfaceType in interfaces)
            {
                Instance._lastAddTryType = interfaceType;
                Instance.AddInternal(interfaceType, obj);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while adding {obj} as type {Instance._lastAddTryType}. Error : {e}");
            isAllReferencesAdded = false;
        }

        return isAllReferencesAdded;
    }

    public static bool Add(object obj) => Instance.AddInternal(obj);
    protected virtual bool AddInternal(object obj) => AddInternal(obj.GetType(), obj);

    protected virtual bool AddInternal(Type type, object obj)
    {
        _lastAddedType = type;

        if (!References.ContainsKey(_lastAddedType))
            References.Add(_lastAddedType, new List<object>());

        if (References[_lastAddedType].Contains(obj))
        {
            Debug.LogError($"References already contains obj of type : {_lastAddedType},{obj}! Can't add...");
            return false;
        }

        References[_lastAddedType].Add(obj);
        return true;
    }

    /// <summary>
    /// Tries to get obj with type T in access index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static bool TryGet<T>(out T obj, int accessIndex = 0) where T : class => Instance.TryGetInternal(out obj, accessIndex);

    /// <summary>
    /// Tries to returns a registered Reference of type. Return first of registered type if accessIndex is not specified.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    protected virtual bool TryGetInternal<T>(out T obj, int accessIndex = 0) where T : class
    {
        _lastAccessedType = typeof(T);

        try
        {
            if (References.ContainsKey(_lastAccessedType))
                if (References[_lastAccessedType].Count > accessIndex)
                {
                    obj = References[_lastAccessedType][accessIndex] as T;
                    return true;
                }

            obj = default;
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error while trying to get a Reference of type : {_lastAccessedType}! Error : {e}");
            obj = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get object with given T type in given access index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static bool TryGet<T>(out T obj, System.Type type, int accessIndex = 0) where T : class => Instance.TryGetInternal(out obj, type, accessIndex);
    protected virtual bool TryGetInternal<T>(out T obj, System.Type type, int accessIndex = 0) where T : class
    {
        _lastAccessedType = type;

        try
        {
            if (References.ContainsKey(type))
                if (References[type].Count > accessIndex)
                {
                    obj = References[type][accessIndex] as T;
                    return true;
                }

            obj = default;
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error while trying to get a Reference of type : {_lastAccessedType}! Error : {e}");
            obj = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get obj with given type in given access index
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="type"></param>
    /// <param name="accessIndex"></param>
    /// <returns></returns>
    public static bool TryGet(out object obj, System.Type type, int accessIndex = 0) => Instance.TryGetInternal(out obj, type, accessIndex);
    protected virtual bool TryGetInternal(out object obj, System.Type type, int accessIndex = 0)
    {
        _lastAccessedType = type;

        try
        {
            if (References.ContainsKey(type))
                if (References[type].Count > accessIndex)
                {
                    obj = References[type][accessIndex];
                    return true;
                }

            obj = default;
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error while trying to get a Reference of type : {_lastAccessedType}! Error : {e}");
            obj = default;
            return false;
        }
    }

    public static bool TryGetAll<T>(List<T> objs) => Instance.TryGetAllInternal(objs);
    public static List<T> TryGetAll<T>()
    {
        List<T> allReferences = new List<T>();
        if (Instance.TryGetAllInternal(allReferences))
            return allReferences;

        return Enumerable.Empty<T>() as List<T>;
    }

    protected virtual bool TryGetAllInternal<T>(List<T> objs)
    {
        _lastAccessedType = typeof(T);

        try
        {
            objs.AddRange(References[_lastAccessedType].Cast<T>());
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error while trying to get a Reference of type : {_lastAccessedType}! Error : {e}");
            objs = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get all of the objects with the specified type to given reference list
    /// </summary>
    /// <param name="type"></param>
    /// <param name="objs"></param>
    /// <returns></returns>
    public static bool TryGetAll(System.Type type, List<object> objs) => Instance.TryGetAllInternal(type, objs);
    public static List<object> TryGetAll(System.Type type)
    {
        List<object> allReferences = new List<object>();

        if (Instance.TryGetAllInternal(type, allReferences))
            return allReferences;

        return Enumerable.Empty<object>() as List<object>;
    }

    protected virtual bool TryGetAllInternal(System.Type type, List<object> objs)
    {
        _lastAccessedType = type;

        try
        {
            objs = References[_lastAccessedType];
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error while trying to get a Reference of type : {_lastAccessedType}! Error : {e}");
            objs = null;
            return false;
        }
    }

    /// <summary>
    /// Remove given object with its interfaces
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool RemoveWithInterfaces(object obj)
    {
        bool isAllReferencesAdded = true;

        try
        {
            RemoveOnlyInterfaces(obj);
            Instance.RemoveInternal(obj);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while adding {obj} as type {Instance._lastAddTryType}. Error : {e}");
            isAllReferencesAdded = false;
        }

        return isAllReferencesAdded;
    }

    /// <summary>
    /// Removes only the interfaces of the given obj
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool RemoveOnlyInterfaces(object obj)
    {
        bool isAllReferencesRemoved = true;

        try
        {
            Type[] interfaces = obj.GetType().GetInterfaces();

            foreach (var interfaceType in interfaces)
            {
                Instance._lastAddTryType = interfaceType;
                Instance.RemoveInternal(interfaceType, obj);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while removing {obj} as type {Instance._lastAddTryType}. Error : {e}");
            isAllReferencesRemoved = false;
        }

        return isAllReferencesRemoved;
    }

    /// <summary>
    /// Removes given object with its type
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Remove(object obj) => Instance.RemoveInternal(obj);
    protected bool RemoveInternal(object obj) => RemoveInternal(obj.GetType(), obj);
    protected bool RemoveInternal(Type type, object obj)
    {
        _lastRemovedType = type;

        if (!References.ContainsKey(_lastRemovedType))
        {
            Debug.LogError($"There is no reference of type : {_lastRemovedType}");
            return false;
        }

        if (!References[_lastRemovedType].Contains(obj))
        {
            Debug.LogError($"There is no obj found in references of type : {_lastRemovedType},{obj}");
            return false;
        }

        References[_lastRemovedType].Remove(obj);
        return true;
    }

    /// <summary>
    /// Removes obj of a given type in given index
    /// </summary>
    /// <param name="type"></param>
    /// <param name="Index"></param>
    /// <returns></returns>
    public static bool RemoveAtIndex(System.Type type, int Index = 0) => Instance.RemoveAtIndexInternal(type, Index);
    protected bool RemoveAtIndexInternal(System.Type type, int Index = 0)
    {
        _lastRemovedType = type;

        if (!References.ContainsKey(_lastRemovedType))
        {
            Debug.LogError($"There is no reference of type : {_lastRemovedType}");
            return false;
        }

        if (References[_lastRemovedType].Count <= Index)
        {
            Debug.LogError($"There is no Index : {Index} in References of type :{_lastRemovedType}");
            return false;
        }

        References[_lastRemovedType].RemoveAt(Index);
        return true;
    }
}
