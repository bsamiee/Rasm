# [RASM_GRASSHOPPER_ATTRIBUTES]

`ComponentChrome` is the component attribute interaction policy: every host callback — layout, pivot, foreground paint, context menu, pointer, wheel, keyboard, text input, focus, tooltip, resize, cursor — lands as one case of the `ChromeEvent` union, one `Respond` fold answers it with a right-biased `ChromeDecision` merge, and every answer seals into one bounded `ChromeReceipt` stream. The canvas snap setting mutates only inside a `SnapWindow` restoration capsule, resize rides a `ResizeSession` over the verified `ResizingFrame`, and the two host attribute bases the platform forces both route through one `ChromeDispatch` spine, so the policy is declared once and projected twice.

## [01]-[INDEX]

- [02]-[EVENT_ALGEBRA]: the `ChromeEvent` union, its pointer and key vocabularies, and the `ChromeDecision` merge monoid
- [03]-[SNAP_AND_RESIZE]: the `SnapWindow` restoration capsule and the `ResizeSession` over the host frame
- [04]-[CHROME_POLICY]: the `ComponentChrome` declaration, the bounded receipt stream, and its projections
- [05]-[HOST_PROJECTION]: the dual attribute bases and the one dispatch spine
- [06]-[RESEARCH]

## [02]-[EVENT_ALGEBRA]

- Owner: `ChromeEvent` is the closed interaction vocabulary — one case per verified host callback family, each carrying the host payload verbatim; `ChromeDecision` is the right-biased merge monoid, so composing two policies is `left | right` with the right's settled slots winning and redraw accumulating.
- Cases: `Layout`, `Pivot`, `Paint`, `Menu`, `Pointer`, `Key`, `Text`, `Focus`, `Tooltip`, `Resize`, and `Cursor`; `PointerKind`, `KeyPhase`, and `ResizeStage` close the sub-discriminants, with `Leave` carrying no mouse payload so `Pointer` types its args as presence.
- Entry: a policy is one `Func<ChromeEvent, ChromeState, ChromeDecision>` — total by the union's generated dispatch, never a callback subclass per interaction.
- Growth: a new host callback family is one union case; a new decision slot is one `ChromeDecision` member folded into `|`.
- Boundary: payloads stay host values — `ResponseMouseArgs`, `KeyEventArgs`, `TextInputEventArgs`, `Context`, `Skin`, `Capsule`, `Shade`, `Shape`, `ContextMenu` cross unwrapped because the decision, not the payload, is this page's domain; the input panel projects through `ComponentSpec.Panel`, never a chrome case.

```csharp signature
namespace Rasm.Grasshopper.Components;

// --- [TYPES] -----------------------------------------------------------------------------

[SmartEnum]
public sealed partial class PointerKind {
    public static readonly PointerKind Over = new();
    public static readonly PointerKind Leave = new();
    public static readonly PointerKind Down = new();
    public static readonly PointerKind Drag = new();
    public static readonly PointerKind Up = new();
    public static readonly PointerKind Wheel = new();
    public static readonly PointerKind SingleClick = new();
    public static readonly PointerKind DoubleClick = new();
}

[SmartEnum]
public sealed partial class KeyPhase {
    public static readonly KeyPhase Down = new();
    public static readonly KeyPhase Up = new();
}

[SmartEnum]
public sealed partial class ResizeStage {
    public static readonly ResizeStage Begin = new();
    public static readonly ResizeStage Track = new();
    public static readonly ResizeStage Commit = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChromeEvent {
    private ChromeEvent() { }

    public sealed record Layout(Grasshopper2.UI.Skinning.Shape Shape) : ChromeEvent;
    public sealed record Pivot(Eto.Drawing.PointF Old, Eto.Drawing.PointF New) : ChromeEvent;
    public sealed record Paint(
        Eto.Drawing.Context Surface,
        Grasshopper2.UI.Skinning.Skin Skin,
        Grasshopper2.UI.Primitives.Capsule Capsule,
        Grasshopper2.UI.Skinning.Shade Shade) : ChromeEvent;
    public sealed record Menu(Eto.Forms.ContextMenu Host) : ChromeEvent;
    public sealed record Pointer(PointerKind Kind, Option<Grasshopper2.UI.Flex.ResponseMouseArgs> Args) : ChromeEvent;
    public sealed record Key(KeyPhase Phase, Eto.Forms.KeyEventArgs Args) : ChromeEvent;
    public sealed record Text(Eto.Forms.TextInputEventArgs Args) : ChromeEvent;
    public sealed record Focus(bool Gained) : ChromeEvent;
    public sealed record Tooltip(Eto.Drawing.PointF At) : ChromeEvent;
    public sealed record Resize(ResizeStage Stage, Eto.Drawing.PointF At, Eto.Drawing.RectangleF Bounds) : ChromeEvent;
    public sealed record Cursor(Eto.Drawing.PointF At) : ChromeEvent;
}

// --- [MODELS] ----------------------------------------------------------------------------

public readonly record struct ChromeState(Eto.Drawing.RectangleF Bounds, Eto.Drawing.PointF Pivot);

public readonly record struct ChromeDecision(
    Option<Grasshopper2.UI.Flex.Response> Verdict,
    bool Redraw,
    Option<Eto.Drawing.RectangleF> Bounds,
    Option<Eto.Forms.Cursor> Pointer,
    Option<string> Tip,
    Option<Grasshopper2.Undo.Actions.ResizeAction> Undo) {
    public static readonly ChromeDecision Pass = new(default, false, default, default, default, default);

    public static ChromeDecision operator |(ChromeDecision left, ChromeDecision right) => new(
        right.Verdict | left.Verdict,
        left.Redraw || right.Redraw,
        right.Bounds | left.Bounds,
        right.Pointer | left.Pointer,
        right.Tip | left.Tip,
        right.Undo | left.Undo);
}
```

## [03]-[SNAP_AND_RESIZE]

- Owner: `SnapWindow` is the boundary-scoped restoration capsule over the global canvas snap setting — open captures the prior value, dispose restores it, so a snap mutation without its restoration receipt is unconstructible; `ResizeSession` owns one interactive resize over the verified `ResizingFrame` under caller-held `SnappingConstraints` and `SnappingSettings`.
- Entry: `ResizeSession.Begin` admits the frame, the grip padding, the caller-held snapping surfaces, and the snap window in one acquisition, refusing when the host `Begin` reports no edge under the grip; `Track` advances and returns the frame's `Resized` rectangle, `CursorAt` projects the edge cursor, and disposal restores the snap state on every exit path.
- Receipt: the resize commit's undo rides `ChromeDecision.Undo` as a host `ResizeAction`, so mutation and undo stay one act.
- Packages: `Grasshopper2.UI.ResizingFrame` — constructor `(RectangleF, SizeF, SizeF, SnappingConstraints, SnappingSettings)`, `bool Begin(PointF, Padding)`, `void Continue(PointF)`, `Resized`, `CursorAt(PointF, Padding)` — and the `Grasshopper2.UI.Canvas` snapping surfaces are the composed mechanics; no local snap solver exists beside them.
- Growth: a new resize constraint is one `ResizePolicy` member; a new snapped setting is a second capsule of the same shape.
- Boundary: `SnappingConstraints` and `SnappingSettings` arrive pre-built from the caller — this page never reaches a `Document` or a settings store to build them, keeping the chrome session-independent; the capsule bodies are the named boundary-kernel statement seam.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record ResizePolicy(Eto.Drawing.SizeF Minimum, Eto.Drawing.SizeF Maximum, Eto.Drawing.Padding Grip, bool SnapToObjects);

public readonly struct SnapWindow : IDisposable {
    private readonly bool prior;

    private SnapWindow(bool prior) => this.prior = prior;

    public static SnapWindow Open(bool desired) {
        bool held = Grasshopper2.Settings.CanvasSnapToObjects.Value;
        Grasshopper2.Settings.CanvasSnapToObjects.Value = desired;
        return new SnapWindow(held);
    }

    public void Dispose() => Grasshopper2.Settings.CanvasSnapToObjects.Value = prior;
}

// --- [SERVICES] --------------------------------------------------------------------------

public sealed class ResizeSession : IDisposable {
    private readonly Grasshopper2.UI.ResizingFrame frame;

    private readonly SnapWindow snap;

    private readonly Op key;

    private ResizeSession(Grasshopper2.UI.ResizingFrame frame, SnapWindow snap, Op key) => (this.frame, this.snap, this.key) = (frame, snap, key);

    public static Fin<ResizeSession> Begin(
        Eto.Drawing.RectangleF bounds,
        ResizePolicy policy,
        Grasshopper2.UI.Canvas.SnappingConstraints constraints,
        Grasshopper2.UI.Canvas.SnappingSettings settings,
        Eto.Drawing.PointF at,
        Op? key = null) {
        Op op = key.OrDefault();
        return Hosted.Bound(() => {
                Grasshopper2.UI.ResizingFrame frame = new(bounds, policy.Minimum, policy.Maximum, constraints, settings);
                return frame.Begin(at, policy.Grip)
                    ? Fin.Succ(new ResizeSession(frame, SnapWindow.Open(policy.SnapToObjects), op))
                    : Fin.Fail<ResizeSession>(new GhFault.Refused(op, $"{nameof(Begin)}:{at}"));
            }, op)
            .Bind(identity);
    }

    public Fin<Eto.Drawing.RectangleF> Track(Eto.Drawing.PointF at) =>
        Hosted.Bound(() => { frame.Continue(at); return frame.Resized; }, key);

    public Option<Eto.Forms.Cursor> CursorAt(Eto.Drawing.PointF at, Eto.Drawing.Padding grip) => Optional(frame.CursorAt(at, grip));

    public void Dispose() => snap.Dispose();
}
```

## [04]-[CHROME_POLICY]

- Owner: `ComponentChrome` is the declaration a `ComponentSpec` carries — one respond fold plus the optional resize policy; `ChromeReceipt` is the per-interaction evidence row, and its projections are pure folds over one stream, never parallel cells.
- Entry: `ChromeDispatch.Decide` is the one spine both host bases call — respond, record, return.
- Receipt: `ChromeReceipt` carries the event kind, the merged decision, and the instant; the stream is a bounded ring — the oldest row falls off past `Window`, so a long-lived attribute instance never grows unbounded under per-paint layout traffic; `LatestByKind` and `RedrawCount` are the standing projections.
- Growth: a new projection is one fold over the same stream; a new policy slot is one `ComponentChrome` member.
- Boundary: the receipt cell lives on the host attribute instance — chrome policy itself stays an immutable value shared across components.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record ComponentChrome {
    public static readonly ComponentChrome None = new();

    public Func<ChromeEvent, ChromeState, ChromeDecision> Respond { get; init; } = static (_, _) => ChromeDecision.Pass;

    public Option<ResizePolicy> Resize { get; init; } = default;
}

public readonly record struct ChromeReceipt(string Kind, ChromeDecision Decision, DateTimeOffset At);

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class ChromeDispatch {
    public const int Window = 256;

    public static ChromeDecision Decide(ComponentChrome chrome, Atom<Seq<ChromeReceipt>> receipts, ChromeEvent happening, ChromeState state) =>
        Recorded(receipts, chrome.Respond(happening, state), happening);

    public static HashMap<string, ChromeReceipt> LatestByKind(Seq<ChromeReceipt> receipts) =>
        receipts.Fold(HashMap<string, ChromeReceipt>(), static (held, receipt) => held.AddOrUpdate(receipt.Kind, receipt));

    public static int RedrawCount(Seq<ChromeReceipt> receipts) =>
        receipts.Filter(static receipt => receipt.Decision.Redraw).Count;

    private static ChromeDecision Recorded(Atom<Seq<ChromeReceipt>> receipts, ChromeDecision decision, ChromeEvent happening) =>
        (receipts.Swap(held => (held.Count >= Window ? held.Tail : held).Add(new ChromeReceipt(happening.GetType().Name, decision, DateTimeOffset.UtcNow))),
            decision).Item2;
}
```

## [05]-[HOST_PROJECTION]

- Owner: `ChromeHost` and `ResizableChromeHost` are the two host projections the platform forces — the resizable base is a distinct host generic — and both are override-thin shells over `ChromeDispatch.Decide`; `ChromeHost.Mount` selects the base from the declared resize policy, so the caller never names either concrete.
- Entry: every host callback body is one dispatch expression over the verified base member — `LayoutBounds(Shape)`, `PivotMoved(PointF, PointF)`, `DrawForegroundDecorations(Context, Skin, Capsule, Shade)`, `ShowTooltipAt(PointF)` — with the base behavior preserved before the policy observes; pointer, wheel, key, text, and focus wire once through `ChromeWiring` onto the verified `Responses` hook events.
- Auto: an unanswered decision falls back to the host default — `Response.Ignored`, the base tooltip verdict, the default cursor — so `ComponentChrome.None` is behaviorally inert; mounting chrome replaces the internal modular attribute layout, so a chrome policy owns its own layout decisions.
- Growth: a new host callback is one override on each shell dispatching an existing or new event case.
- Boundary: the constructor wiring and the override statements are the named platform-forced seam; no other page names an attribute base member; `Bounds` and `Pivot` read through the verified `IAttributes` contract.

```csharp signature
// --- [COMPOSITION] -----------------------------------------------------------------------

internal static class ChromeWiring {
    public static void Wire(
        Grasshopper2.UI.Flex.Responses responder,
        Func<PointerKind, Option<Grasshopper2.UI.Flex.ResponseMouseArgs>, Grasshopper2.UI.Flex.Response> pointer,
        Func<KeyPhase, Eto.Forms.KeyEventArgs, Grasshopper2.UI.Flex.Response> key,
        Func<Eto.Forms.TextInputEventArgs, Grasshopper2.UI.Flex.Response> text,
        Action<bool> focus) {
        responder.MouseOverHook += args => ignore(pointer(PointerKind.Over, Optional(args)));
        responder.MouseLeaveHook += () => ignore(pointer(PointerKind.Leave, None));
        responder.MouseDownHook += args => pointer(PointerKind.Down, Optional(args));
        responder.MouseDragHook += args => pointer(PointerKind.Drag, Optional(args));
        responder.MouseUpHook += args => pointer(PointerKind.Up, Optional(args));
        responder.MouseWheelHook += args => pointer(PointerKind.Wheel, Optional(args));
        responder.MouseSingleClickHook += args => pointer(PointerKind.SingleClick, Optional(args));
        responder.MouseDoubleClickHook += args => pointer(PointerKind.DoubleClick, Optional(args));
        responder.KeyDownHook += args => key(KeyPhase.Down, args);
        responder.KeyUpHook += args => key(KeyPhase.Up, args);
        responder.TextInputHook += args => text(args);
        responder.GotFocus += (_, _) => focus(true);
        responder.LostFocus += (_, _) => focus(false);
    }
}

public sealed class ChromeHost :
    Grasshopper2.Doc.Attributes.ComponentAttributes,
    Grasshopper2.Doc.Attributes.IContextMenuAware,
    Grasshopper2.Doc.Attributes.ICursorAwareAttributes {
    private readonly ComponentChrome chrome;

    private readonly Atom<Seq<ChromeReceipt>> receipts = Atom(Seq<ChromeReceipt>());

    private ChromeHost(Component owner, ComponentChrome chrome) : base(owner) {
        this.chrome = chrome;
        ChromeWiring.Wire(Responder, PointerHook, KeyHook, TextHook, FocusHook);
    }

    public static Grasshopper2.Doc.IAttributes Mount(Component owner, ComponentChrome chrome) =>
        chrome.Resize.Match(
            Some: policy => (Grasshopper2.Doc.IAttributes)new ResizableChromeHost(owner, chrome, policy),
            None: () => new ChromeHost(owner, chrome));

    public Seq<ChromeReceipt> Receipts => receipts.Value;

    private ChromeState State => new(Bounds, Pivot);

    protected override void LayoutBounds(Grasshopper2.UI.Skinning.Shape shape) {
        base.LayoutBounds(shape);
        ignore(Decide(new ChromeEvent.Layout(shape)));
    }

    protected override void DrawForegroundDecorations(
        Eto.Drawing.Context context, Grasshopper2.UI.Skinning.Skin skin,
        Grasshopper2.UI.Primitives.Capsule capsule, Grasshopper2.UI.Skinning.Shade shade) {
        base.DrawForegroundDecorations(context, skin, capsule, shade);
        ignore(Decide(new ChromeEvent.Paint(context, skin, capsule, shade)));
    }

    public override bool ShowTooltipAt(Eto.Drawing.PointF point) =>
        Decide(new ChromeEvent.Tooltip(point)).Tip.IsSome || base.ShowTooltipAt(point);

    protected override void PivotMoved(Eto.Drawing.PointF oldPivot, Eto.Drawing.PointF newPivot) {
        base.PivotMoved(oldPivot, newPivot);
        ignore(Decide(new ChromeEvent.Pivot(oldPivot, newPivot)));
    }

    public void AppendToMenu(Eto.Forms.ContextMenu menu) => ignore(Decide(new ChromeEvent.Menu(menu)));

    public Eto.Forms.Cursor CursorAt(Eto.Drawing.PointF at) =>
        Decide(new ChromeEvent.Cursor(at)).Pointer.IfNone(Eto.Forms.Cursors.Default);

    private ChromeDecision Decide(ChromeEvent happening) => ChromeDispatch.Decide(chrome, receipts, happening, State);

    private Grasshopper2.UI.Flex.Response PointerHook(PointerKind kind, Option<Grasshopper2.UI.Flex.ResponseMouseArgs> args) =>
        Decide(new ChromeEvent.Pointer(kind, args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private Grasshopper2.UI.Flex.Response KeyHook(KeyPhase phase, Eto.Forms.KeyEventArgs args) =>
        Decide(new ChromeEvent.Key(phase, args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private Grasshopper2.UI.Flex.Response TextHook(Eto.Forms.TextInputEventArgs args) =>
        Decide(new ChromeEvent.Text(args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private void FocusHook(bool gained) => ignore(Decide(new ChromeEvent.Focus(gained)));
}

public sealed class ResizableChromeHost :
    Grasshopper2.Doc.Attributes.ResizableAttributes<Component>,
    Grasshopper2.Doc.Attributes.IContextMenuAware,
    Grasshopper2.Doc.Attributes.ICursorAwareAttributes {
    private readonly ComponentChrome chrome;

    private readonly ResizePolicy policy;

    private readonly Atom<Seq<ChromeReceipt>> receipts = Atom(Seq<ChromeReceipt>());

    internal ResizableChromeHost(Component owner, ComponentChrome chrome, ResizePolicy policy) : base(owner) {
        (this.chrome, this.policy) = (chrome, policy);
        ChromeWiring.Wire(Responder, PointerHook, KeyHook, TextHook, FocusHook);
    }

    public Seq<ChromeReceipt> Receipts => receipts.Value;

    protected override Eto.Drawing.Padding EdgeSize => policy.Grip;

    public Eto.Drawing.SizeF Size {
        get => Bounds.Size;
        set => ignore(Decide(new ChromeEvent.Resize(ResizeStage.Track, Pivot, new Eto.Drawing.RectangleF(Bounds.Location, value))));
    }

    protected override void LayoutBounds(Grasshopper2.UI.Skinning.Shape shape) {
        base.LayoutBounds(shape);
        ignore(Decide(new ChromeEvent.Layout(shape)));
    }

    protected override void DrawForegroundDecorations(
        Eto.Drawing.Context context, Grasshopper2.UI.Skinning.Skin skin,
        Grasshopper2.UI.Primitives.Capsule capsule, Grasshopper2.UI.Skinning.Shade shade) {
        base.DrawForegroundDecorations(context, skin, capsule, shade);
        ignore(Decide(new ChromeEvent.Paint(context, skin, capsule, shade)));
    }

    public override bool ShowTooltipAt(Eto.Drawing.PointF point) =>
        Decide(new ChromeEvent.Tooltip(point)).Tip.IsSome || base.ShowTooltipAt(point);

    protected override void PivotMoved(Eto.Drawing.PointF oldPivot, Eto.Drawing.PointF newPivot) {
        base.PivotMoved(oldPivot, newPivot);
        ignore(Decide(new ChromeEvent.Pivot(oldPivot, newPivot)));
    }

    public void AppendToMenu(Eto.Forms.ContextMenu menu) => ignore(Decide(new ChromeEvent.Menu(menu)));

    public Eto.Forms.Cursor CursorAt(Eto.Drawing.PointF at) =>
        Decide(new ChromeEvent.Cursor(at)).Pointer.IfNone(Eto.Forms.Cursors.Default);

    private ChromeState State => new(Bounds, Pivot);

    private ChromeDecision Decide(ChromeEvent happening) => ChromeDispatch.Decide(chrome, receipts, happening, State);

    private Grasshopper2.UI.Flex.Response PointerHook(PointerKind kind, Option<Grasshopper2.UI.Flex.ResponseMouseArgs> args) =>
        Decide(new ChromeEvent.Pointer(kind, args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private Grasshopper2.UI.Flex.Response KeyHook(KeyPhase phase, Eto.Forms.KeyEventArgs args) =>
        Decide(new ChromeEvent.Key(phase, args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private Grasshopper2.UI.Flex.Response TextHook(Eto.Forms.TextInputEventArgs args) =>
        Decide(new ChromeEvent.Text(args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private void FocusHook(bool gained) => ignore(Decide(new ChromeEvent.Focus(gained)));
}
```

## [06]-[RESEARCH]

- [RESIZABLE_BASE]-[OPEN]: the resizable attribute base — the `ResizableAttributes<T>` type name, arity, constructor, `EdgeSize`, and the `Size` accessor pair the host drives during a resize interaction, plus the seam where a decided `Bounds` applies back to the attribute; verify through the decompile rail and re-shape `ResizableChromeHost` to the host's own resize protocol.
- [AWARE_INTERFACES]-[OPEN]: the `IContextMenuAware` and `ICursorAwareAttributes` member shapes — the shells assume `AppendToMenu(ContextMenu)` and `CursorAt(PointF)`; verify the interface members and homes through the decompile rail.
- [RESIZE_ACTION]-[OPEN]: the `Grasshopper2.Undo.Actions.ResizeAction` constructor and the `ActionList` seam a commit-stage undo row lands on, so a resize commit mints and drains its undo instead of receiving one pre-built.
- [SNAPPING_CONSTRAINTS]-[OPEN]: the full `SnappingConstraints.CreateFromDocument` parameter list — the session takes constraints and settings pre-built until the arity is verified.
- [SNAP_SETTING]-[OPEN]: the settings cell behind the canvas snap flag — `SnapWindow` assumes `Grasshopper2.Settings.CanvasSnapToObjects.Value`; verify the cell spelling and value shape through the decompile rail.
