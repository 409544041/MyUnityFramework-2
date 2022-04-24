using UnityEngine;
using System;
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public static class Utils
{
    #region var
    /// <summary> ������� </summary>
    public static bool NetAvailable
    {
        get
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
    /// <summary> �Ƿ������� </summary>
    public static bool IsWifi
    {
        get
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }
    /// <summary> ��ȡIP��ַ </summary>
    public static string IPAddress
    {
        get
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        //IPv4
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }
    }
    /// <summary> ��ȡ�豸ID </summary>
    public static string GetDeviceId
    {
        get
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
    /// <summary> ��ȡGUID </summary>
    public static string GetGuid
    {
        get
        {
            return Guid.NewGuid().ToString();
        }
    }
    /// <summary> ��ȡʱ��� </summary>
    public static long GetTimeStamp
    {
        get
        {
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (long)ts.TotalMilliseconds;
        }
    }
    #endregion

    #region function
    /// <summary> ����� </summary>
    public static int Random(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    /// <summary> ����� </summary>
    public static float Random(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    /// <summary> Base64���� </summary>
    public static string Encode(string message)
    {
        byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
        return Convert.ToBase64String(bytes);
    }
    /// <summary> Base64���� </summary>
    public static string Decode(string message)
    {
        byte[] bytes = Convert.FromBase64String(message);
        return Encoding.GetEncoding("utf-8").GetString(bytes);
    }
    /// <summary> �жϽǶ� </summary>
    public static float PointToAngle(Vector2 p1, Vector2 p2)
    {
        float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;

        if (angle >= 0 && angle <= 180)
        {
            return angle;
        }
        else
        {
            return 360 + angle;
        }
    }
    /// <summary> �����ڴ� </summary>
    public static void ClearMemory()
    {
        GC.Collect(); Resources.UnloadUnusedAssets();
    }
    #endregion
}