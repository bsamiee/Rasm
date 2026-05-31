using Rasm.Rhino.UI;
using Rasm.TestKit;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using GumballMode = Rhino.UI.Gumball.GumballMode;

namespace Rasm.Rhino.Tests.UI;

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class OverlayFilterCases {
    // Unbind=false generator: pure right-biased Option-or is a clean monoid with the empty filter as identity.
    internal static readonly Gen<OverlayFilter> Filter =
        Gen.OneOfConst(Option<ObjectType>.None, Some(ObjectType.Curve), Some(ObjectType.Brep), Some(ObjectType.Mesh)).Select(
        Gen.OneOfConst(Option<ActiveSpace>.None, Some(ActiveSpace.ModelSpace), Some(ActiveSpace.PageSpace)),
        static (geometry, space) => new OverlayFilter(Geometry: geometry, Space: space));

    // Field-wise oracle bypassing LanguageExt equality resolution (which throws on native Viewport/enum payloads
    // under the managed test runner): compare boxed option contents with built-in enum equality. The monoid is over these scalar fields.
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

    [Fact]
    public void BoundsUnionAndPaintCompositionAccumulateIndependentDecisions() {
        int paints = 0;
        OverlayDecision left = OverlayDecision.Include(new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1, y: 1, z: 1)))
            .IfFail(error => throw new InvalidOperationException(message: error.Message))
            + OverlayDecision.Paint(draw: _ => { paints++; return Fin.Succ(value: unit); })
                .IfFail(error => throw new InvalidOperationException(message: error.Message));
        OverlayDecision right = OverlayDecision.Include(new BoundingBox(min: new Point3d(x: 2, y: 2, z: 2), max: new Point3d(x: 3, y: 3, z: 3)))
            .IfFail(error => throw new InvalidOperationException(message: error.Message))
            + OverlayDecision.Paint(draw: _ => { paints++; return Fin.Succ(value: unit); })
                .IfFail(error => throw new InvalidOperationException(message: error.Message));
        OverlayDecision merged = left + right;

        Spec.Some(merged.Bounds, bounds => {
            Assert.Equal(expected: Point3d.Origin, actual: bounds.Min);
            Assert.Equal(expected: new Point3d(x: 3, y: 3, z: 3), actual: bounds.Max);
        });
        Spec.Some(merged.Draw, draw => Spec.Succ(result: draw(arg: null!), then: static _ => { }));
        Assert.Equal(expected: 2, actual: paints);
    }
}

public sealed class OverlayPhaseLaws {
    [Fact]
    public void PhaseCatalogClassifiesDrawAndBoundingLifecycle() {
        Spec.Cases(items: OverlayPhase.Items, key: static phase => phase.Key, law: static phase => {
            bool draws = phase == OverlayPhase.Foreground || phase == OverlayPhase.Overlay || phase == OverlayPhase.PostDraw;
            bool bounds = phase == OverlayPhase.Bounds || phase == OverlayPhase.ZoomBounds;
            Assert.Equal(expected: draws, actual: phase.Draws);
            Assert.Equal(expected: bounds, actual: phase.Bounding);
        });
    }

    [Fact]
    public void MouseWheelIsTheOnlyNonViewportNativeMousePhase() {
        Spec.Cases(items: MousePhase.Items, key: static phase => phase.Key, law: static phase =>
            Assert.Equal(expected: phase != MousePhase.Wheel, actual: phase.ViewportNative));
    }
}

public sealed class UiGradientAxisLaws {
    [Fact]
    public void LongestEdgeSelectsDominantAxisAndExplicitAxesProjectThroughBoundsCentre() {
        BoundingBox box = new(min: new Point3d(x: 0, y: 10, z: 20), max: new Point3d(x: 2, y: 18, z: 23));
        (Point3d y0, Point3d y1) = UiGradient.Of(from: System.Drawing.Color.Red, to: System.Drawing.Color.Blue).AxisOf(box: box);
        (Point3d x0, Point3d x1) = new UiGradient(Stops: Seq<ColorStop>(), Axis: UiGradientAxis.X).AxisOf(box: box);

        Assert.Equal(expected: new Point3d(x: 1, y: 10, z: 21.5), actual: y0);
        Assert.Equal(expected: new Point3d(x: 1, y: 18, z: 21.5), actual: y1);
        Assert.Equal(expected: new Point3d(x: 0, y: 14, z: 21.5), actual: x0);
        Assert.Equal(expected: new Point3d(x: 2, y: 14, z: 21.5), actual: x1);
    }
}

public sealed class GumballActionLaws {
    private static UiGumballSnapshot Snapshot(GumballMode mode) =>
        new(
            PreTransform: Transform.Identity,
            GumballTransform: Transform.Identity,
            TotalTransform: Transform.Identity,
            Mode: mode,
            InRelocate: false);

    [Fact]
    public void NativeModesProjectThroughActionTable() {
        Seq<(GumballMode Mode, GumballVerb Verb, GumballAxis Axis)> cases = Seq(
            (GumballMode.Menu, GumballVerb.Menu, GumballAxis.None),
            (GumballMode.TranslateFree, GumballVerb.Translate, GumballAxis.Free),
            (GumballMode.TranslateXY, GumballVerb.Translate, GumballAxis.XY),
            (GumballMode.ScaleZ, GumballVerb.Scale, GumballAxis.Z),
            (GumballMode.RotateY, GumballVerb.Rotate, GumballAxis.Y),
            (GumballMode.ExtrudeX, GumballVerb.Extrude, GumballAxis.X),
            (GumballMode.CutZ, GumballVerb.Cut, GumballAxis.Z));

        _ = cases.Iter(static row => {
            GumballAction action = Snapshot(mode: row.Mode).Action;
            Spec.Holds(condition: action.Verb == row.Verb && action.Axis == row.Axis, label: $"{row.Mode} mapped to {action}");
        });
        Spec.Holds(condition: !Snapshot(mode: GumballMode.None).Action.Active, label: "None mode did not fall back to inactive action");
    }
}
