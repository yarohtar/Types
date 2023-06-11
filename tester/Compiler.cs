using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PTS;

namespace tester
{
    class ProofFile
    {
        Context context;
        string code;
        List<ProofFile> includes;
        string returned;
        Func<string, LambdaTerm> MakeLambda;

        public ProofFile(string code, Context context)
        {
            //treba jos da se radi na sintaksi...
            context.SyntaxRules.Add((code) => SyntaxRules.ForToAt(code));
            context.SyntaxRules.Add((code) => SyntaxRules.ProdToHash(code));
            context.SyntaxRules.Add((code) => SyntaxRules.RemoveOutsideBrackets(code));
            context.SyntaxRules.Add((code) => SyntaxRules.RemoveSpaceFollowing('#', code));
            context.SyntaxRules.Add((code) => SyntaxRules.RemoveSpaceFollowing('@', code));
            context.SyntaxRules.Add((code) => SyntaxRules.RemoveSpaceAround('=', code));
            context.SyntaxRules.Add((code) => SyntaxRules.AddApplicationBrackets(code));
            context.SyntaxRules.Add((code) => SyntaxRules.ArrowToHash(code));

            code = code.Trim();
            this.code = code;
            this.context = context;
            includes = new List<ProofFile>();
            MakeLambda = (code) => LambdaTermBuilder.MakeLambdaTerm(code, context);
        }

        public ProofFile(string code) : this(code, new Context())
        {
            
        }

        public void Compile()
        {
            while (code.Length > 0)
            {
                string line = code.Split(';')[0];
                int shift = line.Length + 1;

                line = line.Trim();


                if (line.Split(' ')[0] == "include")
                {
                    var s = new StreamReader(line.Split(' ')[1]);
                    var p = new ProofFile(s.ReadToEnd());
                    includes.Add(p);
                    s.Close();
                    p.Compile();
                    context.AddContext(p.context);
                }
                else if (line.Split(' ')[0] == "axiom")
                {
                    line = line.Substring(6).Trim();
                    string name = line.Split(':')[0];
                    line = line.Substring(name.Length + 1).Trim();
                    name = name.Trim();
                    context.AddAxiom(name, MakeLambda(line));
                }
                else if(line.Split(' ')[0] == "theorem")
                {
                    string thm = "";
                    int ind = 0;
                    int br = 0;
                    int first = 0;
                    while(!thm.Contains('{') || br>0)
                    {
                        if (code[ind] == '{')
                        {
                            if (br == 0)
                                first = ind;
                            br++;
                        }
                        if (code[ind] == '}')
                            br--;
                        
                        thm += code[ind];
                        ind++;
                        if (ind > code.Length)
                            throw new Exception("Bad brackets.");
                    }
                    shift = thm.Length + 1;
                    ProofFile f = new ProofFile(thm.Substring(first + 1, thm.Length - first - 2), new Context(context));
                    f.Compile();
                    thm = thm.Trim();
                    string statement = thm.Split('{')[0].Substring(7).Trim();
                    string name = statement.Split(':')[0];
                    statement = statement.Substring(name.Length + 1).Trim();
                    name = name.Trim();
                    if (f.returned is null)
                        Console.WriteLine("Not proved " + name);
                    else 
                    {
                        var returned = MakeLambda(f.returned);
                        if (context.GetType(returned, "local") == MakeLambda(statement))
                        {
                            context.SyntaxRules.Add((code) => code == name ? returned.GetCode : code);
                            Console.WriteLine("proved " + name);
                        }
                        else 
                            Console.WriteLine("Not proved " + name);
                    }
                }
                else if(line.Split(' ')[0] == "let")
                {
                    string thm = "";
                    int ind = 0;
                    int br = 0;
                    int first = 0;
                    while (!thm.Contains('{') || br > 0)
                    {
                        if (code[ind] == '{')
                        {
                            if (br == 0)
                                first = ind;
                            br++;
                        }
                        if (code[ind] == '}')
                            br--;

                        thm += code[ind];
                        ind++;
                        if (ind > code.Length)
                            throw new Exception("Bad brackets.");
                    }
                    shift = thm.Length + 1;
                    string statement = thm.Split('{')[0].Substring(4).Trim();
                    string name = statement.Split(':')[0];
                    statement = statement.Substring(name.Length + 1).Trim();
                    name = name.Trim();
                    var c = new Context(context);
                    c.AddLocal(name, MakeLambda(statement));
                    ProofFile f = new ProofFile(thm.Substring(first + 1, thm.Length - first - 2), c);
                    f.Compile();
                    if (f.returned is null)
                        continue;
                    returned = MakeLambda("@" + name + ":" + "(" + statement + ")" + "." + "(" + f.returned + ")").GetCode;
                    break;
                }
                else if (line.StartsWith("infix "))
                {
                    var s1 = line.Split(' ');
                    if (s1.Length == 3)
                        context.SyntaxRules.Add((code) => SyntaxRules.InfixNotation(s1[1], s1[2], code));
                }
                else if (line.StartsWith("rule "))
                {
                    line = line.Substring(5);
                    string name = line.Split('=')[0];
                    line = line.Substring(name.Length + 1).Trim();
                    name = name.Trim();
                    string s1 = line;
                    context.SyntaxRules.Add((code) => code == name ? s1 : code);
                }
                else
                {
                        if(line.StartsWith("output"))
                        {
                            int b = 0;
                        }
                        var a = MakeLambda(line);
                        if (a.GetCode != "_arg_void")
                        {
                            context.GetType(a);
                            returned = a.GetCode;
                            break;
                        }
                }
                try
                {
                    code = code.Substring(shift);
                }
                catch
                {
                    code = "";
                }
            }

        }
    }
}
