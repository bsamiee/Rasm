# Advanced Surface

Universal advanced capability reference for pinned library versions. **Not** basic `Map`/`Bind`/`Create`. Canonical detail: `docs/external-libs/`.

Pin truth: `LanguageExt.Core` 5.0.0-beta-77, `Thinktecture.Runtime.Extensions` 10.2.0, `MathNet.Numerics` 6.0.0-beta2, `CSparse` 4.3.0, C# 14.0 / net10.0, `Microsoft.Net.Compilers.Toolset` 5.3.0.

---

## Load With

| Task | Read |
| ---- | ---- |
| Operators, Schedule, Kleisli, lowering | `docs/external-libs/languageext/operators.md`, `combinators.md` |
| Effects, rails | `docs/external-libs/languageext/effects.md` |
| Thinktecture attrs, unions, VOs | `docs/external-libs/thinktecture/union-attributes.md`, `objects.md` |
| Sparse / dense numerics | `docs/external-libs/mathnet/sparse.md`, `linear.md` |
| C# 14 language | `docs/external-libs/csharp/language.md` |

---

## LanguageExt Operators

| Symbol | Carrier | Semantics |
| ------ | ------- | --------- |
| `>>` | `K<F,A>` | Kleisli bind / discard-first sequence |
| `>>>` | `K<F,A>` | Applicative sequence |
| `*` | `K<F,A>` | Functor map / applicative apply |
| `>> lower` / `+k` | `K<F,A>` | Downcast to concrete rail (`Prelude.lower`) |
| `\|` | `K<Validation<E,A>>` | Choice — first success wins |
| `\|` | `Fallible` + `CatchM` | Catch / recovery |
| `\|` | `Eff<RT,A>` + `Finally` | Finally composition — not peer-Eff choice |
| `&` | `Validation<E,A>` | Applicative product — accumulate errors |
| `+` | `Error` / monoid `E` | Error append |

Schedule: `Schedule.a \| Schedule.b` (union), `union`/`intersect` Prelude functions, `ScheduleTransformer +`; intersect is **not** ASCII `&` on `Schedule`.

**Absent from pinned XML:** `Decision`, `\|>`, `ComposeK`, `HyloM`, `FoldArrows`.

---

## LanguageExt Combinators

| Combinator | Notes |
| ---------- | ----- |
| `.TraverseM(f) >> lower` | Fin / Validation / Option lowering |
| `.TraverseM(f).As()` | Eff / IO lowering |
| `.Traverse(identity) >> lower` | Sequence replacement |
| `.Choose(f)` | Filter-map to Option |
| `.MapFail` / `.BindFail` / `.IfFail` | `IfFail` not on `Fin` — use `Match` / `BiBind` |
| `.BiBind(Succ:, Fail:)` | Both branches |
| `Validation.Apply` / `&` | Multi-field validation product |
| `guard` / `Optional(…).ToFin` | Prelude guards |
| `Try.lift<Fin<T>>().Run()` | Native boundary |
| `Eff.runtime<RT>()` / `Reader.Asks` / `Prelude.liftEff` | Effects — not `Env.Asks` |
| `Prelude.catch` / `IO.Retry(Schedule)` / `Prelude.retry` | Recovery |
| `Atom.Swap` / `SwapMaybe` | State — Subscribe absent in 5.0.0-beta-77 |

`Validation<string,T>` is not supported — use `Validation<StringM,T>` or `Validation<Error,T>` (`CSP0703` forbids `Validation<Seq<Error>,T>` in domain/application/shared; GH UI uses `Validation<Seq<UiFault>,T>`).

Time units: `LanguageExt.UnitsOfMeasure.ms` (explicit import).

---

## Thinktecture Attributes

| Attribute | Effect |
| --------- | ------ |
| `[Union(SwitchMapStateParameterName = "…")]` | State-threaded `.Switch` / `.Map` |
| `SwitchMethods` / `MapMethods` | `SwitchMapMethodsGeneration` enum |
| `[UnionSwitchMapOverload(StopAt = typeof(...))]` | Partial overload generation |
| `[ValueObject<T>]` | Branded scalar |
| `[ComplexValueObject]` | Multi-field VO — partial only |
| `[ValidationError<TFault>]` | Typed factory validation |
| `[SmartEnum]` / `[SmartEnum<TKey>]` | Total enum dispatch |
| `[UseDelegateFromConstructor]` | SmartEnum delegate from ctor |
| `[KeyMemberEqualityComparer<T>]` / `[MemberEqualityComparer<,>]` | Key/member equality |

Thinktecture does **not** emit union `operator +`/`|` in 10.2.0. Set `SerializationFrameworks = None` when JSON integration packages are not pinned.

---

## MathNet + CSparse

| Surface | Owner |
| ------- | ----- |
| Dense factorizations, statistics, integration | MathNet |
| `BiCgStab`, preconditioners, `Iterator<T>` | MathNet iterative |
| `SparseCholesky`, AMD ordering, CSC solve | CSparse — `mathnet/sparse.md` §7–§10 |
| CSR/CSC hybrid strategy | `mathnet/sparse.md` |

Package id for direct sparse library: **`CSparse`**. No LOBPCG type in MathNet 6.0.0-beta2 XML.

---

## C# 14 Highlights

| Feature | Use |
| ------- | --- |
| Extension blocks | Instance/static operators without wrappers |
| Collection expressions | `[..a, ..b, x]` |
| `params ReadOnlySpan<T>` | Arity-polymorphic APIs |
| `field` keyword | Inline property validation |
| Implicit span conversions | Array ↔ span overload resolution — audit ambiguous overload sets |
| Null-conditional assignment | Boundary-only `target?.Prop = value` |
| User-defined compound assignment | Domain monoid/receipt `+=` |
| Switch / list patterns | Total value-returning dispatch |

Full catalog: `docs/external-libs/csharp/language.md`.

---

## Prompt Line

Load `coding-csharp` → `references/advanced-surface.md`; language: `docs/external-libs/csharp/language.md`; libs: `docs/external-libs/languageext/` + `thinktecture/` (+ `mathnet/sparse.md` if numerics). **C#14:** extension blocks; `[..]` collection expressions; `params ReadOnlySpan<T>`; `field`/`required`; switch + list/property patterns. **LE/TT:** `>>` `>>>` `*` `&` `+`; `TraverseM >> lower` or `.As()` on Eff/IO; `.Choose` `.MapFail`/`.BindFail`/`.BiBind`; `guard` `Optional(…).ToFin` `Try.lift` `Atom.Swap`; `[Union(SwitchMapStateParameterName)]` `[ValueObject<T>]` `[ComplexValueObject]` `[ValidationError<T>]` `[SmartEnum<…>]`; Schedule `|` / `union` / `intersect`.
