﻿using System.Linq;
using UnityEngine;
using EventController;
using System;
using UnityEngine.SceneManagement;

public class AssetsManager : Singleton<AssetsManager>
{
    #region 移动游戏对象到场景
    public void MoveGameObjectToScene(GameObject go, string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            SceneManager.MoveGameObjectToScene(go, scene);
        }
    }
    #endregion

    #region 加载场景
    public void LoadSceneAsync(string path, Action<AsyncOperation> cb = null, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        string name = path.Split('/').Last();
        if (!string.IsNullOrEmpty(name))
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(name, mode);
            cb?.Invoke(asyncOperation);
            asyncOperation.completed += (ao) =>
            {
                Scene scene = SceneManager.GetSceneByName(name);
                SceneManager.SetActiveScene(scene);
                cb?.Invoke(asyncOperation);
            };
        }
    }
    public void UnLoadSceneAsync(string path, Action<float> cb = null)
    {
        string name = path.Split('/').Last();
        if (!string.IsNullOrEmpty(name))
        {
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(name);
            asyncOperation.completed += (ao) =>
            {
                cb?.Invoke(asyncOperation.progress);
            };
        }
    }
    #endregion

    #region 加载窗口
    /// <summary> 
    /// 加载窗口 添加T(Component)类型脚本
    /// path = sceneName/folderName/assetName
    /// scnenName 打包资源的第一层文件夹名称
    /// folderName 打包资源的第二层文件夹名称
    /// assetName 资源名称
    /// state 初始化窗口是否显示
    /// </summary>
    public T LoadWindow<T>(string path, bool state = false) where T : Component
    {
        GameObject go = LoadAsset<GameObject>(path);
        if (go != null)
        {
            go = UnityEngine.Object.Instantiate(go, UIRoot.Instance.GetRectTransform());
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = go.name.Replace("(Clone)", "");
            T t = go.AddComponent<T>();
            EventDispatcher.TriggerEvent(go.name, state);
            return t;
        }
        return null;
    }
    /// <summary> 
    /// 加载窗口 添加T(Component)类型脚本
    /// 加载本地资源，即Resources文件夹下资源
    /// state 初始化窗口是否显示
    /// </summary>
    public T LoadLocalWindow<T>(string path, bool state = false) where T : Component
    {
        GameObject go = LoadLocalAsset<GameObject>(path);
        if (go != null)
        {
            go = UnityEngine.Object.Instantiate(go, UIRoot.Instance.GetRectTransform());
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = go.name.Replace("(Clone)", "");
            T t = go.AddComponent<T>();
            EventDispatcher.TriggerEvent(go.name, state);
            return t;
        }
        return null;
    }
    #endregion

    #region 加载资源
    /// <summary> 
    /// 加载ab包中的资源，返回T
    /// path = sceneName/folderName/assetName
    /// scnenName 打包资源的第一层文件夹名称
    /// folderName 打包资源的第二层文件夹名称
    /// assetName 资源名称
    /// </summary>
    public T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        string[] names = path.Split('/');
        return AssetBundleManager.Instance.LoadAsset<T>(names[0], names[1], names[names.Length - 1]);
    }
    /// <summary> 
    /// 加载本地资源，返回T，即Resources文件夹下资源
    /// </summary>
    public T LoadLocalAsset<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }
    #endregion
}