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

    public static TooltipOp Show(IIcon icon, string caption, string message, bool warnings = false, bool errors = false) =>
        new ShowCase(Icon: icon, Caption: caption, Message: message, Body: TooltipBody.Plain, Warnings: warnings, Errors: errors);
    public static TooltipOp ShowItems(IIcon icon, string caption, string message, GhLazyStrings items, bool warnings = false, bool errors = false) =>
        new ShowCase(Icon: icon, Caption: caption, Message: message, Body: TooltipBody.FromItems(items: items), Warnings: warnings, Errors: errors);
    public static TooltipOp ShowPanes(IIcon icon, string caption, string message, Seq<GhLazyStrings> panes, bool warnings = false, bool errors = false) =>
        new ShowCase(Icon: icon, Caption: caption, Message: message, Body: TooltipBody.FromPanes(panes: panes), Warnings: warnings, Errors: errors);
    public static TooltipOp ShowPainter(IIcon icon, string caption, string message, TooltipPainter painter, bool warnings = false, bool errors = false) =>
        new ShowCase(Icon: icon, Caption: caption, Message: message, Body: TooltipBody.FromPainter(painter: painter), Warnings: warnings, Errors: errors);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        showCase: static _ => GrasshopperUiPolicy.Canvas(),
        hideCase: static _ => GrasshopperUiPolicy.Read,
        invalidateCase: static _ => GrasshopperUiPolicy.Read,
        statusCase: static _ => GrasshopperUiPolicy.Read,
        layoutCase: static _ => GrasshopperUiPolicy.Read);
}

[SkipUnionOps]
[Union]
public partial record CanvasChromeOp {
    private CanvasChromeOp() { }
    public sealed record TooltipCase(TooltipOp Op) : CanvasChromeOp;
    public sealed record FloatingButtonCase(FloatingButtonOp Op) : CanvasChromeOp;
    public sealed record InteractionCase(InteractionOp Op) : CanvasChromeOp;

    public static CanvasChromeOp Tooltip(TooltipOp op) => new TooltipCase(Op: op);
    public static CanvasChromeOp FloatingButton(FloatingButtonOp op) => new FloatingButtonCase(Op: op);
    public static CanvasChromeOp Interaction(InteractionOp op) => new InteractionCase(Op: op);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        tooltipCase: static t => t.Op.UiPolicy,
        floatingButtonCase: static _ => GrasshopperUiPolicy.Canvas(),
        interactionCase: static _ => GrasshopperUiPolicy.Canvas());
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

    internal static Fin<CanvasChromeResult> Of<T>(T value) =>
        value switch {
            Subscription subscription => Fin.Succ<CanvasChromeResult>(new SubscriptionCase(Subscription: subscription)),
            TooltipSnapshot snapshot => Fin.Succ<CanvasChromeResult>(new TooltipStatusCase(Snapshot: snapshot)),
            TooltipLayoutSnapshot snapshot => Fin.Succ<CanvasChromeResult>(new TooltipLayoutCase(Snapshot: snapshot)),
            FloatingButtonSnapshot snapshot => Fin.Succ<CanvasChromeResult>(new FloatingButtonStatusCase(Snapshot: snapshot)),
            Option<FloatingButtonInfo> info => Fin.Succ<CanvasChromeResult>(new FloatingButtonFoundCase(Info: info)),
            InteractionSnapshot snapshot => Fin.Succ<CanvasChromeResult>(new InteractionStatusCase(Snapshot: snapshot)),
            _ => Fin.Fail<CanvasChromeResult>(error: UiFault.InvalidInput(
                op: Op.Of(name: nameof(CanvasChromeResult)),
                detail: $"unsupported chrome payload '{typeof(T).Name}'")),
        };
}

public readonly record struct FloatingButtonHandlers(
    Option<FloatingButtonHandler> Click = default,
    Option<FloatingButtonHandler> MouseDown = default,
    Option<FloatingButtonHandler> MouseUp = default);

[SkipUnionOps]
[Union]
public partial record FloatingButtonOp {
    private FloatingButtonOp() { }
    public sealed record AddCase(FloatingPosition Position, string Name, string Info, IIcon Icon, Option<Color> Colour = default, FloatingButtonHandlers Handlers = default) : FloatingButtonOp;
    public sealed record AddAnchoredCase(PointF Anchor, string Name, string Info, IIcon Icon, Option<Color> Colour = default, FloatingButtonHandlers Handlers = default) : FloatingButtonOp;
    // FloatingButtonCollection.Add returns void; AddNumeric attaches Add + FindByName + MakeNumeric
    // atomically and detaches via Close(name) regardless of upgrade-chain progress.
    public sealed record AddNumericCase(FloatingPosition Position, string Name, string Info, IIcon Icon, GhUiNumber Value, string ValueKey, Option<Color> Colour = default) : FloatingButtonOp;
    public sealed record ModifyCase(string Name, Option<string> Info = default, Option<IIcon> Icon = default, Option<Color> Colour = default, Option<(PointF Point, bool Immediate)> Anchor = default) : FloatingButtonOp;
    public sealed record ShowNamedCase(Seq<string> Names) : FloatingButtonOp;
    public sealed record HideNamedCase(Seq<string> Names) : FloatingButtonOp;
    public sealed record CloseNamedCase(Seq<string> Names) : FloatingButtonOp;
    public sealed record CloseAllCase : FloatingButtonOp;
    public sealed record FindByNameCase(string Name) : FloatingButtonOp;
    public sealed record FindByPointCase(PointF ControlPoint) : FloatingButtonOp;
    public sealed record StatusCase : FloatingButtonOp;
}

[SkipUnionOps]
[Union]
public partial record InteractionOp {
    private InteractionOp() { }
    public sealed record PushCase(IInteraction Target) : InteractionOp;
    public sealed record RegisterCase(IResponsive Responsive, CoordinateSystem System) : InteractionOp;
    // Hover/ContextMenu: GH2 emits MouseHover / PopulateContextMenu — handler may mutate the
    // menu.Items collection or read the hover point.
    public sealed record HoverCase(TimeSpan Delay, Func<MouseHoverSnapshot, Fin<Unit>> Handler) : InteractionOp;
    public sealed record ContextMenuCase(Func<ContextMenuSnapshot, Fin<Unit>> Handler) : InteractionOp;
    public sealed record StatusCase : InteractionOp;
    public sealed record ModifierWatchCase(Func<InputModifierSnapshot, Fin<Unit>> Handler) : InteractionOp;
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct TooltipSnapshot(bool Visible, Option<Guid> OwnerToken);

[StructLayout(LayoutKind.Auto)]
public readonly record struct TooltipLayoutSnapshot(int MinimumWidth, int MaximumWidth, int MaximumHeight, int Padding, int DoublePadding, int IconSize);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonSnapshot(int Count, int NormalCount, int HiddenCount, int VisibleCount, Seq<string> Names);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonInfo(string Name, string Info, FloatingPosition Position, FloatingState State, bool Enabled, bool HasFocus, Color Colour, Option<PointF> Anchor);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InteractionSnapshot(int InteractionCount, int ResponsiveCount, bool HasFocus, Option<string> FocusNomen);

[StructLayout(LayoutKind.Auto)]
public readonly record struct MouseHoverSnapshot(PointF ControlPoint, PointF ContentPoint);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ContextMenuSnapshot(ContextMenu Menu, MouseEventArgs MouseEvent);

// --- [BOUNDARY] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static class TooltipRail {
    private static readonly Op RailOp = Op.Of(name: nameof(TooltipRail));

    internal static GrasshopperUiIntent<TooltipLayoutSnapshot> Layout() =>
        GhUi.Read(run: _ => RailOp.Attempt(body: ReadLayout, what: "Tooltip.Layout constants"));

    private static TooltipLayoutSnapshot ReadLayout() {
        Type layoutType = typeof(GhTooltip).Assembly.GetType(name: "Grasshopper2.UI.Tooltip.Layout", throwOnError: true)!;
        return new TooltipLayoutSnapshot(
            MinimumWidth: ReadInt(type: layoutType, name: "MinimumWidth"),
            MaximumWidth: ReadInt(type: layoutType, name: "MaximumWidth"),
            MaximumHeight: ReadInt(type: layoutType, name: "MaximumHeight"),
            Padding: ReadInt(type: layoutType, name: "Padding"),
            DoublePadding: ReadInt(type: layoutType, name: "DoublePadding"),
            IconSize: ReadInt(type: layoutType, name: "IconSize"));
    }

    private static int ReadInt(Type type, string name) =>
        Convert.ToInt32(value: type.GetField(name: name, bindingAttr: BindingFlags.Public | BindingFlags.Static)!.GetValue(obj: null)!, provider: System.Globalization.CultureInfo.InvariantCulture);
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class CanvasChrome {
    internal static GrasshopperUiIntent<CanvasChromeResult> Dispatch(CanvasChromeOp op) =>
        op.Switch(
            tooltipCase: static t => Tooltip.Dispatch(op: t.Op),
            floatingButtonCase: static f => FloatingButton.Dispatch(op: f.Op),
            interactionCase: static i => Interaction.Dispatch(op: i.Op));
}

internal static class Tooltip {
    // Token gate prevents a stale Subscription's Dispose from hiding a later Show that supplanted it.
    private static readonly OwnedSubscription<int> Owner = new();

    internal static GrasshopperUiIntent<CanvasChromeResult> Dispatch(TooltipOp op) =>
        op.Switch(
            showCase: static s => Bind(invoke: () => ShowNative(show: s)).BindFin(CanvasChromeResult.Of),
            hideCase: static _ => HideNow().Map(static _ => CanvasChromeResult.UnitInstance),
            invalidateCase: static _ => InvalidateNow().Map(static _ => CanvasChromeResult.UnitInstance),
            statusCase: static _ => SnapshotNow().BindFin(CanvasChromeResult.Of),
            layoutCase: static _ => TooltipRail.Layout().BindFin(CanvasChromeResult.Of));

    internal static GrasshopperUiIntent<Unit> HideNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(HideNow)).Attempt(body: HideNative, what: "Tooltip.Frame.Hide"));

    internal static GrasshopperUiIntent<Unit> InvalidateNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(InvalidateNow)).Attempt(body: GhTooltip.Invalidate, what: "Tooltip.Frame.Invalidate"));

    internal static GrasshopperUiIntent<TooltipSnapshot> SnapshotNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(SnapshotNow)).Attempt(body: SnapshotNative, what: "Tooltip.Frame.Visible"));

    private static void HideNative() {
        Owner.Clear(key: 0);
        GhTooltip.Hide();
    }

    private static TooltipSnapshot SnapshotNative() =>
        new(Visible: GhTooltip.Visible, OwnerToken: Owner.Owner(key: 0));

    // Token gates Dispose: a stale Subscription only hides if its captured Guid is still the active
    // owner — a subsequent Show overwrites the token, leaving prior Subscriptions as no-ops.
    private static GrasshopperUiIntent<Subscription> Bind(Action invoke) =>
        GhUi.Canvas(run: scope => scope.NeedCanvas().Bind(_ => Resource(invoke: invoke)));

    private static Fin<Subscription> Resource(Action invoke) =>
        Owner.Bind(key: 0, attach: invoke, detach: _ => GhTooltip.Hide());

    private static void ShowNative(TooltipOp.ShowCase show) =>
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

internal static class FloatingButton {
    // Token gate per name: stale Subscription.Dispose only Close(name) when its Guid still owns the slot.
    private static readonly OwnedSubscription<string> Owners = new();

    internal static GrasshopperUiIntent<CanvasChromeResult> Dispatch(FloatingButtonOp op) =>
        op.Switch(
            addCase: static a => Add(a.Position, a.Name, a.Info, a.Icon, a.Colour, a.Handlers).BindFin(CanvasChromeResult.Of),
            addAnchoredCase: static a => AddAnchored(a.Anchor, a.Name, a.Info, a.Icon, a.Colour, a.Handlers).BindFin(CanvasChromeResult.Of),
            addNumericCase: static a => AddNumeric(a.Position, a.Name, a.Info, a.Icon, a.Value, a.ValueKey, a.Colour).BindFin(CanvasChromeResult.Of),
            modifyCase: static m => Modify(m.Name, m.Info, m.Icon, m.Colour, m.Anchor).Map(AsUnit),
            showNamedCase: static s => SetVisible(s.Names, visible: true).Map(AsUnit),
            hideNamedCase: static h => SetVisible(h.Names, visible: false).Map(AsUnit),
            closeNamedCase: static c => Close(c.Names).Map(AsUnit),
            closeAllCase: static _ => CloseAll().Map(AsUnit),
            findByNameCase: static f => FindByName(f.Name).BindFin(CanvasChromeResult.Of),
            findByPointCase: static f => FindByPoint(f.ControlPoint).BindFin(CanvasChromeResult.Of),
            statusCase: static _ => SnapshotNow().BindFin(CanvasChromeResult.Of));

    private static CanvasChromeResult AsUnit(Unit _) => CanvasChromeResult.UnitInstance;

    internal static GrasshopperUiIntent<Subscription> Add(
        FloatingPosition position, string name, string info, IIcon icon,
        Option<Color> colour, FloatingButtonHandlers handlers) =>
        Install(name: name, attach: canvas => canvas.FloatingButtons.Add(
            position: position, name: name, info: info,
            colour: ColourOf(colour), icon: icon,
            click: handlers.Click.IfNone(NoOp), mouseDown: handlers.MouseDown.IfNone(NoOp), mouseUp: handlers.MouseUp.IfNone(NoOp)));

    internal static GrasshopperUiIntent<Subscription> AddAnchored(
        PointF anchor, string name, string info, IIcon icon,
        Option<Color> colour, FloatingButtonHandlers handlers) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validAnchor in Op.Of(name: nameof(AddAnchored)).AcceptPoint(value: anchor, detail: "non-finite anchor")
            from sub in Install(
                name: name,
                attach: c => c.FloatingButtons.AddAnchored(
                    anchor: validAnchor, name: name, info: info,
                    colour: ColourOf(colour), icon: icon,
                    click: handlers.Click.IfNone(NoOp), mouseDown: handlers.MouseDown.IfNone(NoOp), mouseUp: handlers.MouseUp.IfNone(NoOp))).Run(scope: scope)
            select sub);

    // The plan-flagged silent fail: FindByName returning null after Add() leaves the button registered
    // but un-numerified. The throw propagates through Subscription.Bind's Try.lift wrapper and surfaces
    // as a typed UiFault — caller sees explicit registration failure instead of a half-built control.
    internal static GrasshopperUiIntent<Subscription> AddNumeric(
        FloatingPosition position, string name, string info, IIcon icon,
        GhUiNumber value, string valueKey, Option<Color> colour) =>
        GhUi.Canvas(run: scope =>
            scope.NeedCanvas().Bind(canvas =>
                Op.Of(name: nameof(AddNumeric)).Attempt(
                    body: () => {
                        canvas.FloatingButtons.Add(
                            position: position, name: name, info: info,
                            colour: ColourOf(colour), icon: icon,
                            click: NoOp, mouseDown: NoOp, mouseUp: NoOp);
                        return unit;
                    },
                    what: "FloatingButton.Add")
                .Bind(_ => Optional(canvas.FloatingButtons.FindByName(name: name)).Match(
                    Some: Fin.Succ,
                    None: () => {
                        canvas.FloatingButtons.Close(name);
                        return Fin.Fail<NativeFloatingButton>(error: UiFault.MutationRejected(
                            op: Op.Of(name: nameof(AddNumeric)),
                            detail: $"FloatingButton '{name}' was not registered after Add()."));
                    }))
                .Bind(registered => Op.Of(name: nameof(AddNumeric)).Attempt(
                    body: () => { registered.MakeNumeric(number: value, valueKey: valueKey); return unit; },
                    what: "MakeNumeric"))
                .Bind(_ => Resource(name: name, canvas: canvas, attach: () => { }))));

    internal static GrasshopperUiIntent<Unit> Modify(
        string name,
        Option<string> info = default,
        Option<IIcon> icon = default,
        Option<Color> colour = default,
        Option<(PointF Point, bool Immediate)> anchor = default) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validAnchor in ValidateAnchor(anchor: anchor)
            from _ in Op.Of(name: nameof(Modify)).Attempt(
                body: () => {
                    _ = info.Iter(value => canvas.FloatingButtons.ModifyInfo(name: name, buttonInfo: value));
                    _ = icon.Iter(value => canvas.FloatingButtons.ModifyIcon(name: name, icon: value));
                    _ = colour.Iter(value => canvas.FloatingButtons.ModifyColour(name: name, colour: value));
                    _ = validAnchor.Iter(av => canvas.FloatingButtons.ModifyAnchor(name: name, anchor: av.Point, immediate: av.Immediate));
                },
                what: "ModifyButton")
            select unit);

    private static Fin<Option<(PointF Point, bool Immediate)>> ValidateAnchor(Option<(PointF Point, bool Immediate)> anchor) =>
        anchor.Match(
            Some: a => Op.Of(name: nameof(Modify)).AcceptPoint(value: a.Point, detail: "non-finite anchor").Map(p => Some((Point: p, a.Immediate))),
            None: () => Fin.Succ(Option<(PointF Point, bool Immediate)>.None));

    internal static GrasshopperUiIntent<Unit> SetVisible(Seq<string> names, bool visible) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from _ in Op.Of(name: nameof(SetVisible)).Attempt(
                body: () => { _ = visible ? VisibleOn(canvas, names) : VisibleOff(canvas, names); },
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

    private static GrasshopperUiIntent<Subscription> Install(string name, Action<GhCanvas> attach) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from sub in Resource(name: name, canvas: canvas, attach: () => attach(canvas))
            select sub);

    private static Fin<Subscription> Resource(string name, GhCanvas canvas, Action attach) =>
        Owners.Bind(key: name, attach: attach, detach: target => canvas.FloatingButtons.Close(target));

    private static Color? ColourOf(Option<Color> colour) =>
        colour is { IsSome: true, Case: Color picked } ? picked : null;

    private static FloatingButtonInfo InfoOf(NativeFloatingButton button) =>
        new(Name: button.Name, Info: button.Info,
            Position: button.Position, State: button.State,
            Enabled: button.Enabled, HasFocus: button.HasFocus,
            Colour: button.Colour,
            Anchor: button.Position == FloatingPosition.Anchored ? Some(button.Anchor) : Option<PointF>.None);

    private static Unit VisibleOn(GhCanvas canvas, Seq<string> names) { canvas.FloatingButtons.Show([.. names]); return unit; }
    private static Unit VisibleOff(GhCanvas canvas, Seq<string> names) { canvas.FloatingButtons.Hide([.. names]); return unit; }

    private static Response NoOp(NativeFloatingButton _, MouseEventArgs __) => Response.Ignored;
}

internal static class Interaction {
    internal static GrasshopperUiIntent<CanvasChromeResult> Dispatch(InteractionOp op) =>
        op.Switch(
            pushCase: static p => Push(p.Target).BindFin(CanvasChromeResult.Of),
            registerCase: static r => Register(r.Responsive, r.System).BindFin(CanvasChromeResult.Of),
            hoverCase: static h => Hover(h.Delay, h.Handler).BindFin(CanvasChromeResult.Of),
            contextMenuCase: static c => ContextMenu(c.Handler).BindFin(CanvasChromeResult.Of),
            statusCase: static _ => SnapshotNow().BindFin(CanvasChromeResult.Of),
            modifierWatchCase: static m => GhUi.Event(uiEvent: UiEvent.KeyboardModifiers(handler: m.Handler)).BindFin(CanvasChromeResult.Of));

    internal static GrasshopperUiIntent<Subscription> Push(IInteraction target) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(target).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Push)), detail: "interaction is required"))
            from sub in Subscription.Bind(
                attach: () => canvas.PushInteraction(interaction: valid),
                detach: () => valid.Release(control: canvas),
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

    internal static GrasshopperUiIntent<Subscription> Hover(TimeSpan delay, Func<MouseHoverSnapshot, Fin<Unit>> handler) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hover)), detail: "handler is required"))
            let onHover = (EventHandler<MouseHoverEventArgs>)((_, e) =>
                _ = GrasshopperUi.Handler(valid: () => valid(arg: new MouseHoverSnapshot(ControlPoint: e.ControlPoint, ContentPoint: e.ContentPoint))))
            from sub in Subscription.Bind(
                attach: () => { HoverDelayInstall.Enter(canvas: canvas, delay: delay); canvas.MouseHover += onHover; },
                detach: () => { canvas.MouseHover -= onHover; HoverDelayInstall.Exit(canvas: canvas); },
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
            attach: () => {
                _ = owners.Swap(map => map.SetItem(key, token));
                attach();
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

file static class HoverDelayInstall {
    private static readonly Atom<HashMap<int, Seq<TimeSpan>>> Stacks = Atom(value: HashMap<int, Seq<TimeSpan>>());

    internal static void Enter(GhCanvas canvas, TimeSpan delay) {
        int key = RuntimeHelpers.GetHashCode(canvas);
        TimeSpan prior = canvas.MouseHoverDelay;
        _ = Stacks.Swap(map => map.AddOrUpdate(key, stack => stack + prior, () => Seq(prior)));
        canvas.MouseHoverDelay = delay;
    }

    // BOUNDARY ADAPTER — Atom.Swap captures the prior stack entry for LIFO restore across concurrent subscriptions.
    internal static void Exit(GhCanvas canvas) {
        int key = RuntimeHelpers.GetHashCode(canvas);
        HashMap<int, Seq<TimeSpan>> prior = Stacks.Swap(map =>
            map.Find(key).Match(
                Some: stack => stack.IsEmpty ? map : stack.Init.IsEmpty ? map.Remove(key) : map.SetItem(key, stack.Init),
                None: () => map));
        Seq<TimeSpan> oldStack = prior.Find(key).IfNone(Seq<TimeSpan>());
        canvas.MouseHoverDelay = oldStack.IsEmpty ? canvas.MouseHoverDelay : oldStack.Last();
    }
}
