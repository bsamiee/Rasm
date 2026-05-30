# Effects

Effect type system for C# 14 / .NET 10. LanguageExt v5 per pinned Rasm `LanguageExt.Core`. Advanced operators and combinators: [advanced-surface.md](advanced-surface.md). External detail: `docs/external-libs/languageext/`.

---

## Rail Selection

| Rail | Owns | Boundary |
| ---- | ---- | -------- |
| `Fin<T>` | One fallible synchronous operation. | Native validity, value admission, MathNet result projection. |
| `Validation<Error,T>` | Independent accumulated failures. | Domain requirements, symbol sets. |
| `Validation<Seq<UiFault>,T>` | Batched UI fault accumulation. | GH `ValidateParallel` → `ToFin`. |
| `Eff<RT,T>` | Host context and effectful work. | Analysis `Env`, Exchange `FileRuntime`. |
| `IO<A>` | Deferred side-effect/resource description. | Boundary execution before collapse. |

Use LINQ composition, `Bind`, `Map`, `MapFail`, `BindFail`, and applicative `.Apply().As()` to stay in the rail. Collapse with `Match` or `Run` only at host boundaries.

`Validation<string,T>` does **not** compile in v5.

---

## Runtime Records

Read capability with `Eff.runtime<RT>()` or `Env.Asks` on a concrete sealed record. Rasm uses **`Analyze.Env`** (analysis) and **`FileRuntime`** (exchange via `Eff.Lift`).

```csharp
public sealed record RhinoRuntime(Context Context, Op Op);

public static Eff<RhinoRuntime, Context> ContextOf() =>
    Eff.runtime<RhinoRuntime>().Map(static rt => rt.Context);
```

Boundary collapse: `Fin<T> result = pipeline.Run(runtime).Run();`

---

## Recovery And Schedule

LanguageExt provides `Schedule` algebra, `Prelude.catch`, and `.Retry` — **zero production usage** in `libs/csharp/` today. Document for composition-root scaffolding only.

When adopted:

- Chain transformers: `policyA | policyB` on `ScheduleTransformer`
- Intersect bounds: `intersect(policy, Schedule.upto(duration))` — not C# `&`
- Duration literals: `LanguageExt.UnitsOfMeasure.ms` (requires explicit import)

Production native recovery: **`op.Catch(() => Fin<T>)`** and **`Try.lift<Fin<T>>().Run()`** at boundaries.

---

## Host Decision Monoids

Not LanguageExt types:

| Type | Use |
| ---- | --- |
| `OverlayDecision` | Rhino overlay allow/deny merge (`Ignore`, `operator +`) |
| `Rasm.Grasshopper.Components.Decision` | Component input routing (`Pass`, `Handled`, `operator +`) |

---

## State And Resources

`Atom<T>.Swap` / `SwapMaybe` for UI, blocks, motion — **no** Subscribe API in v5 Atom.

`IO<T>.Bracket`, `Prelude.use` exist in LE v5; unused in production `libs/`.

---

## Rules

- [ALWAYS] `Fin<T>` for local fallible work.
- [ALWAYS] `Validation<Error,T>` or tuple `.Apply().As()` for independent fields.
- [ALWAYS] `Eff<RT,T>` with concrete runtime records for host effects.
- [ALWAYS] `.TraverseM(f).As()` for batch monadic traverse in v5.
- [ALWAYS] `op.Catch` / `Try.lift` at native boundaries — not domain try/catch.
- [NEVER] Collapse rails mid-pipeline.
- [NEVER] use `Validation<string,T>` in v5.
- [NEVER] document LanguageExt `Decision` — type does not exist in pinned package.
