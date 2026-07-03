# [H1][TESTKIT]
>**Dictum:** *The testkit is one rail, not a module-specific helper drawer.*

<br>

## [01]-[FILES]

`tests/csharp/_testkit` is `Rasm.TestKit`: host-free, wire-blind, referenced by every test shell through classification. `tests/csharp/_scenariokit` is `Rasm.ScenarioKit`: the host-aware sibling owning the scenario SDK.

| [INDEX] | [FILE]         | [OWNS]                                                                                                                                       |
| :-----: | -------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Spec.cs`      | `Law` row algebra with mandatory refuting witnesses, `Spec.ForAll`/`Hold`/`Refutes`/`Replay`, metamorphic relation tables, model-based/parallel/dual-path stateful laws, `Fin`/`Validation`/`Option` rail gates, classify/chi-squared distribution laws, byte-identity roundtrips, `Catalog`/`Matrix`/`Family` case tables, sample policy env resolution. |
|  [02]   | `Approx.cs`    | `Tolerance` policy, `Metric` row dispatch (absolute, sign-ambiguous), `Approx.Equal`, and the `Spec.Equal` numeric assertion family.          |
|  [03]   | `Gens.cs`      | `Fault` typed error family and reusable primitive/geometry/value-object/rail generators consumed by multiple specs.                           |
|  [04]   | `Numeric.cs`   | Independent numeric oracles: norms, residuals (entrywise, symmetry, product, solve, eigenpair, orthogonality), centroid/covariance, Laplacian and heat-kernel references, convergence order. |
|  [05]   | `Laws.cs`      | `[Law]` marker attribute, `LawRecord`/`SutTarget`, `Laws.ScanAssembly`/`Sut`/`AssertCoverage` — the declarative law-coverage census over a SUT assembly with typed exemptions. |
|  [06]   | `Seams.cs`     | `Shape`/`VariantPayload` ADTs, `SeamProbe` typed call log with LIFO `SeamRestore`, `VariantWriter` fixture emission, `TmpRoot` scoped roots, `NdjsonOracle` line-gated decode oracles. |
|  [07]   | `Manifests.cs` | `ProjectFacts`, solution/disk project census, `ProjectGraph` reference-topology law, central-package-management facts.                        |

ScenarioKit sibling (`tests/csharp/_scenariokit`): `Scenario.cs` carries `[RhinoScenario]`, the closed fact-key grammar, and `ScenarioContext`; `Scope.cs` carries `DocumentScope`, `Capture.Snapshot`, and `CaptureReceipt`. Scenario content composes these from `tests/csharp/scenarios`; the testkit never references them.

---
## [02]-[SPEC_CONTRACT]

- Use `Spec.ForAll` instead of raw `gen.Sample` so seed/iter/time/thread precedence stays consistent; the env knobs `CsCheck_Seed`/`CsCheck_Iter`/`CsCheck_Time`/`CsCheck_Threads` get first refusal.
- Register algebra laws as `Law` rows (`Law.Of`/`Identity`/`Idempotent`/`Inverse`/`Roundtrip`/`Commutative`/`Associative`/`Distributive`/`Monotone`/`Permutation`) held by `Spec.Hold`. Every row carries a refuting witness; `Spec.Hold` runs the refutation before sampling, and a witness the property survives fails the registration as a tautology.
- Use `Spec.Metamorphic` with `MetamorphicRelation` rows when expected values come from an independent path; one base evaluation feeds every relation row.
- Use `Spec.ModelBased` for actual-vs-model operation sequences, `Spec.Parallel` for linearization laws, and `Spec.DualPath` for two-mutation-path state equality.
- Use `Spec.Valid` / `Spec.Invalid` for `Validation<Error,T>` and `Spec.Succ` / `Spec.Fail` / `Spec.FailCategory` for `Fin<T>`; `Spec.AllErrors`/`Spec.FailMany` own accumulated-error shape.
- Do not use `.IsSucc`, `.IsFail`, `.IsSome`, or `.IsNone` as primary assertions; preserve category diagnostics through `Spec`.
- Use `Spec.Equal` with a `Tolerance` and an optional `Metric` row (`Metric.SignAmbiguous` for eigen-axis comparisons) for generated numeric comparisons; sign ambiguity is a metric row, never a sibling method.
- Use `Numeric.*` residual oracles for matrix/vector expected values; the oracle must not reuse the production operator under test.
- Use `Spec.Catalog(items, expectedKeys, key, law)` for closed catalogs: key uniqueness, exact expected membership, and the per-item law fold in one pass.
- Use `Spec.Matrix` for labeled probe rows; per-row thunks keep call-site generics and each row names its own failure.
- Use `Spec.Family` with `ValueObjectShape` rows for value objects: admission and rejection generators are both mandatory.
- Add model-based, async, snapshot, benchmark, or fuzz adapters only when two concrete consumers exist.

---
## [03]-[GENERATOR_CONTRACT]

- Value-object generators call production `TryCreate`/`Create`/`AcceptValidated` paths.
- Edge-biased scalar generators include tolerance-adjacent values, not just broad random ranges.
- Context/policy generators must produce valid instances and remain reusable across sibling spec families.
- Keep module-local generators inside the spec until at least two consumers need the same concept.

---
## [04]-[SUPPRESSION_POLICY]

- No local `SuppressMessage` attributes for normal xUnit generator classes.
- Keep test classes public for xUnit discovery.
- Keep spec-local generator/static data classes non-public when discovery does not need them.
- Folder-wide analyzer rationale belongs in `.editorconfig`, not in repeated file-local comments.

---
## [05]-[GENERATOR_SHRINKING_RULE]
>**Dictum:** *`throw` inside `Select` breaks shrinking; use `Where(Try)+Select` to preserve it.*

<br>

CsCheck shrinking finds minimal counterexamples by narrowing failed inputs. Two patterns break it:

| [BREAKS]                                                             | [PRESERVES]                                                                        |
| -------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `Gen.Int.Select(i => i > 0 ? new T(i) : throw new ...)`              | `Gen.Int.Where(i => i > 0).Select(i => new T(i))`                                  |
| `Gen.Select(Factory).Select(opt => opt.IfNone(() => throw new ...))` | `Gen.Select(Factory).Where(opt => opt.IsSome).Select(opt => opt.IfNone(default!))` |

For factory-routed value objects returning `Fin<T>` / `TryCreate(out T)`:

```csharp
public static readonly Gen<RefinedShape> Shape =
    RawShape
        .Select(v => RefinedShape.TryCreate(value: v, obj: out RefinedShape s) ? Some(s) : None)
        .Where(opt => opt.IsSome)
        .Select(opt => opt.IfNone(default(RefinedShape)));
```

The `throw` form turns rejected candidates into property failures instead of filtered generation. `WhereLimit` applies to `Gen.Where`; keep predicates broad enough to preserve shrinkable satisfying values.

---
## [06]-[ADMISSION_EXTENSION_HOOK]
>**Dictum:** *Cross-layer types extend admission through an interface, not a switch arm.*

<br>

When a downstream layer type needs upstream admission support, the canonical extension hook is a marker interface or validation protocol:

```csharp
public interface IDomainValid { bool IsValid { get; } }

// in the upstream admission switch:
IDomainValid v => Some(v.IsValid),
```

Downstream record/struct types implement the marker:

```csharp
public readonly record struct Receipt(...) : IDomainValid {
    public bool IsValid => /* invariant predicate */;
}
```

Do not add per-type arms to an upstream admission switch for cross-layer types — that inverts the dependency direction and violates the no-upstream-mirroring rule. When the pattern lands in production, an ArchUnitNET law in `tests/csharp/_architecture` owns the reachability census.

---
## [07]-[ASSEMBLY_FIXTURE_PATTERN]
>**Dictum:** *Shared assembly context is an attribute, not an interface.*

<br>

xUnit v3 has **no `IAssemblyFixture<T>` API** — the v2 folklore does not apply. Shared assembly context uses `[assembly: AssemblyFixture(typeof(T))]` plus constructor injection:

```csharp
// anywhere in the test assembly:
[assembly: AssemblyFixture(typeof(SharedModelFixture))]

// the fixture:
public sealed class SharedModelFixture {
    public Shape Model { get; }
    public SharedModelFixture() =>
        Model = Shape.Of(/* expensive immutable construction */)
            .Match(Succ: static s => s, Fail: static e => throw new InvalidOperationException(e.Message));
}

// consumers:
public sealed class ShapeLaws(SharedModelFixture fixture) {
    private readonly Shape model = fixture.Model;
    [Fact] public void LawUsesSharedModel() => Spec.ForAll(gen, x => /* uses model */);
}
```

Promote to a shared fixture only when 3+ specs duplicate the same `static readonly` construction block. AssemblyFixture is thread-shared across parallel test classes — reserve it for constructed-once expensive immutables (reference factorizations, document loaders), never for per-test mutable state.
