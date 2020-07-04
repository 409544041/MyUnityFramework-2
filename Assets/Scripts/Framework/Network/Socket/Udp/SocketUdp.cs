﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class SocketUdp<T> where T : SessionUdpBase, new()
{
	public T session;

	private Socket socketUdp;

	private IPEndPoint iep;

	private EndPoint ep;

	private Thread thread;

	public SocketUdp()
	{
		session = new T();
		socketUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
	}

	public void StartAsServer(int port)
	{
		try
		{
			socketUdp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
			iep = new IPEndPoint(IPAddress.Broadcast, port);
			ep = iep;
			thread = new Thread(StartServerReceive);
			thread.Start();
			SocketTools.LogMsg("Udp服务端开启成功!", LogLevel.Info);
		}
		catch (Exception ex)
		{
			SocketTools.LogMsg("Udp服务端开启失败!" + ex.Message, LogLevel.Error);
		}
	}

	private void StartServerReceive()
	{
		session.StartReceiveData(socketUdp, ep, thread);
		session.SendMsg();
		session.ReceiveData();
	}

	public void StartAsClient(int port)
	{
		try
		{
			iep = new IPEndPoint(IPAddress.Any, port);
			socketUdp.Bind(iep);
			ep = iep;
			thread = new Thread(StartClientReceive);
			thread.Start();
			SocketTools.LogMsg("Udp客户端开启成功!", LogLevel.Info);
		}
		catch (Exception ex)
		{
			SocketTools.LogMsg("Udp客户端开启失败!" + ex.Message, LogLevel.Error);
		}
	}

	private void StartClientReceive()
	{
		session.StartReceiveData(socketUdp, ep, thread);
		session.ReceiveData();
	}
}