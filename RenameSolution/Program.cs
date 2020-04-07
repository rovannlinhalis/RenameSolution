using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RenameSolution
{
    class Program
    {
        static List<string> extensions = new List<string>();
        

        static void Main(string[] args)
        {
            extensions.Add(".cs");
            extensions.Add(".sln");
            extensions.Add(".csproj");
            extensions.Add(".config");
            extensions.Add(".xml");
            extensions.Add(".resx");




            Console.WriteLine("Digite o diretório da solução:");
            string sdir = Console.ReadLine();
            DirectoryInfo dir = new DirectoryInfo(sdir);
            if (dir.Exists)
            {
                Console.WriteLine("Digite o texto a ser subistituido (case sensitive):");
                string old = Console.ReadLine();

                Console.WriteLine("Digite o texto a ser preenchido (case sensitive):");
                string xnew = Console.ReadLine();

                DirectoryInfo[] dirs = dir.GetDirectories("*", SearchOption.AllDirectories);

                Console.WriteLine("Alterar " + dirs.Length+" pastas. Deseja continuar ? (Y/N)");

                if (Console.ReadLine() == "Y")
                {
                    ProcessaDir(dir, old, xnew);

                    Parallel.ForEach(dirs, new ParallelOptions { }, d =>
                    //foreach (DirectoryInfo d in dirs)
                    {
                        ProcessaDir(d, old, xnew);
                    });
                }

                Console.ReadKey();
            }
            else
                Console.WriteLine("Diretório não existe");
        }

        static void ProcessaDir(DirectoryInfo d, string old, string xnew)
        {
            Console.WriteLine(d.FullName);
            if (d.Exists)
            {
                try
                {
                    FileInfo[] files = d.GetFiles("*", SearchOption.TopDirectoryOnly);
                    Parallel.ForEach(files, new ParallelOptions { }, f =>
                    //foreach (FileInfo f in files)
                    {
                        Console.WriteLine(f.FullName);
                        try
                        {
                            if (!isBinary(f) || extensions.Contains(f.Extension.ToLower()))
                            {
                                string text = File.ReadAllText(f.FullName, Encoding.Default);
                                text = text.Replace(old, xnew);
                                File.WriteAllText(f.FullName, text, Encoding.Default);
                            }
                        }
                        catch { }

                        string newName = f.FullName.Replace(old, xnew);
                        if (newName != f.FullName)
                        {
                            FileInfo nf = new FileInfo(newName);
                            if (!nf.Directory.Exists)
                                nf.Directory.Create();

                            createParentDir(nf.Directory);
                            f.MoveTo(newName, true);
                        }
                    });

                    string newDirName = d.FullName.Replace(old, xnew);
                    if (newDirName != d.FullName)
                    {
                        DirectoryInfo nd = new DirectoryInfo(newDirName);

                        createParentDir(nd);
                        if (!nd.Exists)
                            d.MoveTo(newDirName);
                    }
                }
                catch { }
            }
        }

        static void createParentDir(DirectoryInfo d)
        {
            if (d != null)
            if (d.Root != d.Parent)
            {
                    if (d.Parent == null)
                    {
                        if (!d.Exists)
                            d.Create();
                    }
                    else
                        createParentDir(d.Parent);
            }
            else
            {
                if (!d.Exists)
                    d.Create();
            }
        }

        public static bool isBinary(FileInfo path)
        {
            long length = path.Length;
            if (length == 0) return false;

            using (StreamReader stream = new StreamReader(path.FullName))
            {
                int ch;
                while ((ch = stream.Read()) != -1)
                {
                    if (isControlChar(ch))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool isControlChar(int ch)
        {
            return (ch > Chars.NUL && ch < Chars.BS)
                || (ch > Chars.CR && ch < Chars.SUB);
        }

        public static class Chars
        {
            public static char NUL = (char)0; // Null char
            public static char BS = (char)8; // Back Space
            public static char CR = (char)13; // Carriage Return
            public static char SUB = (char)26; // Substitute
        }


    }
}
