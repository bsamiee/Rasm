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

    // UiHud value algebra: monoid identity, +, Add.
    UiHud hud = UiHud.Empty
        .Add(new UiMark.Text(Value: "rasm-hud", At: new System.Drawing.PointF(8f, 8f), Color: System.Drawing.Color.White, Height: 14))
        .Add(new UiMark.Stroke(From: new System.Drawing.PointF(8f, 28f), To: new System.Drawing.PointF(168f, 28f), Color: System.Drawing.Color.Cyan, Thickness: 2f))
        .Add(new UiMark.Box(Bounds: new System.Drawing.Rectangle(8, 36, 160, 48), Outline: System.Drawing.Color.Yellow, Fill: System.Drawing.Color.FromArgb(48, 48, 48), Thickness: 1));
    Probe.Require(hud.Marks.Count == 3, $"hud.marks={hud.Marks.Count}");
    UiHud merged = hud + UiHud.Empty.Add(new UiMark.Text(Value: "extra", At: new System.Drawing.PointF(8f, 92f), Color: System.Drawing.Color.Gray));
    Probe.Require(merged.Marks.Count == 4, $"hud.merged={merged.Marks.Count}");
    facts.Add("hud.marks", hud.Marks.Count);
    facts.Add("hud.merged", merged.Marks.Count);

    // RhinoUi gateway via the public command-context factory.
    RhinoCommandContext context = Probe.Expect(RhinoCommandContext.Of(doc: scope.Active, mode: RunMode.Scripted), "context", facts);
    RhinoUi ui = context.Ui;

    // HUD overlay rendered into a live viewport (display-only -> interactive:false drivable in Scripted).
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
        "hud overlay render",
        facts);
    facts.Add("hud.render.ok", true);

    // Native collapsible sections: EtoCollapsibleSection construction + EtoCollapsibleSectionHolder fold.
    RasmSection sectionA = Probe.Expect(RasmSection.Of(caption: "Alpha", sectionHeight: 120, content: new Panel(), expanded: true), "section A", facts);
    RasmSection sectionB = Probe.Expect(RasmSection.Of(caption: "Beta", sectionHeight: 96, content: new Panel(), expanded: false), "section B", facts);
    Rhino.UI.Controls.EtoCollapsibleSectionHolder holder = Probe.Expect(PanelOp.Sections(sections: Seq(sectionA, sectionB), scrollbars: true), "sections holder", facts);
    int sectionCount = holder.Sections.Count();
    Probe.Require(sectionCount == 2, $"sections.count={sectionCount}");
    facts.Add("sections.count", sectionCount);
    facts.Add("sections.captionA", sectionA.Caption.Local);
    facts.Add("sections.heightA", sectionA.SectionHeight);

    // UiCanvas: Eto Drawable + UseRhinoStyle + Atom-confined state transition.
    UiCanvas<int> canvas = new UiCanvas<int>(initial: 0, paint: (_, _) => Fin.Succ(value: hud));
    Probe.Require(canvas.State == 0, $"canvas.state0={canvas.State}");
    _ = Probe.Expect(canvas.Transition(transition: count => count + 1), "canvas transition", facts);
    Probe.Require(canvas.State == 1, $"canvas.state1={canvas.State}");
    facts.Add("canvas.state", canvas.State);

    // Gumball Reconfigure: live appearance/source re-application via SetBaseGumball (interactive:false).
    BoundingBox region = new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(10.0, 6.0, 4.0));
    UiGumballSpec spec0 = UiGumballSpec.Of(source: region);
    UiGumballSpec spec1 = UiGumballSpec.Of(source: region, frame: Some(new Plane(origin: new Point3d(2.0, 2.0, 0.0), normal: Vector3d.ZAxis)));
    _ = Probe.Expect(
        ui.Use(UiIntent.Gumball<Unit>(spec: spec0, run: gumball => gumball.Reconfigure(spec: spec1), interactive: false)),
        "gumball reconfigure",
        facts);
    facts.Add("gumball.reconfigure.ok", true);
});
