using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class WanUdpManager : SingletonMonoEvent<WanUdpManager>
{
	private Socket socketUdp;
	private IPEndPoint iep;
	private EndPoint ep;
	private Thread thread;

	private Queue<byte[]> msgQueue = new Queue<byte[]>();//��Ϣ����
	private static readonly string lockNetUdp = "lockNetUdp";//����

	public delegate void BytesValueDelegate(byte[] value);
	public event BytesValueDelegate HandOutMsg;
	public delegate void CallbackDelegate();
	public event CallbackDelegate OnQuitSocket;
	public void Awake()
	{
		socketUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
	}

    #region Server
    public void StartAsServer(int port, Action<bool> cb = null)
	{
		try
		{
			socketUdp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
			iep = new IPEndPoint(IPAddress.Broadcast, port);
			ep = iep;
			thread = new Thread(ReceiveMsg);
			thread.Start();
			Debug.Log("Udp����˿����ɹ�!");
			cb?.Invoke(true);
		}
		catch (Exception ex)
		{
			Debug.LogError("Udp����˿���ʧ��! " + ex.Message);
			cb?.Invoke(false);
		}
	}
	#endregion
	#region Client
	public void StartAsClient(int port, Action<bool> cb = null)
	{
		try
		{
			iep = new IPEndPoint(IPAddress.Any, port);
			socketUdp.Bind(iep);
			ep = iep;
			thread = new Thread(ReceiveMsg);
			thread.Start();
			Debug.Log("Udp�ͻ��˿����ɹ�!");
			cb?.Invoke(true);
		}
		catch (Exception ex)
		{
			Debug.LogError("Udp�ͻ��˿���ʧ��! " + ex.Message);
			cb?.Invoke(false);
		}
	}
	#endregion

	// Update is called once per frame
	public void Update()
	{
		if (msgQueue.Count > 0)
		{
			lock (lockNetUdp)
			{
                HandOutMsg(msgQueue.Dequeue());//ȡ��Ϣ�� ���зַ�
            }
		}
	}
	public void SendMsg(byte[] data)
	{
		try
		{
			socketUdp.SendTo(data, data.Length, SocketFlags.None, ep);
		}
		catch (Exception ex)
		{
			Debug.LogError("SndMsgError:" + ex.Message);
		}
	}
	private void ReceiveMsg()
	{
		while (true)
		{
			try
			{
				byte[] bytes = new byte[1024];
				socketUdp.ReceiveFrom(bytes, ref ep);
                AddMsgQueue(bytes);
                Thread.Sleep(100);
			}
			catch
			{
				return;
			}
		}
	}
	/// <summary>����Ϣ������� </summary>
	private void AddMsgQueue(byte[] bytes)
	{
		lock (lockNetUdp)
		{
			msgQueue.Enqueue(bytes);
		}
	}
	public void Close()
	{
		if (thread != null)
		{
			thread.Interrupt();
			thread.Abort();
		}
		if (socketUdp != null)
		{
			socketUdp.Close();
		}
        OnQuitSocket.Invoke();
        Debug.Log("UDP�ѹر�...");
	}
}
