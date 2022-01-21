using System;
using System.Collections.Generic;
using EventListner;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace log_models
{
    /// <summary>
    /// Definition of the log state class
    /// </summary>
    class Log_state : IEventListner
    {

        private string FileJson;


        public Log_state()
        {
       
            FileJson = @"../../../../config/json/log_state.json";
        }


        /// <summary>
        /// Method update called from the event manager
        /// </summary>
        /// <param name="type"></param>
        /// <param name="listUpdate_state"></param>
        public override void Update(string type, Dictionary<string, string> listUpdate_state)
        {

            string jsonString = File.ReadAllText(FileJson);
            JObject jsonObject = JsonConvert.DeserializeObject(jsonString) as JObject;

            //Definition of the create type event
            if (type == "create")
            {
                Dictionary<string, Dictionary<string, string>> newStateLog = new Dictionary<string, Dictionary<string, string>>();
                listUpdate_state.Add("totalFileNb", "0");
                listUpdate_state.Add("totalFileSize", "0");
                listUpdate_state.Add("remainingFiles", "0");
                listUpdate_state.Add("progression", "0");
                listUpdate_state.Add("state", "END");


                string id = "Id" + DateTime.Now.Ticks.ToString();
                newStateLog.Add(id, listUpdate_state);

                var lastWork = JsonConvert.SerializeObject(newStateLog, Formatting.Indented);
                if (jsonString == "")
                {
                    jsonString += lastWork;
                }
                else
                {
                    jsonString = jsonString.Remove(jsonString.Length - 2) + ",";
                    jsonString += lastWork.Remove(0, 1);
                }

                File.WriteAllText(FileJson, jsonString);

            }

            //Definition of the run type event
            else if (type == "run")
            {
                //Loop on all slots
                foreach (JProperty log_state in (JToken)jsonObject)
                {
                    if (Convert.ToString(log_state.Value["Name"]) == listUpdate_state["name"])
                    {
                        JToken jtokenName = jsonObject.SelectToken(log_state.Name + ".Name");
                        JToken jtokenSourceRepo = jsonObject.SelectToken(log_state.Name + ".SourceRepo");
                        JToken jtokenTargetRepo = jsonObject.SelectToken(log_state.Name + ".TargetRepo");
                        JToken jtokenSaveType = jsonObject.SelectToken(log_state.Name + ".SaveType");
                        JToken jtokenTotalFileNb = jsonObject.SelectToken(log_state.Name + ".totalFileNb");
                        JToken jtokenTotalFileSize = jsonObject.SelectToken(log_state.Name + ".totalFileSize");
                        JToken jtokenRemainingFiles = jsonObject.SelectToken(log_state.Name + ".remainingFiles");
                        JToken jtokenProgression = jsonObject.SelectToken(log_state.Name + ".progression");
                        JToken jtokenState = jsonObject.SelectToken(log_state.Name + ".state");


                        jtokenTotalFileNb.Replace(listUpdate_state["totalFileNb"]);
                        jtokenTotalFileSize.Replace(listUpdate_state["totalFileSize"]);
                        jtokenRemainingFiles.Replace(listUpdate_state["remainingFiles"]);
                        jtokenProgression.Replace(listUpdate_state["progression"]);
                        jtokenState.Replace(listUpdate_state["state"]);


                        File.WriteAllText(FileJson, Convert.ToString(jsonObject));

                        break;
                    }
                }

            }

            //Definition of the remove type event
            else if (type == "remove")
            {
                //Loop on all slots
                foreach (JProperty log_state in (JToken)jsonObject)
                {
                    if (Convert.ToString(log_state.Value["Name"]) == listUpdate_state["name"])
                    {
                        jsonObject.Remove(log_state.Name);

                        if (jsonObject.Count == 0)
                        {
                            jsonString = "";
                            File.WriteAllText(FileJson, Convert.ToString(jsonString));
                        }
                        else
                        {
                            File.WriteAllText(FileJson, Convert.ToString(jsonObject));
                        }
                        break;
                    }

                }
            }

            //Definition for the modify type event
            else if (type == "modify")
            {
                foreach (JProperty log_state in (JToken)jsonObject)
                {
                    if (Convert.ToString(log_state.Value["Name"]) == listUpdate_state["FormerName"])
                    {
                        JToken jtokenName = jsonObject.SelectToken(log_state.Name + ".Name");
                        JToken jtokenSourceRepo = jsonObject.SelectToken(log_state.Name + ".SourceRepo");
                        JToken jtokenTargetRepo = jsonObject.SelectToken(log_state.Name + ".TargetRepo");
                        JToken jtokenSaveType = jsonObject.SelectToken(log_state.Name + ".SaveType");

                        jtokenName.Replace(listUpdate_state["Name"]);
                        jtokenSourceRepo.Replace(listUpdate_state["SourceRepo"]);
                        jtokenTargetRepo.Replace(listUpdate_state["TargetRepo"]);
                        jtokenSaveType.Replace(listUpdate_state["SaveType"]);

                        File.WriteAllText(FileJson, Convert.ToString(jsonObject));

                        break;
                    }
                }
            }
        }
    }
}
