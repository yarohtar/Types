using System;
using System.Collections.Generic;
using System.Text;

namespace PTS
{
    public partial class Context
    {
        Dictionary<string, LambdaTerm> TypingContext = new Dictionary<string, LambdaTerm>();
        public LambdaTerm GetType(LambdaTerm A, string context = "local")
        {
            TypingContext.Clear();
            if (context == "local")
                return GetType(A, (string a) => TypingContext.ContainsKey(a) 
                ? TypingContext[a] : GetLocalTypeOfVar(a), context);
            if (context == "global")
                return GetType(A, (string a) => TypingContext.ContainsKey(a)
                ? TypingContext[a] : GetGlobadTypeOfVar(a), context);
            throw new Exception(context + " is not a valid context.");
        }
        private LambdaTerm GetType(LambdaTerm A, Func<string, LambdaTerm> GetTypeOfVar, string context)
        {
            if (A is null)
                return null;
            if (A.IsVariable)
            {
                if (PTSDefinition.IsSort(A.GetCode))
                    return new LambdaTerm(PTSDefinition.Axiom(A.GetCode));
                var t = GetTypeOfVar(A.GetCode);
                if(t is null)
                {
                    throw new Exception("Variable " + A.GetCode + " was not introduced.");
                }
                return t;
            }
            else if (A.IsProduct)
            {
                var A1 = A.GetArgumentType;
                var A2 = A.GetBody;
                var t1 = GetType(A1, GetTypeOfVar, context);
                if (t1 is null)
                    throw new Exception("Couldn't type " + A1.GetCode);
                var a1 = t1.GetCode;
                if (!PTSDefinition.IsSort(a1))
                    throw new Exception(a1 + " was not a sort.");
                AddTyping(A.GetArgument, A1);
                var t2 = GetType(A2, GetTypeOfVar, context);
                if (t2 is null)
                    throw new Exception("Couldn't type " + A2.GetCode);
                var a2 = t2.GetCode;
                if (!PTSDefinition.IsSort(a2))
                    throw new Exception(a2 + " was not a sort.");
                RemoveTyping(A.GetArgument);
                var a3 = PTSDefinition.Rule(a1, a2);
                if (a3 is null)
                    throw new Exception("(" + a1 + ", " + a2 + ", s) was not a rule for any s.");
                var L = new LambdaTerm(a3);
                return L;
            }
            else if (A.IsAbstraction)
            {
                var A1 = A.GetArgumentType;
                var A2 = A.GetBody;
                var t1 = GetType(A1, GetTypeOfVar, context);
                if (t1 is null)
                    throw new Exception("Couldn't type " + A1.GetCode);
                if (!PTSDefinition.IsSort(t1.GetCode))
                    throw new Exception(t1.GetCode + " was not a sort.");
                AddTyping(A.GetArgument, A1);
                var t2 = GetType(A2, GetTypeOfVar, context);
                RemoveTyping(A.GetArgument);
                if (t2 is null)
                    throw new Exception("Couldn't type " + A2.GetCode);
                return LambdaTerm.NewAbstraction('#', A.GetArgument, A1, t2);
            }
            else if (A.IsApplication)
            {
                var A1 = A.GetFunction;
                var A2 = A.GetInput;
                var a1 = GetType(A1, GetTypeOfVar, context).BetaNormalForm();
                if (a1 is null)
                    throw new Exception("Couldn't type " + A1.GetCode);
                if (!a1.IsProduct)
                    throw new Exception("Type of " + A1.GetCode + " has to be a product type.");
                var a2 = GetType(A2, GetTypeOfVar, context);
                if (a2 is null)
                    throw new Exception("Couldn't type " + A2.GetCode);
                if(a1.GetArgumentType.BetaNormalForm() == a2.BetaNormalForm())
                {
                    return a1.GetBody.Replace(a1.GetArgument, A2);
                }
                else
                {
                    bool l = a1.GetArgumentType.BetaNormalForm() == a2.BetaNormalForm();
                    var a2b = a2.BetaNormalForm();
                    throw new Exception("Argument type in " + A1.GetCode + ", and type of " + A2.GetCode + " have to match.");
                }
            }
            return null;
        }
        private void AddTyping(string newvar, LambdaTerm newtype)
        {
            if (newvar == "_")
                return;
            if(TypingContext.ContainsKey(newvar))
            {
                throw new Exception("Cannot bind " + newvar + " twice.");
            }
            TypingContext[newvar] = newtype;
        }

        private void RemoveTyping(string x)
        {
            TypingContext.Remove(x);
        }
    }
}
