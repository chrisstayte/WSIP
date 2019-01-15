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

        public Project2(string name, string path) : base(name, path)
        {
            _numberOfLAS = 0;
            _numberOfTIF = 0;
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
                if(_numberOfTIF != value)
                {
                    _numberOfTIF = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
    }
}
