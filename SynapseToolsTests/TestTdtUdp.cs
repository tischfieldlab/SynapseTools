using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynapseTools;
using System.Linq;

namespace SynapseToolsTests
{
    [TestClass]
    public class TestTdtUdp
    {
        public TestTdtUdp()
        {
            
        }

        [TestMethod]
        public void TestCreateClient()
        {
            var client = new TdtUdp("localhost");
        }
        [TestMethod]
        public void TestSendData()
        {
            int iterations = 100;
            int packetLength = 4;
            Random randNum = new Random();

            var client = new TdtUdp("localhost");
            for(int i=0; i< iterations; i++)
            {
                float[] data = new float[packetLength];
                for(int j=0; j< packetLength; j++)
                {
                    data[j] = (float)j;
                }
                client.Send(data);
                Thread.Sleep(500);
            }
        }

        [TestMethod]
        public void TestReceiveData()
        {
            int iterations = 1000;

            var client = new TdtUdp("172.17.50.89");
            for (int i = 0; i < iterations; i++)
            {
                float[] data = client.Receive<float>();
                //Debug.WriteLine(data);
                Debug.WriteLine("[{0}]", string.Join(", ", data.Select(v => v.ToString())));
            }
        }

    }
}
