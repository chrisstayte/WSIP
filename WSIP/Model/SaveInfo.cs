using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSIP.Model
{
    class SaveInfo
    {
        private static string _saveFolder
        {
            get
            {
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WSIP");
            }
        }

        public static string SaveFolder
        {
            get
            {
                return _saveFolder;
            }
        }

        public static string SettingsFile
        {
            get
            {
                return Path.Combine(_saveFolder, "settings.json");
            }
        }
    }
}
