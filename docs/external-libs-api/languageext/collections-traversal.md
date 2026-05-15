# [H1][LANGUAGEEXT_COLLECTIONS_TRAVERSAL]
>**Dictum:** *Collections are traversal contracts, not storage decoration.*

<br>

[IMPORTANT] Rasm uses `Seq<T>` as the default cross-module sequence because it composes with LanguageExt traversal, validation, and effect APIs.

---
## [1][COLLECTION_SELECTION]
>**Dictum:** *Choose the collection by operation shape.*

<br>

| [INDEX] | [TYPE] | [USE] | [AVOID_WHEN] |
| :-----: | ------ | ----- | ------------ |
| **[1]** | `Seq<T>` | General immutable sequence, traversal, folds, GH2 flow values. | Hot inner loops need spans. |
| **[2]** | `Arr<T>` | Strict immutable indexed batch. | Lazy or streaming traversal is desired. |
| **[3]** | `Lst<T>` | List-specific structural operations. | `Seq<T>` already covers traversal. |
| **[4]** | `HashMap<K,V>` | Fast immutable keyed lookup. | Key reflection causes host issues. |
| **[5]** | `Map<K,V>` | Ordered immutable keyed lookup. | Ordering has no semantic value. |
| **[6]** | `Set<T>` | Ordered uniqueness. | Hash semantics are enough. |
| **[7]** | `HashSet<T>` | Fast uniqueness. | Stable ordering is required. |
| **[8]** | `Que<T>` | FIFO worklist. | Random access is required. |
| **[9]** | `Stck<T>` | LIFO worklist. | FIFO order is required. |
| **[10]** | `Iterable<T>` | Lazy iterable abstraction. | Prefer `Seq<T>` at module boundaries unless laziness is material. |

---
## [2][FUSION]
>**Dictum:** *One pass beats filter-map-repeat pipelines.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| **[1]** | `Choose` | Fuse filter and projection into one traversal. |
| **[2]** | `Map` | Pure projection preserving cardinality. |
| **[3]** | `Bind` | Projection that emits zero, one, or many values. |
| **[4]** | `Fold` | Total accumulation. |
| **[5]** | `FoldWhile` | Early-exit accumulation. |
| **[6]** | `Exists` and `ForAll` | Predicate reduction. |
| **[7]** | `Head` | Optional first value. |

[CRITICAL] Do not use mutable accumulators in domain transforms. Fold into immutable state and project once.

---
## [3][TRAVERSAL]
>**Dictum:** *Traverse turns many rails into one rail of many values.*

<br>

Use traversal when each item can fail or produce an effect:
- `Seq<T>.Traverse(...)` for `Validation<Error,T>` accumulation.
- `Seq<T>.TraverseM(...)` for monadic sequencing.
- `.As()` when the traversal returns an abstract `K<F,A>` and the concrete rail is required.
- `Bind(static chunk => chunk)` when a traversed sequence returns nested sequences that must flatten.

Rasm pattern:
- Validate each geometry into `Validation<Error,T>`.
- Traverse native projections into `Fin<Seq<T>>`.
- Run effectful per-shape projections into `Eff<Env, Seq<T>>`.
- Flatten `Seq<Seq<T>>` only after the rail succeeds.

---
## [4][ACCUMULATION]
>**Dictum:** *Prepend then reverse when fold order matters.*

<br>

| [INDEX] | [PATTERN] | [WHY] |
| :-----: | --------- | ----- |
| **[1]** | `.Cons(value)` in folds | O(1) prepend. |
| **[2]** | `.Rev()` at projection boundary | Restores insertion order. |
| **[3]** | `.Add(value)` outside hot folds | Clear append when cost is acceptable. |
| **[4]** | `Seq<T>()` as identity | Empty immutable sequence. |
| **[5]** | `toSeq(source)` at boundary | Converts external enumerables into repo vocabulary. |
| **[6]** | `HeadOrNone`, `HeadOrInvalid` | Safe head access where empty input is expected. |

---
## [5][INTEROP]
>**Dictum:** *External collections convert at the adapter edge.*

<br>

| [INDEX] | [SOURCE] | [ADAPTER_POLICY] |
| :-----: | -------- | ---------------- |
| **[1]** | `IEnumerable<T>` | Convert with `toSeq` before domain traversal. |
| **[2]** | Arrays | Convert with `toSeq` or `AsIterable().ToSeq()` when rail APIs are needed. |
| **[3]** | GH2 `Tree<T>` | Convert through `Bridge.Read<T>` into `Seq<Flow<T>>`. |
| **[4]** | RhinoCommon result arrays | Convert immediately and validate with `Fin<T>` or `Validation<Error,T>`. |
| **[5]** | Hot-path spans | Keep span-local; lift to `Fin<T>` at public surface. |

---
## [6][RULES]
>**Dictum:** *Traversal code should read as algebra, not procedure.*

<br>

- Use `Seq<T>` for public analysis and bridge flows.
- Use `Choose` over `Filter` plus `Map`.
- Use `Traverse` or `TraverseM` instead of manual loops over fallible operations.
- Use `HashMap<K,V>` and `Map<K,V>` only when keyed lookup is the real domain model.
- Use BCL frozen dictionaries only for boundary hazards already proven by host behavior.
- Use `Atom<T>` or STM refs for shared mutable state; do not use atomic collection wrappers as ordinary domain collections.
