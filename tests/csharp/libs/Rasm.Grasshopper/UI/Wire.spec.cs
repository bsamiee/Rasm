using Rasm.Grasshopper.UI;
using Rasm.TestKit;

namespace Rasm.Grasshopper.Tests.UI;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class WireGens {
    public static readonly Gen<int> Caps = Gen.Int[start: 0, finish: 1024];
    // A flat object walk with deliberate duplicates so Distinct is load-bearing, and small element range so
    // saturation (cap >= length) is reachable.
    public static readonly Gen<int[]> Walk = Gen.Int[start: 0, finish: 12].Array[0, 48];

    // The object-count bound Wire.GraphObjects/GraphOf apply: Take(cap) over the flat walk, then Distinct.
    public static int[] Bounded(int[] walk, WireObjectLimit cap) => [.. walk.Take(count: cap.Value).Distinct()];
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class WireObjectLimitLaws {
    [Fact]
    public void AcceptsNonNegative() =>
        Spec.ValueObjectAccepts(valid: WireGens.Caps, tryCreate: static x => WireObjectLimit.TryCreate(value: x, obj: out _));

    [Fact]
    public void RejectsNegative() =>
        Spec.Cases(items: [-1, -32, int.MinValue], key: static x => x,
            law: static x => Assert.False(condition: WireObjectLimit.TryCreate(value: x, obj: out _)));

    [Fact]
    public void ValueRoundtripsAndDefaultIsThirtyTwo() {
        Spec.ValueObjectRoundtrip(validGen: WireGens.Caps, tryCreate: WireObjectLimit.TryCreate, read: static (WireObjectLimit h) => h.Value);
        Assert.Equal(expected: 32, actual: WireObjectLimit.DefaultCount);
        Assert.Equal(expected: 32, actual: WireObjectLimit.Create(value: WireObjectLimit.DefaultCount).Value);
    }
}

// Pins the object-count bounding CONTRACT that Wire.GraphObjects applies (Take-then-Distinct over a flat host
// walk) before any rename to MaxObjects: the bound is PREFIX-MONOTONE in the cap, SATURATES at the full
// distinct walk, and never yields more objects than the cap. Oracle: set containment + cardinality, independent
// of the projection. The live host-keyed walk itself is bridge-owned (needs GhObjectList).
public sealed class WireObjectBoundingLaws {
    [Fact]
    public void BoundingIsPrefixMonotone() =>
        Spec.ForAll(WireGens.Walk.Select(WireGens.Caps, WireGens.Caps), static tuple => {
            (int[] walk, int a, int b) = tuple;
            int[] small = WireGens.Bounded(walk: walk, cap: WireObjectLimit.Create(value: Math.Min(val1: a, val2: b)));
            int[] large = WireGens.Bounded(walk: walk, cap: WireObjectLimit.Create(value: Math.Max(val1: a, val2: b)));
            Assert.All(collection: small, action: x => Assert.Contains(expected: x, collection: large));
        });

    [Fact]
    public void SaturatesAtFullDistinctWalk() =>
        Spec.ForAll(WireGens.Walk, static walk =>
            Assert.Equal(expected: [.. walk.Distinct()], actual: WireGens.Bounded(walk: walk, cap: WireObjectLimit.Create(value: walk.Length))));

    [Fact]
    public void BoundedCountNeverExceedsCap() =>
        Spec.ForAll(WireGens.Walk.Select(WireGens.Caps), static tuple => {
            (int[] walk, int cap) = tuple;
            Assert.True(condition: WireGens.Bounded(walk: walk, cap: WireObjectLimit.Create(value: cap)).Length <= cap);
        });
}

public sealed class PickToleranceLaws {
    [Fact]
    public void AcceptsNonNegativeFinite() =>
        Spec.ValueObjectAccepts(valid: Gen.Double[start: 0.0, finish: 1.0e3].Select(static (double d) => (float)d),
            tryCreate: static x => PickTolerance.TryCreate(value: x, obj: out _));

    [Fact]
    public void RejectsNegativeAndNonFinite() =>
        Spec.Cases(items: [-1f, -0.001f, float.NaN, float.PositiveInfinity, float.NegativeInfinity], key: static x => x,
            law: static x => Assert.False(condition: PickTolerance.TryCreate(value: x, obj: out _)));
}
