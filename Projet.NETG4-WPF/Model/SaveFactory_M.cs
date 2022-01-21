using System;
using System.Collections.Generic;
using System.Text;

namespace SaveModel
{
    /// <summary>
    /// Factory to create differente types of save depend of the user input
    /// </summary>
    class SaveFactory_M
    {
        /// <summary>
        /// Method to instantiate an objet depend of the save type
        /// </summary>
        /// <param name="newSaveType">Save type of the Backup Job</param>
        /// <returns>instance of a save object</returns>
        public Save_M makeSave(string newSaveType)
        {

            if (newSaveType == "Complete")
            {
                return new SaveComplete_M();
            }

            else if (newSaveType == "Diff")
            {
                return new SaveDiff_M();
            }

            else { return null; }
        }
    }
}
