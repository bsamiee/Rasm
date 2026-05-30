# [H1][LANGUAGEEXT_COMBINATORS]
>**Dictum:** *Combinators preserve rail shape; collapse only at host boundaries.*

<br>

[IMPORTANT] Advanced combinators only. Rasm boundary extensions (`Op.Need`, `Op.Catch`, `AcceptValidated`) live in `rasm.md` §3 and `libs/csharp/Rasm/Domain/Validation.cs`.

---
## [1][RAIL_TRANSFORMS]
>**Dictum:** *Transform success and failure without leaving the carrier.*

<br>

| [INDEX] | [COMBINATOR] | [RASM_USE] |
| :-----: | ------------ | ---------- |
| [1] | `.MapFail(f)` | heavy at boundaries |
| [2] | `.BindFail(f)` | moderate |
| [3] | `.IfFail(f)` | moderate |
| [4] | `.BiBind(Succ:, Fail:)` | rare — `Blocks/Operations.cs` |
| [5] | `.BiMap(succ, fail)` | exists in LE; **zero** production `.BiMap(` in libs |
| [6] | `.ToFin()` / `.ToValidation()` / `.ToOption()` | heavy |
| [7] | `.Match(Succ:, Fail:)` | common at host boundaries |

Hot paths may use record-case `switch` on `Fin` where measured; `.Match` remains common at collapse sites.

---
## [2][APPLICATIVE]
>**Dictum:** *Independent failures accumulate; dependent steps bind.*

<br>

| [INDEX] | [FORM] | [USE] |
| :-----: | ------ | ----- |
| [1] | `(a,b,c,d).Apply(f).As()` | Domain + analysis validation |
| [2] | `Validation<Error,T>` | Domain requirements |
| [3] | `Validation<Seq<UiFault>,T>` | GH UI via `ValidateParallel` → `ToFin` |
| [4] | `[ValidationError<TFault>]` on Thinktecture VOs | Factory returns `TFault?` |

`Validation<string,T>` does **not** compile in v5.

---
## [3][COLLECTION_TRAVERSAL]
>**Dictum:** *Traverse keeps collection shape and failure rail together.*

<br>

| [INDEX] | [COMBINATOR] | [RASM_USE] |
| :-----: | ------------ | ---------- |
| [1] | `.TraverseM(f).As()` | **heavy** — `.As()` mandatory in v5 |
| [2] | `.Traverse(identity).As()` | moderate — v4 `Sequence` replacement |
| [3] | `.Choose(f)` | **heavy** — prefer over `.Filter().Map()` |
| [4] | `.Fold(init, f)` | heavy |
| [5] | `.Strict()` | moderate before boundary materialization |

[CRITICAL] `TraverseM` returns `K<F, Seq<B>>`. Without `.As()`, the expression does not lower to `Fin<Seq<B>>`.

`HashMap` algebra: `.AddOrUpdate`, `.Find`, `.Filter` — see `collections.md`.

---
## [4][EFFECT_AND_RECOVERY]
>**Dictum:** *Host effects use runtime records; recovery is mostly unused in production.*

<br>

| [INDEX] | [COMBINATOR] | [RASM_USE] |
| :-----: | ------------ | ---------- |
| [1] | `Eff.runtime<RT>()` | Analysis (`Analyze.cs`) |
| [2] | `Env.Asks` / `Eff.Lift` | Analysis + Exchange (`FileRuntime`) |
| [3] | `.Run(env: runtime)` | Exchange file ops |
| [4] | `Try.lift<Fin<T>>(f).Run()` | GH/Rhino native boundaries |
| [5] | `Prelude.catch(...)` / `@catch` | **zero** production usage |
| [6] | `.Retry(schedule:)` | **zero** production usage |

Boundary exception capsule in Rasm: **`op.Catch(() => Fin<T>)`** — not LanguageExt `@catch`.

Collapse recipe: `pipeline.Run(runtime).Run()` yields `Fin<T>` from `Eff<RT,T>`.

`Fin<T>.ToEff<RT>()` is **not** a general Rasm pattern; internal lift exists for `Validation<Error,T>` on `Analyze.Env` only.

---
## [5][STATE]
>**Dictum:** *Atoms react through host paint loops, not Subscribe APIs.*

<br>

| [INDEX] | [COMBINATOR] | [RASM_USE] |
| :-----: | ------------ | ---------- |
| [1] | `Atom<T>.Swap(f)` | heavy — Blocks, Wire, Interaction |
| [2] | `Atom<T>.SwapMaybe(f)` | Motion/UI pacers |
| [3] | `Atom<T>.SwapIO` / `SwapMaybeIO` | **zero** production usage |

v5 **removed** `Subscribe`/`OnChange`/`Reset` on `Atom<T>`.

Plain `Atom<HashMap<…>>` is used; `AtomHashMap`/`AtomSeq`/`AtomQue` types are **unused** in production.

---
## [6][TRAITS_AND_ARROWS]
>**Dictum:** *Trait arrows are skill doctrine, not production libs.*

<br>

`ComposeK`, `HyloM`, `FoldArrows`, `FanoutK` appear in `.claude/skills/coding-csharp/references/transforms.md` as schematic patterns — **zero** matches in `libs/csharp/`. `Next.Loop` exists in LanguageExt v5 XML for trampolining.

Use file-local `from..in` unless measured duplication proves trait abstraction.

---
## [7][RULES]
>**Dictum:** *Choose combinator by failure semantics, not familiarity.*

<br>

- One fallible step → `Fin` + `Bind`/`Map`.
- Independent fields → `Validation` + `.Apply().As()`.
- Host context → `Eff<RT,T>` + concrete runtime record.
- Many fallible elements → `TraverseM` + `.As()`.
- Optional merge → `Choose` or Option `\|`.
- Do not create helper wrappers around combinators listed here.
