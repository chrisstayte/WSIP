using Ookii.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using WSIP.Helpers;
using WSIP.Model;

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

        public RelayCommand SelectProjectFolder { get; private set; }
        public RelayCommand ProcessResults { get; private set; }

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
            SelectProjectFolder = new RelayCommand(selectProjectFolder);
            ProcessResults = new RelayCommand(processResults);
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

        private void selectProjectFolder(object parameter)
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

        private void processResults(object parameter)
        {
    
            if (ProcessButtonText == "Process")
            {
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;

                if (!Directory.Exists(_projectFolder))
                {
                    System.Windows.MessageBox.Show("Project Folder Doesn't Exist!", "WSIP", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Projects.Clear();

                List<string> projects = Directory.GetDirectories(_projectFolder).ToList();
                foreach (string project in projects)
                {
                    if (project != null)
                    {
                        Projects.Add(new Model.Project2(Path.GetFileName(project), project));
                    }
                }

                ProcessButtonText = "Cancel";

                foreach (Project2 project in Projects)
                {
                    Task.Run(() => getProjectSize(project), token);
                    
                }
            } else
            {
                tokenSource.Cancel();
                ProcessButtonText = "Process";
            }
            
        }

        #endregion

        #region Methods

        private void getProjectSize(Project2 project)
        {
            project.ProcessStatus = "Processing";
            getDirectorySize(project, project.ProjectPath);
            if (token.IsCancellationRequested)
                project.ProcessStatus = "Cancelled";
            else
                project.ProcessStatus = "Done";
        }

        private void getDirectorySize(Project2 project, string directoryPath)
        {
            string[] files = new String[0];
            string[] subdirectorys = new String[0];

            try
            {
                files = Directory.GetFiles(directoryPath);
                subdirectorys = Directory.GetDirectories(directoryPath);
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }
            if (token.IsCancellationRequested)
                return;

            foreach (String file in files)
            {
                if (token.IsCancellationRequested)
                    return;
                if (Path.GetExtension(file).ToLower() == ".tif")
                    project.NumberOfTIF += 1;
                if (Path.GetExtension(file).ToLower() == ".las")
                    project.NumberOfLAS += 1;
                    
                getFileSize(project, file);
            }
            foreach (String subdirectory in subdirectorys)
            {
                if (Path.GetExtension(subdirectory).ToLower() == ".gdb")
                    project.NumberOfGDB += 1;
                getDirectorySize(project, subdirectory);
            }

        }

        private void getFileSize(Project project, string filePath)
        {
            try
            {
                FileInfo fi = new FileInfo(filePath);
                project.Size += fi.Length;
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }
        }

        #endregion

    }
}
