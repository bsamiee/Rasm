using System;
using System.Linq;
using Rasm.Domain;
using Rasm.Rhino.UI;
using Rasm.Rhino.Commands;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;
using Eto.Forms;

Scenario.Run("ui-paint", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    facts.Add("mainThread", RhinoApp.IsOnMainThread);

    // Text + stroke + solid box + segmented curve (Line/Arc/Bezier) + radial-fill box: the full screen-space mark surface.
    UiHud hud = UiHud.Empty
        .Add(new UiMark.Text(Value: "rasm-hud", At: new System.Drawing.PointF(8f, 8f), Color: System.Drawing.Color.White, Font: UiFont.Of("Arial", 14f)))
        .Add(new UiMark.Stroke(From: new System.Drawing.PointF(8f, 28f), To: new System.Drawing.PointF(168f, 28f), Pen: UiStroke.Of(System.Drawing.Color.Cyan, 2f)))
        .Add(new UiMark.Box(Bounds: new System.Drawing.Rectangle(8, 36, 160, 48), Outline: UiStroke.Of(System.Drawing.Color.Yellow, 1f), Fill: new UiFill.Solid(System.Drawing.Color.FromArgb(48, 48, 48))))
        .Add(new UiMark.Curve(
            Segs: Seq<UiCurveSeg>(
                new UiCurveSeg.Line(From: new System.Drawing.PointF(8f, 96f), To: new System.Drawing.PointF(48f, 96f)),
                new UiCurveSeg.Arc(Bounds: new System.Drawing.RectangleF(48f, 80f, 32f, 32f), StartAngle: 180f, SweepAngle: 180f),
                new UiCurveSeg.Bezier(Start: new System.Drawing.PointF(80f, 96f), Control1: new System.Drawing.PointF(96f, 64f), Control2: new System.Drawing.PointF(128f, 128f), End: new System.Drawing.PointF(160f, 96f))),
            Pen: UiStroke.Of(System.Drawing.Color.Orange, 2f)))
        .Add(new UiMark.Box(Bounds: new System.Drawing.Rectangle(8, 116, 160, 24), Outline: UiStroke.Of(System.Drawing.Color.Magenta, 1f), Fill: new UiFill.Radial(Start: System.Drawing.Color.Red, End: System.Drawing.Color.Blue, Center: new System.Drawing.PointF(88f, 128f), Origin: new System.Drawing.PointF(88f, 128f), Radius: new System.Drawing.SizeF(80f, 12f))));
    Probe.Require(hud.Marks.Count == 5, $"hud.marks={hud.Marks.Count}");
    facts.Add("hud.marks", hud.Marks.Count);

    RhinoCommandContext context = Probe.Expect(RhinoCommandContext.Of(doc: scope.Active, mode: RunMode.Scripted), "context", facts);
    RhinoUi ui = context.Ui;

    UiViewportPreview hudPreview = UiViewportPreview.Hud(hud: _ => Fin.Succ(value: hud));
    _ = Probe.Expect(
        ui.Use(UiViewportRequest.Preview<Unit>(
            preview: hudPreview,
            run: previewScope => {
                RhinoView view = scope.Active.Views.ActiveView ?? throw new InvalidOperationException(message: "no active view");
                using System.Drawing.Bitmap bitmap = view.CaptureToBitmap() ?? throw new InvalidOperationException(message: "capture returned null");
                facts.Add("hud.capture.w", bitmap.Width);
                facts.Add("hud.capture.h", bitmap.Height);
                _ = string.IsNullOrEmpty(value: CAPTURE_PATH) ? unit : Op.Side(() => bitmap.Save(filename: CAPTURE_PATH));
                return Fin.Succ(value: unit);
            },
            interactive: false)),
        "hud overlay render (text+stroke+box+curve+radial)",
        facts);
    facts.Add("hud.render.ok", true);

    // Dotted preview: line-likes route through DisplayPipeline.DrawDottedPolyline (screen-anchored pattern).
    LineCurve dottedCurve = new LineCurve(new Point3d(0.0, 0.0, 0.0), new Point3d(12.0, 8.0, 0.0));
    UiViewportPreview dotted = UiViewportPreview.Of(
        geometry: new Curve[] { dottedCurve },
        style: new UiPreviewStyle(Stroke: Some(UiStroke.Of(System.Drawing.Color.Lime, 2f)), Dotted: true));
    _ = Probe.Expect(
        ui.Use(UiViewportRequest.Preview<Unit>(
            preview: dotted,
            run: _ => { scope.Active.Views.Redraw(); return Fin.Succ(value: unit); },
            interactive: false)),
        "dotted curve preview render",
        facts);
    facts.Add("dotted.render.ok", true);

    // UiCanvas owns a sprite atlas + state cell; paint is not driven headless, so verify the state rail only.
    UiCanvas<int> canvas = new UiCanvas<int>(initial: 0, paint: (_, _) => Fin.Succ(value: hud));
    Probe.Require(canvas.State == 0, $"canvas.state0={canvas.State}");
    _ = Probe.Expect(canvas.Transition(transition: count => count + 1), "canvas transition", facts);
    Probe.Require(canvas.State == 1, $"canvas.state1={canvas.State}");
    facts.Add("canvas.state", canvas.State);

    RasmSection sectionA = Probe.Expect(RasmSection.Of(caption: "Alpha", sectionHeight: 120, content: new Panel(), expanded: true), "section A", facts);
    RasmSection sectionB = Probe.Expect(RasmSection.Of(caption: "Beta", sectionHeight: 96, content: new Panel(), expanded: false), "section B", facts);
    Rhino.UI.Controls.EtoCollapsibleSectionHolder holder = Probe.Expect(PanelOp.Sections(sections: Seq(sectionA, sectionB), scrollbars: true), "sections holder", facts);
    int sectionCount = holder.Sections.Count();
    Probe.Require(sectionCount == 2, $"sections.count={sectionCount}");
    facts.Add("sections.count", sectionCount);

    BoundingBox region = new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(10.0, 6.0, 4.0));
    UiGumballSpec spec0 = UiGumballSpec.Of(source: region);
    UiGumballSpec spec1 = UiGumballSpec.Of(source: region, frame: Some(new Plane(origin: new Point3d(2.0, 2.0, 0.0), normal: Vector3d.ZAxis)));
    _ = Probe.Expect(
        ui.Use(UiIntent.Gumball<Unit>(spec: spec0, run: gumball => gumball.Reconfigure(spec: spec1), interactive: false)),
        "gumball reconfigure",
        facts);
    facts.Add("gumball.reconfigure.ok", true);
});
