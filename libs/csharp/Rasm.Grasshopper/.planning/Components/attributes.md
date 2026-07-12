# [RASM_GRASSHOPPER_ATTRIBUTES]

`ComponentChrome` is the component-attribute interaction policy. Verified host callbacks enter one `ChromeEvent` union, one response fold returns only decisions the shells can project, and every answer receives a bounded monotone trace ordinal. `ResizableAttributes<T>` remains the sole resize, snapping, cursor, persistence, and undo owner; the two host bases project one policy spine without duplicating that protocol.

## [01]-[INDEX]

- [02]-[EVENT_ALGEBRA]: the `ChromeEvent` union, its pointer and key vocabularies, and the `ChromeDecision` merge monoid
- [03]-[CHROME_POLICY]: the `ComponentChrome` declaration and bounded monotone receipt stream
- [04]-[HOST_PROJECTION]: the dual thin host shells and one dispatch spine

## [02]-[EVENT_ALGEBRA]

- Owner: `ChromeEvent` is the closed interaction vocabulary — one case per verified host callback family, each carrying the host payload verbatim; `ChromeDecision` is the right-biased merge monoid, so composing two policies is `left | right` with the right's settled slots winning and redraw accumulating.
- Cases: `Layout`, `Pivot`, `Paint`, `Menu`, `Pointer`, `Key`, `Text`, `Focus`, `Tooltip`, `Resize`, and `Cursor`; `PointerKind` and `KeyPhase` close the sub-discriminants, while `Resize` observes the live host `Size` after invalidation and `Leave` carries no mouse payload.
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
    public sealed record Resize(Eto.Drawing.SizeF Size) : ChromeEvent;
    public sealed record Cursor(Eto.Drawing.PointF At) : ChromeEvent;
}

// --- [MODELS] ----------------------------------------------------------------------------

public readonly record struct ChromeState(Eto.Drawing.RectangleF Bounds, Eto.Drawing.PointF Pivot);

public readonly record struct ChromeDecision(
    Option<Grasshopper2.UI.Flex.Response> Verdict,
    Option<Eto.Forms.Cursor> Pointer,
    Option<string> Tip) {
    public static readonly ChromeDecision Pass = new(default, default, default);

    public static ChromeDecision operator |(ChromeDecision left, ChromeDecision right) => new(
        right.Verdict | left.Verdict,
        right.Pointer | left.Pointer,
        right.Tip | left.Tip);
}
```

## [03]-[CHROME_POLICY]

- Owner: `ComponentChrome` carries one response fold plus optional size limits. `ChromeTrace` stores a strictly increasing per-instance ordinal, event kind, and projected decision.
- Entry: `ChromeDispatch.Decide` is the one spine both host bases call — respond, record, return.
- Receipt: the bounded stream drops its oldest row past `Window`; `LatestByKind` derives from that one stream.
- Growth: a new projection is one fold over the same stream; a new policy slot is one `ComponentChrome` member.
- Boundary: the receipt cell lives on the host attribute instance — chrome policy itself stays an immutable value shared across components.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record ResizePolicy(Eto.Drawing.SizeF Minimum, Eto.Drawing.SizeF Maximum);

public sealed record ComponentChrome {
    public static readonly ComponentChrome None = new();

    public Func<ChromeEvent, ChromeState, ChromeDecision> Respond { get; init; } = static (_, _) => ChromeDecision.Pass;

    public Option<ResizePolicy> Resize { get; init; } = default;
}

public readonly record struct ChromeTrace(long Ordinal, string Kind, ChromeDecision Decision);

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class ChromeDispatch {
    public const int Window = 256;

    public static ChromeDecision Decide(
        ComponentChrome chrome,
        Atom<Seq<ChromeTrace>> receipts,
        Atom<long> ordinal,
        ChromeEvent happening,
        ChromeState state) => Recorded(receipts, ordinal, chrome.Respond(happening, state), happening);

    public static HashMap<string, ChromeTrace> LatestByKind(Seq<ChromeTrace> receipts) =>
        receipts.Fold(HashMap<string, ChromeTrace>(), static (held, receipt) => held.AddOrUpdate(receipt.Kind, receipt));

    private static ChromeDecision Recorded(
        Atom<Seq<ChromeTrace>> receipts, Atom<long> ordinal, ChromeDecision decision, ChromeEvent happening) {
        long next = ordinal.Swap(static current => checked(current + 1L));
        _ = receipts.Swap(held => (held.Count >= Window ? held.Tail : held).Add(new ChromeTrace(
            Ordinal: next,
            Kind: happening.GetType().Name,
            Decision: decision)));
        return decision;
    }
}
```

## [04]-[HOST_PROJECTION]

- Owner: `ChromeHost` and `ResizableChromeHost` are thin projections over the one dispatch spine. The resizable shell delegates the entire interaction protocol to `ResizableAttributes<T>` and observes committed size changes through `InvalidateLayout`.
- Entry: every host callback body is one dispatch expression over the verified base member — `LayoutBounds(Shape)`, `PivotMoved(PointF, PointF)`, `DrawForegroundDecorations(Context, Skin, Capsule, Shade)`, `ShowTooltipAt(PointF)` — with the base behavior preserved before the policy observes; pointer, wheel, key, text, and focus wire once through `ChromeWiring` onto the verified `Responses` hook events.
- Auto: an unanswered decision falls back to `Response.Ignored`, the base tooltip verdict, or the default cursor. `ResizeAction(IDocumentObject)` snapshots pre-resize `IResizableAttributes.Size` and `Pivot`; the host responder constructs it before drag, commits it through document undo on release, and owns snap toggling through `CanvasSnapToObjects`.
- Growth: a new host callback is one override on each shell dispatching an existing or new event case.
- Boundary: `ResizableAttributes<T>(T owner, SizeF minimumSize, SizeF maximumSize)` owns clamping, rounding, `CustomValues` persistence, bounds invalidation, `ResizingFrame`, `SnappingConstraints.CreateFromDocument`, `SnappingSettings.Current`, `CanvasSnapToObjects`, cursor edges, and undo; `EdgeSize` is the host constant `6`, while `Size` is the live public property.

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
    Grasshopper2.UI.IContextMenuAware,
    Grasshopper2.Doc.ICursorAwareAttributes {
    private readonly ComponentChrome chrome;

    private readonly Atom<Seq<ChromeTrace>> receipts = Atom(Seq<ChromeTrace>());

    private readonly Atom<long> ordinal = Atom(0L);

    private ChromeHost(Component owner, ComponentChrome chrome) : base(owner) {
        this.chrome = chrome;
        ChromeWiring.Wire(Responder, PointerHook, KeyHook, TextHook, FocusHook);
    }

    public static Grasshopper2.Doc.IAttributes Mount(Component owner, ComponentChrome chrome) =>
        chrome.Resize.Match(
            Some: policy => (Grasshopper2.Doc.IAttributes)new ResizableChromeHost(owner, chrome, policy),
            None: () => new ChromeHost(owner, chrome));

    public Seq<ChromeTrace> Receipts => receipts.Value;

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

    private ChromeDecision Decide(ChromeEvent happening) => ChromeDispatch.Decide(chrome, receipts, ordinal, happening, State);

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
    Grasshopper2.UI.IContextMenuAware,
    Grasshopper2.Doc.ICursorAwareAttributes {
    private readonly ComponentChrome chrome;

    private readonly Atom<Seq<ChromeTrace>> receipts = Atom(Seq<ChromeTrace>());

    private readonly Atom<long> ordinal = Atom(0L);

    private bool mounted;

    private Eto.Drawing.SizeF observedSize;

    internal ResizableChromeHost(Component owner, ComponentChrome chrome, ResizePolicy policy) :
        base(owner, policy.Minimum, policy.Maximum) {
        this.chrome = chrome;
        observedSize = Size;
        ChromeWiring.Wire(Responder, PointerHook, KeyHook, TextHook, FocusHook);
        mounted = true;
    }

    public Seq<ChromeTrace> Receipts => receipts.Value;

    public override void InvalidateLayout() {
        base.InvalidateLayout();
        if (mounted && observedSize != Size) {
            observedSize = Size;
            ignore(Decide(new ChromeEvent.Resize(Size)));
        }
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

    private ChromeDecision Decide(ChromeEvent happening) => ChromeDispatch.Decide(chrome, receipts, ordinal, happening, State);

    private Grasshopper2.UI.Flex.Response PointerHook(PointerKind kind, Option<Grasshopper2.UI.Flex.ResponseMouseArgs> args) =>
        Decide(new ChromeEvent.Pointer(kind, args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private Grasshopper2.UI.Flex.Response KeyHook(KeyPhase phase, Eto.Forms.KeyEventArgs args) =>
        Decide(new ChromeEvent.Key(phase, args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private Grasshopper2.UI.Flex.Response TextHook(Eto.Forms.TextInputEventArgs args) =>
        Decide(new ChromeEvent.Text(args)).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);

    private void FocusHook(bool gained) => ignore(Decide(new ChromeEvent.Focus(gained)));
}
```
