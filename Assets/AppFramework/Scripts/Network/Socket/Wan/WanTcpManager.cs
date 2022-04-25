using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class WanTcpManager : SingletonMonoEvent<WanTcpManager>
{
    private Socket socketTcp = null;
    public int backlog = 100;
    private int overtime = 5000;

    private Action<bool> serverCallBack;
    private Action<bool> clientCallBack;
    private Action closeCallBack;

    private Queue<byte[]> msgQueue = new Queue<byte[]>();//��Ϣ����
    private static readonly string lockNetTcp = "lockNetTcp";//����

    public delegate void BytesValueDelegate(byte[] value);
    public event BytesValueDelegate HandOutMsg;

    // Start is called before the first frame update
    public void Awake()
    {
        socketTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }
    #region Server
    public void StartAsServer(string ip, int port, Action<bool> cb = null)
    {
        try
        {
            serverCallBack = cb;
            socketTcp.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socketTcp.Listen(backlog);
            socketTcp.BeginAccept(ClientConnectCallBack, socketTcp);
            Debug.Log("Tcp����˿����ɹ������ڵȴ�����......");
            serverCallBack?.Invoke(true);
        }
        catch (Exception ex)
        {
            Debug.LogError("Tcp����˿���ʧ�ܣ�" + ex.Message);
            serverCallBack?.Invoke(false);
        }
    }

    private void ClientConnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = socketTcp.EndAccept(ar);
            StartReceiveMsg(socket, delegate
            {
                Debug.Log("�ͻ��˶Ͽ�����......");
            });
            Debug.Log("Tcp���ӿͻ��˳ɹ������ڽ�������......");
        }
        catch (Exception ex)
        {
            Debug.LogError("Tcp�������ر�:" + ex.Message);
            serverCallBack?.Invoke(false);
        }
        socketTcp.BeginAccept(ClientConnectCallBack, socketTcp);
    }
    #endregion
    #region Client
    public void StartAsClient(string ip, int port, Action<bool> cb = null)
    {
        try
        {
            clientCallBack = cb;
            IAsyncResult asyncResult = socketTcp.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ServerConnectCallBack, socketTcp);
            bool flag = asyncResult.AsyncWaitHandle.WaitOne(overtime, exitContext: true);
            if (!flag)
            {
                Close();
                SocketTools.LogMsg("Tcp�ͻ������ӳ�ʱ", LogLevel.Error);
                clientCallBack?.Invoke(flag);
            }
        }
        catch (Exception ex)
        {
            SocketTools.LogMsg("Tcp�ͻ�������ʧ�ܣ�" + ex.Message, LogLevel.Error);
            clientCallBack?.Invoke(false);
        }
    }
    private void ServerConnectCallBack(IAsyncResult ar)
    {
        try
        {
            socketTcp.EndConnect(ar);
            StartReceiveMsg(socketTcp, delegate
            {
                SocketTools.LogMsg("Tcp�������Ͽ�����......", LogLevel.Info);
                clientCallBack?.Invoke(false);
            });
            SocketTools.LogMsg("Tcp���ӷ������ɹ������ڽ�������......", LogLevel.Info);
            clientCallBack?.Invoke(true);
        }
        catch (Exception ex)
        {
            SocketTools.LogMsg("Tcp�ͻ��˹رգ�" + ex.Message, LogLevel.Error);
            clientCallBack?.Invoke(false);
        }
    }
    #endregion
    private void StartReceiveMsg(Socket socket, Action closeCallBack = null)
    {
        try
        {
            this.closeCallBack = closeCallBack;
            SocketPackage pEPkg = new SocketPackage();
            socket.BeginReceive(pEPkg.headBuffer, 0, pEPkg.headLength, SocketFlags.None, ReceiveHeadData, pEPkg);
        }
        catch (Exception ex)
        {
            Debug.LogError("StartRcvData:" + ex.Message);
        }
    }

    private void ReceiveHeadData(IAsyncResult ar)
    {
        try
        {
            SocketPackage package = (SocketPackage)ar.AsyncState;
            if (socketTcp.Available == 0)
            {
                Clear();
            }
            else
            {
                int num = socketTcp.EndReceive(ar);
                if (num > 0)
                {
                    package.headIndex += num;
                    if (package.headIndex < package.headLength)
                    {
                        socketTcp.BeginReceive(package.headBuffer, package.headIndex, package.headLength - package.headIndex, SocketFlags.None, ReceiveHeadData, package);
                    }
                    else
                    {
                        package.InitBodyBuffer();
                        socketTcp.BeginReceive(package.bodyBuffer, 0, package.bodyLength, SocketFlags.None, ReceiveBodyData, package);
                    }
                }
                else
                {
                    Clear();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("ReceiveHeadError:" + ex.Message);
        }
    }

    private void ReceiveBodyData(IAsyncResult ar)
    {
        try
        {
            SocketPackage package = (SocketPackage)ar.AsyncState;
            int num = socketTcp.EndReceive(ar);
            if (num > 0)
            {
                package.bodyIndex += num;
                if (package.bodyIndex < package.bodyLength)
                {
                    socketTcp.BeginReceive(package.bodyBuffer, package.bodyIndex, package.bodyLength - package.bodyIndex, SocketFlags.None, ReceiveBodyData, package);
                }
                else
                {
                    //string msg = Encoding.UTF8.GetString(package.bodyBuffer);
                    AddMsgQueue(package.bodyBuffer);
                    package.ResetData();
                    socketTcp.BeginReceive(package.headBuffer, 0, package.headLength, SocketFlags.None, ReceiveHeadData, package);
                }
            }
            else
            {
                Clear();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("RcvBodyError:" + ex.Message);
        }
    }
    /// <summary>����Ϣ�������</summary>
    private void AddMsgQueue(byte[] bytes)
    {
        lock (lockNetTcp)
        {
            msgQueue.Enqueue(bytes);
        }
    }
    // Update is called once per frame
    public void Update()
    {
        if (msgQueue.Count > 0)
        {
            lock (lockNetTcp)
            {
                for (int i = 0; i < msgQueue.Count; i++)
                {
                    HandOutMsg(msgQueue.Dequeue());//ȡ��Ϣ�� ���зַ�
                }
            }
        }
    }

    public void SendMsg(byte[] _data)
    {
        byte[] data = PackageLengthInfo(_data);
        NetworkStream networkStream = null;
        try
        {
            networkStream = new NetworkStream(socketTcp);
            if (networkStream.CanWrite)
            {
                networkStream.BeginWrite(data, 0, data.Length, SendCallBack, networkStream);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMsgError:" + ex.Message);
        }
    }
    private byte[] PackageLengthInfo(byte[] data)
    {
        int num = data.Length;
        byte[] array = new byte[num + 4];
        byte[] bytes = BitConverter.GetBytes(num);
        bytes.CopyTo(array, 0);
        data.CopyTo(array, 4);
        return array;
    }

    private void SendCallBack(IAsyncResult ar)
    {
        NetworkStream networkStream = (NetworkStream)ar.AsyncState;
        try
        {
            networkStream.EndWrite(ar);
            networkStream.Flush();
            networkStream.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMsgError:" + ex.Message);
        }
    }
    private void Clear()
    {
        closeCallBack?.Invoke();
        socketTcp.Close();
    }
    /// <summary>�ر�Tcp</summary>
    public bool Close()
    {
        try
        {
            if (socketTcp != null)
            {
                if (socketTcp.Connected)
                {
                    socketTcp.Shutdown(SocketShutdown.Both);
                }
                socketTcp.Close();
            }
            return true;
        }
        catch (Exception arg)
        {
            Debug.LogError("Tcp�ر�Socket����" + arg);
            return false;
        }
    }
}
