using Ookii.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using WSIP.Helpers;
using WSIP.Model;
using System.Data;
namespace WSIP.ViewModel
{
    class ViewModelMain : ViewModelBase
    {
        string _projectFolder;
        public string ProjectFolder
        {
            get
            {
                return _projectFolder;
            }
            set
            {
                if (_projectFolder != value)
                {
                    _projectFolder = value;
                    RaisePropertyChanged("ProjectFolder");
                }
            }
        }


        // Relay Commands
        public RelayCommand SelectProjectFolderCommand { get; private set; }
        public RelayCommand ProcessResultsCommand { get; private set; }
        public RelayCommand ExportDataCommand { get; private set; }

        private List<String> _autocompletePaths;
        public List<String> AutoCompletePaths
        {
            get
            {
                return _autocompletePaths;
            }
            set
            {
                _autocompletePaths = value;
                NotifyPropertyChanged();
            }
        }
 
        private CollectionViewSource _projectsDataView;
        public ListCollectionView ProjectsDataView
        {
            get
            {
                return (ListCollectionView)_projectsDataView.View;
            }
        }

        private ObservableCollection<Project2> _projects;
        public ObservableCollection<Project2> Projects
        {
            get
            {
                return _projects;
            }
            set
            {
                _projects = value;
                _projectsDataView = new CollectionViewSource
                {
                    Source = _projects
                };
            }
        }

        private string _processButtonText;
        public string ProcessButtonText
        {
            get
            {
                return _processButtonText;
            }
            set
            {
                _processButtonText = value;
                NotifyPropertyChanged("ProcessButtonText");
            }
        }


        private CancellationTokenSource tokenSource;
        private CancellationToken token;

        public ViewModelMain()
        {
            SelectProjectFolderCommand = new RelayCommand(SelectProjectFolder);
            ProcessResultsCommand = new RelayCommand(ProcessResults);
            ExportDataCommand = new RelayCommand(ExportData);
            Projects = new ObservableCollection<Project2>();

            ProcessButtonText = "Process";

            

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }


        #region Events

        private void SelectProjectFolder(object parameter)
        {
            Debug.WriteLine("Parameter: " + (string)parameter);
            Debug.WriteLine("Variable: " + _projectFolder);
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            if (Directory.Exists((string)parameter))
                folderBrowserDialog.SelectedPath = (string)parameter;

            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                System.Windows.MessageBox.Show("Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "WSIP");
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                ProjectFolder = folderBrowserDialog.SelectedPath;
            
        }

        private async void ProcessResults(object parameter)
        {
            
      
            if (ProcessButtonText == "Process")
            {
                if (!Directory.Exists(_projectFolder))
                {
                    System.Windows.MessageBox.Show("Project Folder Doesn't Exist!", "WSIP", MessageBoxButton.OK, MessageBoxImage.Error);
                    //_notificationManager.Show(new NotificationContent
                    //{
                    //    Title = "What Should I Purge",
                    //    Message = "Project Folder Doesn't Exist",
                    //    Type = NotificationType.Error,
                    //});
                    return;
                }

                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;

                //_notificationManager.Show(new NotificationContent
                //{
                //    Title = "What Should I Purge",
                //    Message = "Processing",
                //    Type = NotificationType.Information
                //});

                Projects.Clear();

                List<string> projects = Directory.GetDirectories(_projectFolder).ToList();
                List<string> autocompletePaths = new List<string>();
                foreach (string project in projects)
                {
                    if (project != null)
                    {
                        autocompletePaths.Add(project);
                        Projects.Add(new Model.Project2(Path.GetFileName(project), project));
                    }
                }

                AutoCompletePaths = autocompletePaths;
                ProcessButtonText = "Cancel";

                var tasks = new List<Task>();

                tasks.AddRange(Projects.Select(project => Task.Run(() => GetProjectInformation(project), token)));

                Task task = Task.WhenAll(tasks.ToArray());

                try
                {
                    await task;
                }
                catch { }

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    ResetProcessButton();

                    //if (!token.IsCancellationRequested)
                    //_notificationManager.Show(new NotificationContent
                    //{
                    //    Title = "What Should I Purge",
                    //    Message = "Processed All Projects",
                    //    Type = NotificationType.Success,
                    //});
                }
                    

            } else
            {
                tokenSource.Cancel();
                ResetProcessButton();

                //_notificationManager.Show(new NotificationContent
                //{
                //    Title = "What Should I Purge",
                //    Message = "Cancelled",
                //    Type = NotificationType.Warning
                //});
            }
            
        }

        private void SelectedProject(object parameter, SelectedCellsChangedEventArgs e)
        {
            ProjectFolder = String.Format(@"{0}\{1}", _projectFolder, (string)parameter);
        }

        private void ExportData(object parameter)
        {
            System.Windows.Controls.DataGrid dataGrid = parameter as System.Windows.Controls.DataGrid;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "CSV | *.csv";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string exportPath = dialog.FileName;

                List<String> headers = new List<string>();

                using (StreamWriter wr = new StreamWriter(exportPath))
                {
                    int cols = dataGrid.Columns.Count;

                    for (int i = 1; i < cols; i++)
                    {
                        headers.Add(dataGrid.Columns[i].Header.ToString());
                    }

                   
                    wr.WriteLine(String.Join(",", headers));

                    foreach (Project2 project in _projects)
                    {
                        List<String> data = new List<string>();

                        data.Add(project.Name);
                        data.Add(project.SimpleSize2.ToString());
                        data.Add(project.NumberOfGDB.ToString());
                        data.Add(project.NumberOfLAS.ToString());
                        data.Add(project.NumberOfTIF.ToString());
                        data.Add(project.Owner);
                        data.Add(project.DateCreated);
                        data.Add(project.CustomCheckBox.ToString());

                        wr.WriteLine(String.Join(",", data));
                    }

                   
                    
                    
                }

                

            }

        }


        #endregion

        #region Methods

        private void ResetProcessButton()
        {
            ProcessButtonText = "Process";
        }
             
        private void GetProjectInformation(Project2 project)
        {
            project.ProcessStatus = "Processing";
            GetDirectoryInfo(project, project.ProjectPath);
            GetDirectorySize(project, project.ProjectPath);
            if (token.IsCancellationRequested)
                project.ProcessStatus = "Cancelled";
            else
                project.ProcessStatus = "Done";
        }

        private void GetDirectoryInfo(Project2 project, string directoryPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

            project.DateCreated = String.Format("{0}/{1}/{2}", dirInfo.CreationTime.Month, dirInfo.CreationTime.Day, dirInfo.CreationTime.Year);

            string owner = "Unknown";

            try
            {
                owner = System.IO.File.GetAccessControl(directoryPath).GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
                owner = Path.GetFileName(owner);
            }
            catch  (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            project.Owner = owner;
        }

        private void GetDirectorySize(Project2 project, string directoryPath)
        {
            string[] files = new String[0];
            string[] subdirectorys = new String[0];

            try
            {
                files = Directory.GetFiles(directoryPath);
                subdirectorys = Directory.GetDirectories(directoryPath);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // If Cancel Return
            if (token.IsCancellationRequested)
                return;

            foreach (String file in files)
            {
                // If Cancel Return
                if (token.IsCancellationRequested)
                    return;
                if (Path.GetExtension(file).ToLower() == ".tif")
                    project.NumberOfTIF += 1;
                if (Path.GetExtension(file).ToLower() == ".las")
                    project.NumberOfLAS += 1;
                    
                GetFileSize(project, file);
            }
            foreach (String subdirectory in subdirectorys)
            {
                // If Cancel Return
                if (token.IsCancellationRequested)
                    return;
                if (Path.GetExtension(subdirectory).ToLower() == ".gdb")
                    project.NumberOfGDB += 1;
                GetDirectorySize(project, subdirectory);
            }

        }

        private void GetFileSize(Project project, string filePath)
        {
            try
            {
                FileInfo fi = new FileInfo(filePath);
                project.Size += fi.Length;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion 

    }
}
