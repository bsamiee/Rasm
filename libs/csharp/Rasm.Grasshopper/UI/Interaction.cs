using Eto.Forms;
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
[Union]
public partial record TooltipPainter {
    private TooltipPainter() { }
    public sealed record ShortcutKeysCase(string Prefix, Keys Keys, string Suffix) : TooltipPainter;
    public sealed record ShortcutCharCase(string Prefix, char Key, string Suffix) : TooltipPainter;
    public sealed record TextAndIconCase(Seq<object> Elements) : TooltipPainter;
    public sealed record CustomCase(Action<EtoContext, Eto.Drawing.Rectangle> Paint, Size PaintingSize) : TooltipPainter;

    public static TooltipPainter Shortcut(string prefix, Keys keys, string suffix) => new ShortcutKeysCase(Prefix: prefix, Keys: keys, Suffix: suffix);
    public static TooltipPainter Shortcut(string prefix, char key, string suffix) => new ShortcutCharCase(Prefix: prefix, Key: key, Suffix: suffix);
    public static TooltipPainter TextAndIcon(params object[] elements) => new TextAndIconCase(Elements: toSeq(elements));
    public static TooltipPainter Custom(Action<EtoContext, Eto.Drawing.Rectangle> paint, Size paintingSize) => new CustomCase(Paint: paint, PaintingSize: paintingSize);

    // GH2 CreateShortcutPainter / CreateTextAndIconPainter both return (Action, Size) — deconstruct
    // once at boundary so Show() can pass painter + paintingSize as separate args.
    internal (Action<EtoContext, Eto.Drawing.Rectangle> Paint, Size Size) Resolve() =>
        Switch(
            shortcutKeysCase: static s => GhTooltip.CreateShortcutPainter(prefix: s.Prefix, keys: s.Keys, suffix: s.Suffix),
            shortcutCharCase: static s => GhTooltip.CreateShortcutPainter(prefix: s.Prefix, key: s.Key, suffix: s.Suffix),
            textAndIconCase: static t => GhTooltip.CreateTextAndIconPainter(elements: [.. t.Elements]),
            customCase: static c => (c.Paint, c.PaintingSize));
}

public abstract record TooltipRequest<T> : GhUiRequest<T> {
    public sealed record Show(IIcon Icon, string Caption, string Message, bool Warnings = false, bool Errors = false) : TooltipRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            Tooltip.ShowPlain(icon: Icon, caption: Caption, message: Message, warnings: Warnings, errors: Errors).Run(scope: scope);
    }
    public sealed record ShowItems(IIcon Icon, string Caption, string Message, GhLazyStrings Items, bool Warnings = false, bool Errors = false) : TooltipRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            Tooltip.ShowItems(icon: Icon, caption: Caption, message: Message, items: Items, warnings: Warnings, errors: Errors).Run(scope: scope);
    }
    public sealed record ShowPanes(IIcon Icon, string Caption, string Message, Seq<GhLazyStrings> Panes, bool Warnings = false, bool Errors = false) : TooltipRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            Tooltip.ShowPanes(icon: Icon, caption: Caption, message: Message, panes: Panes, warnings: Warnings, errors: Errors).Run(scope: scope);
    }
    public sealed record ShowPainter(IIcon Icon, string Caption, string Message, TooltipPainter Painter, bool Warnings = false, bool Errors = false) : TooltipRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            Tooltip.ShowPainter(icon: Icon, caption: Caption, message: Message, painter: Painter, warnings: Warnings, errors: Errors).Run(scope: scope);
    }
    public sealed record Hide : TooltipRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Tooltip.HideNow().Run(scope: scope);
    }
    public sealed record Invalidate : TooltipRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Tooltip.InvalidateNow().Run(scope: scope);
    }
    public sealed record Status : TooltipRequest<TooltipSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<TooltipSnapshot> Apply(GrasshopperUi.Scope scope) => Tooltip.SnapshotNow().Run(scope: scope);
    }
}

public abstract record FloatingButtonRequest<T> : GhUiRequest<T> {
    public sealed record Add(
        FloatingPosition Position, string Name, string Info, IIcon Icon,
        Option<Color> Colour = default,
        Option<FloatingButtonHandler> Click = default,
        Option<FloatingButtonHandler> MouseDown = default,
        Option<FloatingButtonHandler> MouseUp = default) : FloatingButtonRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            FloatingButton.Add(position: Position, name: Name, info: Info, icon: Icon,
                colour: Colour, click: Click, mouseDown: MouseDown, mouseUp: MouseUp).Run(scope: scope);
    }
    public sealed record AddAnchored(
        PointF Anchor, string Name, string Info, IIcon Icon,
        Option<Color> Colour = default,
        Option<FloatingButtonHandler> Click = default,
        Option<FloatingButtonHandler> MouseDown = default,
        Option<FloatingButtonHandler> MouseUp = default) : FloatingButtonRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            FloatingButton.AddAnchored(anchor: Anchor, name: Name, info: Info, icon: Icon,
                colour: Colour, click: Click, mouseDown: MouseDown, mouseUp: MouseUp).Run(scope: scope);
    }
    // MakeNumeric is a post-creation instance method on FloatingButton, but FloatingButtonCollection.Add
    // returns void. The attach action therefore Adds, then FindByName-retrieves the just-added button,
    // then upgrades via MakeNumeric — all three steps in one atomic install so Dispose sees a single
    // Close(name) detach regardless of how far through the upgrade chain we reached.
    public sealed record AddNumeric(
        FloatingPosition Position, string Name, string Info, IIcon Icon,
        GhUiNumber Value, string ValueKey,
        Option<Color> Colour = default) : FloatingButtonRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            FloatingButton.AddNumeric(position: Position, name: Name, info: Info, icon: Icon,
                value: Value, valueKey: ValueKey, colour: Colour).Run(scope: scope);
    }
    public sealed record ModifyInfo(string Name, string Info) : FloatingButtonRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => FloatingButton.ModifyInfo(name: Name, info: Info).Run(scope: scope);
    }
    public sealed record ModifyIcon(string Name, IIcon Icon) : FloatingButtonRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => FloatingButton.ModifyIcon(name: Name, icon: Icon).Run(scope: scope);
    }
    public sealed record ModifyColour(string Name, Color Colour) : FloatingButtonRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => FloatingButton.ModifyColour(name: Name, colour: Colour).Run(scope: scope);
    }
    public sealed record ModifyAnchor(string Name, PointF Anchor, bool Immediate = false) : FloatingButtonRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => FloatingButton.ModifyAnchor(name: Name, anchor: Anchor, immediate: Immediate).Run(scope: scope);
    }
    public sealed record ShowNamed(Seq<string> Names) : FloatingButtonRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => FloatingButton.SetVisible(names: Names, visible: true).Run(scope: scope);
    }
    public sealed record HideNamed(Seq<string> Names) : FloatingButtonRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => FloatingButton.SetVisible(names: Names, visible: false).Run(scope: scope);
    }
    public sealed record CloseNamed(Seq<string> Names) : FloatingButtonRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => FloatingButton.Close(names: Names).Run(scope: scope);
    }
    public sealed record CloseAll : FloatingButtonRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => FloatingButton.CloseAll().Run(scope: scope);
    }
    public sealed record FindByName(string Name) : FloatingButtonRequest<Option<FloatingButtonInfo>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Option<FloatingButtonInfo>> Apply(GrasshopperUi.Scope scope) => FloatingButton.FindByName(name: Name).Run(scope: scope);
    }
    public sealed record FindByPoint(PointF ControlPoint) : FloatingButtonRequest<Option<FloatingButtonInfo>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Option<FloatingButtonInfo>> Apply(GrasshopperUi.Scope scope) => FloatingButton.FindByPoint(point: ControlPoint).Run(scope: scope);
    }
    public sealed record Status : FloatingButtonRequest<FloatingButtonSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<FloatingButtonSnapshot> Apply(GrasshopperUi.Scope scope) => FloatingButton.SnapshotNow().Run(scope: scope);
    }
}

public abstract record InteractionRequest<T> : GhUiRequest<T> {
    public sealed record Push(IInteraction Target) : InteractionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Interaction.Push(target: Target).Run(scope: scope);
    }
    public sealed record Register(IResponsive Responsive, CoordinateSystem System) : InteractionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            Interaction.Register(responsive: Responsive, system: System).Run(scope: scope);
    }
    // Hover pump: configure FlexControl.MouseHoverDelay (TimeSpan.Zero disables) and subscribe to
    // MouseHover. Disposing restores the prior delay and detaches the handler.
    public sealed record Hover(TimeSpan Delay, Func<MouseHoverSnapshot, Fin<Unit>> Handler) : InteractionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            Interaction.Hover(delay: Delay, handler: Handler).Run(scope: scope);
    }
    // Right-click hook: PopulateContextMenu fires when GH2 builds the context menu. Handler receives
    // the menu and the originating MouseEvent and may append Eto MenuItems via menu.Items.
    public sealed record ContextMenu(Func<ContextMenuSnapshot, Fin<Unit>> Handler) : InteractionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) =>
            Interaction.ContextMenu(handler: Handler).Run(scope: scope);
    }
    public sealed record Status : InteractionRequest<InteractionSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<InteractionSnapshot> Apply(GrasshopperUi.Scope scope) => Interaction.SnapshotNow().Run(scope: scope);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct TooltipSnapshot(bool Visible, Option<Guid> OwnerToken);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonSnapshot(
    int Count,
    int NormalCount,
    int HiddenCount,
    int VisibleCount,
    Seq<string> Names);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonInfo(
    string Name,
    string Info,
    FloatingPosition Position,
    FloatingState State,
    bool Enabled,
    bool HasFocus,
    Color Colour,
    Option<PointF> Anchor);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InteractionSnapshot(
    int InteractionCount,
    int ResponsiveCount,
    bool HasFocus,
    Option<string> FocusNomen);

[StructLayout(LayoutKind.Auto)]
public readonly record struct MouseHoverSnapshot(PointF ControlPoint, PointF ContentPoint);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ContextMenuSnapshot(ContextMenu Menu, MouseEventArgs MouseEvent);

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class Tooltip {
    // Token gate prevents a stale Subscription's Dispose from hiding a later Show that supplanted it.
    private static readonly Atom<Option<Guid>> Owner = Atom(Option<Guid>.None);

    internal static GrasshopperUiIntent<Subscription> ShowPlain(IIcon icon, string caption, string message, bool warnings, bool errors) =>
        Bind(invoke: () => GhTooltip.Show(icon: icon, caption: caption, message: message, warnings: warnings, errors: errors));

    // Singular LazyStrings overload chosen by positional type match (named 'items:' would resolve
    // to the LazyStrings[] overload — they share the slot name).
    internal static GrasshopperUiIntent<Subscription> ShowItems(IIcon icon, string caption, string message, GhLazyStrings items, bool warnings, bool errors) =>
        Bind(invoke: () => GhTooltip.Show(icon, caption, message, items, warnings, errors));

    internal static GrasshopperUiIntent<Subscription> ShowPanes(IIcon icon, string caption, string message, Seq<GhLazyStrings> panes, bool warnings, bool errors) =>
        Bind(invoke: () => GhTooltip.Show(icon: icon, caption: caption, message: message, items: [.. panes], warnings: warnings, errors: errors));

    internal static GrasshopperUiIntent<Subscription> ShowPainter(IIcon icon, string caption, string message, TooltipPainter painter, bool warnings, bool errors) {
        (Action<EtoContext, Eto.Drawing.Rectangle> paint, Size size) = painter.Resolve();
        return Bind(invoke: () => GhTooltip.Show(icon: icon, caption: caption, message: message, painter: paint, paintingSize: size, warnings: warnings, errors: errors));
    }

    internal static GrasshopperUiIntent<Unit> HideNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(HideNow)).Attempt(body: HideNative, what: "Tooltip.Frame.Hide"));

    internal static GrasshopperUiIntent<Unit> InvalidateNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(InvalidateNow)).Attempt(body: GhTooltip.Invalidate, what: "Tooltip.Frame.Invalidate"));

    internal static GrasshopperUiIntent<TooltipSnapshot> SnapshotNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(SnapshotNow)).Attempt(body: SnapshotNative, what: "Tooltip.Frame.Visible"));

    private static void HideNative() {
        _ = Owner.Swap(static _ => Option<Guid>.None);
        GhTooltip.Hide();
    }

    private static TooltipSnapshot SnapshotNative() =>
        new(Visible: GhTooltip.Visible, OwnerToken: Owner.Value);

    // Token gates Dispose: a stale Subscription only hides if its captured Guid is still the active
    // owner — a subsequent Show overwrites the token, leaving prior Subscriptions as no-ops.
    private static GrasshopperUiIntent<Subscription> Bind(System.Action invoke) =>
        GhUi.Canvas(run: scope => scope.NeedCanvas().Bind(_ => Resource(invoke: invoke)));

    private static Fin<Subscription> Resource(System.Action invoke) {
        Guid token = Guid.NewGuid();
        return Subscription.Bind(
            attach: () => Take(token: token, invoke: invoke),
            detach: () => Release(token: token),
            marshalToUi: true);
    }

    private static void Take(Guid token, System.Action invoke) {
        _ = Owner.Swap(_ => Some(token));
        invoke();
    }

    private static void Release(Guid token) {
        bool owned = Owner.Value is { IsSome: true, Case: Guid held } && held == token;
        _ = owned ? Withdraw() : unit;
    }

    private static Unit Withdraw() {
        _ = Owner.Swap(static _ => Option<Guid>.None);
        _ = Op.Of(name: nameof(Tooltip)).Attempt(body: GhTooltip.Hide, what: "Tooltip.Frame.Hide");
        return unit;
    }
}

internal static class FloatingButton {
    internal static GrasshopperUiIntent<Subscription> Add(
        FloatingPosition position, string name, string info, IIcon icon,
        Option<Color> colour, Option<FloatingButtonHandler> click, Option<FloatingButtonHandler> mouseDown, Option<FloatingButtonHandler> mouseUp) =>
        Install(name: name, attach: canvas => canvas.FloatingButtons.Add(
            position: position, name: name, info: info,
            colour: ColourOf(colour), icon: icon,
            click: click.IfNone(NoOp), mouseDown: mouseDown.IfNone(NoOp), mouseUp: mouseUp.IfNone(NoOp)));

    internal static GrasshopperUiIntent<Subscription> AddAnchored(
        PointF anchor, string name, string info, IIcon icon,
        Option<Color> colour, Option<FloatingButtonHandler> click, Option<FloatingButtonHandler> mouseDown, Option<FloatingButtonHandler> mouseUp) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validAnchor in Op.Of(name: nameof(AddAnchored)).AcceptPoint(value: anchor, detail: "non-finite anchor")
            from sub in Subscription.Bind(
                attach: () => canvas.FloatingButtons.AddAnchored(
                    anchor: validAnchor, name: name, info: info,
                    colour: ColourOf(colour), icon: icon,
                    click: click.IfNone(NoOp), mouseDown: mouseDown.IfNone(NoOp), mouseUp: mouseUp.IfNone(NoOp)),
                detach: () => canvas.FloatingButtons.Close(name),
                marshalToUi: true)
            select sub);

    internal static GrasshopperUiIntent<Subscription> AddNumeric(
        FloatingPosition position, string name, string info, IIcon icon,
        GhUiNumber value, string valueKey, Option<Color> colour) =>
        Install(name: name, attach: canvas => {
            canvas.FloatingButtons.Add(
                position: position, name: name, info: info,
                colour: ColourOf(colour), icon: icon,
                click: NoOp, mouseDown: NoOp, mouseUp: NoOp);
            _ = Optional(canvas.FloatingButtons.FindByName(name: name))
                .IfSome(button => button.MakeNumeric(number: value, valueKey: valueKey));
        });

    internal static GrasshopperUiIntent<Unit> ModifyInfo(string name, string info) =>
        Modify(op: Op.Of(name: nameof(ModifyInfo)), what: "ModifyInfo", apply: canvas => canvas.FloatingButtons.ModifyInfo(name: name, buttonInfo: info));

    internal static GrasshopperUiIntent<Unit> ModifyIcon(string name, IIcon icon) =>
        Modify(op: Op.Of(name: nameof(ModifyIcon)), what: "ModifyIcon", apply: canvas => canvas.FloatingButtons.ModifyIcon(name: name, icon: icon));

    internal static GrasshopperUiIntent<Unit> ModifyColour(string name, Color colour) =>
        Modify(op: Op.Of(name: nameof(ModifyColour)), what: "ModifyColour", apply: canvas => canvas.FloatingButtons.ModifyColour(name: name, colour: colour));

    internal static GrasshopperUiIntent<Unit> ModifyAnchor(string name, PointF anchor, bool immediate) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Op.Of(name: nameof(ModifyAnchor)).AcceptPoint(value: anchor, detail: "non-finite anchor")
            from _ in Op.Of(name: nameof(ModifyAnchor)).Attempt(
                body: () => canvas.FloatingButtons.ModifyAnchor(name: name, anchor: valid, immediate: immediate),
                what: "ModifyAnchor")
            select unit);

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
                body: () => canvas.FloatingButtons.Close([.. names]),
                what: "FloatingButtonCollection.Close")
            select unit);

    internal static GrasshopperUiIntent<Unit> CloseAll() =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from _ in Op.Of(name: nameof(CloseAll)).Attempt(
                body: canvas.FloatingButtons.CloseAll,
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
            from sub in Subscription.Bind(
                attach: () => attach(canvas),
                detach: () => canvas.FloatingButtons.Close(name),
                marshalToUi: true)
            select sub);

    private static GrasshopperUiIntent<Unit> Modify(Op op, string what, Action<GhCanvas> apply) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from _ in op.Attempt(body: () => apply(canvas), what: what)
            select unit);

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
            let priorDelay = canvas.MouseHoverDelay
            let onHover = (EventHandler<MouseHoverEventArgs>)((_, e) =>
                _ = GrasshopperUi.Protect(valid: () => valid(arg: new MouseHoverSnapshot(ControlPoint: e.ControlPoint, ContentPoint: e.ContentPoint))))
            from sub in Subscription.Bind(
                attach: () => { canvas.MouseHoverDelay = delay; canvas.MouseHover += onHover; },
                detach: () => { canvas.MouseHover -= onHover; canvas.MouseHoverDelay = priorDelay; },
                marshalToUi: true)
            select sub);

    internal static GrasshopperUiIntent<Subscription> ContextMenu(Func<ContextMenuSnapshot, Fin<Unit>> handler) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ContextMenu)), detail: "handler is required"))
            let onPopulate = (EventHandler<PopulateContextMenuEventArgs>)((_, e) =>
                _ = GrasshopperUi.Protect(valid: () => valid(arg: new ContextMenuSnapshot(Menu: e.Menu, MouseEvent: e.MouseEvent))))
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
                    FocusNomen: Optional(canvas.FocusObject as IInteraction).Map(i => i.Nomen.Name)),
                what: "FlexControl interaction snapshot")
            select snap);
}
