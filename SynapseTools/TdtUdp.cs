using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SynapseTools
{
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
