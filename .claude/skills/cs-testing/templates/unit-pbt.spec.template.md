# [H1][CSHARP_UNIT_PBT_SPEC_TEMPLATE]
>**Dictum:** *One generated law should hit many independent axes.*

<br>

[IMPORTANT] Replace placeholders, keep normal specs near 175 LOC, and move native Rhino/GH behavior to `*.verify.csx`.

```csharp
using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class <Source>Gens {
    public static readonly Op Key = Op.Of(name: "<source>-test");
    public static readonly Gen<<CaseType>> Case = Gen.OneOfConst(<CaseType>.A, <CaseType>.B);
    public static readonly Gen<<ValueObject>> Value = Gens.<FactoryRoutedValue>;
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class <Concept>Laws {
    [Fact]
    public void FactoriesCloseOverValidAndInvalidDomains() {
        Spec.ForAll(<Source>Gens.Value, value => Assert.True(value.Value is >= <Lo> and <= <Hi>));
        Spec.ForAll(Gens.NonFinite, value => Assert.False(<ValueObject>.TryCreate(value: value, obj: out _)));
    }

    [Fact]
    public void CasesOwnKeysOutputsAndUnsupportedRails() {
        <CaseType>[] all = [<CaseType>.A, <CaseType>.B];
        Spec.SmartEnumKeysUnique(items: all, key: static item => item.Key);
        Spec.ForAll(<Source>Gens.Case, item => {
            Spec.Succ(item.Project<<Supported>>(source: <Fixture>, key: <Source>Gens.Key));
            Spec.Fail(item.Project<<Unsupported>>(source: <Fixture>, key: <Source>Gens.Key));
        });
    }

    [Fact]
    public void MetamorphicOracleMatchesIndependentPath() =>
        Spec.Metamorphic(
            gen: <Source>Gens.<Input>,
            path: static input => <Sut>(input),
            oracle: static input => <IndependentOracle>(input),
            eq: Gens.Approx(relativeTolerance: 1.0e-9));
}
```
