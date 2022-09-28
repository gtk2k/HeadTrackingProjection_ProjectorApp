using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpDiscoveryReceiver : MonoBehaviour
{
    void Start()
    {
        var Client = new UdpClient();
        var RequestData = Encoding.UTF8.GetBytes("SomeRequestData");
        var ServerEp = new IPEndPoint(IPAddress.Any, 0);

        Client.EnableBroadcast = true;
        Client.BeginSend(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888), SendCallback, Client);
        Client.BeginReceive(ReceiveCallback, Client);
        var ServerResponseData = Client.Receive(ref ServerEp);
        var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);
        Debug.Log($"Recived {ServerResponse} from {ServerEp.Address}");

        Client.Close();
    }

    private void SendCallback(IAsyncResult ar)
    {
        var udp =(UdpClient)ar.AsyncState;

        try
        {
            udp.EndSend(ar);
        }
        catch (SocketException ex)
        {
            Debug.LogError($" Send Error > code: {ex.ErrorCode}, message: {ex.Message}");
        }
        catch (ObjectDisposedException)
        {
            Debug.LogError("UDP Socket Closed");
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        var udp = (UdpClient)ar.AsyncState;

        IPEndPoint remoteEP = null;
        byte[] rcvBytes;
        try
        {
            rcvBytes = udp.EndReceive(ar, ref remoteEP);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Receive Error > code: {ex.ErrorCode}, message: {ex.Message}");
            return;
        }
        catch (ObjectDisposedException ex)
        {
            Console.WriteLine("UDP Socket Closed");
            return;
        }

        var msg = Encoding.UTF8.GetString(rcvBytes);

        udp.BeginReceive(ReceiveCallback, udp);
    }
}
