using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;


namespace FileExplorer
{
    public partial class FileExplorer
    {
        private static FileExplorer _fileExplorer;
        private static FileSystemWatcher fileWatcher;
        
        private DirectoryInfo dirInfo;
        
        private FileExplorer()
        {
           

            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            this.dirInfo = new DirectoryInfo(homeDir);

        }
        
        public static FileExplorer GetInstance()
        {
            if (_fileExplorer == null)
                _fileExplorer = new FileExplorer();
            
            return _fileExplorer;
        }


        public void Run()
        {
            string path = dirInfo.FullName;
            
            Console.WriteLine("Current Path: " + path);
            Console.WriteLine("Type - \"help\" - go get help");
           


            while (true)
            {
                GetInput();
            }
            
        }

        private void GetInput()
        {
            // Console.WriteLine("1. Change directory");
            // Console.WriteLine("2. Delete directory");
            // Console.WriteLine("2. Make directory");
            // Console.WriteLine("3. View File");
            // Console.WriteLine("4. Create new File");
            // Console.WriteLine("5. Delete existing File");
            // Console.WriteLine("6. Copy File");
            // Console.WriteLine("7. Rename File");
            // Console.WriteLine("8. cd ../");



            var input = "";
            bool allWhiteSpaces;
            
            do
            {
                ShowCurrentDir(dirInfo.FullName);
                input = Console.ReadLine();
                
                allWhiteSpaces = input.All(ch => ch == ' ');

                if (!string.IsNullOrEmpty(input) && !allWhiteSpaces)
                    ParsingInput(input);
                
               
            } while (string.IsNullOrEmpty(input) || allWhiteSpaces);
            
            

        }

        void ShowCurrentDir(string dirPath)
        {
            
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string newPath = "";

            if (dirInfo.FullName.Length > homePath.Length)
                newPath = dirPath.Remove(0, homePath.Length);
            else
                newPath = dirInfo.Name;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(newPath + " $ ");
            Console.ResetColor();
            
        }
        
        

        private void ParsingInput(string input)
        {
            string[] argv = input.Split();
            Array.Resize(ref argv, argv.Length + 3);
            
            if (argv[0] == Commands.cd.ToString())
            {
                HandlingCd(argv[1]);
            }
            else if (argv[0] == Commands.pwd.ToString())
            {
                HandlingPwd();
            }
            else if (argv[0] == Commands.ls.ToString())
            {
                HandlingLs();
            }
            else if (argv[0] == Commands.mkdir.ToString())
            {
                HandlingMkdir(argv[1]);
            }
            else if (argv[0] == Commands.rm.ToString())
            {
                HandlingRm(argv[1]);
            }
            else if (argv[0] == Commands.cat.ToString())
            {
                ReadFile(argv[1], argv[2]);
            }
            else if (argv[0] == Commands.touch.ToString())
            {
                CreateFile(argv[1], argv[2]);
            }
            else if (argv[0] == Commands.cp.ToString())
            {
                HandlingCp(argv[1], argv[2]);
            }
            else if (argv[0] == Commands.mv.ToString())
            {
                HandlingMv(argv[1], argv[2]);
            }
            else if (argv[0] == Commands.help.ToString())
            {
                HandlingHelp();
            }
            else if (argv[0] == Commands.clear.ToString())
            {
                Console.Clear();
            }
            else if (argv[0] == Commands.exit.ToString())
            {
                Environment.Exit(0);
            }
            else if (argv[0] == Commands.watcher.ToString())
            {
                HandlingWatcher(argv[1], argv[2], argv[3]);
            }
            else
            {
                Console.WriteLine("command not found");
            }
        }


        private void CreateFile(string filePath, string zipFolder)
        {
            var fileFullPath = Path.Combine(dirInfo.FullName, filePath);
            


            if (filePath.Contains("." + Formats.txt.ToString()))
            {
                using (var fs = new FileStream(fileFullPath, FileMode.Create))
                // using (var ds = new DeflateStream(fs, CompressionLevel.NoCompression))
                using (var writer = new StreamWriter(fs))
                {
                    Console.WriteLine("Write here: ");
                    string data = Console.ReadLine();
                    writer.WriteLine(data);
                }
            }
            else if (filePath.Contains("." + Formats.bin.ToString()))
            {
                using (var fs = new FileStream(fileFullPath, FileMode.Create))
                using (var ds = new DeflateStream(fs, CompressionMode.Compress)) 
                using (var writer = new StreamWriter(ds)) 
                {
                    Console.WriteLine("Write something: ");
                    writer.Write(Console.ReadLine());
                }
                Console.WriteLine("Created compressed file: " + new FileInfo(fileFullPath).Length + "bytes length");
            }
            else if(filePath.Contains("." + Formats.zip.ToString()))
            {
                if (Directory.Exists(zipFolder))
                {
                    string zipPath = Path.Combine(dirInfo.Name, fileFullPath);
                    ZipFile.CreateFromDirectory(zipFolder, zipPath);
                }
                else
                {
                    Console.WriteLine($"Couldn't find folder: {zipFolder}");
                }
            }
            else
            {
                Console.WriteLine("Unsupported type of file!");
            }
        }


        private void ReadFile(string filePath, string zipExtractPath)
        {
            
            filePath = Path.Combine(dirInfo.FullName, filePath);
            if (File.Exists(filePath))
            {
                try
                {
                    if (filePath.Contains("." + Formats.txt.ToString()))
                    {
                        using (var fs = new FileStream(filePath, FileMode.Open))
                            // using (var ds = new DeflateStream(fs, CompressionLevel.NoCompression))
                        using (var reader = new StreamReader(fs))
                        {
                            Console.WriteLine("File info: " + reader.ReadToEnd());
                        }
                    }
                    else if (filePath.Contains("." + Formats.bin.ToString()))
                    {
                        using (var fs = new FileStream(filePath, FileMode.Open))
                        using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                        using (var reader = new StreamReader(ds))
                        {
                            Console.WriteLine($"Compressed file info: {reader.ReadToEnd()}");
                        }
                    }
                    else if (filePath.Contains("." + Formats.zip.ToString()))
                    {
                        if (string.IsNullOrEmpty(zipExtractPath))
                        {
                            Console.WriteLine("here1");
                            zipExtractPath = dirInfo.FullName;
                        }
                        else
                        {
                            Console.WriteLine("here2");
                            zipExtractPath = Path.Combine(dirInfo.Name, zipExtractPath);
                        }

                        Console.WriteLine("path: " + zipExtractPath);
                        ZipFile.ExtractToDirectory(filePath, zipExtractPath);
                        Console.WriteLine($"Zip extracted succesfully to: {zipExtractPath}");
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {e.Message}");
                    Console.ResetColor();
                }
            }
            else
                Console.WriteLine("No such file or directory");
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
            foreach (var file in dirInfo.GetFiles())
            {
                Console.WriteLine("\t" + file.Name);
            }
        }
    }

    public partial class FileExplorer
    {
        enum Commands
        {
            cd,
            pwd,
            ls,
            mkdir,
            rm,
            cat,
            touch,
            cp, 
            mv,
            help,
            clear,
            exit,
            watcher
        }

        enum Formats
        {
            txt,
            bin,
            zip
        }

        enum WorkStatus
        {
            start,
            stop,
            log
        }

        void HandlingCd(string argv2)
        {
            if (string.IsNullOrEmpty(argv2))
            {
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                dirInfo = new DirectoryInfo(homePath);
                return;
            }

            List<string> subdirs = new List<string>();
            foreach (var subdir in dirInfo.GetDirectories())
            {
                subdirs.Add(subdir.Name.ToLower());
            }


            string newPath;
            if (subdirs.Contains(argv2))
            {
                newPath = Path.Combine(dirInfo.FullName, argv2);
            }
            else if (argv2 == "../" || argv2 == "..")
            {
                newPath = dirInfo.Parent.FullName;
            }
            else if (Directory.Exists(argv2) && argv2 != "//")
            {
                newPath = argv2;
            }
            else
            {
                Console.WriteLine("No such directory");
                return;
            }

            dirInfo = new DirectoryInfo(newPath);
            
        }


        void HandlingPwd()
        {
            Console.WriteLine(dirInfo.FullName);
        }


        void HandlingLs()
        {
            string path = dirInfo.FullName;
            ShowDirs(path);
            ShowFiles(path);
        }

        void HandlingMkdir(string newDirName)
        {
            Directory.CreateDirectory(Path.Combine(dirInfo.FullName, newDirName));
        }


        void HandlingRm(string deletingPath)
        {
            if (deletingPath.StartsWith('/'))
            {
                if (File.Exists(deletingPath))
                {
                    if (ConfirmDeleting(deletingPath))
                    {
                        File.Delete(deletingPath);
                    }
                }
                else if (Directory.Exists(deletingPath))
                {
                    if (ConfirmDeleting(deletingPath))
                    {
                        Directory.Delete(deletingPath);
                    }
                }
                else
                {
                    Console.WriteLine("No such file or directory");
                }
            }
            else
            {
                string path = Path.Combine(dirInfo.FullName, deletingPath);
                if (File.Exists(path))
                {
                    if (ConfirmDeleting(path))
                    {
                        File.Delete(path);
                    }
                }
                else if(Directory.Exists(path))
                {
                    if (ConfirmDeleting(path))
                    {
                        Directory.Delete(path);
                    }
                }
                else
                {
                    Console.WriteLine("No such file or directory");
                }
            }
        }
        

        bool ConfirmDeleting(string path)
        {
            Console.Write("Are you sure to delete " + path + "? [Y/n] : ");
            string input = " ";
            while (input.ToLower()[0] != 'y' && input.ToLower()[0] != 'n')
            {
                input = Console.ReadLine();
            }

            if (input.ToLower()[0] == 'y')
                return true;
            else
                return false;
        }


        void HandlingCp(string source, string dist)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(dist))
            {
                Console.WriteLine("usage: source_file target_file");
                return;
            }


            try
            {
                string sourcePath = Path.Combine(dirInfo.FullName, source);
                string distPath = Path.Combine(dirInfo.FullName, dist);

                File.Copy(sourcePath, distPath, true);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {e.Message}");
                Console.ResetColor();
            }
        }


        void HandlingMv(string source, string dist)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(dist))
            {
                Console.WriteLine("usage: source_file target_file");
                return;
            }

            try
            {
                string sourcePath = Path.Combine(dirInfo.FullName, source);
                string distPath = Path.Combine(dirInfo.FullName, dist);

                File.Move(sourcePath, distPath);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {e.Message}");
                Console.ResetColor();
            }
        }


        void HandlingHelp()
        {
            string helpFilePath = Path.Combine(Environment.CurrentDirectory, "help.txt");
            if (File.Exists(helpFilePath))
            {
                using (var fs = new FileStream(helpFilePath, FileMode.Open))
                using (var reader = new StreamReader(fs))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
            else
            {
                Console.WriteLine("Help is not available now");
                return;
            }
        }


        void StartWatcher(string path, string filter)
        {
            fileWatcher = new FileSystemWatcher(path, filter);

            string pathLog = Path.Combine(Environment.CurrentDirectory, "log.txt");
            using (var fs = new FileStream(pathLog, FileMode.Create))
            {
            }
            
            fileWatcher.NotifyFilter = NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;

            fileWatcher.Created += FileCreated;
            // fileWatcher.Changed += FileChanged;
            fileWatcher.Deleted += FileDeleted;
            fileWatcher.Renamed += FileRenamed;
            fileWatcher.Error += FileErrorOccured;

            fileWatcher.IncludeSubdirectories = true;    
            fileWatcher.EnableRaisingEvents = true;
            
            
            
        }


        void StopWatcher()
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Dispose();
            string pathLog = Path.Combine(Environment.CurrentDirectory, "log.txt");
            
        }
        
        

        void FileCreated(object o, FileSystemEventArgs e)
        {
            string pathLog = Path.Combine(Environment.CurrentDirectory, "log.txt");
            if (!File.Exists(pathLog))
            {
                File.Create(pathLog);
            }
            using (var fs = new FileStream(pathLog, FileMode.Append))
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine($"Created file [{DateTime.Now.ToString("HH:mm:ss")}]:" +
                                 $" {GetFileName(e.Name)}\n\tLocation: {e.FullPath}");
            }
        }
        
        void FileDeleted(object o, FileSystemEventArgs e)
        {
            string pathLog = Path.Combine(Environment.CurrentDirectory, "log.txt");
            if (!File.Exists(pathLog))
            {
                File.Create(pathLog);
            }
            using (var fs = new FileStream(pathLog, FileMode.Append))
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine($"Deleted file [{DateTime.Now.ToString("HH:mm:ss")}]:" +
                                 $" {GetFileName(e.Name)}" +
                                 $"\n\tLocation: {e.FullPath}");
            }
        }
        
        void FileRenamed(object o, RenamedEventArgs e)
        {
            string pathLog = Path.Combine(Environment.CurrentDirectory, "log.txt");
            if (!File.Exists(pathLog))
            {
                File.Create(pathLog);
            }
            using (var fs = new FileStream(pathLog, FileMode.Append))
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine($"Renamed file [{DateTime.Now.ToString("HH:mm:ss")}]:" +
                                 $" From:{GetFileName(e.OldName)} ---> {GetFileName(e.Name)}" +
                                 $"\n\tLocation: {e.FullPath}");
            }
        }
        
        void FileChanged(object o, FileSystemEventArgs e)
        {
           
            string pathLog = Path.Combine(Environment.CurrentDirectory, "log.txt");
            if (!File.Exists(pathLog))
            {
                File.Create(pathLog);
            }
            using (var fs = new FileStream(pathLog, FileMode.Append))
            using (var writer = new StreamWriter(fs))
            {
                if (e.Name != pathLog)
                {
                    writer.WriteLine($"File changed [{DateTime.Now.ToString("HH:mm:ss")}]: " +
                                     $"{GetFileName(e.Name)}" +
                                     $"\n\tLocation: {e.FullPath}");
                }
            }
        }
        
        void FileErrorOccured(object o, ErrorEventArgs e)
        {
            string pathLog = Path.Combine(Environment.CurrentDirectory, "log.txt");
            if (!File.Exists(pathLog))
            { 
                File.Create(pathLog);
            }
            using (var fs = new FileStream(pathLog, FileMode.Append))
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine($"ERROR [{DateTime.Now.ToString("HH:mm:ss")}]: " +
                                 $"{e.GetException().Message}");
            }
        }


        void HandlingWatcher(string workStatus, string pathWorkingOn, string filter)
        {
            if (string.IsNullOrEmpty(filter))
                filter = "";
            if (string.IsNullOrEmpty(pathWorkingOn))
                pathWorkingOn = "/";
            if (string.IsNullOrEmpty(workStatus))
            {
                Console.WriteLine("usage: [workStatus] [pathWorkingOn] [filter]");
                return;
            }

            if (workStatus.ToLower() == WorkStatus.start.ToString())
            {
                StartWatcher(pathWorkingOn, filter);
            }
            else if (workStatus.ToLower() == WorkStatus.stop.ToString())
            {
                StopWatcher();
            }
            else if (workStatus.ToLower() == WorkStatus.log.ToString())
            {
                string pathLog = Path.Combine(Environment.CurrentDirectory, "log.txt");
                using(var fs = new FileStream(pathLog, FileMode.Open))
                using (var reader = new StreamReader(fs))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
            else
            {
                Console.WriteLine("usage: [workStatus: start/stop]");
            }
                
        }

        string GetFileName(string fullPath)
        {
            return fullPath.Remove(0, fullPath.LastIndexOf('/') + 1);
        }
    }
}