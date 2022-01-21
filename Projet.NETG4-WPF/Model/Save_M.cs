using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;

namespace SaveModel
{
    /// <summary> 
    /// Base class for save
    /// </summary> 

    abstract class Save_M
    {
        public string SaveType { get; set; }
        public string Name { get; set; }
        public double FileSize { get; set; }
        public string DateSave { get; set; }
        public TimeSpan TransferTime { get; set; }
        public float FileNumber { get; set; }
        public string path_listExt = @"../../../../config/json/param_global.json";
        public JObject jsonObject;
        protected static bool priorityFlag { get; set; }
        protected List<string> priorityFilesList;

        public Save_M()
        {
            priorityFlag = false;
            priorityFilesList = new List<string>();
            string str_ext = File.ReadAllText(path_listExt);
            this.DateSave = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            this.jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(str_ext) as JObject;
        }

        /// <summary>
        /// Run a save and return some information data relative to the save
        /// </summary>
        /// <param name="sourcePath">Source directory</param>
        /// <param name="targetPath">Target directory</param>
        /// <param name="name">Name of the Backup Job</param>
        /// <returns>saveListReturn</returns>
        public abstract Dictionary<string, string> RunSave(string sourcePath, string targetPath, string name, object sender, DoWorkEventArgs e);

        public abstract List<string> priority_fileSorted(string sourcePath, string targetPath="");

        public string getKeyCript() {
            string keyCrypt = "";
            JToken jtokenKey = jsonObject.SelectToken("key");

            foreach (JProperty jsonKey in jtokenKey)
            {
                keyCrypt = Convert.ToString(jsonKey.Value);
            }
            return keyCrypt;
        }

        /// <summary>
        /// Method to get the list of all extension we have to crypt save in a json file
        /// </summary>
        /// <returns></returns>
        public List<string> getExtCrypt()
        {
            List<string> ext_to_crypt = new List<string>();

            JToken jtokenExt = jsonObject.SelectToken("encryptExtensions");
            foreach (JProperty jsonExtension in jtokenExt)
            {
                ext_to_crypt.Add(Convert.ToString(jsonExtension.Value));
            }

            return ext_to_crypt;
        }


        public List<string> getExtPrio()
        {
            List<string> ext_prio = new List<string>();

            JToken jtokenExt = jsonObject.SelectToken("prioExtension");
            foreach (JProperty jsonExtension in jtokenExt)
            {
                ext_prio.Add(Convert.ToString(jsonExtension.Value));
            }

            return ext_prio;
        }

        /// <summary>
        /// Fill the list to we send to the log state
        /// </summary>
        /// <param name="name">Name of the backup Job</param>
        /// <param name="totalFileNb">Total number of file in the save</param>
        /// <param name="totalFileSize">Total size of the file in the save</param>
        /// <param name="remainingFiles">Number of remaining files in the current save</param>
        /// <param name="progression">The progression of the save</param>
        /// <param name="state">The state of the Backup Job End or Active</param>
        /// <returns>Return the list with all information for the log state when the save is running</returns>
        public Dictionary<string, string> fill_state_list(string name, float totalFileNb, double totalFileSize, float remainingFiles, float progression,string state)
        {
            Dictionary<string, string> log_state_list = new Dictionary<string, string>();

            log_state_list.Add("name", name);
            log_state_list.Add("totalFileNb", Convert.ToString(totalFileNb));
            log_state_list.Add("totalFileSize", Convert.ToString(totalFileSize));
            log_state_list.Add("remainingFiles", Convert.ToString(remainingFiles));
            log_state_list.Add("progression", Convert.ToString(progression));
            log_state_list.Add("state", state);

            return log_state_list;
        }

        /// <summary>
        /// Fill the list to we send to the daily log
        /// </summary>
        /// <param name="name">Name of the backup Job</param>
        /// <param name="sourcePath">Source path of the repository that we want to save</param>
        /// <param name="targetPath">Taget path of the repository that we want to save</param>
        /// <param name="FileSize">Total of the save</param>
        /// <param name="TransferTime">The time that the save took</param>
        /// <param name="DateSave">The date of the save</param>
        /// <param name="tempsXor">The time that the encryption took</param>
        /// <returns>A list with all information for the daily log</returns>
        public Dictionary<string, string> fill_daily_list(string name, string sourcePath, string targetPath, double FileSize, TimeSpan TransferTime, string DateSave,string tempsXor)
        {
            Dictionary<string, string> log_daily_list = new Dictionary<string, string>();

            log_daily_list.Add("Name", Convert.ToString(name));
            log_daily_list.Add("FileSource", Convert.ToString(sourcePath));
            log_daily_list.Add("FileTarget", Convert.ToString(targetPath));
            log_daily_list.Add("FileSize", Convert.ToString(FileSize));
            log_daily_list.Add("FileTransferTime", Convert.ToString(TransferTime));
            log_daily_list.Add("Time", Convert.ToString(DateSave));
            log_daily_list.Add("timeCrypt", tempsXor);

            return log_daily_list;
        }


        /// <summary>
        /// Method that check if a process that we have save in the json file is running 
        /// </summary>
        /// <param name="fileName">Name of the file being processed when saving</param>
        /// <returns>Return a bool if the file is a process in the blocked list</returns>
        public bool check_process()
        {
            Process[] allProcess = Process.GetProcesses();
            List<string> listProcess = new List<string>();
            List<string> listSoftware = new List<string>();
            bool running = false;

            JToken jtokenExt = jsonObject.SelectToken("software");
            foreach (JProperty jsonExtension in jtokenExt)
            {
                listSoftware.Add(Convert.ToString(jsonExtension.Value));
            }

            foreach (Process runningProcess in allProcess)
            {
                if (listSoftware.Any(runningProcess.ProcessName.Contains))
                {
                    listProcess.Add(runningProcess.ProcessName);
                    running = true;
                }
            }

            return running;
        }
    }
}
