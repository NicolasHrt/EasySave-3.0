using System;
using System.Collections.Generic;
using EventListner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;

using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace log_models
{
    /// <summary>
    /// Definition of the log daily class
    /// </summary>
    class Log_daily : IEventListner
    {
        public DateTime date { get; set; }
        public TimeSpan TransferTime { get; set; }

        private string FileJson;
        private string FormatLog;
        private static Mutex mut = new Mutex();

        /// <summary>
        /// Method update called from the event manager
        /// </summary>
        /// <param name="type"></param>
        /// <param name="listUpdate_daily"></param>
        public override void Update(string type, Dictionary<string, string> listUpdate_daily)
        {
            mut.WaitOne();

            checkFormat();
            //Definition of the run type event
            if (type == "run_daily")
            {
                Dictionary<string, Dictionary<string, string>> result_list = new Dictionary<string, Dictionary<string, string>>();

                string filename = "log_daily_" + string.Format("{0:yyyy-MM-dd}", DateTime.Now);
                string path = FileJson + filename;

                // Check if a daily log exist for the current day
                if (!File.Exists(path))
                {
                    //If not, create a new file and append a new json object to it
                    result_list.Add(Convert.ToString("1"), listUpdate_daily);


                    var last_log = JsonConvert.SerializeObject(result_list, Newtonsoft.Json.Formatting.Indented);
                    //verify in which format to save the logs
                    if (FormatLog == "json")
                    {
                        File.WriteAllText(path, last_log);

                    }
                    else if (FormatLog == "xml")
                    {
                        XNode node = JsonConvert.DeserializeXNode(last_log, "XmlLog");
                        File.WriteAllText(path, node.ToString());
                    }
                }

                else
                {
                    //Append the new new json object to the existing file
                    string logString = File.ReadAllText(path);

                    if (FormatLog == "json")
                    {
                        JObject jsonObject = JsonConvert.DeserializeObject(logString) as JObject;

                        logString = logString.Remove(logString.Length - 2) + ",";

                        int count = jsonObject.Count;
                        int plusOne = count + 1;

                        result_list.Add(Convert.ToString(plusOne), listUpdate_daily);

                        var last_log = JsonConvert.SerializeObject(result_list, Newtonsoft.Json.Formatting.Indented);
                        logString += last_log.Remove(0, 1);
                        File.WriteAllText(path, logString);
                    }

                    //Append the new new xml object to the existing file
                    if (FormatLog == "xml")
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(logString);
                        string json = JsonConvert.SerializeXmlNode(doc);

                        JObject jsonObject = JsonConvert.DeserializeObject(json) as JObject;

                        json = json.Remove(json.Length - 2) + ",";

                        int count = jsonObject.Count;
                        int plusOne = count + 1;

                        result_list.Add(Convert.ToString(plusOne), listUpdate_daily);

                        var last_log = JsonConvert.SerializeObject(result_list, Newtonsoft.Json.Formatting.Indented);
                        json += last_log.Remove(0, 1);

                        XDocument node = JsonConvert.DeserializeXNode(json);

                        File.WriteAllText(path, node.ToString());
                    }

                }
            }
            mut.ReleaseMutex();

        }

        /// <summary>
        /// Method to check the log format save in a json file
        /// </summary>
        public void checkFormat()
        {
            string path_listExt = @"../../../../config/json/param_global.json"; ;

            string str_ext = File.ReadAllText(path_listExt);
            JObject ExtensionJobject = JsonConvert.DeserializeObject(str_ext) as JObject;
            this.FormatLog = ExtensionJobject.Value<string>("logformat");
            if (FormatLog == "json")
            {
                this.FileJson = "../../../../config/json/log daily/";
            }
            else if (FormatLog == "xml")
            {
                this.FileJson = "../../../../config/xml/log daily/";
            }
        }
    }
}
