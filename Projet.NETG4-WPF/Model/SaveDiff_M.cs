using System;
using System.Collections.Generic;
using System.IO;
using EventManager;
using log_models;
using Language;
using System.Linq;
using cryptoSoft;
using System.ComponentModel;
using System.Threading;
using Projet.NETG4_WPF;

namespace SaveModel
{
    /// <summary>
    /// Child class of the save for a differential save
    /// </summary>
    class SaveDiff_M : Save_M
    {
        private EventManager_M event_save;
        private Language_VM Language;

        public SaveDiff_M()
        {
            this.SaveType = "Diff";

            Log_state log_state = new Log_state();
            event_save = new EventManager_M();
            event_save.Register(log_state);

            Language = new Language_VM();
            Language.loadCurrentLanguage();
        }
        /// <summary>
        /// Run a differential save and return some information data relative to the differential save
        /// </summary>
        /// <param name="sourcePath">Source directory</param>
        /// <param name="targetPath">Target directory</param>
        /// <param name="name">Name of the Backup Job</param>
        /// <returns>All information of the save store in a Dictionary</returns>
        public override Dictionary<string, string> RunSave(string sourcePath, string targetPath, string name, object sender, DoWorkEventArgs e)
        {
            string currThreadName = Thread.CurrentThread.Name;
            List<string> sourceFileListSorted = priority_fileSorted(sourcePath, targetPath);

            Dictionary<string, string> saveList = new Dictionary<string, string>();
            Dictionary<string, string> saveListReturn = new Dictionary<string, string>();
            int Count = 0;
            string tempsXor = "0";
            string dateFormat = "ss.fffffff";

            DateTime addTemps = DateTime.MinValue;

            DateTime tempsdeb = DateTime.Now;

            try
            {
                FileNumber = sourceFileListSorted.Count;

                //Get the xor encryption key
                string key = getKeyCript();
                //Get the list of extensions to encrypt
                List<string> ext_to_crypt = getExtCrypt();
    
                //if (!Running)
                //{
                    //Create directory in the new path
                    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                    {
                        //Cancel or pause the save
                        MenuPrincipale.PauseSaveList[name].WaitOne();
                        if ((sender as BackgroundWorker).CancellationPending == true)
                        {
                            List<string> listCancel = new List<string>();
                            listCancel.Add("stoped");
                            listCancel.Add(currThreadName);


                            (sender as BackgroundWorker).ReportProgress(100, listCancel);

                            saveListReturn.Add("Name", "error");


                            e.Cancel = true;
                            return saveListReturn;
                        }
                        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                    }


                    //Copy all the files & Replaces any files with the same name
                    foreach (string sourceFile in sourceFileListSorted)
                    {
                        //Cancel or pause the save
                        MenuPrincipale.PauseSaveList[name].WaitOne();
                        if ((sender as BackgroundWorker).CancellationPending == true)
                        {
                            List<string> listCancel = new List<string>();
                            listCancel.Add("stoped");
                            listCancel.Add(currThreadName);


                            (sender as BackgroundWorker).ReportProgress(100, listCancel);

                            saveListReturn.Add("Name", "error");


                            e.Cancel = true;
                            return saveListReturn;
                        }
                        FileInfo f = new FileInfo(sourceFile);
                    bool currFileIsPrio = false;

                    //Check if the current file is on priority list
                    if (this.priorityFilesList.Contains(f.FullName))
                    {
                        int removeIndex = this.priorityFilesList.IndexOf(f.FullName);
                        this.priorityFilesList.RemoveAt(removeIndex);

                        currFileIsPrio = true;
                    }


                    //If the current file is not in the priority list
                    if (!currFileIsPrio)
                    {
                        //And if a thread have set the priority flag -> pause the thread
                        while (priorityFlag)
                        {
                            if (!priorityFlag)
                            {
                                break;
                            }
                        }
                    }


                    //While there's still priority files
                    if (this.priorityFilesList.Count != 0)
                    {
                        priorityFlag = true;
                    }
                    else
                    {
                        priorityFlag = false;
                    }

                    //Get the current file extension 
                    string file_extension = Path.GetExtension(sourceFile);
                        //Get the current file name 
                        string file_name = Path.GetFileNameWithoutExtension(sourceFile);


                        //Verify if the source File is different from the target file
                        //Verify if the current file extension is to encrypt
                        if (ext_to_crypt.Contains(file_extension))
                        {

                            cryptoSoftObj obj_cryptSoft = new cryptoSoftObj();
                            byte[] encrypt_file = new byte[] { };

                            try
                            {
                                DateTime curTime = DateTime.Now;
                                encrypt_file = (byte[])obj_cryptSoft.run_XOR(sourceFile, key).Clone();
                                double result = (DateTime.Now.Subtract(curTime).TotalMilliseconds);

                                addTemps = addTemps.AddMilliseconds(result);
                                tempsXor = addTemps.ToString(dateFormat);
                                ;
                                //Crypted file copy
                                File.WriteAllBytes(sourceFile.Replace(sourcePath, targetPath), encrypt_file);
                            }
                            catch (Exception except)
                            {
                                tempsXor = Convert.ToString(-1);
                            }
                        }
                        else
                        {
                            File.Copy(sourceFile, sourceFile.Replace(sourcePath, targetPath), true);

                        }

                        FileSize += f.Length;

                        Count++;
                        float progression = Count / FileNumber;
                        float percent = progression * 100;
                        float remainingFiles = FileNumber - Count;
                        int parameterInt = Convert.ToInt32(percent);

                        //Create a list usable by the log state
                        Dictionary<string, string> log_state_listActive = fill_state_list(name, FileNumber, FileSize, remainingFiles, parameterInt, "ACTIVE");

                        //Management of the report progress
                        List<string> listBGWRun = new List<string>();
                        listBGWRun.Add("running");
                        listBGWRun.Add(currThreadName);

                        (sender as BackgroundWorker).ReportProgress(parameterInt, listBGWRun);

                        //Send information to the log state when the save is active
                        event_save.Notify("run", log_state_listActive);
                        log_state_listActive.Clear();


                    }
                    //Calculate and write the transfer time
                    TransferTime = DateTime.Now - tempsdeb;

                    List<string> listBGW = new List<string>();

                    listBGW.Add("completed");
                    listBGW.Add(currThreadName);
                    listBGW.Add(name);
                    listBGW.Add(Convert.ToString(Language.objLanguage.SelectToken("save_done")));
                    listBGW.Add(Convert.ToString(Language.objLanguage.SelectToken("files_size") + " " + FileSize + " Ko"));
                    listBGW.Add(Convert.ToString(Language.objLanguage.SelectToken("transfer_time") + " " + TransferTime));
                    listBGW.Add(Convert.ToString(Language.objLanguage.SelectToken("date_save") + " " + DateSave));


                    (sender as BackgroundWorker).ReportProgress(100, listBGW);

                    //Create a list usable by the log state
                    Dictionary<string, string> log_state_listEND = fill_state_list(name, 0, 0, 0, 0, "END");

                    //Send information to the log state when the save is finish
                    event_save.Notify("run", log_state_listEND);
                    log_state_listEND.Clear();

                    //Stock values for the eventManager
                    Dictionary<string, string> log_daily_list = fill_daily_list(name, sourcePath, targetPath, FileSize, TransferTime, DateSave, tempsXor);

                    //Return the information of the save for the daily log
                    return log_daily_list;

            }

            //Catch an exception if the save doesn't worked
            catch (Exception except)
            {

                List<string> listBGW = new List<string>();
                listBGW.Add("error");
                listBGW.Add(currThreadName);
                listBGW.Add(except.Message);


                (sender as BackgroundWorker).ReportProgress(100, listBGW);

                return saveListReturn;
            }
        }

        /// <summary>
        /// Method to sort all file in priority order an return the list of file sorted 
        /// </summary>
        /// <param name="sourcePath">Target path of the file</param>
        /// <param name="targetPath">Source path of the file</param>
        /// <returns>The list sorted</returns>
        public override List<string> priority_fileSorted(string sourcePath, string targetPath)
        {
            List<string> extlist = getExtPrio();

            List<string> sortedFileList = new List<string>();

            int postion = 0;
            foreach (string sourcefile in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                string TargetFile = sourcefile.Replace(sourcePath, targetPath);
                DateTime LastWriteTimeSource = File.GetLastWriteTime(sourcefile);
                DateTime LastWriteTimeTarget = File.GetLastWriteTime(TargetFile);

                if (extlist.Any(Path.GetExtension(sourcefile).ToLower().Contains) && LastWriteTimeSource > LastWriteTimeTarget)
                {
                    this.priorityFilesList.Add(sourcefile);

                    sortedFileList.Insert(postion, sourcefile);
                    postion++;
                }
                else if(LastWriteTimeSource > LastWriteTimeTarget)
                {
                    sortedFileList.Add(sourcefile);
                }
            }


            return sortedFileList;
        }
    }
}
