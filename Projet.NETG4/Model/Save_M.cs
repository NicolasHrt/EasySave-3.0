using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

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
        public int FileNumber { get; set; }
        public string path_listExt = @"../../../../config/json/param_global.json";
        public JObject jsonObject;
        
        public Save_M()
        {
            string str_ext = File.ReadAllText(path_listExt);
            this.DateSave = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            this.jsonObject =  Newtonsoft.Json.JsonConvert.DeserializeObject(str_ext) as JObject;
        }

        /// <summary>
        /// Run a save and return some information data relative to the save
        /// </summary>
        /// <param name="sourcePath">Source directory</param>
        /// <param name="targetPath">Target directory</param>
        /// <param name="name">Name of the Backup Job</param>
        /// <returns>saveListReturn</returns>
        public abstract Dictionary<string, string> RunSave(string sourcePath, string targetPath, string name);

        public string getKeyCript() {
            string keyCrypt = "";
            JToken jtokenKey = jsonObject.SelectToken("key");

            foreach (JProperty jsonKey in jtokenKey)
            {
                keyCrypt = Convert.ToString(jsonKey.Value);
            }
            return keyCrypt;
        }

        public List<string> getExtCrypt()
        {
            List<string> ext_to_crypt = new List<string>();

            JToken jtokenExt = jsonObject.SelectToken("extensions");
            foreach (JProperty jsonExtension in jtokenExt)
            {
                ext_to_crypt.Add(Convert.ToString(jsonExtension.Value));
            }

            return ext_to_crypt;
        }

        public Dictionary<string, string> fill_state_list(string name, int totalFileNb, double totalFileSize, int remainingFiles, float progression,string state)
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

        public bool check_process(string fileName)
        {
            Process[] allProcess = Process.GetProcesses();
            List<string> listProcess = new List<string>();
            List<string> listSoftware = new List<string>();

            JToken jtokenExt = jsonObject.SelectToken("software");
            foreach (JProperty jsonExtension in jtokenExt)
            {
                listSoftware.Add(Convert.ToString(jsonExtension.Value));
            }

            bool check = false;
            foreach (string software in listSoftware)
            {
                if (fileName == software)
                {
                    foreach (Process runningProcess in allProcess)
                    {
                        if (runningProcess.ProcessName == software)
                        {
                            listProcess.Add(Convert.ToString(software));
                            check = true;
                        }
                    }
                }
            }

            if (check)
            {
                Console.WriteLine("Error : une application que vous tentez de copier est ouverte ");
                foreach (string softwareOpen in listProcess)
                {
                    Console.WriteLine("\t - " + softwareOpen + " est ouvert ! ");
                }
            }


            return check;
        }

    }
}
