using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SaveModel;
using Language;

namespace SaveWork
{
    /// <summary>
    /// Class for display information on the console
    /// </summary>
    public class Menu
    {
        SaveWork_VM SaveWork;
        Language_VM Language;

        /// <summary>
        /// Constructor of the Menu method to instansiate a SaveWork_VM object
        /// </summary>
        public Menu()
        {
            SaveWork = new SaveWork_VM();
            Language = new Language_VM();
        }

        /// <summary>
        /// Main method for display all the action that the user can do
        /// </summary>
        public void DisplayMenu() {
            // Display title at the C# console.
            Console.WriteLine("\n");
            Console.WriteLine("\t\t\t\t\t\t       ██████\r");
            Console.WriteLine("\t\t\t\t\t\t     ██      ██\r");
            Console.WriteLine("\t\t\t\t\t\t    █          █\r");
            Console.WriteLine("\t\t\t\t\t\t   █            █\r");
            Console.WriteLine("\t\t\t\t\t\t   █        █ █ █\r");
            Console.WriteLine("\t\t\t\t\t\t  █  ███    █ █  █\r");
            Console.WriteLine("\t\t\t\t\t\t  █ █   █         █\r");
            Console.WriteLine("\t\t\t\t\t\t  █ █       █ █ █ █\r");
            Console.WriteLine("\t\t\t\t\t\t █   █      █████ █\r");
            Console.WriteLine("\t\t\t\t\t\t █         ██████ █\r");
            Console.WriteLine("\t\t\t\t\t\t █        █████  █\r");
            Console.WriteLine("\t\t\t\t\t\t  █       █ █ █  █\r");
            Console.WriteLine("\t\t\t\t\t\t   ███          █\r");
            Console.WriteLine("\t\t\t\t\t\t      ██     ███\r");
            Console.WriteLine("\t\t\t\t\t\t        █████\r");
            // Console.WriteLine("\n");

            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", "\t\t    ==========================="));
            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", "\t\t    ╔═╗┌─┐┌─┐┬ ┬  ╔═╗┌─┐┬  ┬┌─┐"));
            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", "\t\t    ║╣ ├─┤└─┐└┬┘  ╚═╗├─┤└┐┌┘├┤ "));
            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", "\t\t    ╚═╝┴ ┴└─┘ ┴   ╚═╝┴ ┴ └┘ └─┘"));
            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", "\t\t    ==========================="));
            Console.WriteLine();

            // Show the user the different saves.
            DisplaySaveWorks();


            // Ask the user to choose an option.
            Console.WriteLine("\n" + Language.objLanguage.SelectToken("choose_option") + "\n");
            Console.WriteLine("\t" + Language.objLanguage.SelectToken("create_option"));
            Console.WriteLine("\t" + Language.objLanguage.SelectToken("run_option"));
            Console.WriteLine("\t" + Language.objLanguage.SelectToken("modif_option"));
            Console.WriteLine("\t" + Language.objLanguage.SelectToken("delete_option"));
            Console.WriteLine("\t" + Language.objLanguage.SelectToken("choose_language") + "\n");

            // Ask the user to select an option for the save.
            String input = Console.ReadLine();
            if (int.TryParse(input, out int selectionoption))
            {

                // Use a switch statement to do the selection.
                switch (selectionoption)
                {
                    case 1:
                        DisplayCreateWork();
                        break;
                    case 2:
                        DisplayRunWork();
                        break;
                    case 3:
                        DisplayModifyWork();
                        break;
                    case 4:
                        DisplayDeleteWork();
                        break;
                    case 5:
                        DisplayChangeCurrentLanguage();
                        break;
                    default:
                        Console.WriteLine("\n" + Language.objLanguage.SelectToken("leave_menu_if_not_options") + "\n");
                        Console.WriteLine(Language.objLanguage.SelectToken("leave_action"));
                        Console.ReadLine();
                        DisplayStartMenu();
                        break;
                }
            }
            // Wait for the user to respond before closing.
            ReturnToMenu();
        }

        /// <summary>
        /// The display method for the backup Job creation 
        /// </summary>
        public void DisplayCreateWork()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Console.WriteLine(Language.objLanguage.SelectToken("create_savework_name"));
            string inputName = Console.ReadLine();
            parameters.Add("Name", inputName);
            Console.WriteLine(Language.objLanguage.SelectToken("create_savework_sourcepath"));
            string inputSourceRepo = Console.ReadLine();
            parameters.Add("SourceRepo", inputSourceRepo);
            Console.WriteLine(Language.objLanguage.SelectToken("create_savework_targetpath"));
            string inputTargetRepo = Console.ReadLine();
            parameters.Add("TargetRepo", inputTargetRepo);
            Console.WriteLine(Language.objLanguage.SelectToken("create_savework_savetype"));
            Console.WriteLine(Language.objLanguage.SelectToken("complete_save"));
            Console.WriteLine(Language.objLanguage.SelectToken("differential_save"));
            string inputSaveType = Console.ReadLine();
            inputSaveType = SaveWork.VerifSaveType(inputSaveType);
            parameters.Add("SaveType", inputSaveType);
            SaveWork.CreateSaveWork(parameters);

            ReturnToMenu();
        }

        /// <summary>
        /// The display method to start one or more backups
        /// </summary>
        public void DisplayRunWork()
        {

            List<string> WorkNames = new List<string>();
            string WorkName = "";


            //Store all the save work that the user want to run in an array
            while (true)
            {
                Console.WriteLine("\n" + Language.objLanguage.SelectToken("ask_runsave") + "\n");
                WorkName = Console.ReadLine();

                if(WorkName == "runsave") { 
                    break; 
                }

                WorkNames.Add(WorkName);
            }

            SaveWork.RunSaveWork(WorkNames);

            ReturnToMenu();
        }

        /// <summary>
        /// The display method to delete a Job backup
        /// </summary>
        public void DisplayDeleteWork()
        {
            Console.WriteLine(Language.objLanguage.SelectToken("delete_savework_name"));
            string inputName = Console.ReadLine();

            SaveWork.DeleteSaveWorks(inputName);

            ReturnToMenu();
        }

        /// <summary>
        /// The display method to modify a Job backup
        /// </summary>
        public void DisplayModifyWork()
        {
            Dictionary<string, string> parameters =new Dictionary<string, string>();
            Console.WriteLine(Language.objLanguage.SelectToken("modify_savework_name"));
            string inputName = Console.ReadLine();
            Console.WriteLine(Language.objLanguage.SelectToken("modify_new_savework_name"));
            string Name= Console.ReadLine();
            parameters.Add("Name", Name);
            Console.WriteLine(Language.objLanguage.SelectToken("modify_new_savework_sourcepath"));
            string SourceRepo = Console.ReadLine();
            parameters.Add("SourceRepo", SourceRepo);
            Console.WriteLine(Language.objLanguage.SelectToken("modify_new_savework_targetpath"));
            string TargetRepo = Console.ReadLine();
            parameters.Add("TargetRepo", TargetRepo);
            Console.WriteLine(Language.objLanguage.SelectToken("modify_new_savework_savetype"));
            Console.WriteLine(Language.objLanguage.SelectToken("complete_save"));
            Console.WriteLine(Language.objLanguage.SelectToken("differential_save"));
            string SaveType = Console.ReadLine();
            parameters.Add("SaveType", SaveType);
            SaveWork.ModifySaveWork(inputName, parameters);
        }

        /// <summary>
        /// The method to display a Job backup
        /// </summary>
        public void DisplaySaveWorks()
        {
            Console.WriteLine(Language.objLanguage.SelectToken("display_saveworks") + "\n");
            SaveWork.DisplaySaveWorks();
        }
        /// <summary>
        /// The method to display available languages and change the language
        /// </summary>
        public void DisplayChangeCurrentLanguage()
        {
            int count = 1;
            Console.WriteLine("\n" + Language.objLanguage.SelectToken("available_languages"));
            foreach (string language in Language.availableLanguages)
            {
                Console.WriteLine(count + " - " + language);
                count++;
            }
            count = 1;
            string inputLanguage = Console.ReadLine();
            switch (inputLanguage)
            {
                case "1":
                    Language.changeCurrentLanguage("FR");
                    break;
                case "2":
                    Language.changeCurrentLanguage("EN");
                    break;
            }
            Console.WriteLine(Language.objLanguage.SelectToken("confirm_language"));
            ReturnToMenu();
        }
        /// <summary>
        /// The method to display the menu and load the current language
        /// </summary>
        public void DisplayStartMenu()
        {
            Language.loadCurrentLanguage();
            DisplayMenu();
        }
        /// <summary>
        /// The method to display the menu
        /// </summary>
        public void ReturnToMenu()
        {
            Console.WriteLine(Language.objLanguage.SelectToken("leave_action"));
            Console.ReadLine();
            Console.Clear();
            DisplayStartMenu();
        }

        /// <summary>
        /// Main method of the program 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Menu Interface = new Menu();
            Interface.DisplayStartMenu();
        }
    }
}