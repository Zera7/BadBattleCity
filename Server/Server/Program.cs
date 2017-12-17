using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Contact
{
    class Connector
    {
        private readonly UdpClient UdpClient;
        public IPEndPoint SenderEndPoint;
        public List<byte[]> Messages = new List<byte[]>();

        public Connector(IPEndPoint senderEndPoint, int receivePort)
        {
            UdpClient = new UdpClient(receivePort);
            SenderEndPoint = senderEndPoint;
        }

        public void Start()
        {
            Thread receiverThread = new Thread(Receive);
            receiverThread.Start();
        }

        private void Receive()
        {
            while (true)
            {
                IPEndPoint endPoint = null;
                byte[] bytes = UdpClient.Receive(ref endPoint);
                Messages.Add(bytes);
            }
        }

        public void Send(string message)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                UdpClient.Send(bytes, bytes.Length, SenderEndPoint);
            }
            catch (Exception)
            {
            }
        }
    }
}