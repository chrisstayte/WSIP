using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSIP.Model
{
    class Project2 : Project
    {
        private int _numberOfLAS;
        private int _numberOfTIF;
        private string _processStatus;
        private string _owner;
        private string _dateCreated;

        public Project2(string name, string path) : base(name, path)
        {
            _numberOfLAS = 0;
            _numberOfTIF = 0;
            _processStatus = "Not Started";
            _owner = "Unknown";
            _dateCreated = "Unknown";
        }


        public int NumberOfLAS
        {
            get
            {
                return _numberOfLAS;
            }
            set
            {
                if (_numberOfLAS != value)
                {
                    _numberOfLAS = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        public int NumberOfTIF
        {
            get
            {
                return _numberOfTIF;
            }
            set
            {
                if (_numberOfTIF != value)
                {
                    _numberOfTIF = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        public string ProcessStatus
        {
            get
            {
                return _processStatus;
            }
            set
            {
                _processStatus = value;
                NotifyPropertyChanged();
            }
        }

        public string Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
                NotifyPropertyChanged("Owner");
            }
        }

        public string DateCreated
        {
            get
            {
                return _dateCreated;
            }
            set
            {
                _dateCreated = value;
                NotifyPropertyChanged("DateCreated");
            }
        }


        public override double Size
        {
            get
            {
                return base.Size;
            } set
            {
                if (base.Size != value)
                {
                    base.Size = value;
                    NotifyPropertyChanged("SimpleSize2");
                }
            }
        }

        public double SimpleSize2
        {
            get
            {
                double size = base.Size / 1048576.0;    // To MB
                size = size / 1024.0;                // To GB
                return Math.Round(size, 2);
            }
        }
    }
}
