using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace ETL
{
    class Server
    {
        private string sourceDirPath;
        private string targetDirPath;


        public string temp;

        public Server(string sourceDirPath = "desktop")
        {

            if (sourceDirPath == "desktop")
            {
                sourceDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }


            sourceDirPath = Path.Combine(sourceDirPath, "SourceDirectory");
            
            if (!Directory.Exists(sourceDirPath))
            {
                sourceDirPath = Directory.CreateDirectory(sourceDirPath).FullName;
            }

            this.sourceDirPath = sourceDirPath;


            // creating targerDirectory in project directory
            var projectDir = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var parentProjectDirPath = projectDir.Parent.Parent.Parent.FullName;

            targetDirPath = Directory.CreateDirectory(Path.Combine(parentProjectDirPath, "TargetDirectory")).FullName;

           
        }


        public void RunWather()
        {
            using(FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = sourceDirPath;
                watcher.Created += OnCreated;
                watcher.Filter = "*.txt";
                watcher.EnableRaisingEvents = true;


                
                Console.WriteLine("Press esc to stop wantching : " + sourceDirPath);
                while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
        }


        private void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm") + " Added new file");

            // create a zip from file
            string zipPath = makeZip(sourceDirPath, e.FullPath);


            // deelete old file
            File.Delete(e.FullPath);


            // path for newLocation of zip
            string newZipPath = Path.Combine(targetDirPath, Path.GetFileName(zipPath));


            if (File.Exists(newZipPath))
            {
                Console.WriteLine("Trying to move file that already exsists");
                return;
            }
            else
            {
                File.Move(zipPath, newZipPath);
            }

            // now decompress file
            string compressedFilePath = null;
            DirectoryInfo di = new DirectoryInfo(targetDirPath);

            foreach(FileInfo fi in di.GetFiles())
            {
                if (fi.Extension == ".zip")
                {
                    compressedFilePath = fi.FullName;
                }
            }

            if (compressedFilePath != null)
            {
                ZipFile.ExtractToDirectory(compressedFilePath, targetDirPath);
            }
            Console.WriteLine($"Extracted new file in {this.targetDirPath} at {DateTime.Now.ToString("HH::mm")}");

        }
        
        private string makeZip(string path, string filePath)
        {
            var zipPath = Path.Combine(path, $"archive_{DateTime.Now.ToString("HH_mm_ss")}.zip");

            using (FileStream zipFile = File.Open(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(filePath, $"{DateTime.Now.ToString("HH_mm_ss")}.txt");
            }

            return zipPath;
        }
    }
}
