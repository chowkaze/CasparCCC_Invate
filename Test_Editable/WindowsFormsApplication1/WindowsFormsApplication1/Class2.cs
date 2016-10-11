using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class ReadWriteFile
    {

        private static string folderName = @"c:\Users\Public\Document\InvateLiveGFX_PRESET";

        private string FileDirectory = System.IO.Path.Combine(folderName, "Preset.txt");

        private string Teaminfo = System.IO.Path.Combine(folderName, "Team.txt");

        private string Playerinfo = System.IO.Path.Combine(folderName, "Player.txt");

        private string Casterinfo = System.IO.Path.Combine(folderName, "Caster.txt");

        public void CreateFileOrFolder()
        {
           

            System.IO.Directory.CreateDirectory(folderName);

            if (!System.IO.File.Exists(FileDirectory))
            {
                System.IO.File.Create(FileDirectory);
            }

            if (!System.IO.File.Exists(Teaminfo))
            {
                System.IO.File.Create(Teaminfo);
            }

            if (!System.IO.File.Exists(Playerinfo))
            {
                System.IO.File.Create(Playerinfo);
            }

            if (!System.IO.File.Exists(Casterinfo))
            {
                System.IO.File.Create(Casterinfo);
            }


        }
        public List<string> read_file(string filename)
        {
            
            string line;
            string directfile = "";
            List<string> val = new List<string>();

            switch (filename)
            {
                case "Preset":
                    directfile = FileDirectory;
                    break;
                case "Team":
                    directfile = Teaminfo;
                    break;
                case "Caster":
                    directfile = Casterinfo;
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(directfile);
            while ((line = file.ReadLine()) != null)
            {
               
                       val.Add(line);
                    
 
            }

            file.Close();
            return val;

        }

        public List<string> read_file(string filename, string teamname)
        {
            string directfile = Playerinfo;
            List<string> val = new List<string>();
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(directfile);
            while ((line = file.ReadLine()) != null)
            {
                if(line == teamname)
                {
                    if (line!="End") {
                        val.Add(line);
                    }
                    else
                    {
                        break;
                    }
                }

            }
            file.Close();
            return val;
        }

        public void write_file(List<string> Content,string filetype)
        {
            string directfile = "";

            switch (filetype)
            {
                case "Preset":
                    directfile = FileDirectory;
                    break;
                case "Team":
                    directfile = Teaminfo;
                    break;
                case "Caster":
                    directfile = Casterinfo;
                    break;
                case "Player":
                    directfile = Playerinfo;
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(directfile, true))
            {
                foreach (string line in Content)
                {
                    
                        file.WriteLine(line);
                    
                }
                if(filetype == "Player")
                {
                    file.WriteLine("End");
                }
            }


        }

        public void delete_file()
        {

        }

    }
}
