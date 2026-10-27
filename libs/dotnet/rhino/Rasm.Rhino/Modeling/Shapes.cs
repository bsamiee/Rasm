using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveShape {
    public sealed record Linear(Line Value) : CurveShape;

    public sealed record Polygonal(Polyline Value) : CurveShape;

    public sealed record Circular(Circle Value) : CurveShape;

    public sealed record Arcuate(Arc Value) : CurveShape;

    public sealed record Elliptic(Ellipse Value) : CurveShape;

    public sealed record Parabolic(Point3d Focus) : CurveShape;

    public sealed record Hyperbolic(Point3d Focus1, Point3d Focus2, Point3d Center) : CurveShape;

    public sealed record Planar(Plane Value) : CurveShape;

    public static Option<CurveShape> Recognize(Tolerances tolerances, Curve curve) =>
        curve.IsLinear(tolerances.Absolute) ? new Linear(new Line(curve.PointAtStart, curve.PointAtEnd))
        : curve.TryGetPolyline(out Polyline polyline) ? new Polygonal(polyline)
        : curve.TryGetCircle(out Circle circle, tolerances.Absolute) ? new Circular(circle)
        : curve.TryGetArc(out Arc arc, tolerances.Absolute) ? new Arcuate(arc)
        : curve.TryGetEllipse(out Ellipse ellipse, tolerances.Absolute) ? new Elliptic(ellipse)
        : curve.GetConicSectionType(out Point3d focus1, out Point3d focus2, out Point3d center) switch {
            ConicSectionType.Parabola => new Parabolic(focus1),
            ConicSectionType.Hyperbola => new Hyperbolic(focus1, focus2, center),
            ConicSectionType.Unknown or ConicSectionType.Circle or ConicSectionType.Ellipse =>
                curve.TryGetPlane(out Plane plane, tolerances.Absolute) ? new Planar(plane) : Option<CurveShape>.None,
        };

    public Option<IO<Curve>> ToCurve() =>
        Switch(
            linear: static linear => Some(Products.Made<Curve>(new LineCurve(linear.Value))),
            polygonal: static polygonal => Some(Products.Made<Curve>(new PolylineCurve(polygonal.Value))),
            circular: static circular => Some(Products.Made<Curve>(new ArcCurve(circular.Value))),
            arcuate: static arcuate => Some(Products.Made<Curve>(new ArcCurve(arcuate.Value))),
            elliptic: static elliptic => Some(Products.Made<Curve>(elliptic.Value.ToNurbsCurve())),
            parabolic: static _ => Option<IO<Curve>>.None,
            hyperbolic: static _ => Option<IO<Curve>>.None,
            planar: static _ => Option<IO<Curve>>.None);
}

[Union<Cone, Cylinder, Sphere, Torus>(SkipEqualityComparison = true, SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class AnalyticShape {
    public static Option<AnalyticShape> Recognize(Tolerances tolerances, Surface surface) =>
        surface.TryGetSphere(out Sphere sphere, tolerances.Absolute) ? Some<AnalyticShape>(sphere)
        : surface.TryGetFiniteCylinder(out Cylinder cylinder, tolerances.Absolute) ? Some<AnalyticShape>(cylinder)
        : surface.TryGetCone(out Cone cone, tolerances.Absolute) ? Some<AnalyticShape>(cone)
        : surface.TryGetTorus(out Torus torus, tolerances.Absolute) ? Some<AnalyticShape>(torus)
        : Option<AnalyticShape>.None;

    public IO<NurbsSurface> ToNurbsSurface() =>
        Switch(
            cone: static cone => Products.Made(cone.ToNurbsSurface()),
            cylinder: static cylinder => Products.Made(cylinder.ToNurbsSurface()),
            sphere: static sphere => Products.Made(sphere.ToNurbsSurface()),
            torus: static torus => Products.Made(torus.ToNurbsSurface()));

    public IO<RevSurface> ToRevSurface() =>
        Switch(
            cone: static cone => Products.Made(cone.ToRevSurface()),
            cylinder: static cylinder => Products.Made(cylinder.ToRevSurface()),
            sphere: static sphere => Products.Made(sphere.ToRevSurface()),
            torus: static torus => Products.Made(torus.ToRevSurface()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SurfaceShape {
    public sealed record Planar(Plane Value) : SurfaceShape;

    public sealed record Analytic(AnalyticShape Value) : SurfaceShape;

    public sealed record Extruded(Extrusion Value) : SurfaceShape;

    public static IO<Option<SurfaceShape>> Recognize(Tolerances tolerances, Surface surface) =>
        (surface.TryGetPlane(out Plane plane, tolerances.Absolute)
            ? new Planar(plane)
            : AnalyticShape.Recognize(tolerances, surface).Map(static analytic => (SurfaceShape)new Analytic(analytic))).Match(
            Some: static shape => IO.pure(Some(shape)),
            None: () => IO.lift(() => (Found: surface.TryGetExtrusion(out Extrusion extrusion, tolerances.Absolute), Extrusion: extrusion))
                .Bind(static attempt => attempt.Found
                    ? Products.Held(attempt.Extrusion)
                    : IO.lift(() => {
                        using Extrusion? discarded = attempt.Extrusion;
                        return Option<Extrusion>.None;
                    }))
                .Map(static extruded => extruded.Map(static extrusion => (SurfaceShape)new Extruded(extrusion))));
}
