using Rasm.Rhino.Document;
using Rhino.UI;
using Rhino.UI.Gumball;

namespace Rasm.Rhino.Display;

// --- [TYPES] ---------------------------------------------------------------------------
public enum MousePhase { Move = 0, Down = 1, Up = 2, Click = 3, DoubleClick = 4, Enter = 5, Hover = 6, Leave = 7 }

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MouseStage {
    public sealed record Begin() : MouseStage;

    public sealed record End(bool Canceled) : MouseStage;

    public sealed record Single() : MouseStage;
}

public sealed record MouseEvent(MousePhase Phase, MouseStage Stage, Option<Guid> ViewportId, System.Drawing.Point ViewportPoint, MouseButton Button, bool Shift, bool Ctrl, GumballMode Gumball);

public sealed record MouseCallbacks(Option<Func<MouseEvent, IO<bool>>> Cancel, Func<MouseEvent, IO<Unit>> Sink, Action<Error> Reject);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class MouseHook : MouseCallback, IDisposable {
    private readonly MouseCallbacks callbacks;

    private readonly Disposal disposal;

    private MouseHook(MouseCallbacks callbacks) {
        this.callbacks = callbacks;
        disposal = new(() => Enabled = false);
    }

    public static IO<MouseHook> Enable(MouseCallbacks callbacks) =>
        IO.lift(() => new MouseHook(callbacks) { Enabled = true });

    public void Dispose() => disposal.Dispose();

    protected override void OnMouseMove(MouseCallbackEventArgs e) {
        base.OnMouseMove(e);
        Deliver(MousePhase.Move, new MouseStage.Begin(), e);
    }

    protected override void OnEndMouseMove(MouseCallbackEventArgs e) {
        base.OnEndMouseMove(e);
        Deliver(MousePhase.Move, new MouseStage.End(e.Cancel), e);
    }

    protected override void OnMouseDown(MouseCallbackEventArgs e) {
        base.OnMouseDown(e);
        Deliver(MousePhase.Down, new MouseStage.Begin(), e);
    }

    protected override void OnEndMouseDown(MouseCallbackEventArgs e) {
        base.OnEndMouseDown(e);
        Deliver(MousePhase.Down, new MouseStage.End(e.Cancel), e);
    }

    protected override void OnMouseUp(MouseCallbackEventArgs e) {
        base.OnMouseUp(e);
        Deliver(MousePhase.Up, new MouseStage.Begin(), e);
    }

    protected override void OnEndMouseUp(MouseCallbackEventArgs e) {
        base.OnEndMouseUp(e);
        Deliver(MousePhase.Up, new MouseStage.End(e.Cancel), e);
    }

    protected override void OnMouseDoubleClick(MouseCallbackEventArgs e) {
        base.OnMouseDoubleClick(e);
        Deliver(MousePhase.DoubleClick, new MouseStage.Single(), e);
    }

    protected override void OnMouseEnter(MouseCallbackEventArgs e) {
        base.OnMouseEnter(e);
        Deliver(MousePhase.Enter, new MouseStage.Single(), e);
    }

    protected override void OnMouseHover(MouseCallbackEventArgs e) {
        base.OnMouseHover(e);
        Deliver(MousePhase.Hover, new MouseStage.Single(), e);
    }

    protected override void OnMouseLeave(MouseCallbackEventArgs e) {
        base.OnMouseLeave(e);
        Deliver(MousePhase.Leave, new MouseStage.Single(), e);
    }

    private void Deliver(MousePhase phase, MouseStage stage, MouseCallbackEventArgs e) {
        MouseEvent evt = new(phase, stage, Optional(e.View).Map(static view => view.ActiveViewport.Id), e.ViewportPoint, e.MouseButton, e.ShiftKeyDown, e.CtrlKeyDown, e.IsOverGumball());
        e.Cancel = e.Cancel || (stage.Map(begin: true, end: false, single: false) && Answers.Answer(callbacks.Cancel.Map(cancel => cancel(evt)), callbacks.Reject, static () => false));
        _ = Answers.Answer(callbacks.Sink(evt), callbacks.Reject, unit);
    }
}
