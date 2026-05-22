# Effects

Effect type system for C# 14 / .NET 10. LanguageExt v5 is used as verified in the pinned Rasm `LanguageExt.Core` package. `Fin<T>` carries synchronous failure, `Validation<Error,T>` accumulates independent failures, `Eff<RT,T>` carries runtime-record effects, and `IO<A>` describes boundary work before execution.

---

## Rail Selection

| Rail | Owns | Boundary |
| ---- | ---- | -------- |
| `Fin<T>` | One fallible synchronous operation. | Native validity, value admission, MathNet result projection. |
| `Validation<Error,T>` | Independent accumulated failures. | GH2 inputs, symbol sets, requirement groups. |
| `Eff<RT,T>` | Host context and effectful work. | Rhino docs, GH runtime, filesystem, bridge, clock. |
| `IO<A>` | Deferred side-effect/resource description. | File/process/resource execution before collapse. |

Use LINQ composition, `Bind`, `Map`, `MapFail`, `BiMap`, `Flatten`, and applicative `Apply` to stay in the rail. Collapse with `Match` or `Run` only at host boundaries.

---

## Runtime Records

Runtime records are plain sealed records passed to `Eff`. Read runtime capability with `Eff.runtime<RT>()`, then project with `Map` or bind into the next effect. Keep runtime records concrete and small; do not introduce service locators or container-only abstractions.

```csharp
namespace Domain.Effects;

public sealed record RhinoRuntime(Context Context, Op Op);

public static class RuntimeRail
{
    public static Eff<RhinoRuntime, Context> ContextOf() =>
        Eff.runtime<RhinoRuntime>()
            .Map(static (RhinoRuntime runtime) => runtime.Context);
}
```

`static` lambdas remain preferred for zero-capture projections. Runtime records own dependencies; dependency containers may create records but must not leak into domain transforms.

---

## Recovery And Schedule

`Schedule` is the retry/repeat/backoff algebra. Use verified LanguageExt recovery combinators from local XML and keep recovery at the composition boundary. Domain code returns typed failures; boundary code maps host exceptions once into `Error`.

`Schedule.recurs`, `spaced`, `linear`, `exponential`, `fibonacci`, `upto`, `fixedInterval`, `windowed`, `maxDelay`, `maxCumulativeDelay`, `jitter`, `decorrelate`, and `resetAfter` are first-class policy values. Use them only when a real boundary operation needs retry or repeat semantics.

---

## State And Resources

`Atom<T>` and `Ref<T>` are host-state tools, not domain accumulation tools. Use them for UI, bridge, subscription, or session state. Keep Rhino-owned mutable geometry and GH2 mutable trees out of long-lived atoms unless ownership is explicit and disposal/transfer policy is documented.

`IO<T>.Bracket`, `Finally`, and resource-style composition describe lifetime at boundaries. Prefer scoped projection over storing native mutable resources.

---

## Rules

- [ALWAYS] `Fin<T>` for local fallible work.
- [ALWAYS] `Validation<Error,T>` for independent input accumulation.
- [ALWAYS] `Eff<RT,T>` with runtime records for host effects.
- [ALWAYS] `IO<A>` for boundary side-effect descriptions.
- [ALWAYS] `Schedule` for boundary retry/repeat/backoff.
- [NEVER] Use raw exceptions as domain control flow.
- [NEVER] Collapse rails mid-pipeline.
- [NEVER] introduce legacy runtime trait DI or service-location patterns.
- [NEVER] use host state to hide normal immutable domain transforms.
