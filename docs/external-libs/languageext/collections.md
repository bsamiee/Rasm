# [LANGUAGEEXT_COLLECTIONS]

[IMPORTANT] Use `Seq<T>` as the default domain sequence. Convert external mutable host collections at adapter edges.

## [1][SELECTION]

| [INDEX] | [SURFACE]      | [SEMANTICS]                                           | [USE]                                     |
| :-----: | -------------- | ----------------------------------------------------- | ----------------------------------------- |
|   [1]   | `Seq<T>`       | Lazy immutable sequence with rail-friendly traversal. | Default domain sequence.                  |
|   [2]   | `Arr<T>`       | Strict immutable indexed batch.                       | Stable materialized outputs.              |
|   [3]   | `Iterable<T>`  | Trait-enabled enumerable carrier.                     | Advanced generic traversal only.          |
|   [4]   | `HashMap<K,V>` | Immutable lookup.                                     | Domain maps after key policy is explicit. |
|   [5]   | `HashSet<T>`   | Immutable set.                                        | Block graphs, format capability sets.     |

## [2][TRAVERSAL]

Use `Traverse`, `TraverseM`, `Choose`, `Fold`, `FoldWhile`, `Map`, `Bind`, and `Flatten()` to keep failure and collection shape together. Prefer one traversal that validates, projects, and accumulates over repeated filter-map passes.

| [INDEX] | [COMBINATOR]                                                              | [PREFERENCE]                                          |
| :-----: | ------------------------------------------------------------------------- | ----------------------------------------------------- |
|   [1]   | `.TraverseM(f) >> lower` / unary `+`; `.TraverseM(f).As()` for `Eff`/`IO` | Monadic batch; carrier-qualified lowering (v5)        |
|   [2]   | `.Choose(f)`                                                              | Prefer over `.Filter().Map()` when output is `Option` |
|   [3]   | `.Somes()`                                                                | Extract present options after `Choose`                |
|   [4]   | `.AddOrUpdate` / `.Find` / `.Filter`                                      | `HashMap` algebra                                     |

Full combinator inventory: `combinators.md`.

## [3][INTEROP]

| [INDEX] | [HOST]                   | [RULE]                                                                |
| :-----: | ------------------------ | --------------------------------------------------------------------- |
|   [1]   | Host arrays/lists        | Convert immediately, validate native sentinels, then use `Seq<T>`.    |
|   [2]   | Host tree structures     | Preserve tree semantics at host boundary; project values into rails.  |
|   [3]   | Numeric vectors/matrices | Keep internal to algorithm execution; project into owned result types. |
|   [4]   | BCL spans                | Use only inside measured primitive kernels or boundary adapters.      |

## [4][RULES]

- Do not hand-roll mutable accumulation for domain transforms.
- Do not expose mutable external storage as public collection identity.
- Do not convert back and forth between BCL and LanguageExt collections inside one pipeline.
- Keep hot-path span work isolated and benchmark-gated.
