using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SynapseTools
{
    public class DataReceivedEventArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }
        public Array Data { get; set; }
    }

    public class TdtUdp
    {
        protected static readonly byte CMD_SEND_DATA = 0x00;
        protected static readonly byte CMD_GET_VERSION = 0x01;
        protected static readonly byte CMD_SET_REMOTE_IP = 0x02;
        protected static readonly byte CMD_FORGET_REMOTE_IP = 0x03;
        protected static readonly byte[] HEADER = new byte[] { 0x55, 0xAA };

        protected string hostname;
        protected int port;
        protected UdpClient client;

        public event EventHandler<DataReceivedEventArgs> DataRecieved;

        public TdtUdp(string Hostname, int Port = 22022)
        {
            this.hostname = Hostname;
            this.port = Port;

            this.client = new UdpClient(this.port);
            this.client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            //Send a connection packet
            this.Send(new byte[] { 0x55, 0xAA, CMD_SET_REMOTE_IP, 0x00 });
        }
        public string Hostname
        {
            get { return this.hostname; }
        }
        public int Port
        {
            get { return this.port; }
        }

        public void Close()
        {
            this.tokenSource.Cancel();
            this.listeningTask.Wait();
            this.client.Close();
        }
        public void Send<T>(T[] data)
        {
            int dataLength = Buffer.ByteLength(data);
            byte[] message = new byte[dataLength + 4];
            message[0] = HEADER[0];
            message[1] = HEADER[1];
            message[2] = CMD_SEND_DATA;
            message[3] = (byte)((char)data.Length);
            Buffer.BlockCopy(data, 0, message, 4, dataLength);

            if (BitConverter.IsLittleEndian)
            {
                this.LittleEndianToBigEndian(message);
            }

            this.client.Send(message, message.Length, this.hostname, this.port);
        }

        protected Task listeningTask;
        protected CancellationTokenSource tokenSource;
        public void Listen<T>()
        {
            if(this.listeningTask != null)
            {
                throw new InvalidOperationException("Cannot listen as I am already listening!");
            }
            this.tokenSource = new CancellationTokenSource();
            this.listeningTask = Task.Factory.StartNew(() =>
            {
                //make sure we were not requested to cancel
                while (!this.tokenSource.IsCancellationRequested)
                {
                    //make sure data is available, otherwise we may deadlock blocking on recieve
                    if (this.client.Available > 0) 
                    {
                        var data = this.Receive<T>();
                        this.DataRecieved?.Invoke(this, new DataReceivedEventArgs()
                        {
                            Data = data,
                            Timestamp = new DateTime(Stopwatch.GetTimestamp())
                        });
                    }
                }
            }, this.tokenSource.Token);
        }
        public T[] Receive<T>()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBytes = this.client.Receive(ref endpoint);

            if (!HEADER.SequenceEqual(receiveBytes.Take(2)))
            {
                throw new ApplicationException("Bad header encountered!");
            }

            if (BitConverter.IsLittleEndian)
            {
                this.LittleEndianToBigEndian(receiveBytes);
            }

            T[] data = new T[(receiveBytes.Length - 4) / 4];
            Buffer.BlockCopy(receiveBytes, 4, data, 0, receiveBytes.Length - 4);

            return data;
        }

        protected void LittleEndianToBigEndian(byte[] Data, int BytesPerPoint=4, int Offset=4)
        {
            for(int i=Offset; i < Data.Length; i += BytesPerPoint)
            {
                byte tmp = Data[i + 3];
                Data[i + 3] = Data[i];
                Data[i] = tmp;
                tmp = Data[i + 2];
                Data[i + 2] = Data[i + 1];
                Data[i + 1] = tmp;
            }
        }
    }
}
