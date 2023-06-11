using System;
using PTS;
using System.Collections.Generic;
using System.IO;

namespace tester
{
    class Program
    {
        static Context context;

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
            string PATH = @"C:\Users\igore\source\repos\Types\tester\contradiction.txt";
            //Console.WriteLine("Enter full path to proof file (or just the name, provided it's in the same folder as .exe):");
            //PATH=Console.ReadLine();
            var sr = new StreamReader(PATH);
            code = sr.ReadToEnd();

            ProofFile f = new ProofFile(code, context);
            f.Compile();
        }
    }
}