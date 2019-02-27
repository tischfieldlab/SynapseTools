using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SynapseTools
{
    public class TdtUdp
    {
        protected static readonly byte CMD_SEND_DATA = 0x00;
        protected static readonly byte CMD_GET_VERSION = 0x01;
        protected static readonly byte CMD_SET_REMOTE_IP = 0x02;
        protected static readonly byte CMD_FORGET_REMOTE_IP = 0x03;

        protected Type dataType;
        protected string hostname;
        protected int port;
        protected UdpClient client;

        public TdtUdp(Type dataType, string Hostname, int Port = 22022)
        {
            if(!(dataType.Equals(typeof(float)) || dataType.Equals(typeof(int))))
            {
                throw new InvalidOperationException("Only types int and float are supported, not " + dataType.Name);
            }
            this.dataType = dataType;
            this.hostname = Hostname;
            this.port = Port;

            this.client = new UdpClient();
//            this.client.EnableBroadcast = true;
            this.client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.client.Connect(this.hostname, this.port);

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


        public void Send(Array data)
        {
            int dataLength = Buffer.ByteLength(data);
            byte[] message = new byte[dataLength + 4];
            message[0] = 0x55;
            message[1] = 0xAA;
            message[2] = CMD_SEND_DATA;
            message[3] = (byte)((char)data.Length);
            Buffer.BlockCopy(data, 0, message, 4, dataLength);

            if (BitConverter.IsLittleEndian)
            {
                this.LittleEndianToBigEndian(message);
            }

            this.client.Send(message, message.Length);
        }

        public Array Receive()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, this.port);
            byte[] receiveBytes = this.client.Receive(ref endpoint);

            if (BitConverter.ToInt16((byte[])receiveBytes.Take(2), 0) != 0x55AA)
            {
                throw new ApplicationException("Bad header!");
            }
            receiveBytes.Skip(2).Take(receiveBytes.Length - 4);
            return null;
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
