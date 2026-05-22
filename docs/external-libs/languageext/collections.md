# [H1][LANGUAGEEXT_COLLECTIONS]
>**Dictum:** *Collections encode traversal semantics before storage concerns.*

<br>

[IMPORTANT] Rasm uses `Seq<T>` as the default cross-module sequence. Convert Rhino/GH arrays and host collections at the adapter edge.

---
## [1][SELECTION]
>**Dictum:** *Collection choice follows lifetime and traversal shape.*

<br>

| [INDEX] | [SURFACE] | [SEMANTICS] | [USE] |
| :-----: | --------- | ----------- | ----- |
| [1] | `Seq<T>` | Lazy immutable sequence with rail-friendly traversal. | Default domain sequence. |
| [2] | `Arr<T>` | Strict immutable indexed batch. | Stable materialized outputs. |
| [3] | `Iterable<T>` | Trait-enabled enumerable carrier. | Advanced generic traversal only. |
| [4] | `HashMap<K,V>` | Immutable lookup. | Domain maps after key policy is explicit. |

---
## [2][TRAVERSAL]
>**Dictum:** *Traverse turns many rails into one rail of many values.*

<br>

Use `Traverse`, `TraverseM`, `Choose`, `Fold`, `FoldWhile`, `Map`, `Bind`, and `Flatten()` to keep failure and collection shape together. Prefer one traversal that validates, projects, and accumulates over repeated filter-map passes.

---
## [3][INTEROP]
>**Dictum:** *Host collections convert before domain logic begins.*

<br>

| [INDEX] | [HOST] | [RULE] |
| :-----: | ------ | ------ |
| [1] | Rhino arrays/lists | Convert immediately, validate native sentinels, then use `Seq<T>`. |
| [2] | GH2 `Pear`/`Twig`/`Tree` | Preserve tree semantics at GH2 boundary; project values into rails. |
| [3] | MathNet vectors/matrices | Keep internal to algorithm execution; project into Rasm result types. |
| [4] | BCL spans | Use only inside measured primitive kernels or boundary adapters. |

---
## [4][RULES]
>**Dictum:** *Traversal code reads as algebra, not procedure.*

<br>

- Do not hand-roll mutable accumulation for domain transforms.
- Do not expose MathNet or Rhino mutable storage as public collection identity.
- Do not convert back and forth between BCL and LanguageExt collections inside one pipeline.
- Keep hot-path span work isolated and benchmark-gated.
