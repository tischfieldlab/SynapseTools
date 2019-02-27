using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynapseTools;

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
            var client = new TdtUdp(typeof(float), "localhost");
        }
        [TestMethod]
        public void TestSendData()
        {
            int iterations = 10;
            int packetLength = 50;
            Random randNum = new Random();

            var client = new TdtUdp(typeof(float), "localhost");
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

            var client = new TdtUdp(typeof(float), "172.17.50.89");
            for (int i = 0; i < iterations; i++)
            {
                var data = client.Receive();
                Debug.WriteLine(data);
            }
        }

    }
}
