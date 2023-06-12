# Types
To run, compile tester project.
Enter path to the tester project to some of the proof files (ex proof.txt proves some basic properties of natural numbers)
Console should write out which theorems in the file have been proof checked.

# Notation
When writing a proof file, these are some of the keywords used:
- Prop is a type of all propositions (\*)
- Type is the type of Prop - doesn't have real use in this version (except to make some formailites work) but can be extended to infinite family of types like in Coq (\*)
- @a:b.c is a function from type b to type of c. Eg s=@a:nat.a+1 sends any natural number a to a+1. We apply the function by writing (s 2). This can be reduced to 2+1. If c has type C, this term proves "for all a in b, C holds" (\*)
- #a:b.C is the type of @a:b.c where c:C. Type of #a:b.C is the type of C (always either Prop or Type). Read "For all a in b, C holds" or "whenever we have a term of ths type and a term of type b we can form a term of type C". (\*)
- a->b where a and b are types is replaced by #x:a.b where x is a dummy variable name (note that b does not depend on x here). Read "a implies b".
- When proving stuff, we state a theorem with a bunch of # expresions and prove it by finding any term of that type.
- axiom a : b; says a is of type b. b has to already be a valid type, while a becomes a new variable (\*)
- axiom and : Prop->Prop->Prop; says that and is a function from Prop to Prop->Prop. Or from 2 Props to a Prop.
- axiom and_intro : #A:Prop.#B:Prop.A->B->(and A B); says that given Props A and B and given a term of type A and a term of type B we can have a term of type (and A B)
- theorem a:b {} where a is a name of the theorem and b is a valid type. Inside {} should be given a term of type b (in other words, proof of b). The whole point of the program is to check if these types match. Then in the future, whenever a shows up, it will be replaced by this proof. (\*)
- let a:A { something; } is the term @a:A.something
- infix plus +; says that whenever (a+b) appears it will be replaced by ((plus a) b)
- func a [params]=b; means that whenever (a [params]) shows up it will be replaced by expressoin b (possibly depending on the [params])
- func false=#A:Prop.A; type #A:Prop.A claims that for any Prop A there is a term of type A. In a consistent system this type is always empty, so we define it as "false". Proof by contradiction would find an element of this type.
- func not A=A->false; or (not A) means "A implies contradiction". Say we want to prove A. Assume not A holds and suppose we obtain a contradiction. So (not A)->false. So we found a term of type (not (not A)). We would still need another axiom to conclude A (excluded middle).
- typeof A is a builtin function which returns the type of term A.
- reduced A is a builtin function which returnes the most reduced form of A. Reduction step is (@a:b.c k) reduces to c(k) whenever possible. In a consistent system, reduced A always exists
- input A is a builtin function which for A=(a b) returns b
- function A is a builtin function which for A=(a b) returns a
- we can build other functions using func and these builtin ones

Note that only using only the six points with (\*) we can write and check any proof we want, all the others are for our convenience. However, proof terms get extremely messy even with all the automations.
