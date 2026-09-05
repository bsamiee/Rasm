using System.Text.Json;
using Xunit.Sdk;

namespace Rasm.TestSupport;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Point(int X, int Y);

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class PropertiesTests {
    [Fact]
    public void VerifyRejectsAPredicateThatAcceptsItsCounterexample() =>
        _ = Assert.Throws<XunitException>(static () => TestAssertions.Verify(Properties.Define("vacuous", Gen.Int, static _ => { }, counterexample: 0)));

    [Fact]
    public void VerifyPassesAPredicateOverItsDomainAndALawThatHolds() {
        TestAssertions.Verify(Properties.Define("positive", Gen.Int[1, 1_000], static value => Assert.True(value > 0), counterexample: 0));
        TestAssertions.Verify(Properties.Commutative("addition", Gen.Int[-1_000, 1_000], static (a, b) => a + b));
    }

    [Fact]
    public void VerifyFailsALawThatDoesNotHold() =>
        _ = Assert.Throws<CsCheckException>(static () => TestAssertions.Verify(Properties.Commutative("subtraction", Gen.Int[-1_000, 1_000], static (a, b) => a - b)));

    [Fact]
    public void MetamorphicDetectsABrokenRelation() {
        MetamorphicRelation<int, int> negation = new("negation keeps the square", static x => -x, static (_, source, transformed) => source == transformed);
        MetamorphicRelation<int, int> broken = new("negation shifts the square", static x => -x, static (_, source, transformed) => source == transformed + 1);
        TestAssertions.Metamorphic(Gen.Int[-100, 100], static x => x * x, negation);
        _ = Assert.Throws<CsCheckException>(() => TestAssertions.Metamorphic(Gen.Int[-100, 100], static x => x * x, broken));
    }
}

public sealed class TestAssertionsTests {
    [Fact]
    public void ResultAssertionsRejectTheOppositeCase() {
        TestAssertions.Succ(Fin.Succ(1), static value => Assert.Equal(1, value));
        TestAssertions.Fail(Fin.Fail<int>(Error.New("failed")));
        TestAssertions.Some(Some(1));
        TestAssertions.None(Option<int>.None);
        _ = Assert.Throws<XunitException>(static () => TestAssertions.Succ(Fin.Fail<int>(Error.New("failed"))));
        _ = Assert.Throws<XunitException>(static () => TestAssertions.Fail(Fin.Succ(1)));
        _ = Assert.Throws<XunitException>(static () => TestAssertions.Some(Option<int>.None));
        _ = Assert.Throws<XunitException>(static () => TestAssertions.None(Some(1)));
    }

    [Fact]
    public void CaseTableNamesTheRowThatDisagrees() {
        TestAssertions.CaseTable(("true row", static () => true, true), ("false row", static () => false, false));
        TrueException failure = Assert.Throws<TrueException>(static () => TestAssertions.CaseTable(("wrong row", static () => false, true)));
        Assert.Contains("wrong row", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValueObjectsAcceptValidInputAndRejectInvalidInput() {
        TestAssertions.ValueObjects(new TestAssertions.ValueObjectCase<int, int>(Gen.Int[1, 1_000], Gen.Int[-1_000, 0], TryPositive, static value => value));
        _ = Assert.Throws<CsCheckException>(static () => TestAssertions.ValueObjects(new TestAssertions.ValueObjectCase<int, int>(Gen.Int[1, 1_000], Gen.Int[-1_000, 0], TryAny, static value => value)));
    }

    [Fact]
    public void RoundtripBytesHoldsForARecordOfIntegers() =>
        TestAssertions.RoundtripBytes(Gen.Int.Select(Gen.Int, static (x, y) => new Point(x, y)), JsonSerializerOptions.Default.GetTypeInfo<Point>());

    [Fact]
    public void ChiSquaredRejectsABiasedGenerator() {
        TestAssertions.ChiSquared(Gen.Bool, static value => value ? 1 : 0, 500, 500);
        _ = Assert.Throws<CsCheckException>(static () => TestAssertions.ChiSquared(Gen.Const(0), static value => value, 500, 500));
    }

    [Fact]
    public void KeySetRejectsDuplicateAndMissingKeys() {
        TestAssertions.KeySet([1, 2], [2, 1], static key => key);
        _ = Assert.Throws<EqualException>(static () => TestAssertions.KeySet([1, 1], [1, 2], static key => key));
        _ = Assert.Throws<EqualException>(static () => TestAssertions.KeySet([1, 2], [1, 3], static key => key));
    }

    private static bool TryPositive(int value, out int positive) {
        positive = value;
        return value > 0;
    }

    private static bool TryAny(int value, out int any) {
        any = value;
        return true;
    }
}
