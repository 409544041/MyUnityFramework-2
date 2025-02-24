﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.Rendering;

public static class Developer
{
    #region Animator Event
    private static int DEVELOP_ANIMATOR_TIME_ID = -1;//动画时间任务id
    private static int DEVELOP_FRAMES_TIME_ID = -1;//动画时间任务id
    public static void PlayBack(this Animator _animator, string stateName, UnityAction callback = null)
    {
        _animator.Play(stateName, callback, -1);
    }
    public static void Play(this Animator _animator, string stateName, UnityAction callback = null)
    {
        _animator.Play(stateName, callback, 1);
    }
    public static void Play(this Animator _animator, string stateName, UnityAction callback = null, float speed = 1)
    {
        AnimationClip[] AnimationClips = _animator.runtimeAnimatorController.animationClips;
        float _time = 0;
        for (int i = 0; i < AnimationClips.Length; i++)
        {
            if (AnimationClips[i].name == stateName)
            {
                _time = AnimationClips[i].length;
            }
        }
        _animator.enabled = true;
        _animator.StartPlayback();
        _animator.speed = speed;
        _animator.Play(stateName, 0, speed < 0 ? 1 : 0);
        DEVELOP_ANIMATOR_TIME_ID = TimerManager.Instance.StartTimer((float time) =>
        {
            if (time >= _time)
            {
                callback?.Invoke();
                TimerManager.Instance.EndTimer(DEVELOP_ANIMATOR_TIME_ID);
            }
        });
    }

    public static void PlayFrames(this Image image, List<Sprite> sequenceFrames, float time = 0.05f, UnityAction callback = null, bool loop = false, bool isNativeSize = false)
    {
        if (image == null)
        {
            Debug.LogError("Image is null!!!");
            return;
        }
        image.PlayFrames(sequenceFrames.ToArray(), time, callback, loop, isNativeSize);
    }
    public static void PlayFrames(this Image image, Sprite[] sequenceFrames, float time = 0.05f, UnityAction callback = null, bool loop = false, bool isNativeSize = false)
    {
        if (image == null)
        {
            Debug.LogError("Image is null!!!");
            return;
        }
        int index = 0;//可以用来控制起始播放的动画帧索引
        float recordTime = 0;
        DEVELOP_FRAMES_TIME_ID = TimerManager.Instance.StartTimer((float currentTime) =>
        {
            if (currentTime - recordTime >= time)
            {
                recordTime = currentTime;
                //当我们需要在整个动画播放完之后  重复播放后面的部分 就可以展现我们纯代码播放的自由性
                if (index > sequenceFrames.Length - 1)
                {
                    callback?.Invoke();
                    if (loop)
                    {
                        index = 0;
                    }
                    else
                    {
                        TimerManager.Instance.EndTimer(DEVELOP_FRAMES_TIME_ID);
                    }
                }
                else
                {
                    image.sprite = sequenceFrames[index];
                    index++;
                    if (isNativeSize)
                    {
                        image.SetNativeSize();
                    }
                }
            }
        });
    }
    #endregion

    #region Enable Objects & Component
    /// <summary>
    /// 设置游戏对象显示隐藏
    /// </summary>
    /// <param name="go"></param>
    /// <param name="enable"></param>
    public static void SetGameObjectActive(this GameObject go, bool enable = true)
    {
        if (go == null) { Debug.LogError($"GameObject({go.name}) is null!!!"); return; }
        go.SetActive(enable);
    }
    /// <summary>
    /// 设置脚本所在游戏对象的显示隐藏
    /// </summary>
    /// <param name="com"></param>
    /// <param name="enable"></param>
    public static void SetGameObjectActive(this Component com, bool enable = true)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return; }
        com.gameObject.SetActive(enable);
    }
    /// <summary>
    /// 设置游戏对象上脚本的开启和关闭
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <param name="enable"></param>
    public static void SetComponentEnable<T>(this GameObject go, bool enable = true) where T : Behaviour
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return; }
        T t = go.GetComponent<T>();
        if (t != null)
        {
            t.enabled = enable;
        }
        else
        {
            Debug.LogError($"对象{go.name} --- 没有脚本:{typeof(T).Name}");
        }
    }
    /// <summary>
    /// 设置脚本所在游戏对象上的脚本的开启和关闭
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="com"></param>
    /// <param name="enable"></param>
    public static void SetComponentEnable<T>(this Component com, bool enable = true) where T : Behaviour
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return; }
        T t = com.gameObject.GetComponent<T>();
        if (t != null)
        {
            t.enabled = enable;
        }
        else
        {
            Debug.LogError("对象{com.gameObject.name} --- 没有脚本:{typeof(T).Name}");
        }
    }
    #endregion

    #region Try Get Component
    /// <summary>
    /// 获取GameObject上的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T TryGetComponent<T>(this GameObject go) where T : Component
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return null; }
        T t = go.GetComponent<T>();
        if (t == null)
        {
            t = go.AddComponent<T>();
        }
        return t;
    }
    /// <summary>
    /// 获取Component对象上的其他组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="com"></param>
    /// <returns></returns>
    public static T TryGetComponent<T>(this Component com) where T : Component
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return null; }
        T t = com.GetComponent<T>();
        if (t == null)
        {
            t = com.gameObject.AddComponent<T>();
        }
        return t;
    }
    #endregion

    #region OnClick params
    /// <summary>
    /// 多个参数的GameObject点击事件
    /// </summary>
    /// <param name="go"></param>
    /// <param name="cb"></param>
    public static void OnClick(this GameObject go, UnityAction cb)
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return; }
        EventListener.EventListener.Get(go).onClick = (obj) =>
        {
            cb?.Invoke();
        };
    }
    /// <summary>
    /// 多个参数的GameObject点击事件
    /// </summary>
    /// <param name="go"></param>
    /// <param name="cb"></param>
    /// <param name="arg"></param>
    public static void OnClick<T>(this GameObject go, UnityAction<T> cb, T arg)
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return; }
        EventListener.EventListener.Get(go).onClick = (obj) =>
        {
            cb?.Invoke(arg);
        };
    }
    /// <summary>
    /// 多个参数的GameObject点击事件
    /// </summary>
    /// <param name="go"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    public static void OnClick<T0,T1>(this GameObject go, UnityAction<T0,T1> cb, T0 arg0,T1 arg1)
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return; }
        EventListener.EventListener.Get(go).onClick = (obj) =>
        {
            cb?.Invoke(arg0,arg1);
        };
    }
    /// <summary>
    /// 多个参数的GameObject点击事件
    /// </summary>
    /// <param name="go"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    public static void OnClick<T0,T1,T2>(this GameObject go, UnityAction<T0,T1,T2> cb, T0 arg0,T1 arg1,T2 arg2)
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return; }
        EventListener.EventListener.Get(go).onClick = (obj) =>
        {
            cb?.Invoke(arg0,arg1,arg2);
        };
    }
    /// <summary>
    /// 多个参数的GameObject点击事件
    /// </summary>
    /// <param name="go"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    public static void OnClick<T0,T1,T2,T3>(this GameObject go, UnityAction<T0,T1,T2,T3> cb, T0 arg0,T1 arg1,T2 arg2,T3 arg3)
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return; }
        EventListener.EventListener.Get(go).onClick = (obj) =>
        {
            cb?.Invoke(arg0,arg1,arg2,arg3);
        };
    }
    /// <summary>
    /// 多个参数的组件点击事件
    /// </summary>
    /// <param name="com"></param>
    /// <param name="cb"></param>
    public static void OnClick(this Component com, UnityAction cb)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return; }
        EventListener.EventListener.Get(com).onClick = (obj) =>
        {
            cb?.Invoke();
        };
    }
    /// <summary>
    /// 多个参数的组件点击事件
    /// </summary>
    /// <param name="com"></param>
    /// <param name="cb"></param>
    /// <param name="arg"></param>
    public static void OnClick<T>(this Component com, UnityAction<T> cb, T arg)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return; }
        EventListener.EventListener.Get(com).onClick = (obj) =>
        {
            cb?.Invoke(arg);
        };
    }
    /// <summary>
    /// 多个参数的组件点击事件
    /// </summary>
    /// <param name="com"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    public static void OnClick<T0,T1>(this Component com, UnityAction<T0,T1> cb, T0 arg0,T1 arg1)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return; }
        EventListener.EventListener.Get(com).onClick = (obj) =>
        {
            cb?.Invoke(arg0,arg1);
        };
    }
    /// <summary>
    /// 多个参数的组件点击事件
    /// </summary>
    /// <param name="com"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    public static void OnClick<T0,T1,T2>(this Component com, UnityAction<T0,T1,T2> cb, T0 arg0,T1 arg1,T2 arg2)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return; }
        EventListener.EventListener.Get(com).onClick = (obj) =>
        {
            cb?.Invoke(arg0,arg1,arg2);
        };
    }
    /// <summary>
    /// 多个参数的组件点击事件
    /// </summary>
    /// <param name="com"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    public static void OnClick<T0,T1,T2,T3>(this Component com, UnityAction<T0,T1,T2,T3> cb, T0 arg0,T1 arg1,T2 arg2,T3 arg3)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return; }
        EventListener.EventListener.Get(com).onClick = (obj) =>
        {
            cb?.Invoke(arg0,arg1,arg2,arg3);
        };
    }
    /// <summary>
    /// 多个参数的Button点击事件
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="cb"></param>
    public static void BtnOnClick(this Button btn, UnityAction cb)
    {
        if(btn == null){ Debug.LogError($"Button({btn}) is null!!!"); return; }
        btn.onClick.AddListener(cb);
    }
    /// <summary>
    /// 多个参数的Button点击事件
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="cb"></param>
    /// <param name="arg"></param>
    public static void BtnOnClick<T>(this Button btn, UnityAction<T> cb, T arg)
    {
        if(btn == null){ Debug.LogError($"Button({btn}) is null!!!"); return; }
        btn.onClick.AddListener(() =>
        {
            cb?.Invoke(arg);
        });
    }
    /// <summary>
    /// 多个参数的Button点击事件
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    public static void BtnOnClick<T0,T1>(this Button btn, UnityAction<T0,T1> cb, T0 arg0, T1 arg1)
    {
        if(btn == null){ Debug.LogError($"Button({btn}) is null!!!"); return; }
        btn.onClick.AddListener(() =>
        {
            cb?.Invoke(arg0, arg1);
        });
    }
    /// <summary>
    /// 多个参数的Button点击事件
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    public static void BtnOnClick<T0,T1,T2>(this Button btn, UnityAction<T0,T1,T2> cb, T0 arg0, T1 arg1,T2 arg2)
    {
        if(btn == null){ Debug.LogError($"Button({btn}) is null!!!"); return; }
        btn.onClick.AddListener(() =>
        {
            cb?.Invoke(arg0, arg1, arg2);
        });
    }
    /// <summary>
    /// 多个参数的Button点击事件
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="cb"></param>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    public static void BtnOnClick<T0,T1,T2,T3>(this Button btn, UnityAction<T0,T1,T2,T3> cb, T0 arg0, T1 arg1,T2 arg2,T3 arg3)
    {
        if(btn == null){ Debug.LogError($"Button({btn}) is null!!!"); return; }
        btn.onClick.AddListener(() =>
        {
            cb?.Invoke(arg0, arg1, arg2, arg3);
        });
    }
    #endregion

    #region EventTrigger
    /// <summary>
    /// 添加EventTrigger事件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="eventType"></param>
    /// <param name="ua"></param>
    public static void AddEventTrigger(this GameObject obj, EventTriggerType eventType, UnityAction<BaseEventData> ua)
    {
        if(obj == null){ Debug.LogError($"GameObject({obj.name}) is null!!!"); return; }
        EventTrigger eventTrigger = obj.TryGetComponent<EventTrigger>();
        eventTrigger.triggers = new List<EventTrigger.Entry>();

        UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(ua);

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(callback);
        eventTrigger.triggers.Add(entry);
    }

    /// <summary>
    /// 移除EventTrigger事件
    /// </summary>
    /// <param name="obj"></param>
    public static void RemoveEventTrigger(this GameObject obj)
    {
        if(obj == null){ Debug.LogError($"GameObject({obj.name}) is null!!!"); return; }
        EventTrigger eventTrigger = obj.TryGetComponent<EventTrigger>();
        eventTrigger.triggers.Clear();
        UnityEngine.Object.Destroy(eventTrigger);
    }

    /// <summary>
    /// 添加EventTrigger事件
    /// </summary>
    /// <param name="com"></param>
    /// <param name="eventType"></param>
    /// <param name="ua"></param>
    public static void AddEventTrigger(this Component com, EventTriggerType eventType, UnityAction<BaseEventData> ua)
    {
        if(com == null){ Debug.LogError($"GameObject({com}) is null!!!"); return; }
        EventTrigger eventTrigger = com.TryGetComponent<EventTrigger>();
        eventTrigger.triggers = new List<EventTrigger.Entry>();

        UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(ua);

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(callback);
        eventTrigger.triggers.Add(entry);
    }

    /// <summary>
    /// 移除EventTrigger事件
    /// </summary>
    /// <param name="com"></param>
    public static void RemoveEventTrigger(this Component com)
    {
        if(com == null){ Debug.LogError($"GameObject({com}) is null!!!"); return; }
        EventTrigger eventTrigger = com.TryGetComponent<EventTrigger>();
        eventTrigger.triggers.Clear();
        UnityEngine.Object.Destroy(eventTrigger);
    }
    #endregion

    #region Find Objects
    /// <summary>
    /// 根据路径查找gameObject
    /// </summary>
    /// <param name="go"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static GameObject FindGameObject(this GameObject go, string path)
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return null; }
        return go.transform.Find(path).gameObject;
    }

    /// <summary>
    /// 根据路径查找gameObject
    /// </summary>
    /// <param name="com"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static GameObject FindGameObject(this Component com, string path)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return null; }
        return com.transform.Find(path).gameObject;
    }

    /// <summary>
    /// 根据路径查找组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Component FindComponent(this GameObject go, string path)
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return null; }
        return go.transform.Find(path).GetComponent<Component>();
    }

    /// <summary>
    /// 根据路径查找组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="com"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Component FindComponent(this Component com, string path)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return null; }
        return com.transform.Find(path).GetComponent<Component>();
    }
    /// <summary>
    /// 根据路径查找组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T FindComponent<T>(this GameObject go, string path) where T : Component
    {
        if(go == null){ Debug.LogError($"GameObject({go.name}) is null!!!"); return null; }
        return go.transform.Find(path).GetComponent<T>();
    }

    /// <summary>
    /// 根据路径查找组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="com"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T FindComponent<T>(this Component com, string path) where T : Component
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return null; }
        return com.transform.Find(path).GetComponent<T>();
    }

    /// <summary>
    /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
    /// </summary>
    /// <param name="gomeObject">要在其中进行查找的父物体</param>
    /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
    /// <returns></returns>
    public static GameObject FindDeepGameObject(this GameObject gomeObject, string childName)
    {
        if(gomeObject == null){ Debug.LogError($"GameObject({gomeObject.name}) is null!!!"); return null; }
        Transform transform = gomeObject.transform.Find(childName);
        if (transform == null)
        {
            foreach (Transform trs in gomeObject.transform)
            {
                transform = trs.gameObject.FindDeepGameObject(childName).transform;
                if (transform != null)
                    return transform.gameObject;
            }
        }
        return transform.gameObject;
    }

    /// <summary>
    /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
    /// </summary>
    /// <param name="com">要在其中进行查找的父物体</param>
    /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
    /// <returns></returns>
    public static GameObject FindDeepGameObject(this Component com, string childName)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return null; }
        Transform transform = com.transform.Find(childName);
        if (transform == null)
        {
            foreach (Transform trs in com.transform)
            {
                transform = trs.gameObject.FindDeepGameObject(childName).transform;
                if (transform != null)
                    return transform.gameObject;
            }
        }
        return transform.gameObject;
    }

    /// <summary>
    /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
    /// </summary>
    /// <param name="gomeObject">要在其中进行查找的父物体</param>
    /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
    /// <returns></returns>
    public static Component FindDeepComponent(this GameObject gomeObject, string childName)
    {
        if(gomeObject == null){ Debug.LogError($"GameObject({gomeObject.name}) is null!!!"); return null; }
        Transform transform = gomeObject.transform.Find(childName);
        if (transform == null)
        {
            foreach (Transform trs in gomeObject.transform)
            {
                transform = trs.gameObject.FindDeepComponent(childName).transform;
                if (transform != null)
                    return transform.GetComponent<Component>();
            }
        }
        return transform.GetComponent<Component>();
    }

    /// <summary>
    /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
    /// </summary>
    /// <param name="com">要在其中进行查找的父物体</param>
    /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
    /// <returns></returns>
    public static Component FindDeepComponet(this Component com, string childName)
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return null; }
        Transform transform = com.transform.Find(childName);
        if (transform == null)
        {
            foreach (Transform trs in com.transform)
            {
                transform = trs.FindDeepComponet(childName).transform;
                if (transform != null)
                    return transform.GetComponent<Component>();
            }
        }
        return transform.GetComponent<Component>();
    }
    /// <summary>
    /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
    /// </summary>
    /// <param name="gomeObject">要在其中进行查找的父物体</param>
    /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
    /// <returns></returns>
    public static T FindDeepComponent<T>(this GameObject gomeObject, string childName) where T : Component
    {
        if(gomeObject == null){ Debug.LogError($"GameObject({gomeObject.name}) is null!!!"); return null; }
        Transform transform = gomeObject.transform.Find(childName);
        if (transform == null)
        {
            foreach (Transform trs in gomeObject.transform)
            {
                transform = trs.gameObject.FindDeepComponent<T>(childName).transform;
                if (transform != null)
                    return transform.GetComponent<T>();
            }
        }
        return transform.GetComponent<T>();
    }

    /// <summary>
    /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
    /// </summary>
    /// <param name="com">要在其中进行查找的父物体</param>
    /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
    /// <returns></returns>
    public static T FindDeepComponet<T>(this Component com, string childName) where T : Component
    {
        if(com == null){ Debug.LogError($"Component({com}) is null!!!"); return null; }
        Transform transform = com.transform.Find(childName);
        if (transform == null)
        {
            foreach (Transform trs in com.transform)
            {
                transform = trs.FindDeepComponet<T>(childName).transform;
                if (transform != null)
                    return transform.GetComponent<T>();
            }
        }
        return transform.GetComponent<T>();
    }
    #endregion

    #region FindLoopSelectable
    /// </summary>
    /// 循环寻找下一个UI组件
    /// <param name="current"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static Selectable FindLoopSelectable(this Selectable current, Vector3 dir)
    {
        Selectable first = current.FindSelectable(dir);//用一个向量Vector3去寻找第一个Selectable
        if (first != null)//如果下一个为null，用递归方法循环继续寻找第一个
        {
            current = first.FindLoopSelectable(dir);
        }
        return current;
    }
    #endregion
}