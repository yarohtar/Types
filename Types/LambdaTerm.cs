using System;
using System.Collections.Generic;
using System.Text;

namespace PTS
{
    /// <summary>
    /// Term := sort | var | @var:Term.Term | #var:Term.Term | (Term Term)
    /// </summary>
    public class LambdaTerm : IEquatable<LambdaTerm>
    {
        static string allowedVarChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890_";

        //public Context context;

        string operation;
        string code;

        LambdaTerm function;
        LambdaTerm input;
        LambdaTerm type;
        LambdaTerm body;
        string x;
        string var;

        HashSet<string> FreeVariables;
        HashSet<string> BoundVariables;

        public LambdaTerm(string code)
        {
            code = code.Trim();
            FreeVariables = new HashSet<string>();
            BoundVariables = new HashSet<string>();
            if (code[0] == '(')
            {
                operation = "app";
                int i = code.Length-1;
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
                function = new LambdaTerm(code.Substring(1, i - 1));
                input = new LambdaTerm(code.Substring(i + 1, code.Length - 2 - i));

                this.code = "(" + function.code + " " + input.code + ")";

                BoundVariables.UnionWith(function.BoundVariables);
                BoundVariables.UnionWith(input.BoundVariables);

                FreeVariables.UnionWith(function.FreeVariables);
                FreeVariables.UnionWith(input.FreeVariables);
            }
            else if (code[0] == '@' || code [0]=='#')
            {
                operation = code[0]=='@' ? "abs" : "prod";
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
                type = new LambdaTerm(code.Substring(2 + x.Length, i - x.Length - 2));
                body = new LambdaTerm(code.Substring(i + 1));

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

        public LambdaTerm(LambdaTerm t) : this(t.code)
        {

        }

        public static LambdaTerm NewAbstraction(char c, string arg, LambdaTerm type, LambdaTerm body)
        {
            if(body.GetVars.Contains(arg))
                return new LambdaTerm(c + arg + ":" + type.code + "." + body.RenameForbidden(new HashSet<string> { arg }).code);
            return new LambdaTerm(c + arg + ":" + type.code + "." + body.code);
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
        public LambdaTerm GetArgumentType
        {
            get => IsProduct || IsAbstraction ? type : null;
        }
        public LambdaTerm GetBody
        {
            get => IsProduct || IsAbstraction ? body : null;
        }
        public LambdaTerm GetFunction
        {
            get => IsApplication ? function : null;
        }
        public LambdaTerm GetInput
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

        public LambdaTerm BetaNormalForm()
        {
            if (IsApplication)
            {
                var f = function.BetaNormalForm();
                var i = input.BetaNormalForm();
                if (f.IsAbstraction)
                {
                    return f.body.Replace(f.GetArgument, i).BetaNormalForm();
                }
                return new LambdaTerm("(" + f.GetCode + " " + i.GetCode + ")");
            }
            else if(IsAbstraction || IsProduct)
            {
                var t = type.BetaNormalForm();
                var b = body.BetaNormalForm();
                string argument = GetArgument;
                return NewAbstraction(code[0], argument, t, b);
                if (b.BoundVariables.Contains(argument))
                    b = b.RenameForbidden(new HashSet<string>() { argument });
                return new LambdaTerm(code[0] + argument + ":" + t.code + "." + b.code);
            }
            return new LambdaTerm(this);
        }

        private static bool eq(LambdaTerm a, LambdaTerm b)
        {
            if (a.code == b.code)
                return true;
            if (a.operation != b.operation)
                return false;
            if (a.IsApplication)
            {
                return a.GetFunction == b.GetFunction && a.GetInput == b.GetInput;
            }
            if (a.IsVariable)
                return false;
            if (a.type != b.type)
                return false;
            HashSet<string> vars = new HashSet<string>();
            vars.UnionWith(a.GetVars);
            vars.UnionWith(b.GetVars);
            string x = NewVarName(vars);
            return a.AlphaConversion(x).body ==
                b.AlphaConversion(x).body;
        }
        /// <summary>
        /// Alpha-beta equality.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(LambdaTerm other)
        {
            return eq(BetaNormalForm(), other.BetaNormalForm());
        }

        private static string NewVarName(HashSet<string> a)
        {
            int i = 0;
            while (a.Contains("x" + i))
                i++;
            return "x" + i;
        }

        public static bool operator ==(LambdaTerm left, LambdaTerm right)
        {
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }
        public static bool operator !=(LambdaTerm left, LambdaTerm right)
        {
            return !(left == right);
        }
        internal LambdaTerm Replace(string x, LambdaTerm A)
        {
            if (BoundVariables.Contains(x))
                throw new Exception("INTERNAL: Cannot replace a bound variable " + x + " in " + code);
            A = A.RenameForbidden(GetVars);
            var l = RenameForbidden(A.GetVars);
            return l.ReplaceSafe(x, A);
        }
        private LambdaTerm ReplaceSafe(string x, LambdaTerm A)
        {
            if (!FreeVariables.Contains(x))
                return new LambdaTerm(this);
            if (IsVariable)
            {
                if (code == x)
                    return new LambdaTerm(A);
                return new LambdaTerm(this);
            }
            if (IsApplication)
            {
                return new LambdaTerm("(" + function.Replace(x, A).code + " " + input.Replace(x, A).code + ")");
            }
            return NewAbstraction(code[0], GetArgument, type.Replace(x, A), body.Replace(x, A));
        }

        public LambdaTerm AlphaConversion(string x)
        {
            if (IsVariable || IsApplication)
                throw new Exception(code + " is not a product nor an abstraction. Alpha conversion is not possible.");
            if (body.GetVars.Contains(x))
                throw new Exception(x + " is already in " + body.code + ". Alpha conversion is not possible.");
            return NewAbstraction(code[0], x, type, body.RenameFreeVar(GetArgument, x));
        }
        private LambdaTerm RenameFreeVar(string old, string x)
        {
            if(IsVariable)
            {
                if (code == old)
                    return new LambdaTerm(x);
                return new LambdaTerm(code);
            }
            if(IsApplication)
            {
                return new LambdaTerm("(" + function.RenameFreeVar(old, x).code + " " + input.RenameFreeVar(old, x).code + ")");
            }
            return NewAbstraction(code[0], GetArgument, type.RenameFreeVar(old,x), body.RenameFreeVar(old,x));
        }

        private LambdaTerm RenameForbidden(HashSet<string> forbidden)
        {
            var s = new HashSet<string>(BoundVariables);
            s.IntersectWith(forbidden);
            if (s.Count == 0)
                return new LambdaTerm(this);
            forbidden.UnionWith(FreeVariables);
            if (IsVariable)
                return new LambdaTerm(this);
            if(IsApplication)
                return new LambdaTerm("(" + function.RenameForbidden(forbidden).code + 
                    " " + input.RenameForbidden(forbidden).code + ")");
            var l = new LambdaTerm(this);
            if (forbidden.Contains(l.GetArgument))
            {
                var f = new HashSet<string>(forbidden);
                f.UnionWith(l.GetVars);
                l = l.AlphaConversion(NewVarName(f));
            }
            forbidden.Add(l.GetArgument);
            return NewAbstraction(code[0], l.GetArgument, l.type.RenameForbidden(forbidden), l.body.RenameForbidden(forbidden));
        }
    }
}
