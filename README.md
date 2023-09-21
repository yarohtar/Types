# Types
To run, compile the tester project.
Enter the path to the tester project to some of the proof files (e.g. proof.txt proves some basic properties of natural numbers)
The console should write out which theorems in the file have been proof-checked.

# Notation
While writing a proof, you should be familiar with the Calculus of Constructions. That is the version of the lambda calculus that this program uses to check proofs.
The following are some keywords and examples used. It might be useful to consult the example proof files included in the project while reading these.
- `Prop` is a type of all propositions $(*)$
- `Type` is the type of `Prop` $(*)$ - doesn't have an actual use in this version (except to make some formalities work), but can be extended to an infinite family of types like in Coq
- $\lambda a:b.c$ (syntax: `@a:b.c`) is a function from type $b$ to the type of $c$. Eg $s=\lambda a:nat.a+1$ sends any natural number $a to $a+1$. We apply the function by writing $(s\ 2)$. Under $\beta$-reduction this becomes $2+1$. If $c$ has type $C$, the term $\lambda a:b.c$ proves "for all $a$ in $b$, $C$ holds" $(*)$
- $\Pi a:b.c$ (syntax: `# a:b.c`) is the type of $\lambda a:b.c$ where $c:C$. Type of $\Pi a:b.C$ is the type of $C$ (always either `Prop` or `Type`). The term should be read "For all $a$ in $b$, $C$ holds" or "whenever we have a term of this type and a term of type $b$ we can form a term of type $C$". $(*)$
- $a\rightarrow b$ (syntax: `a->b`) where $a$ and $b$ are types is replaced by $\Pi x:a.b$ where $x$ is a dummy variable name (note that $b$ does not depend on $x$ here). Read "$a$ implies $b$".
- When proving stuff, we state a theorem with a bunch of $\Pi$ (or `#`) expressions and prove it by finding any term of that type.
- `axiom a : b;` says `a` is of type `b`. `b` has to already be a valid type, while `a` has to be a valid variable name, which is then added to the context $(*)$
- Example: `axiom and : Prop->Prop->Prop;` says that `and` is a function from `Prop` to `Prop->Prop`. So `(and a)` would be a function from `Prop` to `Prop`, while `((and a) b)` would just be a `Prop` (in this example `a` and `b` need to have type `Prop`)
- Example: `axiom and_intro : #A:Prop.#B:Prop.A->B->(and A B);` says that given `A` and `B` which are of type `Prop` and given a term of type `A` and a term of type `B` we can have a term of type `(and A B)`
- `theorem a:b {}` where `a` is a name of the theorem and `b` is a valid type. Inside `{}` should be given a term of type `b` (in other words, proof of `b`). The whole point of the program is to check if these types match. Then in the future, whenever `a` shows up, it will be replaced by this proof given here. $(*)$
- `let a:A { something; }` is the term `@a:A.something`
- `infix plus +;` says that whenever `(a+b)` appears it will be replaced by `((plus a) b)`
- `func a [params]=b;` means that whenever `(a [params])` shows up it will be replaced by expression b (possibly depending on the `[params]`)
- Example: `func false=#A:Prop.A;`. The type `#A:Prop.A` claims that for any A of type `Prop` there is a term of type `A`. In a consistent system, this type is always empty, so we define it as "false". Proof by contradiction would find an element of this type.
- Example `func not A=A->false;`. Now `(not A)` means "`A` implies contradiction". Say we want to prove `A`. Assume not `A` holds and suppose we obtain a contradiction. So `(not A)->false`. So we found a term of type `(not (not A))`. We would still need another axiom to conclude `A` (excluded middle).
- `typeof A` is a builtin function which returns the type of term `A`.
- `reduced A` is a builtin function which returnes the most reduced form of `A`. Reduction step is `(@a:b.c k)` reduces to `c(k)` whenever possible. In a consistent system, `reduced A` always terminates
- `input A` is a builtin function which for `input (a b)` returns `b`
- `function A` is a builtin function which for `function (a b)` returns `a`
- we can build other functions using `func` keyword and these builtin ones (more builtin functions should be added for this to be more effective)

Note that using only the six points with $(*)$ we can write and check any proof we want, all the others are for our convenience. However, proof terms get extremely messy even with all the automations.
