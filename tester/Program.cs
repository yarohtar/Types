using System;
using PTS;
using System.Collections.Generic;
using System.IO;

namespace tester
{
    class Program
    {
        static Context context;
        static string PATH = @"C:\Users\igore\source\repos\Types\tester\";

        static void Main(string[] args)
        {
            context = new Context();

            //ovde se nalazi definicija PTSa preko skupa sorti, funkcije aksioma i funkcije pravila
            PTSDefinition.DefinePTS(new string[] { "Prop", "Type"}, (string a) => a == "Prop" ? "Type" : a == "Type" ? "Type" : null,
                (string a, string b) =>
                {
                    if (b == "Prop" || b == "Type")
                    {
                        if (a == "Prop" || a == "Type")
                            return b;
                    }
                    return null;
                });

            string code="";
            //u liniji ispod potrebno je navesti fajl u kome se nalazi dokaz koji treba proveriti
            Console.WriteLine("Enter path to folder with proof files (use \\):");
            PATH=Console.ReadLine();
            if(!PATH.EndsWith('\\'))
                PATH+="\\";
            Console.WriteLine("Enter name of proof file:");
            string name = Console.ReadLine();
            Console.WriteLine("Compiling " + PATH + name);
            ProofFile.PATH = PATH;
            var sr = new StreamReader(PATH+name);
            code = sr.ReadToEnd();
            sr.Close();

            ProofFile f = new ProofFile(code, context);
            f.Compile();
        }
    }
}