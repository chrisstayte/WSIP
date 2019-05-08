using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WSIP.Model
{
    class Settings : INotifyPropertyChanged
    {
        public Settings()
        {
            DarkMode = false;
        }

        public void ResetSettings()
        {
            DarkMode = false;
        }


        // Basic ViewModelBase
        internal void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        internal void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _darkMode;
        public bool DarkMode
        {
            get
            {
                return _darkMode;
            }
            set
            {
                _darkMode = value;
                NotifyPropertyChanged();
            }
        }
    }
}
