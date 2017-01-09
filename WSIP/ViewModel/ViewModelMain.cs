using Ookii.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WSIP.Helpers;
using WSIP.Model;

namespace WSIP.ViewModel
{
    class ViewModelMain : ViewModelBase
    {
        public ObservableCollection<Project> Projects { get; set; }
        public ViewModelMain()
        {
            SelectProjectFolder = new RelayCommand(selectProjectFolder);
            ProcessResults = new RelayCommand(processResults);
            Projects = new ObservableCollection<Project>();

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

        public RelayCommand SelectProjectFolder { get; private set; }
        public RelayCommand ProcessResults { get; private set; }
        public RelayCommand SortByName { get; private set; }

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
                    Projects.Add(new Model.Project(Path.GetFileName(project), project));
                }
            }

            foreach (Project project in Projects)
            {
                Task.Run(() => getProjectSize(project));
            }
        }

        private void getProjectSize(Project project)
        {
            getDirectorySize(project, project.ProjectPath);
        }
        private void getDirectorySize(Project project, string directoryPath)
        {
            string[] files = new String [0];
            string[] subdirectorys = new String[0];

            try
            {
                files = Directory.GetFiles(directoryPath);
                subdirectorys = Directory.GetDirectories(directoryPath);
            } catch (Exception ex)
            {

            }

            foreach (String file in files)
            {
                getFileSize(project, file);
            }
            foreach (String subdirectory in subdirectorys)
            {
                if (Path.GetExtension(subdirectory) == ".gdb")
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
            } catch (Exception ex)
            {

            }
        }

    }
}
