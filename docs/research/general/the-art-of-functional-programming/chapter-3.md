# Building Abstractions with Functions

## Lambda calculus

Lambda calculus is an idealized, minimal functional language and the theoretical foundation of functional programming. It has only variables and functions, yet Church encoding can represent numbers, booleans, pairs, lists, and general computation.

### Lambda expressions

Every lambda expression is built by one of three rules:
- A variable such as `x` is an expression.
- If `x` is a variable and `e` an expression, the function abstraction `lambda x. e` is an expression.
- If `e1` and `e2` are expressions, the function application `e1 e2` is an expression.

In BNF:

```text
<var> ::= x | y | myVar | ...
<lambda_expr> ::= <var>
                | λ <var> . <lambda_expr>
                | <lambda_expr> <lambda_expr>
```

Parentheses remove ambiguity. `lambda x. x` is the identity function; `(lambda x. x) y` applies it to `y`.

### Reduction

Running a lambda-calculus program means repeatedly reducing its expression until no reduction remains; the result is its value. Variables and function abstractions are fully reduced. An application of the form `(lambda x. e1) e2` is a reducible expression, or redex; reduction substitutes the actual argument `e2` for the formal parameter `x` in `e1`. Thus `(lambda x. x x) y` reduces to `y y`.

Not every expression reaches a value. `(lambda x. x x) (lambda x. x x)` reduces to itself forever and represents an undefined, non-terminating computation.

### First-class functions

Variables and functions are both expressions. A function can be passed to another function or returned from one just like any other value. This first-class status enables higher-order functions. Functional languages retain this property; conventional imperative languages generally treat functions as a separate, second-class category.

### Currying and partial application

Lambda calculus supports only unary functions. Currying represents a multi-argument function as nested unary functions: `λx. λy. f x y`.

Applying it to `u` and `v` proceeds one argument at a time:

`(λx. λy. f x y) u v` reduces to `(λy. f u y) v`, then to `f u v`.

Stopping after the first application fixes `x` to `u` and returns a unary function. This is partial application.

### Reduction strategies

When both an application and its argument are redexes, evaluation order matters:
- Call by value: fully reduce the argument, then substitute its value.
- Call by name: substitute the unreduced argument expression.

For `(lambda x. x x) ((lambda y. y) z)`, call by value first reduces the argument to `z`; call by name first duplicates the argument. Both ultimately produce `z z`. In general, the paths differ but the results agree whenever both terminate.

They differ when an unused argument does not terminate, as in `(lambda y. z) ((lambda x. x x) (lambda x. x x))`.

Call by value attempts to reduce the argument forever; call by name substitutes it into a body containing no `y`, so the result is `z`.

## Functions in OCaml

The syntax below is OCaml-specific, while most of the underlying treatment of function abstraction and application works similarly in functional languages such as Haskell and Scala; evaluation strategy is a notable point of difference.

### Declaration and application

OCaml's `fun` corresponds to lambda abstraction; `fun x -> x *. x` accepts a float and returns its square.

A function is an expression that evaluates to a function value. An anonymous function can be named with `let`; the shorter named-function syntax is equivalent:

`let square = fun x -> x *. x` and the syntactic sugar `let square x = x *. x` are equivalent.

Application places the function beside its argument: `(fun x -> x *. x) 2.` evaluates to `4.`.

OCaml is strict and uses call by value: it evaluates an argument before entering the function body. Haskell is non-strict and evaluates an argument only if needed. Consequently:

```ocaml
let cons x = 42
cons (1 / 0)  (* Division_by_zero *)
```

The analogous Haskell expression returns `42` because the body does not use the erroneous argument. Call by name can duplicate work when a used argument appears multiple times, while avoiding work for an unused argument.

### Function types

OCaml is strongly and statically typed, so every expression has a type. A function from `t1` to `t2` has type `t1 -> t2`. OCaml infers `square : float -> float` from the floating-point operator `*.` and rejects incompatible applications at compile time.

An input or output may itself be a function type. Because `->` associates to the right, `int -> bool -> string` means `int -> (bool -> string)`: accept an integer and return a function that accepts a boolean and returns a string. One inhabitant is:

```ocaml
fun x -> fun y -> if (x > 0) && y then "Hello" else "Good bye"
```

Function types expose the public contract and are often the fastest way to understand how a function can be used.

### Functions as black-box abstractions

Naming a computation turns it into a reusable concept. A client of `square` depends on its promise, not its implementation:

```ocaml
let circle_area r = pi *. square r
```

`circle_area` composes existing functions and becomes a new abstraction in its own right. Building programs stepwise means identifying computations parameterized by inputs, naming them as functions, treating each as a black box, and using them to construct higher-level functions.

## Currying for function chaining

Currying formulates a function that accepts multiple arguments as a nested chain of unary functions. Most functional programming languages, including OCaml, incorporate this technique. In OCaml, operators are ordinary functions when parenthesized. Integer addition has type `(+) : int -> int -> int`, equivalent to `int -> (int -> int)`.

Multi-argument syntax is sugar for nested unary functions:

```ocaml
fun x y -> x * x + y * y
(* fun x -> fun y -> x * x + y * y *)

let rectangle_area w h = w *. h
(* let rectangle_area = fun w -> fun h -> w *. h *)
```

Partial application specializes such a function by fixing its first arguments:

```ocaml
let rectangle_area_of_width_2 = rectangle_area 2.
rectangle_area_of_width_2 3.  (* 6. *)
let inc = (+) 1
let double = ( * ) 2
```

Spaces in `( * )` avoid collision with the OCaml comment opener `(*`. `rectangle_area 2.5 3.5` should be read as `(rectangle_area 2.5) 3.5`: first produce a unary function, then apply it. This view is useful when deciding how to arrange a multi-argument function's parameters.

## Recursive functions

A functional formulation can express repetition through recursive structure rather than assignment-driven loops. The recurrence `sum(0) = 0`; `sum(n) = n + sum(n - 1)` for `n > 0` translates directly into OCaml using `rec`:

```ocaml
let rec sum n = if n <= 0 then 0 else n + sum (n - 1)
```

This version builds deferred additions. Evaluating `sum 5` expands through `5 + (4 + (3 + ...))`; each suspended call requires a stack frame, so stack use grows linearly with `n` and sufficiently large input such as `sum 1000000` causes `Stack_overflow`.

### Tail recursion

Carry the changing state as arguments: a running sum `s`, counter `c`, and upper bound `n`.

The recurrence is `sum(s, c, n) = s` when `c > n`, otherwise `sum(s + c, c + 1, n)`.

```ocaml
let rec sum_iter s c n =
  if c > n then s else sum_iter (s + c) (c + 1) n
```

The recursive call is the final operation, so there is no deferred calculation after it returns. Its process advances directly:

`sum_iter 0 1 5` advances through `sum_iter 1 2 5`, `sum_iter 3 3 5`, `sum_iter 6 4 5`, `sum_iter 10 5 5`, and `sum_iter 15 6 5`.

OCaml and Haskell optimize tail calls so this process does not grow the call stack. Recursion can therefore implement loops without special `for` or `while` constructs. A tail-recursive OCaml function can run forever without stack overflow, whereas Java does not generally optimize tail recursion and eventually exhausts its stack.

Tail recursion is valuable for large inputs, but is not automatically preferable: when stack depth is safe, a direct non-tail-recursive definition may express the computation more clearly. Recursive functions are especially natural for recursively structured data such as lists and trees.

## Higher-order functions as general computation methods

A higher-order function accepts a function or returns one. First-class functions let such functions capture patterns that vary over another computation.

### Summation

Summation over `[m, n]` varies only in the function that generates each term:

```ocaml
let rec sum term m n =
  if m > n then 0 else term m + sum term (m + 1) n

let sum_integers = sum (fun i -> i)
let sum_integer_squares = sum (fun i -> i * i)

sum_integers 1 3         (* 6 *)
sum_integer_squares 1 3  (* 14 *)
```

The mathematical sigma notation names the concept of summation; `sum` captures an executable method for calculating it. Partial application removes pass-through bounds and produces focused functions by fixing `term`.

### Accumulation

Products have the same recursive shape but use multiplication and identity `1`:

```ocaml
let rec product term m n =
  if m > n then 1 else term m * product term (m + 1) n

let product_integers = product (fun x -> x)
let product_integer_squares = product (fun x -> x * x)

product_integers 1 3         (* 6 *)
product_integer_squares 1 3  (* 36 *)
```

Factor the shared pattern into `accumulate`. `combiner` combines the current term with the recursive accumulation, `init` is the empty-range value, `term` generates values, and `[m, n]` is inclusive:

```ocaml
let rec accumulate combiner init term m n =
  if m > n then init
  else combiner (term m) (accumulate combiner init term (m + 1) n)

let sum = accumulate (+) 0
let product = accumulate ( * ) 1
```

Its type is `('a -> 'b -> 'b) -> 'b -> (int -> 'a) -> int -> int -> 'b`.

The abstraction hierarchy rises from concrete computations such as sums of integers or squares, through `sum` and `product`, to `accumulate`. Higher levels reduce duplication and mental effort: solving a new problem becomes configuring a known method rather than rebuilding recursion.

`let sum_integer_cubes = accumulate (+) 0 (fun x -> x * x * x) 1` configures that general method.

Calling `sum_integer_cubes n` computes `1^3 + 2^3 + ... + n^3`.

## Challenge solutions and derived techniques

### Prime detection

A number is prime when no divisor from `2` through its square root divides it. Stop once `m * m > n`:

```ocaml
let rec is_prime n =
  if n < 2 then false
  else if n = 2 then true
  else
    let rec aux m =
      if n mod m = 0 then false
      else if m * m > n then true
      else aux (m + 1)
    in aux 2
```

### Fibonacci: direct and efficient

The direct recurrence repeats subproblems and becomes impractical for large `n`:

```ocaml
let rec fib n =
  if n <= 0 then 0
  else if n <= 2 then 1
  else fib (n - 1) + fib (n - 2)
```

Only the two adjacent Fibonacci values are needed to produce the next. Carry them in a tail-recursive helper:

```ocaml
let super_fib n =
  let rec helper a b m =
    if m >= n then a else helper b (a + b) (m + 1)
  in helper 0 1 0
```

This computes `super_fib 60 = 1548008755920` quickly.

### Applying and composing functions

```ocaml
let twice f x = f (f x)
(* twice : ('a -> 'a) -> 'a -> 'a *)

let compose f g x = f (g x)
(* compose : ('a -> 'b) -> ('c -> 'a) -> 'c -> 'b *)
```

With `inc = (+) 1` and `double = ( * ) 2`, `twice inc 0 = 2` and `compose double inc 3 = 8`.

### Filtered accumulation

Apply `combiner` only when the generated term satisfies predicate `p`:

```ocaml
let rec filtered_accumulate combiner init term p m n =
  if m > n then init
  else if p (term m) then
    combiner (term m)
      (filtered_accumulate combiner init term p (m + 1) n)
  else filtered_accumulate combiner init term p (m + 1) n
```

Its type is `('a -> 'b -> 'b) -> 'b -> (int -> 'a) -> ('a -> bool) -> int -> int -> 'b`.

For example, summing identity terms that are prime over `1..4` yields `5`; over `2..5`, `10`; over `3..7`, `15`.

## Function reasoning checkpoints

- `(lambda x. x y) (lambda z. z)` reduces to `y`.
- Higher-order functions are the distinctive abstraction mechanism enabled by first-class functions; merely defining ordinary functions is not unique to functional languages.
- A strict OCaml application evaluates an erroneous or non-terminating argument even when the body ignores it; the corresponding non-strict Haskell application can return without evaluating it.
- OCaml string concatenation as a function has type `(^) : string -> string -> string`, not a tuple-argument type.
- `(fun x -> 42) (endless 1)` never terminates in OCaml because the argument is evaluated first.
- `((>) 10) 9` is `true`: partial application fixes the left operand, so it tests whether `10 > 9`.
- If `judgment f x = if f x then "it's true" else "it's false"`, then `judgment (fun x -> x mod 2 <> 0) 11` returns `"it's true"`.
- Accumulating terms whose sign is positive at odd indices and negative at even indices computes `1 - 2 + 3 - 4 + ...`, through the requested upper bound.
