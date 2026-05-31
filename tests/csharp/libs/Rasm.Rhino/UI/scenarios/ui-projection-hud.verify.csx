using System;
using Rasm.Rhino.Camera;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;

// Runtime laws for the world-anchored projection + HUD-stacking rails (all native: GetTransform/IsVisible P/Invoke, and
// UiHudLayout.Of reads the live viewport Size). Three directly-observable surfaces are asserted here:
//   (1) CameraScope.ScreenPoint  — on-frustum world point projects to an in-port pixel; Point3d.Unset fails the guard.
//   (2) CameraScope.Visible      — the occlusion DECISION backing UiPreviewContext.Label (in-view => true, behind-camera => false).
//   (3) UiHudLayout.StackMany    — top/bottom anchors carve non-overlapping bands within the real port bounds.
// Out of scope by design (no falsifiable fact reachable from a public, non-conduit surface): ProjectionMemo caching
// (internal type) and UiRenderState.Screen2d / UiPreviewContext.Label drawing (need a live DisplayPipeline that only
// exists inside a conduit draw, whose per-frame Fin is swallowed by OverlayDecision.Paint). SmartEnum `.Items` round-trips
// are pure-managed and belong in static specs. ScreenPoint has no clipping, so behind-camera still projects to Succ —
// the cull is proven through Visible, not ScreenPoint.
Scenario.Run("ui-projection-hud", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    scope.Clear();
    facts.Add("mainThread", RhinoApp.IsOnMainThread);

    // Frame a box centred on the origin so a known world point lands on-screen and the camera frustum is well-defined.
    Brep brep = Brep.CreateFromBox(new Box(
        Plane.WorldXY,
        new Interval(t0: -20.0, t1: 20.0),
        new Interval(t0: -20.0, t1: 20.0),
        new Interval(t0: 0.0, t1: 10.0))) ?? throw new InvalidOperationException(message: "box brep");
    _ = scope.Active.Objects.AddBrep(brep: brep);
    RhinoView view = scope.Active.Views.ActiveView ?? throw new InvalidOperationException(message: "no active view");
    RhinoViewport vp = view.ActiveViewport;
    _ = vp.ZoomExtents();
    scope.Active.Views.Redraw();

    RhinoCamera camera = RhinoCamera.Live(document: scope.Active, mode: RunMode.Scripted);
    ViewportTarget target = new ViewportTarget.Current();
    Point3d onScreen = new Point3d(x: 0.0, y: 0.0, z: 5.0);   // box centre — framed, so projects inside the port

    // --- (1) ScreenPoint: on-frustum -> in-port pixel; invalid -> Fail -----------------------
    System.Drawing.PointF px = Probe.Expect(
        camera.RunValue(operation: CameraOps.Query(query: cs => cs.ScreenPoint(point: onScreen)), target: target),
        "screen point on-frustum",
        facts);
    System.Drawing.Size port = vp.Size;
    bool inPort = px.X >= 0f && px.X <= port.Width && px.Y >= 0f && px.Y <= port.Height;
    facts.Add("screen.x", px.X);
    facts.Add("screen.y", px.Y);
    facts.Add("port.width", port.Width);
    facts.Add("port.height", port.Height);
    facts.Add("screen.inPort", inPort);
    Probe.Require(inPort, $"projected pixel ({px.X},{px.Y}) outside port {port.Width}x{port.Height}");
    Probe.ExpectRejected(
        camera.RunValue(operation: CameraOps.Query(query: cs => cs.ScreenPoint(point: Point3d.Unset)), target: target),
        "screen point invalid -> Fail");

    // --- (2) Visible occlusion decision (backs UiPreviewContext.Label) ------------------------
    bool onScreenVisible = Probe.Expect(
        camera.RunValue(operation: CameraOps.Query(query: cs => cs.Visible(source: new CameraSubject.AtPoint(Value: onScreen))), target: target),
        "visible on-screen",
        facts);
    Point3d behind = vp.CameraLocation - (vp.CameraDirection * 100.0);   // 100 units behind the camera -> outside the frustum
    bool behindVisible = Probe.Expect(
        camera.RunValue(operation: CameraOps.Query(query: cs => cs.Visible(source: new CameraSubject.AtPoint(Value: behind))), target: target),
        "visible behind-camera",
        facts);
    facts.Add("visible.onScreen", onScreenVisible);
    facts.Add("visible.behind", behindVisible);
    Probe.Require(onScreenVisible, "framed box-centre must be visible");
    Probe.Require(!behindVisible, $"behind-camera point {behind} must be culled (Label occlusion gate)");

    // --- (3) UiHudLayout.Of + StackMany: non-overlapping bands within the live port -----------
    UiHudLayout layout = UiHudLayout.Of(viewport: vp, dpiScale: 2f);
    (Seq<System.Drawing.RectangleF> regions, _) = UiHudLayout.StackMany(
        layout: layout,
        items: Seq(
            (UiAnchor.TopLeft, new System.Drawing.SizeF(120f, 24f)),
            (UiAnchor.TopCenter, new System.Drawing.SizeF(80f, 24f)),
            (UiAnchor.BottomCenter, new System.Drawing.SizeF(200f, 32f))));
    bool withinBounds = regions.ForAll(r => layout.Bounds.Contains(r));
    bool nonOverlap = !regions[0].IntersectsWith(regions[1])
        && !regions[0].IntersectsWith(regions[2])
        && !regions[1].IntersectsWith(regions[2]);
    facts.Add("hud.regions", regions.Count);
    facts.Add("hud.withinBounds", withinBounds);
    facts.Add("hud.nonOverlap", nonOverlap);
    Probe.Require(regions.Count == 3, $"expected 3 stacked regions, got {regions.Count}");
    Probe.Require(withinBounds, "stacked HUD regions escaped the viewport bounds");
    Probe.Require(nonOverlap, "stacked HUD regions overlap");
});
