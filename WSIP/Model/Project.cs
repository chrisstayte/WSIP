using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WSIP.Model
{
    class Project : INotifyPropertyChanged
    {
        private string _name;
        private int _numberOfGDB;
        private double _size;
        private string _projectPath;
       
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Project(string name, string path)
        {
            _name = name;
            _projectPath = path;
            NumberOfGDB = 0;
            Size = 0;
        }

        public string Name
        {
            get
            {
                return _name;
            } 
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        public int NumberOfGDB
        {
            get
            {
                return _numberOfGDB;
            } 
            set
            {
                if (_numberOfGDB != value)
                {
                    _numberOfGDB = value;
                    NotifyPropertyChanged();
                }
            }
        } 

        public double Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("SimpleSize");
                }
            }
        }

        public string ProjectPath
        {
            get
            {
                return _projectPath;
            }
        }

        public string SimpleSize
        {
            get
            {
                if (_size == 0)
                {
                    return "Not Calculated";
                }
                double size = _size / 1048576.0;    // To MB
                if (size < 1)
                    return "Empty";
                size = size / 1024.0;                // To GB
                return Math.Round(size, 2).ToString();
            }
        }


    }
}
