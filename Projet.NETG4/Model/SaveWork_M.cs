using System;
using System.Collections.Generic;
using System.Text;

namespace SaveWork
{
    class SaveWork_M
    {
        //Attributes
        public string Name { get; set; }
        public string SourceRepo { get; set; }
        public string TargetRepo { get; set; }
        public string SaveType { get; set; }

        //Constructor
        public SaveWork_M()
        {
            this.Name = "SaveWork";
            this.SaveType = "Complete";
        }

    }

}
