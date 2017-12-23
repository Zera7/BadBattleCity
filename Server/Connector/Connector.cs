using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

public class Connector
{
    public struct Messages
    {
        public byte[] Message;
        public IPEndPoint Address;
    }

    private Thread receiverThread;

    public IPEndPoint SenderDefaultEndPoint;
    public IPEndPoint LastReseivePoint = null;
    public readonly UdpClient UdpClient;
    public List<Messages> AllMessages = new List<Messages>();
    public List<IPEndPoint> Clients = new List<IPEndPoint>();

    public Connector(IPEndPoint senderEndPoint, int receivePort)
    {
        UdpClient = new UdpClient(receivePort);
        SenderDefaultEndPoint = senderEndPoint;
    }

    public void Start()
    {
        receiverThread = new Thread(Receive);
        receiverThread.Start();
    }

    public void Stop()
    {
        if (receiverThread != null && receiverThread.IsAlive)
            receiverThread.Abort();
        UdpClient.Close();
    }

    private void Receive()
    {
        while (true)
        {
            byte[] bytes = UdpClient.Receive(ref LastReseivePoint);
            AllMessages.Add(new Messages
            {
                Address = LastReseivePoint,
                Message = bytes
            });
        }
    }

    public void Send(string message, IPEndPoint endPoint)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            UdpClient.Send(bytes, bytes.Length, endPoint);
        }
        catch (Exception)
        {
        }
    }

    public Messages SyncReceive()
    {
        byte[] bytes = UdpClient.Receive(ref LastReseivePoint);
        AllMessages.Add(new Messages
        {
            Address = LastReseivePoint,
            Message = bytes
        });
        return AllMessages[AllMessages.Count - 1];
    }
}
