using System;
using System.Collections.Generic;
using System.Text;

namespace PTS
{
    public partial class Context
    {
        Dictionary<string, LambdaTerm> globalContext = new Dictionary<string, LambdaTerm>();

        Dictionary<string, LambdaTerm> localContext = new Dictionary<string, LambdaTerm>();
        Stack<(string, LambdaTerm)> locals = new Stack<(string, LambdaTerm)>();

        HashSet<string> vars = new HashSet<string>();

        public List<Func<string, string>> SyntaxRules = new List<Func<string, string>>();

        public Context()
        {

        }

        public Context(Context c)
        {
            globalContext = new Dictionary<string, LambdaTerm>(c.globalContext);
            localContext = new Dictionary<string, LambdaTerm>(c.localContext);
            locals = new Stack<(string, LambdaTerm)>(c.locals);
            vars = new HashSet<string>(c.vars);
            SyntaxRules = new List<Func<string, string>>(c.SyntaxRules);
        }

        public void AddContext(Context b)
        {
            if (localContext.Count > 0 || b.localContext.Count > 0)
                throw new Exception("Cannot combine context that are not purely global");
            foreach (var x in b.globalContext)
            {
                globalContext[x.Key] = x.Value;
                vars.Add(x.Key);
            }
            SyntaxRules.AddRange(b.SyntaxRules);
        }

        public Context SubContext()
        {
            var con = new Context();
            con.globalContext = new Dictionary<string, LambdaTerm>(globalContext);
            foreach(var k in localContext.Keys)
            {
                con.globalContext[k] = new LambdaTerm(localContext[k]);
            }
            con.locals = new Stack<(string, LambdaTerm)>();
            con.vars = new HashSet<string>(vars);
            return con;
        }

        public void AddAxiom(string newvar, LambdaTerm newtype)
        {
            var t = GetType(newtype, "global");
            if (t is null)
                throw new Exception("Couldn't type " + newtype.GetCode);
            if (!PTSDefinition.IsSort(t.GetCode))
                throw new Exception(t.GetCode + " was not a sort.");
            if (newvar == "_")
                return;
            if (globalContext.ContainsKey(newvar))
            {
                throw new Exception();
            }
            globalContext[newvar] = newtype;
            vars.Add(newvar);
        }

        public void RemoveAxiom(string x)
        {
            globalContext.Remove(x);
            vars.Remove(x);
        }

        public void AddLocal(string newvar, LambdaTerm newtype)
        {
            var t = GetType(newtype, "local");
            if (t is null)
                throw new Exception("Couldn't type " + newtype.GetCode);
            if (!PTSDefinition.IsSort(t.GetCode))
                throw new Exception(t.GetCode + " was not a sort.");
            if (newvar == "_")
                return;
            if (localContext.ContainsKey(newvar))
            {
                throw new Exception();
            }
            locals.Push((newvar, newtype));
            localContext[newvar] = newtype;
            vars.Add(newvar);
        }
        public (string, LambdaTerm) PopLocal()
        {
            var a = locals.Pop();
            localContext.Remove(a.Item1);
            vars.Remove(a.Item1);
            return a;
        }
        public Stack<(string, LambdaTerm)> PopLocals()
        {
            var s = new Stack<(string, LambdaTerm)>();
            while (locals.Count > 0)
            {
                var x = locals.Pop();
                vars.Remove(x.Item1);
                s.Push(x);
            }
            localContext.Clear();
            return s;
        }

        internal LambdaTerm GetLocalTypeOfVar(string x)
            => localContext.ContainsKey(x) ? new LambdaTerm(localContext[x]) :
            globalContext.ContainsKey(x) ? new LambdaTerm(globalContext[x]) : null;
        internal LambdaTerm GetGlobadTypeOfVar(string x)
            => globalContext.ContainsKey(x) ? new LambdaTerm(globalContext[x]) : null;

        public HashSet<string> GetVars
        {
            get
            {
                var v = new HashSet<string>(vars);
                v.UnionWith(TypingContext.Keys);
                return v;
            }
        }
    }
}
