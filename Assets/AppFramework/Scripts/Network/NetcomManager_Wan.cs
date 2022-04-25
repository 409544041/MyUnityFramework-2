using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Socket
/// </summary>
public partial class NetcomManager : SingletonEvent<NetcomManager>
{
    #region TCP
    public void StartWanTcpClient(string ip, Action<byte[]> cb)
    {
        WanTcpManager.Instance.StartAsClient(ip, Port, (bool isConnect) => { });
        WanTcpManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
    }
    public void SendWanTcpMsg(byte[] bytes)
    {
        WanTcpManager.Instance.SendMsg(bytes);
    }
    public void WanTcpQuit()
    {
        WanTcpManager.Instance.Close();
    }
    /// <summary>
    /// ���л�
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static byte[] Serialize(IMessage message)
    {
        return message.ToByteArray();
    }
    /// <summary>
    /// �����л�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="packct"></param>
    /// <returns></returns>
    public static T DeSerialize<T>(byte[] packct) where T : IMessage, new()
    {
        IMessage message = new T();
        try
        {
            return (T)message.Descriptor.Parser.ParseFrom(packct);
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }
    #endregion
}