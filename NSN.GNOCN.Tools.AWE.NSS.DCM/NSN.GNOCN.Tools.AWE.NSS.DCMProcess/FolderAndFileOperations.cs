using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NSN.GNOCN.Tools.AWE.NSS.DCMCSProcess
{
    public class FolderAndFileOperations
    {        
        public void DeleteAllFilesAndFolders(string rootPath, string days)
        {
            try
            {
                var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), rootPath));               
                directory.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(d => d.CreationTime.AddDays(Convert.ToInt32(days)) < DateTime.Now).ToList().ForEach(dir => dir.Delete(true));
                
                //directory.GetDirectories("*", SearchOption.TopDirectoryOnly).Select(d => d.CreationTime < DateTime.Now.AddDays(0)).ToList();
                //var fileGenerationDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), path));
                //// fileGenerationDir.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(file => file.Delete()); // For Deleting of all files available in folder or sub folders
                //fileGenerationDir.GetDirectories("*", SearchOption.AllDirectories).ToList().ForEach(dir => dir.Delete(true));
            }
            catch 
            {

            }

            
        }
    }
}
