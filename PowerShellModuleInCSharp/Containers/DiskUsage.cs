using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellModuleInCSharp.Containers
{
    class DiskUsage //DiskUsage is a Report Object
    {
        public string Name { get; set; } //Name of report

        public DateTime Date { get; set; } //Date when report run

        public string Drive { get; set; } //Name of drive 

        public string Export { get; set; } //Where html report saved

    }
}
