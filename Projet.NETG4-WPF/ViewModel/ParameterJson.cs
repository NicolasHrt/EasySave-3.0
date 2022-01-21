using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace parameterJson
{
    /// <summary>
    /// The class for manipulate in the parameter json
    /// </summary>
    public class ParameterJson
    {
        private JObject PropertyJobject;
        private string str_ext;
        private string path_listExt;
        private string jsonproperty;

        /// <summary>
        /// Constructeur of the ParamterJson class where we assign the path to the variable
        /// </summary>
        /// <param name="jsonproperty">The property that we want to change in the parameter json</param>
        public ParameterJson(string jsonproperty)
        {
            this.jsonproperty = jsonproperty;
            path_listExt = @"../../../../config/json/param_global.json";
            str_ext = File.ReadAllText(path_listExt);
            PropertyJobject = JsonConvert.DeserializeObject(str_ext) as JObject;
        }

        /// <summary>
        /// The method to get mulitple parameter in a list
        /// </summary>
        /// <returns>List of all parameter that the user specifie</returns>
        public List<string> getParam()
        {
            List<string> propeties = new List<string>();

            JToken jtokenExt = PropertyJobject.SelectToken(jsonproperty);
            foreach (JProperty jsonExtension in jtokenExt)
            {
                propeties.Add(Convert.ToString(jsonExtension.Value));
            }
            return propeties;
        }

        /// <summary>
        /// The method to use if we have one parameter to send in one property
        /// </summary>
        /// <returns>The parameter that the user ask</returns>
        public string getOneParam()
        {
            string propertie;
            propertie = PropertyJobject.Value<string>(jsonproperty);
            updateJson();

            return propertie;
        }

        /// <summary>
        /// Add a value in a property in the parameter json 
        /// </summary>
        /// <param name="prop">The value the user want to add in a specific property</param>
        public void addParam(string prop)
        {

            JObject properties = (JObject)PropertyJobject[jsonproperty];
            string id = "Id" + DateTime.Now.Ticks.ToString();
            properties.Add(id, prop);
            updateJson();
        }

        /// <summary>
        /// Remove a property in the parameter json
        /// </summary>
        /// <param name="prop">The property that the user want to remove</param>
        public void removeParam(string prop)
        {
            JToken jtokenExt = PropertyJobject.SelectToken(jsonproperty);
            
            foreach (JProperty jsonExtension in jtokenExt)
            {
                if(prop == Convert.ToString(jsonExtension.Value))
                {
                    JObject properties = (JObject)PropertyJobject[jsonproperty];
                    properties.Remove(jsonExtension.Name);
                    break;

                }
            }
            updateJson();
        }

        /// <summary>
        /// The method to modify a specific value in a property
        /// </summary>
        /// <param name="prop">The new name of the value</param>
        public void modifyOneParam(string prop)
        {
            JToken jtokenFormatLog = PropertyJobject.SelectToken(jsonproperty);
            jtokenFormatLog.Replace(prop);
            updateJson();
        }

        /// <summary>
        /// The method to update the json after each opperation
        /// </summary>
        public void updateJson()
        {
            str_ext = JsonConvert.SerializeObject(PropertyJobject, Formatting.Indented);
            File.WriteAllText(path_listExt, str_ext);
        }
    }
}
