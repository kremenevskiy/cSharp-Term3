using System;
using System.Drawing;
using System.IO;

namespace FileExplorer
{
    public class FileExplorer
    {
        private static FileExplorer _fileExplorer;
        
        private string path;
        private DirectoryInfo dirInfo;
        
        private FileExplorer()
        {
            var currentDir = Environment.CurrentDirectory;
            this.dirInfo = new DirectoryInfo(currentDir).Parent.Parent.Parent;
            this.path = dirInfo.ToString();

        }
        
        public static FileExplorer GetInstance()
        {
            if (_fileExplorer == null)
                _fileExplorer = new FileExplorer();
            
            return _fileExplorer;
        }


        public void Run()
        {
            
            Console.Clear();
            Console.WriteLine("Current Path: " + path);
            ShowDirs(path);
            ShowFiles(path);
            
            


        }

        private void GetInput()
        {
            // Console.WriteLine("1. Change directory");
            // Console.WriteLine("2. Delete directory");
            // Console.WriteLine("3. View File");
            // Console.WriteLine("4. Create new File");
            // Console.WriteLine("5. Delete existing File");
            // Console.WriteLine("6. Copy File");
            // Console.WriteLine("7. Rename File");
            // Console.WriteLine("8. cd ../");
            
            
        }

        private void ShowDirs(string path)
        {
            Console.WriteLine("Subdirs:");
            foreach (var subDir in dirInfo.GetDirectories())
            {
                Console.WriteLine("\t" + subDir.Name + "/");
            }
        }

        private void ShowFiles(string path)
        {
            Console.WriteLine("Files: ");
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                Console.WriteLine("\t" + file.Name);
            }
        }
    }
}