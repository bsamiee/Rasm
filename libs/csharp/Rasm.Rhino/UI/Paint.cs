using DrawingColor = System.Drawing.Color;
using EtoColor = Eto.Drawing.Color;

namespace Rasm.Rhino.UI;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record UiMark {    // screen-space (px), backend-neutral
    private UiMark() { }
    public sealed record Text(string Value, System.Drawing.PointF At, DrawingColor Color, int Height = 12, bool Middle = false, Option<string> Font = default) : UiMark;
    public sealed record Stroke(System.Drawing.PointF From, System.Drawing.PointF To, DrawingColor Color, float Thickness = 1f) : UiMark;
    public sealed record Box(System.Drawing.Rectangle Bounds, DrawingColor Outline, DrawingColor Fill, int Thickness = 1) : UiMark;

    internal Fin<Unit> Draw(DisplayPipeline pipeline) =>
        RhinoUi.Protect(valid: () => Fin.Succ(value: Op.Side(() => Switch(
            text: t => pipeline.Draw2dText(text: t.Value, color: t.Color, screenCoordinate: new Point2d(t.At.X, t.At.Y), middleJustified: t.Middle, height: t.Height, fontface: t.Font.IfNone(string.Empty)),
            stroke: s => pipeline.Draw2dLine(from: s.From, to: s.To, color: s.Color, thickness: s.Thickness),
            box: b => pipeline.Draw2dRectangle(rectangle: b.Bounds, strokeColor: b.Outline, thickness: b.Thickness, fillColor: b.Fill)))));

    internal Fin<Unit> Paint(Eto.Drawing.Graphics graphics) =>
        RhinoUi.Protect(valid: () => Fin.Succ(value: Op.Side(() => Switch(
            text: t => graphics.DrawText(new Eto.Drawing.Font(t.Font.IfNone("Arial"), t.Height), Convert(t.Color), t.At.X, t.At.Y, t.Value),
            stroke: s => graphics.DrawLine(Convert(s.Color), new Eto.Drawing.PointF(s.From.X, s.From.Y), new Eto.Drawing.PointF(s.To.X, s.To.Y)),
            box: b => { graphics.FillRectangle(Convert(b.Fill), b.Bounds.X, b.Bounds.Y, b.Bounds.Width, b.Bounds.Height); graphics.DrawRectangle(Convert(b.Outline), b.Bounds.X, b.Bounds.Y, b.Bounds.Width, b.Bounds.Height); }))));

    private static EtoColor Convert(DrawingColor color) => EtoColor.FromArgb(color.R, color.G, color.B, color.A);
}

public readonly record struct UiHud(Seq<UiMark> Marks) {
    public static UiHud Empty => new(Seq<UiMark>());
    public static UiHud operator +(UiHud left, UiHud right) => new(left.Marks + right.Marks);
    public UiHud Add(UiMark mark) => new(Marks.Add(mark));
    internal Fin<Unit> Draw(DisplayPipeline pipeline) => Marks.TraverseM(mark => mark.Draw(pipeline: pipeline)).As().Map(static _ => unit);
    internal Fin<Unit> Paint(Eto.Drawing.Graphics graphics) => Marks.TraverseM(mark => mark.Paint(graphics: graphics)).As().Map(static _ => unit);
}

public readonly record struct UiCanvasContext<TState>(MousePhase Phase, TState State, Eto.Forms.MouseEventArgs Args) {
    public Eto.Drawing.PointF Location => Args.Location;
    public MouseDecision<TState> Pass => new(State: State, Cancel: false);
    public MouseDecision<TState> Next(TState state, bool cancel = false) => new(State: state, Cancel: cancel);
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class UiCanvas<TState> : Eto.Forms.Drawable {
    private readonly Atom<TState> state;
    private readonly Func<TState, Eto.Drawing.Size, Fin<UiHud>> paint;
    private readonly Func<UiCanvasContext<TState>, Fin<MouseDecision<TState>>>? mouse;

    public UiCanvas(TState initial, Func<TState, Eto.Drawing.Size, Fin<UiHud>> paint, Func<UiCanvasContext<TState>, Fin<MouseDecision<TState>>>? mouse = null) {
        state = Atom(initial);
        this.paint = paint;
        this.mouse = mouse;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(this);
    }

    public TState State => state.Value;

    public Fin<Unit> Transition(Func<TState, TState> transition) =>
        Op.Of(name: nameof(Transition)).Need(transition).Map(apply => { _ = state.Swap(apply); Invalidate(); return unit; });

    protected override void OnPaint(Eto.Forms.PaintEventArgs e) =>
        _ = RhinoUi.Protect(valid: () => paint(arg1: state.Value, arg2: Size).Bind(hud => hud.Paint(graphics: e.Graphics)));

    protected override void OnMouseDown(Eto.Forms.MouseEventArgs e) => _ = Dispatch(phase: MousePhase.Down, args: e);
    protected override void OnMouseMove(Eto.Forms.MouseEventArgs e) => _ = Dispatch(phase: MousePhase.Move, args: e);
    protected override void OnMouseUp(Eto.Forms.MouseEventArgs e) => _ = Dispatch(phase: MousePhase.Up, args: e);

    private Unit Dispatch(MousePhase phase, Eto.Forms.MouseEventArgs args) =>
        Optional(mouse).Iter(change => _ = RhinoUi.Protect(valid: () => change(arg: new UiCanvasContext<TState>(Phase: phase, State: state.Value, Args: args))
            .Map(decision => { _ = state.Swap(_ => decision.State); Invalidate(); return unit; })));
}
