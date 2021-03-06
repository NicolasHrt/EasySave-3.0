using System;
using System.Collections.Generic;
using System.IO;
using EventManager;
using log_models;
using Language;
using cryptoSoft;

namespace SaveModel
{
    /// <summary>
    /// Child class of the save for a complete save
    /// </summary>
    class SaveComplete_VM : Save_M
    {
        private EventManager_M event_save;
        Language_VM Language;


        public SaveComplete_VM()
        {
            this.SaveType = "Complete";
            Log_state log_state = new Log_state();
            event_save = new EventManager_M();
            event_save.Register(log_state);
            Language = new Language_VM();
            Language.loadCurrentLanguage();
        }

        /// <summary>
        /// Run a complete save and return some information data relative to the complete save
        /// </summary>
        /// <param name="sourcePath">Source directory</param>
        /// <param name="targetPath">Target directory</param>
        /// <param name="name">Name of the Backup Job</param>
        /// <returns>All information of the save store in a Dictionary</returns>
        public override Dictionary<string, string> RunSave(string sourcePath, string targetPath, string name)
        {
            Dictionary<string, string> saveList = new Dictionary<string, string>();
            Dictionary<string, string> saveListReturn = new Dictionary<string, string>();
            int Count = 1;

            string tempsXor = " ";
            string dateFormat = "ss.fffffff";
            DateTime addTemps = DateTime.MinValue;

            DateTime tempsdeb = DateTime.Now;

            try
            {
                FileNumber = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories).Length;

                //Recuperation de la clé de chiffrement 
                string key = getKeyCript();
                //Recuperation des extensions à chiffrer
                List<string> ext_to_crypt = getExtCrypt();

                bool running = false;

                //Check fore each file to copy if the user marqued it and if it's running
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    string file_name = Path.GetFileNameWithoutExtension(newPath);

                    if (check_process(file_name))
                    {
                        running = true;
                    }
                }

                if (!running)
                {
                    //Create directory in the new path
                    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                    }

                    //Copy all the files & Replaces any files with the same name
                    foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    {
                        //Récupère l'extension du fichier
                        string file_extension = Path.GetExtension(newPath);
                        //Récupère le nom du fichier
                        string file_name = Path.GetFileNameWithoutExtension(newPath);


                        FileInfo f = new FileInfo(newPath);

                        //Vérifie si le fichier en cours est compris dans les extensions a chiffrer
                        if (ext_to_crypt.Contains(file_extension))
                        {

                            cryptoSoftObj obj_cryptSoft = new cryptoSoftObj();
                            byte[] encrypt_file = new byte[] { };
                            try
                            {
                                DateTime currTemp = DateTime.Now;
                                encrypt_file = (byte[])obj_cryptSoft.run_XOR(newPath, key).Clone();
                                double result = (DateTime.Now.Subtract(currTemp).TotalMilliseconds);

                                Console.WriteLine("Chiffrement temps : " + result);

                                addTemps = addTemps.AddMilliseconds(result);
                                Console.WriteLine("Addition du temps de chiffrement: " + addTemps.ToString(dateFormat));

                                tempsXor = addTemps.ToString(dateFormat);

                                //Copie du fichier chiffré dans le repertoire cible
                                File.WriteAllBytes(newPath.Replace(sourcePath, targetPath), encrypt_file);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("erreur de chiffrement : " + e);
                                tempsXor = Convert.ToString(-1);
                            }
                        }
                        else
                        {
                            //Copie du fichier dans le repertoire cible
                            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                        }


                        //Write in console information about the files 
                        Console.WriteLine(Path.GetFileName(newPath) + " {0} octets", f.Length);
                        Console.WriteLine("{0} / {1} files modified", Count, FileNumber);

                        FileSize += f.Length;

                        float progression = FileNumber / Count;
                        int remainingFiles = FileNumber - Count;

                        //Create a list usable by the log state
                        Dictionary<string, string> log_state_listActive = fill_state_list(name, FileNumber, FileSize, remainingFiles, progression, "ACTIVE");

                        //Send information to the log state when the save is active
                        event_save.Notify("run", log_state_listActive);
                        log_state_listActive.Clear();


                        //Send information to the log state when the save is finish
                        if (Count == FileNumber)
                        {

                            //Create a list usable by the log state
                            Dictionary<string, string> log_state_listEND = fill_state_list(name, 0, 0, 0, 0, "END");

                            //Send information to the log state when the save is active
                            event_save.Notify("run", log_state_listEND);
                            log_state_listEND.Clear();
                        }


                        Count++;



                    }

                    //Calculate the transfer time
                    TransferTime = DateTime.Now - tempsdeb;

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("\n" + Language.objLanguage.SelectToken("save_done"));
                    Console.WriteLine(Language.objLanguage.SelectToken("files_size") + "{0} octets", FileSize);
                    Console.WriteLine(Language.objLanguage.SelectToken("transfer_time") + "{0}", TransferTime);
                    Console.WriteLine(Language.objLanguage.SelectToken("date_save") + "{0}", DateSave);
                    Console.ResetColor();

                    //Stock values for the eventManager
                    Dictionary<string, string> log_daily_list = fill_daily_list(name, sourcePath, targetPath, FileSize, TransferTime, DateSave, tempsXor);

                    //Return the information of the save for the daily log
                    return log_daily_list;

                }
                else
                {
                    saveListReturn.Add("Name", "error");

                    return saveListReturn;

                }
            }


            //Catch an exception if the save doesn't worked
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Language.objLanguage.SelectToken("error") + e.Message);
                Console.ResetColor();

                saveListReturn.Add("Name", "error");


                return saveListReturn;

            }

        }
    }
}
