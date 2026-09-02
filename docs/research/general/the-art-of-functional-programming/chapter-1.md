# Chapter 1: Introduction

## Orientation

Functional programming is a programming paradigm: a style and way of thinking about software. It is especially strong at abstraction, composition, concise solutions, and increased safety. The durable skill is not memorizing functional syntax in one language, but learning principles and problem-solving techniques that transfer across languages, frameworks, and tools.

The governing balance is:
- Understand the fundamental principles of the functional paradigm
- Apply those principles pragmatically to real problems

The practical emphasis includes routine programming work, collection processing for an e-commerce application, hierarchical data (JSON), and foundational language techniques (parsing, type checking, interpretation, and compilation).

The material is aimed at beginner and intermediate software engineers, engineering managers, computer science students, and people developing problem-solving skills for coding interviews. OCaml is the main demonstration language, Haskell is used occasionally, and Java provides imperative contrasts. The techniques also transfer to languages that support functional style, including Swift, Kotlin, JavaScript, Go, Python, and Java through its Stream API.

### Conceptual progression

1. Expressions: composition from simpler expressions, syntax, types, semantics, parsing, type checking, interpretation, and compilation
2. Functions: lambda calculus, computation patterns, currying, recursion, and higher-order functions
3. Complex data: tuples, lists, algebraic data types for hierarchical data, and pattern matching
4. Common patterns: `map`, `filter`, `fold`, and `zip` over lists and other structures
5. Dataflow programming: composing programs from existing components
6. Applications: collection processing and JSON representation and handling

## Programming paradigms

Programming paradigms include imperative, object-oriented, functional, dataflow, and logical programming.

Imperative programming models a program as an ordered sequence of commands that change program state: first perform one action, then another. Object-oriented structure does not imply functional implementation, methods inside classes are commonly written imperatively.

Imperative programming's historical dominance is connected to the von Neumann architecture used by most computers.

## Von Neumann architecture

Minimal von Neumann machines have:
- CPU with a control unit, arithmetic logic unit, and a small set of registers
- Memory holding both program instructions and data
- Bus connecting CPU and memory

Machine instructions are binary and primitive: arithmetic, equality tests, loads, and stores. Loads move data from memory into registers, stores move register data back into memory.

Execution follows a fetch-execute cycle:
1. Fetch an instruction from memory
2. Execute it in the CPU
3. Fetch the next instruction and continue

Jump and branch instructions alter the otherwise sequential order. Jumps return to an earlier instruction, branches choose an instruction when a condition holds. Together they implement loops and conditionals.

## Why imperative programming is low-level

Programming directly for this architecture centers on stepwise memory updates. Programs are sequences of instructions that move data between memory and CPU, perform arithmetic or logic, and write values back.

An imperative sum of the squares from `1` through `n` illustrates this model:

```java
int sum = 0;
int i = 0;
while (i < n) {
    i = i + 1;
    sum = sum + i * i;
}
```

`sum` and `i` correspond to memory cells. `i = i + 1` conceptually loads `i`, adds one in the CPU, and stores the result. The loop maps to branch and jump instructions.

This close conceptual coupling makes imperative programs comparatively weak at abstraction and composition. Loops often form a monolithic unit rather than an assembly of reusable parts.

## Functional composition and dataflow

The chapter illustrates the same computation as a composition of reusable operations using the following functional notation:

```text
(fold (+) 0 . map square) [1..n]
```

- `map square` applies `square` to every list element
- `fold (+) 0` combines the transformed elements with addition, beginning from `0`
- `.` feeds the output of one function into the next
- Only `square`, `(+)`, and `0` are specific to the task, `map`, `fold`, and composition are general-purpose components

For a concrete input, the data flows as:

```text
[1; 2; 3; 4] -> map square -> [1; 4; 9; 16] -> fold (+) 0 -> 30
```

The structural contrast between these versions is:
- The imperative version prescribes how variables are initialized and updated. Understanding it requires mentally executing its steps.
- The functional version describes what transformation occurs. Its pipeline structure exposes the computation directly and lowers the cognitive burden of understanding it.

### Extending the computation

To sum only the squares of prime numbers, imperative code copies or modifies the loop and inserts a conditional. The functional form adds a reusable filtering stage:

```text
(fold (+) 0 . map square . filter isPrime) [1..n]
```

Its dataflow for a concrete input is:

```text
[1; 2; 3; 4]
  -> filter isPrime -> [2; 3]
  -> map square     -> [4; 9]
  -> fold (+) 0     -> 13
```

`filter` selects the values satisfying `isPrime`, `map` transforms them, and `fold` aggregates them. The solution changes by plugging another general component into the pipeline rather than restructuring a monolithic loop.

Functional programming is not universally better than imperative programming. Each paradigm fits some situations better than others, expertise includes recognizing when functional composition is useful and when another approach is more suitable.

## Why functional programming matters

### Broader problem-solving toolkit

Functional programming is particularly suitable for:
- Hierarchical structures and languages with well-defined syntax, including JSON, XML, and domain-specific languages
- Filtering, transforming, and aggregating collections in mobile applications, web applications, and backend services
- Managing complexity through abstraction and composition

Composition builds complex programs from simpler programs. Abstraction captures recurring computation patterns in general, reusable functions. These capabilities improve software design and support reusable code even when day-to-day work is not primarily functional.

Industry adoption reinforces the value of the skill: Java has added functional features. Elm, Elixir, Scala, Swift, and Kotlin support functional programming directly. ReactiveX and Akka Streams are heavily based on the paradigm.

### The movement toward declarative systems

Functional programming is part of a wider transition from imperative to declarative software. Declarative systems state what result is wanted and leave the mechanism to the system, imperative systems prescribe an ordered sequence of steps.

Examples of declarative approaches include:
- Functional programming
- Declarative UI, including Flutter and Jetpack Compose
- Declarative build systems, including Maven and Gradle
- Declarative build pipelines, including declarative Jenkins pipelines
- Declarative infrastructure as code, including Terraform and CloudFormation

Ant represents the contrasting imperative build style: it explicitly orders the commands used to perform a build. Declarative build definitions work at a higher level, hide implementation details, and make complex builds easier to compose from smaller steps. Similar advantages apply to UI and infrastructure definitions.

Learning functional programming develops declarative thinking that transfers across languages, libraries, frameworks, build tools, deployment pipelines, and infrastructure tools.

### Changed mental model

Functional concepts can initially feel awkward because they require a different way of thinking. That discomfort is expected: a worthwhile programming language or paradigm changes how a programmer understands and structures problems.

## Working environments

Examples can be evaluated in the browser with Try OCaml and Try Haskell, or locally with compilers and REPLs.

### OCaml toolchain

OPAM is the package manager used to install and select OCaml compiler versions. The setup shown uses OCaml `4.12.0`, the newest version at the time:

```sh
opam init
eval `opam env`
opam switch create 4.12.0
eval `opam env`
which opam
which ocamlc
```

The installed tools have distinct roles:
- `ocamlc` compiles OCaml source to bytecode
- `ocamlopt` compiles OCaml source to native object code and links standalone executables
- `ocamlrun` executes bytecode produced by `ocamlc`

### OCaml REPL: utop

```sh
opam install utop
utop
```

In `utop`, an expression conventionally ends with `;;`. Evaluation prints both its type and value:

```ocaml
utop # 1 + 2;;
- : int = 3
```

Useful interaction:
- Tab performs completion
- Up arrow recalls previously entered expressions
- Input can span multiple lines until terminated by `;;`
- `#quit;;` exits

### Haskell compiler and REPL

GHC is the Glasgow Haskell Compiler. After installation, `which ghc` verifies the compiler and `which ghci` verifies its REPL. `ghci` evaluates Haskell expressions directly:

```haskell
Prelude> 1 + 2 * 3
7
```

`:quit` exits `ghci`.

## Quiz conclusions

1. Imperative abstraction and composition are limited because the paradigm is conceptually tied to the von Neumann architecture and its movement and mutation of data, not because imperative programming lacks functions or combination operators
2. Imperative programming and functional programming are programming paradigms, microservice is not
3. Quiz 3 selects only domain-specific languages and well-defined hierarchical syntax (JSON, XML)
4. Maven and Terraform follow declarative approaches, Ant is imperative
