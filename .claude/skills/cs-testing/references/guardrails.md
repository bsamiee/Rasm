# [H1][GUARDRAILS]
>**Dictum:** *Tests that re-derive their expected values from the implementation prove nothing.*

<br>

The single failure mode of AI-generated tests is circularity — the test computes its own expected value by running the same code under test, then asserts that the result equals itself. The guardrails below detect and prevent every common form of this anti-pattern.

---
## [1][ORACLE_INDEPENDENCE]
>**Dictum:** *Expected values come from laws, external math, fixed constants, or the test input — never from re-running the impl.*

<br>

| [SOURCE]                            | [VALID?] | [EXAMPLE]                                                                                          |
| ----------------------------------- | :------: | -------------------------------------------------------------------------------------------------- |
| Algebraic identity                  |  ✓      | `Spec.Identity(gen, x => -(-x))` — involution proven by math, not by `-(-1.0) == 1.0`              |
| External constant (RFC, IEEE)       |  ✓      | `Assert.Equal(Math.PI, …)` when testing a constant exposed in production                           |
| Test input itself                   |  ✓      | `Spec.Roundtrip(gen, forward, back)` — expected = original input                                   |
| Hand-computed known answer          |  ✓      | `Assert.Equal(expected: 3.0, actual: Stat.Of(Seq(1, 3, 5)).Mean)` — mean of `{1,3,5}` is provably 3 |
| Re-running the SUT                  |  ✗      | `Assert.Equal(Foo(x), Foo(x))` — tautology                                                          |
| Production helper that wraps the SUT |  ✗      | `Assert.Equal(Production.Helper(x), SUT(x))` — same code, different wrapper                        |

[CRITICAL] When tempted to compute the expected value, ask: *does this computation appear in the source file under test?* If yes, find the underlying mathematical/algebraic law that justifies the expected value and assert that instead.

---
## [2][FORBIDDEN_PATTERNS]

| [INDEX] | [PATTERN]                                              | [REASON]                                                                 |
| :-----: | ------------------------------------------------------ | ------------------------------------------------------------------------ |
|   [1]   | `Mock<T>` / `Substitute.For<T>` of a domain interface  | Domain has no interfaces (CSP0501). Use runtime records.                 |
|   [2]   | `[InlineData(...)]` for SmartEnum case enumeration     | Cases drift; use `Gen.OneOfConst` so the spec breaks on new cases.       |
|   [3]   | `var` in test declarations                             | Same rule as production: explicit types preserve intent across refactors. |
|   [4]   | `if`/`else`/`for`/`foreach`/`try`/`catch` in spec body | Same control-flow rules as production; use `switch` expressions + LINQ.  |
|   [5]   | Sharing mutable state across `[Fact]` methods          | xUnit runs facts in parallel; shared state causes flakes.                |
|   [6]   | Assertions that swallow exceptions silently            | Use `Spec.Throws<T>` or `Assert.Throws<T>`, not bare `try/catch`.        |
|   [7]   | `Thread.Sleep` / `Task.Delay` for timing-sensitive    | Use `it.live` patterns or refactor the SUT to inject the clock.          |
|   [8]   | `[SuppressMessage]` on a spec for a CA-rule failure    | Fix the analyzer signal; tests obey the same gate as production.         |
|   [9]   | Re-deriving an expected value from the impl           | Circular — use external oracle (see [section 1](#1oracle_independence)). |
|  [10]   | Parallel construction path (bypass `TryCreate`)        | Generators MUST call production smart constructors.                      |

---
## [3][CIRCULAR_TEST_DETECTION_CHECKLIST]

For each `[Fact]` you author, walk:

- [ ] **Where does the expected value come from?** If "the same function that produced the actual", rewrite.
- [ ] **Could a mutation of the production code pass this test?** If the body of `Foo(x)` were inverted (negate, off-by-one), would the test catch it? If no, the test is asserting structure instead of behavior.
- [ ] **Does the generator route through a production factory?** If `TestData.MakeFoo(...)` mints values via direct constructor while production uses `Foo.TryCreate`, the test exercises a parallel path that drifts under refactor.
- [ ] **Is the failure message actionable?** A failure that prints `Assert.True failed` and nothing else forces re-derivation; pass a `what:` / message argument.
- [ ] **Is shrinking enabled?** If the test uses `Spec.ForAll`/CsCheck `Sample`, yes — CsCheck shrinks. If the test uses hand-written loops, no — convert to `ForAll`.

---
## [4][NAMESPACE_QUALIFICATION_TRAP]
>**Dictum:** *Test namespace `Rasm.Tests.Vectors` shadows source namespace `Rasm.Vectors`.*

<br>

When a spec lives at `tests/csharp/libs/Rasm/Vectors/Atoms.spec.cs` with `namespace Rasm.Tests.Vectors;`, the unqualified token `Vectors.Direction` resolves to `Rasm.Tests.Vectors.Direction` (test-local, usually doesn't exist), not to `Rasm.Vectors.Direction`.

**Fix:** fully qualify the production type at the call site.

```csharp
// Wrong — Vectors.Direction resolves into the test namespace
Spec.Succ(Direction.Of(value: v, tolerance: ε, key: K));

// Right — fully qualified
Spec.Succ(Rasm.Vectors.Direction.Of(value: v, tolerance: ε, key: K));
```

Do not rename the test namespace to dodge this. IDE0130 enforces namespace-folder match; the qualification is the correct fix.

---
## [5][NATIVE_BOUNDARY_TRAP]
>**Dictum:** *RhinoCommon types are mixed-mode; calling a property doesn't tell you which side it lands on.*

<br>

Pure managed (safe in static rail):
- `new Point3d(x, y, z)`, `Vector3d.XAxis`, `Vector3d.Zero` — value-type construction
- `point.X`, `point.Y`, `point.Z`, `vec.Length` — field/computed access
- `vec1 + vec2`, `vec * scalar`, `-vec` — operator overloads (managed)

Native (forces bridge rail):
- `vec.IsTiny(tolerance)` → `UnsafeNativeMethods.ON_3dVector_IsTiny`
- `vec.IsValid` → native validity probe
- `vec.Unitize()` → native normalization
- `Vector3d.VectorAngle(a, b)` → native angle
- `RhinoMath.UnitScale` → native unit conversion
- Anything on `Brep`, `NurbsCurve`, `Surface`, `RhinoDoc`

[IMPORTANT] Read the source file under test before classifying. A test that LOOKS pure but calls `vec.IsTiny` at any point will fail in the static process with `System.IO.FileNotFoundException` or `UnsafeNativeMethods.*` and must be moved to the bridge.

---
## [6][LOC_OVERFLOW_REMEDIES]
>**Dictum:** *A spec over 175 LOC is a signal, not a target to relax.*

<br>

When `wc -l <spec>` reports > 175:

1. **Pack laws** sharing a generator into one packed-Action property.
2. **Promote generators** used by 2+ specs to `Rasm.TestKit/Gens.cs`.
3. **Split by concept** — one spec per source `.cs` file. If `Atoms.cs` defines 7 types and 175 LOC isn't enough to cover them, do not split that spec; the right answer is to write denser laws per type (one packed Fact per type) until coverage holds.
4. **Drop tautological assertions** — every assertion should kill at least one mutant. Assertions that always pass for any non-pathological impl are bloat.
5. **Move edge cases into the algebra generator** — `Gen.Frequency([(99, validGen), (1, edgeGen)])` lets one Sample loop cover both the happy path and the rare boundary.

[NEVER] Split a spec into two files JUST to fit the LOC budget. The cap exists to force density; splitting defeats the constraint.

---
## [7][LANGUAGE_EXT_MATCH_DOCTRINE]
>**Dictum:** *Named-arg `Match` is the only assertion path on `Fin<T>`, `Validation<E,T>`, `Option<T>`, `Either<L,R>`, `Eff<RT,T>`.*

<br>

LE v5's own test suite uses this idiom verbatim. Adopt it everywhere; never read `.IsSucc`/`.SuccessValue`/`.FailValue` properties directly.

```csharp
result.Match(
    Succ: value => Assert.Equal(expected, value),
    Fail: error => Assert.Fail($"unexpected fail: {error}"));
```

The `Spec.Succ` / `Spec.Fail` helpers in `Rasm.TestKit` wrap this pattern with the `?? throw new ArgumentNullException(nameof(result))` guard (LE v5's `Fin<A>` is `public abstract class`, so the null check is correct, not a CA1062 false positive).

For `Eff<RT,T>` pipelines: construct a runtime as a plain `record` carrying `Func<>` delegates, call `.Run(rt)` to collapse to `Fin<T>`, then `Match`. No interface mocks.

---
## [8][SWITCH_PRECEDENCE_TRAP]
>**Dictum:** *`switch` and `with` expressions outrank `*`/`/`/`%`. Parenthesize the switch input when arithmetic precedes it.*

<br>

```csharp
// Wrong — parses as `(Count - 1) * (Math.Clamp(...) switch { ... })`.
//   Switch input becomes `Math.Clamp(fraction, 0, 1)` ∈ [0, 1], not the intended `(Count-1) * Math.Clamp(...)`.
//   Every percentile silently miscomputes; no analyzer warning; unit-test author would have reproduced the bug.
private static double Quantile(Seq<double> sorted, double fraction) =>
    (sorted.Count - 1) * Math.Clamp(value: fraction, min: 0.0, max: 1.0) switch {
        double idx when ... => sorted[(int)Math.Floor(idx)],
        ...
    };

// Right — parens force the multiplication into the switch input.
private static double Quantile(Seq<double> sorted, double fraction) =>
    ((sorted.Count - 1) * Math.Clamp(value: fraction, min: 0.0, max: 1.0)) switch {
        double idx when ... => sorted[(int)Math.Floor(idx)],
        ...
    };
```

C# precedence (Microsoft docs, "C# operators and expressions"): *unary > range > switch and with > multiplicative > additive > ...*. Most engineers internalize `*` as higher than `switch` because most languages do not have switch expressions; C#'s ordering is counter-intuitive.

[CRITICAL]:
- [ALWAYS] When an arrow-body lambda combines arithmetic and a switch expression, write the switch input as a parenthesized expression. Doing so is a no-op when the parser already agreed with you and a one-character disambiguation when it didn't.
- [ALWAYS] Independent metamorphic oracles catch this bug class; oracle-tied hand-asserted unit tests do NOT (the assertion expression replicates the same precedence error). See `Stats.spec.cs::MedianMatchesSortedMiddleOracle` for the historical exemplar.

---
## [9][SNAPSHOT_DISCIPLINE]
>**Dictum:** *Snapshot the runtime-observable output, never source-generator artifacts.*

<br>

If a future scenario adopts `Verify.XunitV3`, snapshot ONLY:
- JSON serialization output of Thinktecture `[Union]` / `[SmartEnum]` instances at runtime
- `ToString()` of complex error chains and DU variant payloads
- Cobertura/diagnostic output shapes when authoring tooling

NEVER snapshot:
- `obj/**/.g.cs` source-generator outputs (couples test stability to compiler versions)
- Internal exception stack traces (line numbers drift on refactor)
- Anything containing absolute paths, GUIDs, or timestamps without scrubbing
