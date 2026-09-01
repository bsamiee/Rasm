# Chapter 6 - Dataflow Programming with Functions

## 1. Dataflow programming

A dataflow program is a directed graph:
- A node is an operation that accepts inputs and produces output.
- An edge carries one node's output to another node's input.
- The graph makes both composition and data dependencies explicit.
- Nodes without a dependency between them are eligible to run in parallel; a dataflow executor can infer that independence from the graph.

For example, given:

```text
Z = A * B + C
W = Z + 4
Y = Z^2 - (3 * Z + B)
```

`W` and `Y` both depend on `Z`, but not on each other, they can be computed simultaneously after `Z` is available.

The graph form exposes small building blocks, can enable automatic parallel execution from dependency analysis, and encourages reusable libraries of components. Components can be recombined to solve problems their creators did not anticipate. This style is common in control systems and visual programming.

## 2. Pure functions as components

Pure functions behave like dataflow nodes: they map inputs to outputs, always return the same output for the same input, and produce no side effects. Functions become composable when they share an input/output representation.

Lists provide such an interface:
- `map f`: transform every element; list -> list.
- `filter p`: retain elements satisfying `p`; list -> list.
- `fold`: combine the input into one value, configured by an initial value and accumulation function.
- `zipWith f`: combine corresponding elements from two input lists into one output list.

### 2.1 Linear list pipelines

To sum the squares of even integers in `[a, b]`, separate the work into stages:

```text
enumerate [a, b] -> filter even -> map square -> fold (+) from 0
```

```ocaml
let even x = x mod 2 = 0
let square x = x * x
let rec enumerate_integers a b =
  if a > b then [] else a :: enumerate_integers (a + 1) b
let sum_even_squares a b =
  enumerate_integers a b |> List.filter even
  |> List.map square |> List.fold_left (+) 0
```

In OCaml, `x |> f` means `f x`. Piping makes textual order match left-to-right data flow.

The same downstream pipeline can consume another source. For a binary tree, only enumeration changes:

```ocaml
type 'a bin_tree =
  | Leaf
  | Node of 'a bin_tree * 'a * 'a bin_tree

let rec fold_tree f init t =
  match t with Leaf -> init
  | Node (l, x, r) -> f (fold_tree f init l) x (fold_tree f init r)

let enumerate_tree_elements t =
  fold_tree (fun l x r -> x :: l @ r) [] t

let sum_tree_even_squares t =
  enumerate_tree_elements t |> List.filter even
  |> List.map square |> List.fold_left (+) 0
```

The dataflow shape is reusable:

```text
source-specific enumeration -> filter even -> map square -> sum
```

### 2.2 Multiple input signals

`zipWith` is a two-input component. One input can be the integers `1..n`; another can be the prime numbers selected from that same interval. Zipping them formats each prime with its one-based position:

```ocaml
let count_primes n =
  let numbers = enumerate_integers 1 n in
  zipWith
    (fun i p -> "Prime at " ^ string_of_int i ^ " is " ^ string_of_int p)
    numbers (List.filter is_prime numbers)
```

For `n = 6`, the result is:

```text
["Prime at 1 is 2"; "Prime at 2 is 3"; "Prime at 3 is 5"]
```

## 3. Rule of composition

Favor small, independent programs that do one thing well, and connect them through a simple common interface. In Unix, plain-text streams rather than binary formats provide that interface, and `|` connects programs.

```sh
cat /usr/share/dict/words | head -5 | tail -1
```

The command composes three focused programs to return the fifth word. The OCaml analogue uses focused functions such as `map`, `filter`, `fold`, and `zipWith`, lists as the shared interface, and `|>` to express the connection.

## 4. Why OCaml's finite lists are insufficient

Consider finding the first prime greater than or equal to `n`:

```text
enumerate integers from n -> filter primes -> take head
```

An OCaml list cannot represent the unbounded input signal. OCaml is strict. Constructing `hd :: tl` evaluates both arguments. This definition recurses without producing a completed list and eventually overflows the stack:

```ocaml
let rec naturals_from n = n :: naturals_from (n + 1)
```

A finite workaround performs too much work:

```ocaml
let first_prime_between a b =
  enumerate_integers a b |> List.filter is_prime |> List.hd
```

It constructs the entire interval and filters every element even though only the first match is required. An imperative loop can stop at the first match, but loses the declarative composition of the pipeline. The desired design combines composable components, potentially infinite signals, and incremental demand-driven computation.

## 5. Delayed evaluation

A thunk delays an expression by wrapping it in a function that accepts unit:

```ocaml
let delayed_fib40 = fun () -> fib 40
let result = delayed_fib40 ()
```

OCaml provides `lazy` and `Lazy.force` directly:

```ocaml
let delayed_fib40 = lazy (fib 40)  (* int Lazy.t *)
let result = Lazy.force delayed_fib40
```

`lazy expression` does not evaluate `expression`. The first `Lazy.force` evaluates it and memoizes the result; later forces return the cached value. Delaying an exceptional expression also delays its exception: `lazy (1 / 0)` is a value, while forcing it raises division by zero.

## 6. An OCaml stream: a list with a delayed tail

This non-empty stream representation makes its head available now and promises its tail later:

```ocaml
type 'a stream = Cons of 'a * 'a stream Lazy.t
let stream_hd (Cons (h, _)) = h
let stream_tl (Cons (_, t)) = Lazy.force t
```

This definition has no `Nil`; it models streams that continue indefinitely. `stream_hd` observes the available element, while `stream_tl` forces exactly one delayed step.

```ocaml
let rec naturals_from n =
  Cons (n, lazy (naturals_from (n + 1)))
let naturals = naturals_from 0
```

The recursive call is under `lazy`. Constructing `naturals` produces `0` and a suspended tail rather than recursing forever.

Use a finite observation to inspect a stream:

```ocaml
let rec stream_take n s =
  if n <= 0 then []
  else stream_hd s :: stream_take (n - 1) (stream_tl s)
```

`stream_take 10 naturals` returns `[0; 1; 2; 3; 4; 5; 6; 7; 8; 9]`; converting a demanded finite prefix to a list bounds evaluation instead of attempting to realize the infinite stream.

## 7. Higher-order stream components

### 7.1 Mapping

```ocaml
let rec stream_map f s =
  Cons (f (stream_hd s), lazy (stream_map f (stream_tl s)))

let square_naturals = stream_map (fun x -> x * x) naturals
```

In this definition, the current output is computed immediately and mapping the tail is delayed. Unlike OCaml's `List.map`, the input and output may be infinite.

### 7.2 Filtering

```ocaml
let rec stream_filter p s =
  if p (stream_hd s) then
    Cons (stream_hd s, lazy (stream_filter p (stream_tl s)))
  else
    stream_filter p (stream_tl s)

let evens = stream_filter (fun x -> x mod 2 = 0) naturals
```

Filtering requests input elements until it finds one satisfying the predicate. If no future element satisfies it, the request never returns.

### 7.3 Combining and synchronizing

```ocaml
let rec stream_zipWith f s1 s2 =
  Cons (
    f (stream_hd s1) (stream_hd s2),
    lazy (stream_zipWith f (stream_tl s1) (stream_tl s2)))
```

Each output requires one element from each input, `stream_zipWith` synchronizes the streams before applying `f`. Multiplying `naturals` by itself pointwise produces square numbers.

## 8. Stream-based dataflow

Streams are the common interface connecting stream selectors and higher-order stream functions. They retain component composition while allowing infinite signals and incremental evaluation.

```ocaml
let first_prime_greater_equal n =
  naturals_from n |> stream_filter is_prime |> stream_hd
```

Evaluation proceeds only as far as needed: candidates are generated and tested one at a time; once the first prime is found, no later candidates are produced.

In a non-strict language such as Haskell, ordinary lists can fill the same role because the tail is not evaluated until demanded:

```haskell
naturals_from n = n : naturals_from (n + 1)
naturals = naturals_from 0
take 10 naturals
```

Strictness is presented as a language-design tradeoff. Non-strict lists naturally support infinite sequences. The stated tradeoff is internally uneven: strict semantics is said to make space and time harder to reason about, while non-strictness is said to produce undesirable side effects.

## 9. Applied constructions

### 9.1 Largest circle area

The shape and area definitions are:

```ocaml
type shape =
  | Circle of float
  | Rectangle of float * float

let area = function
  | Circle r -> 3.14 *. r *. r
  | Rectangle (w, h) -> w *. h
```

With `is_circle` recognizing `Circle _`, the dataflow is:

```text
shapes -> filter circles -> map area -> fold max from 0.0
```

```ocaml
let max_circle shapes =
  shapes |> List.filter is_circle |> List.map area |> List.fold_left max 0.
```

The initial value makes the empty-list result `0.0`; a largest radius of `2.0` gives area `12.56` under this `3.14` approximation.

### 9.2 Alternating merge

Emit the head of the first stream, then swap the inputs for the delayed recursive step:

```ocaml
let rec stream_merge (Cons (x, xs)) s2 =
  Cons (x, lazy (stream_merge s2 (Lazy.force xs)))
```

Merging evens with odds begins `[0; 1; 2; 3; ...]`; reversing the inputs begins `[1; 0; 3; 2; ...]`.

### 9.3 Self-referential Fibonacci stream

If `fibs = <1; 1; 2; 3; 5; ...>`, then zipping `fibs` with its tail using `(+)` produces the stream after its first two elements. Matching that observation requires seeds `1` and `1`:

```ocaml
let rec fibs = Cons (1, lazy (Cons (1, lazy (
  stream_zipWith (+) fibs (stream_tl fibs)))))
```

The challenge solution instead seeds the same construction with `1` and `2`; that version generates `<1; 2; 3; 5; ...>` and does not match the stated stream. Laziness permits either self-reference without demanding the unfinished tail during construction.

## 10. Knowledge checks

- Rule of composition: favor small independent programs and a simple shared interface such as plain text.
- Functional dataflow works because pure functions have no side effects and can connect through shared representations such as lists or streams.
- `2 |> square` is `square 2`, it evaluates to `4`.
- `[0; 1; 2] |> List.map (fun x -> x > 0) |> List.fold_left (&&) true` evaluates to `false` because `0 > 0` is false.
- Streams model infinite sequences and support incremental on-demand computation.
- `[1; 2; 3; 4] |> List.filter ((<=) 3) |> List.map (( * ) 2) |> List.fold_left (+) 0` keeps `[3; 4]`, doubles to `[6; 8]`, and evaluates to `14`; none of the listed choices matches that derivation.
- `lazy (1 / 0)` creates an `int lazy_t` value without raising; `Lazy.force` triggers the division and raises the exception.
