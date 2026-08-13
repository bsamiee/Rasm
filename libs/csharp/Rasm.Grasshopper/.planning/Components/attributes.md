# [RASM_GRASSHOPPER_ATTRIBUTES]

`ComponentChrome` is the component-attribute interaction policy. Verified host callbacks enter one `ChromeEvent` union, one response fold returns only decisions the shells can project, and every answer receives a bounded monotone trace ordinal. `ChromeCell` holds the whole spine — trace stream, ordinal, layout shape, dispatch, and the responder subscription — so each host shell carries one field plus the callbacks its own base actually declares. `ComponentAttributes` and `ResizableAttributes<T>` are siblings over `Attributes<T>`, not a chain, and `ResizableAttributes<T>` remains the sole resize, snapping, cursor, persistence, and undo owner.

## [01]-[INDEX]

- [02]-[EVENT_ALGEBRA]: `ChromeEvent` closes the callback vocabulary and `ChromeDecision` merges two policies
- [03]-[CHROME_POLICY]: `ComponentChrome` declares the policy and `ChromeCell` owns the one dispatch spine
- [04]-[HOST_PROJECTION]: two host shells project that spine through the callbacks each base declares

## [02]-[EVENT_ALGEBRA]

- Owner: `ChromeEvent` is the closed interaction vocabulary — one case per verified host callback family, each carrying the host payload verbatim; `ChromeDecision` is the right-biased merge monoid, so composing two policies is `left | right` with the right's settled slots winning and redraw accumulating.
- Cases: `Layout`, `Pivot`, `Paint`, `Menu`, `Pointer`, `Key`, `Text`, `Focus`, `Tooltip`, `Resize`, and `Cursor`; `PointerKind` and `KeyPhase` close the sub-discriminants, while `Resize` observes the live host `Size` after the setter's own invalidation and `Leave` carries no mouse payload. `Resize` reaches the resizable shell alone and `Cursor` the component shell alone, because the resizable base implements the cursor contract explicitly and the component base carries no size.
- Entry: a policy is one `Func<ChromeEvent, ChromeState, ChromeDecision>` — total by the union's generated dispatch, never a callback subclass per interaction.
- Growth: a new host callback family is one union case; a new decision slot is one `ChromeDecision` member folded into `|`.
- Law: chrome hit-testing rides the host's own region geometry — the cell stores the `Shape` its shell's last layout callback received, `ChromeState.Region` derives the current `Capsule` through `Capsule.CreateFromOuter(Shape, Bounds)` against live bounds rather than caching a paint-time capsule, and `Hits` folds `Bounds.Contains` as the coarse pre-filter with `SlabF.Contains` as the exact rounded-capsule answer. Every policy therefore decides on the real region and never on its bounding box; before the first layout the rectangle is the whole answer, which is exactly what the host itself knows.
- Boundary: payloads stay host values — `ResponseMouseArgs`, `KeyEventArgs`, `TextInputEventArgs`, `Context`, `Skin`, `Capsule`, `Shade`, `Shape`, `ContextMenu` cross unwrapped because the decision, not the payload, is this page's domain; the input panel projects through `ComponentSpec.Panel`, never a chrome case. `Canvas/paint.md`'s `PathSpec.Hits` answers canvas-owned custom geometry the host publishes no slab for; it never reaches this island, whose region owner is the host `Capsule`.

```csharp signature
// --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
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

public readonly record struct ChromeState(
    Eto.Drawing.RectangleF Bounds, Eto.Drawing.PointF Pivot,
    Option<Grasshopper2.UI.Skinning.Shape> Shape) {
    public Option<Grasshopper2.UI.Primitives.Capsule> Region =>
        Shape.Map(shape => Grasshopper2.UI.Primitives.Capsule.CreateFromOuter(shape, Bounds));

    // Bounds is the coarse pre-filter and SlabF.Contains answers the rounded capsule exactly; before the
    // first layout no shape exists, so the rectangle is the whole answer.
    public bool Hits(Eto.Drawing.PointF at) =>
        Bounds.Contains(at) && Region.Map(capsule => capsule.Slab.Contains(at)).IfNone(true);
}

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

- Owner: `ComponentChrome` is the immutable policy value — one response fold with optional size limits, shared across every component that declares it. `ChromeCell` is the per-instance spine: the trace stream, the ordinal, the last skin `Shape`, the one `Decide` fold, and the responder subscription, so a host shell owns one field and no dispatch machinery of its own.
- Entry: `ChromeCell.Decide(ChromeEvent, IAttributes)` is the whole spine — respond, record, return; each shell hands its own `IAttributes` receiver so `Bounds` and `Pivot` read live and no shell caches placement.
- Receipt: `ChromeTrace` stores a strictly increasing per-instance ordinal, event kind, and projected decision; the bounded stream drops its oldest row past `Window`, and `LatestByKind` derives from that one stream.
- Law: the cell subscribes the host `Responses` hook events at construction and never subclasses the responder, because both host bases expose theirs as a private sealed nested class behind a get-only `Responder` — hooks are the host's own declared extension seam.
- Growth: a new projection is one fold over the same stream; a new policy slot is one `ComponentChrome` member; a new host callback is one shell override calling `Decide`.
- Boundary: the cell holds mutable per-instance state and lives on the host attribute instance; the policy value holds none and crosses instances freely.

```csharp signature
// --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
namespace Rasm.Grasshopper.Components;

// --- [MODELS] ----------------------------------------------------------------------------

public sealed record ResizePolicy(Eto.Drawing.SizeF Minimum, Eto.Drawing.SizeF Maximum);

public sealed record ComponentChrome {
    public static readonly ComponentChrome None = new();

    public Func<ChromeEvent, ChromeState, ChromeDecision> Respond { get; init; } = static (_, _) => ChromeDecision.Pass;

    public Option<ResizePolicy> Resize { get; init; } = default;
}

public readonly record struct ChromeTrace(long Ordinal, string Kind, ChromeDecision Decision);

// --- [SERVICES] --------------------------------------------------------------------------

public sealed class ChromeCell {
    public const int Window = 256;

    private readonly ComponentChrome chrome;

    private readonly Atom<Seq<ChromeTrace>> receipts = Atom(Seq<ChromeTrace>());

    private readonly Atom<long> ordinal = Atom(0L);

    private readonly Atom<Option<Grasshopper2.UI.Skinning.Shape>> skinShape = Atom(Option<Grasshopper2.UI.Skinning.Shape>.None);

    // Pointer, key, text, and focus reach the cell only through the host responder's hook events, which fire
    // exactly where that responder's own logic would answer Ignored — the ZUI grip on the component base and
    // the edge grab on the resizable base therefore keep priority over every chrome verdict.
    internal ChromeCell(ComponentChrome chrome, Grasshopper2.Doc.IAttributes host, Grasshopper2.UI.Flex.Responses responder) {
        this.chrome = chrome;
        responder.MouseOverHook += args => ignore(Pointer(PointerKind.Over, Optional(args), host));
        responder.MouseLeaveHook += () => ignore(Pointer(PointerKind.Leave, None, host));
        responder.MouseDownHook += args => Pointer(PointerKind.Down, Optional(args), host);
        responder.MouseDragHook += args => Pointer(PointerKind.Drag, Optional(args), host);
        responder.MouseUpHook += args => Pointer(PointerKind.Up, Optional(args), host);
        responder.MouseWheelHook += args => Pointer(PointerKind.Wheel, Optional(args), host);
        responder.MouseSingleClickHook += args => Pointer(PointerKind.SingleClick, Optional(args), host);
        responder.MouseDoubleClickHook += args => Pointer(PointerKind.DoubleClick, Optional(args), host);
        responder.KeyDownHook += args => Verdict(new ChromeEvent.Key(KeyPhase.Down, args), host);
        responder.KeyUpHook += args => Verdict(new ChromeEvent.Key(KeyPhase.Up, args), host);
        responder.TextInputHook += args => Verdict(new ChromeEvent.Text(args), host);
        responder.GotFocus += (_, _) => ignore(Decide(new ChromeEvent.Focus(Gained: true), host));
        responder.LostFocus += (_, _) => ignore(Decide(new ChromeEvent.Focus(Gained: false), host));
    }

    public Seq<ChromeTrace> Receipts => receipts.Value;

    public HashMap<string, ChromeTrace> LatestByKind =>
        receipts.Value.Fold(HashMap<string, ChromeTrace>(), static (held, receipt) => held.AddOrUpdate(receipt.Kind, receipt));

    public ChromeDecision Decide(ChromeEvent happening, Grasshopper2.Doc.IAttributes host) {
        ChromeDecision decision = chrome.Respond(happening, new ChromeState(host.Bounds, host.Pivot, skinShape.Value));
        long next = ordinal.Swap(static current => checked(current + 1L));
        _ = receipts.Swap(held => (held.Count >= Window ? held.Tail : held).Add(new ChromeTrace(
            Ordinal: next,
            Kind: happening.GetType().Name,
            Decision: decision)));
        return decision;
    }

    internal ChromeDecision Laid(Grasshopper2.UI.Skinning.Shape shape, Grasshopper2.Doc.IAttributes host) {
        ignore(skinShape.Swap(_ => Some(shape)));
        return Decide(new ChromeEvent.Layout(shape), host);
    }

    private Grasshopper2.UI.Flex.Response Pointer(
        PointerKind kind, Option<Grasshopper2.UI.Flex.ResponseMouseArgs> args, Grasshopper2.Doc.IAttributes host) =>
        Verdict(new ChromeEvent.Pointer(kind, args), host);

    private Grasshopper2.UI.Flex.Response Verdict(ChromeEvent happening, Grasshopper2.Doc.IAttributes host) =>
        Decide(happening, host).Verdict.IfNone(Grasshopper2.UI.Flex.Response.Ignored);
}
```

## [04]-[HOST_PROJECTION]

- Owner: `ChromeHost` and `ResizableChromeHost` are two shells over one `ChromeCell`. Each holds the cell, its own host-callback overrides, and nothing else — the two bases share no layout or decoration member, which is the whole reason two shells exist.
- Entry: `ChromeHost` extends `ComponentAttributes` and hooks `LayoutBounds(Shape)` and `DrawForegroundDecorations(Context, Skin, Capsule, Shade)`, the component base's own layout and decoration seams. `ResizableChromeHost` extends `ResizableAttributes<Component>`, which derives `Attributes<Component>` directly and declares neither member, so it hooks `Layout(Shape)` and `protected Draw(Context, Skin, Capsule)` and reads its shade as `skin.Shades[Owner]` exactly as the base body does.
- Law: a chrome verdict is plug-in behavior, never primary. Each hook fires only where the host responder's own logic would answer `Response.Ignored`, so a `Down` verdict is silent over a ZUI grip on the component base and over an engaged edge on the resizable base — which is the correct layering, because the host owns parameter insertion and resizing and chrome owns what is left. A policy needing primary ownership of a gesture belongs on a canvas responder (`Canvas/interaction.md`), which overrides rather than subscribes.
- Auto: an unanswered decision falls back to `Response.Ignored`, the base tooltip verdict, or the default cursor. `ResizableAttributes<T>.Size`'s setter is the whole size commit — clamp, round, `CustomValues` persistence, bounds re-frame, then the empty `InvalidateLayout()` — so the resizable shell's `InvalidateLayout` override is the committed-size observation point, and `mounted` guards the call the base constructor makes before the shell's own fields exist.
- Boundary: `ResizableAttributes<T>` implements `ICursorAwareAttributes.CursorAt` EXPLICITLY, so a subclass cannot override it and re-listing the interface would re-implement the map and silently delete the host's edge-resize cursors; the resizable shell therefore carries no cursor arm and `ChromeEvent.Cursor` reaches the component shell alone. The base also owns `ResizingFrame`, `SnappingConstraints.CreateFromDocument`, `SnappingSettings.Current`, `CanvasSnapToObjects` toggling, and the `ResizeAction` undo record; `EdgeSize` is its `public const int` `6`.

```csharp signature
// --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
using Grasshopper2.Components;

namespace Rasm.Grasshopper.Components;

// --- [COMPOSITION] -----------------------------------------------------------------------

public sealed class ChromeHost :
    Grasshopper2.Doc.Attributes.ComponentAttributes,
    Grasshopper2.UI.IContextMenuAware,
    Grasshopper2.Doc.ICursorAwareAttributes {
    private readonly ChromeCell cell;

    private ChromeHost(Component owner, ComponentChrome chrome) : base(owner) =>
        cell = new ChromeCell(chrome, this, Responder);

    public static Grasshopper2.Doc.IAttributes Mount(Component owner, ComponentChrome chrome) =>
        chrome.Resize.Match(
            Some: policy => (Grasshopper2.Doc.IAttributes)new ResizableChromeHost(owner, chrome, policy),
            None: () => new ChromeHost(owner, chrome));

    public ChromeCell Chrome => cell;

    protected override void LayoutBounds(Grasshopper2.UI.Skinning.Shape shape) {
        base.LayoutBounds(shape);
        ignore(cell.Laid(shape, this));
    }

    protected override void DrawForegroundDecorations(
        Eto.Drawing.Context context, Grasshopper2.UI.Skinning.Skin skin,
        Grasshopper2.UI.Primitives.Capsule capsule, Grasshopper2.UI.Skinning.Shade shade) {
        base.DrawForegroundDecorations(context, skin, capsule, shade);
        ignore(cell.Decide(new ChromeEvent.Paint(context, skin, capsule, shade), this));
    }

    public override bool ShowTooltipAt(Eto.Drawing.PointF point) =>
        cell.Decide(new ChromeEvent.Tooltip(point), this).Tip.IsSome || base.ShowTooltipAt(point);

    protected override void PivotMoved(Eto.Drawing.PointF oldPivot, Eto.Drawing.PointF newPivot) {
        base.PivotMoved(oldPivot, newPivot);
        ignore(cell.Decide(new ChromeEvent.Pivot(oldPivot, newPivot), this));
    }

    public void AppendToMenu(Eto.Forms.ContextMenu menu) => ignore(cell.Decide(new ChromeEvent.Menu(menu), this));

    public Eto.Forms.Cursor CursorAt(Eto.Drawing.PointF at) =>
        cell.Decide(new ChromeEvent.Cursor(at), this).Pointer.IfNone(Eto.Forms.Cursors.Default);
}

public sealed class ResizableChromeHost :
    Grasshopper2.Doc.Attributes.ResizableAttributes<Component>,
    Grasshopper2.UI.IContextMenuAware {
    private readonly ChromeCell cell;

    private Eto.Drawing.SizeF observedSize;

    private bool mounted;

    // The base constructor assigns Size, whose setter calls the virtual InvalidateLayout before any field
    // below is initialized; `mounted` is the construction fence that override reads.
    internal ResizableChromeHost(Component owner, ComponentChrome chrome, ResizePolicy policy) :
        base(owner, policy.Minimum, policy.Maximum) {
        cell = new ChromeCell(chrome, this, Responder);
        observedSize = Size;
        mounted = true;
    }

    public ChromeCell Chrome => cell;

    public override void InvalidateLayout() {
        base.InvalidateLayout();
        if (mounted && observedSize != Size) {
            observedSize = Size;
            ignore(cell.Decide(new ChromeEvent.Resize(Size), this));
        }
    }

    public override void Layout(Grasshopper2.UI.Skinning.Shape shape) {
        base.Layout(shape);
        ignore(cell.Laid(shape, this));
    }

    // ResizableAttributes<T> leaves Attributes<T>'s protected Draw open; the base body resolves its shade the
    // same way, so the chrome sees the identical (capsule, shade) pair the component shell's decoration seam does.
    protected override void Draw(
        Eto.Drawing.Context context, Grasshopper2.UI.Skinning.Skin skin, Grasshopper2.UI.Primitives.Capsule capsule) {
        base.Draw(context, skin, capsule);
        ignore(cell.Decide(new ChromeEvent.Paint(context, skin, capsule, skin.Shades[Owner]), this));
    }

    public override bool ShowTooltipAt(Eto.Drawing.PointF point) =>
        cell.Decide(new ChromeEvent.Tooltip(point), this).Tip.IsSome || base.ShowTooltipAt(point);

    protected override void PivotMoved(Eto.Drawing.PointF oldPivot, Eto.Drawing.PointF newPivot) {
        base.PivotMoved(oldPivot, newPivot);
        ignore(cell.Decide(new ChromeEvent.Pivot(oldPivot, newPivot), this));
    }

    public void AppendToMenu(Eto.Forms.ContextMenu menu) => ignore(cell.Decide(new ChromeEvent.Menu(menu), this));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
