# [H1][DENSITY]
>**Dictum:** *Generation multipliers determine LOC efficiency; pick the highest multiplier that fits.*

<br>

The 175 LOC cap is a forcing function: average ≥5 properties per 100 LOC of spec, ≥100 generated cases per property. The techniques below stack — a single `Spec.ForAll(gen, packedBody)` yields `assertions × 100 = O(thousands)` of effective assertions per line.

---
## [1][TECHNIQUE_TABLE]

| [INDEX] | [TECHNIQUE]                                        | [MULTIPLIER]      | [USE_WHEN]                                                                       |
| :-----: | -------------------------------------------------- | ----------------- | -------------------------------------------------------------------------------- |
|   [1]   | `Spec.ForAll(gen, packed Action)`                  | assertions × 100  | Multiple properties share one input shape and one failure mode                   |
|   [2]   | `Spec.Metamorphic(gen, path, oracle)`              | 100 cases × indep oracle | SUT result ≡ external algorithm result (LINQ, RFC vector, closed form)    |
|   [3]   | `Gen.OneOfConst(case₁, ..., caseₙ)`                | n × 1             | Bounded vocabularies (SmartEnum, DU cases, fixed instances)                      |
|   [4]   | `gen.Select(gen).Sample((a, b) => …)`              | 100 × 100 pairs   | Binary algebraic laws (commutative, associative)                                 |
|   [5]   | `gen.Select(gen, gen).Sample((a, b, c) =>`         | 100 triples       | Ternary laws (associativity, distributivity)                                     |
|   [6]   | `Gen.Frequency([(w₁,g₁), …])`                      | weighted          | Bias distribution toward boundaries                                              |
|   [7]   | `Gen.Recursive`                                    | depth-bounded     | Tree / recursive structure generation                                            |
|   [8]   | Generator routed through smart constructor         | infinite domain   | Production-fidelity arbitraries with shrink-stability                            |
|   [9]   | `Spec.Regression(gen, property, seed)`             | 1 fixed case      | Pin a previously-shrunk failing input by its seed string                         |
|  [10]   | `action.Faster(other, sigma: 6)` inside `[Fact(Explicit=true)]` | paired-sample t-test | Perf-regression rail without BenchmarkDotNet; statistical significance built in |

---
## [2][PACKING_PATTERN]
>**Dictum:** *One Sample call, multiple assertions, one shrink trail.*

<br>

The packed body asserts multiple invariants in sequence. CsCheck shrinks on the first thrown assertion — the failure message points at the specific predicate that broke. No need to split into N separate `[Fact]`s.

```csharp
[Fact]
public void StatisticsHoldOverNonEmptyFiniteInputs() =>
    Spec.ForAll(StatGens.NonEmptyFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: static s => {
        Assert.True(s.Mean >= s.Minimum - 1.0e-9 && s.Mean <= s.Maximum + 1.0e-9, "Mean ∈ [Min, Max]");
        Assert.True(s.Variance >= 0.0, "Variance ≥ 0");
        Assert.True(s.Count > 0, "Count > 0");
    }));
```

[IMPORTANT]: Pass a `message` to `Assert.True` so the shrunk failure identifies which invariant broke. Without it the test reports "Assert.True failed" and you re-derive context manually.

---
## [3][CASE_EXHAUSTIVE_PATTERN]
>**Dictum:** *Use `Gen.OneOfConst` for closed vocabularies; never re-list cases in inline data.*

<br>

```csharp
public static readonly Gen<SignedAxis> Axis = Gen.OneOfConst(
    SignedAxis.PositiveX, SignedAxis.NegativeX,
    SignedAxis.PositiveY, SignedAxis.NegativeY,
    SignedAxis.PositiveZ, SignedAxis.NegativeZ);

[Fact]
public void WorldAxisIsUnitLength() =>
    Spec.ForAll(AtomGens.Axis, static a => Spec.EqualWithin(left: a.World.Length, right: 1.0, tolerance: 1.0e-12, what: "axis world length"));
```

[CRITICAL]:
- [NEVER] Forget a case in `Gen.OneOfConst`. When a new case is added to the production SmartEnum, the spec MUST add the case to the generator. Static analysis won't catch this.
- [ALWAYS] Co-locate the case generator with the spec that uses it (`<Module>Gens` static class at top of spec). Generators that span modules belong in `Rasm.TestKit/Gens.cs`.

---
## [4][BINARY_AND_TERNARY_LAWS]
>**Dictum:** *Tuple generators replace nested loops.*

<br>

```csharp
[Fact]
public void AdditionIsAssociativeOverFiniteDoubles() =>
    Spec.Associative(Gens.Finite, op: static (double a, double b) => a + b,
        eq: static (double l, double r) => Math.Abs(l - r) < 1.0e-9);

[Fact]
public void ToleranceUsesMaximumAbsoluteBound() =>
    Spec.ForAll(Gens.Positive.Select(Gens.Finite, Gens.Finite), tuple => {
        (double tolerance, double min, double max) = tuple;
        StatContext c = StatContext.Tolerance(tolerance: tolerance, minimum: min, maximum: max);
        bool expected = Math.Max(Math.Abs(min), Math.Abs(max)) <= tolerance;
        Assert.True(c is StatContext.ToleranceCase t && t.WithinTolerance == expected);
    });
```

[IMPORTANT]: When the equality is approximate (floating-point), pass an explicit `eq` predicate to `Spec.Commutative` / `Spec.Associative`. Default `EqualityComparer<T>.Default` uses bit-equality on `double`, which fails on rounding.

---
## [5][GENERATOR_REFINEMENT]
>**Dictum:** *Filter at the generator, not inside the property.*

<br>

```csharp
// Wrong — filters inside the property, wastes 99% of generated cases:
[Fact]
public void NonZeroVectorHasUnitDirection() =>
    Spec.ForAll(Gens.Vec, v => {
        if (v.Length < 1e-9) return;  // skipped, but CsCheck still counts the case
        // ...
    });

// Right — generator already refines the domain:
public static readonly Gen<Vector3d> NonZeroVec = Vec.Where(static v => v.Length > 1.0e-6);
[Fact]
public void NonZeroVectorHasUnitDirection() =>
    Spec.ForAll(Gens.NonZeroVec, v => /* every case is non-zero */);
```

[CRITICAL]: `Gen.Where` with a predicate that rejects most inputs leads to exhaustion of CsCheck's generation budget. If a refinement is rare (<10% acceptance), construct the valid value directly inside `Select` rather than filtering.

---
## [6][LOC_BUDGET]
>**Dictum:** *Plan LOC before authoring; collapse when over.*

<br>

| [SECTION]        | [LOC_BUDGET] |
| ---------------- | ------------ |
| imports          | 4-7          |
| `[CONSTANTS]`    | 10-25        |
| `[ALGEBRAIC]`    | 80-130       |
| `[EDGE_CASES]`   | 0-30         |
| **Total cap**    | **175**      |

When the spec exceeds 175 LOC:
1. **Pack** independent properties sharing one generator into a single packed `[Fact]`.
2. **Move** edge cases that share a generator with algebra into the algebra section.
3. **Split** only when distinct concept (e.g. one spec per source `.cs` file).
4. **Promote** generator definitions to `Rasm.TestKit/Gens.cs` when used by 2+ specs.
