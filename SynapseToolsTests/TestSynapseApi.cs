using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynapseTools;

namespace SynapseToolsTests
{
    [TestClass]
    public class TestSynapseApi
    {
        protected SynapseClient client;
        public TestSynapseApi()
        {
            this.client = SynapseClient.GetClient();
        }

        [TestMethod]
        public void TestModeToggle()
        {
            Assert.AreEqual(SynapseMode.Idle, this.client.Mode);
            this.client.Mode = SynapseMode.Preview;
            Assert.AreEqual(SynapseMode.Preview, this.client.Mode);
            Thread.Sleep(3000);
            this.client.Mode = SynapseMode.Idle;
            Assert.AreEqual(SynapseMode.Idle, this.client.Mode);
        }
        [TestMethod]
        public void TestMethod2()
        {
            Assert.AreEqual("Best", this.client.GetPersistMode());
        }
        [TestMethod]
        public void TestMethod3()
        {
            CollectionAssert.AreEqual(new List<string>() { "Best", "Last", "Fresh" }, this.client.GetPersistModes());
        }
        [TestMethod]
        public void TestApiSurface()
        {
            var m = this.client.Mode;
            this.client.GetSystemStatus();
            this.client.GetPersistModes();
            this.client.GetPersistMode();
            this.client.GetSamplingRates();
            this.client.GetKnownSubjects();
            this.client.GetKnownUsers();
            this.client.GetKnownExperiments();
            this.client.GetKnownTanks();
            this.client.GetKnownBlocks();
            this.client.GetCurrentSubject();
            this.client.GetCurrentUser();
            this.client.GetCurrentExperiment();
            this.client.GetCurrentTank();
            //this.client.GetCurrentBlock();
            this.client.GetGizmoNames();
            this.client.GetParameterNames("FibPho1");
            this.client.GetParameterInfo("FibPho1", "DriveOn-1");
            this.client.GetParameterSize("FibPho1", "DriveOn-1");
            //this.client.GetParameterValue(string gizmoName, string paramName);
            //this.client.GetParameterValues(string GizmoName, string ParamName, int Count = -1, int OffSet = 0);

            // writers 
            /*
            this.client.IssueTrigger("1");
            this.client.SetPersistMode("Best");
            this.client.SetCurrentSubject(string Name);
            this.client.SetCurrentUser(string Name, string pwd = "");
            this.client.SetCurrentExperiment(string Name);
            this.client.SetCurrentTank(string Name);
            this.client.SetCurrentBlock(string Name);
            this.client.CreateTank(string path);
            this.client.CreateSubject(string Name, string desc = "", string icon = "mouse");
            this.client.SetParameterValue(string GizmoName, string ParamName, dynamic Value);
            this.client.SetParameterValues(string GizmoName, string ParamName, List < dynamic > values, int offSet = 0);
            this.client.AppendExperimentMemo(string Experiment, string Memo);
            this.client.AppendSubjectMemo(string Subject, string Memo);
            this.client.AppendUserMemo(string User, string Memo);
            */
        }
    }
}
