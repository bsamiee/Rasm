using System.Reflection;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using EtoContext = Eto.Drawing.Context;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhLazyStrings = Grasshopper2.UI.LazyStrings;
using GhTooltip = Grasshopper2.UI.Tooltip.Frame;
using GhUiNumber = Grasshopper2.UI.UiNumber;
using NativeFloatingButton = Grasshopper2.UI.Flex.FloatingButton;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public partial record TooltipPainter {
    private TooltipPainter() { }
    public sealed record ShortcutKeysCase(string Prefix, Keys Keys, string Suffix) : TooltipPainter;
    public sealed record ShortcutCharCase(string Prefix, char Key, string Suffix) : TooltipPainter;
    public sealed record TextAndIconCase(Seq<object> Elements) : TooltipPainter;
    public sealed record CustomCase(Action<EtoContext, Rectangle> Paint, Size PaintingSize) : TooltipPainter;

    public static TooltipPainter Shortcut(string prefix, Keys keys, string suffix) => new ShortcutKeysCase(Prefix: prefix, Keys: keys, Suffix: suffix);
    public static TooltipPainter Shortcut(string prefix, char key, string suffix) => new ShortcutCharCase(Prefix: prefix, Key: key, Suffix: suffix);
    public static TooltipPainter TextAndIcon(params object[] elements) => new TextAndIconCase(Elements: toSeq(elements));
    public static TooltipPainter Custom(Action<EtoContext, Rectangle> paint, Size paintingSize) => new CustomCase(Paint: paint, PaintingSize: paintingSize);

    // GH2 painters return (Action, Size); deconstruct here so Show() passes them as separate args.
    internal (Action<EtoContext, Rectangle> Paint, Size Size) Resolve() =>
        Switch(
            shortcutKeysCase: static s => GhTooltip.CreateShortcutPainter(prefix: s.Prefix, keys: s.Keys, suffix: s.Suffix),
            shortcutCharCase: static s => GhTooltip.CreateShortcutPainter(prefix: s.Prefix, key: s.Key, suffix: s.Suffix),
            textAndIconCase: static t => GhTooltip.CreateTextAndIconPainter(elements: [.. t.Elements]),
            customCase: static c => (c.Paint, c.PaintingSize));
}

[SkipUnionOps]
[Union]
public partial record TooltipBody {
    private TooltipBody() { }
    public sealed record PlainCase : TooltipBody;
    public sealed record ItemsCase(GhLazyStrings Items) : TooltipBody;
    public sealed record PanesCase(Seq<GhLazyStrings> Panes) : TooltipBody;
    public sealed record PainterCase(TooltipPainter Painter) : TooltipBody;

    public static readonly TooltipBody Plain = new PlainCase();
    public static TooltipBody FromItems(GhLazyStrings items) => new ItemsCase(Items: items);
    public static TooltipBody FromPanes(Seq<GhLazyStrings> panes) => new PanesCase(Panes: panes);
    public static TooltipBody FromPainter(TooltipPainter painter) => new PainterCase(Painter: painter);
}

[SkipUnionOps]
[Union]
public partial record TooltipOp {
    private TooltipOp() { }
    public sealed record ShowCase(IIcon Icon, string Caption, string Message, TooltipBody Body, bool Warnings = false, bool Errors = false) : TooltipOp;
    public sealed record HideCase : TooltipOp;
    public sealed record InvalidateCase : TooltipOp;
    public sealed record StatusCase : TooltipOp;
    public sealed record LayoutCase : TooltipOp;

    public static TooltipOp Show(IIcon icon, string caption, string message, TooltipBody? body = null, bool warnings = false, bool errors = false) =>
        new ShowCase(Icon: icon, Caption: caption, Message: message, Body: body ?? TooltipBody.Plain, Warnings: warnings, Errors: errors);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        showCase: static _ => GrasshopperUiPolicy.Canvas(),
        hideCase: static _ => GrasshopperUiPolicy.Read,
        invalidateCase: static _ => GrasshopperUiPolicy.Read,
        statusCase: static _ => GrasshopperUiPolicy.Read,
        layoutCase: static _ => GrasshopperUiPolicy.Read);
}

[SkipUnionOps]
[Union]
public partial record CanvasChromeOp : IUiOp {
    private CanvasChromeOp() { }
    public sealed record TooltipCase(TooltipOp Op) : CanvasChromeOp;
    public sealed record FloatingButtonCase(FloatingButtonOp Op) : CanvasChromeOp;
    public sealed record InteractionCase(InteractionOp Op) : CanvasChromeOp;
    public sealed record ComposeCase(Seq<CanvasChromeOp> Ops) : CanvasChromeOp;

    public static CanvasChromeOp Tooltip(TooltipOp op) => new TooltipCase(Op: op);
    public static CanvasChromeOp FloatingButton(FloatingButtonOp op) => new FloatingButtonCase(Op: op);
    public static CanvasChromeOp Interaction(InteractionOp op) => new InteractionCase(Op: op);
    public static CanvasChromeOp Compose(params CanvasChromeOp[] ops) => new ComposeCase(Ops: toSeq(ops));

    // Compose folds each child policy into the union; an empty compose reads.
    public GrasshopperUiPolicy UiPolicy => Switch(
        tooltipCase: static t => t.Op.UiPolicy,
        floatingButtonCase: static _ => GrasshopperUiPolicy.Canvas(),
        interactionCase: static _ => GrasshopperUiPolicy.Canvas(),
        composeCase: static c => c.Ops.Fold(GrasshopperUiPolicy.Read, static (acc, child) => acc | child.UiPolicy));
}

[SkipUnionOps]
[Union]
public partial record CanvasChromeResult {
    private CanvasChromeResult() { }
    public sealed record SubscriptionCase(Subscription Subscription) : CanvasChromeResult;
    public sealed record UnitCase : CanvasChromeResult;
    public sealed record TooltipStatusCase(TooltipSnapshot Snapshot) : CanvasChromeResult;
    public sealed record TooltipLayoutCase(TooltipLayoutSnapshot Snapshot) : CanvasChromeResult;
    public sealed record FloatingButtonStatusCase(FloatingButtonSnapshot Snapshot) : CanvasChromeResult;
    public sealed record FloatingButtonFoundCase(Option<FloatingButtonInfo> Info) : CanvasChromeResult;
    public sealed record InteractionStatusCase(InteractionSnapshot Snapshot) : CanvasChromeResult;

    internal static readonly CanvasChromeResult UnitInstance = new UnitCase();

    // Producers know their result type statically and construct their case directly (no runtime type-switch);
    // Sub/Found/Status are the shared projections for the recurring FloatingButton result shapes.
    internal static CanvasChromeResult Sub(Subscription subscription) => new SubscriptionCase(Subscription: subscription);
    internal static CanvasChromeResult Found(Option<FloatingButtonInfo> info) => new FloatingButtonFoundCase(Info: info);
    internal static CanvasChromeResult Status(FloatingButtonSnapshot snapshot) => new FloatingButtonStatusCase(Snapshot: snapshot);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonHandlers(
    Option<FloatingButtonHandler> Click = default,
    Option<FloatingButtonHandler> MouseDown = default,
    Option<FloatingButtonHandler> MouseUp = default);

[SkipUnionOps]
[Union]
public partial record FloatingPlacement {
    private FloatingPlacement() { }
    public sealed record PositionCase(FloatingPosition Value) : FloatingPlacement;
    public sealed record AnchorCase(PointF Point) : FloatingPlacement;
    public sealed record RelativeCase(string AnchorName, PointF Offset) : FloatingPlacement;

    public static FloatingPlacement Position(FloatingPosition value) => new PositionCase(Value: value);
    public static FloatingPlacement Anchor(PointF point) => new AnchorCase(Point: point);
    public static FloatingPlacement Relative(string anchorName, PointF offset) => new RelativeCase(AnchorName: anchorName, Offset: offset);

    internal Fin<(FloatingPosition Position, Option<PointF> Anchor)> Resolve(GhCanvas canvas) =>
        Switch(
            state: canvas,
            positionCase: static (_, p) => Fin.Succ(value: (Position: p.Value, Anchor: Option<PointF>.None)),
            anchorCase: static (_, a) => Op.Of(name: nameof(Anchor)).AcceptPoint(value: a.Point, detail: "non-finite anchor")
                .Map(point => (Position: FloatingPosition.Anchored, Anchor: Some(point))),
            relativeCase: static (canvas, r) =>
                from anchorName in Op.Of(name: nameof(Relative)).AcceptText(value: r.AnchorName)
                from validOffset in Op.Of(name: nameof(Relative)).AcceptPoint(value: r.Offset, detail: "non-finite offset")
                from anchor in Optional(canvas.FloatingButtons.FindByName(name: anchorName))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Relative)), detail: $"anchor button '{anchorName}' not found"))
                select (Position: FloatingPosition.Anchored, Anchor: Some(new PointF(x: anchor.Anchor.X + validOffset.X, y: anchor.Anchor.Y + validOffset.Y))));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingNumeric(GhUiNumber Value, string ValueKey, Option<Func<decimal, Fin<Unit>>> Changed = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonSpec(
    FloatingPlacement Placement,
    string Name,
    string Info,
    IIcon Icon,
    Option<Color> Colour = default,
    FloatingButtonHandlers Handlers = default,
    Option<FloatingNumeric> Numeric = default);

[SkipUnionOps]
[Union]
public partial record FloatingButtonOp {
    private FloatingButtonOp() { }
    public sealed record AddCase(FloatingButtonSpec Spec) : FloatingButtonOp;
    public sealed record ModifyCase(string Name, Option<string> Info = default, Option<IIcon> Icon = default, Option<Color> Colour = default, Option<(PointF Point, bool Immediate)> Anchor = default) : FloatingButtonOp;
    public sealed record ShowNamedCase(Seq<string> Names) : FloatingButtonOp;
    public sealed record HideNamedCase(Seq<string> Names) : FloatingButtonOp;
    public sealed record CloseNamedCase(Seq<string> Names) : FloatingButtonOp;
    public sealed record CloseAllCase : FloatingButtonOp;
    public sealed record FindByNameCase(string Name) : FloatingButtonOp;
    public sealed record FindByPointCase(PointF ControlPoint) : FloatingButtonOp;
    public sealed record StatusCase : FloatingButtonOp;

    public static FloatingButtonOp Add(FloatingButtonSpec spec) => new AddCase(Spec: spec);
}

[SkipUnionOps]
[Union]
public partial record InteractionOp {
    private InteractionOp() { }
    public sealed record PushCase(IInteraction Target) : InteractionOp;
    public sealed record RegisterCase(IResponsive Responsive, CoordinateSystem System) : InteractionOp;
    // Hover/ContextMenu: GH2 emits MouseHover / PopulateContextMenu — handler may mutate the
    // menu.Items collection or read the hover point.
    public sealed record HoverCase(Option<TimeSpan> Delay, Func<MouseHoverSnapshot, Fin<Unit>> Handler) : InteractionOp;
    public sealed record ContextMenuCase(Func<ContextMenuSnapshot, Fin<Unit>> Handler) : InteractionOp;
    public sealed record StatusCase : InteractionOp;
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct TooltipSnapshot(bool Visible, Option<Guid> OwnerToken, Option<PointF> MouseLocationWhenShown = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct TooltipLayoutSnapshot(int MinimumWidth, int MaximumWidth, int MaximumHeight, int Padding, int DoublePadding, int IconSize);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonSnapshot(int Count, int NormalCount, int HiddenCount, int VisibleCount, Seq<string> Names);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonInfo(string Name, string Info, FloatingPosition Position, FloatingState State, bool Enabled, bool HasFocus, Color Colour, Option<PointF> Anchor, Option<decimal> NumericValue = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InteractionSnapshot(int InteractionCount, int ResponsiveCount, bool HasFocus, Option<string> FocusNomen);

[SkipUnionOps]
[Union]
public partial record DecisionDelta {
    private DecisionDelta() { }
    public sealed record BoundsCase(RectangleF Value) : DecisionDelta;
    public sealed record SizeCase(SizeF Value) : DecisionDelta;
    public sealed record CursorCase(Cursor Value) : DecisionDelta;
    public sealed record ResponseCase(Response Value) : DecisionDelta;
    public static DecisionDelta Bounds(RectangleF value) => new BoundsCase(Value: value);
    public static DecisionDelta Size(SizeF value) => new SizeCase(Value: value);
    public static DecisionDelta Cursor(Cursor value) => new CursorCase(Value: value);
    public static DecisionDelta Respond(Response value) => new ResponseCase(Value: value);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ResizeSession(RectangleF Start, RectangleF Current, Option<SnappingSnapshot> Snap = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct MouseHoverSnapshot(PointF ControlPoint, PointF ContentPoint);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ContextMenuSnapshot(ContextMenu Menu, MouseEventArgs MouseEvent);

// --- [BOUNDARY] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static class TooltipRail {
    private static readonly Op RailOp = Op.Of(name: nameof(TooltipRail));

    internal static GrasshopperUiIntent<TooltipLayoutSnapshot> Layout() =>
        GhUi.Read(run: _ =>
            RailOp.Attempt(body: () => typeof(GhTooltip).Assembly.GetType(name: "Grasshopper2.UI.Tooltip.Layout", throwOnError: true)!, what: "Tooltip.Layout type")
                .Bind(ReadLayout));

    private static Fin<TooltipLayoutSnapshot> ReadLayout(Type layoutType) =>
        Seq("MinimumWidth", "MaximumWidth", "MaximumHeight", "Padding", "DoublePadding", "IconSize")
            .TraverseM(name => ReadInt(type: layoutType, name: name)).As()
            .Map(values => new TooltipLayoutSnapshot(
                MinimumWidth: values[0],
                MaximumWidth: values[1],
                MaximumHeight: values[2],
                Padding: values[3],
                DoublePadding: values[4],
                IconSize: values[5]));

    // Optional(value as int?) drops the Convert.ToInt32 boxing path and fails closed when a constant is
    // absent or non-int instead of throwing into the outer Attempt.
    private static Fin<int> ReadInt(Type type, string name) =>
        Optional(type.GetField(name: name, bindingAttr: BindingFlags.Public | BindingFlags.Static))
            .Bind(field => Optional(field.GetValue(obj: null)))
            .Bind(value => Optional(value as int?))
            .ToFin(Fail: UiFault.MutationRejected(op: RailOp, detail: $"Tooltip.Layout.{name} is not a readable int constant"));
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class CanvasChrome {
    internal static GrasshopperUiIntent<CanvasChromeResult> Dispatch(CanvasChromeOp op) =>
        op.Switch(
            tooltipCase: static t => Tooltip.Dispatch(op: t.Op),
            floatingButtonCase: static f => FloatingButton.Dispatch(op: f.Op),
            interactionCase: static i => Interaction.Dispatch(op: i.Op),
            composeCase: static c => Compose(ops: c.Ops));

    // Compose threads every child Dispatch; each must yield a Subscription so the leases fold via `|` into
    // one disposable. A non-Subscription child (status/find/layout) breaks the contract and short-circuits.
    private static GrasshopperUiIntent<CanvasChromeResult> Compose(Seq<CanvasChromeOp> ops) =>
        GhUi.Canvas(run: scope =>
            ops.TraverseM(child => Dispatch(op: child).Run(scope: scope)).As()
                .Bind(static results => results
                    .TraverseM(static result => result is CanvasChromeResult.SubscriptionCase sub
                        ? Fin.Succ(value: sub.Subscription)
                        : Fin.Fail<Subscription>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Compose)), detail: "every composed op must yield a subscription"))).As()
                    .Map(static subs => CanvasChromeResult.Sub(subscription: subs.Fold(Subscription.Empty, static (acc, sub) => acc | sub)))));
}

internal static class Tooltip {
    // Token gate prevents a stale Subscription's Dispose from hiding a later Show that supplanted it.
    private static readonly OwnedSubscription<int> Owner = new();
    // Eto.Forms.Mouse.Position captured at Show time substitutes for the host-private
    // Tooltip.Frame.MouseLocationWhenShown; Hide clears it so a stale snapshot reads None.
    private static readonly Atom<Option<PointF>> ShownLocation = Atom(value: Option<PointF>.None);

    internal static GrasshopperUiIntent<CanvasChromeResult> Dispatch(TooltipOp op) =>
        op.Switch(
            showCase: static s => Bind(invoke: () => ShowNative(show: s)).Map(CanvasChromeResult.Sub),
            hideCase: static _ => HideNow().Map(static _ => CanvasChromeResult.UnitInstance),
            invalidateCase: static _ => InvalidateNow().Map(static _ => CanvasChromeResult.UnitInstance),
            statusCase: static _ => SnapshotNow().Map(static snap => (CanvasChromeResult)new CanvasChromeResult.TooltipStatusCase(Snapshot: snap)),
            layoutCase: static _ => TooltipRail.Layout().Map(static snap => (CanvasChromeResult)new CanvasChromeResult.TooltipLayoutCase(Snapshot: snap)));

    internal static GrasshopperUiIntent<Unit> HideNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(HideNow)).Attempt(body: HideNative, what: "Tooltip.Frame.Hide"));

    internal static GrasshopperUiIntent<Unit> InvalidateNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(InvalidateNow)).Attempt(body: GhTooltip.Invalidate, what: "Tooltip.Frame.Invalidate"));

    internal static GrasshopperUiIntent<TooltipSnapshot> SnapshotNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(SnapshotNow)).Attempt(body: SnapshotNative, what: "Tooltip.Frame.Visible"));

    private static void HideNative() {
        Owner.Clear(key: 0);
        _ = ShownLocation.Swap(static _ => Option<PointF>.None);
        GhTooltip.Hide();
    }

    private static TooltipSnapshot SnapshotNative() =>
        new(Visible: GhTooltip.Visible, OwnerToken: Owner.Owner(key: 0), MouseLocationWhenShown: ShownLocation.Value);

    private static GrasshopperUiIntent<Subscription> Bind(Action invoke) =>
        GhUi.Canvas(run: scope => scope.NeedCanvas().Bind(_ => Resource(invoke: invoke)));

    private static Fin<Subscription> Resource(Action invoke) =>
        Owner.Bind(key: 0, attach: invoke, detach: _key => {
            _ = ShownLocation.Swap(static _ => Option<PointF>.None);
            GhTooltip.Hide();
        });

    private static void ShowNative(TooltipOp.ShowCase show) {
        _ = ShownLocation.Swap(static _ => Mouse.IsSupported ? Some(Mouse.Position) : Option<PointF>.None);
        show.Body.Switch(
            state: show,
            plainCase: static (s, _) => GhTooltip.Show(icon: s.Icon, caption: s.Caption, message: s.Message, warnings: s.Warnings, errors: s.Errors),
            itemsCase: static (s, i) => GhTooltip.Show(s.Icon, s.Caption, s.Message, i.Items, s.Warnings, s.Errors),
            panesCase: static (s, p) => GhTooltip.Show(icon: s.Icon, caption: s.Caption, message: s.Message, items: [.. p.Panes], warnings: s.Warnings, errors: s.Errors),
            painterCase: static (s, p) => {
                (Action<EtoContext, Rectangle> paint, Size size) = p.Painter.Resolve();
                GhTooltip.Show(icon: s.Icon, caption: s.Caption, message: s.Message, painter: paint, paintingSize: size, warnings: s.Warnings, errors: s.Errors);
            });
    }
}

internal static class FloatingButton {
    // Token gate per name: stale Subscription.Dispose only Close(name) when its Guid still owns the slot.
    private static readonly OwnedSubscription<string> Owners = new();

    internal static GrasshopperUiIntent<CanvasChromeResult> Dispatch(FloatingButtonOp op) =>
        op.Switch(
            addCase: static a => Add(a.Spec).Map(CanvasChromeResult.Sub),
            modifyCase: static m => Modify(m.Name, m.Info, m.Icon, m.Colour, m.Anchor).Map(static _ => CanvasChromeResult.UnitInstance),
            showNamedCase: static s => SetVisible(s.Names, visible: true).Map(static _ => CanvasChromeResult.UnitInstance),
            hideNamedCase: static h => SetVisible(h.Names, visible: false).Map(static _ => CanvasChromeResult.UnitInstance),
            closeNamedCase: static c => Close(c.Names).Map(static _ => CanvasChromeResult.UnitInstance),
            closeAllCase: static _ => CloseAll().Map(static _ => CanvasChromeResult.UnitInstance),
            findByNameCase: static f => FindByName(f.Name).Map(CanvasChromeResult.Found),
            findByPointCase: static f => FindByPoint(f.ControlPoint).Map(CanvasChromeResult.Found),
            statusCase: static _ => SnapshotNow().Map(CanvasChromeResult.Status));

    internal static GrasshopperUiIntent<Subscription> Add(FloatingButtonSpec spec) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from placement in Optional(spec.Placement).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Add)), detail: "placement is required"))
            from icon in Optional(spec.Icon).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Add)), detail: "icon is required"))
            from name in Op.Of(name: nameof(Add)).AcceptText(value: spec.Name)
            from _duplicate in canvas.FloatingButtons.IsDefined(name: name)
                ? Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Add)), detail: $"button '{name}' already exists"))
                : Fin.Succ(unit)
            from resolved in placement.Resolve(canvas: canvas)
            from sub in AttachButton(
                canvas: canvas,
                spec: spec with { Name = name, Info = spec.Info ?? string.Empty, Icon = icon },
                position: resolved.Position,
                anchor: resolved.Anchor)
            select sub);

    private static Fin<Subscription> AttachButton(
        GhCanvas canvas,
        FloatingButtonSpec spec,
        FloatingPosition position,
        Option<PointF> anchor) {
        NativeFloatingButton? registered = null;
        EventHandler<FloatingButtonEventArgs<GhUiNumber>>? changed = null;
        return Resource(
            name: spec.Name,
            canvas: canvas,
            attach: () => {
                Color? colour = spec.Colour.Map(static picked => (Color?)picked).IfNone((Color?)null);
                FloatingButtonHandler click = spec.Handlers.Click.IfNone(NoOp);
                FloatingButtonHandler mouseDown = spec.Handlers.MouseDown.IfNone(NoOp);
                FloatingButtonHandler mouseUp = spec.Handlers.MouseUp.IfNone(NoOp);
                _ = anchor.Map(point => Op.Side(() => canvas.FloatingButtons.AddAnchored(anchor: point, name: spec.Name, info: spec.Info, colour: colour, icon: spec.Icon, click: click, mouseDown: mouseDown, mouseUp: mouseUp)))
                    .IfNone(() => Op.Side(() => canvas.FloatingButtons.Add(position: position, name: spec.Name, info: spec.Info, colour: colour, icon: spec.Icon, click: click, mouseDown: mouseDown, mouseUp: mouseUp)));
                registered = canvas.FloatingButtons.FindByName(name: spec.Name)
                    ?? throw new InvalidOperationException(message: $"FloatingButton '{spec.Name}' was not registered after Add().");
                _ = spec.Numeric.Iter(numeric => {
                    registered.MakeNumeric(number: numeric.Value, valueKey: numeric.ValueKey);
                    changed = numeric.Changed is { IsSome: true, Case: Func<decimal, Fin<Unit>> sink }
                        ? (_, _) => _ = GrasshopperUi.Handler(valid: () => registered.NumericValue is GhUiNumber current ? sink(arg: current.Value) : Fin.Succ(value: unit))
                        : null;
                    _ = Optional(changed).Iter(handler => registered.ValueChanged += handler);
                });
            },
            detach: _ => {
                if (registered is not null && changed is not null) {
                    registered.ValueChanged -= changed;
                }
            });
    }

    internal static GrasshopperUiIntent<Unit> Modify(
        string name,
        Option<string> info = default,
        Option<IIcon> icon = default,
        Option<Color> colour = default,
        Option<(PointF Point, bool Immediate)> anchor = default) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from _found in Optional(canvas.FloatingButtons.FindByName(name: name))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Modify)), detail: $"button '{name}' not found"))
            from validAnchor in anchor.Match(
                Some: a => Op.Of(name: nameof(Modify)).AcceptPoint(value: a.Point, detail: "non-finite anchor").Map(p => Some((Point: p, a.Immediate))),
                None: () => Fin.Succ(Option<(PointF Point, bool Immediate)>.None))
            from _ in Seq(
                ModifyField(value: info, mutate: v => canvas.FloatingButtons.ModifyInfo(name: name, buttonInfo: v), field: "ModifyInfo"),
                ModifyField(value: icon, mutate: v => canvas.FloatingButtons.ModifyIcon(name: name, icon: v), field: "ModifyIcon"),
                ModifyField(value: colour, mutate: v => canvas.FloatingButtons.ModifyColour(name: name, colour: v), field: "ModifyColour"),
                ModifyField(value: validAnchor, mutate: av => canvas.FloatingButtons.ModifyAnchor(name: name, anchor: av.Point, immediate: av.Immediate), field: "ModifyAnchor"))
                .TraverseM(static step => step).As()
            select unit);

    // Each present field self-names its own Op.Attempt so a single failed mutation short-circuits the
    // TraverseM with the precise field; an absent field is a no-op Succ.
    private static Fin<Unit> ModifyField<T>(Option<T> value, Action<T> mutate, string field) =>
        value.Match(
            Some: present => Op.Of(name: nameof(Modify)).Attempt(body: () => mutate(present), what: field),
            None: static () => Fin.Succ(value: unit));

    internal static GrasshopperUiIntent<Unit> SetVisible(Seq<string> names, bool visible) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from _ in Op.Of(name: nameof(SetVisible)).Attempt(
                body: () => _ = visible
                    ? Op.Side(() => canvas.FloatingButtons.Show([.. names]))
                    : Op.Side(() => canvas.FloatingButtons.Hide([.. names])),
                what: visible ? "Show" : "Hide")
            select unit);

    internal static GrasshopperUiIntent<Unit> Close(Seq<string> names) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from _ in Op.Of(name: nameof(Close)).Attempt(
                body: () => {
                    canvas.FloatingButtons.Close([.. names]);
                    _ = names.Iter(Owners.Clear);
                    return unit;
                },
                what: "FloatingButtonCollection.Close")
            select unit);

    internal static GrasshopperUiIntent<Unit> CloseAll() =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from _ in Op.Of(name: nameof(CloseAll)).Attempt(
                body: () => {
                    canvas.FloatingButtons.CloseAll();
                    Owners.ClearAll();
                    return unit;
                },
                what: "FloatingButtonCollection.CloseAll")
            select unit);

    internal static GrasshopperUiIntent<Option<FloatingButtonInfo>> FindByName(string name) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from result in Op.Of(name: nameof(FindByName)).Attempt(
                body: () => Optional(canvas.FloatingButtons.FindByName(name: name)).Map(InfoOf),
                what: "FloatingButtonCollection.FindByName")
            select result);

    internal static GrasshopperUiIntent<Option<FloatingButtonInfo>> FindByPoint(PointF point) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Op.Of(name: nameof(FindByPoint)).AcceptPoint(value: point, detail: "non-finite point")
            from result in Op.Of(name: nameof(FindByPoint)).Attempt(
                body: () => Optional(canvas.FloatingButtons.FindByPoint(point: valid)).Map(InfoOf),
                what: "FloatingButtonCollection.FindByPoint")
            select result);

    internal static GrasshopperUiIntent<FloatingButtonSnapshot> SnapshotNow() =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from snap in Op.Of(name: nameof(SnapshotNow)).Attempt(
                body: () => new FloatingButtonSnapshot(
                    Count: canvas.FloatingButtons.Count,
                    NormalCount: canvas.FloatingButtons.StateCount(state: FloatingState.Normal),
                    HiddenCount: canvas.FloatingButtons.StateCount(state: FloatingState.Hidden),
                    VisibleCount: toSeq(canvas.FloatingButtons.VisibleButtons).Count,
                    Names: toSeq(canvas.FloatingButtons.Names)),
                what: "FloatingButtonCollection snapshot")
            select snap);

    private static Fin<Subscription> Resource(string name, GhCanvas canvas, Action attach, Action<string>? detach = null) =>
        Owners.Bind(
            key: name,
            attach: attach,
            detach: target => {
                _ = Optional(detach).Iter(d => d(target));
                canvas.FloatingButtons.Close(target);
            });

    private static FloatingButtonInfo InfoOf(NativeFloatingButton button) =>
        new(Name: button.Name, Info: button.Info,
            Position: button.Position, State: button.State,
            Enabled: button.Enabled, HasFocus: button.HasFocus,
            Colour: button.Colour,
            Anchor: button.Position == FloatingPosition.Anchored ? Some(button.Anchor) : Option<PointF>.None,
            // NumericValue is null until MakeNumeric; project the host UiNumber.Value (decimal) through Option.
            NumericValue: Optional(button.NumericValue).Map(static value => value.Value));

    private static Response NoOp(NativeFloatingButton _, MouseEventArgs __) => Response.Ignored;
}

internal static class Interaction {
    internal static GrasshopperUiIntent<CanvasChromeResult> Dispatch(InteractionOp op) =>
        op.Switch(
            pushCase: static p => Push(p.Target).Map(CanvasChromeResult.Sub),
            registerCase: static r => Register(r.Responsive, r.System).Map(CanvasChromeResult.Sub),
            hoverCase: static h => Hover(h.Delay, h.Handler).Map(CanvasChromeResult.Sub),
            contextMenuCase: static c => ContextMenu(c.Handler).Map(CanvasChromeResult.Sub),
            statusCase: static _ => SnapshotNow().Map(static snap => (CanvasChromeResult)new CanvasChromeResult.InteractionStatusCase(Snapshot: snap)));

    // PushInteraction only pushes onto FlexControl._focus; the interaction self-terminates by popping
    // itself when complete. There is no public pop-by-reference (UnregisterIResponsive no-ops on an
    // interaction never registered in _responsives), so the detach is intentionally empty — the old
    // valid.Release(canvas) released the interaction WITHOUT popping _focus, leaking a dangling entry.
    internal static GrasshopperUiIntent<Subscription> Push(IInteraction target) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(target).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Push)), detail: "interaction is required"))
            from sub in Subscription.Bind(
                attach: () => canvas.PushInteraction(interaction: valid),
                detach: static () => { },
                marshalToUi: true)
            select sub);

    internal static GrasshopperUiIntent<Subscription> Register(IResponsive responsive, CoordinateSystem system) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(responsive).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Register)), detail: "responsive is required"))
            from sub in Subscription.Bind(
                attach: () => canvas.RegisterIResponsive(responsive: valid, system: system),
                detach: () => canvas.UnregisterIResponsive(responsive: valid),
                marshalToUi: true)
            select sub);

    // Delay None inherits the canvas's current MouseHoverDelay (GH2 exposes no default constant); an explicit
    // delay must be positive and is leased so the prior value restores on detach.
    internal static GrasshopperUiIntent<Subscription> Hover(Option<TimeSpan> delay, Func<MouseHoverSnapshot, Fin<Unit>> handler) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validDelay in delay.Match(
                Some: requested => requested > TimeSpan.Zero
                    ? Fin.Succ(value: Some(requested))
                    : Fin.Fail<Option<TimeSpan>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Hover)), detail: "hover delay must be positive")),
                None: () => Fin.Succ(value: Option<TimeSpan>.None))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hover)), detail: "handler is required"))
            let onHover = (EventHandler<MouseHoverEventArgs>)((_, e) =>
                _ = GrasshopperUi.Handler(valid: () => valid(arg: new MouseHoverSnapshot(ControlPoint: e.ControlPoint, ContentPoint: e.ContentPoint))))
            // Single-element array carries the lease token across attach/detach without a StrongBox allocation.
            let token = new Guid[1]
            from sub in Subscription.Bind(
                attach: () => {
                    _ = validDelay.Iter(value => token[0] = CanvasPropertyLease.Enter(canvas: canvas, slot: nameof(GhCanvas.MouseHoverDelay), value: value, get: static c => c.MouseHoverDelay, set: static (c, leased) => c.MouseHoverDelay = leased));
                    canvas.MouseHover += onHover;
                },
                detach: () => {
                    canvas.MouseHover -= onHover;
                    _ = validDelay.Iter(_unused => CanvasPropertyLease.Exit<TimeSpan>(canvas: canvas, slot: nameof(GhCanvas.MouseHoverDelay), token: token[0], set: static (c, leased) => c.MouseHoverDelay = leased));
                },
                marshalToUi: true)
            select sub);

    internal static GrasshopperUiIntent<Subscription> ContextMenu(Func<ContextMenuSnapshot, Fin<Unit>> handler) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ContextMenu)), detail: "handler is required"))
            let onPopulate = (EventHandler<PopulateContextMenuEventArgs>)((_, e) =>
                _ = GrasshopperUi.Handler(valid: () => valid(arg: new ContextMenuSnapshot(Menu: e.Menu, MouseEvent: e.MouseEvent))))
            from sub in Subscription.Bind(
                attach: () => canvas.PopulateContextMenu += onPopulate,
                detach: () => canvas.PopulateContextMenu -= onPopulate,
                marshalToUi: true)
            select sub);

    internal static GrasshopperUiIntent<InteractionSnapshot> SnapshotNow() =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from snap in Op.Of(name: nameof(SnapshotNow)).Attempt(
                body: () => new InteractionSnapshot(
                    InteractionCount: toSeq(canvas.Interactions).Count,
                    ResponsiveCount: toSeq(canvas.Responsives).Count,
                    HasFocus: canvas.FocusObject is not null,
                    FocusNomen: UiRail.FocusNomenOf(canvas: canvas)),
                what: "FlexControl interaction snapshot")
            select snap);
}

internal sealed class OwnedSubscription<TKey> {
    private readonly Atom<HashMap<TKey, Guid>> owners = Atom(value: HashMap<TKey, Guid>());

    internal Option<Guid> Owner(TKey key) => owners.Value.Find(key: key);

    internal void Clear(TKey key) => _ = owners.Swap(map => map.Remove(key));

    internal void ClearAll() => _ = owners.Swap(_ => HashMap<TKey, Guid>());

    internal Fin<Subscription> Bind(TKey key, Action attach, Action<TKey> detach) {
        Guid token = Guid.NewGuid();
        return Subscription.Bind(
            // Register the token only after attach() commits so a throwing attach never orphans an ownership slot
            // whose detach gate would then leak.
            attach: () => {
                attach();
                _ = owners.Swap(map => map.SetItem(key, token));
            },
            detach: () => _ = owners.Value.Find(key: key)
                    .Filter(held => held == token)
                    .Iter(_unused => {
                        _ = owners.Swap(map => map.Remove(key));
                        detach(obj: key);
                    }),
            marshalToUi: true,
            teardown: SubscriptionTeardown.TokenGated);
    }
}

// Per-canvas property ownership keyed by RuntimeHelpers.GetHashCode(canvas): the identity hash is stable
// for the canvas's lifetime and avoids retaining the live canvas from the static atom.
internal static class CanvasPropertyLease {
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct Entry<T>(Guid Token, T Value);

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct State<T>(T Baseline, Seq<Entry<T>> Entries);

    private static readonly Atom<HashMap<int, object>> Stacks = Atom(value: HashMap<int, object>());

    internal static Guid Enter<T>(GhCanvas canvas, string slot, T value, Func<GhCanvas, T> get, Action<GhCanvas, T> set) {
        Guid token = Guid.NewGuid();
        int key = Key(canvas: canvas, slot: slot);
        _ = Stacks.Swap(map => map.AddOrUpdate(
            key: key,
            Some: state => state is State<T> typed ? typed with { Entries = typed.Entries + new Entry<T>(Token: token, Value: value) } : new State<T>(Baseline: get(arg: canvas), Entries: Seq(new Entry<T>(Token: token, Value: value))),
            None: () => new State<T>(Baseline: get(arg: canvas), Entries: Seq(new Entry<T>(Token: token, Value: value)))));
        set(arg1: canvas, arg2: value);
        return token;
    }

    internal static bool IsTop<T>(GhCanvas canvas, string slot, Guid token, T value) {
        int key = Key(canvas: canvas, slot: slot);
        return Stacks.Value.Find(key: key)
            .Map(state => state is State<T> typed
                && !typed.Entries.IsEmpty
                && typed.Entries.Last().Token == token
                && EqualityComparer<T>.Default.Equals(x: typed.Entries.Last().Value, y: value))
            .IfNone(noneValue: false);
    }

    // BOUNDARY ADAPTER — token removal restores the top remaining value even when disposals are non-LIFO.
    // A foreign-T slot (hash collision on a differently-typed lease) pattern-matches None and is left untouched
    // rather than InvalidCastException-ing on a bare (State<T>) cast.
    internal static void Exit<T>(GhCanvas canvas, string slot, Guid token, Action<GhCanvas, T> set) {
        int key = Key(canvas: canvas, slot: slot);
        Option<T> restore = Option<T>.None;
        _ = Stacks.Swap(map => map.Find(key)
            .Bind(state => state is State<T> typed ? Some((Typed: typed, Kept: typed.Entries.Filter(entry => entry.Token != token))) : Option<(State<T> Typed, Seq<Entry<T>> Kept)>.None)
            .Match(
                Some: pair => {
                    restore = Some(pair.Kept.IsEmpty ? pair.Typed.Baseline : pair.Kept.Last().Value);
                    return pair.Kept.IsEmpty ? map.Remove(key) : map.SetItem(key, pair.Typed with { Entries = pair.Kept });
                },
                None: () => map));
        _ = restore.Iter(value => set(arg1: canvas, arg2: value));
    }

    private static int Key(GhCanvas canvas, string slot) => HashCode.Combine(RuntimeHelpers.GetHashCode(o: canvas), slot);
}
