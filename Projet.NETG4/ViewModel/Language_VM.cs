using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Language
{
    /// <summary>
    /// Class which manage languages 
    /// </summary>
    class Language_VM
    {
        string currentLanguagePath;
        string languagePath;
        public List<string> availableLanguages;
        public JObject objLanguage { get; set; }

        /// <summary>
        /// Constructor of the class
        /// </summary>
        public Language_VM()
        {
            currentLanguagePath = @"../../../../config/languages/currentLanguage.json";
            availableLanguages = new List<string>() { "FR", "EN" };

        }
        /// <summary>
        /// Change the current language to use
        /// </summary>
        /// <param name="currentLanguage"> Attribute which contain the current language to use</param>
        public void changeCurrentLanguage(string currentLanguage)
        {
            JObject jsonObjLanguage = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(currentLanguagePath)) as JObject;
            JToken jtokenLanguage = jsonObjLanguage.SelectToken("currentLanguage");
            jtokenLanguage.Replace(currentLanguage);
            File.WriteAllText(currentLanguagePath, Convert.ToString(jsonObjLanguage));
            loadCurrentLanguage();
        }
        /// <summary>
        /// Load the objLanguage with the good path
        /// </summary>
        public void loadCurrentLanguage()
        {
            JObject jsonObjCurrentLanguage = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(currentLanguagePath)) as JObject;
            JToken jtokenLanguage = jsonObjCurrentLanguage.SelectToken("currentLanguage");

            switch (Convert.ToString(jtokenLanguage))
            {
                case "FR":
                    languagePath = @"../../../../config/languages/FR.json";
                    break;
                case "EN":
                    languagePath = @"../../../../config/languages/EN.json";
                    break;
            }

            objLanguage = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(languagePath)) as JObject;
        }
        
    }
}
