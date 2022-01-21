using System;
using System.Collections.Generic;
using EventListner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.IO;


namespace log_models
{
    /// <summary>
    /// Definition of the log daily class
    /// </summary>
    class Log_daily : IEventListner
    {   

        string FileJson = @"../../../../config/json/log daily/";

        /// <summary>
        /// Method update called from the event manager
        /// </summary>
        /// <param name="type"></param>
        /// <param name="listUpdate_daily"></param>
        public override void Update(string type, Dictionary<string, string> listUpdate_daily)
        {
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

                    var last_log = JsonConvert.SerializeObject(result_list, Formatting.Indented);

                    File.WriteAllText(path, last_log);
                }

                else
                {
                    //Append the new new json object to the existing file
                    string jsonString = File.ReadAllText(path);
                    JObject jsonObject = JsonConvert.DeserializeObject(jsonString) as JObject;

                    jsonString = jsonString.Remove(jsonString.Length - 2) + ",";

                    int count = jsonObject.Count;
                    int plusOne = count + 1;

                    result_list.Add(Convert.ToString(plusOne), listUpdate_daily);

                    var last_log = JsonConvert.SerializeObject(result_list, Formatting.Indented);

                    jsonString += last_log.Remove(0, 1);
                    File.WriteAllText(path, jsonString);

                }

            }
        }
    }
}
