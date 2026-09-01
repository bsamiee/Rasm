# Expressions - Building Blocks of Functional Programs

## The expression-oriented model

Functional programming evaluates expressions to values. Imperative languages divide language constructs into categories:
- Expressions evaluate to values: arithmetic such as `1 + 2`, booleans such as `true || false`, and strings such as `"Hello"`
- Statements issue commands and are characterized by side effects: assignments, statement-form conditionals, and loops

In the functional model, values move through function arguments and return values instead of variable assignments. Conditionals are expressions, and repeated computation uses recursion instead of `for` or `while` loops. Because every construct is an expression, simple expressions can be combined into larger ones without crossing an expression-statement boundary.

### Primitive and compound expressions

Literal integers such as `1` and `2` are primitive expressions. Operators combine operands into compound expressions, and a compound expression can itself be an operand:

```ocaml
(1 + 2) * (3 - 4 * 5)
```

OCaml distinguishes integer arithmetic from floating-point arithmetic:

| Operation      | Integer | Float |
| -------------- | ------- | ----- |
| Addition       | `+`     | `+.`  |
| Subtraction    | `-`     | `-.`  |
| Multiplication | `*`     | `*.`  |
| Division       | `/`     | `/.`  |
| Remainder      | `mod`   | -     |

Floats appear as `2.0` or `2.`:

```ocaml
3.14 *. 2. *. 2.
```

Comparison operators such as `=`, `>`, `>=`, `<`, and `<=` produce `bool` values. Boolean expressions combine through `not`, `&&`, and `||`. `not` is unary because it accepts one operand; the infix arithmetic, comparison, and binary logical operators shown here accept two.

```ocaml
((1 + 2) * (3 - 4 * 5) = -51) || (1 > 2)
not (true && (1 > 2))
```

String literals are enclosed in quotes, and `^` concatenates strings:

```ocaml
"Hello " ^ "FP"  (* "Hello FP" *)
```

### Conditional expressions

An OCaml conditional has the form:

```ocaml
if e1 then e2 else e3
```

The condition `e1` is evaluated first. If it is `true`, the entire expression has the value of `e2`; otherwise it has the value of `e3`. In this value-producing form, the explicit `else` branch supplies a value for the other outcome. Every position may contain another expression, conditionals can nest arbitrarily.

Because a conditional is an expression, it can be an operand of another expression:

```ocaml
(if 1 > 2 then 0 else 42) + 3
```

This uniform ability to combine is a central advantage of an expression-only model. Statement-form `if` cannot go where an operator expects a value.

## Syntax and expression structure

The syntax of a language is the set of rules that determines which character sequences are valid programs. Validity is language-specific: the same input string may be accepted by one language and rejected by another.

Textual programming-language syntax is hierarchical. In the chapter's OCaml example, it is built in layers:
1. ASCII characters form literals, operators, keywords, and names
2. Those units are arranged according to grammar rules
3. The resulting expression has a hierarchical structure

Natural-language syntax has the same shape: letters form words, and words and punctuation form phrases and sentences. Recognition requires both valid units and a valid arrangement of those units. Recursive grammar rules also explain the hierarchy inside a phrase:

```bnf
<noun_phrase> ::= <noun>
                | <adjective> <noun_phrase>
                | <noun_phrase> <noun>

<noun>      ::= baby | cat | dog | ...
<adjective> ::= cute | small | ...
```

Under these rules, `cute baby cat` consists of the adjective `cute` applied to the noun phrase `baby cat`; the tree captures that grouping rather than treating the phrase as a flat sequence.

BNF describes alternatives with `|` and can define expressions recursively:

```bnf
<expr> ::= <number>
         | <unop> <expr>
         | <expr> <binop> <expr>
         | if <expr> then <expr> else <expr>

<number> ::= 0 | 1 | 2 | ...
<unop>  ::= not | ...
<binop> ::= + | - | * | ...
```

Recursive productions allow expressions of unbounded depth. Their structure is a tree. For `1 + 2 * 3`, precedence makes `2 * 3` the right operand of `+`, the tree records multiplication below the right branch of addition rather than treating the input as a flat sequence.

## Parsing

Parsers determine whether input is syntactically valid and construct its structural representation. Parsing proceeds in conceptual stages:
1. Recognize language units. Characters are grouped into literals, operators, and keywords. If a group is not a valid language unit, parsing stops. `1xyz + 2` yields an invalid-literal error because `1xyz` cannot be recognized as a valid literal.
2. Recognize grammatical structure. The units are checked against the grammar by attempting to build a parse tree. `1 +` yields a syntax error: `+` requires a right operand, even though both `1` and `+` are individually recognized units.

For `if 1 = 2 then 0 else 42`, the recognized units form an `if`-expression tree whose condition is equality between `1` and `2`, with `0` and `42` as the two branches.

The parser normally produces an abstract syntax tree (AST) for later compiler phases. An AST preserves structured content while omitting details no longer needed, such as whitespace, parentheses, and syntax keywords. The AST is subsequently used for type checking and code generation.

The parser partitions all possible input strings: syntactically valid expressions are accepted; every other string is rejected before later phases.

## Types and type checking

Syntactic validity is insufficient to establish that an expression makes sense. `1 + true` has the grammatical shape "expression, binary operator, expression," but `+` expects numeric operands and receives an integer and a boolean. Such an expression does not type check.

### Dynamic and static typing

With dynamic typing, type checking is not performed at compile time. JavaScript, for example, accepts `1 + true` and coerces `true` to `1`, producing `2`; an invalid operation such as calling a nonexistent `length` method on a number instead fails during execution. This flexibility allows type mistakes to survive until runtime. TypeScript adds static typing to JavaScript to mitigate this problem.

Statically typed languages such as OCaml and Haskell check expressions before execution. OCaml rejects `1 + true` during compilation. Static checking narrows the accepted syntactically valid expressions to those that also satisfy the typing rules.

### Recursive type inference

After parsing succeeds, the type checker infers the AST's type from its leaves upward:
- Integer literals have type `int`; `true` and `false` have type `bool`; string literals have type `string`
- If unary operator `unop` accepts type `t` and returns type `t`, and `e : t`, then `unop e : t`
- If binary operator `binop` accepts `t1` and `t2` and returns `t3`, and `e1 : t1` and `e2 : t2`, then `e1 binop e2 : t3`
- For `if e1 then e2 else e3`, `e1` must have type `bool`, and `e2` and `e3` must have the same type `t`; the whole conditional has type `t`

Consequently, each of these is rejected for a distinct type mismatch:

```ocaml
if "Not bool" then 0 else 42       (* condition is not bool *)
if true then 0 else "a string"     (* branches have different types *)
```

Compiler acceptance forms nested sets: all strings contain the syntactically valid expressions; those contain the expressions that type check. Syntax errors never reach the type checker, and type errors never reach code generation or execution.

## Values and semantics

Syntax concerns whether and how characters form an expression. Semantics concerns the expression's meaning or value. For `12 + 34`, parsing the character sequence and constructing its AST are syntactic work; obtaining `46` is semantic evaluation.

### Interpretation

At its simplest, an interpreter takes an AST and evaluates it directly by following its recursive structure:
- Numbers, booleans, and strings evaluate to their values
- For `unop e`, evaluate `e`, then apply `unop`
- For `e1 binop e2`, evaluate both operands, then apply `binop` to their values
- For `if e1 then e2 else e3`, evaluate only the condition and the selected branch: evaluate `e2` if the condition is `true`, otherwise evaluate `e3`

Values propagate from the leaves upward until the root receives its value. For `(1 + 2) > (3 - 4 * 5)`, the subexpressions evaluate to `3` and `-17`, the root comparison evaluates to `true`. This recursive evaluation mirrors recursive type inference because both algorithms follow the expression tree.

### Compilation

In the compiled model presented here, the source AST is not evaluated directly. The compiler generates either bytecode for a bytecode runner or native code for a CPU. OCaml provides `ocamlc` for bytecode and `ocamlopt` for native code. The complete path is:

```text
input characters -> parser/AST -> type checker -> code generator -> bytecode runner or CPU -> value
```

Parsing and AST construction concern syntax; evaluating interpreted code or executing generated code produces semantics.

## Naming expressions and building abstractions

Names make the intent of computations visible:

```ocaml
let pi = 3.14
let radius = 2.
let circle_area = pi *. radius *. radius
```

An imperative variable is a labeled memory cell whose contents change over time. For example, a loop that sums squares repeatedly updates cells named `sum` and `i`. Functional `let` bindings name a computation, not a memory cell to update later.

Once `pi` names `3.14`, the name forms a stable conceptual unit that can participate in higher-level definitions such as `circle_area`. Naming supports abstraction by letting a complex concept be treated as one unit and combined into still higher-level concepts. In OCaml, `=` compares values; it is not a reassignment operator.

### Global and local scope

Top-level bindings are visible to following expressions. `let ... in ...` introduces a name whose scope is limited to its body and is itself an expression:

```bnf
<expr> ::= ... | let <name> = <expr> in <expr>
```

```ocaml
let pi = 3.14
let circle_area =
  let radius = 2. in
  pi *. radius *. radius
```

Here `radius` is unavailable outside the `let ... in ...` body. Because both the bound expression and body are expressions, local bindings can nest arbitrarily:

```ocaml
let pi = 3.14 in
let radius = 2. in
pi *. radius *. radius
```

Local names shadow a global name without modifying it:

```ocaml
let a = 42
let b = let a = 1 in a + 1
```

The local `a` makes `b = 2`; the global `a` remains `42`.

## Functional rewrites

### Maximum of three values

Do not update a running maximum. Construct one expression that computes it:

```ocaml
let max_number =
  let m = if x > y then x else y in
  if m > z then m else z
```

### Absolute value

```ocaml
let a = if x >= 0 then x else -x
```

## Consolidated conclusions

- Functional programs retain functions but replace variable assignments and `for`/`while` loops with expression composition, value passing, and recursion
- With `let x = 42`, `x = x + 1` compares `42` and `43`; it evaluates to `false` and does not mutate `x`
- `if true then 42 else "Bye"` is syntactically valid but fails type checking because its branches have types `int` and `string`
- `if 00aa = 0 then "Hi" else "Bye"` fails during parsing because `00aa` is not a valid literal
- `let s = if 42 mod 2 = 0 then "even" else "odd" in "The number is " ^ s` evaluates to `"The number is even"`
- `let s = if 1 > 2 then "a" else "b" in (String.length s) > 0` has type `bool`
- Java statement-form `if` cannot serve as an operand of `>` because it does not evaluate to a value
