# [H1][UNIT_PBT_TEMPLATE]
>**Dictum:** *Copy the shape; replace `<Source>`, `<Module>`, and the laws.*

<br>

Template for a static-rail unit PBT spec. Live reference: `tests/csharp/libs/Rasm/Vectors/Atoms.spec.cs` (32 tests, 15 LOC of generators, 90% coverage of Atoms.cs's pure-managed surface).

---
## [1][CANONICAL_LAYOUT]

```csharp
using Rasm.<Module>;
using Rasm.TestKit;
// using Rhino;             // only if test uses RhinoMath / pure-managed RhinoCommon
// using Rhino.Geometry;    // only if test uses Point3d / Vector3d constants
using Xunit.Sdk;

namespace Rasm.Tests.<Module>;

// --- [CONSTANTS] ----------------------------------------------------------------------------
[System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Design", checkId: "CA1515", Justification = "xUnit discovers public test surface.")]
public static class <Module>Gens {
    public static readonly Op Key = Op.Of(name: "<module>-test");

    // Generators ROUTE THROUGH production factories. No parallel construction paths.
    public static readonly Gen<<SmartEnumOrUnion>> Cases = Gen.OneOfConst(
        <SmartEnumOrUnion>.<CaseA>, <SmartEnumOrUnion>.<CaseB>);

    public static readonly Gen<<ValueObject>> Valid = Gens.<UnderlyingGen>.Select(static (<Underlying> raw) =>
        <ValueObject>.TryCreate(value: raw, obj: out <ValueObject> v)
            ? v
            : throw new InvalidOperationException("generator invariant broken: <ValueObject>"));
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class <ValueObject>Props {
    [Fact]
    public void ClosureRejectsOutsideDomain() =>
        Spec.ForAll(Gens.<Underlying>.Where(static x => <invalidPredicate>),
            static x => Assert.False(<ValueObject>.TryCreate(value: x, obj: out _)));

    [Fact]
    public void ClosureAcceptsInsideDomain() =>
        Spec.ForAll(Gens.<Underlying>.Where(static x => <validPredicate>),
            static x => Assert.True(<ValueObject>.TryCreate(value: x, obj: out _)));

    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.Roundtrip(<Module>Gens.Valid,
            forward: static (<ValueObject> v) => v.Value,
            back: static (<Underlying> x) => <ValueObject>.TryCreate(value: x, obj: out <ValueObject> v)
                ? v
                : throw new InvalidOperationException("roundtrip lost value"));
}

public sealed class <SmartEnumOrUnion>Laws {
    [Fact] public void <CaseA>HasExpectedKey() => Assert.Equal(expected: <key>, actual: <SmartEnumOrUnion>.<CaseA>.Key);
    [Fact] public void <CaseB>HasExpectedKey() => Assert.Equal(expected: <key>, actual: <SmartEnumOrUnion>.<CaseB>.Key);

    [Fact]
    public void InvariantHoldsAcrossEveryCase() =>
        Spec.ForAll(<Module>Gens.Cases, static c => Spec.EqualWithin(
            left: <invariantProjection>(c), right: <expectedConstant>, tolerance: 0.0, what: "<invariant name>"));
}
```

---
## [2][CHECKPOINTS]

Before submitting:

- [ ] Spec file ≤ 175 LOC (`wc -l`).
- [ ] All test method names PascalCase (no underscores; CA1707).
- [ ] `[Fact]` / `[Theory]` on its own line above the method declaration.
- [ ] Imports: `Rasm.<Module>`, `Rasm.TestKit`, optional `Rhino*` for pure-managed Rhino types, `Xunit.Sdk` only if `XunitException` is thrown.
- [ ] All generators in `<Module>Gens` route through production `TryCreate`/`Validate`/`Of`.
- [ ] No `[SuppressMessage]` for CA rules — fix the underlying signal.
- [ ] No `if`/`else`/`for`/`foreach`/`try`/`catch` in test bodies — use `switch` expressions, ternary, pattern matching.
- [ ] Pure-managed only — `wc -l $(grep -l 'IsTiny\|VectorAngle\|UnitScale\|Brep\b' source.cs)` of the source returns zero, OR the test goes to bridge rail.
- [ ] All asserts have an actionable message (`what:` parameter on `Spec.EqualWithin`, third arg on `Assert.True`).
- [ ] `bash scripts/test.sh "FullyQualifiedName~<TestClass>"` passes.

---
## [3][COMMON_VARIANTS]

**Closure for a `[ValueObject<double>]` with `[0, range]` constraint:**

```csharp
[Fact]
public void ClosureRejectsOutsideRange() =>
    Spec.ForAll(Gens.Finite.Where(static x => x is < 0.0 or > <upperBound>),
        static x => Assert.False(<VO>.TryCreate(value: x, obj: out _)));
```

**Algebraic law over a binary operator with floating-point equality:**

```csharp
[Fact]
public void OperatorIsCommutative() =>
    Spec.Commutative(<Module>Gens.Valid,
        op: static (<T> a, <T> b) => a.<Op>(b),
        eq: static (<T> l, <T> r) => Math.Abs(l.Value - r.Value) < 1.0e-9);
```

**Fin-rail closure (invalid input):**

```csharp
[Fact]
public void RejectsZeroVector() =>
    Spec.Fail(Rasm.<Module>.<Operation>.Of(value: <zero>, key: <Module>Gens.Key));
```

**Packed assertions in one Sample loop:**

```csharp
[Fact]
public void InvariantsHoldOverGeneratedDomain() =>
    Spec.ForAll(<Module>Gens.NonEmpty, xs => Spec.Succ(
        <Operation>.Of(values: xs, key: <Module>Gens.Key),
        then: static result => {
            Assert.True(result.Property1 >= 0.0, "Property1 ≥ 0");
            Assert.True(result.Property2 <= result.Property3, "Property2 ≤ Property3");
            Assert.Equal(expected: xs.Count, actual: result.Count);
        }));
```

**Metamorphic law against an independent oracle** (strongest density-per-LOC technique):

```csharp
[Fact]
public void <Operation>MatchesIndependentOracle() =>
    Spec.Metamorphic(<Module>Gens.<Generator>,
        path:   static (<Input> x) => <Operation>.Of(x).Match(Succ: r => r.Value, Fail: _ => double.NaN),
        oracle: static (<Input> x) => System.Linq.Enumerable.<IndependentAggregation>(x.AsIterable()),
        eq:     Gens.Approx(relativeTolerance: 1.0e-6));
```

The oracle must reach the same answer by a different code path (LINQ, closed-form formula, RFC vector, etc.). Live example: `tests/csharp/libs/Rasm/Domain/Stats.spec.cs::MeanMatchesLinqAverage`.

**Order-invariance via `Spec.Permutation`** (uses `Gen.Shuffle` internally):

```csharp
[Fact]
public void <Aggregation>IsPermutationInvariant() =>
    Spec.Permutation(Gens.NonEmptyArray(Gens.Finite),
        f:  static (double[] xs) => <Aggregation>(xs),
        eq: Gens.Approx(1.0e-9));
```

**Monotonicity via `Spec.Monotone`** (consumes ordered pairs):

```csharp
[Fact]
public void <Projection>IsMonotone() =>
    Spec.Monotone(Gens.OrderedPair(Gens.Finite),
        projection: static (double x) => <Projection>(x));
```

**Regression seed pin** (after CsCheck shrinks a failure, capture the seed string):

```csharp
[Fact]
public void Regression_<Issue>UnderlowMagnitude() =>
    Spec.Regression(<Module>Gens.<Generator>,
        property: static xs => Spec.Succ(<Operation>.Of(xs)),
        seed: "0_8a3f1c9b");
```

**Explicit perf rail** (excluded from normal runs; run via `dotnet test --filter "Explicit=true"`):

```csharp
[Fact(Explicit = true)]
public void <Operation>IsFasterThanBaseline() {
    Action baseline = () => <BaselineImpl>(input);
    Action candidate = () => <Operation>(input);
    candidate.Faster(baseline, sigma: 6);  // CsCheck extension; throws on regression
}
```
