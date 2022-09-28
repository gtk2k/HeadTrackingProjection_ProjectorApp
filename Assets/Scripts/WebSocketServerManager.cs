using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebSocketServerManager
{
    public event Action<string> OnClientConnected;
    public event Action <string> OnClientReset;
    public event Action<string, Pose> OnMarkerPose;
    public event Action<string, Pose> OnClientPose;
    public event Action<string, string> OnClientTextMessage;
    public event Action<string, ushort, string> OnClientClosed;
    public event Action<string, Exception> OnClientError;

    private WebSocketServer _wss;
    private SynchronizationContext _ctx;
    private Dictionary<string, WebSocket> _clients;

    public enum MessageType : byte
    {
        None = 0,
        AppReset = 1,
        MarkerPose = 2,
        PlayerPose = 3
    }

    private class WSSBehaviour : WebSocketBehavior
    {
        public event Action<string> OnClientConnected;
        public event Action<string, byte[]> OnClientBinaryMessage;
        public event Action<string, string> OnClientTextMessage;
        public event Action<string, ushort, string> OnClientClosed;
        public event Action<string, Exception> OnClientError;

        protected override void OnOpen()
        {
            OnClientConnected?.Invoke(ID);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                OnClientBinaryMessage?.Invoke(ID, e.RawData);
            }
            else
            {
                OnClientTextMessage?.Invoke(ID, e.Data);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            OnClientClosed?.Invoke(ID, e.Code, e.Reason);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            OnClientError?.Invoke(ID, e.Exception);
        }
    }

    public WebSocketServerManager(int port)
    {
        _ctx = SynchronizationContext.Current;
        _wss = new WebSocketServer(port);
        _clients = new Dictionary<string, WebSocket>();
        _wss.AddWebSocketService<WSSBehaviour>("/", (behaviour) =>
        {
            behaviour.OnClientConnected += Behaviour_OnClientConnected;
            behaviour.OnClientBinaryMessage += Behaviour_OnClientBinaryMessage;
            behaviour.OnClientTextMessage += Behaviour_OnClientTextMessage;
            behaviour.OnClientClosed += Behaviour_OnClientClosed;
            behaviour.OnClientError += Behaviour_OnClientError;
        });
    }

    private void Behaviour_OnClientConnected(string id)
    {
        _ctx.Post(_ =>
        {
            OnClientConnected?.Invoke(id);
        }, null);
    }

    private void Behaviour_OnClientBinaryMessage(string id, byte[] data)
    {
        _ctx.Post(_ =>
        {
            var type = (MessageType)data[0];
            Debug.Log(type);
            if (type == MessageType.AppReset)
            {
                OnClientReset?.Invoke(id);
                return;
            }
            var px = (float)BitConverter.ToSingle(data, 1);
            var py = (float)BitConverter.ToSingle(data, 5);
            var pz = (float)BitConverter.ToSingle(data, 9);
            var rx = (float)BitConverter.ToSingle(data, 13);
            var ry = (float)BitConverter.ToSingle(data, 17);
            var rz = (float)BitConverter.ToSingle(data, 21);
            var rw = (float)BitConverter.ToSingle(data, 25);
            //var sx = (float)BitConverter.ToSingle(data, 29);
            //var sy = (float)BitConverter.ToSingle(data, 33);
            //var sz = (float)BitConverter.ToSingle(data, 37);
            var pose = new Pose
            {
                position = new Vector3(px, py, pz),
                rotation = new Quaternion(rx, ry, rz, rw)
            };
            if (type == MessageType.MarkerPose)
            {
                OnMarkerPose?.Invoke(id, pose);
            }
            else
            {
                OnClientPose?.Invoke(id, pose);
            }
        }, null);
    }

    private void Behaviour_OnClientTextMessage(string id, string data)
    {
        _ctx.Post(_ =>
        {
            OnClientTextMessage?.Invoke(id, data);
        }, null);
    }

    private void Behaviour_OnClientClosed(string id, ushort code, string reason)
    {
        _ctx.Post(_ =>
        {
            OnClientClosed?.Invoke(id, code, reason);
        }, null);
    }

    private void Behaviour_OnClientError(string id, Exception ex)
    {
        _ctx.Post(_ =>
        {
            OnClientError?.Invoke(id, ex);
        }, null);
    }

    public void ServerStart()
    {
        _wss.Start();
    }

    public void ServerStop()
    {
        if (_wss != null)
        {
            _wss.Stop();
        }
        _wss = null;
    }

    public void Send(string id, MessageType type, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var px = BitConverter.GetBytes(position.x);
        var py = BitConverter.GetBytes(position.y);
        var pz = BitConverter.GetBytes(position.z);
        var rx = BitConverter.GetBytes(rotation.x);
        var ry = BitConverter.GetBytes(rotation.y);
        var rz = BitConverter.GetBytes(rotation.z);
        var rw = BitConverter.GetBytes(rotation.w);
        //var sx = BitConverter.GetBytes(scale.x);
        //var sy = BitConverter.GetBytes(scale.y);
        //var sz = BitConverter.GetBytes(scale.z);
        var data = new byte[7 * 4 + 1];
        data[0] = (byte)type;
        Buffer.BlockCopy(px, 0, data, 1, 4);
        Buffer.BlockCopy(py, 0, data, 5, 4);
        Buffer.BlockCopy(pz, 0, data, 9, 4);
        Buffer.BlockCopy(rx, 0, data, 13, 4);
        Buffer.BlockCopy(ry, 0, data, 17, 4);
        Buffer.BlockCopy(rz, 0, data, 21, 4);
        Buffer.BlockCopy(rw, 0, data, 25, 4);
        //Buffer.BlockCopy(sx, 0, data, 29, 4);
        //Buffer.BlockCopy(sy, 0, data, 33, 4);
        //Buffer.BlockCopy(sz, 0, data, 37, 4);

        _clients[id].Send(data);
    }
}
