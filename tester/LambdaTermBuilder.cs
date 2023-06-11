using System;
using System.Collections.Generic;
using System.Text;
using PTS;

namespace tester
{
    class LambdaTermBuilder
    {
        static string allowedVarChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890_";

        public static Context context;

        string operation;
        string code;

        LambdaTermBuilder function;
        LambdaTermBuilder input;
        LambdaTermBuilder type;
        LambdaTermBuilder body;
        string x;
        string var;

        HashSet<string> FreeVariables;
        HashSet<string> BoundVariables;


        private void Copy(LambdaTermBuilder a)
        {
            operation = a.operation;
            code = a.code;
            function = a.function;
            input = a.input;
            type = a.type;
            body = a.body;
            x = a.x;
            var = a.var;
            FreeVariables = new HashSet<string>(a.FreeVariables);
            BoundVariables = new HashSet<string>(a.BoundVariables);
        }

        public LambdaTermBuilder(string code)
        {
            code = code.Trim();
            if (code.Contains("_arg_"))
                throw new Exception("Code cannot contain _arg_");
            code = SyntaxRules.RemoveOutsideBrackets(code);
            foreach(var f in BuilderFunction.Functions)
            {
                string c = f.ApplyFunction(code, context);
                if (c.Contains("_arg_"))
                {
                    if(c=="_arg_error")
                        continue;
                    else
                    {
                        this.code = "_arg_void";
                        return;
                    }
                }
                else
                {
                    Copy(new LambdaTermBuilder(c));
                    return;
                }
            }
            foreach (var f in context.SyntaxRules)
            {
                code = f(code);
            }
            FreeVariables = new HashSet<string>();
            BoundVariables = new HashSet<string>();
            if (code[0] == '(')
            {
                operation = "app";
                int i = code.Length - 1;
                if (code[i] != ')')
                    throw new Exception(code + " started with ( and did not end with ).");
                int br = 0;
                while (true)
                {
                    if (code[i] == ')')
                        br++;
                    if (code[i] == '(')
                        br--;
                    if (br == 1 && code[i] == ' ')
                        break;
                    i--;
                    if (i < 0)
                        throw new Exception(code + " was not in the form (A B) where A and B are valid terms.");
                }
                function = new LambdaTermBuilder(code.Substring(1, i - 1));
                input = new LambdaTermBuilder(code.Substring(i + 1, code.Length - 2 - i));

                this.code = "(" + function.code + " " + input.code + ")";

                BoundVariables.UnionWith(function.BoundVariables);
                BoundVariables.UnionWith(input.BoundVariables);

                FreeVariables.UnionWith(function.FreeVariables);
                FreeVariables.UnionWith(input.FreeVariables);
            }
            else if (code[0] == '@' || code[0] == '#')
            {
                operation = code[0] == '@' ? "abs" : "prod";
                x = code.Split(':')[0].Substring(1);
                if (!IsValidVarName(x))
                    throw new Exception(x + " is not a valid variable name.");
                if (PTSDefinition.IsSort(x))
                    throw new Exception("Cannot bind a name dedicated to a sort.");
                int i = 0;
                int br = 0;
                while (true)
                {
                    if (code[i] == '#' || code[i] == '@')
                        br++;
                    if (code[i] == '.')
                        br--;
                    if (br == 0)
                        break;
                    i++;
                    if (i == code.Length)
                        throw new Exception(code + " was not in the form " + code[0] + "x:A.B where A is a valid term.");
                }
                type = new LambdaTermBuilder(code.Substring(2 + x.Length, i - x.Length - 2));
                body = new LambdaTermBuilder(code.Substring(i + 1));
                this.code = code[0] + x + ":" + type.code + "." + body.code;
                if (body.BoundVariables.Contains(x))
                {
                    body = body.RenameForbidden(new HashSet<string> { x });
                    this.code = code[0] + x + ":" + type.code + "." + body.code;
                }
                var forbidden = context.GetVars;
                if (forbidden.Contains(x))
                {
                    forbidden.UnionWith(body.GetVars);
                    Copy(AlphaConversion(NewVarName(forbidden)));
                }

                BoundVariables.UnionWith(body.BoundVariables);
                BoundVariables.UnionWith(type.BoundVariables);
                if (x != "_")
                    BoundVariables.Add(x);

                FreeVariables.UnionWith(body.FreeVariables);
                FreeVariables.UnionWith(type.FreeVariables);
                FreeVariables.Remove(x);

                if (body.BoundVariables.Contains(x))
                {
                    throw new Exception("INTERNAL: Failed to bind " + x + " in " + body.code);
                }
                if (context.GetVars.Contains(x))
                    throw new Exception("INTERNAL: Cannot bind a variable from context.");

                this.code = code[0] + x + ":" + type.code + "." + body.code;
            }
            else
            {
                operation = "var";
                if (code == "_")
                    throw new Exception("Cannot use _ except when binding variables.");
                if (!IsValidVarName(code))
                    throw new Exception(code + " is not a valid variable name.");
                var = code;
                this.code = code;
                if (!PTSDefinition.IsSort(code))
                    FreeVariables.Add(code);
            }
        }

        public static bool IsValidVarName(string x)
        {
            foreach (char c in x)
                if (!allowedVarChars.Contains(c.ToString()))
                    return false;
            return true;
        }

        public LambdaTermBuilder(LambdaTermBuilder t) : this(t.code)
        {

        }

        #region Properties
        public bool IsApplication
        {
            get => operation == "app";
        }
        public bool IsAbstraction
        {
            get => operation == "abs";
        }
        public bool IsProduct
        {
            get => operation == "prod";
        }
        public bool IsVariable
        {
            get => operation == "var";
        }

        public string GetArgument
        {
            get => IsProduct || IsAbstraction ? x : null;
        }
        public LambdaTermBuilder GetArgumentType
        {
            get => IsProduct || IsAbstraction ? type : null;
        }
        public LambdaTermBuilder GetBody
        {
            get => IsProduct || IsAbstraction ? body : null;
        }
        public LambdaTermBuilder GetFunction
        {
            get => IsApplication ? function : null;
        }
        public LambdaTermBuilder GetInput
        {
            get => IsApplication ? input : null;
        }
        public string GetVariable
        {
            get => IsVariable ? var : null;
        }

        public string GetCode
        {
            get => code;
        }
        public HashSet<string> GetVars
        {
            get
            {
                var k = new HashSet<string>(FreeVariables);
                k.UnionWith(BoundVariables);
                return k;
            }
        }
        #endregion

        public LambdaTermBuilder BetaReduction()
        {
            if (IsApplication)
            {
                if (function.IsAbstraction)
                {
                    return function.body.Replace(function.GetArgument, input.RenameForbidden(function.BoundVariables));
                }
            }
            return null;
        }
        public LambdaTermBuilder BetaNormalForm()
        {
            if (IsApplication)
            {
                var f = function.BetaNormalForm();
                var i = input.BetaNormalForm();
                if (f.IsAbstraction)
                {
                    return f.body.Replace(f.GetArgument, i.RenameForbidden(f.BoundVariables));
                }
                return new LambdaTermBuilder("(" + f.GetCode + " " + i.GetCode + ")");
            }
            else if (IsAbstraction || IsProduct)
            {
                var t = type.BetaNormalForm();
                var b = body.BetaNormalForm();
                return new LambdaTermBuilder(code[0] + GetArgument + ":(" + t.code + ")." + b.code);
            }
            return new LambdaTermBuilder(this);
        }

        private static string NewVarName(HashSet<string> a)
        {
            int i = 0;
            while (a.Contains("x" + i))
                i++;
            return "x" + i;
        }

        public LambdaTermBuilder Replace(string x, LambdaTermBuilder A)
        {
            if (BoundVariables.Contains(x))
                throw new Exception("Cannot replace a bound variable " + x + " in " + code);
            if (!FreeVariables.Contains(x))
                return new LambdaTermBuilder(this);
            if (x == "_")
                return new LambdaTermBuilder(this);
            if (IsVariable)
            {
                if (code == x)
                    return new LambdaTermBuilder(A);
                return new LambdaTermBuilder(this);
            }
            if (IsApplication)
            {
                return new LambdaTermBuilder("(" + function.Replace(x, A).code + " " + input.Replace(x, A).code + ")");
            }
            return new LambdaTermBuilder(code[0] + GetArgument + ":(" + type.Replace(x, A).code + ")." + body.Replace(x, A).code);
        }

        public LambdaTermBuilder AlphaConversion(string x)
        {
            if (IsVariable || IsApplication)
                throw new Exception(code + " is not a product nor an abstraction. Alpha conversion is not possible.");
            if (body.GetVars.Contains(x))
                throw new Exception(x + " is already in " + body.code + ". Alpha conversion is not possible.");
            var l = new LambdaTermBuilder(code[0] + x + ":(" + type.code + ")." + body.Rename(GetArgument, x).code);
            if (l.GetArgument != x)
                throw new Exception(body.Rename(GetArgument, x).code);
            return l;
        }

        private LambdaTermBuilder Rename(string old, string x)
        {
            if (IsVariable)
            {
                if (code == old)
                    return new LambdaTermBuilder(x);
                return new LambdaTermBuilder(code);
            }
            if (IsApplication)
            {
                return new LambdaTermBuilder("(" + function.Rename(old, x).code + " " + input.Rename(old, x).code + ")");
            }
            return new LambdaTermBuilder(code[0] + GetArgument + ":(" + type.Rename(old, x).code + ")." + body.Rename(old, x).code);
        }

        private LambdaTermBuilder RenameForbidden(HashSet<string> forbidden)
        {
            forbidden.UnionWith(FreeVariables);
            if (IsVariable)
                return new LambdaTermBuilder(this);
            if (IsApplication)
                return new LambdaTermBuilder("(" + function.RenameForbidden(forbidden).code +
                    " " + input.RenameForbidden(forbidden).code + ")");
            var f = new HashSet<string>(forbidden);
            f.Add(GetArgument);
            var l = new LambdaTermBuilder(code[0] + GetArgument + ":(" + type.RenameForbidden(f).code +
                ")." + body.RenameForbidden(f).code);
            if (forbidden.Contains(l.GetArgument))
            {
                f = new HashSet<string>(forbidden);
                f.UnionWith(l.GetVars);
                f.UnionWith(context.GetVars);
                string x = NewVarName(f);
                l = l.AlphaConversion(x);
            }
            return l;
        }

        public static LambdaTerm MakeLambdaTerm(string code, Context context)
        {
            LambdaTermBuilder.context = context;
            var l = new LambdaTermBuilder(code);
            return new LambdaTerm(l.code/*,context*/);
        }
    }
}
