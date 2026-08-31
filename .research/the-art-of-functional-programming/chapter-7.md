# Applying Functional Programming in Practice

Functional programming applies directly to collection-heavy mobile, web, and backend systems. Many collection-processing problems reduce to three operations:
- `map`: transform each element.
- `filter`: retain elements satisfying a predicate.
- `fold`: accumulate the elements into one result.

Specialized collection functions can often be built from these operations, then composed as a vocabulary for dataflow programming.

## Collection processing

### Product model and core pipeline

```ocaml
type product_type = ELECTRONIC | BOOK | COSMETIC
type product = { name : string; product_type : product_type; price : float }

let products = [
  { name = "iPad"; product_type = ELECTRONIC; price = 800.0 };
  { name = "Pride and Prejudice"; product_type = BOOK; price = 10.0 };
  { name = "Mac Pro"; product_type = ELECTRONIC; price = 2000.0 };
  { name = "Smart TV"; product_type = ELECTRONIC; price = 500.0 }
]
```

The total price of electronic products is a three-stage flow:

1. Filter the products to electronic items.
2. Map each remaining product to its price.
3. Fold the prices by floating-point addition from `0.0`.

```ocaml
products
|> List.filter (fun product -> product.product_type = ELECTRONIC)
|> List.map (fun product -> product.price)
|> List.fold_left (+.) 0.
(* 3300. *)
```

The pipe operator makes the collection and each successive transformation read from left to right.

### Retrieve a single element

`find` returns the first element of an ordered collection that satisfies a predicate:

```ocaml
let find p l = List.filter p l |> List.hd

List.find (fun product -> product.price <= 1000.0) products
(* iPad, ELECTRONIC, 800. *)
```

The filter-and-head formulation and `List.find` raise an exception when no element matches. “First” depends on order; for an unordered collection such as a set, the corresponding operation can only promise some matching element.

`contains` is the corresponding reusable existence test:

```ocaml
let contains p l = List.filter p l <> []

contains (fun product -> product.price <= 1000.0) products
(* true *)
```

### Order elements

`List.sort` takes a comparison function and returns a sorted list. Sorting products from low to high price uses `Stdlib.compare` on the price fields:

```ocaml
List.sort
  (fun product1 product2 -> Stdlib.compare product1.price product2.price)
  products
(* Pride and Prejudice 10.; Smart TV 500.; iPad 800.; Mac Pro 2000. *)
```

Reversal can be expressed as a right fold that appends each current element to the accumulator:

```ocaml
let reverse l = List.fold_right (fun x acc -> acc @ [x]) l []
```

Applied to `products`, this produces Smart TV, Mac Pro, Pride and Prejudice, then iPad.

### Retrieve a prefix

`take n l` returns the first `n` elements. A non-positive `n` or an empty list yields `[]`; when `n` exceeds the list length, all elements are returned.

```ocaml
let rec take n l =
  if n <= 0 then []
  else
    match l with
    | [] -> []
    | hd :: tl -> hd :: take (n - 1) tl
```

Sorting by ascending price and taking two yields Pride and Prejudice and Smart TV.

`take_while` generalizes prefix selection. It retains consecutive elements while the predicate holds and stops immediately before the first failure:

```ocaml
let rec take_while p l =
  match l with
  | [] -> []
  | hd :: tl -> if p hd then hd :: take_while p tl else []
```

Sorting by price and then taking while `product.price < 1000.0` yields Pride and Prejudice, Smart TV, and iPad.

### A collection DSL

In the chapter's OCaml construction, a list supplies the shared representation; functions and pattern matching supply the mechanisms; operations such as `map`, `filter`, `fold`, `find`, existence testing, `sort`, `reverse`, `take`, and `take_while` supply the collection vocabulary.

This forms a domain-specific language for collections:
- A collection is handled as one unit rather than by naming and manipulating every element.
- Every operation accepts or returns the shared collection form, so operations compose into dataflow pipelines.
- Reusing established operations makes collection logic concise and readable.
- The approach transfers to collection APIs in languages such as Swift, Kotlin, JavaScript, and Java.

## JSON processing

JSON is hierarchical data that can originate in local application storage or a network service such as a REST API. Algebraic datatypes represent its alternatives directly, while recursive and higher-order functions provide reusable transformations.

### Example structure

```json
{
  "title": "Godfather",
  "genre": ["crime", "drama"],
  "year": 1972,
  "actors": [
    { "actor": "Marlon Brando", "character": "Vito Corleone", "is_major_character": true },
    { "actor": "Al Pacino", "character": "Michael Corleone", "is_major_character": true },
    { "actor": "Lenny Montana", "character": "Luca Brasi", "is_major_character": false }
  ],
  "is_on_netflix": true
}
```

### Algebraic representation

In the chapter's algebraic representation, the primitive cases are null, string, integer, float, and boolean. An array contains JSON values, which may have different cases. An object contains string keys paired with arbitrary JSON values.

```ocaml
type json =
  | Null | String of string | Int of int | Float of float | Bool of bool
  | Array of json list
  | Object of (string * json) list
```

The recursive occurrences in `Array` and `Object` express arbitrary nesting. In this OCaml representation, lists hold both ordered array elements and object key-value pairs.

The example movie is represented directly with these constructors:

```ocaml
let movie =
  Object [
    ("title", String "Godfather");
    ("genre", Array [String "crime"; String "drama"]);
    ("year", Int 1972);
    ("actors", Array [
      Object [
        ("actor", String "Marlon Brando");
        ("character", String "Vito Corleone");
        ("is_major_character", Bool true)];
      Object [
        ("actor", String "Al Pacino");
        ("character", String "Michael Corleone");
        ("is_major_character", Bool true)];
      Object [
        ("actor", String "Lenny Montana");
        ("character", String "Luca Brasi");
        ("is_major_character", Bool false)]]);
    ("is_on_netflix", Bool true)]
```

### Convert JSON to text

The provided OCaml conversion follows the datatype one constructor at a time:
- `Null` becomes an empty string in this implementation.
- A `String` value is wrapped in quotation marks.
- Integers, floats, and booleans use their standard string conversions.
- An array recursively converts its elements, joins them with commas, and wraps them in brackets.
- An object converts each key-value pair as a quoted key, a colon, and a recursively converted value; pairs are comma-separated and wrapped in braces.

```ocaml
let rec json_to_string js =
  match js with
  | Null -> ""
  | String s -> "\"" ^ s ^ "\""
  | Int i -> string_of_int i
  | Float f -> string_of_float f
  | Bool b -> string_of_bool b
  | Array l ->
      let ss = List.map json_to_string l in
      "[" ^ String.concat "," ss ^ "]"
  | Object records ->
      let pair (key, value) =
        "\"" ^ key ^ "\":" ^ json_to_string value in
      let ss = List.map pair records in
      "{" ^ String.concat "," ss ^ "}"
```

The result is compact: it does not add line breaks or indentation. Formatting can be applied afterward by a JSON beautifier.

### Extract an object member

In the provided OCaml function, `member : string -> json -> json` looks up a field only when its input is an `Object`. Every non-object case returns `Null`. For an object, `List.find` selects the first pair whose key equals the requested field, and `snd` returns its value. If an object lacks the field, `List.find` raises an exception rather than returning `Null`.

```ocaml
let member field json =
  match json with
  | Object records ->
      let record = List.find (fun record -> fst record = field) records in
      snd record
  | Null | String _ | Int _ | Float _ | Bool _ | Array _ -> Null

movie |> member "actors" |> json_to_string
```

Here, `movie` denotes the example object represented with the `json` constructors. The pipeline extracts its actors array and converts it to text.

### Higher-order functions on JSON arrays

For an algebraic datatype representing a hierarchical structure, meaningful higher-order functions provide general computation patterns over its collection cases.

In the provided OCaml operations, array mapping preserves the `Array` constructor while applying a transformation to every contained JSON value, and array filtering preserves only values satisfying a predicate. Both reject a non-array input with an exception; the scalar extractors below likewise reject a constructor other than the one they expect.

```ocaml
let json_array_map f json =
  match json with
  | Array l -> Array (List.map f l)
  | _ -> failwith "Not a JSON array"

let json_array_filter p json =
  match json with
  | Array l -> Array (List.filter p l)
  | _ -> failwith "Not a JSON array"

let to_string json =
  match json with
  | String s -> s
  | _ -> failwith "Not a JSON string"

let to_bool json =
  match json with
  | Bool b -> b
  | _ -> failwith "Not a JSON boolean"
```

The actor transformation extracts `actor`, `character`, and `is_major_character`; extracts their string and Boolean values; chooses "major character" or "supporting character"; and returns a one-field object:

```ocaml
let to_compact_actor obj =
  let actor = member "actor" obj |> to_string
  and character = member "character" obj |> to_string
  and is_major = member "is_major_character" obj |> to_bool in
  let character_type =
    if is_major then "major character" else "supporting character"
  in
  Object [
    ("actor", String
      (actor ^ " plays " ^ character ^ " as a " ^ character_type))
  ]

movie
|> member "actors"
|> json_array_map to_compact_actor
|> json_to_string
```

The three results describe Marlon Brando as Vito Corleone and Al Pacino as Michael Corleone, both major characters, and Lenny Montana as Luca Brasi, a supporting character.

Filtering the actors array by `member "is_major_character" obj |> to_bool` retains only the Marlon Brando and Al Pacino objects.

### Higher-order functions on JSON objects

In the chapter's representation, an object is a collection of key-value pairs. The provided OCaml object mapping transforms every pair, and object filtering retains the pairs satisfying a predicate. Both preserve the `Object` constructor and reject non-object inputs.

```ocaml
let json_object_map f json =
  match json with
  | Object records -> Object (List.map f records)
  | _ -> failwith "Not a JSON object"

let json_object_filter p json =
  match json with
  | Object records -> Object (List.filter p records)
  | _ -> failwith "Not a JSON object"
```

To compact only the actors field while preserving all other fields:

```ocaml
json_object_map
  (fun (key, value) ->
    let mapped_value =
      if key = "actors" then json_array_map to_compact_actor value
      else value
    in
    (key, mapped_value))
  movie
```

To remove the Netflix field while preserving all remaining fields:

```ocaml
movie
|> json_object_filter (fun (key, value) -> key <> "is_on_netflix")
|> json_to_string
```

Together, the algebraic representation, constructor-specific conversions, field access, and array/object map and filter operations form a JSON DSL. It represents and manipulates a hierarchical format at a high abstraction level while retaining explicit control over each structural case.
