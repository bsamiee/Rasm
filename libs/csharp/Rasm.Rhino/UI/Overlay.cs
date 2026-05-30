using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Rasm.Analysis;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MousePhase {
    // ViewportNative gates Rhino.UI.MouseCallback delivery: Wheel has no native On* hook (Eto-canvas only).
    public static readonly MousePhase
        Move = new(key: 0, viewportNative: true), MoveEnd = new(key: 1, viewportNative: true), Down = new(key: 2, viewportNative: true), DownEnd = new(key: 3, viewportNative: true),
        Up = new(key: 4, viewportNative: true), UpEnd = new(key: 5, viewportNative: true), DoubleClick = new(key: 6, viewportNative: true), Enter = new(key: 7, viewportNative: true),
        Hover = new(key: 8, viewportNative: true), Leave = new(key: 9, viewportNative: true), Wheel = new(key: 10, viewportNative: false);
    public bool ViewportNative { get; }
}
[SmartEnum<int>]
public sealed partial class OverlayPhase {
    // Draws => pipeline paint phases; Bounding => bounding-box calculation phases. Self-classifying single source of truth.
    public static readonly OverlayPhase
        Enabled = new(key: 0, draws: false, bounding: false), Cull = new(key: 1, draws: false, bounding: false),
        PreDrawObjects = new(key: 2, draws: false, bounding: false), PreDrawObject = new(key: 3, draws: false, bounding: false),
        Foreground = new(key: 4, draws: true, bounding: false), Overlay = new(key: 5, draws: true, bounding: false), PostDraw = new(key: 6, draws: true, bounding: false),
        Bounds = new(key: 7, draws: false, bounding: true), ZoomBounds = new(key: 8, draws: false, bounding: true);
    public bool Draws { get; }
    public bool Bounding { get; }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiDirection(Point3d Tip, Vector3d Direction);

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MouseContext<TState>(MousePhase Phase, TState State, global::Rhino.UI.MouseCallbackEventArgs Args) {
    public bool Cancelled => Args.Cancel;
    public bool CanCancelNative => Phase == MousePhase.Move || Phase == MousePhase.Down || Phase == MousePhase.Up || Phase == MousePhase.DoubleClick;
    public Point2d CursorLocation => global::Rhino.UI.MouseCursor.Location;
    public global::Rhino.UI.Gumball.GumballMode GumballMode => Args.IsOverGumball();
    public bool IsOverGumball => GumballMode != global::Rhino.UI.Gumball.GumballMode.None;
    public global::Rhino.UI.MouseButton MouseButton => Args.MouseButton;
    public bool Shift => Args.ShiftKeyDown;
    public bool Control => Args.CtrlKeyDown;
    public Option<RhinoView> View => Optional(Args.View);
    public Option<System.Drawing.Point> ViewportPoint => View.IsSome ? Some(Args.ViewportPoint) : Option<System.Drawing.Point>.None;   // top-left (0,0) is a valid pixel; gate on View presence, not IsEmpty
    public Option<Line> WorldLine =>
        (View.Case, ViewportPoint.Case) switch {
            (RhinoView view, System.Drawing.Point point) => Optional(view.Document).Case switch {
                RhinoDoc document => Camera.RhinoCamera.Live(document: document)
                    .RunValue(
                        operation: Camera.CameraOps.Query(query: scope => scope.FrustumLine(screenX: point.X, screenY: point.Y)),
                        target: new Camera.ViewportTarget.View(Value: view))
                    .ToOption(),
                _ => Option<Line>.None,
            },
            _ => Option<Line>.None,
        };
    public MouseDecision<TState> Pass => MouseDecision.Pass(State);
    public MouseDecision<TState> Stop => MouseDecision.Stop(State);
    public MouseDecision<TState> RejectIfGumball => IsOverGumball ? MouseDecision.Reject(State) : Pass;   // hard-cancel the native event while a gumball owns the cursor
    public MouseDecision<TState> Next(TState state, bool cancel = false, Option<string> toolTip = default) =>
        MouseDecision.Next(state, cancel, toolTip);
    public MouseDecision<TState> Hint(string value) => MouseDecision.Hint(State, value);
    public Fin<Point3d> Project(Plane plane) {
        Option<Line> worldLine = WorldLine;   // single evaluation of the camera-ray derivation
        return from line in Op.Of(name: nameof(Project)).Need(worldLine)
               from validPlane in plane.IsValid switch { true => Fin.Succ(value: plane), false => Fin.Fail<Plane>(error: Op.Of(name: nameof(Project)).InvalidInput()) }
               from point in global::Rhino.Geometry.Intersect.Intersection.LinePlane(line: line, plane: validPlane, lineParameter: out double parameter) switch { true => Fin.Succ(value: line.PointAt(t: parameter)), false => Fin.Fail<Point3d>(error: Op.Of(name: nameof(Project)).InvalidResult()) }
               select point;
    }
}

public readonly record struct MouseDecision<TState>(TState State, bool Cancel, Option<string> ToolTip = default);

// Non-generic companion (CA1000): single owner of the Pass/Stop/Next/Hint vocabulary — MouseContext + UiCanvasContext + UiCanvasKey delegate here.
public static class MouseDecision {
    public static MouseDecision<TState> Pass<TState>(TState state) => new(State: state, Cancel: false);
    public static MouseDecision<TState> Stop<TState>(TState state) => new(State: state, Cancel: true);
    public static MouseDecision<TState> Reject<TState>(TState state) => new(State: state, Cancel: true, ToolTip: Some("Gumball active"));
    public static MouseDecision<TState> Next<TState>(TState state, bool cancel = false, Option<string> toolTip = default) => new(State: state, Cancel: cancel, ToolTip: toolTip);
    public static MouseDecision<TState> Hint<TState>(TState state, string value) =>
        string.IsNullOrWhiteSpace(value: value) ? Pass(state) : new MouseDecision<TState>(State: state, Cancel: false, ToolTip: Some(value));
}

public readonly record struct OverlayContext<TState>(OverlayPhase Phase, TState State, object? Args = null, bool Enabled = false) {
    public Option<TArgs> As<TArgs>() where TArgs : class => Optional(Args as TArgs);
    public Fin<TArgs> Require<TArgs>() where TArgs : class => Op.Of(name: nameof(Require)).Need(As<TArgs>());
}

public readonly record struct OverlayDecision(Option<BoundingBox> Bounds = default, Option<bool> Cull = default, Option<Func<DisplayPipeline, Fin<Unit>>> Draw = default, Option<Func<DrawObjectEventArgs, Fin<Unit>>> DrawObject = default) {
    public static OverlayDecision Ignore => new();
    public static Fin<OverlayDecision> Include(BoundingBox bounds) => bounds.IsValid switch { true => Fin.Succ(value: new OverlayDecision(Bounds: Some(bounds))), false => Fin.Fail<OverlayDecision>(error: Op.Of(name: nameof(OverlayDecision)).InvalidResult()) };
    public static Fin<OverlayDecision> Include(GeometryBase geometry) =>
        BoundsOf(source: geometry, op: Op.Of(name: nameof(OverlayDecision))).Bind(Include);
    public static OverlayDecision CullObject(bool cull = true) => new(Cull: Some(cull));
    public static Fin<OverlayDecision> Paint(Func<DisplayPipeline, Fin<Unit>> draw) =>
        Op.Of(name: nameof(Paint)).Need(draw)
            .Map(static value => new OverlayDecision(Draw: Some(value)));
    public static Fin<OverlayDecision> PaintObject(Func<DrawObjectEventArgs, Fin<Unit>> draw) =>
        Op.Of(name: nameof(PaintObject)).Need(draw)
            .Map(static value => new OverlayDecision(DrawObject: Some(value)));
    public static OverlayDecision operator +(OverlayDecision left, OverlayDecision right) => Add(left: left, right: right);
    public static OverlayDecision Fold(Seq<OverlayDecision> decisions) => decisions.Fold(Ignore, static (state, decision) => state + decision);
    // Right-biased per-field fold EXCEPT Bounds (unioned — overlays accumulate extent) and Draw/DrawObject (fault-isolated
    // via Combine so both paints ALWAYS run; a left paint fault no longer blanks later overlays — errors accumulate via `+`).
    public static OverlayDecision Add(OverlayDecision left, OverlayDecision right) => new(
        Bounds: (left.Bounds.Case, right.Bounds.Case) switch { (BoundingBox a, BoundingBox b) => Some(BoundingBox.Union(a, b)), _ => right.Bounds | left.Bounds },
        Cull: right.Cull | left.Cull,
        Draw: Combine(left: left.Draw, right: right.Draw),
        DrawObject: Combine(left: left.DrawObject, right: right.DrawObject));

    // Method-boundary Match (CSP0705): projects a Fin<Unit> to its Error (Error.Empty when succeeded) for monoid accumulation.
    private static Error ErrorOf(Fin<Unit> result) => result.Match(Succ: static _ => Error.Empty, Fail: static e => e);

    private static Option<Func<TArg, Fin<Unit>>> Combine<TArg>(Option<Func<TArg, Fin<Unit>>> left, Option<Func<TArg, Fin<Unit>>> right) =>
        (left.Case, right.Case) switch {
            (Func<TArg, Fin<Unit>> a, Func<TArg, Fin<Unit>> b) => Some<Func<TArg, Fin<Unit>>>(arg => (SafePaint(paint: a, arg: arg), SafePaint(paint: b, arg: arg)) switch {
                ( { IsSucc: true }, { IsSucc: true }) => Fin.Succ(value: unit),
                (Fin<Unit> ra, Fin<Unit> rb) => Fin.Fail<Unit>(error: ErrorOf(ra) + ErrorOf(rb)),
            }),
            _ => right | left,
        };

    private static Fin<Unit> SafePaint<TArg>(Func<TArg, Fin<Unit>> paint, TArg arg) =>
        Op.Of(name: nameof(Combine)).Catch(() => paint(arg: arg));
    internal static Fin<BoundingBox> BoundsOf(object source, Op op) =>
        Analyze.Run(operation: Analyze.Bounds<object, BoundingBox>(aspect: Analysis.Bounds.AxisAligned), input: source)
            .ToFin()
            .Bind(boxes => boxes.Count switch { > 0 => Fin.Succ(value: boxes[0]), _ => Fin.Fail<BoundingBox>(error: op.InvalidResult()) })
            .Bind(box => box.IsValid ? Fin.Succ(value: box) : Fin.Fail<BoundingBox>(error: op.InvalidResult()));
}

public readonly record struct OverlayFilter(Option<ObjectType> Geometry = default, Option<ActiveSpace> Space = default, Option<Seq<Guid>> ObjectIds = default, Option<(bool On, bool CheckSubObjects)> Selection = default, Option<(RhinoViewport Viewport, bool Exclusive)> Viewport = default, bool Unbind = false) {
    public static OverlayFilter Reset => new(Unbind: true);

    // Right-biased per-field monoid; a Reset on the right wins outright (clears the composed filter).
    public static OverlayFilter operator +(OverlayFilter left, OverlayFilter right) => right.Unbind ? right : new(
        Geometry: right.Geometry | left.Geometry,
        Space: right.Space | left.Space,
        ObjectIds: right.ObjectIds | left.ObjectIds,
        Selection: right.Selection | left.Selection,
        Viewport: right.Viewport | left.Viewport,
        Unbind: left.Unbind || right.Unbind);

    internal Fin<Unit> Apply(DisplayConduit conduit) {
        OverlayFilter filter = this;   // struct lambdas cannot capture `this`
        return Op.Of(name: nameof(OverlayFilter)).Need(conduit).Map(valid => {
            _ = Op.SideWhen(filter.Unbind, () => {
                valid.GeometryFilter = ObjectType.AnyObject;
                valid.SpaceFilter = ActiveSpace.None;
                valid.SetSelectionFilter(on: false, checkSubObjects: false);
                valid.SetObjectIdFilter(ids: Seq<Guid>().AsIterable());
                valid.UnbindAll();
            });
            _ = filter.Geometry.Iter(value => valid.GeometryFilter = value);
            _ = filter.Space.Iter(value => valid.SpaceFilter = value);
            _ = filter.Selection.Iter(value => valid.SetSelectionFilter(on: value.On, checkSubObjects: value.CheckSubObjects));
            _ = filter.ObjectIds.Map(static ids => ids.Filter(static id => id != Guid.Empty).Distinct()).Iter(ids => valid.SetObjectIdFilter(ids: ids.AsIterable()));
            _ = Op.SideWhen(!filter.Unbind, () => filter.Viewport.Iter(value => {
                valid.UnbindAll();
                _ = value.Exclusive ? Op.Side(() => valid.ExclusiveBind(viewport: value.Viewport)) : Op.Side(() => valid.Bind(viewport: value.Viewport));
            }));
            return unit;
        });
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiRenderState(bool OnTop = false, Option<bool> DepthWrite = default, Option<CullFaceMode> CullFace = default, Option<Transform> Model = default, bool Screen2d = false) {
    // BOUNDARY ADAPTER - render-state push/pop stack must balance even if draw throws.
    internal Unit Scope(DisplayPipeline pipeline, Action draw) {
        _ = Op.SideWhen(OnTop, () => pipeline.PushDepthTesting(false));
        _ = Op.SideWhen(Screen2d, pipeline.Push2dProjection);   // pixel-space 2D projection; matched by PopProjection (both void on DisplayPipeline, not RhinoViewport)
        _ = DepthWrite.Iter(pipeline.PushDepthWriting);
        _ = CullFace.Iter(pipeline.PushCullFaceMode);
        _ = Model.Iter(pipeline.PushModelTransform);
        try { draw(); } finally {
            _ = Model.Iter(_ => pipeline.PopModelTransform());
            _ = CullFace.Iter(_ => pipeline.PopCullFaceMode());
            _ = DepthWrite.Iter(_ => pipeline.PopDepthWriting());
            _ = Op.SideWhen(Screen2d, pipeline.PopProjection);   // reverse-order pop of the 2D projection
            _ = Op.SideWhen(OnTop, pipeline.PopDepthTesting);   // Pop matches the entry Push(false) — balanced stack.
        }
        return unit;
    }
}

// Where to run the world-space gradient axis through a geometry's bounds (LongestEdge avoids a degenerate near-flat axis).
public enum UiGradientAxis { LongestEdge, Diagonal, X, Y, Z }

// Data-driven gradient ramp for the Hatch/Mesh draw arms (false-color fields, draft-angle ramps): a stop sequence + an
// axis selector resolved against the geometry bounds at draw time, so one style stays geometry-agnostic. Linear=false => radial.
[StructLayout(LayoutKind.Auto)]
public readonly record struct UiGradient(Seq<ColorStop> Stops, bool Linear = true, float Repeat = 0f, UiGradientAxis Axis = UiGradientAxis.LongestEdge) {
    public static UiGradient Of(DrawingColor from, DrawingColor to, bool linear = true, UiGradientAxis axis = UiGradientAxis.LongestEdge) =>
        new(Stops: Seq(new ColorStop(from, 0d), new ColorStop(to, 1d)), Linear: linear, Axis: axis);

    // Resolve (point1, point2) for the native DrawGradient* call; a sub-tolerance chosen extent (flat/planar geometry)
    // falls back to the box diagonal so point1 never equals point2 (which silently produces no ramp).
    internal (Point3d P1, Point3d P2) AxisOf(BoundingBox box) {
        Point3d c = box.Center;
        Vector3d d = box.Diagonal;
        (Point3d P1, Point3d P2) pick = Axis switch {
            UiGradientAxis.Diagonal => (box.Min, box.Max),
            UiGradientAxis.X => (new Point3d(box.Min.X, c.Y, c.Z), new Point3d(box.Max.X, c.Y, c.Z)),
            UiGradientAxis.Y => (new Point3d(c.X, box.Min.Y, c.Z), new Point3d(c.X, box.Max.Y, c.Z)),
            UiGradientAxis.Z => (new Point3d(c.X, c.Y, box.Min.Z), new Point3d(c.X, c.Y, box.Max.Z)),
            _ => (Math.Abs(d.X) >= Math.Abs(d.Y) && Math.Abs(d.X) >= Math.Abs(d.Z))
                ? (new Point3d(box.Min.X, c.Y, c.Z), new Point3d(box.Max.X, c.Y, c.Z))
                : Math.Abs(d.Y) >= Math.Abs(d.Z)
                    ? (new Point3d(c.X, box.Min.Y, c.Z), new Point3d(c.X, box.Max.Y, c.Z))
                    : (new Point3d(c.X, c.Y, box.Min.Z), new Point3d(c.X, c.Y, box.Max.Z)),
        };
        return pick.P1.DistanceTo(pick.P2) < RhinoMath.ZeroTolerance ? (box.Min, box.Max) : pick;
    }
}

public readonly record struct UiPreviewStyle(
    Option<UiStroke> Stroke = default,
    Option<DrawingColor> Text = default,
    Option<OverlayPhase> Phase = default,
    Option<DisplayMaterial> Material = default,
    int Thickness = 2,
    int WireDensity = 0,
    float PointRadius = 4f,
    PointStyle PointStyle = PointStyle.Simple,
    double Transparency = 0.55,
    bool FalseColor = false,
    UiRenderState Render = default,
    double MarkerSize = 16.0,
    Option<UiGradient> Gradient = default,
    bool Dotted = false) {
    internal OverlayPhase PhaseOrDefault => Phase.IfNone(OverlayPhase.PostDraw);

    [SuppressMessage(category: "Reliability", checkId: "CA2000:Dispose objects before losing scope", Justification = "Owned default DisplayMaterial is disposed in the method's terminal finally; caller-supplied materials are not owned.")]
    internal Fin<Unit> Draw(UiPreviewContext context, object geometry) {
        UiPreviewStyle s = this;   // struct copy: nested closures cannot capture `this` on a readonly record struct
        DisplayPipeline pipeline = context.Display;
        DrawingColor stroke = s.Stroke.Map(static value => value.Color).IfNone(() => context.Document.CreateDefaultAttributes().DrawColor(context.Document));
        DrawingColor text = s.Text.IfNone(DrawingColor.White);
        (DisplayMaterial material, bool ownsMaterial) = s.Material.Case switch {
            DisplayMaterial supplied => (supplied, false),
            _ => (new DisplayMaterial(diffuse: stroke, transparency: s.Transparency), true),
        };
        Fin<Unit> Paint(Action draw) => Fin.Succ(value: s.Render.Scope(pipeline: pipeline, draw: draw));
        IEnumerable<Point3d> PolyPoints(Curve curve) {   // screen-anchored dotted sampling over the curve domain
            Interval domain = curve.Domain;
            return toSeq(Enumerable.Range(start: 0, count: 65)).Map(i => curve.PointAt(t: domain.ParameterAt(normalizedParameter: (double)i / 64)));
        }
        // line-likes route through native dotted calls when Dotted; else DisplayPen (dashed/halo) or the color overload.
        Unit DrawCurveWithPen(Curve curve) => (s.Dotted, s.Stroke.Case) switch {
            (true, _) => Op.Side(() => pipeline.DrawDottedPolyline(points: PolyPoints(curve: curve), color: stroke, close: curve.IsClosed)),
            (false, UiStroke value) => Op.Side(() => pipeline.DrawCurve(curve: curve, pen: value.ToDisplayPen())),
            (false, _) => Op.Side(() => pipeline.DrawCurve(curve: curve, color: stroke, thickness: s.Thickness)),
        };
        Fin<Unit> Lines(Curve curve) => Paint(() => _ = DrawCurveWithPen(curve: curve));                                          // caller-owned curve — never disposed here
        Fin<Unit> Owned(Func<Curve> make) => Paint(() => { using Curve managed = make(); _ = DrawCurveWithPen(curve: managed); });  // transient curve created + disposed in scope
        Fin<Unit> Shaded(Action shade, Action wires) => Paint(() => { shade(); wires(); });                                        // param order = draw order (shade beneath wires)
        // Universal gradient: a set UiGradient routes shaded geometry through its render mesh -> DrawGradientMesh, hatches ->
        // DrawGradientHatch, curves -> DrawGradientLines (one native call); geometry with no gradient substrate returns null
        // and falls through to the solid arms below. Meshing is dynamic-aware (Fast while orbiting, Quality when settled) and
        // every transient render mesh is disposed at the draw boundary. The axis comes from the SOURCE geometry bounds.
        Option<Fin<Unit>> GradientPaint(UiGradient g) {
            MeshingParameters mp = context.Mode.Dynamic ? MeshingParameters.FastRenderMesh : MeshingParameters.QualityRenderMesh;
            Fin<Unit> MeshGradient(Mesh? m, BoundingBox box) =>
                m is { IsValid: true } && m.Vertices.Count > 0
                    ? Paint(() => { (Point3d p1, Point3d p2) = g.AxisOf(box); pipeline.DrawGradientMesh(mesh: m, stops: g.Stops.AsEnumerable(), point1: p1, point2: p2, linearGradient: g.Linear, repeat: g.Repeat); })
                    : Fin.Succ(value: unit);   // degenerate render mesh: the style asked for a gradient, so no-op rather than fall to solid
            // factory mirrors the file's Owned(Func<Curve>) idiom: the transient render mesh is disposed by the consuming `using`.
            Fin<Unit> Meshed(Func<Mesh?> make, BoundingBox box) =>
                Op.Of(name: nameof(Draw)).Catch(() => { using Mesh? managed = make(); return MeshGradient(m: managed, box: box); });
            return geometry switch {
                Mesh mesh => Some(MeshGradient(m: mesh, box: mesh.GetBoundingBox(accurate: false))),   // native mesh is caller-owned — never disposed here
                Brep brep => Some(Meshed(make: () => Combine(Mesh.CreateFromBrep(brep: brep, meshingParameters: mp)), box: brep.GetBoundingBox(accurate: false))),
                SubD subd => Some(Meshed(make: () => Mesh.CreateFromSubD(subd: subd, displayDensity: s.WireDensity > 0 ? Math.Clamp(s.WireDensity, 1, 6) : 3), box: subd.GetBoundingBox(accurate: false))),
                Extrusion extrusion => Some(Meshed(make: () => Mesh.CreateFromExtrusion(extrusion: extrusion, meshingParameters: mp), box: extrusion.GetBoundingBox(accurate: false))),   // Extrusion : Surface — must precede Surface
                Surface surface => Some(Meshed(make: () => Mesh.CreateFromSurface(surface: surface, meshingParameters: mp), box: surface.GetBoundingBox(accurate: false))),
                Hatch hatch => Some(Paint(() => { (Point3d p1, Point3d p2) = g.AxisOf(hatch.GetBoundingBox(accurate: false)); pipeline.DrawGradientHatch(hatch: hatch, stops: g.Stops.AsEnumerable(), point1: p1, point2: p2, linearGradient: g.Linear, repeat: g.Repeat, boundaryThickness: s.Thickness, boundaryColor: stroke); })),
                Curve curve => Some(Paint(() => {
                    Seq<Point3d> pts = toSeq(PolyPoints(curve: curve));
                    (Point3d p1, Point3d p2) = g.AxisOf(curve.GetBoundingBox(accurate: false));
                    pipeline.DrawGradientLines(lines: pts.Zip(pts.Tail).Map(static pair => new Line(pair.Item1, pair.Item2)).AsEnumerable(), strokeWidth: s.Thickness, stops: g.Stops.AsEnumerable(), point1: p1, point2: p2, linearGradient: g.Linear, repeat: g.Repeat);
                })),
                _ => Option<Fin<Unit>>.None,
            };

            // BOUNDARY ADAPTER — fold Brep per-face render meshes into one owned mesh (returned to Meshed's `using`); source faces disposed here.
            static Mesh? Combine(Mesh[]? parts) => parts is { Length: > 0 } faces ? Append(faces) : null;
            static Mesh Append(Mesh[] faces) { Mesh joined = new(); joined.Append(meshes: faces); _ = toSeq(faces).Iter(static face => face.Dispose()); return joined; }
        }
        try {
            return geometry switch {
                null => Fin.Fail<Unit>(error: Op.Of(name: nameof(Draw)).InvalidInput()),
                _ when s.Gradient.Bind(GradientPaint).Case is Fin<Unit> painted => painted,
                Line line => Owned(() => new LineCurve(line)),
                Arc arc => Owned(() => new ArcCurve(arc)),
                Circle circle => Owned(() => new ArcCurve(circle)),
                Polyline polyline => Owned(polyline.ToNurbsCurve),
                Ellipse ellipse => Owned(() => ellipse.ToNurbsCurve()),
                Curve curve => Lines(curve),
                Mesh mesh => s.FalseColor
                    ? Paint(() => pipeline.DrawMeshFalseColors(mesh: mesh))
                    : Shaded(() => pipeline.DrawMeshShaded(mesh: mesh, material: material), () => pipeline.DrawMeshWires(mesh: mesh, color: stroke, thickness: s.Thickness)),
                Brep brep => Shaded(() => pipeline.DrawBrepShaded(brep: brep, material: material), () => pipeline.DrawBrepWires(brep: brep, color: stroke, wireDensity: s.WireDensity)),
                SubD subd => Shaded(() => pipeline.DrawSubDShaded(subd: subd, material: material), () => pipeline.DrawSubDWires(subd: subd, color: stroke, thickness: s.Thickness)),
                Ray3d ray => Paint(() => pipeline.DrawArrow(line: new Line(from: ray.Position, to: ray.Position + ray.Direction), color: stroke, screenSize: s.MarkerSize, relativeSize: 0d)),
                UiDirection direction => Paint(() => pipeline.DrawMarker(tip: direction.Tip, direction: direction.Direction, color: stroke, thickness: s.Thickness, size: s.MarkerSize, rotation: 0d)),
                Sphere sphere => Paint(() => pipeline.DrawSphere(sphere: sphere, color: stroke, thickness: s.Thickness)),
                Cone cone => Paint(() => pipeline.DrawCone(cone: cone, color: stroke, thickness: s.Thickness)),
                Cylinder cylinder => Paint(() => pipeline.DrawCylinder(cylinder: cylinder, color: stroke, thickness: s.Thickness)),
                Torus torus => Paint(() => pipeline.DrawTorus(torus: torus, color: stroke, thickness: s.Thickness)),
                Box box => Paint(() => pipeline.DrawBox(box: box, color: stroke, thickness: s.Thickness)),
                BoundingBox box => Paint(() => pipeline.DrawBox(box, stroke, s.Thickness)),
                Extrusion extrusion => Paint(() => pipeline.DrawExtrusionWires(extrusion: extrusion, color: stroke, wireDensity: s.WireDensity)),
                ClippingPlaneSurface clipping => Paint(() => pipeline.DrawClippingPlaneWires(clippingPlane: clipping, color: stroke)),
                Surface surface => Paint(() => pipeline.DrawSurface(surface: surface, wireColor: stroke, wireDensity: s.WireDensity)),
                // 10-arg native point glyph: stroke+fill, pixel diameter, native HiDPI auto-scale (replaces the under-called 4-arg overload).
                Point point => Paint(() => pipeline.DrawPoint(point: point.Location, style: s.PointStyle, strokeColor: stroke, fillColor: stroke, radius: s.PointRadius, strokeWidth: s.Thickness, secondarySize: 0f, rotationRadians: 0f, diameterIsInPixels: true, autoScaleForDpi: true)),
                Point3d point => Paint(() => pipeline.DrawPoint(point: point, style: s.PointStyle, strokeColor: stroke, fillColor: stroke, radius: s.PointRadius, strokeWidth: s.Thickness, secondarySize: 0f, rotationRadians: 0f, diameterIsInPixels: true, autoScaleForDpi: true)),
                ConstructionPlane cp => Paint(() => pipeline.DrawConstructionPlane(constructionPlane: cp)),
                PointCloud cloud => Paint(() => pipeline.DrawPointCloud(cloud: cloud, size: s.PointRadius, color: stroke)),
                Hatch hatch => Paint(() => pipeline.DrawHatch(hatch: hatch, hatchColor: stroke, boundaryColor: stroke)),
                TextDot dot => Paint(() => pipeline.DrawDot(dot: dot, fillColor: stroke, textColor: text, borderColor: stroke)),
                TextEntity entity => Paint(() => pipeline.DrawText(text: entity, color: text)),
                AnnotationBase annotation => Paint(() => pipeline.DrawAnnotation(annotation: annotation, color: text)),
                Light light => Paint(() => pipeline.DrawLight(light: light, wireframeColor: stroke)),
                _ => Fin.Succ(value: unit),
            };
        } finally {
            _ = Op.SideWhen(ownsMaterial, material.Dispose);
        }
    }
}

// World->screen projection cached per viewport draw epoch — ChangeCounter advances on any viewport mutation, so the
// cache self-invalidates when the camera or scene moves. Conduit-owned (one instance per preview lifetime); a frame
// re-uses cached pixels for repeated projections of the same world point, recomputing only after the epoch advances.
internal sealed class ProjectionMemo {
    private readonly Atom<(uint Epoch, HashMap<Point3d, System.Drawing.PointF> Cache)> cell = Atom((0u, HashMap<Point3d, System.Drawing.PointF>()));
    // epoch is read at the call site (viewport.ChangeCounter) so the memo stays pure-managed + unit-testable; the cache
    // self-invalidates whenever epoch advances. Point3d keys compare BIT-EXACT — a jittered anchor misses (Overlay.spec law).
    internal Fin<System.Drawing.PointF> Screen(uint epoch, Point3d world, Func<Point3d, Fin<System.Drawing.PointF>> project) {
        HashMap<Point3d, System.Drawing.PointF> live = cell.Swap(p => p.Epoch == epoch ? p : (epoch, HashMap<Point3d, System.Drawing.PointF>())).Cache;
        return live.Find(world) is { IsSome: true, Case: System.Drawing.PointF hit }
            ? Fin.Succ(value: hit)
            : project(world).Map(pt => { _ = cell.Swap(p => p.Epoch == epoch ? (p.Epoch, p.Cache.AddOrUpdate(world, pt)) : p); return pt; });
    }
}

// Orthogonal pipeline-state discriminator: suppress transient chrome (tooltips, debug grips, snap guides) in exports.
[StructLayout(LayoutKind.Auto)]
public readonly record struct UiRenderMode(bool Capturing, bool Printing, bool Dynamic, int NestLevel) {
    public bool Decorative => !Capturing && !Printing;
}

public readonly record struct UiPreviewContext(
    RhinoDoc Document,
    OverlayPhase Phase,
    RhinoViewport Viewport,
    DisplayPipeline Display,
    Option<UiGumballSnapshot> Gumball,
    UiSpriteAtlas? Atlas = null) {
    internal ProjectionMemo? Memo { get; init; }   // conduit-supplied per-frame projection cache; internal so it stays off the public ctor surface
    internal object? Args { get; init; }           // PreDrawObject event args for per-object restyle (As<DrawObjectEventArgs>)
    internal float DpiScale => Display.DpiScale;
    internal UiHudLayout Layout => UiHudLayout.Of(viewport: Viewport, dpiScale: Display.DpiScale);
    public UiRenderMode Mode => new(Capturing: Display.IsInViewCapture, Printing: Display.IsPrinting, Dynamic: Display.IsDynamicDisplay, NestLevel: Display.NestLevel);
    public Option<T> As<T>() where T : class => Optional(Args as T);

    // Pixels-per-world-unit at a frustum point (perspective-correct); WorldSize converts a pixel size into the world
    // length that keeps an on-screen grip/marker/dimension-tick zoom-invariant. Distinct from Screen's pixel coordinate.
    public Fin<double> PixelScale(Point3d at) {
        RhinoViewport viewport = Viewport;
        return Op.Of(name: nameof(PixelScale)).Catch(() =>
            viewport.GetWorldToScreenScale(pointInFrustum: at, pixelsPerUnit: out double ppu) && ppu > 0d
                ? Fin.Succ(value: ppu) : Fin.Fail<double>(error: Op.Of(name: nameof(PixelScale)).InvalidResult()));
    }
    public Fin<double> WorldSize(Point3d at, float pixels) {
        float dpi = DpiScale;   // struct lambdas cannot capture `this`
        return PixelScale(at: at).Map(ppu => pixels * dpi / ppu);
    }

    // World->screen for this frame's viewport; the conduit-owned memo (when present) caches per draw epoch so repeated
    // same-point projection is O(1). No clipping in the native xform — pair with Label's occlusion gate when needed.
    public Fin<System.Drawing.PointF> Screen(Point3d world) {
        RhinoDoc document = Document;
        RhinoViewport viewport = Viewport;
        Fin<System.Drawing.PointF> Project(Point3d w) =>
            Camera.RhinoCamera.Live(document: document).RunValue(
                operation: Camera.CameraOps.Query(query: scope => scope.ScreenPoint(point: w)),
                target: new Camera.ViewportTarget.Id(Value: viewport.Id));
        return Memo is { } memo ? memo.Screen(epoch: viewport.ChangeCounter, world: world, project: Project) : Project(w: world);
    }

    // World-anchored label: occlusion gate (behind-camera/culled points are skipped) -> memoized projection -> screen
    // text mark. Distinct domain from Draw (world input + occlusion invariant + memo lifetime) — its own member.
    public Fin<Unit> Label(Point3d world, string text, UiAnchor anchor = UiAnchor.TopCenter, DrawingColor? color = null) {
        RhinoDoc document = Document;
        RhinoViewport viewport = Viewport;
        UiPreviewContext self = this;
        return from visible in Camera.RhinoCamera.Live(document: document).RunValue(
                   operation: Camera.CameraOps.Query(query: scope => scope.Visible(source: new Camera.CameraSubject.AtPoint(Value: world))),
                   target: new Camera.ViewportTarget.Id(Value: viewport.Id))
               from drawn in visible switch {
                   true => from at in self.Screen(world: world)
                           from marked in self.Mark(mark: new UiMark.Text(Value: text, At: at, Color: color ?? DrawingColor.White, Anchor: anchor))
                           select marked,
                   false => Fin.Succ(value: unit),
               }
               select drawn;
    }

    // Subject overload: occlusion gates on the whole extent (a box/sphere label survives when its anchor pixel is
    // behind-camera but the extent is visible) and anchors at the subject centroid. Wider domain, same invariant.
    public Fin<Unit> Label(Camera.CameraSubject subject, string text, UiAnchor anchor = UiAnchor.TopCenter, DrawingColor? color = null) {
        RhinoDoc document = Document;
        RhinoViewport viewport = Viewport;
        UiPreviewContext self = this;
        return from visible in Camera.RhinoCamera.Live(document: document).RunValue(
                   operation: Camera.CameraOps.Query(query: scope => scope.Visible(source: subject)),
                   target: new Camera.ViewportTarget.Id(Value: viewport.Id))
               from drawn in visible switch {
                   true => from box in subject.BoundsOf(op: Op.Of(name: nameof(Label)))
                           from at in self.Screen(world: box.Center)
                           from marked in self.Mark(mark: new UiMark.Text(Value: text, At: at, Color: color ?? DrawingColor.White, Anchor: anchor))
                           select marked,
                   false => Fin.Succ(value: unit),
               }
               select drawn;
    }

    internal Fin<Unit> Mark(UiMark mark) => mark.Render(surface: new UiSurface.Pipeline(Display: Display, Atlas: Atlas));
}

// --- [STEPS] ------------------------------------------------------------------------------
public readonly record struct InteractionGuard(MousePhase Phase, Option<global::Rhino.UI.MouseButton> Button = default) {
    internal bool Matches<TState>(MouseContext<TState> context) =>
        context.Phase == Phase && Button.Map(button => context.MouseButton == button).IfNone(true);
    // A viewport guard keyed on a non-native phase (Wheel — no native MouseCallback On* hook) can never fire on the
    // viewport rail; admit fails fast with a typed InvalidInput instead of silently never-matching.
    internal Fin<Unit> AdmitViewport() => Phase.ViewportNative ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Op.Of(name: nameof(AdmitViewport)).InvalidInput());
}

// Plain abstract record (NOT [Union]): generic union types force `allows ref struct` under the source
// generator — incompatible with record cases (see Commands/Command.cs PromptTransition). Manual `this switch`.
public abstract record InteractionStep<TState> {
    private InteractionStep() { }
    public sealed record PhaseGuard(InteractionGuard Guard) : InteractionStep<TState>;
    public sealed record Snap(Plane Plane, Func<MouseContext<TState>, Point3d, TState> Project) : InteractionStep<TState>;
    public sealed record Transition(Func<MouseContext<TState>, TState> Project) : InteractionStep<TState>;
    public sealed record Emit(Func<MouseContext<TState>, Fin<Unit>> Effect) : InteractionStep<TState>;
    public sealed record Debounce(TimeSpan Interval, TimeProvider Clock) : InteractionStep<TState> { internal Atom<long> Gate { get; } = Atom(long.MinValue); }

    // PhaseGuard mismatch → Fail(Cancelled) stops the fold; Snap projection-miss / off-window Debounce skip the step.
    internal Fin<MouseDecision<TState>> Apply(MouseContext<TState> context, MouseDecision<TState> current) =>
        this switch {
            PhaseGuard guard => guard.Guard.AdmitViewport().Bind(_ => guard.Guard.Matches(context: context) switch {
                true => Fin.Succ(value: current),
                false => Fin.Fail<MouseDecision<TState>>(error: new Fault.Cancelled()),
            }),
            Snap snap => from point in context.Project(plane: snap.Plane) select current with { State = snap.Project(arg1: context, arg2: point) },
            Transition transition => Fin.Succ(value: current with { State = transition.Project(arg: context) }),
            Emit emit => emit.Effect(arg: context).Map(_ => current),
            Debounce debounce => (debounce.Clock.GetElapsedTime(startingTimestamp: debounce.Gate.Value) >= debounce.Interval) switch {
                true => Fin.Succ(value: (debounce.Gate.Swap(_ => debounce.Clock.GetTimestamp()), current).Item2),
                false => Fin.Fail<MouseDecision<TState>>(error: new Fault.Cancelled()),
            },
            _ => Fin.Succ(value: current),
        };
}

public static class InteractionStep {   // non-generic host (CA1000): InteractionStep.Compose(steps) infers TState
    public static Func<MouseContext<TState>, Fin<MouseDecision<TState>>> Compose<TState>(Seq<InteractionStep<TState>> steps) =>
        context => steps.Fold(
            Fin.Succ(value: context.Pass),
            (accumulator, step) => accumulator.Bind(decision => step.Apply(context: context, current: decision)));
}

public readonly record struct UiPreviewScope(
    RhinoDoc Document,
    RasmOverlay<UiViewportPreview> Overlay,
    Option<UiGumball> Gumball) {
    public Fin<Unit> Set(UiViewportPreview preview) {
        RasmOverlay<UiViewportPreview> overlay = Overlay;
        RhinoDoc document = Document;
        return Op.Of(name: nameof(Set)).Need(preview)
            .Bind(valid => valid.Validate().Map(_ => valid))
            .Bind(valid => overlay.Transition(transition: _ => valid, document: document));
    }

    public Fin<bool> UpdateGumball(Point3d point, Line worldLine) =>
        from active in Op.Of(name: nameof(UpdateGumball)).Need(Gumball)
        from _ in active.CheckKeys()
        from changed in active.Update(point: point, line: worldLine)
        select changed;

    public Fin<bool> PickGumball(PickContext pick, GetPoint point) => from active in Op.Of(name: nameof(PickGumball)).Need(Gumball) from validPick in Op.Of(name: nameof(PickGumball)).Need(pick) from validPoint in Op.Of(name: nameof(PickGumball)).Need(point) from picked in active.Pick(pick: validPick, point: validPoint) select picked;
}

public sealed record UiViewportInteraction<TState>(
    TState Initial,
    UiViewportPreview Preview,
    Func<MouseContext<TState>, Fin<MouseDecision<TState>>> Mouse,
    Option<UiGumballSpec> Gumball = default) {
    internal Fin<T> Use<T>(RhinoDoc document, Func<UiPreviewScope, Fin<T>> run) =>
        from validRun in Op.Of(name: nameof(Use)).Need(run)
        from validMouse in Op.Of(name: nameof(Use)).Need(Mouse)
        from result in UiViewportPreview.Use(document: document, preview: Preview, gumball: Gumball, run: scope => new Callback(initial: Initial, change: validMouse).Use(run: _ => validRun(arg: scope)))
        select result;

    private sealed class Callback(TState initial, Func<MouseContext<TState>, Fin<MouseDecision<TState>>> change) : global::Rhino.UI.MouseCallback, IDisposable {
        private readonly Atom<TState> state = Atom(initial);
        private bool ownsToolTip;
        private bool disposed;

        internal Fin<T> Use<T>(Func<Callback, Fin<T>> run) =>
            from validRun in Op.Of(name: nameof(Use)).Need(run)
            from _ in guard(!disposed, Op.Of(name: nameof(Use)).InvalidInput())
            from result in RhinoUi.Protect(valid: () => {
                Enabled = true;
                // BOUNDARY ADAPTER - native mouse callback lifetime must close after viewport interaction exits.
                try { return validRun(arg: this); } finally { Dispose(); }
            })
            select result;

        public void Dispose() {
            _ = disposed switch {
                true => unit,
                false => Disable(),
            };
            disposed = true;
            GC.SuppressFinalize(obj: this);
        }

        protected override void OnMouseMove(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Move, args: e);
        protected override void OnEndMouseMove(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.MoveEnd, args: e);
        protected override void OnMouseDown(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Down, args: e);
        protected override void OnEndMouseDown(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.DownEnd, args: e);
        protected override void OnMouseUp(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Up, args: e);
        protected override void OnEndMouseUp(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.UpEnd, args: e);
        protected override void OnMouseDoubleClick(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.DoubleClick, args: e);
        protected override void OnMouseEnter(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Enter, args: e);
        protected override void OnMouseHover(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Hover, args: e);
        protected override void OnMouseLeave(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Leave, args: e);

        private Unit Apply(MousePhase phase, global::Rhino.UI.MouseCallbackEventArgs args) {
            MouseContext<TState> context = new(Phase: phase, State: state.Value, Args: args);
            return RhinoUi.Protect(valid: () => change(arg: context))
                .Map(decision => {
                    _ = state.Swap(_ => decision.State);
                    _ = decision.ToolTip.Case switch {
                        string tooltip => Op.Side(() => {
                            ownsToolTip = true;
                            global::Rhino.UI.MouseCursor.SetToolTip(tooltip: tooltip);
                        }),
                        _ => unit,
                    };
                    args.Cancel = context.CanCancelNative switch {
                        true => args.Cancel || decision.Cancel,
                        _ => args.Cancel,
                    };
                    return unit;
                })
                .Match(Succ: static _ => unit, Fail: error => {
                    RhinoApp.WriteLine(message: $"{nameof(UiViewportInteraction<>)}: {error}");
                    return Disable();
                });
        }

        private Unit Disable() {
            _ = ownsToolTip switch {
                true => Op.Side(static () => global::Rhino.UI.MouseCursor.SetToolTip(tooltip: string.Empty)),
                false => unit,
            };
            ownsToolTip = false;
            Enabled = false;
            return unit;
        }
    }
}

public sealed record UiViewportPreview {
    private readonly Func<UiPreviewContext, Fin<Unit>> draw;
    private readonly Func<Fin<OverlayDecision>> bounds;
    private readonly Func<Fin<Unit>> validate;
    private readonly Func<OverlayPhase, bool> fires;   // declared phase gate; the ctor enforces it so a draw func can never fire off-phase

    private UiViewportPreview(Func<UiPreviewContext, Fin<Unit>> draw, Func<Fin<OverlayDecision>> bounds, Func<Fin<Unit>> validate, Func<OverlayPhase, bool> fires) {
        this.fires = fires;
        this.draw = context => fires(arg: context.Phase) ? draw(arg: context) : Fin.Succ(value: unit);   // structural guard: off-phase is a no-op
        this.bounds = bounds;
        this.validate = validate;
    }

    // Which phases this preview's draw fires on; the Conduit gates its per-phase OverlayDecision allocation on this.
    internal bool Fires(OverlayPhase phase) => fires(arg: phase);

    public static UiViewportPreview Empty { get; } =
        new(draw: static _ => Fin.Succ(value: unit), bounds: static () => Fin.Succ(value: OverlayDecision.Ignore), validate: static () => Fin.Succ(value: unit), fires: static _ => false);

    public static UiViewportPreview Of<TGeometry>(IEnumerable<TGeometry> geometry, UiPreviewStyle style = default) where TGeometry : notnull {
        Fin<Seq<object>> items = Op.Of(name: nameof(UiViewportPreview)).Need(geometry)
            .Bind(static source => toSeq(source).Map(static item => (object)item).TraverseM(item => Op.Of(name: nameof(UiViewportPreview)).Need(item)
                .Bind(static value => OverlayDecision.BoundsOf(source: value, op: Op.Of(name: nameof(UiViewportPreview))).Map(_ => value))).As())
            .Bind(static values => values.IsEmpty switch {
                false => Fin.Succ(value: values),
                true => Fin.Fail<Seq<object>>(error: Op.Of(name: nameof(UiViewportPreview)).InvalidInput()),
            });
        return new(
            draw: context => from active in items
                             from _ in active.TraverseM(item => style.Draw(context: context, geometry: item)).As()
                             select unit,
            bounds: () => from active in items
                          from decision in active
                .TraverseM(static item => OverlayDecision.BoundsOf(source: item, op: Op.Of(name: nameof(UiViewportPreview))).Bind(OverlayDecision.Include))
                .As()
                .Map(static decisions => decisions.Fold(OverlayDecision.Ignore, static (state, decision) => state + decision))
                          select decision,
            validate: () => items.Map(static _ => unit),
            fires: phase => phase == style.PhaseOrDefault);
    }

    public static UiViewportPreview FromSelection(Commands.CommandSelection selection, UiPreviewStyle style = default) {
        Fin<UiViewportPreview> preview =
            Op.Of(name: nameof(FromSelection)).Need(selection)
                .Bind(valid => valid.Geometry<GeometryBase>())
                .Map(geometry => Of(geometry: geometry, style: style));

        return new(
            draw: context => preview.Bind(active => active.Draw(context: context)),
            bounds: () => preview.Bind(active => active.Bounds()),
            validate: () => preview.Map(static _ => unit),
            fires: phase => phase == style.PhaseOrDefault);
    }

    // `fires` declares which phases the draw runs on (default: paint phases only — NOT PreDrawObject); the ctor enforces it
    // structurally so a future preview cannot accidentally fire per-object-per-frame on the wrong phase.
    public static UiViewportPreview Draw(Func<UiPreviewContext, Fin<Unit>> draw, Func<Fin<OverlayDecision>> bounds, Func<OverlayPhase, bool>? fires = null) =>
        new(
            draw: context => Op.Of(name: nameof(Draw)).Need(draw).Bind(valid => valid(arg: context)),
            bounds: () => Op.Of(name: nameof(Draw)).Need(bounds).Bind(valid => valid()),
            validate: () => Op.Of(name: nameof(Draw)).Need(draw)
                .Bind(_ => Op.Of(name: nameof(Draw)).Need(bounds))
                .Map(static _ => unit),
            fires: fires ?? (static phase => phase.Draws));

    public static UiViewportPreview Hud(Func<UiPreviewContext, Fin<UiHud>> hud) =>
        Draw(
            // chrome (HUD) is suppressed in exports (ViewCapture/print) via UiRenderMode.Decorative.
            draw: context => context.Mode.Decorative
                ? Op.Of(name: nameof(Hud)).Need(hud).Bind(valid => valid(arg: context)).Bind(h => h.Render(surface: new UiSurface.Pipeline(Display: context.Display, Atlas: context.Atlas)))
                : Fin.Succ(value: unit),
            bounds: static () => Fin.Succ(value: OverlayDecision.Ignore),
            fires: static phase => phase == OverlayPhase.Foreground || phase == OverlayPhase.Overlay);

    // Per-object restyle (isolate / x-ray / section): a restyled object draws via UiPreviewStyle and suppresses its native
    // draw (DrawObject = false). The ONLY preview that opts into the per-object PreDrawObject phase. Activates the
    // producer-less PaintObject + PreDrawObject path.
    public static UiViewportPreview Replace(Func<RhinoObject, Option<UiPreviewStyle>> restyle) =>
        Replace(validRestyle: Op.Of(name: nameof(Replace)).Need(restyle));

    private static UiViewportPreview Replace(Fin<Func<RhinoObject, Option<UiPreviewStyle>>> validRestyle) =>
        new(
            draw: context => from valid in validRestyle
                             from result in context.As<DrawObjectEventArgs>().Case switch {
                                 DrawObjectEventArgs target => valid(arg: target.RhinoObject).Case switch {
                                     UiPreviewStyle style => style.Draw(context: context, geometry: target.RhinoObject.Geometry).Map(_ => { target.DrawObject = false; return unit; }),
                                     _ => Fin.Succ(value: unit),
                                 },
                                 _ => Fin.Succ(value: unit),
                             }
                             select result,
            bounds: static () => Fin.Succ(value: OverlayDecision.Ignore),
            validate: () => validRestyle.Map(static _ => unit),
            fires: static phase => phase == OverlayPhase.PreDrawObject);

    public static UiViewportPreview Add(UiViewportPreview left, UiViewportPreview right) =>
        (Optional(left).IfNone(Empty), Optional(right).IfNone(Empty)) switch {
            (UiViewportPreview a, UiViewportPreview b) => new(
                draw: context => (SafeDraw(preview: a, context: context), SafeDraw(preview: b, context: context)) switch {
                    ( { IsSucc: true }, { IsSucc: true }) => Fin.Succ(value: unit),
                    (Fin<Unit> ra, Fin<Unit> rb) => Fin.Fail<Unit>(error: ErrorOf(result: ra) + ErrorOf(result: rb)),
                },
                bounds: () => from first in a.Bounds() from second in b.Bounds() select first + second,
                validate: () => a.Validate().Bind(_ => b.Validate()),
                fires: phase => a.Fires(phase: phase) || b.Fires(phase: phase)),
        };

    private static Fin<Unit> SafeDraw(UiViewportPreview preview, UiPreviewContext context) =>
        Op.Of(name: nameof(Add)).Catch(() => preview.Draw(context: context));

    private static Error ErrorOf(Fin<Unit> result) =>
        result.Match(Succ: static _ => Error.Empty, Fail: static e => e);

    public static UiViewportPreview operator +(UiViewportPreview left, UiViewportPreview right) =>
        Add(left: left, right: right);

    // monoid fold paralleling UiStatus.Combine — folds N previews through `+` (empty -> Empty identity).
    public static UiViewportPreview FromMany(Seq<UiViewportPreview> previews) =>
        previews.Fold(Empty, static (state, preview) => state + preview);

    internal Fin<Unit> Draw(UiPreviewContext context) => draw(arg: context);
    internal Fin<OverlayDecision> Bounds() => bounds();
    internal Fin<Unit> Validate() => validate();

    internal static Fin<T> Use<T>(RhinoDoc document, UiViewportPreview preview, Option<UiGumballSpec> gumball, Func<UiPreviewScope, Fin<T>> run) =>
        from validDocument in Op.Of(name: nameof(Use)).Need(document)
        from validPreview in Op.Of(name: nameof(Use)).Need(preview).Bind(valid => valid.Validate().Map(_ => valid))
        from validRun in Op.Of(name: nameof(Use)).Need(run)
        from result in gumball.Case switch {
            UiGumballSpec spec => UiGumball.Use(
                document: validDocument,
                spec: spec,
                run: active => new Conduit(document: validDocument, initial: validPreview, gumball: () => Some(active.Snapshot)).Use(
                    document: validDocument,
                    run: overlay => validRun(arg: new UiPreviewScope(Document: validDocument, Overlay: overlay, Gumball: Some(active))))),
            _ => new Conduit(document: validDocument, initial: validPreview, gumball: static () => Option<UiGumballSnapshot>.None).Use(
                document: validDocument,
                run: overlay => validRun(arg: new UiPreviewScope(Document: validDocument, Overlay: overlay, Gumball: Option<UiGumball>.None))),
        }
        select result;

    private sealed class Conduit(RhinoDoc document, UiViewportPreview initial, Func<Option<UiGumballSnapshot>> gumball) : RasmOverlay<UiViewportPreview>(initial) {
        private readonly UiSpriteAtlas atlas = new();
        private readonly ProjectionMemo memo = new();   // per-conduit world->screen cache threaded into each frame's context

        // Gate the per-phase OverlayDecision allocation on the preview's declared `fires`: a non-Replace preview never
        // builds the PaintObject closure on PreDrawObject, and a Replace preview never builds Paint on the draw phases.
        protected override Fin<OverlayDecision> Change(OverlayContext<UiViewportPreview> context) =>
            context.Phase.Bounding
                ? context.State.Bounds()
                : context.Phase == OverlayPhase.PreDrawObject
                    ? context.State.Fires(phase: OverlayPhase.PreDrawObject)
                        ? context.Require<DrawObjectEventArgs>()
                            .Bind(_ => OverlayDecision.PaintObject(draw: target => context.State.Draw(context: new UiPreviewContext(
                                Document: document,
                                Phase: context.Phase,
                                Viewport: target.Viewport,
                                Display: target.Display,
                                Gumball: gumball(),
                                Atlas: atlas) { Memo = memo, Args = target })))
                        : Fin.Succ(value: OverlayDecision.Ignore)
                    : context.Phase.Draws && context.State.Fires(phase: context.Phase)
                        ? context.Require<DrawEventArgs>()
                            .Bind(args => OverlayDecision.Paint(draw: display => context.State.Draw(context: new UiPreviewContext(
                                Document: document,
                                Phase: context.Phase,
                                Viewport: args.Viewport,
                                Display: display,
                                Gumball: gumball(),
                                Atlas: atlas) { Memo = memo })))
                        : Fin.Succ(value: OverlayDecision.Ignore);

        // BOUNDARY ADAPTER - sprite atlas GPU handles released with the conduit scope.
        protected override void Dispose(bool disposing) {
            if (disposing) {
                atlas.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

// Active-mode read (distinct from the GumballAxes [Flags] enable-mask): one (Verb, Axis) projection replaces a 24-arm
// GumballMode switch at every downstream gizmo consumer.
public enum GumballVerb { None, Menu, Translate, Scale, Rotate, Extrude, Cut }
public enum GumballMotionAxis { None, Free, X, Y, Z, XY, YZ, ZX }
[StructLayout(LayoutKind.Auto)]
public readonly record struct GumballAction(GumballVerb Verb, GumballMotionAxis Axis) {
    public bool Active => Verb is not GumballVerb.None;
    public bool Planar => Axis is GumballMotionAxis.XY or GumballMotionAxis.YZ or GumballMotionAxis.ZX;
}

public readonly record struct UiGumballSnapshot(
    Transform PreTransform,
    Transform GumballTransform,
    Transform TotalTransform,
    global::Rhino.UI.Gumball.GumballMode Mode,
    bool InRelocate,
    Option<global::Rhino.UI.Gumball.GumballFrame> Frame = default) {
    private static readonly GumballAction NoneAction = new(GumballVerb.None, GumballMotionAxis.None);
    private static readonly FrozenDictionary<global::Rhino.UI.Gumball.GumballMode, GumballAction> Actions = Seq(
            (global::Rhino.UI.Gumball.GumballMode.Menu, new GumballAction(GumballVerb.Menu, GumballMotionAxis.None)),
            (global::Rhino.UI.Gumball.GumballMode.TranslateFree, new GumballAction(GumballVerb.Translate, GumballMotionAxis.Free)),
            (global::Rhino.UI.Gumball.GumballMode.TranslateX, new GumballAction(GumballVerb.Translate, GumballMotionAxis.X)),
            (global::Rhino.UI.Gumball.GumballMode.TranslateY, new GumballAction(GumballVerb.Translate, GumballMotionAxis.Y)),
            (global::Rhino.UI.Gumball.GumballMode.TranslateZ, new GumballAction(GumballVerb.Translate, GumballMotionAxis.Z)),
            (global::Rhino.UI.Gumball.GumballMode.TranslateXY, new GumballAction(GumballVerb.Translate, GumballMotionAxis.XY)),
            (global::Rhino.UI.Gumball.GumballMode.TranslateYZ, new GumballAction(GumballVerb.Translate, GumballMotionAxis.YZ)),
            (global::Rhino.UI.Gumball.GumballMode.TranslateZX, new GumballAction(GumballVerb.Translate, GumballMotionAxis.ZX)),
            (global::Rhino.UI.Gumball.GumballMode.ScaleX, new GumballAction(GumballVerb.Scale, GumballMotionAxis.X)),
            (global::Rhino.UI.Gumball.GumballMode.ScaleY, new GumballAction(GumballVerb.Scale, GumballMotionAxis.Y)),
            (global::Rhino.UI.Gumball.GumballMode.ScaleZ, new GumballAction(GumballVerb.Scale, GumballMotionAxis.Z)),
            (global::Rhino.UI.Gumball.GumballMode.ScaleXY, new GumballAction(GumballVerb.Scale, GumballMotionAxis.XY)),
            (global::Rhino.UI.Gumball.GumballMode.ScaleYZ, new GumballAction(GumballVerb.Scale, GumballMotionAxis.YZ)),
            (global::Rhino.UI.Gumball.GumballMode.ScaleZX, new GumballAction(GumballVerb.Scale, GumballMotionAxis.ZX)),
            (global::Rhino.UI.Gumball.GumballMode.RotateX, new GumballAction(GumballVerb.Rotate, GumballMotionAxis.X)),
            (global::Rhino.UI.Gumball.GumballMode.RotateY, new GumballAction(GumballVerb.Rotate, GumballMotionAxis.Y)),
            (global::Rhino.UI.Gumball.GumballMode.RotateZ, new GumballAction(GumballVerb.Rotate, GumballMotionAxis.Z)),
            (global::Rhino.UI.Gumball.GumballMode.ExtrudeX, new GumballAction(GumballVerb.Extrude, GumballMotionAxis.X)),
            (global::Rhino.UI.Gumball.GumballMode.ExtrudeY, new GumballAction(GumballVerb.Extrude, GumballMotionAxis.Y)),
            (global::Rhino.UI.Gumball.GumballMode.ExtrudeZ, new GumballAction(GumballVerb.Extrude, GumballMotionAxis.Z)),
            (global::Rhino.UI.Gumball.GumballMode.CutX, new GumballAction(GumballVerb.Cut, GumballMotionAxis.X)),
            (global::Rhino.UI.Gumball.GumballMode.CutY, new GumballAction(GumballVerb.Cut, GumballMotionAxis.Y)),
            (global::Rhino.UI.Gumball.GumballMode.CutZ, new GumballAction(GumballVerb.Cut, GumballMotionAxis.Z)))
        .ToFrozenDictionary(keySelector: static row => row.Item1, elementSelector: static row => row.Item2);

    public Fin<TGeometry> Apply<TGeometry>(TGeometry geometry, bool duplicate = true) where TGeometry : GeometryBase { Transform transform = TotalTransform; return from _ in guard(transform.IsValid, Op.Of(name: nameof(Apply)).InvalidInput()) from source in Op.Of(name: nameof(Apply)).Need(geometry) from target in duplicate switch { true => Optional(source.Duplicate() as TGeometry).ToFin(Fail: Op.Of(name: nameof(Apply)).InvalidResult()), false => Fin.Succ(value: source) } from __ in target.Transform(xform: transform) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Apply)).InvalidResult()) } select target; }
    public Fin<Seq<TGeometry>> Apply<TGeometry>(IEnumerable<TGeometry> geometry, bool duplicate = true) where TGeometry : GeometryBase { UiGumballSnapshot snapshot = this; return Op.Of(name: nameof(Apply)).Need(geometry).Bind(items => toSeq(items).TraverseM(item => snapshot.Apply(geometry: item, duplicate: duplicate)).As()); }

    public GumballAction Action => Actions.GetValueOrDefault(key: Mode, defaultValue: NoneAction);
}

// One `|`-folded vocabulary over the 14 native appearance enable-bools — replaces hand-set bool-poking per call site.
[Flags]
public enum GumballAxes {
    None = 0,
    TranslateX = 1, TranslateY = 2, TranslateZ = 4, TranslateXY = 8, TranslateYZ = 16, TranslateZX = 32,
    RotateX = 64, RotateY = 128, RotateZ = 256, ScaleX = 512, ScaleY = 1024, ScaleZ = 2048,
    Menu = 4096, Relocate = 8192,
    Translate = TranslateX | TranslateY | TranslateZ | TranslateXY | TranslateYZ | TranslateZX,
    Rotate = RotateX | RotateY | RotateZ,
    Scale = ScaleX | ScaleY | ScaleZ,
    All = Translate | Rotate | Scale | Menu | Relocate,
}

internal static class GumballAxesProjection {
    // BOUNDARY ADAPTER — projects the axis-mask onto the mutable native appearance settings (14 enable bools).
    internal static void ApplyTo(this GumballAxes axes, global::Rhino.UI.Gumball.GumballAppearanceSettings settings) {
        bool H(GumballAxes flag) => (axes & flag) == flag;
        settings.TranslateXEnabled = H(GumballAxes.TranslateX); settings.TranslateYEnabled = H(GumballAxes.TranslateY); settings.TranslateZEnabled = H(GumballAxes.TranslateZ);
        settings.TranslateXYEnabled = H(GumballAxes.TranslateXY); settings.TranslateYZEnabled = H(GumballAxes.TranslateYZ); settings.TranslateZXEnabled = H(GumballAxes.TranslateZX);
        settings.RotateXEnabled = H(GumballAxes.RotateX); settings.RotateYEnabled = H(GumballAxes.RotateY); settings.RotateZEnabled = H(GumballAxes.RotateZ);
        settings.ScaleXEnabled = H(GumballAxes.ScaleX); settings.ScaleYEnabled = H(GumballAxes.ScaleY); settings.ScaleZEnabled = H(GumballAxes.ScaleZ);
        settings.MenuEnabled = H(GumballAxes.Menu); settings.RelocateEnabled = H(GumballAxes.Relocate);
    }
}

public sealed record UiGumballSpec {
    private readonly Func<global::Rhino.UI.Gumball.GumballObject, Fin<Unit>> configure;

    private UiGumballSpec(Func<global::Rhino.UI.Gumball.GumballObject, Fin<Unit>> configure, global::Rhino.UI.Gumball.GumballAppearanceSettings? appearance, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> appearanceActions) {
        this.configure = configure;
        Appearance = Settings(seed: appearance, actions: appearanceActions);
    }

    internal global::Rhino.UI.Gumball.GumballAppearanceSettings? Appearance { get; }

    internal Fin<Unit> Configure(global::Rhino.UI.Gumball.GumballObject gumball) =>
        Op.Of(name: nameof(UiGumballSpec)).Need(configure)
            .Bind(valid => valid(arg: gumball));

    public static UiGumballSpec Of(
        object source,
        Option<Plane> frame = default,
        GumballAxes axes = GumballAxes.All,
        global::Rhino.UI.Gumball.GumballAppearanceSettings? appearance = null,
        params Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>[] appearanceActions) =>
        new(
            configure: gumball => Op.Of(name: nameof(UiGumballSpec)).Need(source).Bind(value => From(gumball: gumball, source: value, frame: frame)),
            appearance: appearance,
            // axis-mask prepended only when it restricts (All => no forced appearance-settings object); user actions can still override.
            appearanceActions: (axes == GumballAxes.All ? Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>>() : Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>>(s => axes.ApplyTo(settings: s))) + toSeq(appearanceActions));

    private static Fin<Unit> From(global::Rhino.UI.Gumball.GumballObject gumball, object source, Option<Plane> frame) =>
        (frame.Case, source) switch {
            (Plane plane, BoundingBox box) => Valid(gumball.SetFromBoundingBox(frame: plane, frameBoundingBox: box)),
            (Plane plane, GeometryBase geometry) => Bounds(source: geometry, frame: Some(plane)).Bind(box => Valid(gumball.SetFromBoundingBox(frame: plane, frameBoundingBox: box))),
            (_, BoundingBox box) => Valid(gumball.SetFromBoundingBox(boundingBox: box)),
            (_, Line line) => Valid(gumball.SetFromLine(line: line)),
            (_, Plane plane) => Valid(gumball.SetFromPlane(plane: plane)),
            (_, Arc arc) => Valid(gumball.SetFromArc(arc: arc)),
            (_, Circle circle) => Valid(gumball.SetFromCircle(circle: circle)),
            (_, Ellipse ellipse) => Valid(gumball.SetFromEllipse(ellipse: ellipse)),
            (_, Light light) => Valid(gumball.SetFromLight(light: light)),
            (_, Hatch hatch) => Valid(gumball.SetFromHatch(hatch: hatch)),
            (_, Curve curve) => Valid(gumball.SetFromCurve(curve: curve)),
            (_, Extrusion extrusion) => Valid(gumball.SetFromExtrusion(extrusion: extrusion)),
            (_, object value) => Bounds(source: value, frame: Option<Plane>.None).Bind(box => Valid(gumball.SetFromBoundingBox(boundingBox: box))),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidInput()),
        };

    private static Fin<Unit> Valid(bool value) => value switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()) };

    private static Fin<BoundingBox> Bounds(object source, Option<Plane> frame) => (frame.Case, source) switch { (Plane plane, GeometryBase geometry) => Op.Of(name: nameof(UiGumballSpec)).Need(geometry).Bind(value => value.GetBoundingBox(plane: plane) switch { BoundingBox box when box.IsValid => Fin.Succ(value: box), _ => Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()) }), (_, object value) => OverlayDecision.BoundsOf(source: value, op: Op.Of(name: nameof(UiGumballSpec))).Bind(static box => box.IsValid ? Fin.Succ(value: box) : Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult())) };

    private static global::Rhino.UI.Gumball.GumballAppearanceSettings? Settings(global::Rhino.UI.Gumball.GumballAppearanceSettings? seed, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> actions) => (seed, actions.IsEmpty) switch { (global::Rhino.UI.Gumball.GumballAppearanceSettings settings, false) => Apply(settings: settings, actions: actions), (_, false) => Apply(settings: new global::Rhino.UI.Gumball.GumballAppearanceSettings(), actions: actions), _ => seed };

    private static global::Rhino.UI.Gumball.GumballAppearanceSettings Apply(global::Rhino.UI.Gumball.GumballAppearanceSettings settings, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> actions) { _ = actions.Iter(action => action(obj: settings)); return settings; }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmOverlay<TState>(TState initial) : DisplayConduit, IDisposable {
    private readonly Atom<TState> state = Atom(initial);
    private bool disposed;

    public TState State => state.Value;

    // Single disposed-gate rail; per-arm Op.Of(nameof(...)) provenance preserved at each call site.
    private Fin<T> Live<T>(Op op, Func<Fin<T>> body) =>
        from _ in guard(!disposed, op.InvalidInput())
        from result in body()
        select result;

    public Fin<Unit> Transition(Func<TState, TState> transition, RhinoDoc? document = null) =>
        Live(op: Op.Of(name: nameof(Transition)), body: () => Op.Of(name: nameof(Transition)).Need(transition)
            .Map(apply => { _ = state.Swap(f: apply); _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }));

    public Fin<Unit> Enable(bool enabled = true, RhinoDoc? document = null) =>
        Live(op: Op.Of(name: nameof(Enable)), body: () => Fin.Succ(value: Op.Side(() => { Enabled = enabled; _ = Optional(document).Iter(static doc => doc.Views.Redraw()); })));

    public Fin<Unit> Filter(OverlayFilter filter, RhinoDoc? document = null) =>
        Live(op: Op.Of(name: nameof(Filter)), body: () => filter.Apply(conduit: this).Map(_ => { _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }));

    public Fin<T> Use<T>(RhinoDoc document, Func<RasmOverlay<TState>, Fin<T>> run) =>
        from validDocument in Op.Of(name: nameof(Use)).Need(document)
        from validRun in Op.Of(name: nameof(Use)).Need(run)
        from result in Live(op: Op.Of(name: nameof(Use)), body: () => RhinoUi.Protect(valid: () => {
            Enabled = true;
            validDocument.Views.Redraw();
            // BOUNDARY ADAPTER - display conduit lifetime must be closed after the caller rail exits.
            try { return validRun(arg: this); } finally { Dispose(); validDocument.Views.Redraw(); }
        }))
        select result;

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(obj: this);
    }

    protected virtual void Dispose(bool disposing) {
        _ = disposing;
        _ = disposed switch {
            true => unit,
            false => Disable(),
        };
        disposed = true;
    }

    private Unit Disable() {
        Enabled = false;
        return unit;
    }

    protected virtual Fin<OverlayDecision> Change(OverlayContext<TState> context) =>
        Fin.Succ(value: OverlayDecision.Ignore);

    protected sealed override void OnEnable(bool enable) =>
        _ = Apply(phase: OverlayPhase.Enabled, args: null, enabled: enable);

    protected sealed override void ObjectCulling(CullObjectEventArgs e) =>
        _ = Apply(phase: OverlayPhase.Cull, args: e);

    protected sealed override void PreDrawObjects(DrawEventArgs e) =>
        _ = Apply(phase: OverlayPhase.PreDrawObjects, args: e);

    protected sealed override void PreDrawObject(DrawObjectEventArgs e) =>
        _ = Apply(phase: OverlayPhase.PreDrawObject, args: e);

    protected sealed override void DrawForeground(DrawEventArgs e) =>
        _ = Apply(phase: OverlayPhase.Foreground, args: e);

    protected sealed override void DrawOverlay(DrawEventArgs e) =>
        _ = Apply(phase: OverlayPhase.Overlay, args: e);

    protected sealed override void PostDrawObjects(DrawEventArgs e) =>
        _ = Apply(phase: OverlayPhase.PostDraw, args: e);

    protected sealed override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) =>
        _ = Apply(phase: OverlayPhase.Bounds, args: e);

    protected sealed override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) =>
        _ = Apply(phase: OverlayPhase.ZoomBounds, args: e);

    private Fin<Unit> Apply(OverlayPhase phase, object? args, bool enabled = false) =>
        RhinoUi.Protect(valid: () =>
            Change(context: new OverlayContext<TState>(Phase: phase, State: State, Args: args, Enabled: enabled))
                .Bind(decision => ((Optional(args as DrawEventArgs).Case, Optional(args as DrawObjectEventArgs).Case, decision.Draw.Case, decision.DrawObject.Case) switch {
                    (_, DrawObjectEventArgs target, _, Func<DrawObjectEventArgs, Fin<Unit>> paint) => paint(arg: target),
                    (DrawEventArgs target, _, Func<DisplayPipeline, Fin<Unit>> paint, _) => paint(arg: target.Display),
                    _ => Fin.Succ(value: unit),
                }).Map(_ => {
                    _ = Optional(args as CalculateBoundingBoxEventArgs).Iter(target => decision.Bounds.Iter(box => target.IncludeBoundingBox(box)));
                    _ = Optional(args as CullObjectEventArgs).Iter(target => decision.Cull.Iter(value => target.CullObject = value));
                    return unit;
                })));
}

public sealed class UiGumball : IDisposable {
    private readonly RhinoDoc document;
    private readonly global::Rhino.UI.Gumball.GumballDisplayConduit conduit;
    private bool disposed;

    private UiGumball(RhinoDoc document, global::Rhino.UI.Gumball.GumballDisplayConduit conduit) {
        this.document = document;
        this.conduit = conduit;
    }

    public UiGumballSnapshot Snapshot =>
        new(
            PreTransform: conduit.PreTransform,
            GumballTransform: conduit.GumballTransform,
            TotalTransform: conduit.TotalTransform,
            Mode: conduit.PickResult.Mode,
            InRelocate: conduit.InRelocate,
            Frame: Some(conduit.Gumball.Frame));   // live frame plane/scale-mode (downstream constraint/relocate reads it without re-deriving)

    public Fin<bool> Pick(PickContext pick, GetPoint point) =>
        from _ in guard(!disposed, Op.Of(name: nameof(Pick)).InvalidInput())
        from validPick in Op.Of(name: nameof(Pick)).Need(pick)
        from validPoint in Op.Of(name: nameof(Pick)).Need(point)
        from picked in RhinoUi.Protect(valid: () => Fin.Succ(value: conduit.PickGumball(pickContext: validPick, getPoint: validPoint)))
        select picked;

    // Single disposed+valid gate over the native UpdateGumball boundary (now Op.Catch-capsuled); both arms redraw on commit.
    private Fin<bool> Drive(Op op, bool valid, Func<bool> update) =>
        from _ in guard(!disposed && valid, op.InvalidInput())
        from changed in op.Catch(() => { bool c = update(); document.Views.Redraw(); return Fin.Succ(value: c); })
        select changed;

    public Fin<bool> Update(Point3d point, Line line) =>
        Drive(op: Op.Of(name: nameof(Update)), valid: point.IsValid && line.IsValid, update: () => conduit.UpdateGumball(point: point, worldLine: line));

    public Fin<bool> Update(Plane frame) =>
        Drive(op: Op.Of(name: nameof(Update)), valid: frame.IsValid, update: () => conduit.UpdateGumball(frame: frame));

    // Snap/grid/axis-lock constraint injection: pre-compose a transform applied before the gumball drag (closes the
    // read/constrain/apply cycle alongside Snapshot's read and Update's apply).
    public Fin<Unit> Constrain(Transform pre) =>
        from _ in guard(!disposed && pre.IsValid, Op.Of(name: nameof(Constrain)).InvalidInput())
        from done in RhinoUi.Protect(valid: () => Fin.Succ(value: Op.Side(() => { conduit.PreTransform = pre; document.Views.Redraw(); })))
        select done;

    public Fin<Unit> Reconfigure(UiGumballSpec spec) =>
        from _ in guard(!disposed, Op.Of(name: nameof(Reconfigure)).InvalidInput())
        from valid in Op.Of(name: nameof(Reconfigure)).Need(spec)
        from result in RhinoUi.Protect(valid: () => {
            global::Rhino.UI.Gumball.GumballObject source = new();
            // BOUNDARY ADAPTER - source is copied into the conduit; dispose after handoff.
            try {
                return valid.Configure(gumball: source).Map(_ => {
                    conduit.SetBaseGumball(gumball: source, appearanceSettings: valid.Appearance);
                    document.Views.Redraw();
                    return unit;
                });
            } finally {
                source.Dispose();
            }
        })
        select result;

    public Fin<Unit> CheckKeys() =>
        disposed switch {
            false => RhinoUi.Protect(valid: () => Fin.Succ(value: Op.Side(conduit.CheckShiftAndControlKeys))),
            true => Fin.Fail<Unit>(error: Op.Of(name: nameof(CheckKeys)).InvalidInput()),
        };

    public void Dispose() {
        _ = disposed switch {
            true => unit,
            false => Release(),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<T> Use<T>(RhinoDoc document, UiGumballSpec spec, Func<UiGumball, Fin<T>> run) =>
        from validDocument in Op.Of(name: nameof(UiGumball)).Need(document)
        from validSpec in Op.Of(name: nameof(UiGumball)).Need(spec)
        from scope in RhinoUi.Protect(valid: () => Create(document: validDocument, spec: validSpec))
        from result in RhinoUi.Protect(valid: () => {
            // BOUNDARY ADAPTER - gumball/conduit are native disposable view state.
            try { return run(arg: scope); } finally { scope.Dispose(); }
        })
        select result;

    private static Fin<UiGumball> Create(RhinoDoc document, UiGumballSpec spec) {
        global::Rhino.UI.Gumball.GumballObject? nativeSource = new();
        // BOUNDARY ADAPTER - native gumball ownership transfers only after conduit creation succeeds.
        try {
            return spec.Configure(gumball: nativeSource).Bind(_ => RhinoUi.Protect(valid: () => {
                global::Rhino.UI.Gumball.GumballDisplayConduit? nativeConduit = new(document.ActiveSpace);
                try {
                    nativeConduit.SetBaseGumball(gumball: nativeSource, appearanceSettings: spec.Appearance);
                    nativeConduit.Enabled = true;
                    document.Views.Redraw();
                    UiGumball scope = new(document: document, conduit: nativeConduit);
                    nativeConduit = null;
                    return Fin.Succ(value: scope);
                } finally {
                    nativeConduit?.Dispose();
                }
            }));
        } finally {
            nativeSource?.Dispose();
        }
    }

    private Unit Release() {
        try {
            conduit.Enabled = false;
            document.Views.Redraw();
        } finally {
            conduit.Dispose();
        }
        return unit;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class UiViewportRequest {
    public static UiIntent<T> Preview<T>(UiViewportPreview preview, Func<UiPreviewScope, Fin<T>> run, Option<UiGumballSpec> gumball = default, bool interactive = true) =>
        UiIntent.OfScope(run: scope => UiViewportPreview.Use(document: scope.Document, preview: preview, gumball: gumball, run: run), interactive: interactive);

    public static UiIntent<T> Interaction<TState, T>(UiViewportInteraction<TState> interaction, Func<UiPreviewScope, Fin<T>> run) =>
        UiIntent.OfScope(run: scope =>
            from active in Op.Of(name: nameof(Interaction)).Need(interaction)
            from result in active.Use(document: scope.Document, run: run)
            select result, interactive: true);
}
