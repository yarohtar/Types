using System;
using System.Collections.Generic;
using System.Text;
using PTS;

namespace tester
{
    class BuilderFunction
    {
        static string allowedVarChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890_";
        string name;
        string code;
        Func<List<string>, string> function;
        static Context context;

        public static List<BuilderFunction> Functions = new List<BuilderFunction> { Output, Reduced, TypeOf, Remove, Add, Function, Input, ArgType, Body };

        public BuilderFunction(string code)
        {
            List<string> args = new List<string>();
            string declaration = code.Split('=')[0];
            code = code.Substring(declaration.Length + 1).Trim();
            if (code.Contains("_arg_"))
                throw new Exception();
            declaration = declaration.Trim();
            var l = declaration.Split(' ');
            name = l[0];

            for (int i = 1; i < l.Length; i++)
            {
                args.Add(l[i]);
            }

            string newcode = "";
            string var = "";
            for(int i=0; i<code.Length; i++)
            {
                if(!allowedVarChars.Contains(code[i]))
                {
                    bool isinputvar = false;
                    for(int j=0; j<args.Count; j++)
                    {
                        if (args[j] == var)
                        {
                            newcode += "_arg_" + j;
                            isinputvar = true;
                            break;
                        }
                    }
                    if (!isinputvar)
                        newcode += var;
                    var = "";
                    newcode += code[i];
                }
                else
                {
                    var += code[i];
                }
            }
            bool isinputvar1 = false;
            for (int j = 0; j < args.Count; j++)
            {
                if (args[j] == var)
                {
                    newcode += "_arg_" + j;
                    isinputvar1 = true;
                    break;
                }
            }
            if (!isinputvar1)
                newcode += var;
            this.code = newcode;
            function = DerivedFunction;
        }

        public static string RemoveFunction(string name)
        {
            for (int i = 0; i < Functions.Count; i++)
            {
                if (Functions[i].name == name)
                {
                    Functions.RemoveAt(i);
                    return "_arg_removed";
                }
            }
            return "_arg_error";
        }

        private BuilderFunction(string name, Func<List<string>, string> func)
        {
            this.name = name;
            function = func;
        }

        public static BuilderFunction Add
        {
            get => new BuilderFunction("func", (a)=>"");
        }

        public static BuilderFunction TypeOf
        {
            get => new BuilderFunction("typeof", GetType);
        }

        public static BuilderFunction Function
        {
            get => new BuilderFunction("function", GetFunction);
        }
        public static BuilderFunction Input
        { get => new BuilderFunction("input", GetInput); }
        public static BuilderFunction ArgType { get => new BuilderFunction("argtype", GetTypeOfVar); }
        public static BuilderFunction Body { get => new BuilderFunction("body", GetBody); }

        public static BuilderFunction Reduced
        {
            get => new BuilderFunction("reduced", BetaReduced);
        }

        public static BuilderFunction Output
        {
            get => new BuilderFunction("output", Print);
        }

        public static BuilderFunction Remove
        {
            get => new BuilderFunction("remove_function", (list) => list.Count == 1 ? RemoveFunction(list[0]) : "_arg_error");
        }

        public string ApplyFunction(string code, Context context)
        {
            BuilderFunction.context = context;
            if (code.Split(' ')[0] != name)
                return "_arg_error";
            if (name == "func")
                return AddFunction(code.Substring(4).Trim());
            if (code == name)
                return function(new List<string>());
            return function(GetInputs(code.Substring(name.Length).Trim()));
        }

        private static string AddFunction(string code)
        {
            Functions.Add(new BuilderFunction(code));
            return "_arg_added";
        }

        private static string Print(List<string> inputs)
        {
            if (inputs.Count != 1)
                return "_arg_error";
            var l = LambdaTermBuilder.MakeLambdaTerm(inputs[0], context);
            Console.WriteLine(l.GetCode);
            return "_arg_output";
        }

        private static string GetType(List<string> inputs)
        {
            if (inputs.Count != 1)
                return "_arg_error";
            return context.GetType(LambdaTermBuilder.MakeLambdaTerm(inputs[0], context), "local").GetCode;
        }



        private static string GetVar(List<string> inputs)
        {
            if (inputs.Count != 1)
                return "arg_error";
            return LambdaTermBuilder.MakeLambdaTerm(inputs[0], context).GetArgument;
        }

        private static string GetTypeOfVar(List<string> inputs)
        {
            if (inputs.Count != 1)
                return "arg_error";
            return LambdaTermBuilder.MakeLambdaTerm(inputs[0], context).GetArgumentType.GetCode;
        }

        private static string GetBody(List<string> inputs)
        {
            if (inputs.Count != 1)
                return "arg_error";
            return LambdaTermBuilder.MakeLambdaTerm(inputs[0], context).GetBody.GetCode;
        }

        private static string GetFunction(List<string> inputs)
        {
            if (inputs.Count != 1)
                return "arg_error";
            return LambdaTermBuilder.MakeLambdaTerm(inputs[0], context).GetFunction.GetCode;
        }

        private static string GetInput(List<string> inputs)
        {
            if (inputs.Count != 1)
                return "arg_error";
            return LambdaTermBuilder.MakeLambdaTerm(inputs[0], context).GetInput.GetCode;
        }

        private static string BetaReduced(List<string> inputs)
        {
            if (inputs.Count != 1)
                return "_arg_error";
            return LambdaTermBuilder.MakeLambdaTerm(inputs[0], context).BetaNormalForm().GetCode;
        }

        private static string MakeApplication(List<string> inputs)
        {
            if (inputs.Count != 2)
                return "_arg_error";
            var f = LambdaTermBuilder.MakeLambdaTerm(inputs[0], context);
            var i = LambdaTermBuilder.MakeLambdaTerm(inputs[1], context);
            return LambdaTermBuilder.MakeLambdaTerm("((" + f.GetCode + ") (" + i.GetCode + "))", context).GetCode;
        }

        private static string MakeAbstraction(List<string> inputs)
        {
            if (inputs.Count != 3)
                return "_arg_error";
            var type = LambdaTermBuilder.MakeLambdaTerm(inputs[1], context);
            var body = LambdaTermBuilder.MakeLambdaTerm(inputs[2], context);
            return LambdaTermBuilder.MakeLambdaTerm("@" + inputs[0] + ":(" + type.GetCode + ").(" + body.GetCode + ")", context).GetCode;
        }

        private string MakeProduct(List<string> inputs)
        {
            if (inputs.Count != 3)
                return "_arg_error";
            var type = LambdaTermBuilder.MakeLambdaTerm(inputs[1], context);
            var body = LambdaTermBuilder.MakeLambdaTerm(inputs[2], context);
            return LambdaTermBuilder.MakeLambdaTerm("#" + inputs[0] + ":(" + type.GetCode + ").(" + body.GetCode + ")", context).GetCode;
        }

        private string DerivedFunction(List<string> inputs)
        {
            string a = code;
            for(int i=0; i<inputs.Count; i++)
            {
                a = a.Replace("_arg_" + i, "(" + LambdaTermBuilder.MakeLambdaTerm(inputs[i], context).GetCode + ")");
            }
            if (a.Contains("_arg_"))
                return "_arg_error";
            return a;
        }

        private static List<string> GetInputs(string code)
        {
            List<string> inputs = new List<string>();
            while(true)
            {
                code = code.Trim();
                if (code.Length == 0)
                    break;
                string input = GetInput(code);
                inputs.Add(input);
                if (input.Length == code.Length)
                    break;
                code = code.Substring(input.Length);
            }
            return inputs;
        }
        private static string GetInput(string code)
        {
            string open = "{[(";
            string closed = "}])";
            code = code.Replace("\n", " ");
            int br = 0;
            for(int i=0; i<code.Length; i++)
            {
                if (open.Contains(code[i]))
                    br++;
                else if (closed.Contains(code[i]))
                    br--;
                else if (code[i] == ' ' && br==0)
                    return code.Substring(0, i);
            }
            if (br > 0)
                return "_arg_error";
            return code;
        }
    }
}
