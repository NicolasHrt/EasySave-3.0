using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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


namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket _clientSocket;
        private byte[] _buffer;

        public static List<ManualResetEvent> PauseSaveList;
        public static Dictionary<string, BackgroundWorker> WorkerList;
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        public bool IsDarkTheme { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _buffer = new byte[1024];
            StartConnect();
            StartReceiving();
        }

        /// <summary>
        /// Method to add a save to the list of saves to run
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addToRunList(object sender, MouseButtonEventArgs e)
        {
            bool found = false;
            DataRowView dataRowView = (DataRowView)BackupJob.SelectedItem;
            string jobName = Convert.ToString("Job Name");

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

                listJob.Items.Add(dataRowView[jobName]);
            }

            else
            {
                if (dataRowView[jobName] == null)
                {
                    System.Windows.MessageBox.Show("You does'nt have selected a job");
                }
                else
                {
                    System.Windows.MessageBox.Show("You have already selected this job");
                }
            }
        }

        /// <summary>
        /// Method to remove a save to the list of saves to run
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
                System.Windows.MessageBox.Show("You already have deleted this job to the run list");
            }
        }

        private void Launcher_Click(object sender, RoutedEventArgs e)
        {
            RunningSave.Visibility = Visibility.Visible;


            //launcher();
        }

        /// <summary>
        /// Method that launch saves
        /// </summary>
        /// <param name="newList"></param>
        void launcher(List<string> newList)
        {
            Dispatcher.Invoke(() =>
            {
                TBAffichage.Text = "";
            });
            List<string> listSave = new List<string>();

            DataTable DataSet = new DataTable();
            DataSet.Columns.Add("name");
            DataSet.Columns.Add("Progression (%)");
            DataSet.Columns.Add("Status");


            /*  DataSet.Columns.Add("state", typeof(System.Windows.Controls.ProgressBar));*/

            foreach (Object item in newList)
            {
                listSave.Add(Convert.ToString(item));

                DataRow dr = DataSet.NewRow();

                Dispatcher.Invoke(() =>
                {
                    int pBar = 0;
                    dr[1] = pBar;
                    dr[0] = item;
                    dr[2] = "Running";


                    DataSet.Rows.Add(dr);
                    DgProgressSaves.ItemsSource = null;
                    DgProgressSaves.ItemsSource = DataSet.DefaultView;
                });
            }

        }

        /// <summary>
        /// Method called when there is a change in the BGW "worker"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {


            //initialisation de la barre de progression avec le pourcentage de progression
            List<string> str = new List<string>();
            str = (List<string>)e.UserState;
            int row = 0;
            if (str[0] == "running")
            {
                foreach (DataRowView drv in DgProgressSaves.Items)
                {
                    // compare value in datarow of view
                    if (str[1].Contains((string)drv.Row.ItemArray[0]))
                    {
                        // select item
                        DataRowView rowView = DgProgressSaves.Items[row] as DataRowView; //Get RowView
                        rowView.BeginEdit();
                        rowView[1] = e.ProgressPercentage.ToString();
                        rowView.EndEdit();

                        if (Convert.ToInt32(e.ProgressPercentage) != 100)
                        {
                            rowView.BeginEdit();
                            rowView[2] = "Running";
                            rowView.EndEdit();
                        }
                        else
                        {
                            rowView.BeginEdit();
                            rowView[2] = "Completed";
                            rowView.EndEdit();
                        }

                        DgProgressSaves.Items.Refresh(); // Refresh table

                    }
                    row++;

                }
            }

            else if (str[0] == "completed")
            {
                Dispatcher.Invoke(() =>
                {
                    TBAffichage.Inlines.Add("----------------------------------------------- \n");
                    TBAffichage.Inlines.Add(str[2] + " \n");
                    TBAffichage.Inlines.Add(str[3] + " \n");
                    TBAffichage.Inlines.Add(str[4] + " \n");
                    TBAffichage.Inlines.Add(str[5] + " \n");
                    TBAffichage.Inlines.Add(str[6] + " \n");
                });
                

            }
            else if (str[0] == "error")
            {
                Dispatcher.Invoke(() =>
                {
                    TBAffichage.Inlines.Add("----------- ERROR -----------\n");
                    TBAffichage.Inlines.Add(str[1] + "\n");
                });
                

            }

            else if (str[0] == "stoped")
            {
                foreach (DataRowView drv in DgProgressSaves.Items)
                {
                    // compare value in datarow of view
                    if (str[1].Contains((string)drv.Row.ItemArray[0]))
                    {
                        // select item
                        DataRowView rowView = DgProgressSaves.Items[row] as DataRowView; //Get RowView
                        rowView.BeginEdit();
                        rowView[2] = "Canceled";
                        rowView.EndEdit();
                    }
                }

            }

            /*  if (e.UserState == null)
           {
               pbStatus.Value = e.ProgressPercentage;
           }*/

        }

        /// <summary>
        /// Method to change the current background of the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            }

            //else set IsDarkTheme to true and SetBaseTheme to dark
            else
            {
                IsDarkTheme = true;
                theme.SetBaseTheme(Theme.Dark);
            }

            //to apply the changes use the SetTheme function
            paletteHelper.SetTheme(theme);
            //===================================>
        }

        /// <summary>
        /// Method to exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitApp(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Method to leave the Running Page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeaveRunning(object sender, RoutedEventArgs e)
        {
            RunningSave.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// Method to Pause a save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseSaveClick(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)DgProgressSaves.SelectedItem;
            string currentSave = (string)dataRowView["name"];
            WorkerList[currentSave].CancelAsync();



            //string currSave = "TreadRun_" + (string)dataRowView["name"];
            //foreach (ProcessThread thread in currentThreads)
            //{
            //    var varTest = thread;

            //    // Do whatever you need
            //    if (varTest == currSave)
            //    {
            //        thread.Abort();
            //    }
        }
        /// <summary>
        /// Method to try to connect with the server when the client application is running
        /// </summary>
        private void LoopConnect()
        {
            int attempts = 0;

            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(IPAddress.Parse("127.0.0.1"), 11000);
                    StartReceiving();
                }
                catch (SocketException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        TextConnect.Text = null;
                        TextConnect.Text = "SERVER CONNECTION : Connection attempts: " + attempts.ToString();
                    });

                }

            }
            Dispatcher.Invoke(() =>
            {
                TextConnect.Text = null;
                TextConnect.Text = "SERVER CONNECTION : Connected !";
            });

        }
        /// <summary>
        /// Method to listen to incoming datas
        /// </summary>
        private void StartReceiving()
        {
            //Thread.Sleep(1000);
            while (_clientSocket.Connected)
            {
                try
                {
                    _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, _clientSocket);
                    break;
                }
                catch (SocketException) { }
            }
        }
        /// <summary>
        /// Callback method of StartReceiving method
        /// </summary>
        /// <param name="AR"></param>
        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                //Get data and put it in a string
                int lengthrec = _clientSocket.EndReceive(AR);
                byte[] data = new byte[lengthrec];
                Array.Copy(_buffer, data, lengthrec);
                string strbuffer = Encoding.ASCII.GetString(data);
                List<string> DataWithType = strbuffer.Split("|").ToList();
                string flag = DataWithType[0];

                if(flag == "SendJobs")
                {
                    DataTable Jobs = new DataTable();
                    Jobs.Columns.Add("Name");
                    Jobs.Columns.Add("Source Repository");
                    Jobs.Columns.Add("Target Repository");
                    Jobs.Columns.Add("Save Type");
                    List<string> JobsList = DataWithType[1].Split(Environment.NewLine).ToList();
                    foreach (string Job in JobsList)
                    {
                        DataRow dr = Jobs.NewRow();
                        List<string> value = Job.Split(";").ToList();
                        for (int i = 0; i < value.Count; i++)
                        {
                            dr[i] = value[i];
                        }

                        Jobs.Rows.Add(dr);

                    }
                    Dispatcher.Invoke(() =>
                    {
                        BackupJob.ItemsSource = null;
                        BackupJob.ItemsSource = Jobs.DefaultView;
                    });
                }
                else if (flag == "RunningSave")
                {
                    List<string> UpdateList = DataWithType[1].Split(Environment.NewLine).ToList();
                    editRunSaveRows(UpdateList);
                }
                else if (flag == "RunningPage")
                {
                    List<string> listSaves = DataWithType[1].Split(Environment.NewLine).ToList();
                    Dispatcher.Invoke(() =>
                    {
                        RunningSave.Visibility = Visibility.Visible;
                    });
                    launcher(listSaves);
                }
                
                StartReceiving();

            }
            catch { }

        }
        /// <summary>
        /// Method to run asynchronously the LoopConnect method
        /// </summary>
        private void StartConnect()
        {
            Task TaskConnect = Task.Run(() => {
                LoopConnect();
            });
        }
        /// <summary>
        /// Method to edit in real time saves informations for the client
        /// </summary>
        /// <param name="str"></param>
        void editRunSaveRows(List<string> str)
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
                    if (int.TryParse(str[2], out int value))
                    {
                        rowView[1] = str[2];
                    }
                    rowView[2] = str[0];
                    rowView.EndEdit();
                    Dispatcher.Invoke(() =>
                    {
                        DgProgressSaves.Items.Refresh(); // Refresh table
                    });
                    
                }
                row++;
            }

            if (str[0] == "running")
            {
                //Do work if running
            }

            else if (str[0] == "completed")
            {
                //Do work if completed
                Dispatcher.Invoke(() =>
                {
                    TBAffichage.Inlines.Add("----------------------------------------------- \n");
                    TBAffichage.Inlines.Add(str[2] + " :\n \n");
                    TBAffichage.Inlines.Add(str[3] + " \n");
                    TBAffichage.Inlines.Add(str[4] + " \n");
                    TBAffichage.Inlines.Add(str[5] + " \n");
                    TBAffichage.Inlines.Add(str[6] + " \n \n");
                });

            }

            else if (str[0] == "error")
            {
                //Do work if error
                Dispatcher.Invoke(() =>
                {
                    TBAffichage.Inlines.Add("----------- ERROR -----------\n");
                    TBAffichage.Inlines.Add(str[1] + " :\n \n");
                    TBAffichage.Inlines.Add(str[2] + " \n \n");
                });

            }

            else if (str[0] == "stoped")
            {
                //Do work if stoped
                Dispatcher.Invoke(() =>
                {
                    TBAffichage.Inlines.Add("----------- Stop Action -----------\n");
                    TBAffichage.Inlines.Add(str[1] + " :\n \n");
                    TBAffichage.Inlines.Add("La run a bien été stoppé." + " \n \n");
                });

            }
        }
    }
}
