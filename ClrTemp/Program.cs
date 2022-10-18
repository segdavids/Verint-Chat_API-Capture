using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrTemp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //DELETE ANY EXISTING FILE IN THE FOLDER
            System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\inetpub\wwwroot\Temp\");

            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
        }
    }
}
