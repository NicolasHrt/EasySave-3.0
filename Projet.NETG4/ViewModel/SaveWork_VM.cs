using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using SaveModel;
using EventManager;
using System.Linq;
using log_models;
using Language;
using System.Data;

namespace SaveWork
{
    /// <summary>
    /// Class with all method for the CRUD of the Backup Job and verification of the user input
    /// </summary>
    public class SaveWork_VM
    {
        //Declaration of objects
        SaveWork_M SaveWork;
        SaveFactory_M FactorySave;
        Save_M Save;
        string FileJson;
        EventManager_M eventSavework;
        Log_daily obj_logDaily;
        Log_state obj_logState;
        Language_VM Language;
        JObject jsonObject;
        string jsonString;


        //Constructor to instantiate objects
        public SaveWork_VM()
        {
            SaveWork = new SaveWork_M();
            // This path must correspond to the path where the Json is on your computer
            FileJson = @"../../../../config/json/SaveWorks.json";

            obj_logDaily = new Log_daily();
            obj_logState = new Log_state();
            FactorySave = new SaveFactory_M();
            eventSavework = new EventManager_M();
            Save = null;

            eventSavework.Register(obj_logDaily);
            eventSavework.Register(obj_logState);

            Language = new Language_VM();

        }



        /// <summary>
        /// Method to create a Backup Job
        /// </summary>
        /// <param name="parameters">Name of the new Backup Job</param>
        public void CreateSaveWork(Dictionary<string, string> parameters)
        {
            Language.loadCurrentLanguage();
            UpdateJson();

            Dictionary<string, Dictionary<string, string>> newSaveWork = new Dictionary<string, Dictionary<string, string>>();
            //Search for a empty space and put info into json
            if (VerifValidSaveWork(parameters["Name"], parameters["SourceRepo"], parameters["TargetRepo"]))
            {
                string id = "Id" + DateTime.Now.Ticks.ToString();
                newSaveWork.Add(id, parameters);

                var lastWork = JsonConvert.SerializeObject(newSaveWork, Formatting.Indented);
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



           /*     Dictionary<string, string> listUpdate = new Dictionary<string, string>();

                listUpdate.Add("name", Convert.ToString(jsonObject.SelectToken(i + ".Name")));
                listUpdate.Add("sourceRep", Convert.ToString(jsonObject.SelectToken(i + ".SourceRepo")));
                listUpdate.Add("targetRep", Convert.ToString(jsonObject.SelectToken(i + ".TargetRepo")));
                listUpdate.Add("saveType", Convert.ToString(jsonObject.SelectToken(i + ".SaveType")));*/

                eventSavework.Notify("create", parameters);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Language.objLanguage.SelectToken("create"));
                Console.ResetColor();

            }
            //if the user input are not valid the backjob is not created and it print an error in the console

            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error_giveninformations_invalid"));
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Method to run one or more SaveWorks
        /// </summary>
        /// <param name="name">Name of the Backup Job</param>
        public void RunSaveWork(List<string> names)
        {
            Language.loadCurrentLanguage();
            UpdateJson();
            string SourceRepoJson = "";
            string TargetRepoJson = "";
            string SaveType = "";
            bool exist = false;
            Dictionary<string, string> listUpdate = new Dictionary<string, string>();

            foreach (string name in names)
            {
                foreach (JProperty work in (JToken)jsonObject)
                {
                    if (Convert.ToString(work.Value["Name"]) == name)
                    {
                        SaveType = Convert.ToString(work.Value["SaveType"]);
                        SourceRepoJson = Convert.ToString(work.Value["SourceRepo"]);
                        TargetRepoJson = Convert.ToString(work.Value["TargetRepo"]);
                        Save = FactorySave.makeSave(SaveType);

                        //Take data from runsave for daily logs
                        listUpdate = Save.RunSave(SourceRepoJson, TargetRepoJson, name);

                        eventSavework.Notify("run_daily", listUpdate);
                        listUpdate.Clear();
                        exist = true;
                        break;
                    }
                    else { exist = false; }
                }
                if (exist == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Language.objLanguage.SelectToken("error_savework_not_exists") + " : " + name);
                    Console.ResetColor();
                }
            }

        }



        //if the name of a save exist it will run the save and send the information of the save like transfert time to the daily log
        /*if (Save != null)
        {
            listUpdate = Save.RunSave(SourceRepoJson, TargetRepoJson, name);
            if (listUpdate["Name"] != "error")
            {
                eventSavework.Notify("run_daily", listUpdate);
                listUpdate.Clear();
            }
            Save = null;
        }*/
        //if the name doesn't exist print this error


        /// <summary>
        /// Method to delete a Backup Job
        /// </summary>
        /// <param name="Name">Name of the Backup Job to delete</param>
        public void DeleteSaveWorks(string Name)
        {
            Dictionary<string, string> update_logState = new Dictionary<string, string>();
            update_logState["name"] = Name;
            Language.loadCurrentLanguage();
            UpdateJson();

            bool check = false;
            //Search where is the backup Job with the name that the user put in the console 

            foreach (JProperty work in (JToken)jsonObject)
            {
                if (Convert.ToString(work.Value["Name"]) == Name)
                {
                    check = jsonObject.Remove(work.Name);

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
            //If the Backup job has been deleted print this message
            if (check)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Language.objLanguage.SelectToken("delete"));
                Console.ResetColor();
            }
            //If the Backup job doesn't exist print this error
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error_delete_savework"));
                Console.ResetColor();
            }

            eventSavework.Notify("remove", update_logState);


        }


        /// <summary>
        /// Method to modify a Backup Job
        /// </summary>
        /// <param name="Name">Name of Backup Job to modify</param>
        /// <param name="parameters">Dictionary that contain all parameter to modify</param>
        public void ModifySaveWork(string Name, Dictionary<string, string> parameters)
        {
            Language.loadCurrentLanguage();
            UpdateJson();
            //Dictionary<string, string> listUpdate = new Dictionary<string, string>();

            //Verify if the user input is valid for the modification
            if (VerifValidSaveWork(Name,parameters["Name"], parameters["SourceRepo"], parameters["TargetRepo"]))
            {
                foreach (JProperty work in (JToken)jsonObject)
                {
                    if (Convert.ToString(work.Value["Name"]) == Name)
                    {
                        JToken jtokenName = jsonObject.SelectToken(work.Name + ".Name");
                        JToken jtokenSourceRepo = jsonObject.SelectToken(work.Name + ".SourceRepo");
                        JToken jtokenTargetRepo = jsonObject.SelectToken(work.Name + ".TargetRepo");
                        JToken jtokenSaveType = jsonObject.SelectToken(work.Name + ".SaveType");

                        jtokenName.Replace(parameters["Name"]);
                        jtokenSourceRepo.Replace(parameters["SourceRepo"]);
                        jtokenTargetRepo.Replace(parameters["TargetRepo"]);
                        jtokenSaveType.Replace(VerifSaveType(parameters["SaveType"]));

                        File.WriteAllText(FileJson, Convert.ToString(jsonObject));

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(Language.objLanguage.SelectToken("modify"));
                        Console.ResetColor();
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Language.objLanguage.SelectToken("error_modify_savework"));
                        Console.ResetColor();
                    }
                }
            }
        }

        /// <summary>
        /// Display all Backup Job
        /// </summary>
        public DataTable DisplaySaveWorks()
        {
            UpdateJson();
            DataTable DataJob = new DataTable();
            Dictionary<int, Dictionary<string, string>> JsonWorks = new Dictionary<int, Dictionary<string, string>>();
            int i = 1;
            if (jsonString != "")
            {
                DataJob.Columns.Add("Name");
                DataJob.Columns.Add("SourceRepo");
                DataJob.Columns.Add("TargetRepo");
                DataJob.Columns.Add("Type of Save");

                foreach (JProperty work in (JToken)jsonObject)
                {

                    DataJob.Rows.Add(Convert.ToString(work.Value["Name"]), Convert.ToString(work.Value["SourceRepo"]), Convert.ToString(work.Value["TargetRepo"]), Convert.ToString(work.Value["SaveType"]));
                    //Dictionary<string, string> JsonWork = new Dictionary<string, string>();
                    //JsonWork.Add("Name", Convert.ToString(work.Value["Name"]));
                    //JsonWork.Add("SourceRepo", Convert.ToString(work.Value["SourceRepo"]));
                    //JsonWork.Add("TargetRepo", Convert.ToString(work.Value["TargetRepo"]));
                    //JsonWork.Add("SaveType", Convert.ToString(work.Value["SaveType"]));
                    //JsonWorks.Add(i, JsonWork);
                    //JsonWork.Clear();
                    //i++;
                    

                }
                return DataJob;
            }
            else
            {
                return DataJob;
            }
            
        }

        /// <summary>
        /// Verify the SaveType with user input
        /// </summary>
        /// <param name="inputSavetype">backup type number</param>
        /// <returns>The SaveType that the user choose</returns>
        public string VerifSaveType(string inputSavetype)
        {
            //transform the user's entry into the backup type in order to always have a valid backup type
            switch (inputSavetype)
            {
                case "1":
                    inputSavetype = "Complete";
                    break;
                case "2":
                    inputSavetype = "Diff";
                    break;
                default:
                    inputSavetype = "Complete";
                    break;
            }

            return inputSavetype;
        }

        /// <summary>
        /// Verify the Validity of the user input
        /// </summary>
        /// <param name="Name">Name of the Backup Job</param>
        /// <param name="SourceRepo">Source Repository of the Backup Job</param>
        /// <param name="TargetRepo">Target Repository of the Backup Job</param>
        /// <returns>A bool that confirm the validity of the user input</returns>
        public bool VerifValidSaveWork(string Name, string SourceRepo, string TargetRepo)
        {
            string jsonString = File.ReadAllText(FileJson);
            JObject jsonObject = JsonConvert.DeserializeObject(jsonString) as JObject;
            bool valid = true;

            //Verify if the name already exist in json
            if (jsonString != "")
            {
                foreach (JProperty work in (JToken)jsonObject)
                {
                    if (Convert.ToString(work.Value["Name"]) == Name)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Language.objLanguage.SelectToken("error_saveworkName_exists") + Name);
                        Console.ResetColor();
                        valid = false;
                        break;
                    }
                }
            }

            //Verify if the name is empty or not 
            if (Name == "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error_name_empty"));
                Console.ResetColor();
                valid = false;
            }

            //Verify if the SourceDirectory exist
            if (!Directory.Exists(SourceRepo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error_sourcepath_not_exists") + SourceRepo);
                Console.ResetColor();
                valid = false;
            }

            //Verify if the TargetDirectory exist
            if (!Directory.Exists(TargetRepo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error_targetpath_not_exists") + TargetRepo);
                Console.ResetColor();
                valid = false;
            }

            //return the validity of the user input in a bool 
            return valid;
        }

        //Verif when the Backup job is modify
        public bool VerifValidSaveWork(string NameToModif, string Name, string SourceRepo, string TargetRepo)
        {
            UpdateJson();
            bool valid = true;

            //Verify if the name already exist in json
            if (NameToModif != Name)
            {
                if (jsonString != "")
                {
                    foreach (JProperty work in (JToken)jsonObject)
                    {
                        if (Convert.ToString(work.Value["Name"]) == Name)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(Language.objLanguage.SelectToken("error_saveworkName_exists") + Name);
                            Console.ResetColor();
                            valid = false;
                            break;
                        }
                    }
                }
            }


            //Verify if the name is empty or not 
            if (Name == "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error_name_empty"));
                Console.ResetColor();
                valid = false;
            }

            //Verify if the SourceDirectory exist
            if (!Directory.Exists(SourceRepo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error_sourcepath_not_exists") + SourceRepo);
                Console.ResetColor();
                valid = false;
            }

            //Verify if the TargetDirectory exist
            if (!Directory.Exists(TargetRepo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error_targetpath_not_exists") + TargetRepo);
                Console.ResetColor();
                valid = false;
            }

            //return the validity of the user input in a bool 
            return valid;
        }
        public void UpdateJson()
        {
            this.jsonString = File.ReadAllText(FileJson);
            this.jsonObject = JsonConvert.DeserializeObject(jsonString) as JObject;
        }
    }
}

