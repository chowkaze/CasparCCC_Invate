using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class CGDataItem
    {
        private String Rawdatafile;
        public String Datafilename { get; private set; }



        public CGDataItem(string Rawfile)
        {
            Rawdatafile = Rawfile;

            Datafilename = Rawdatafile.Substring(1, Rawdatafile.IndexOf("\"", 1) - 1);

        }

        public override string ToString()
        {
            return Datafilename;
        }
    }
}
