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
using WSIP.View;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;

namespace WSIP.ViewModel
{
    class ViewModelMain : ViewModelBase
    {
        public ViewModelMain()
        {
            SelectProjectFolderCommand = new RelayCommand(SelectProjectFolder);
            ProcessResultsCommand = new RelayCommand(ProcessResults);
            ExportDataCommand = new RelayCommand(ExportData);
            SetThemeCommand = new RelayCommand(SetTheme);
            Projects = new ObservableCollection<Project2>();

            ProcessButtonText = "Process";

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionNumber = $"Version {version.Major}.{version.Minor}";

            Settings = new Settings();

            LoadSettings();
            SetTheme(null);
        }
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

        static public Settings Settings { get; set; }

        private bool _processRunning;
        public bool ProcessRunning
        {
            get
            {
                return _processRunning;
            }
            set
            {
                _processRunning = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("ProcessNotRunning");
            }
        }

        public bool ProcessNotRunning
        {
            get
            {
                return !_processRunning;
            }
        }

        private string _versionNumber;
        public string VersionNumber
        {
            get
            {
                return _versionNumber;
            }
            set
            {
                _versionNumber = value;
                NotifyPropertyChanged();
            }
        }

        private string _themeName;
        public string ThemeName
        {
            get
            {
                return _themeName;
            }
            set
            {
                _themeName = value;
                NotifyPropertyChanged();
            }
        }

        private double _progressPercent;
        public double ProgressPercent
        {
            get { return _progressPercent; }
            set
            {
                if (_progressPercent != value)
                {
                    _progressPercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // Relay Commands
        public RelayCommand SelectProjectFolderCommand { get; private set; }
        public RelayCommand ProcessResultsCommand { get; private set; }
        public RelayCommand ExportDataCommand { get; private set; }
        public RelayCommand SetThemeCommand { get; private set; }

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
                    var view = new AlertView
                    {
                        DataContext = new AlertViewModel("Project Folder Doesn't Exist")
                    };
                    await DialogHost.Show(view, "RootDialog");
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
                ProcessRunning = true;
                ProgressPercent = 0;

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
                    ProgressPercent = 100;

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

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV | *.csv"
            };

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
                        List<String> data = new List<string>
                        {
                            project.Name,
                            project.SimpleSize2.ToString(),
                            project.NumberOfGDB.ToString(),
                            project.NumberOfLAS.ToString(),
                            project.NumberOfTIF.ToString(),
                            project.Owner,
                            project.DateCreated,
                            project.DateLastModified,
                            project.CustomCheckBox.ToString()
                        };

                        wr.WriteLine(String.Join(",", data));
                    }                     
                }
            }
        }
    
        private void SetTheme(object parameter)
        {
            try
            {
                new PaletteHelper().SetLightDark(Settings.DarkMode);
                ThemeName = Settings.DarkMode ? "Dark" : "Light";
            }
            catch
            {
                Settings.ResetSettings();
                new PaletteHelper().SetLightDark(Settings.DarkMode);
                ThemeName = Settings.DarkMode ? "Dark" : "Light";
            }
            SaveSettings();
        }

        #endregion

        #region Methods

        private void ResetProcessButton()
        {
            ProcessButtonText = "Process";
            ProcessRunning = false;
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
            ProgressPercent += 100 / Projects.Count();
        }

        private void GetDirectoryInfo(Project2 project, string directoryPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

            project.DateCreated = dirInfo.CreationTime.ToShortDateString();
            project.DateLastModified = dirInfo.LastWriteTime.ToShortDateString();

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

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SaveInfo.SettingsFile))
                {
                    string save = File.ReadAllText(SaveInfo.SettingsFile);
                    Settings = JsonConvert.DeserializeObject<Model.Settings>(save);
                }
            }
            catch
            {

            }
        }

        private void SaveSettings()
        {
            try
            {
                if (!Directory.Exists(SaveInfo.SaveFolder))
                {
                    Directory.CreateDirectory(SaveInfo.SaveFolder);
                }

                var save = JsonConvert.SerializeObject(Settings, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                File.WriteAllText(SaveInfo.SettingsFile, save);
            }
            catch
            {

            }
        }

        #endregion 

    }
}
