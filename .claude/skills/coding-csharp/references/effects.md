# Effects

Effect type system for C# 14 / .NET 10. LanguageExt `5.0.0-beta-77`. Advanced operators and combinators: [advanced-surface.md](advanced-surface.md). External detail: `docs/external-libs/languageext/`.

---

## Rail Selection

| Rail | Owns | Boundary |
| ---- | ---- | -------- |
| `Fin<T>` | One fallible synchronous operation. | Native validity, value admission, numeric result projection. |
| `Validation<Error,T>` | Independent accumulated failures. | Domain requirements, symbol sets. |
| `Validation<Seq<TFault>,T>` | Batched fault accumulation. | Parallel UI or form validation → `ToFin`. |
| `Eff<RT,T>` | Host context and effectful work. | Document, filesystem, clock, bridge runtimes. |
| `IO<A>` | Deferred side-effect/resource description. | Boundary execution before collapse. |

Use LINQ composition, `Bind`, `Map`, `MapFail`, `BindFail`, and applicative `Apply` / `&` to stay in the rail. Collapse with `Match` or `Run` only at host boundaries.

`Validation<string,T>` is **not supported** — use `StringM` or `Validation<Error,T>`; not `Validation<Seq<Error>,T>` (`CSP0703`). GH UI parallel faults: `Validation<Seq<UiFault>,T>`.

---

## Runtime Records

Read capability with `Eff.runtime<RT>()` or `Reader.Asks` / `Ask<R,A>` on a concrete sealed record.

```csharp
public sealed record AppRuntime(Context Context, Op Op);

public static Eff<AppRuntime, Context> ContextOf() =>
    Eff.runtime<AppRuntime>().Map(static rt => rt.Context);
```

Boundary collapse: pick explicit `Run` / `RunAsync` / `RunIO` overload — do not assume `Run().Run()` always yields `Fin<T>`.

---

## Recovery And Schedule

LanguageExt provides `Schedule` algebra, `Prelude.catch`, `IO.Retry(Schedule)`, and `Prelude.retry`.

When adopted:

- Schedule union: `Schedule.a | Schedule.b` or `union(a,b)`
- Intersect bounds: `intersect(policy, Schedule.upto(duration))` — not C# `&`
- Transformer chain: `transformerA + transformerB`
- Eff recovery: `Prelude.catch` / `@catch` / `IfFailEff` — not peer-Eff `|`

---

## Host Decision Monoids

Application-defined monoids for host policy merge — not LanguageExt types. Pattern: identity element + `operator +` for semigroup append; use at composition boundaries only.

---

## State And Resources

`Atom<T>.Swap` / `SwapMaybe` for managed reactive state — **no** Subscribe API in 5.0.0-beta-77.

`IO<T>.Bracket`, `Prelude.use` — resource scope in LanguageExt 5.0.0-beta-77.

---

## Rules

- [ALWAYS] `Fin<T>` for local fallible work.
- [ALWAYS] `Validation<Error,T>` or tuple `Apply` / `&` for independent fields.
- [ALWAYS] `Eff<RT,T>` when host context is required.
- [NEVER] Mix rails within one module file.
- [NEVER] Use exceptions for domain control flow.
