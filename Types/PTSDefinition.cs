using System;
using System.Collections.Generic;
using System.Text;

namespace PTS
{
    public static class PTSDefinition
    {
        static Func<string, bool> issort;
        static Func<string, string> axioms;
        static Func<string, string, string> rules;
        static bool defined = false;

        static void SetSorts(ICollection<string> sorts)
        {
            issort = (string a) => sorts.Contains(a);
        }
        static void SetAxioms(Func<string, string> ax)
        {
            axioms = (string a) => issort(a) && issort(ax(a)) ? ax(a) : null;
        }
        static void SetRules(Func<string, string, string> r)
        {
            rules = (string a, string b) => issort(a) && issort(b) && issort(r(a, b)) ? r(a, b) : null;
        }

        public static void DefinePTS(ICollection<string> s, Dictionary<string,string> a, Dictionary<(string, string), string> r)
        {
            if (defined)
                return;
            defined = true;
            SetSorts(s);
            SetAxioms((string x) => a[x]);
            SetRules((string x, string y) => r[(x,y)]);
        }

        public static void DefinePTS(ICollection<string> s, Func<string, string> a, Func<string, string, string> r)
        {
            if (defined)
                return;
            defined = true;
            SetSorts(s);
            SetAxioms(a);
            SetRules(r);
        }

        public static bool IsSort(string a) => issort(a);
        public static string Axiom(string a) => axioms(a);
        public static string Rule(string a, string b) => rules(a, b);
    }
}
