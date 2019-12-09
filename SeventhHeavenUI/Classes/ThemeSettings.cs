using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeaven.Classes
{
    public class ThemeSettings
    {
        public string Name { get; set; } // "Dark Mode", "Light Mode", "Custom"

        public string PrimaryAppBackground { get; set; }
        public string SecondaryAppBackground { get; set; }
        public string PrimaryControlBackground { get; set; }
        public string PrimaryControlForeground { get; set; }
        public string PrimaryControlPressed { get; set; }
        public string PrimaryControlMouseOver { get; set; }
        public string PrimaryControlDisabledBackground { get; set; }
        public string PrimaryControlDisabledForeground { get; set; }
    }
}
