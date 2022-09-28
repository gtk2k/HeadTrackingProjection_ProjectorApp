using System;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

public class WebSocketManager
{
    public event Action OnOpen;
    public event Action OnAppReset;
    public event Action<Pose> OnMarkerPose;
    public event Action<Pose> OnPlayerPose;
    public event Action<string> OnTextMessage;
    public event Action<ushort, string> OnClose;
    public event Action<Exception> OnError;

    private WebSocket _ws;
    private SynchronizationContext _ctx;

    public enum MessageType : byte
    {
        None = 0,
        AppReset = 1,
        MarkerPose = 2,
        PlayerPose = 3
    }

    public WebSocketManager(string url)
    {
        _ctx = SynchronizationContext.Current;
        _ws = new WebSocket(url);
        _ws.OnOpen += Ws_OnOpen;
        _ws.OnMessage += _ws_OnMessage;
        _ws.OnClose += Ws_OnClose;
        _ws.OnError += Ws_OnError;
    }

    private void Ws_OnOpen(object sender, System.EventArgs e)
    {
        _ctx.Post(_ =>
        {
            Debug.Log($"WS OnOpen");
            OnOpen?.Invoke();
        }, null);
    }

    private void _ws_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
        {
            var data = e.RawData;
            var type = (MessageType)data[0];
            if (type == MessageType.AppReset)
            {
                OnAppReset?.Invoke();
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
                OnMarkerPose?.Invoke(pose);
            }
            else
            {
                OnPlayerPose?.Invoke(pose);
            }
        }
        else
        {
            OnTextMessage?.Invoke(e.Data);
        }
    }

    private void Ws_OnClose(object sender, CloseEventArgs e)
    {
        _ctx.Post(_ =>
        {
            OnClose?.Invoke(e.Code, e.Reason);
        }, null);
    }

    private void Ws_OnError(object sender, ErrorEventArgs e)
    {
        _ctx.Post(_ =>
        {
            OnError?.Invoke(e.Exception);
        }, null);
    }

    public void Connect()
    {
        _ws.Connect();
    }

    public void Close()
    {
        if (_ws != null)
        {
            _ws.Close();
        }
        _ws = null;
    }

    public void SendAppReset()
    {
        Debug.Log($"SendAppReset()");
        var data = new byte[1];
        data[0] = (byte)MessageType.AppReset;
        _ws.Send(data);
    }

    public void SendPose(MessageType type, Vector3 position, Quaternion rotation)
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

        _ws.Send(data);
    }
}
