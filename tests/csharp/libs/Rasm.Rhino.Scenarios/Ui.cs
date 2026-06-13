using Eto.Forms;
using Rasm.Bridge.Scenarios;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Scenarios;

// --- [OPERATIONS] ---------------------------------------------------------------------------

// Ownership: the UI theme — motion handles (decay/spring CAS retarget), overlay filter monoid
// through a live display conduit, gumball updates, screen-space HUD marks with live capture, the
// UiCanvas state rail, collapsible sections, and the world-anchored projection + HUD-stacking
// rails (ScreenPoint, Visible occlusion, StackMany band carving).
internal static class UiScenarios {
    [RhinoScenario(theme: "ui")]
    internal static Fin<Unit> MotionOverlay(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        from view in Optional(scope.Doc.Views.ActiveView).ToFin(Fail: Error.New(message: "no active view"))
        from context in ctx.Expect(label: "context", projection: RhinoCommandContext.Of(doc: scope.Doc, mode: RunMode.Scripted))
        let ui = context.Ui
        from decayChecked in DecayProbe(ctx: ctx, ui: ui, view: view)
        from springChecked in SpringProbe(ctx: ctx, ui: ui, view: view)
        let composed = new OverlayFilter(Geometry: Some(ObjectType.Curve)) + new OverlayFilter(Space: Some(ActiveSpace.ModelSpace))
        let filterFact = Note(ctx: ctx, key: "filter.geometry", value: Text(value: composed.Geometry))
        from filtered in ctx.Expect(label: "overlay filter compose + reset", projection: ui.Use(UiViewportRequest.Preview<Unit>(
            preview: UiViewportPreview.Empty,
            run: previewScope =>
                from bound in previewScope.Overlay.Filter(filter: composed, document: scope.Doc)
                from cleared in previewScope.Overlay.Filter(filter: composed + OverlayFilter.Reset, document: scope.Doc)
                select cleared,
            interactive: false)))
        let applyFact = Note(ctx: ctx, key: "filter.apply.ok", value: true)
        let region = new BoundingBox(new Point3d(0.0, 0.0, 0.0), new Point3d(8.0, 5.0, 3.0))
        from changed in ctx.Expect(label: "gumball frame update", projection: ui.Use(UiIntent.Gumball<bool>(
            spec: UiGumballSpec.Of(source: region),
            run: gumball => gumball.Update(frame: new Plane(origin: new Point3d(1.0, 1.0, 0.0), normal: Vector3d.ZAxis)),
            interactive: false)))
        let changedFact = Note(ctx: ctx, key: "gumball.changed", value: changed)
        select Done(scope: scope);

    [RhinoScenario(theme: "ui")]
    internal static Fin<Unit> Paint(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        let hud = ScreenHud()
        from markCount in ctx.Require(label: "hud marks", observed: hud.Marks.Count == 5)
        let marksFact = Note(ctx: ctx, key: "hud.marks", value: hud.Marks.Count)
        from context in ctx.Expect(label: "context", projection: RhinoCommandContext.Of(doc: scope.Doc, mode: RunMode.Scripted))
        let ui = context.Ui
        let hudPreview = UiViewportPreview.Hud(hud: layout => Fin.Succ(value: hud))
        from hudRendered in ctx.Expect(label: "hud overlay render (text+stroke+box+curve+radial)", projection: ui.Use(UiViewportRequest.Preview<Unit>(
            preview: hudPreview,
            run: previewScope => HudCapture(ctx: ctx, doc: scope.Doc),
            interactive: false)))
        let hudFact = Note(ctx: ctx, key: "hud.render.ok", value: true)
        from dottedRendered in ctx.Expect(label: "dotted curve preview render", projection: ui.Use(UiViewportRequest.Preview<Unit>(
            preview: DottedPreview(),
            run: previewScope => RedrawFin(doc: scope.Doc),
            interactive: false)))
        let dottedFact = Note(ctx: ctx, key: "dotted.render.ok", value: true)
        from canvasChecked in CanvasRail(ctx: ctx, hud: hud)
        from sectionA in ctx.Expect(label: "section A", projection: RasmSection.Of(caption: "Alpha", sectionHeight: 120, content: new Panel(), expanded: true))
        from sectionB in ctx.Expect(label: "section B", projection: RasmSection.Of(caption: "Beta", sectionHeight: 96, content: new Panel(), expanded: false))
        from holder in ctx.Expect(label: "sections holder", projection: PanelOp.Sections(sections: Seq(sectionA, sectionB), scrollbars: true))
        let sectionCount = holder.Sections.Count()
        from sectionLaw in ctx.Require(label: "sections count", observed: sectionCount == 2)
        let sectionsFact = Note(ctx: ctx, key: "sections.count", value: sectionCount)
        let region = new BoundingBox(new Point3d(0.0, 0.0, 0.0), new Point3d(10.0, 6.0, 4.0))
        let spec0 = UiGumballSpec.Of(source: region)
        let spec1 = UiGumballSpec.Of(source: region, frame: Some(new Plane(origin: new Point3d(2.0, 2.0, 0.0), normal: Vector3d.ZAxis)))
        from reconfigured in ctx.Expect(label: "gumball reconfigure", projection: ui.Use(UiIntent.Gumball<Unit>(
            spec: spec0,
            run: gumball => gumball.Reconfigure(spec: spec1),
            interactive: false)))
        let reconfigureFact = Note(ctx: ctx, key: "gumball.reconfigure.ok", value: true)
        select Done(scope: scope);

    [RhinoScenario(theme: "ui")]
    internal static Fin<Unit> ProjectionHud(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        from brep in BoxBrep()
        let boxId = scope.Doc.Objects.AddBrep(brep: brep)
        from view in Optional(scope.Doc.Views.ActiveView).ToFin(Fail: Error.New(message: "no active view"))
        let vp = view.ActiveViewport
        let framed = Frame(doc: scope.Doc, viewport: vp)
        let camera = RhinoCamera.Live(document: scope.Doc, mode: RunMode.Scripted)
        let target = (ViewportTarget)new ViewportTarget.Current()
        let onScreen = new Point3d(x: 0.0, y: 0.0, z: 5.0)
        from px in ctx.Expect(label: "screen point on-frustum", projection: camera.RunValue(operation: CameraOps.Query(query: cs => cs.ScreenPoint(point: onScreen)), target: target))
        let port = vp.Size
        let inPort = px.X >= 0f && px.X <= port.Width && px.Y >= 0f && px.Y <= port.Height
        let screenXFact = Note(ctx: ctx, key: "screen.x", value: px.X)
        let screenYFact = Note(ctx: ctx, key: "screen.y", value: px.Y)
        let portWidthFact = Note(ctx: ctx, key: "port.width", value: port.Width)
        let portHeightFact = Note(ctx: ctx, key: "port.height", value: port.Height)
        let inPortFact = Note(ctx: ctx, key: "screen.inPort", value: inPort)
        from inPortLaw in ctx.Require(label: "projected pixel inside port", observed: inPort)
        from invalidRejected in Rejected(outcome: camera.RunValue(operation: CameraOps.Query(query: cs => cs.ScreenPoint(point: Point3d.Unset)), target: target), detail: "screen point invalid: expected rejection")
        from onScreenVisible in ctx.Expect(label: "visible on-screen", projection: camera.RunValue(operation: CameraOps.Query(query: cs => cs.Visible(source: new CameraSubject.AtPoint(Value: onScreen))), target: target))
        let behind = vp.CameraLocation - (vp.CameraDirection * 100.0)
        from behindVisible in ctx.Expect(label: "visible behind-camera", projection: camera.RunValue(operation: CameraOps.Query(query: cs => cs.Visible(source: new CameraSubject.AtPoint(Value: behind))), target: target))
        let visibleFact = Note(ctx: ctx, key: "visible.onScreen", value: onScreenVisible)
        let behindFact = Note(ctx: ctx, key: "visible.behind", value: behindVisible)
        from visibleLaw in ctx.Require(label: "framed box-centre visible", observed: onScreenVisible)
        from culledLaw in ctx.Require(label: "behind-camera point culled", observed: !behindVisible)
        let layout = UiHudLayout.Of(viewport: vp, dpiScale: 2f)
        let stacked = UiHudLayout.StackMany(
            layout: layout,
            items: Seq(
                (UiAnchor.TopLeft, new System.Drawing.SizeF(120f, 24f)),
                (UiAnchor.TopCenter, new System.Drawing.SizeF(80f, 24f)),
                (UiAnchor.BottomCenter, new System.Drawing.SizeF(200f, 32f))))
        let regions = stacked.Regions
        let withinBounds = regions.ForAll(layout.Bounds.Contains)
        let nonOverlap = !regions[0].IntersectsWith(rect: regions[1])
            && !regions[0].IntersectsWith(rect: regions[2])
            && !regions[1].IntersectsWith(rect: regions[2])
        let regionsFact = Note(ctx: ctx, key: "hud.regions", value: regions.Count)
        let boundsFact = Note(ctx: ctx, key: "hud.withinBounds", value: withinBounds)
        let overlapFact = Note(ctx: ctx, key: "hud.nonOverlap", value: nonOverlap)
        from regionCountLaw in ctx.Require(label: "three stacked regions", observed: regions.Count == 3)
        from boundsLaw in ctx.Require(label: "regions within viewport bounds", observed: withinBounds)
        from overlapLaw in ctx.Require(label: "regions non-overlapping", observed: nonOverlap)
        select Done(scope: scope);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "Ownership transfers into the Fin rail; the document copies the geometry and the transient brep is finalizer-released.")]
    private static Fin<Brep> BoxBrep() =>
        Optional(Brep.CreateFromBox(new Box(
            Plane.WorldXY,
            new Interval(t0: -20.0, t1: 20.0),
            new Interval(t0: -20.0, t1: 20.0),
            new Interval(t0: 0.0, t1: 10.0))))
        .ToFin(Fail: Error.New(message: "box brep construction failed"));

    // UiCanvas owns a sprite atlas + state cell; paint is not driven headless, so only the state
    // rail is verified.
    private static Fin<Unit> CanvasRail(ScenarioContext ctx, UiHud hud) {
        using UiCanvas<int> canvas = new(initial: 0, paint: (state, painter) => Fin.Succ(value: hud));
        return ctx.Require(label: "canvas initial state", observed: canvas.State == 0)
            .Bind(ignored => ctx.Expect(label: "canvas transition", projection: canvas.Transition(transition: count => count + 1)))
            .Bind(ignored => ctx.Require(label: "canvas transitioned state", observed: canvas.State == 1))
            .Map(ignored => Note(ctx: ctx, key: "canvas.state", value: canvas.State))
            .Map(ignored => unit);
    }

    // BOUNDARY ADAPTER — motion handle bracket: the decay sink mutation and the handle's
    // disposal both live outside the Fin comprehension dialect.
    private static Fin<Unit> DecayProbe(ScenarioContext ctx, RhinoUi ui, RhinoView view) {
        double decaySunk = double.NaN;
        Fin<MotionHandle<double, double>> acquired = ctx.Expect(
            label: "decay animate handle",
            projection: ui.Use(UiViewportRequest.Animate<double, double>(
                spec: MotionSpec.Decay<double, double>(from: 0.0, velocity: 10.0, friction: 5.0, vector: MotionVector.Double),
                view: view,
                sink: value => decaySunk = value,
                timeSource: TimeProvider.System)));
        if (acquired is not Fin<MotionHandle<double, double>>.Succ(MotionHandle<double, double> decay)) {
            return acquired.Map(handle => unit);
        }
        using (decay) {
            _ = Note(ctx: ctx, key: "decay.initialSunk", value: decaySunk);
            return ctx.Expect(label: "decay retarget", projection: decay.Retarget(target: 0.0, velocity: Some(20.0)))
                .Map(ignored => Note(ctx: ctx, key: "decay.velocity", value: decay.Velocity))
                .Bind(ignored => ctx.Require(label: "decay sink received an initial value", observed: !double.IsNaN(d: decaySunk)));
        }
    }

    private static Unit Done(DocumentScope scope) {
        scope.Dispose();
        return unit;
    }

    private static UiViewportPreview DottedPreview() =>
        UiViewportPreview.Of(
            geometry: new Curve[] { new LineCurve(new Point3d(0.0, 0.0, 0.0), new Point3d(12.0, 8.0, 0.0)) },
            style: new UiPreviewStyle(Stroke: Some(UiStroke.Of(System.Drawing.Color.Lime, 2f)), Dotted: true));

    private static Unit Frame(RhinoDoc doc, RhinoViewport viewport) {
        _ = viewport.ZoomExtents();
        doc.Views.Redraw();
        return unit;
    }

    // BOUNDARY ADAPTER — live view capture inside the preview conduit; the bitmap is a native
    // handle owned by this bracket.
    private static Fin<Unit> HudCapture(ScenarioContext ctx, RhinoDoc doc) {
        if (doc.Views.ActiveView is not RhinoView view) {
            return Fin.Fail<Unit>(error: Error.New(message: "no active view"));
        }
        using System.Drawing.Bitmap? bitmap = view.CaptureToBitmap();
        if (bitmap is null) {
            return Fin.Fail<Unit>(error: Error.New(message: "capture returned null"));
        }
        _ = Note(ctx: ctx, key: "hud.capture.w", value: bitmap.Width);
        _ = Note(ctx: ctx, key: "hud.capture.h", value: bitmap.Height);
        return Fin.Succ(value: unit);
    }

    private static T Note<T>(ScenarioContext ctx, string key, T value) {
        ctx.Fact(key: key, value: value);
        return value;
    }

    private static Fin<Unit> RedrawFin(RhinoDoc doc) {
        doc.Views.Redraw();
        return Fin.Succ(value: unit);
    }

    private static Fin<Unit> Rejected<T>(Fin<T> outcome, string detail) =>
        outcome switch {
            Fin<T>.Fail => Fin.Succ(value: unit),
            _ => Fin.Fail<Unit>(error: Error.New(message: detail)),
        };

    // Text + stroke + solid box + segmented curve (Line/Arc/Bezier) + radial-fill box: the full
    // screen-space mark surface.
    private static UiHud ScreenHud() =>
        UiHud.Empty
            .Add(new UiMark.Text(Value: "rasm-hud", At: new System.Drawing.PointF(8f, 8f), Color: System.Drawing.Color.White, Font: UiFont.Family("Arial", 14f)))
            .Add(new UiMark.Stroke(From: new System.Drawing.PointF(8f, 28f), To: new System.Drawing.PointF(168f, 28f), Pen: UiStroke.Of(System.Drawing.Color.Cyan, 2f)))
            .Add(new UiMark.Box(Bounds: new System.Drawing.Rectangle(8, 36, 160, 48), Outline: UiStroke.Of(System.Drawing.Color.Yellow, 1f), Fill: new UiFill.Solid(System.Drawing.Color.FromArgb(48, 48, 48))))
            .Add(new UiMark.Curve(
                Segs: Seq<UiCurveSeg>(
                    new UiCurveSeg.Line(From: new System.Drawing.PointF(8f, 96f), To: new System.Drawing.PointF(48f, 96f)),
                    new UiCurveSeg.Arc(Bounds: new System.Drawing.RectangleF(48f, 80f, 32f, 32f), StartAngle: 180f, SweepAngle: 180f),
                    new UiCurveSeg.Bezier(Start: new System.Drawing.PointF(80f, 96f), Control1: new System.Drawing.PointF(96f, 64f), Control2: new System.Drawing.PointF(128f, 128f), End: new System.Drawing.PointF(160f, 96f))),
                Pen: UiStroke.Of(System.Drawing.Color.Orange, 2f)))
            .Add(new UiMark.Box(Bounds: new System.Drawing.Rectangle(8, 116, 160, 24), Outline: UiStroke.Of(System.Drawing.Color.Magenta, 1f), Fill: new UiFill.Radial(Start: System.Drawing.Color.Red, End: System.Drawing.Color.Blue, Center: new System.Drawing.PointF(88f, 128f), Origin: new System.Drawing.PointF(88f, 128f), Radius: new System.Drawing.SizeF(80f, 12f))));

    // BOUNDARY ADAPTER — spring handle bracket: live retarget toward a new target (CAS the
    // runner cell, no driver restart).
    private static Fin<Unit> SpringProbe(ScenarioContext ctx, RhinoUi ui, RhinoView view) {
        Fin<MotionHandle<double, double>> acquired = ctx.Expect(
            label: "spring animate handle",
            projection: ui.Use(UiViewportRequest.Animate<double, double>(
                spec: MotionSpec.Spring<double, double>(from: 0.0, to: 100.0, config: SpringPreset.Snappy.Config, vector: MotionVector.Double),
                view: view,
                sink: value => { },
                timeSource: TimeProvider.System)));
        if (acquired is not Fin<MotionHandle<double, double>>.Succ(MotionHandle<double, double> spring)) {
            return acquired.Map(handle => unit);
        }
        using (spring) {
            return ctx.Expect(label: "spring retarget", projection: spring.Retarget(target: 50.0))
                .Map(ignored => Note(ctx: ctx, key: "spring.velocity", value: spring.Velocity))
                .Map(ignored => unit);
        }
    }

    private static string Text(object? value) =>
        Convert.ToString(value: value, provider: System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
}
