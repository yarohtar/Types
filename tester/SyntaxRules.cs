using System;
using System.Collections.Generic;
using System.Text;
using PTS;

namespace tester
{
    static class SyntaxRules
    {
        public static List<Func<string, string>> Rules = new List<Func<string, string>>();
        static SortedDictionary<int, List<Func<string, string>>> Rules1 = new SortedDictionary<int, List<Func<string, string>>>();

        public static void AddRuleAtLevel(Func<string, string> rule, int level)
        {
            Rules1[level].Add(rule);
        }

        public static string RemoveOutsideBrackets(string code)
        {
            if (code[0] == '(' && code[code.Length - 1] == ')')
            {
                int br = 0;
                for (int i = 0; i < code.Length - 1; i++)
                {
                    if (code[i] == '(')
                        br++;
                    if (code[i] == ')')
                        br--;
                    /*if (code[i] == ' ' && br == 1)
                        return code;*/
                    if (br == 0)
                        return code;
                }
                return RemoveOutsideBrackets(code.Substring(1, code.Length - 2));
            }
            return code;
        }

        public static string AddApplicationBrackets(string code)
        {
            int i = code.Length - 1;
            int br = 0;
            while (i >= 0)
            {
                if (code[i] == ')')
                    br++;
                if (code[i] == '(')
                    br--;
                if (br == 0 && code[i] == ' ')
                    return "(" + code + ")";
                i--;
            }
            return code;
        }

        public static string ForToAt(string code)
        {
            return code.Replace("for ", "@");
        }
        public static string ProdToHash(string code)
        {
            return code.Replace("prod ", "#");
        }
        public static string RemoveSpaceLeading(char a, string code)
        {
            var c = code.Split(a);
            for (int i = 0; i < c.Length - 1; i++)
            {
                c[i] = c[i].TrimEnd(' ');
            }
            string c1 = c[0];
            for (int i = 1; i < c.Length; i++)
            {
                c1 += a + c[i];
            }
            return c1;
        }
        public static string RemoveSpaceFollowing(char a, string code)
        {
            var c = code.Split(a);
            for (int i = 1; i < c.Length; i++)
            {
                c[i] = c[i].TrimStart(' ');
            }
            string c1 = c[0];
            for (int i = 1; i < c.Length; i++)
            {
                c1 += a + c[i];
            }
            return c1;
        }
        public static string RemoveSpaceAround(char a, string code)
        {
            var c = code.Split(a);
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = c[i].Trim();
            }
            string c1 = c[0];
            for (int i = 1; i < c.Length; i++)
            {
                c1 += a + c[i];
            }
            return c1;
        }

        public static string ArrowToHash(string code)
        {
            int br = 0;
            if (code[0] == '#' || code[0] == '@')
                return code;
            for (int i = 0; i < code.Length - 2; i++)
            {
                if (code[i] == '(')
                    br++;
                if (code[i] == ')')
                    br--;
                if (br == 0 && code.Substring(i, 2) == "->")
                {
                    return "#_:" + code.Substring(0, i) + "." + code.Substring(i + 2);
                }
            }
            return code;
        }

        public static string InfixNotation(string function, string operation, string code)
        {
            int br = 0;
            if (code[0] == '#' || code[0] == '@')
                return code;
            for (int i = 0; i < code.Length - operation.Length; i++)
            {
                if (code[i] == '(')
                    br++;
                if (code[i] == ')')
                    br--;
                if (br == 0 && code.Substring(i, operation.Length) == operation)
                {
                    return "((" + function + " " + code.Substring(0, i) + ") " + code.Substring(i + operation.Length) + ")";
                }
            }
            return code;
        }
        public static string Convert(string code)
        {
            code = code.Trim();
            foreach (var kv in Rules1)
            {
                foreach (var f in kv.Value)
                {
                    code = f(code);
                }
            }
            if (code[0] == '(')
            {
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
                var function = Convert(code.Substring(1, i - 1));
                var input = Convert(code.Substring(i + 1, code.Length - 2 - i));

                return "(" + function + " " + input + ")";
            }
            else if (code[0] == '@' || code[0] == '#')
            {
                var x = code.Split(':')[0].Substring(1);
                if (!LambdaTerm.IsValidVarName(x))
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
                var type = Convert(code.Substring(2 + x.Length, i - x.Length - 2));
                var body = Convert(code.Substring(i + 1));
                return code[0] + x + ":" + type + "." + body;
            }
            else
            {
                if (code == "_")
                    throw new Exception("Cannot use _ except when binding variables.");
                if (!LambdaTerm.IsValidVarName(code))
                    throw new Exception(code + " is not a valid variable name.");
                return code;
            }
        }
        public static string PowerSet(string code)
        {
            if (LambdaTerm.IsValidVarName(code))
                if (code[0] == 'p')
                    return "#_:" + code.Substring(1) + ".Prop";

            return code;
        }
    }
}
