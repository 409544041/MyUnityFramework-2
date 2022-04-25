using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoRoot : SingletonMono<GoRoot>
{
    private Dictionary<string, Transform> rootPairs = new Dictionary<string, Transform>();

    /// <summary> ��ȡ3D��Ϸ��������� </summary>
    public Transform GoTransform { get { return transform; } private set { } }
    // Start is called before the first frame update
    void Awake()
    {

    }

    /// <summary> ���3D����Ԥ���壬����GameObject</summary>
    public GameObject AddChild(GameObject prefab, Transform parent = null)
    {
        Transform objParent = parent ? parent : GoTransform;
        GameObject go = Instantiate(prefab, objParent);
        return go;
    }

    /// <summary> ���3D����Ԥ���壬����GameObject </summary>
    public Transform TryGetEmptyNode(string name)
    {
        if (!rootPairs.ContainsKey(name)) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            rootPairs.Add(name, go.transform);
            return go.transform;
        } else {
            return rootPairs[name];
        }
    }

}
