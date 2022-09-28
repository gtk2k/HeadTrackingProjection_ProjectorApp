using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpDiscoveryBroadcaster : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var Server = new UdpClient(8888);
        var ResponseData = Encoding.UTF8.GetBytes("SomeResponseData");

        while (true)
        {
            var ClientEp = new IPEndPoint(IPAddress.Any, 0);
            var ClientRequestData = Server.Receive(ref ClientEp);
            var ClientRequest = Encoding.UTF8.GetString(ClientRequestData);

            Debug.Log($"Recived {ClientRequest} from {ClientEp.Address.ToString()}, sending response");
            Server.Send(ResponseData, ResponseData.Length, ClientEp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
