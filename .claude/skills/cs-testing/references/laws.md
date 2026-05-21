# [H1][LAWS]
>**Dictum:** *Laws are mathematical truths independent of implementation; generators exercise the full domain.*

<br>

Property laws are external oracles — identities that hold regardless of implementation strategy. When a law breaks, the defect is in the code, never in the specification. Generators that route through production `TryCreate` / `Fin<T>` factories guarantee that tests exercise the exact validation boundary callers see; CsCheck's shrinking then isolates the minimal counterexample.

---
## [1][SMART_CONSTRUCTOR_LAWS]
>**Dictum:** *Every Thinktecture `[ValueObject<T>]` requires four laws at minimum.*

<br>

| [LAW]       | [STATEMENT]                                                  | [CODE_SHAPE]                                                                                                                 |
| ----------- | ------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| Closure⁻    | Factory rejects every value outside the validated domain.    | `Spec.ForAll(invalidGen, x => Assert.False(T.TryCreate(x, out _)))`                                                          |
| Closure⁺    | Factory accepts every value inside the validated domain.     | `Spec.ForAll(validGen, x => Assert.True(T.TryCreate(x, out _)))`                                                             |
| Roundtrip   | `back ∘ forward = id` over the validated domain.             | `Spec.Roundtrip(validGen, forward: v => v.Value, back: x => T.TryCreate(x, out var v) ? v : throw …)`                        |
| Non-finite⁻ | NaN / ±∞ rejected when the underlying primitive is `double`. | `Spec.ForAll(Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity), x => Assert.False(T.TryCreate(x, out _)))` |

[IMPORTANT]: Closure tests use `Gens.Finite.Where(invalidPredicate)` to focus shrinking on edge boundaries. Avoid `Gens.Double` (unbounded; produces ±∞ noise).

---
## [2][SMART_ENUM_AND_UNION_LAWS]
>**Dictum:** *Bounded vocabularies require enumeration, not generation.*

<br>

| [LAW]           | [STATEMENT]                                                     | [CODE_SHAPE]                                                                                |
| --------------- | --------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| KeyUniqueness   | Distinct keys across all `SmartEnum<T>` cases.                  | `Assert.Equal(n, new[]{ A.Key, B.Key, … }.Distinct().Count())`                              |
| KeyInvariant    | Each case's documented key value.                               | `Assert.Equal(expected: 1, actual: SignedAxis.PositiveX.Key)`                               |
| ExhaustiveScan  | A `[Fact]` per case OR `Gen.OneOfConst` covering every case.    | `public static readonly Gen<T> Cases = Gen.OneOfConst(A, B, C);`                            |
| CaseInvariant   | A property that must hold for every case (sign, length, count). | `Spec.ForAll(Cases, x => Spec.EqualWithin(Math.Abs(x.Sign), 1.0, 0.0, "unit magnitude"))`   |

[CRITICAL]: Never write `[Theory] [InlineData(...)]` for SmartEnum cases — use `Gen.OneOfConst` so shrinking can report the failing case.

---
## [3][ALGEBRAIC_OPERATION_LAWS]
>**Dictum:** *Composable operations have algebraic identities; encode them.*

<br>

| [LAW]         | [STATEMENT]                            | [HELPER]                                                                                  |
| ------------- | -------------------------------------- | ----------------------------------------------------------------------------------------- |
| Identity      | `f(x) = x` for an alleged-identity f.  | `Spec.Identity(gen, f)`                                                                   |
| Idempotent    | `f(f(x)) = f(x)`.                      | `Spec.Idempotent(gen, f)`                                                                 |
| Inverse       | `g(f(x)) = x`.                         | `Spec.Inverse(gen, f, g)`                                                                 |
| Involutive    | `f(f(x)) = x` (self-inverse).          | `Spec.Identity(gen, x => f(f(x)))`                                                        |
| Commutative   | `op(a, b) = op(b, a)`.                 | `Spec.Commutative(gen, op)`                                                               |
| Associative   | `op(op(a, b), c) = op(a, op(b, c))`.   | `Spec.Associative(gen, op)`                                                               |
| Monotone      | Input order preserved through `f`.     | `gen.Select(gen).Sample((a, b) => order(a, b) → order(f(a), f(b)))`                       |
| Absorption    | `op(absorber, x) = absorber` ∀ x.      | `Spec.ForAll(gen, x => Assert.Equal(absorber, op(absorber, x)))`                          |

[IMPORTANT]: When the operation returns `Fin<T>` (fallible), wrap with `Spec.Succ`/`Spec.Fail` to project the algebra over success cases. Failures are themselves a law: invalid inputs MUST produce `Fin.Fail`.

---
## [4][FIN_RAIL_LAWS]
>**Dictum:** *`Fin<T>` rails encode failure as a value; tests must exercise both arms.*

<br>

| [LAW]                 | [STATEMENT]                                                          | [CODE_SHAPE]                                                            |
| --------------------- | -------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| InvalidInputFails     | Each documented invalid-input case yields `Fin.Fail`.                | `Spec.Fail(Op.X(badInput, key))`                                        |
| ValidInputSucceeds    | Each valid input yields `Fin.Succ`.                                  | `Spec.Succ(Op.X(goodInput, key))`                                       |
| FailMessageContains   | Fail carries a stable error identity (when error types are exposed). | `Spec.Fail(result, then: e => Assert.Contains("invalid", e.Message))`   |
| BindPropagates        | `Fin.Succ(x).Bind(f) = f(x)`.                                        | `Spec.ForAll(gen, x => Assert.Equal(f(x), Fin.Succ(x).Bind(f)))`        |
| BindFailIdentity      | `Fin.Fail(e).Bind(f) = Fin.Fail(e)`.                                 | `Assert.True(Fin.Fail<int>(error).Bind(f).IsFail)`                      |

[CRITICAL]: When testing internal `Fin<T>`-returning factories (e.g. `Direction.Of(value, tolerance, key)`), the test project MUST be listed as `<InternalsVisibleTo>` in `Directory.Build.props`. The Rasm friend list already includes `Rasm.Tests` and `Rasm.TestKit`.

---
## [5][STATISTICAL_LAWS]
>**Dictum:** *Aggregate functions over `Seq<double>` satisfy invariants regardless of input.*

<br>

| [LAW]                    | [STATEMENT]                                          | [CODE_SHAPE]                                                                              |
| ------------------------ | ---------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| EmptyFails               | `aggregate(empty)` ⇒ `Fin.Fail`.                     | `Spec.Fail(Stat.Of(Seq<double>(), key))`                                                  |
| SingletonZeroVariance    | One element ⇒ Variance = 0, Min = Max = element.     | `Spec.ForAll(SingletonGen, xs => …)`                                                      |
| ConstantSequence         | All-equal input collapses Mean / Min / Max / Var.    | `Spec.ForAll(ConstantGen, xs => Spec.EqualWithin(stat.Variance, 0.0, ε))`                 |
| MeanInBounds             | `Min ≤ Mean ≤ Max`.                                  | `Spec.ForAll(gen, xs => Assert.True(s.Min ≤ s.Mean ≤ s.Max))`                             |
| VarianceNonNegative      | `Variance ≥ 0`.                                      | `Spec.ForAll(gen, xs => Assert.True(s.Variance >= 0.0))`                                  |
| CountMatchesInput        | `Count == |input|`.                                  | `Spec.ForAll(gen, xs => Assert.Equal(xs.Count, s.Count))`                                 |
| NaNInputFails            | Any NaN in input ⇒ `Fin.Fail`.                       | `Spec.Fail(Stat.Of(Seq(1.0, double.NaN), key))`                                           |

[REFERENCE] Worked example: `tests/csharp/libs/Rasm/Domain/Stats.spec.cs`.

---
## [6][METAMORPHIC_LAWS]
>**Dictum:** *When an INDEPENDENT algorithm computes the same result, encode the equivalence — not a snapshot.*

<br>

A metamorphic test asserts `SUT(x) ≡ Oracle(x)` (or a relation `R(f(x), f(T(x)))`) over a generated domain where the SUT and the Oracle reach the same answer via genuinely different code paths. The oracle is not the SUT under a different name; it is an external reference computation (LINQ aggregate, NIST vector, hand-derived closed form, prior-art library, slower brute-force search) — or another invocation of the SUT itself under a transformed input where the relation is provable.

**Formal vocabulary** (Chen et al., HKUST-CS98-01, 1998):

- **Oracle problem**: an oracle is a mechanism that decides correctness for one input. For floating-point math, ML models, and aggregations, hand-writing the oracle either (a) duplicates the SUT (circular) or (b) compares at last-ULP (fragile). Both poison the test.
- **Metamorphic relation (MR)**: a property over related inputs `x`, `T(x)` such that `R(f(x), f(T(x)))` holds. The MR removes the need for a pointwise oracle — only the relation is asserted.
- **PBT subsumes MR**: every MR is a property; `Spec.Metamorphic` is the PBT entry point for the special case where the oracle is a different computation over the same input.

**MR classes (with Rasm-relevant exemplars):**

| [CLASS]                | [FORM]                                | [EXAMPLE]                                                                  |
| ---------------------- | ------------------------------------- | -------------------------------------------------------------------------- |
| Identity / arithmetic  | `f(g(x)) = x`                         | `back(forward(v)) = v` (roundtrip)                                         |
| Symmetric              | `f(T(x)) = f(x)`                      | `sin(π − x) = sin(x)`; cross-product anticommutativity                     |
| Additive               | `f(x + k) = f(x) + h(k)`              | `Stat.Of(xs.Map(x => x + k)).Mean = Stat.Of(xs).Mean + k`                  |
| Multiplicative         | `f(k·x) = k^n · f(x)`                 | `det(k·A) = k^n · det(A)` (bridge rail)                                    |
| Permutation invariance | `f(σ(x)) = f(x)`                      | `Stat.Of(shuffle(xs)) = Stat.Of(xs)` for Mean/Min/Max/Var (NOT M2 ordering) |
| Monotonicity           | `x ≤ y ⇒ f(x) ≤ f(y)`                 | quantile interpolation is monotone in fraction                             |
| Oracle equivalence     | `f₁(x) = f₂(x)` for independent impls | Welford-online Mean vs LINQ `Average`                                      |
| Invertive              | `f ∘ f⁻¹ = id`                        | encrypt/decrypt, serialize/deserialize                                     |
| Distributive           | `f(x op y) = f(x) op' f(y)`           | `(A·B)·C = A·(B·C)` matrix associativity                                   |

Use `Spec.Metamorphic(gen, path: …, oracle: …, eq: …)` to express oracle equivalence; use `Spec.ForAll` with a transform inside the property body to express the other MR classes. The optional `eq` is a tolerance predicate (essential for floating-point comparisons).

| [INDEX] | [SUT]                                                | [INDEPENDENT_ORACLE]                                                                                        | [LIVE_AT]                                              |
| :-----: | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- | ------------------------------------------------------ |
|   [1]   | `Stat.Of(xs).Mean` (Welford online)                  | `System.Linq.Enumerable.Average(xs)` — eager LINQ aggregate                                                 | `Stats.spec.cs::MeanMatchesLinqAverage`                |
|   [2]   | `Stat.Of(xs).Variance` (Welford `M2/n`)              | `Avg(xs.Select(x => (x − Avg(xs))²))` — textbook two-pass                                                   | `Stats.spec.cs::VarianceMatchesTextbookTwoPassFormula` |
|   [3]   | `Stat.Of(xs).Rms` (identity-derived `√(μ²+σ²)`)      | `√(Avg(xs.Select(x => x²)))` — direct quadratic mean E[X²]                                                  | `Stats.spec.cs::RmsMatchesDirectQuadraticMean`         |
|   [4]   | `Stat.Of(xs).Minimum/Maximum` (Welford running extrema) | `Enumerable.Min(xs)` / `Enumerable.Max(xs)` — eager LINQ extrema                                            | `Stats.spec.cs::ExtremaMatchLinqMinMax`                |
|   [5]   | `Vector3d.CrossProduct(a, b)` (RhinoCommon)          | `dot(cross, a) == 0 ∧ dot(cross, b) == 0` — perpendicularity definition                                    | `Atoms.spec.cs::CrossProductIsPerpendicularToBothInputs` |
|   [6]   | `Distribution.Of(xs, [50]).Median` (quantile interp at fraction 0.5) | `sorted[n/2]` (odd) or `(sorted[n/2−1]+sorted[n/2])/2` (even) — textbook median                            | `Stats.spec.cs::MedianMatchesSortedMiddleOracle` — see [7][DIVERGENCE_TRIAGE_WORKFLOW] for the C# precedence bug this test localized |
|   [7]   | Custom fold over `Seq<T>`                            | `xs.AsIterable().Aggregate(seed, fn)` — LINQ aggregate                                                       | candidate                                              |
|   [8]   | Geometry-kernel projection (closed-form shape)       | Parametric formula for sphere/plane/line                                                                    | candidate (bridge rail)                                |
|   [9]   | Crypto / hashing                                     | RFC-published test vectors                                                                                  | n/a (not yet in scope)                                 |

[CRITICAL]:
- The oracle must NOT call the SUT. If you write `oracle: xs => Stat.Of(xs).Mean`, you have a tautology, not a metamorphic test.
- The oracle's algorithm must differ from the SUT's. **Same formula, different inlining** is not metamorphic — the same code path is exercised twice and a shared bug stays hidden. The Variance example above qualifies because Welford accumulates `M2 = Σ(x_i − μ_i)(x_i − μ_{i−1})` online while the oracle computes `Σ(x − μ)²` with a globally-fixed μ: two genuinely different orderings of arithmetic with provably equal exact result but distinct floating-point error footprints.
- When SUT and oracle share infrastructure (`Math.Sqrt`, `LINQ.Average`), the metamorphic part is the COMPOSITION, not the primitive. `√(μ² + σ²)` vs `√(Avg(x²))` use the same Sqrt but compose it over different intermediate quantities — that is sufficient.
- An MR test can catch language-level bugs that NO hand-asserted unit test would catch. The Distribution.Median test (entry [6] above) localized a C# operator-precedence error in `(Count - 1) * Math.Clamp(...) switch { ... }` (switch binds tighter than `*`, so the multiplication landed OUTSIDE the switch). The unit-test author would have written `Assert.Equal(Quantile(sorted, 0.5), sorted[2])` — same precedence bug, equal failure. Only an INDEPENDENT computation through a different formula exposed it.

---
## [7][DIVERGENCE_TRIAGE_WORKFLOW]
>**Dictum:** *When a metamorphic test fails, the failure is the data — triage before assuming bug location.*

<br>

A new metamorphic test that fails on first run is more likely to be doing its job than to be wrong. CsCheck reports the shrunk counterexample plus a replay seed. The triage sequence is mechanical:

| [INDEX] | [STEP]                                                    | [ARTIFACT]                                                                                                                  |
| :-----: | --------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | **Record the shrunk input + seed**                        | CsCheck output: `Set seed: "..." or -e CsCheck_Seed=... (N shrinks, M skipped, K total).` and the literal input values.    |
|   [2]   | **Verify oracle independence**                             | Re-read oracle code. Does it call the SUT directly or indirectly? Is the algorithm genuinely different? If the oracle re-derives from SUT logic, the test is circular — rewrite the oracle, do NOT fix production. |
|   [3]   | **Write a sharper diagnostic test** with hardcoded values  | `[Fact]` calling SUT and oracle separately on the shrunk input, printing each result via `Assert.Fail($"sut=...; oracle=...; intermediate=...")`. Add probes for ANY intermediate quantity the SUT exposes (here: `d.Percentiles`, `d.Summary.Minimum/Maximum`). |
|   [4]   | **Localize the divergence**                                | Compare diagnostic output to the SUT's expected algebra. If intermediates (`Stat.Min`/`Max`) are correct but the final result is wrong, the bug is in the SUT's final composition layer (here: the `Quantile` function or the `OrderBy.AsIterable().ToSeq()` chain that feeds it). If intermediates are wrong, the bug is earlier. |
|   [5]   | **Decision: fix production OR park test**                 | If the bug is in production and the fix is in-scope, fix it and convert the test from `candidate` to `live`. If production stays untouched, REMOVE the failing assertion (do not commit a red test or a Skip-with-no-issue-link); record the discovery and the shrunk seed in this catalog so the next author has a hot lead. |
|   [6]   | **Delete the diagnostic scaffolding**                     | The `Assert.Fail` diagnostic test is throw-away; remove it once triage is complete. The signal lives in the catalog entry + the seed, not in retained debug code. |

**Closed case — `Distribution.Median` (resolved May 2026):**

The metamorphic test `Stats.spec.cs::MedianMatchesSortedMiddleOracle` failed on its first run. Shrunk counterexample: 5 elements `[218247, 2.76e-18, 636851, -71038.5, -2.78e-12]`. SUT returned `-142077`; oracle returned `2.76e-18` (the true median = sorted[2] for n=5).

Triage walked the workflow above. Step [3] diagnostic probes verified:
- `Stat.Of` computed Count/Min/Max correctly (independently of Quantile)
- `values.OrderBy(v => v).AsIterable().ToSeq()` produced the correctly sorted Seq with correct indexing (`sorted[2] = 2.76e-18`)
- An inline Quantile formula on the same sorted Seq returned `2.76e-18` (correct)
- **The same formula expressed as an arrow-body switch expression** `(sorted.Count - 1) * Math.Clamp(fraction, 0.0, 1.0) switch { ... }` returned `-142077`

Step [4] localized the divergence to **C# operator precedence**: the `switch` and `with` expressions bind TIGHTER than the multiplicative operators (`* / %`). The expression parses as `(sorted.Count - 1) * (Math.Clamp(...) switch { ... })`, so the switch input was `Math.Clamp(fraction)` (range `[0, 1]`) — never `(Count-1) * Math.Clamp(fraction)`. For fraction=0.5, idx is 0.5 (not 2.0); case 1's integer-equality guard fails; the third arm runs with floor/ceil pointing at the wrong indices; the outer `* (Count-1)` then scales the wrong intermediate. The SUT had been silently miscomputing every quantile for every input since the file was written.

Step [5] fix: wrap the switch input in explicit parens to force precedence — `((sorted.Count - 1) * Math.Clamp(...)) switch { ... }`. Two-character production change. Test now passes. Entry [6] in the metamorphic catalog is `live`.

Shrunk seed: `"5jO3CrAoSXCf"` (preserved for future regression should anyone refactor Quantile).

**Why this was unreachable by hand-written unit tests:** the test author who would have written `Assert.Equal(Quantile(sorted, 0.5), sorted[2])` would have used the SAME formula in the assertion expression, hit the SAME precedence bug, and seen the test pass spuriously. Only an INDEPENDENT computation through an unrelated path — the classical sorted-middle algorithm — broke the camouflage.

[CRITICAL]:
- [NEVER] Commit a red metamorphic test "to track the bug". Failing tests in main poison the signal: every future failure has to be visually filtered against the known-failing baseline. Either fix production, document a real catalog entry, OR remove the test until the fix lands.
- [NEVER] Add `[Fact(Skip = "production bug")]` without a tracked issue or a documented catalog entry. A skipped test with no follow-up rots into dead code.
- [ALWAYS] When step [4] localizes the divergence, **trust the metamorphic test** — algebraic laws over independent oracles are the most reliable failure detector in the stack. Hand-asserted tests can pass-by-coincidence; metamorphic tests cannot.
- [ALWAYS] When you see arrow-body lambdas combining arithmetic with switch expressions, parenthesize the switch input explicitly. `switch` outranks `*`/`/`/`%`. This bug class is invisible to the eye and to the analyzer.

---
## [8][REGRESSION_SEED_PINNING]
>**Dictum:** *A shrunk failing case has a stable string identity; preserve it as the regression record.*

<br>

When CsCheck shrinks a property failure, it reports a seed string. To preserve that exact input as a regression test, use `Spec.Regression(gen, property, seed: "<reported-seed>")`. The single sample replays the exact case (no random search) and continues guarding the regression in perpetuity.

```csharp
[Fact]
public void Regression_StatVarianceUnderlowMagnitude() =>
    Spec.Regression(StatGens.NonEmptyFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key)), seed: "0_8a3f1c9b");
```

[IMPORTANT] Pinned seeds are the alternative to `[InlineData(...)]` regression blocks. They:
- Re-shrink on changes to the generator (CsCheck guarantees the seed reproduces the same value if and only if the generator shape is unchanged; if the seed drifts, the regression case lost validity and must be re-pinned)
- Live next to the algebraic property they regression-test (same file, same generator)
- Carry one-line evidence of the historical failure

[NEVER] Maintain a parallel `RegressionData.cs` with copy-pasted failing inputs. The seed IS the test data.

---
## [9][LAW_DENSITY_RULE]
>**Dictum:** *Pack laws sharing the same generator into one `[Fact]`.*

<br>

Bad — three properties, three fact methods, three Sample loops:

```csharp
[Fact] public void MinExists() => Spec.ForAll(gen, xs => { var s = Stat.Of(xs); Assert.True(s.IsSucc); });
[Fact] public void MaxExists() => Spec.ForAll(gen, xs => { var s = Stat.Of(xs); Assert.True(s.IsSucc); });
[Fact] public void MeanExists() => Spec.ForAll(gen, xs => { var s = Stat.Of(xs); Assert.True(s.IsSucc); });
```

Good — one fact, packed assertions, one Sample loop, three properties per generated case:

```csharp
[Fact]
public void StatisticsHoldOverNonEmptyFiniteInputs() =>
    Spec.ForAll(StatGens.NonEmptyFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: static s => {
        Assert.True(s.Mean >= s.Minimum - 1.0e-9 && s.Mean <= s.Maximum + 1.0e-9);
        Assert.True(s.Variance >= 0.0);
        Assert.True(s.Count > 0);
    }));
```

[IMPORTANT]: Pack ONLY when the laws share both generator AND assertion lifecycle. Different generators or independent failure modes belong in separate `[Fact]`s — the granular failure signal is more valuable than the LOC saving.
