using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Permissions;

namespace FileExplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            FileExplorer fileExplorer = FileExplorer.GetInstance();
            fileExplorer.Run();

        }
    }
}