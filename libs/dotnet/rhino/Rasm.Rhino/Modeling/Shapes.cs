using Rasm.RhinoCommon.Document.Units;

namespace Rasm.RhinoCommon.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveShape {
    public sealed record Linear(Line Value) : CurveShape;

    public sealed record Polyline(Rhino.Geometry.Polyline Value) : CurveShape;

    public sealed record Circle(Rhino.Geometry.Circle Value) : CurveShape;

    public sealed record Arc(Rhino.Geometry.Arc Value) : CurveShape;

    public sealed record Ellipse(Rhino.Geometry.Ellipse Value) : CurveShape;

    public sealed record Parabola(Point3d Focus) : CurveShape;

    public sealed record Hyperbola(Point3d Focus1, Point3d Focus2, Point3d Center) : CurveShape;

    public sealed record Planar(Plane Value) : CurveShape;

    public static Option<CurveShape> Recognize(Tolerances tolerances, Curve curve) =>
        curve.IsLinear(tolerances.Absolute) ? new Linear(new Line(curve.PointAtStart, curve.PointAtEnd))
        : curve.TryGetPolyline(out Rhino.Geometry.Polyline polyline) ? new Polyline(polyline)
        : curve.TryGetCircle(out Rhino.Geometry.Circle circle, tolerances.Absolute) ? new Circle(circle)
        : curve.TryGetArc(out Rhino.Geometry.Arc arc, tolerances.Absolute) ? new Arc(arc)
        : curve.TryGetEllipse(out Rhino.Geometry.Ellipse ellipse, tolerances.Absolute) ? new Ellipse(ellipse)
        : curve.GetConicSectionType(out Point3d focus1, out Point3d focus2, out Point3d center) switch {
            ConicSectionType.Parabola => new Parabola(focus1),
            ConicSectionType.Hyperbola => new Hyperbola(focus1, focus2, center),
            ConicSectionType.Unknown or ConicSectionType.Circle or ConicSectionType.Ellipse =>
                curve.TryGetPlane(out Plane plane, tolerances.Absolute) ? new Planar(plane) : Option<CurveShape>.None,
        };

    public Option<IO<Curve>> ToCurve() =>
        Switch(
            linear: static linear => Some(Products.Made<Curve>(new LineCurve(linear.Value))),
            polyline: static polyline => Some(Products.Made<Curve>(new PolylineCurve(polyline.Value))),
            circle: static circle => Some(Products.Made<Curve>(new ArcCurve(circle.Value))),
            arc: static arc => Some(Products.Made<Curve>(new ArcCurve(arc.Value))),
            ellipse: static ellipse => Some(Products.Made<Curve>(ellipse.Value.ToNurbsCurve())),
            parabola: static _ => Option<IO<Curve>>.None,
            hyperbola: static _ => Option<IO<Curve>>.None,
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
