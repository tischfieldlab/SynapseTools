using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SynapseTools
{

    public enum SynapseMode
    {
        Error,
        Idle,
        Standby,
        Preview,
        Record
    }

    public class ParameterInfo
    {
        public string Name;
        public string Unit;
        public dynamic Min;
        public dynamic Max;
        public Type Type;
        public ParameterAccess Access;
        public bool IsArray;
        public int ArraySize;
    }
    public class SystemStatus
    {
        public string DataRate;
        public int Errors;
        public string RecDur;
        public int SysLoad;
        public int UiLoad;
    }
    [Flags]
    public enum ParameterAccess
    {
        Read = 1,
        Write = 2
    }

    public class SynapseClient
    {
        private static SynapseClient self;
        private static HttpClient synClient;

        protected SynapseClient(string Hostname = "localhost", int Port = 24414)
        {
            if (synClient is null)
            {
                synClient = new HttpClient();
                synClient.BaseAddress = new UriBuilder("http", Hostname, Port).Uri;
                synClient.DefaultRequestHeaders.Accept.Clear();
                synClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            JsonConvert.DefaultSettings = (() =>
            {
                var Settings = new JsonSerializerSettings();
                Settings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy(), true));
                Settings.NullValueHandling = NullValueHandling.Ignore;
                return Settings;
            });
        }
        public static SynapseClient GetClient()
        {
            if (self is null)
            {
                self = new SynapseClient();
            }
            return self;
        }

        protected async Task<T> Get<T>(string Endpoint, string Token = null)
        {
            Console.WriteLine("Get: " + Endpoint);
            HttpResponseMessage response = await synClient.GetAsync(Endpoint);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response:\n" + responseBody);

            var responseObject = JObject.Parse(responseBody);
            if (Token != null)
            {
                var tknObject = responseObject.GetValue(Token);
                return tknObject.ToObject<T>();
            }
            return responseObject.ToObject<T>();
        }
        protected async Task<T> Options<T>(string endpoint, string token = null)
        {
            Console.WriteLine("OPTIONS: " + endpoint);
            var request = new HttpRequestMessage(HttpMethod.Options, endpoint);
            HttpResponseMessage response = await synClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response:\n" + responseBody);

            var responseObject = JObject.Parse(responseBody);
            if (token != null)
            {
                var tknObject = responseObject.GetValue(token);
                return tknObject.ToObject<T>();
            }
            return responseObject.ToObject<T>();
        }

        protected async Task Put<T>(string endpoint, T data)
        {
            Console.WriteLine("PUT: " + endpoint + "; " + data.ToString());
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            HttpResponseMessage response = await synClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }


        public SynapseMode Mode
        {
            get { return this.Get<SynapseMode>("/system/mode", "mode").Result; }
            set { this.Put("/system/mode", new { mode = value }).Wait(); }
        }





        public void IssueTrigger(string id)
        {
            this.Put<string>("/trigger/" + id, null).Wait();
		}
        
        public SystemStatus GetSystemStatus()
        {
            return this.Get<SystemStatus>("/system/status").Result;
        }

        public List<String> GetPersistModes()
        {
            return this.Options<List<String>>("/system/persist", "modes").Result;
        }

        public string GetPersistMode()
        {
            return this.Get<string>("/system/persist", "mode").Result;
        }

        public void SetPersistMode(string modeStr)
        {
            this.Put("/system/persist", new { mode = modeStr }).Wait();
        }
        
	    public Dictionary<string, float> GetSamplingRates()
        {
            return this.Get<Dictionary<string, float>>("/processor/samprate").Result;
		}
        public List<string> GetKnownSubjects()
        {
            return this.Options<List<string>>("/subject/name", "subjects").Result;
        }

        public List<string> GetKnownUsers()
        {
            return this.Options<List<string>>("/user/name", "users").Result;
        }

        public List<string> GetKnownExperiments()
        {
            return this.Options<List<string>>("/experiment/name", "experiments").Result;
        }

        public List<string> GetKnownTanks()
        {
            return this.Options<List<string>>("/tank/name", "tanks").Result;
        }

        public List<string> GetKnownBlocks()
        {
            return this.Options<List<string>>("/block/name", "blocks").Result;
        }

        public string GetCurrentSubject()
        {
            return this.Get<string>("/subject/name", "subject").Result;
        }

        public string GetCurrentUser()
        {
            return this.Get<string>("/user/name", "user").Result;
        }

        public string GetCurrentExperiment()
        {
            return this.Get<string>("/experiment/name", "experiment").Result;
        }

        public string GetCurrentTank()
        {
            return this.Get<string>("/tank/name", "tank").Result;
        }

        public string GetCurrentBlock()
        {
            return this.Get<string>("/block/name", "block").Result;
        }

        public void SetCurrentSubject(string Name)
        {
            this.Put("/subject/name", new { subject = Name }).Wait();
        }

        public void SetCurrentUser(string Name, string pwd = "")
        {
            this.Put("/user/name", new { user = Name, pwd = pwd }).Wait();
        }

        public void SetCurrentExperiment(string Name)
        {
            this.Put("/experiment/name", new { experiment = Name }).Wait();
        }

        public void SetCurrentTank(string Name)
        {
            this.Put("/tank/name", new { tank = Name }).Wait();
        }

        public void SetCurrentBlock(string Name)
        {
            this.Put("/block/name", new { block = Name }).Wait();
        }

        public void CreateTank(string path)
        {
            this.Put("/tank/path", new { tank = path }).Wait();
        }

        public void CreateSubject(string Name, string desc = "", string icon = "mouse")
        {
            this.Put("/subject/name/new", new { subject = Name, desc = desc, icon = icon }).Wait();
        }

        public List<string> GetGizmoNames()
        {
            return this.Options<List<string>>("/gizmos", "gizmos").Result;
        }

        public List<string> GetParameterNames(string GizmoName)
        {
            return this.Options<List<string>>($"/params/{GizmoName}", "parameters").Result;
        }
        public ParameterInfo GetParameterInfo(string GizmoName, string ParamName)
        {
            var info = this.Get<List<string>>($"/params/info/{GizmoName}.{ParamName}", "info").Result;
            var pi = new ParameterInfo() { Name = info[0], Unit = info[1] };

            //type and min/max
            switch (info[5])
            {
                case "Float":
                    pi.Type = typeof(float);
                    pi.Min = float.Parse(info[2]);
                    pi.Max = float.Parse(info[3]);
                    break;
                case "Int":
                    pi.Type = typeof(int);
                    pi.Min = int.Parse(info[2]);
                    pi.Max = int.Parse(info[3]);
                    break;
                case "Logic":
                    pi.Type = typeof(bool);
                    pi.Min = int.Parse(info[2]);
                    pi.Max = int.Parse(info[3]);
                    break;
            }

            //Array info
            if(info[6] == "No")
            {
                pi.IsArray = false;
            }
            else if(info[6] == "Yes")
            {
                pi.IsArray = true;
                pi.ArraySize = -1;
            }
            else
            {
                pi.IsArray = true;
                pi.ArraySize = int.Parse(info[6]);
            }

            //Access info
            if (info[4].Contains("Read"))
            {
                pi.Access &= ParameterAccess.Read;
            }
            if (info[4].Contains("Write"))
            {
                pi.Access &= ParameterAccess.Write;
            }
            return pi;
        }

        public int GetParameterSize(string GizmoName, string ParamName)
        {
            return this.Get<int>($"/params/size/{GizmoName}.{ParamName}", "value").Result;
        }
	   /* public void GetParameterValue(string gizmoName, string paramName){

            value = this.Get<float>($"/params/{gizmoName}.{paramName}", "value")

		    didConvert = [True]
    		retval = this.parseJsonFloat(value, didConvert)
		
		    if not didConvert[0]:
			    retval = value)

		    return retval
		}
        public void GetParameterValues(string GizmoName, string ParamName, int Count = -1, int OffSet = 0)
		{

            if (Count == -1) {
                try
                {
                    Count = this.GetParameterSize(GizmoName, ParamName)

                } catch {
					Count = 1;
                }
            }

			values = this.Get<List<("/params/{GizmoName}.{ParamName}"),
								"values",
								{"count" : count, "offSet" : offSet}))

			// HACK to pass variable by reference
			didConvert = [True]
	        retval = this.parseJsonFloatList(values, didConvert)
				
			if not didConvert[0]:
				retval = values)
					
			return retval[:min(count, len(retval))]
		}*/
        public void SetParameterValue(string GizmoName, string ParamName, dynamic Value)
        {
            this.Put($"/params/{GizmoName}.{ParamName}", new { value = Value }).Wait();
        }
        public void SetParameterValues(string GizmoName, string ParamName, List<dynamic> values, int offSet = 0)
        {
            this.Put($"/params/{GizmoName}.{ParamName}", new { offSet = offSet, values = values }).Wait();
        }
        public void AppendExperimentMemo(string Experiment, string Memo)
        {
            this.Put("/experiment/notes", new { experiment = Experiment, memo = Memo }).Wait();
        }
        public void AppendSubjectMemo(string Subject, string Memo)
        {
            this.Put("/subject/notes", new { subject = Subject, memo = Memo }).Wait();
        }
        public void AppendUserMemo(string User, string Memo)
        {
            this.Put("/user/notes", new { user = User, memo = Memo }).Wait();
        }

    }
}
