# Common Computation Patterns

`map`, `filter`, `fold`, and `zip` capture recurring computation shapes. Each replaces repeated recursion with a higher-order operation whose arguments express the part that varies.

## `map`: transform values while preserving context

### Lists

Functions that square or cube every list element have identical recursion; only the element transformation differs. Factoring that transformation into `f` gives:

```ocaml
let rec map f l =
  match l with
  | [] -> []
  | hd :: tl -> f hd :: map f tl
(* map : ('a -> 'b) -> 'a list -> 'b list *)
```

`map f l` replaces every element `e` with `f e`, raising the level of reasoning from recursive steps to transformation of a sequence as a unit.

```ocaml
let square_list = List.map (fun x -> x * x)
let cube_list = List.map (fun x -> x * x * x)
square_list [1; 2; 3]  (* [1; 4; 9] *)
cube_list [1; 2; 3]    (* [1; 8; 27] *)
```

### Binary trees

```ocaml
type 'a bin_tree = Leaf | Node of 'a bin_tree * 'a * 'a bin_tree
let rec map_tree f t =
  match t with
  | Leaf -> Leaf
  | Node (l, v, r) -> Node (map_tree f l, f v, map_tree f r)
(* map_tree : ('a -> 'b) -> 'a bin_tree -> 'b bin_tree *)
```

Only node values change. `Leaf` remains `Leaf`, and every `Node` retains its position and children. The tree's shape is unchanged.

### Optional values

```ocaml
type 'a option = None | Some of 'a
let map_option f o =
  match o with None -> None | Some x -> Some (f x)
(* map_option : ('a -> 'b) -> 'a option -> 'b option *)
```

`None` stays empty; `Some x` becomes `Some (f x)`. This transforms a successful value while propagating the empty case.

```ocaml
let rec longest_string l =
  match l with
  | [] -> None
  | hd :: tl ->
      match longest_string tl with
      | None -> Some hd
      | Some x -> if String.length hd > String.length x then Some hd else Some x
let max_length l = Option.map String.length (longest_string l)
max_length []                    (* None *)
max_length ["a"; "abc"; "ab"]    (* Some 3 *)
```

There is no longest string for `[]`, absence remains `None`; an existing longest string is mapped to its length.

### Containers, domains, and lifting

The list, tree, and option signatures share one form:

```text
map : ('a -> 'b) -> 'a context -> 'b context
```

Contexts may be containers or domains such as asynchronous computation:

```text
map_future : ('a -> 'b) -> 'a future -> 'b future
```

The computed value changes without changing the represented asynchronous computation. `map` for a domain or context should transform its value without altering that context.

With the curried signatures used here, partial application makes `map` a lifting function. Given `square : int -> int`, context-specific maps lift it to `int list -> int list`, `int bin_tree -> int bin_tree`, `int option -> int option`, or `int future -> int future`.

Haskell's `Functor` captures types that support this generalized mapping operation:

```haskell
class Functor f where
  fmap :: (a -> b) -> f a -> f b
  (<$) :: a -> f b -> f a
```

`fmap` transforms every contextual value; `<$` preserves the context while replacing every value with a constant.

## `filter`: retain elements satisfying a predicate

Predicates return `bool`. `filter` abstracts recursion that decides whether each element belongs in the result:

```ocaml
let rec filter p l =
  match l with
  | [] -> []
  | hd :: tl -> if p hd then hd :: filter p tl else filter p tl
(* filter : ('a -> bool) -> 'a list -> 'a list *)
```

This list definition keeps input order and contains only elements satisfying `p`.

```ocaml
let even x = x mod 2 = 0
let positive x = x > 0
List.filter even [1; 2; 3; 4]              (* [2; 4] *)
List.filter positive [-1; 0; 1; 2; -3; 4] (* [1; 2; 4] *)
```

Filtering applies naturally to collections and can also be defined for sets. Existing predicates compose with negation:

```ocaml
let compose f g x = f (g x)
let odds = List.filter (compose not even)
let negatives_or_zero = List.filter (compose not positive)
odds [1; 2; 3; 4]                       (* [1; 3] *)
negatives_or_zero [-1; 0; 1; 2; -3; 4] (* [-1; 0; -3] *)
```

## `fold`: replace constructors to aggregate a structure

### Right fold over lists

List sum and product differ only in the result for `[]` and the binary operation replacing `::`:

```ocaml
let rec fold_right f init l =
  match l with
  | [] -> init
  | hd :: tl -> f hd (fold_right f init tl)
(* fold_right : ('a -> 'b -> 'b) -> 'b -> 'a list -> 'b *)
```

`fold_right f init` replaces `[]` with `init` and every `::` with `f`:

```text
[x1; x2; x3] = x1 :: (x2 :: (x3 :: []))
fold_right f init [x1; x2; x3] = f x1 (f x2 (f x3 init))
```

The grouping is right-associative; `init` is the result for the empty list.

```ocaml
let sum_list = fold_right (+) 0
let prod_list = fold_right ( * ) 1
let any l = List.fold_right (||) l false
let all l = List.fold_right (&&) l true
let length l = List.fold_right (fun _ len -> len + 1) l 0
let map f l = List.fold_right (fun x acc -> f x :: acc) l []
let filter p l = List.fold_right (fun x acc -> if p x then x :: acc else acc) l []
```

In many mainstream languages, `fold` is called `reduce`: a combining function and initial value accumulate all elements into one result. The `map`, `fold`, and `reduce` abstractions inspired the MapReduce approach to large-data processing, with Apache Hadoop as an implementation.

OCaml's library order is `List.fold_right : ('a -> 'b -> 'b) -> 'a list -> 'b -> 'b`; unlike the local definition, the list precedes the initial value, which limits convenient partial application with other functions. Haskell's `foldr` uses the local order.

### Left fold

```ocaml
let rec fold_left f acc l =
  match l with
  | [] -> acc
  | hd :: tl -> fold_left f (f acc hd) tl
(* fold_left : ('a -> 'b -> 'a) -> 'a -> 'b list -> 'a *)
```

```text
fold_left f init [x1; x2; x3] = f (f (f init x1) x2) x3
fold_right (-) 0 [1; 2; 3] = 1 - (2 - (3 - 0)) = -2
fold_left  (-) 0 [1; 2; 3] = ((0 - 1) - 2) - 3 = -6
```

This `fold_left` definition is tail-recursive because its recursive call is its final operation.

### Binary-tree fold

Replace `Leaf` with `init` and `Node` with a function combining the folded left subtree, node value, and folded right subtree:

```ocaml
let rec fold_tree f init t =
  match t with
  | Leaf -> init
  | Node (l, x, r) -> f (fold_tree f init l) x (fold_tree f init r)
(* fold_tree : ('a -> 'b -> 'a -> 'a) -> 'a -> 'b bin_tree -> 'a *)
let sum_tree = fold_tree (fun suml x sumr -> suml + x + sumr) 0
let size t = fold_tree (fun sizel _ sizer -> sizel + 1 + sizer) 0 t
let tree_elements = fold_tree (fun l x r -> [x] @ l @ r) []
```

For `Node (Node (Leaf, 2, Leaf), 1, Node (Node (Leaf, 4, Leaf), 3, Leaf))`, these return `10`, `4`, and `[1; 2; 3; 4]`.

### Option and natural-number folds

`fold_option` replaces `None` with `init` and `Some x` with `f x`:

```ocaml
let fold_option f init o = match o with None -> init | Some x -> f x
(* fold_option : ('a -> 'b) -> 'b -> 'a option -> 'b *)
fold_option (fun x -> x * x) 42 None     (* 42 *)
fold_option (fun x -> x * x) 42 (Some 3) (* 9 *)
```

For naturals, `Zero` is replaced by `init`, and `f` is applied once per `Succ`:

```ocaml
type nat = Zero | Succ of nat
let rec fold_nat f init n =
  match n with Zero -> init | Succ m -> f (fold_nat f init m)
(* fold_nat : ('a -> 'a) -> 'a -> nat -> 'a *)
let nat_to_int = fold_nat ((+) 1) 0
let nat_to_string = fold_nat (fun x -> "Succ (" ^ x ^ ")") "Zero"
nat_to_int (Succ (Succ (Succ (Succ Zero))))       (* 4 *)
nat_to_string (Succ (Succ (Succ (Succ Zero))))    (* "Succ (Succ (Succ (Succ (Zero))))" *)
```

Designing a fold is especially valuable for recursive data: it exposes the general aggregation pattern once and lets many operations reuse it.

## `zip` and `zipWith`: combine corresponding elements

```ocaml
let rec zip l1 l2 =
  match l1, l2 with
  | [], _ | _, [] -> []
  | x :: xs, y :: ys -> (x, y) :: zip xs ys
(* zip : 'a list -> 'b list -> ('a * 'b) list *)
```

`zip [1; 2; 3] [4; 5; 6]` produces `[(1, 4); (2, 5); (3, 6)]`. This definition stops when either list ends, ignoring excess elements of the longer list. Pairing x- and y-coordinate lists forms 2D points.

`zipWith` generalizes pairing by accepting a binary function:

```ocaml
let rec zipWith f l1 l2 =
  match l1, l2 with
  | [], _ | _, [] -> []
  | x :: xs, y :: ys -> f x y :: zipWith f xs ys
(* zipWith : ('a -> 'b -> 'c) -> 'a list -> 'b list -> 'c list *)
```

`zip` is `zipWith (fun x y -> (x, y))`.

```ocaml
zipWith (+) [1; 2; 3] [4; 5; 6] (* [5; 7; 9] *)
let square_list l = zipWith ( * ) l l
let diff l = zipWith (-) (List.tl l) l
diff [1; 9; 100; 37] (* [8; 91; -63] *)
let total_abs_diff l = List.fold_right (+) (List.map abs (diff l)) 0
total_abs_diff [1; 9; 100; 37] (* 162 *)
```

Adjacent differences align the tail with the original list, compute `next - current`, map absolute value over the differences, then sum them. Because `List.tl []` raises, `diff` and `total_abs_diff` as written require a nonempty input; a singleton yields an empty difference list and total `0`.

## Challenge solutions

### Map over `either`

By convention, `Right` carries a valid value and `Left` an error. Mapping transforms only `Right`, paralleling `Some`/`None`:

```ocaml
type ('a, 'b) either = Left of 'a | Right of 'b
let map_either f e =
  match e with Left x -> Left x | Right y -> Right (f y)
(* map_either : ('a -> 'b) -> ('c, 'a) either -> ('c, 'b) either *)
```

### Tree elements through `fold_tree`

```ocaml
let tree_to_list t = fold_tree (fun l x r -> [x] @ l @ r) [] t
```

The empty tree becomes `[]`; each node places its value before the accumulated left and right lists.

### Check nondecreasing order with `zipWith`

```ocaml
let all l = List.fold_right (&&) l true
let is_ascending_sorted l =
  if l = [] then true else all (zipWith (<=) l (List.tl l))
```

Each element is compared with its successor. Empty and singleton lists are sorted; adjacent equal values are accepted. The `fold_left` challenge is the tail-recursive definition given above.

## Quiz conclusions

1. `List.map ((+) 1) [1; 2; 3]` is `[2; 3; 4]`
2. `zipWith` has type `('a -> 'b -> 'c) -> 'a list -> 'b list -> 'c list`
3. `List.map fst [(0., 0.); (1., 2.); (1.5, 3.5)]` is `[0.; 1.; 1.5]`
4. Right folds that prepend `x` when `p x` is true implement `filter`
5. Folding `[1; 2]` into a parenthesized string from `"0"` yields `"(1+(2+0))"`, exposing right association
6. Comparing a list with its tail using `zipWith (=)` and `all` tests whether all elements are equal: true for `[]`, singletons, and repeated equal values, false for `[1; 2]`
7. `fold_nat ((^) "-") ""` returns one hyphen per `Succ`: `""`, `"-"`, and `"--"` for zero, one, and two
8. Haskell's `Functor` captures a type that can be mapped over; `fmap` is its generalized map operation
