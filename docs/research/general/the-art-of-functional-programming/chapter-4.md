# Compound Data Types

Compound data groups multiple data objects into one structure so related values can be handled as a single concept.

## Tuples: fixed-size products

A tuple groups a fixed number of ordered values. Its elements may have different types, so it suits concepts whose number of components is known in advance.

```ocaml
(42, "Hi FP")           (* int * string *)
("Hi FP", 42)           (* string * int; a different type and value *)
let p = (2., 1.)         (* float * float *)
(1, (2, "FP"))          (* int * (int * string) *)
```

In OCaml, the pair type whose components have types `t1` and `t2` is `t1 * t2`, read "t1 cross t2." Pairs naturally represent concepts with exactly two parts, such as 2D points, rational numbers, and complex numbers. Tuples can nest. Scheme uses nested pairs to represent lists; OCaml and Haskell define lists with algebraic data types.

Tuples also let a function return several related results as one value:

```ocaml
let div_mod x y = (x / y, x mod y)
div_mod 5 2                 (* (2, 1) *)
```

### Deconstructing tuples with pattern matching

Pattern matching decomposes compound data and binds its components:

```ocaml
let translate_point p dx dy = match p with
  | (x, y) -> (x +. dx, y +. dy)
```

OCaml can match several values together, avoiding nested matches:

```ocaml
let distance_point p1 p2 = match p1, p2 with
  | (x1, y1), (x2, y2) ->
      sqrt ((x1 -. x2) ** 2. +. (y1 -. y2) ** 2.)
```

In OCaml, patterns can appear directly in function parameters:

```ocaml
let distance_point (x1, y1) (x2, y2) =
  sqrt ((x1 -. x2) ** 2. +. (y1 -. y2) ** 2.)

let translate_point (x, y) dx dy = (x +. dx, y +. dy)
```

## Immutability

Once an immutable value is constructed, it cannot be changed. Translating a point therefore constructs a new pair rather than updating the old one.

Immutability makes a value's contents stable over time, so understanding it does not require tracing a history of updates. It also provides thread safety. The resulting functional style transforms input data into new output data. In the chapter's functional list-reversal example, this makes the code resemble a mathematical function: it returns a new value and gives the same output for the same input.

## Lists: ordered homogeneous sequences

An OCaml list holds an ordered sequence of values of one element type. It is built by two constructors:

- `[]` (nil) constructs the empty list.
- `::` (cons) constructs a non-empty list from a head element and a tail list.

```ocaml
2 :: []
1 :: (2 :: [])
[1; 2; 3]                  (* sugar for 1 :: (2 :: (3 :: [])) *)
```

Because each cons cell holds a head and a link to its tail, an OCaml list is singly linked; `[]` marks its end. Consing is also the natural way to enumerate a range:

```ocaml
let rec enumerate_integers a b =
  if a > b then [] else a :: enumerate_integers (a + 1) b
```

### List patterns

Every OCaml list has exactly one of two shapes:

```ocaml
match l with
| [] ->                   (* empty *)
| hd :: tl ->             (* head plus remaining list *)
```

`_` ignores a component. Partial head and tail operations must handle the fact that an empty list has neither:

```ocaml
let hd l = match l with
  | [] -> failwith "Empty list"
  | x :: _ -> x
let tl l = match l with
  | [] -> failwith "Empty list"
  | _ :: xs -> xs
```

OCaml provides these as `List.hd` and `List.tl`.

### Recursive list operations

Access beyond the head repeatedly descends through tails. With zero-based indexing:

```ocaml
let rec nth l n = if n <= 0 then List.hd l else nth (List.tl l) (n - 1)
```

For valid nonnegative indexes, `List.nth` provides the corresponding built-in operation. List recursion commonly uses `[]` as its base case:

```ocaml
let rec length l = match l with
  | [] -> 0
  | _ :: tl -> 1 + length tl
let rec append l1 l2 = match l1 with
  | [] -> l2
  | x :: xs -> x :: append xs l2
```

`List.length` provides length, and OCaml's `@` operator behaves like `append`.

### Immutable lists versus mutable arrays

An existing OCaml list cannot be modified. Replacing its first element means constructing a new list that reuses its tail:

```ocaml
let l = [1; 2; 3]
let changed = 4 :: List.tl l       (* [4; 2; 3]; l remains unchanged *)
```

A mutable array permits indexed access and in-place updates. Aliasing makes such mutation less transparent: if `b` refers to array `a`, then updating `b[0]` also changes what is observed through `a`. Immutable lists avoid this behavior because their elements never change after construction.

The contrast is visible in reversal. Functional code constructs a new reversed list; imperative array code swaps elements in place.

```ocaml
let rec reverse l = match l with
  | [] -> l
  | hd :: tl -> reverse tl @ [hd]
```

### Lists as generic containers

OCaml's list type is `'a list`, where `'a` is a type variable. Examples include `int list`, `string list`, `(float * float) list`, lists of functions, and lists of lists.

Generic functions operate independently of the element type; defining such type-independent containers and operations is generic programming:

```ocaml
length : 'a list -> int
(@)    : 'a list -> 'a list -> 'a list
```

## Algebraic data types

Compound types arise from two forms:
- **Combination (product):** a value contains all listed components. `int * string` contains every integer-string pair.
- **Alternation (sum):** a value is one of several alternatives. A list is either `[]` or a head-tail pair.

An algebraic data type combines sums and products. Each constructor may carry zero, one, or several values.

### Modeling shapes

```ocaml
type shape =
  | Circle of float
  | Rectangle of float * float
  | ComplexShape of shape list
```

`Circle` carries a radius, `Rectangle` carries width and height, and `ComplexShape` carries a list of shapes. The self-reference makes the type recursive.

```ocaml
let rec area s = match s with
  | Circle r -> 3.14 *. r *. r
  | Rectangle (w, h) -> w *. h
  | ComplexShape shapes -> match shapes with
      | [] -> 0.
      | hd :: tl -> area hd +. area (ComplexShape tl)
```

For this calculation, child shapes are assumed not to overlap, so the area of a complex shape is the sum of its children's areas.

### Parameterized and recursive types

A parameterized binary tree can contain values of any one type:

```ocaml
type 'a bin_tree =
  | Leaf
  | Node of 'a bin_tree * 'a * 'a bin_tree
```

`Leaf` carries no data. `Node` carries a left tree, a value, and a right tree.

```ocaml
let rec size t = match t with
  | Leaf -> 0
  | Node (l, _, r) -> size l + 1 + size r
let rec sum_tree t = match t with
  | Leaf -> 0
  | Node (l, x, r) -> sum_tree l + x + sum_tree r
```

`size` is generic because it ignores stored values; `sum_tree` is specialized to integer trees.

An algebraic data type may have several type parameters:

```ocaml
type ('a, 'b) either = Left of 'a | Right of 'b
```

OCaml type names are lowercase, hence `either`; Haskell names the corresponding type `Either`. By convention, `Right` carries a successful value and `Left` carries error information:

```ocaml
let safe_div a b =
  if b <> 0 then Right (a / b) else Left "Division by zero"
```

### Algebraic definitions of built-in types

Several fundamental types have the same form:

```ocaml
type bool = True | False
type 'a mylist = Nil | Cons of 'a * 'a mylist
type 'a option = None | Some of 'a
```

OCaml's actual boolean values are written `true` and `false`; its list constructors are the special forms `[]` and `::`.

`option` represents either absence or a present value. It gives a meaningful result for partial operations such as finding the maximum of an empty list:

```ocaml
let rec list_max l = match l with
  | [] -> None
  | hd :: tl -> match list_max tl with
      | None -> Some hd
      | Some m -> Some (max hd m)
```

`either` can attach information to both alternatives; `option` carries no data in the `None` case.

## Algebraic data types versus classes

Classes with inheritance and algebraic data types favor opposite extensions:

| Change                    | Classes and inheritance                                  | Algebraic data types and pattern matching                |
| ------------------------- | -------------------------------------------------------- | -------------------------------------------------------- |
| Add a data representation | Easy: add a subclass                                     | Difficult: every operation must handle a new constructor |
| Add an operation          | Difficult: change the base class and existing subclasses | Easy: add a new pattern-matching function                |

For shapes, a `Triangle` subclass can be added without changing `Circle`, `Rectangle`, or `ComplexShape`. But adding `perimeter` requires declarations and implementations throughout the class hierarchy. With an algebraic `shape`, `perimeter : shape -> float` can be added independently: a circle uses `2. *. Float.pi *. r`, a rectangle uses `2. *. (w +. h)`, and a complex shape sums child perimeters under the same no-overlap assumption. Adding a `Triangle` constructor instead forces `area`, `perimeter`, and every other operation to gain a new case.

The expression problem asks for an abstraction that makes both new representations and new operations easy to add while retaining static type safety. It has many proposed solutions but no definitive one. The practical rule is to prefer classes when representations will grow, and algebraic data types when operations will grow.

## Programming challenge solutions

The required interfaces are `longest_string : string list -> string option`, `concat : string -> string list -> string`, `height : 'a bin_tree -> int`, `pred : nat -> nat option`, and `add : nat -> nat -> nat`.

```ocaml
let rec longest_string l = match l with
  | [] -> None
  | hd :: tl -> match longest_string tl with
      | None -> Some hd
      | Some x ->
          if String.length hd > String.length x then Some hd else Some x
let rec concat separator l = match l with
  | [] -> ""
  | [hd] -> hd
  | hd :: tl -> hd ^ separator ^ concat separator tl
let rec height t = match t with
  | Leaf -> 0
  | Node (l, _, r) -> 1 + max (height l) (height r)
type nat = Zero | Succ of nat
let pred n = match n with
  | Zero -> None
  | Succ x -> Some x
let rec add n m = match m with
  | Zero -> n
  | Succ x -> add (Succ n) x
```

Key cases:
- `longest_string [] = None`; ties keep the longest string found in the tail.
- `concat "," [] = ""`, `concat "," ["a"] = "a"`, and `concat "," ["a"; "b"] = "a,b"`.
- Tree height is `0` for `Leaf`; a `Node` adds one to the larger subtree height.
- `pred Zero = None`; `pred (Succ x) = Some x`.
- Natural-number addition moves one `Succ` at a time from the second argument to the first until the second reaches `Zero`.

## Quiz conclusions

1. `string * int * bool` has infinitely many values because its string and integer components range over infinitely many possibilities.
2. `type mytype = BoolVal of bool | Constant` has exactly three values: `BoolVal true`, `BoolVal false`, and `Constant`.
3. A safe head returns `None` for `[]` and `Some hd` for a non-empty list instead of throwing on the empty case.
4. `l[0] = "John"` is not a valid OCaml list update. Construct a new list, such as `"John" :: List.tl l`.
5. Omitting `ComplexShape` from a match over `shape` is not a syntax error; it is a non-exhaustive match that the compiler warns may fail for that constructor.
6. A list-to-string function that surrounds a recursively joined body with brackets produces `"[]"`, `"[1]"`, and `"[1; 2]"` for `[]`, `[1]`, and `[1; 2]`.
7. `Option.map String.length (List.nth_opt l n)` returns `None` when the indexed string is absent and `Some length` when present: `None`, `Some 5`, and `Some 3` for the tested inputs.
8. For `f Zero = ""`, `f (Succ Zero) = "/"`, and `f (Succ n) = "/ " ^ f n`, the tested results are `""`, `"/"`, and `"/ /"`.
