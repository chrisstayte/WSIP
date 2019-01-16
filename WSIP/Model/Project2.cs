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
        private bool _completedProcessing;

        public Project2(string name, string path) : base(name, path)
        {
            _numberOfLAS = 0;
            _numberOfTIF = 0;
            _completedProcessing = false;
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

        public bool CompletedProcessing
        {
            get
            {
                return _completedProcessing;
            }
            set
            {
                _completedProcessing = value;
                NotifyPropertyChanged();
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
