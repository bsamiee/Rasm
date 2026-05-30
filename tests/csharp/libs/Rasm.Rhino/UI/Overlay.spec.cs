using Rasm.Rhino.UI;
using Rasm.TestKit;
using Rhino.DocObjects;

namespace Rasm.Rhino.Tests.UI;

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class OverlayFilterCases {
    // Unbind=false generator: pure right-biased Option-or is a clean monoid with the empty filter as identity.
    internal static readonly Gen<OverlayFilter> Filter =
        Gen.OneOfConst(Option<ObjectType>.None, Some(ObjectType.Curve), Some(ObjectType.Brep), Some(ObjectType.Mesh)).Select(
        Gen.OneOfConst(Option<ActiveSpace>.None, Some(ActiveSpace.ModelSpace), Some(ActiveSpace.PageSpace)),
        static (geometry, space) => new OverlayFilter(Geometry: geometry, Space: space));

    // Field-wise oracle bypassing LanguageExt equality resolution (which throws on the native Viewport/enum payloads
    // under VSTest): compare boxed option contents with built-in enum equality. The monoid is over these scalar fields.
    private static bool OptEq<T>(Option<T> x, Option<T> y) where T : struct, Enum =>
        (x.Case, y.Case) switch {
            (T xv, T yv) => EqualityComparer<T>.Default.Equals(xv, yv),
            (null, null) => true,
            _ => false,
        };
    internal static bool Eq(OverlayFilter a, OverlayFilter b) =>
        OptEq(a.Geometry, b.Geometry) && OptEq(a.Space, b.Space) && a.Unbind == b.Unbind;
}

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class OverlayFilterMonoidLaws {
    [Fact]
    public void CompositionIsAssociative() =>
        Spec.Associative(OverlayFilterCases.Filter, static (a, b) => a + b, eq: OverlayFilterCases.Eq);

    [Fact]
    public void EmptyFilterIsTwoSidedIdentity() =>
        Spec.ForAll(OverlayFilterCases.Filter, filter => {
            Spec.Holds(condition: OverlayFilterCases.Eq(new OverlayFilter() + filter, filter), label: "left identity broken");
            Spec.Holds(condition: OverlayFilterCases.Eq(filter + new OverlayFilter(), filter), label: "right identity broken");
        });

    // Reset is right-absorbing: a Reset on the right clears any composed filter.
    [Fact]
    public void ResetOnTheRightAbsorbsAnyFilter() =>
        Spec.ForAll(OverlayFilterCases.Filter, filter =>
            Spec.Holds(condition: OverlayFilterCases.Eq(filter + OverlayFilter.Reset, OverlayFilter.Reset), label: "reset not right-absorbing"));

    [Fact]
    public void RightFieldsWinOverLeft() {
        OverlayFilter merged = new OverlayFilter(Geometry: Some(ObjectType.Curve), Space: Some(ActiveSpace.ModelSpace)) + new OverlayFilter(Geometry: Some(ObjectType.Brep));
        Spec.Some(merged.Geometry, value => Assert.Equal(expected: ObjectType.Brep, actual: value));
        Spec.Some(merged.Space, value => Assert.Equal(expected: ActiveSpace.ModelSpace, actual: value));
    }
}

public sealed class OverlayDecisionFoldLaws {
    // Fold threads the monoid `+`; Cull is right-biased so the last decision wins.
    [Fact]
    public void FoldIsRightBiasedOverCull() {
        OverlayDecision folded = OverlayDecision.Fold(Seq(OverlayDecision.Ignore, OverlayDecision.CullObject(cull: true), OverlayDecision.CullObject(cull: false)));
        Spec.Some(folded.Cull, value => Spec.Holds(condition: !value, label: "fold not right-biased over cull"));
    }

    [Fact]
    public void FoldOfEmptyIsIgnore() =>
        Assert.Equal(expected: OverlayDecision.Ignore, actual: OverlayDecision.Fold(Seq<OverlayDecision>()));
}
