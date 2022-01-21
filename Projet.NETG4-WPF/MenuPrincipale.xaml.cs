using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using SaveWork;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Projet.NETG4_WPF;
using Language;
using parameterJson;
using MaterialDesignThemes.Wpf;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace Projet.NETG4_WPF
{
    /// <summary>
    /// Logique d'interaction pour MenuPrincipale.xaml
    /// </summary>
    public partial class MenuPrincipale : Window
    {
        public string ModifyOrCreate { get; set; }

        public string InitialJobName { get; set; }
        public bool IsDarkTheme { get; set; }
        public AutoResetEvent doneEvent = new AutoResetEvent(false);

        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private SaveWork_VM SaveWork;
        private Language_VM Languages;

        public static Dictionary<string, ManualResetEvent> PauseSaveList;
        public static Dictionary<string, BackgroundWorker> WorkerList;

        private Mutex Mumu;

        private byte[] _buffer = new byte[1024];
        private List<Socket> _clientSockets = new List<Socket>();
        private Socket _serverSocket;

        public MenuPrincipale()
        {
            if (Process.GetProcessesByName("Projet.NETG4-WPF").Length == 1)
            {
                InitializeComponent();
                SaveWork = new SaveWork_VM();
                Languages = new Language_VM();
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Mumu = new Mutex();
                SetupServer();
                RefreshWindow();
                //Thread.Sleep(500);
                SendJobsToClients();

            }
            else
            {
                this.Close();
            }
        }

        private void toggleTheme(object sender, RoutedEventArgs e)
        {
            //Theme Code ========================>

            //get the current theme used in the application
            ITheme theme = paletteHelper.GetTheme();

            //if condition true, then set IsDarkTheme to false and, SetBaseTheme to light
            if (IsDarkTheme = theme.GetBaseTheme() == BaseTheme.Dark)
            {
                IsDarkTheme = false;
                theme.SetBaseTheme(Theme.Light);
                BackgroundCreate.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#ffffff");
                BackgroundParameter.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#ffffff");
                BackgroundRunningSave.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#ffffff");
                ConsoleRun.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#e9eff2");
            }

            //else set IsDarkTheme to true and SetBaseTheme to dark
            else
            {
                IsDarkTheme = true;
                theme.SetBaseTheme(Theme.Dark);
                BackgroundCreate.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#303030");
                BackgroundParameter.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#303030");
                BackgroundRunningSave.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#303030");
                ConsoleRun.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#0e2d3d");
            }

            //to apply the changes use the SetTheme function
            paletteHelper.SetTheme(theme);
            //===================================>
        }

        #region create
        private void CreateJob_Click(object sender, RoutedEventArgs e)
        {
            ModifyOrCreate = "create";
            CreateSave.Visibility = Visibility.Visible;
            ClearCreateModify();

        }

        private void CreateModifyJob_Click(object sender, RoutedEventArgs e)
        {
            //Languages.loadCurrentLanguage();
            bool valid = true;
            SaveWork = new SaveWork_VM();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (job_name_textbox.Text != string.Empty)
            {
                ErrorJobName.Text = string.Empty;
                parameters.Add("Name", job_name_textbox.Text);
            }
            else
            {
                ErrorJobName.Text = Convert.ToString(Languages.objLanguage.SelectToken("error_job_name"));
                valid = false;
            }

            if (source_repo_textbox.Text != string.Empty && Directory.Exists(source_repo_textbox.Text))
            {
                ErrorSourceRepo.Text = string.Empty;
                parameters.Add("SourceRepo", source_repo_textbox.Text);
            }
            else
            {
                ErrorSourceRepo.Text = Convert.ToString(Languages.objLanguage.SelectToken("error_repo"));
                valid = false;
            }

            if (target_repo_textbox.Text != string.Empty && Directory.Exists(target_repo_textbox.Text))
            {
                ErrorTargetRepo.Text = string.Empty;
                parameters.Add("TargetRepo", target_repo_textbox.Text);
            }
            else
            {
                ErrorTargetRepo.Text = Convert.ToString(Languages.objLanguage.SelectToken("error_repo"));
                valid = false;
            }

            parameters.Add("SaveType", CheckSaveType());

            if (valid)
            {
                if (ModifyOrCreate == "create")
                {
                    SaveWork.CreateSaveWork(parameters);
                    System.Windows.MessageBox.Show(Convert.ToString(Languages.objLanguage.SelectToken("success_create")));
                    //Main.RefreshWindow();
                    CreateSave.Visibility = Visibility.Hidden;
                    ClearCreateModify();
                    RefreshData();
                    SendJobsToClients();
                }
                else if (ModifyOrCreate == "modify")
                {
                    SaveWork.ModifySaveWork(InitialJobName, parameters);
                    System.Windows.MessageBox.Show(Convert.ToString(Languages.objLanguage.SelectToken("success_modify")));
                    //Main.RefreshWindow();
                    CreateSave.Visibility = Visibility.Hidden;
                    ClearCreateModify();
                    RefreshData();
                    SendJobsToClients();
                }
            }
            else
            {
                //System.Windows.MessageBox.Show(Convert.ToString(Languages.objLanguage.SelectToken("informations_invalid")));
            }
        }

        private void ChooseFolderSource(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var status = dialog.ShowDialog(); // ShowDialog() returns bool? (Nullable bool)
            if (status == System.Windows.Forms.DialogResult.OK)
            {
                source_repo_textbox.Text = dialog.SelectedPath;
            }
        }

        private void ChooseFolderTarget(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var status = dialog.ShowDialog(); // ShowDialog() returns bool? (Nullable bool)
            if (status == System.Windows.Forms.DialogResult.OK)
            {
                target_repo_textbox.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// The method to send the good Savetype 
        /// </summary>
        /// <returns></returns>
        private string CheckSaveType()
        {
            string SaveType = string.Empty;
            if (save_type_full.IsChecked == true)
            {
                SaveType = "Complete";
            }
            else if (save_type_diff.IsChecked == true)
            {
                SaveType = "Diff";
            }
            return SaveType;
        }

        #endregion

        /// <summary>
        /// Remove a job from the listBox to run
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveWork_Click(object sender, RoutedEventArgs e)
        {
            if (listJob.SelectedItem != null)
            {
                string itemTxt = listJob.SelectedItem.ToString();
                listJob.Items.RemoveAt
                    (listJob.Items.IndexOf(listJob.SelectedItem));
                //MessageBox.Show("Job Successfully Removed");
            }
            else
            {
                System.Windows.MessageBox.Show(Convert.ToString(Languages.objLanguage.SelectToken("select_error_delete")));
            }
        }

        /// <summary>
        /// Run all the jobs in the listBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launcher_Click(object sender, RoutedEventArgs e)
        {
            if (listJob.Items.Count > 0)
            {
                RunningSave.Visibility = Visibility.Visible;
                TBAffichage.Text = "";
                launcher();
            }

        }

        // Méthode qui initialise la barre de progression 
        public void launcher()
        {

            TBAffichage.Text = "";
            List<string> listSave = new List<string>();
            WorkerList = new Dictionary<string, BackgroundWorker>();
            PauseSaveList = new Dictionary<string, ManualResetEvent>();

            DataTable DataSet = new DataTable();
            DataSet.Columns.Add(Convert.ToString(Languages.objLanguage.SelectToken("job_name_label")));
            DataSet.Columns.Add("Progression (%)");
            DataSet.Columns.Add("Status");


            /*  DataSet.Columns.Add("state", typeof(System.Windows.Controls.ProgressBar));*/

            foreach (Object item in listJob.Items)
            {
                listSave.Add(Convert.ToString(item));

                DataRow dr = DataSet.NewRow();

                Dispatcher.Invoke(() =>
                {
                    int pBar = 0;
                    dr[1] = pBar;
                    dr[0] = item;
                    dr[2] = Convert.ToString(Languages.objLanguage.SelectToken("running"));


                    DataSet.Rows.Add(dr);
                    DgProgressSaves.ItemsSource = null;
                    DgProgressSaves.ItemsSource = DataSet.DefaultView;
                });
                BackgroundWorker worker = new BackgroundWorker();

                worker.WorkerSupportsCancellation = true;
                //création, initialisation et mise à jour de l'objet BackgroundWorker
                worker.WorkerReportsProgress = true;
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                worker.RunWorkerAsync(item);

                WorkerList.Add(Convert.ToString(item), worker);
                PauseSaveList.Add(Convert.ToString(item), new ManualResetEvent(true));

            }
            SendRunningPageToClients(listSave);
        }





        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            SaveWork.RunSaveWork(Convert.ToString(e.Argument), sender, e);

        }

        public void editRunSaveRows(List<string> str, ProgressChangedEventArgs e)
        {

            int row = 0;

            foreach (DataRowView drv in DgProgressSaves.Items)
            {
                // compare value in datarow of view
                if (str[1].Contains((string)drv.Row.ItemArray[0]))
                {
                    // select item
                    DataRowView rowView = DgProgressSaves.Items[row] as DataRowView; //Get RowView
                    rowView.BeginEdit();
                    rowView[1] = e.ProgressPercentage.ToString();
                    rowView[2] = str[0];
                    rowView.EndEdit();

                    DgProgressSaves.Items.Refresh(); // Refresh table
                }
                row++;
            }

        }

        /// <summary>
        /// worker_ProgressChanged
        /// </summary>
        /// <param name="sender"></param> Variable usable to have information about the calling savework
        /// <param name="e"></param> Variable  usefull to have the progression of the calling savework
        public void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Str[0]=flag, Str[1]=Current Thread Name, Str[2]=Save Name, Str[3]=Message, Str[4]=file size, Str[5]=transfert time, Str[6]=date save

            List<string> str = new List<string>();
            str = (List<string>)e.UserState; //Stock all savework's infos


            Task TaskTest = Task.Run(() =>
            {
                Thread.Sleep(100);
                SendUpdateSaveToClients(str, e.ProgressPercentage); //Send informations to the client
            });

            //Update the datagrid
            editRunSaveRows(str, e);


            if (str[0] == "running")
            {
                //Do work if running
            }

            else if (str[0] == "completed")
            {
                //Do work if completed
                TBAffichage.Inlines.Add("----------------------------------------------- \n");
                TBAffichage.Inlines.Add(str[2] + " :\n \n");
                TBAffichage.Inlines.Add(str[3] + " \n");
                TBAffichage.Inlines.Add(str[4] + " \n");
                TBAffichage.Inlines.Add(str[5] + " \n");
                TBAffichage.Inlines.Add(str[6] + " \n \n");
            }

            else if (str[0] == "error")
            {
                //Do work if error
                TBAffichage.Inlines.Add("----------- ERROR -----------\n");
                TBAffichage.Inlines.Add(str[1] + " :\n \n");
                TBAffichage.Inlines.Add(str[2] + " \n \n");
            }

            else if (str[0] == "stoped")
            {
                //Do work if stoped
                TBAffichage.Inlines.Add("----------- Stop Action -----------\n");
                TBAffichage.Inlines.Add(str[1] + " :\n \n");
                TBAffichage.Inlines.Add(Convert.ToString(Languages.objLanguage.SelectToken("save_stop")) + " \n \n");
            }

            /*  if (e.UserState == null)
           {
               pbStatus.Value = e.ProgressPercentage;
           }*/

        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }


        private void BackupJob_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Add the name of the job selected to the run ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addToRunList(object sender, MouseButtonEventArgs e)
        {
            bool found = false;
            DataRowView dataRowView = (DataRowView)BackupJob.SelectedItem; //Get the name of the selected item
            string jobName = Convert.ToString(Languages.objLanguage.SelectToken("job_name_label"));

            foreach (var item in listJob.Items)
            {
                if (item.ToString().Equals(dataRowView[jobName]))
                {
                    found = true;
                    break;
                }
            }

            if (dataRowView[jobName] != null && !found)
            {

                listJob.Items.Add(dataRowView[jobName]); //Add the selected item to the run save list
            }

            else
            {
                if (dataRowView[jobName] == null)
                {
                    System.Windows.MessageBox.Show(Convert.ToString(Languages.objLanguage.SelectToken("select_error")));
                }
                else
                {
                    System.Windows.MessageBox.Show(Convert.ToString(Languages.objLanguage.SelectToken("select_error_already_select")));
                }
            }
        }

        /// <summary>
        /// Delete a job the selected job in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteJob_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)BackupJob.SelectedItem;
            string jobName = Convert.ToString(Languages.objLanguage.SelectToken("job_name_label"));
            if (dataRowView != null)
            {
                SaveWork = new SaveWork_VM();
                SaveWork.DeleteSaveWorks(Convert.ToString(dataRowView[jobName]));
                RefreshData();
            }
            SendJobsToClients();

        }

        /// <summary>
        /// Display the CreateModify.xaml window as Modify window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModifyJob_Click(object sender, RoutedEventArgs e)
        {
            if (BackupJob.SelectedItem == null)
            {
                System.Windows.MessageBox.Show(Convert.ToString(Languages.objLanguage.SelectToken("select_error_modify")));
            }
            else
            {
                string jobName = Convert.ToString(Languages.objLanguage.SelectToken("job_name_label"));
                string sourceRepo = Convert.ToString(Languages.objLanguage.SelectToken("source_repo_label"));
                string targetRepo = Convert.ToString(Languages.objLanguage.SelectToken("target_repo_label"));
                string saveType = Convert.ToString(Languages.objLanguage.SelectToken("save_type_label"));
                ClearCreateModify();
                CreateSave.Visibility = Visibility.Visible;
                ModifyOrCreate = "modify";
                DataRowView dataRowView = (DataRowView)BackupJob.SelectedItem;
                job_name_textbox.Text = Convert.ToString(dataRowView[jobName]);
                InitialJobName = Convert.ToString(dataRowView[jobName]);
                source_repo_textbox.Text = Convert.ToString(dataRowView[sourceRepo]);
                target_repo_textbox.Text = Convert.ToString(dataRowView[targetRepo]);
                if (Convert.ToString(dataRowView[saveType]) == "Complete")
                {
                    save_type_full.IsChecked = true;
                }
                else
                {
                    save_type_diff.IsChecked = true;
                }

            }

        }

        /// <summary>
        /// Refresh this window
        /// </summary>
        public void RefreshWindow()
        {
            RefreshData();
            Languages.loadCurrentLanguage();

            //Main page
            create_button.Content = Languages.objLanguage.SelectToken("create_button");
            modify_button.Content = Languages.objLanguage.SelectToken("modify_button");
            delete_button.Content = Languages.objLanguage.SelectToken("delete_button");
            run_button.Content = Languages.objLanguage.SelectToken("run_button");
            parameters_button.Content = Languages.objLanguage.SelectToken("parameters_button");
            parameters_title.Content = Languages.objLanguage.SelectToken("parameters_button");
            save_list_label.Text = Convert.ToString(Languages.objLanguage.SelectToken("save_list_label"));
            save_wait_label.Text = Convert.ToString(Languages.objLanguage.SelectToken("save_wait_label"));

            //Save running 
            Save_running_title.Text = Convert.ToString(Languages.objLanguage.SelectToken("save_running"));
            OneSave_Radio.Content = Languages.objLanguage.SelectToken("one_save");
            AllSave_Radio.Content = Languages.objLanguage.SelectToken("all_save");


            RefreshCreateLanguage();

            RefreshParamLanguage();


        }

        /// <summary>
        /// Refresh data from the list save
        /// </summary>
        public void RefreshData()
        {
            BackupJob.ItemsSource = null;
            BackupJob.ItemsSource = SaveWork.DisplaySaveWorks().DefaultView;
        }

        /// <summary>
        /// Refresh the language on the create page
        /// </summary>
        public void RefreshCreateLanguage()
        {
            backupJobCreate_title.Text = Convert.ToString(Languages.objLanguage.SelectToken("new_backupjob"));
            HintAssist.SetHint(job_name_textbox, Languages.objLanguage.SelectToken("job_name_label"));
            HintAssist.SetHint(source_repo_textbox, Languages.objLanguage.SelectToken("source_repo_label"));
            HintAssist.SetHint(target_repo_textbox, Languages.objLanguage.SelectToken("target_repo_label"));
            cancel_button.Content = Languages.objLanguage.SelectToken("cancel_button");
            save_button.Content = Languages.objLanguage.SelectToken("save_button");
            save_type_full.Content = Languages.objLanguage.SelectToken("save_type_full");
            save_type_diff.Content = Languages.objLanguage.SelectToken("save_type_diff");
            cancel_button.Content = Languages.objLanguage.SelectToken("cancel_button");
            save_button.Content = Languages.objLanguage.SelectToken("save_button");
        }

        /// <summary>
        /// Refresh the language on the parameter page
        /// </summary>
        public void RefreshParamLanguage()
        {
            language_label.Content = Languages.objLanguage.SelectToken("language_label");
            log_daily_format_label.Content = Languages.objLanguage.SelectToken("log_daily_format_label");
            fileSize_label.Content = Languages.objLanguage.SelectToken("file_size_max_label");
            crypted_ext_label.Content = Languages.objLanguage.SelectToken("crypted_ext_label");
            prio_ext_label.Content = Languages.objLanguage.SelectToken("prioritary_ext_label");
            blocked_software_label.Content = Languages.objLanguage.SelectToken("blocked_software_label");
            cancelParameter_button.Content = Languages.objLanguage.SelectToken("cancel_button");
            apply_button.Content = Languages.objLanguage.SelectToken("apply_button");
        }

        /// <summary>
        /// Clear the create 
        /// </summary>
        public void ClearCreateModify()
        {
            job_name_textbox.Text = string.Empty;
            source_repo_textbox.Text = string.Empty;
            target_repo_textbox.Text = string.Empty;
            ErrorJobName.Text = string.Empty;
            ErrorSourceRepo.Text = string.Empty;
            ErrorTargetRepo.Text = string.Empty;
        }

        /// <summary>
        /// Event called when clicking on the exit button of the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitApp(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Event called when clicking on the exit button of the create page
        /// /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeaveCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateSave.Visibility = Visibility.Hidden;
            ClearCreateModify();
        }

        #region parameter        

        /// <summary>
        /// Display the parameter window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Parameters_Click(object sender, RoutedEventArgs e)
        {
            RefreshParameter();
            GlobalSetting.Visibility = Visibility.Visible;
        }

        private void RefreshParameter()
        {
            DisplayLanguage();
            DisplayFormat();
            DisplayFileSize();
            DisplayEncryptExtension();
            DisplayPrioExtension();
            DisplaySoftware();
        }

        /// <summary>
        /// The method to apply the modification of the parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeParam(object sender, RoutedEventArgs e)
        {
            ChangeLanguage();
            ChangeFormat();
            ChangeFileSize();
            RefreshWindow();
            Clear_Param();
            GlobalSetting.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Display the current language choose by the user
        /// </summary>
        private void DisplayLanguage()
        {
            ParameterJson parameter = new ParameterJson("currentLanguage");
            string propertie;
            propertie = parameter.getOneParam();
            if (propertie == "FR")
            {
                CheckLanguage.SelectedIndex = 0;
            }
            else if (propertie == "EN")
            {
                CheckLanguage.SelectedIndex = 1;
            }
        }

        /// <summary>
        /// The method to change the language in the json parameter file
        /// </summary>
        private void ChangeLanguage()
        {
            ParameterJson parameterLangue = new ParameterJson("currentLanguage");
            if (CheckLanguage.SelectedIndex == 0)
            {
                parameterLangue.modifyOneParam("FR");
            }
            else if (CheckLanguage.SelectedIndex == 1)
            {
                parameterLangue.modifyOneParam("EN");
            }
        }

        /// <summary>
        /// The method to display the current log format in the parameter page
        /// </summary>
        private void DisplayFormat()
        {
            ParameterJson parameter = new ParameterJson("logformat");
            string propertie;
            propertie = parameter.getOneParam();
            if (propertie == "json")
            {
                JsonCheck.IsChecked = true;
            }
            else if (propertie == "xml")
            {
                XmlCheck.IsChecked = true;
            }
        }

        /// <summary>
        /// The method to change the log format in the parameter json file
        /// </summary>
        private void ChangeFormat()
        {
            ParameterJson parameterLog = new ParameterJson("logformat");
            if (JsonCheck.IsChecked == true)
            {
                parameterLog.modifyOneParam("json");
            }
            else if (XmlCheck.IsChecked == true)
            {
                parameterLog.modifyOneParam("xml");
            }
        }

        private void DisplayFileSize()
        {
            ParameterJson parameter = new ParameterJson("fileSizeMax");
            filesizeMax_textbox.Text = parameter.getOneParam();

        }

        private void ChangeFileSize()
        {
            ParameterJson parameter = new ParameterJson("fileSizeMax");
            parameter.modifyOneParam(filesizeMax_textbox.Text);

        }

        /// <summary>
        /// The method to display all the current extension that we encrypt in the parameter page
        /// </summary>
        public void DisplayEncryptExtension()
        {
            ParameterJson parameter = new ParameterJson("encryptExtensions");
            List<string> listExt = new List<string>();
            listExt = parameter.getParam();
            foreach (string ext in listExt)
            {
                ListeExtension.Items.Add(ext);
            }
        }

        /// <summary>
        /// The method to add a new extension to encrypt in the json parameter file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddEncryptExtension_Click(object sender, RoutedEventArgs e)
        {
            string TextBoxValue = NewExtension.Text;
            if (NewExtension.Text != string.Empty && TextBoxValue[0] == Convert.ToChar("."))
            {
                ParameterJson parameter = new ParameterJson("encryptExtensions");
                parameter.addParam(NewExtension.Text);
                ListeExtension.Items.Clear();
                NewExtension.Text = string.Empty;
                ErrorExtension.Text = string.Empty;
                DisplayEncryptExtension();
            }
            else
            {
                ErrorExtension.Text = Convert.ToString(Languages.objLanguage.SelectToken("error_extension"));
            }
        }

        /// <summary>
        /// The method to delete an extension to encrypt in the parameter file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteEncryptExtension_Click(object sender, RoutedEventArgs e)
        {
            ParameterJson parameter = new ParameterJson("encryptExtensions");
            parameter.removeParam(Convert.ToString(ListeExtension.SelectedValue));
            ListeExtension.Items.Clear();
            DisplayEncryptExtension();
        }

        /// <summary>
        /// Display the priority of extensions choose by the user
        /// </summary>
        public void DisplayPrioExtension()
        {
            ParameterJson parameter = new ParameterJson("prioExtension");
            List<string> listExt = new List<string>();
            listExt = parameter.getParam();
            foreach (string ext in listExt)
            {
                ListePrioExtension.Items.Add(ext);
            }
        }

        /// <summary>
        /// Add a priority extension
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddPrioExtension_Click(object sender, RoutedEventArgs e)
        {
            string TextBoxValue = NewPrioExtension.Text;
            if (NewPrioExtension.Text != string.Empty && TextBoxValue[0] == Convert.ToChar("."))
            {
                ParameterJson parameter = new ParameterJson("prioExtension");
                parameter.addParam(NewPrioExtension.Text);
                ListePrioExtension.Items.Clear();
                NewPrioExtension.Text = string.Empty;
                ErrorPrioExtension.Text = string.Empty;
                DisplayPrioExtension();
            }
            else
            {
                ErrorPrioExtension.Text = Convert.ToString(Languages.objLanguage.SelectToken("error_extension"));
            }
        }

        /// <summary>
        /// Remove a priority extension
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeletPrioExtension_Click(object sender, RoutedEventArgs e)
        {
            ParameterJson parameter = new ParameterJson("prioExtension");
            parameter.removeParam(Convert.ToString(ListePrioExtension.SelectedValue));
            ListePrioExtension.Items.Clear();
            DisplayPrioExtension();
        }

        /// <summary>
        /// displays the software choose by the user 
        /// </summary>
        public void DisplaySoftware()
        {
            ParameterJson parameter = new ParameterJson("software");
            List<string> listExt = new List<string>();
            listExt = parameter.getParam();
            foreach (string ext in listExt)
            {
                ListeSoftware.Items.Add(ext);
            }
        }

        /// <summary>
        /// The method to add a new extension to encrypt in the json parameter file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSoftware_Click(object sender, RoutedEventArgs e)
        {
            string TextBoxValue = NewSoftware.Text;
            if (NewSoftware.Text != string.Empty)
            {
                ParameterJson parameter = new ParameterJson("software");
                parameter.addParam(NewSoftware.Text);
                ListeSoftware.Items.Clear();
                NewSoftware.Text = string.Empty;
                ErrorSoftware.Text = string.Empty;
                DisplaySoftware();
            }
            else
            {
                string error = Convert.ToString(Languages.objLanguage.SelectToken("error_software"));
                ErrorSoftware.Text = error;
            }
        }

        /// <summary>
        /// The method to delete a software to block in the parameter file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteSoftware_Click(object sender, RoutedEventArgs e)
        {
            ParameterJson parameter = new ParameterJson("software");
            parameter.removeParam(Convert.ToString(ListeSoftware.SelectedValue));
            ListeSoftware.Items.Clear();
            DisplaySoftware();
        }

        /// <summary>
        /// Event called when clicking of the exit button of the setting page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Clear_Param();
            GlobalSetting.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Reset the display of the setting page
        /// </summary>
        private void Clear_Param()
        {
            ListeSoftware.Items.Clear();
            NewSoftware.Text = string.Empty;
            ErrorSoftware.Text = string.Empty;
            ListeExtension.Items.Clear();
            NewExtension.Text = string.Empty;
            ErrorExtension.Text = string.Empty;
            ListePrioExtension.Items.Clear();
            NewPrioExtension.Text = string.Empty;
            ErrorPrioExtension.Text = string.Empty;
        }

        #endregion

        /// <summary>
        /// Exit the running page and ensure to cancel all savework's threads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeaveRunning(object sender, RoutedEventArgs e)
        {
            RunningSave.Visibility = Visibility.Hidden;
            foreach (KeyValuePair<string, BackgroundWorker> worker in WorkerList)
            {
                worker.Value.CancelAsync();
            }
        }

        /// <summary>
        /// Stop one or all running threads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelSaveClick(object sender, RoutedEventArgs e)
        {
            if (OneSave_Radio.IsChecked == true)
            {
                DataRowView dataRowView = (DataRowView)DgProgressSaves.SelectedItem;
                if (dataRowView != null)
                {
                    string currentSave = (string)dataRowView[Convert.ToString(Languages.objLanguage.SelectToken("job_name_label"))];
                    WorkerList[currentSave].CancelAsync();
                }
            }
            else
            {
                foreach (KeyValuePair<string, BackgroundWorker> worker in WorkerList)
                {
                    worker.Value.CancelAsync();
                }
            }

        }

        /// <summary>
        /// Setup the server to wait for clients 
        /// </summary>
        private void SetupServer()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 11000));
            _serverSocket.Listen(5);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        /// <summary>
        /// Callback method of the server 
        /// </summary>
        /// <param name="AR"></param>
        private void AcceptCallBack(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSockets.Add(socket);
            SendJobsToClients();
            //socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        /// <summary>
        /// Set in pause one or all running threads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PauseSaveClick(object sender, RoutedEventArgs e)
        {
            if (OneSave_Radio.IsChecked == true)
            {
                DataRowView dataRowView = (DataRowView)DgProgressSaves.SelectedItem;
                if (dataRowView != null)
                {
                    string currentSave = (string)dataRowView[Convert.ToString(Languages.objLanguage.SelectToken("job_name_label"))];
                    PauseSaveList[currentSave].Reset();
                }
            }
            else
            {
                foreach (KeyValuePair<string, ManualResetEvent> PauseSave in PauseSaveList)
                {
                    PauseSave.Value.Reset();
                }
            }
        }

        /// <summary>
        /// Callback method when the server receive message
        /// </summary>
        /// <param name="AR"></param>
        public void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] databuff = new byte[received];
            Array.Copy(_buffer, databuff, received);
            string text = Encoding.UTF8.GetString(databuff);
            string response = string.Empty;
            if (text.ToLower() == "get time")
            {
                response = DateTime.Now.ToLongTimeString();
            }
            else
            {
                response = "Invalid Request";
            }
            byte[] data = Encoding.UTF8.GetBytes(response);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        /// <summary>
        /// Callback method when the server send a message
        /// </summary>
        /// <param name="AR"></param>
        public void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        /// <summary>
        /// Method which allows to send the list of saveworks to the client
        /// </summary>
        private void SendJobsToClients()
        {
            string strJobs = "SendJobs|";
            strJobs += string.Join(Environment.NewLine, SaveWork.DisplaySaveWorks().Rows.OfType<DataRow>().Select(x => string.Join(";", x.ItemArray)));
            byte[] bufferJobs = Encoding.ASCII.GetBytes(strJobs);
            _clientSockets.ForEach(delegate (Socket socket)
            {
                socket.BeginSend(bufferJobs, 0, bufferJobs.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            });
        }

        /// <summary>
        /// Method which allows to send real time data to the client
        /// </summary>
        public void SendUpdateSaveToClients(List<string> str, int progress)
        {
            string strRunningSave = "RunningSave|";
            str.Add(Convert.ToString(progress));
            strRunningSave += string.Join(Environment.NewLine, str.ToArray());
            byte[] bufferUpdate = Encoding.ASCII.GetBytes(strRunningSave);
            _clientSockets.ForEach(delegate (Socket socket)
            {
                socket.BeginSend(bufferUpdate, 0, bufferUpdate.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            });
        }

        /// <summary>
        /// Method which allows to display and initialize the running page  to the client
        /// </summary>
        public void SendRunningPageToClients(List<string> SaveNames)
        {
            string strRunningSave = "RunningPage|";
            strRunningSave += string.Join(Environment.NewLine, SaveNames.ToArray());
            byte[] bufferUpdate = Encoding.ASCII.GetBytes(strRunningSave);
            _clientSockets.ForEach(delegate (Socket socket)
            {
                socket.BeginSend(bufferUpdate, 0, bufferUpdate.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            });
        }


        /// <summary>
        /// Method which alllows to resume one or all running saves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResumeSaveClick(object sender, RoutedEventArgs e)
        {
            if (OneSave_Radio.IsChecked == true)
            {
                DataRowView dataRowView = (DataRowView)DgProgressSaves.SelectedItem;
                if (dataRowView != null)
                {
                    string currentSave = (string)dataRowView[Convert.ToString(Languages.objLanguage.SelectToken("job_name_label"))];
                    PauseSaveList[currentSave].Set();
                }
            }
            else
            {
                foreach (KeyValuePair<string, ManualResetEvent> PauseSave in PauseSaveList)
                {
                    PauseSave.Value.Set();
                }
            }
        }

        /// <summary>
        /// Method to drag and move the main window
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}


