using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Language
{
    /// <summary>
    /// Method to change the language
    /// </summary>
    class Language_VM
    {
        private string currentLanguagePath;
        private string languagePath;
        public List<string> availableLanguages;
        public JObject objLanguage { get; set; }

        /// <summary>
        /// Constructor of the language class that atribute the value of the path to the language path
        /// </summary>
        public Language_VM()
        {
            currentLanguagePath = @"../../../../config/json/param_global.json";
            availableLanguages = new List<string>() { "FR", "EN" };

        }

        /// <summary>
        /// The method to change the current language
        /// </summary>
        /// <param name="currentLanguage"></param>
        public void changeCurrentLanguage(string currentLanguage)
        {
            JObject jsonObjLanguage = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(currentLanguagePath)) as JObject;
            JToken jtokenLanguage = jsonObjLanguage.SelectToken("currentLanguage");
            jtokenLanguage.Replace(currentLanguage);
            File.WriteAllText(currentLanguagePath, Convert.ToString(jsonObjLanguage));
            loadCurrentLanguage();
        }

        /// <summary>
        /// The method to load the current language stored in the json file
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
