using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveShape {
    public sealed record Linear(Line Value) : CurveShape;

    public sealed record Polyline(global::Rhino.Geometry.Polyline Value) : CurveShape;

    public sealed record Circle(global::Rhino.Geometry.Circle Value) : CurveShape;

    public sealed record Arc(global::Rhino.Geometry.Arc Value) : CurveShape;

    public sealed record Ellipse(global::Rhino.Geometry.Ellipse Value) : CurveShape;

    public sealed record Parabola(Point3d Focus) : CurveShape;

    public sealed record Hyperbola(Point3d Focus1, Point3d Focus2, Point3d Center) : CurveShape;

    public sealed record Planar(Plane Value) : CurveShape;

    public static Option<CurveShape> Recognize(Tolerances tolerances, Curve curve) =>
        curve.IsLinear(tolerances.Absolute) ? new Linear(new Line(curve.PointAtStart, curve.PointAtEnd))
        : curve.TryGetPolyline(out global::Rhino.Geometry.Polyline polyline) ? new Polyline(polyline)
        : curve.TryGetCircle(out global::Rhino.Geometry.Circle circle, tolerances.Absolute) ? new Circle(circle)
        : curve.TryGetArc(out global::Rhino.Geometry.Arc arc, tolerances.Absolute) ? new Arc(arc)
        : curve.TryGetEllipse(out global::Rhino.Geometry.Ellipse ellipse, tolerances.Absolute) ? new Ellipse(ellipse)
        : curve.GetConicSectionType(out Point3d focus1, out Point3d focus2, out Point3d center) switch {
            ConicSectionType.Parabola => new Parabola(focus1),
            ConicSectionType.Hyperbola => new Hyperbola(focus1, focus2, center),
            ConicSectionType.Unknown or ConicSectionType.Circle or ConicSectionType.Ellipse =>
                curve.TryGetPlane(out Plane plane, tolerances.Absolute) ? new Planar(plane) : Option<CurveShape>.None,
        };

    public Option<IO<Curve>> ToCurve(Action<IDisposable> retain) =>
        Switch(
            retain,
            linear: static (held, linear) => Some(Copies.Acquire(nameof(LineCurve), _ => Answer.One<Curve>(new LineCurve(linear.Value)), held)),
            polyline: static (held, polyline) => Some(Copies.Acquire(nameof(PolylineCurve), _ => Answer.One<Curve>(new PolylineCurve(polyline.Value)), held)),
            circle: static (held, circle) => Some(Copies.Acquire(nameof(ArcCurve), _ => Answer.One<Curve>(new ArcCurve(circle.Value)), held)),
            arc: static (held, arc) => Some(Copies.Acquire(nameof(ArcCurve), _ => Answer.One<Curve>(new ArcCurve(arc.Value)), held)),
            ellipse: static (held, ellipse) => Some(Copies.Acquire(nameof(global::Rhino.Geometry.Ellipse.ToNurbsCurve), _ => Answer.One<Curve>(ellipse.Value.ToNurbsCurve()), held)),
            parabola: static (_, _) => Option<IO<Curve>>.None,
            hyperbola: static (_, _) => Option<IO<Curve>>.None,
            planar: static (_, _) => Option<IO<Curve>>.None);
}

[Union<Cone, Cylinder, Sphere, Torus>(SkipEqualityComparison = true, SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class AnalyticShape {
    public static Option<AnalyticShape> Recognize(Tolerances tolerances, Surface surface) =>
        surface.TryGetSphere(out Sphere sphere, tolerances.Absolute) ? Some<AnalyticShape>(sphere)
        : surface.TryGetFiniteCylinder(out Cylinder cylinder, tolerances.Absolute) ? Some<AnalyticShape>(cylinder)
        : surface.TryGetCone(out Cone cone, tolerances.Absolute) ? Some<AnalyticShape>(cone)
        : surface.TryGetTorus(out Torus torus, tolerances.Absolute) ? Some<AnalyticShape>(torus)
        : Option<AnalyticShape>.None;

    public IO<NurbsSurface> ToNurbsSurface(Action<IDisposable> retain) =>
        Switch(
            retain,
            cone: static (held, cone) => Copies.Acquire(nameof(Cone.ToNurbsSurface), _ => Answer.One(cone.ToNurbsSurface()), held),
            cylinder: static (held, cylinder) => Copies.Acquire(nameof(Cylinder.ToNurbsSurface), _ => Answer.One(cylinder.ToNurbsSurface()), held),
            sphere: static (held, sphere) => Copies.Acquire(nameof(Sphere.ToNurbsSurface), _ => Answer.One(sphere.ToNurbsSurface()), held),
            torus: static (held, torus) => Copies.Acquire(nameof(Torus.ToNurbsSurface), _ => Answer.One(torus.ToNurbsSurface()), held));

    public IO<RevSurface> ToRevSurface(Action<IDisposable> retain) =>
        Switch(
            retain,
            cone: static (held, cone) => Copies.Acquire(nameof(Cone.ToRevSurface), _ => Answer.One(cone.ToRevSurface()), held),
            cylinder: static (held, cylinder) => Copies.Acquire(nameof(Cylinder.ToRevSurface), _ => Answer.One(cylinder.ToRevSurface()), held),
            sphere: static (held, sphere) => Copies.Acquire(nameof(Sphere.ToRevSurface), _ => Answer.One(sphere.ToRevSurface()), held),
            torus: static (held, torus) => Copies.Acquire(nameof(Torus.ToRevSurface), _ => Answer.One(torus.ToRevSurface()), held));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SurfaceShape {
    public sealed record Planar(Plane Value) : SurfaceShape;

    public sealed record Analytic(AnalyticShape Value) : SurfaceShape;

    public sealed record Extruded(Extrusion Value) : SurfaceShape;

    public static IO<Option<SurfaceShape>> Recognize(Tolerances tolerances, Surface surface, Action<IDisposable> retain) =>
        (surface.TryGetPlane(out Plane plane, tolerances.Absolute)
            ? new Planar(plane)
            : AnalyticShape.Recognize(tolerances, surface).Map(static analytic => (SurfaceShape)new Analytic(analytic))).Match(
            Some: static shape => IO.pure(Some(shape)),
            None: () => Copies.Acquire(nameof(Surface.TryGetExtrusion), _ => surface.TryGetExtrusion(out Extrusion extrusion, tolerances.Absolute)
                ? Answer.Optional(extrusion)
                : Answer.Successor(extrusion, static _ => Answer.Optional<Extrusion>(product: null)), retain)
                .Map(static extruded => extruded.Map(static extrusion => (SurfaceShape)new Extruded(extrusion))));
}
