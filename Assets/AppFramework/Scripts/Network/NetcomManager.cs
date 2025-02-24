﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public partial class NetcomManager : SingletonMonoEvent<NetcomManager>
{
    public const string HttpAppID = "";
    public const string HttpAppSecret = "";
    /// <summary>
    /// Token验证
    /// </summary>
    public static string Token { get; set; }
    /// <summary>
    /// HTTP服务器地址
    /// </summary>
    public static string HttpAddress
    {
        private set { }
        get
        {
            return Root.AppConfig.IsTestServer ? "" : "";//测试服务器:生产服务器
        }
    }
    /// <summary>
    /// Socket服务器地址
    /// </summary>
    public static string SocketAddress
    {
        private set { }
        get
        {
            return Root.AppConfig.IsTestServer ? "" : "";//测试服务器:生产服务器
        }
    }
    /// <summary>
    /// Socket服务器端口
    /// </summary>
    public static int SocketPort
    {
        private set { }
        get
        {
            return Root.AppConfig.IsTestServer ? 17888 : 8080;//测试服务器:生产服务器
        }
    }
    public static string ABUrl
    {
        private set { }
        get
        {
            string test_url = Path.Combine(Application.dataPath.Replace("Assets", ""), "AssetBundle/");//本地AB包地址
            string pro_url = "https://meta-oss.genimous.com/vr-ota/dev_test/";//服务器AB包地址
            return Root.AppConfig.IsTestServer ? test_url : pro_url;
        }
    }
    public static string AppUrl
    {
        private set { }
        get
        {
            string test_url = Path.Combine(Application.dataPath.Replace("Assets", ""), "App/");//本地AB包地址
            string pro_url = "https://meta-oss.genimous.com/vr-ota/App/";//服务器AB包地址
            return Root.AppConfig.IsTestServer ? test_url : pro_url;
        }
    }

    private Queue<UnityWebRequester> uwrQueue = new Queue<UnityWebRequester>();
    public UnityWebRequester Uwr
    {
        private set { }
        get
        {
            if (uwrQueue.Count <= 1)
            {
                InitQueue();
            }
            return uwrQueue.Dequeue();
        }
    }
    public override void InitManager(Transform parent)
    {
        base.InitManager(parent);
        InitQueue(50);
    }

    public void InitQueue(int count = 20)
    {
        for (int i = 0; i < count; i++)
        {
            UnityWebRequester uwr = new UnityWebRequester(this);
            uwrQueue.Enqueue(uwr);
        }
    }
}