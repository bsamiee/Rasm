using System.Reflection;
using System.Runtime.CompilerServices;
using AppKit;
using CoreGraphics;
using Eto.Forms;
using Foundation;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using EtoContext = Eto.Drawing.Context;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhDocument = Grasshopper2.Doc.Document;
using GhLazyStrings = Grasshopper2.UI.LazyStrings;
using GhResponse = Grasshopper2.UI.Flex.Response;
using GhTooltip = Grasshopper2.UI.Tooltip.Frame;
using GhUiNumber = Grasshopper2.UI.UiNumber;
using IAttributes = Grasshopper2.Doc.IAttributes;
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
    public sealed record ScreencapCase(Option<string> Folder) : TooltipOp;

    public static TooltipOp Show(IIcon icon, string caption, string message, TooltipBody? body = null, bool warnings = false, bool errors = false) =>
        new ShowCase(Icon: icon, Caption: caption, Message: message, Body: body ?? TooltipBody.Plain, Warnings: warnings, Errors: errors);

    public static TooltipOp Screencap(Option<string> folder = default) => new ScreencapCase(Folder: folder);

    internal CanvasChromePlan Plan() => Tooltip.Plan(op: this);
}

[SkipUnionOps]
[Union]
public partial record CanvasChromeOp : IUiOp<CanvasChromeResult> {
    private CanvasChromeOp() { }
    public sealed record TooltipCase(TooltipOp Op) : CanvasChromeOp;
    public sealed record FloatingButtonCase(FloatingButtonOp Op) : CanvasChromeOp;
    public sealed record InteractionCase(InteractionOp Op) : CanvasChromeOp;
    public sealed record ComposeCase(Seq<CanvasChromeOp> Ops) : CanvasChromeOp;

    public static CanvasChromeOp Tooltip(TooltipOp op) => new TooltipCase(Op: op);
    public static CanvasChromeOp FloatingButton(FloatingButtonOp op) => new FloatingButtonCase(Op: op);
    public static CanvasChromeOp Interaction(InteractionOp op) => new InteractionCase(Op: op);
    public static CanvasChromeOp Compose(params CanvasChromeOp[] ops) => new ComposeCase(Ops: toSeq(ops));

    GrasshopperUiIntent<CanvasChromeResult> IUiOp<CanvasChromeResult>.Intent() => Plan().Run;

    internal CanvasChromePlan Plan() => Switch(
        tooltipCase: static t => t.Op.Plan(),
        floatingButtonCase: static f => f.Op.Plan(),
        interactionCase: static i => i.Op.Plan(),
        composeCase: static c => CanvasChrome.Compose(ops: c.Ops));
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

    internal static CanvasChromeResult Sub(Subscription subscription) => new SubscriptionCase(Subscription: subscription);
    internal static CanvasChromeResult Found(Option<FloatingButtonInfo> info) => new FloatingButtonFoundCase(Info: info);
    internal static CanvasChromeResult Status(FloatingButtonSnapshot snapshot) => new FloatingButtonStatusCase(Snapshot: snapshot);
}

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

[SkipUnionOps]
[Union]
public partial record FloatingButtonOp {
    private FloatingButtonOp() { }
    public sealed record AddCase(FloatingButtonSpec Spec) : FloatingButtonOp;
    public sealed record ModifyCase(string Name, Option<string> Info = default, Option<IIcon> Icon = default, Option<Color> Colour = default, Option<(PointF Point, bool Immediate)> Anchor = default, Option<bool> Enabled = default) : FloatingButtonOp;
    public sealed record NamedCase(FloatingButtonNamedAction Action, Seq<string> Names) : FloatingButtonOp;
    public sealed record CloseAllCase : FloatingButtonOp;
    public sealed record FindByNameCase(string Name) : FloatingButtonOp;
    public sealed record FindByPointCase(PointF ControlPoint) : FloatingButtonOp;
    public sealed record StatusCase : FloatingButtonOp;

    public static FloatingButtonOp Add(FloatingButtonSpec spec) => new AddCase(Spec: spec);

    internal CanvasChromePlan Plan() => FloatingButton.Plan(op: this);
}

[SmartEnum<int>]
public sealed partial class FloatingButtonNamedAction {
    public static readonly FloatingButtonNamedAction Show = new(key: 0, run: static names => FloatingButton.SetVisible(names: names, visible: true));
    public static readonly FloatingButtonNamedAction Hide = new(key: 1, run: static names => FloatingButton.SetVisible(names: names, visible: false));
    public static readonly FloatingButtonNamedAction Close = new(key: 2, run: static names => FloatingButton.Close(names: names));

    [UseDelegateFromConstructor]
    internal partial GrasshopperUiIntent<Unit> Run(Seq<string> names);
}

[SkipUnionOps]
[Union]
public partial record DecisionDelta {
    private DecisionDelta() { }
    public sealed record BoundsCase(RectangleF Value) : DecisionDelta;
    public sealed record SizeCase(SizeF Value) : DecisionDelta;
    public sealed record CursorCase(Cursor Value) : DecisionDelta;
    public sealed record ResponseCase(GhResponse Value) : DecisionDelta;
    public static DecisionDelta Bounds(RectangleF value) => new BoundsCase(Value: value);
    public static DecisionDelta Size(SizeF value) => new SizeCase(Value: value);
    public static DecisionDelta Cursor(Cursor value) => new CursorCase(Value: value);
    public static DecisionDelta Respond(GhResponse value) => new ResponseCase(Value: value);
}

[SkipUnionOps]
[Union]
public partial record InteractionOp {
    private InteractionOp() { }
    public sealed record PushCase(IInteraction Target) : InteractionOp;
    public sealed record RegisterCase(IResponsive Responsive, CoordinateSystem System) : InteractionOp;
    public sealed record HoverCase(Option<TimeSpan> Delay, Func<MouseHoverSnapshot, Fin<Unit>> Handler) : InteractionOp;
    public sealed record ContextMenuCase(Func<ContextMenuSnapshot, Fin<Unit>> Handler) : InteractionOp;
    public sealed record ResizeCase(IResizableAttributes Target, SnapSetting Snap, Func<ResizeSession, Fin<DecisionDelta>> Decide) : InteractionOp;
    public sealed record GestureCase(NSGestureRecognizer Recognizer, Seq<NSGestureRecognizerState> States, Func<GestureSnapshot, Fin<Unit>> Handle) : InteractionOp;
    public sealed record PressureCase(NSPressureBehavior Behavior) : InteractionOp;
    // bool is true for KeyDown and false for KeyUp.
    public sealed record KeyboardCase(Func<KeyEventArgs, bool, Fin<Unit>> Handle) : InteractionOp;
    public sealed record StatusCase : InteractionOp;

    public static InteractionOp Resize(IResizableAttributes target, SnapSetting snap, Func<ResizeSession, Fin<DecisionDelta>> decide) => new ResizeCase(Target: target, Snap: snap, Decide: decide);
    public static InteractionOp Gesture(NSGestureRecognizer recognizer, Seq<NSGestureRecognizerState> states, Func<GestureSnapshot, Fin<Unit>> handle) => new GestureCase(Recognizer: recognizer, States: states, Handle: handle);
    public static InteractionOp Pressure(NSPressureBehavior behavior) => new PressureCase(Behavior: behavior);
    public static InteractionOp Keyboard(Func<KeyEventArgs, bool, Fin<Unit>> handle) => new KeyboardCase(Handle: handle);

    internal CanvasChromePlan Plan() => Interaction.Plan(op: this);
}

// --- [MODELS] -----------------------------------------------------------------------------
internal readonly record struct CanvasChromePlan(GrasshopperUiIntent<CanvasChromeResult> Run, Option<Func<GrasshopperUi.Scope, Fin<Subscription>>> Subscription) {
    internal GrasshopperUiPolicy Policy => Run.Policy;

    internal static CanvasChromePlan Result(GrasshopperUiIntent<CanvasChromeResult> intent) =>
        new(Run: intent, Subscription: default);

    internal static CanvasChromePlan Unit(GrasshopperUiIntent<Unit> intent) =>
        Result(intent: intent.Map(static _ => CanvasChromeResult.UnitInstance));

    internal static CanvasChromePlan SubscriptionOf(GrasshopperUiIntent<Subscription> intent) {
        return new(
            Run: intent.Map(CanvasChromeResult.Sub),
            Subscription: Some(intent.Run));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FloatingButtonHandlers(
    Option<FloatingButtonHandler> Click = default,
    Option<FloatingButtonHandler> MouseDown = default,
    Option<FloatingButtonHandler> MouseUp = default);

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

[StructLayout(LayoutKind.Auto)]
public readonly record struct ResizeSession(RectangleF Start, RectangleF Current, Option<SnappingSnapshot> Snap = default, Option<PointF> MouseContent = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct MouseHoverSnapshot(PointF ControlPoint, PointF ContentPoint);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ContextMenuSnapshot(ContextMenu Menu, MouseEventArgs MouseEvent, bool HasExistingItems = false);

[StructLayout(LayoutKind.Auto)]
public readonly record struct GestureSnapshot(NSGestureRecognizer Recognizer, NSGestureRecognizerState State, PointF Location);

// --- [BOUNDARIES] -------------------------------------------------------------------------
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

    // Fail closed when a reflected layout constant is absent or non-int.
    private static Fin<int> ReadInt(Type type, string name) =>
        Optional(type.GetField(name: name, bindingAttr: BindingFlags.Public | BindingFlags.Static))
            .Bind(field => Optional(field.GetValue(obj: null)))
            .Bind(value => Optional(value as int?))
            .ToFin(Fail: UiFault.MutationRejected(op: RailOp, detail: $"Tooltip.Layout.{name} is not a readable int constant"));
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class CanvasChrome {
    internal static CanvasChromePlan Compose(Seq<CanvasChromeOp> ops) {
        Seq<CanvasChromePlan> plans = ops.Map(static op => op.Plan()).Strict();
        GrasshopperUiPolicy policy = plans.Fold(GrasshopperUiPolicy.Read, static (acc, plan) => acc | plan.Policy);
        return CanvasChromePlan.SubscriptionOf(intent: new GrasshopperUiIntent<Subscription>(
            run: scope => plans
                .TraverseM(plan => plan.Subscription is { IsSome: true, Case: Func<GrasshopperUi.Scope, Fin<Subscription>> run }
                    ? run(arg: scope)
                    : plan.Run.Run(scope: scope).Map(static _ => Subscription.Empty))
                .As()
                .Map(static subs => subs.Fold(Subscription.Empty, static (acc, sub) => acc | sub)),
            policy: policy));
    }
}

internal static class Tooltip {
    // Tooltip.Frame is process-static; token-gate Dispose so a stale Show cannot hide its successor.
    private static readonly Atom<Option<Guid>> Owner = Atom(value: Option<Guid>.None);
    // Show-time Mouse.Position substitutes for host-private Tooltip.Frame.MouseLocationWhenShown.
    private static readonly Atom<Option<PointF>> ShownLocation = Atom(value: Option<PointF>.None);

    internal static CanvasChromePlan Plan(TooltipOp op) =>
        op.Switch(
            showCase: static s => CanvasChromePlan.SubscriptionOf(intent: GhUi.Canvas(run: scope =>
                scope.NeedCanvas().Bind(_canvas => {
                    Guid token = Guid.NewGuid();
                    return Subscription.Bind(
                        attach: () => {
                            ShowNative(show: s);
                            _ = Owner.Swap(_swap => Some(token)).Iter(static _ => { });
                        },
                        detach: () => _ = Owner.Value
                            .Filter(held => held == token)
                            .Iter(_held => Withdraw()),
                        marshalToUi: true,
                        teardown: SubscriptionTeardown.TokenGated);
                }))),
            hideCase: static _ => CanvasChromePlan.Unit(intent: HideNow()),
            invalidateCase: static _ => CanvasChromePlan.Unit(intent: InvalidateNow()),
            statusCase: static _ => CanvasChromePlan.Result(intent: SnapshotNow().Map(static snap => (CanvasChromeResult)new CanvasChromeResult.TooltipStatusCase(Snapshot: snap))),
            layoutCase: static _ => CanvasChromePlan.Result(intent: TooltipRail.Layout().Map(static snap => (CanvasChromeResult)new CanvasChromeResult.TooltipLayoutCase(Snapshot: snap))),
            screencapCase: static s => CanvasChromePlan.Unit(intent: ScreencapNow(folder: s.Folder)));

    internal static GrasshopperUiIntent<Unit> HideNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(HideNow)).Attempt(body: () => _ = Withdraw(), what: "Tooltip.Frame.Hide"));

    internal static GrasshopperUiIntent<Unit> InvalidateNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(InvalidateNow)).Attempt(body: GhTooltip.Invalidate, what: "Tooltip.Frame.Invalidate"));

    internal static GrasshopperUiIntent<TooltipSnapshot> SnapshotNow() =>
        GhUi.Read(run: scope => Op.Of(name: nameof(SnapshotNow)).Attempt(body: SnapshotNative, what: "Tooltip.Frame.Visible"));

    internal static GrasshopperUiIntent<Unit> ScreencapNow(Option<string> folder) =>
        GhUi.Read(run: scope => Op.Of(name: nameof(ScreencapNow)).Attempt(
            // BOUNDARY ADAPTER — None projects to string.Empty because GhTooltip disables capture via IsNullOrWhiteSpace.
            body: () => folder.Match(
                Some: static path => GhTooltip.ScreencapFolder = path,
                None: static () => GhTooltip.ScreencapFolder = string.Empty),
            what: "Tooltip.Frame.ScreencapFolder").Map(static _ => unit));

    private static Unit Withdraw() {
        _ = Owner.Swap(static _ => Option<Guid>.None);
        _ = ShownLocation.Swap(static _ => Option<PointF>.None);
        GhTooltip.Hide();
        return unit;
    }

    private static TooltipSnapshot SnapshotNative() =>
        new(Visible: GhTooltip.Visible, OwnerToken: Owner.Value, MouseLocationWhenShown: ShownLocation.Value);

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

internal sealed class OwnedSubscription<TKey> {
    private readonly Atom<HashMap<TKey, Guid>> owners = Atom(value: HashMap<TKey, Guid>());

    internal void Clear(TKey key) => _ = owners.Swap(map => map.Remove(key));

    internal void ClearAll() => _ = owners.Swap(_ => HashMap<TKey, Guid>());

    internal Fin<Subscription> Bind(TKey key, Action attach, Action<TKey> detach) {
        Guid token = Guid.NewGuid();
        return Subscription.Bind(
            // Register only after attach commits so a throwing attach cannot orphan an ownership slot.
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

internal static class FloatingButton {
    // Token gate per name: stale Subscription.Dispose only Close(name) when its Guid still owns the slot.
    private static readonly OwnedSubscription<string> Owners = new();

    internal static CanvasChromePlan Plan(FloatingButtonOp op) =>
        op.Switch(
            addCase: static a => CanvasChromePlan.SubscriptionOf(intent: Add(a.Spec)),
            modifyCase: static m => CanvasChromePlan.Unit(intent: Modify(m.Name, m.Info, m.Icon, m.Colour, m.Anchor, m.Enabled)),
            namedCase: static n => CanvasChromePlan.Unit(intent: n.Action.Run(names: n.Names)),
            closeAllCase: static _ => CanvasChromePlan.Unit(intent: CloseAll()),
            findByNameCase: static f => CanvasChromePlan.Result(intent: FindByName(f.Name).Map(CanvasChromeResult.Found)),
            findByPointCase: static f => CanvasChromePlan.Result(intent: FindByPoint(f.ControlPoint).Map(CanvasChromeResult.Found)),
            statusCase: static _ => CanvasChromePlan.Result(intent: SnapshotNow().Map(CanvasChromeResult.Status)));

    internal static GrasshopperUiIntent<Subscription> Add(FloatingButtonSpec spec) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from placement in Optional(spec.Placement).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Add)), detail: "placement is required"))
            from icon in Optional(spec.Icon).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Add)), detail: "icon is required"))
            from name in Op.Of(name: nameof(Add)).AcceptText(value: spec.Name)
            from _duplicate in guard(!canvas.FloatingButtons.IsDefined(name: name), (Error)UiFault.InvalidInput(op: Op.Of(name: nameof(Add)), detail: $"button '{name}' already exists")).ToFin()
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
        Atom<Option<NativeFloatingButton>> registered = Atom(value: Option<NativeFloatingButton>.None);
        Atom<Option<EventHandler<FloatingButtonEventArgs<GhUiNumber>>>> changed = Atom(value: Option<EventHandler<FloatingButtonEventArgs<GhUiNumber>>>.None);
        return Owners.Bind(
            key: spec.Name,
            attach: () => {
                Color? colour = spec.Colour.Map(static picked => (Color?)picked).IfNone((Color?)null);
                FloatingButtonHandler click = spec.Handlers.Click.IfNone(NoOp);
                FloatingButtonHandler mouseDown = spec.Handlers.MouseDown.IfNone(NoOp);
                FloatingButtonHandler mouseUp = spec.Handlers.MouseUp.IfNone(NoOp);
                _ = anchor.Map(point => Op.Side(() => canvas.FloatingButtons.AddAnchored(anchor: point, name: spec.Name, info: spec.Info, colour: colour, icon: spec.Icon, click: click, mouseDown: mouseDown, mouseUp: mouseUp)))
                    .IfNone(() => Op.Side(() => canvas.FloatingButtons.Add(position: position, name: spec.Name, info: spec.Info, colour: colour, icon: spec.Icon, click: click, mouseDown: mouseDown, mouseUp: mouseUp)));
                NativeFloatingButton? resolved = null;
                // BOUNDARY ADAPTER — ThrowIfFail lets Subscription.Bind's Try.lift reject vanished registrations.
                _ = Optional(canvas.FloatingButtons.FindByName(name: spec.Name))
                    .ToFin(Fail: UiFault.MutationRejected(op: Op.Of(name: nameof(Add)), detail: $"button '{spec.Name}' was not registered after Add()"))
                    .Map(found => registered.Swap(_swap => (resolved = found, Some(found)).Item2))
                    .ThrowIfFail();
                _ = (resolved, spec.Numeric) is (NativeFloatingButton button, { IsSome: true, Case: FloatingNumeric numeric })
                    ? Op.Side(() => {
                        button.MakeNumeric(number: numeric.Value, valueKey: numeric.ValueKey);
                        _ = numeric.Changed
                            .Map(sink => (EventHandler<FloatingButtonEventArgs<GhUiNumber>>)((_, _) =>
                                _ = GrasshopperUi.Handler(valid: () => button.NumericValue is GhUiNumber current ? sink(arg: current.Value) : Fin.Succ(value: unit))))
                            .Iter(handler => {
                                _ = changed.Swap(_swap => Some(handler));
                                button.ValueChanged += handler;
                            });
                    })
                    : unit;
            },
            detach: target => {
                _ = changed.Value.Iter(handler => registered.Value.Iter(button => button.ValueChanged -= handler));
                canvas.FloatingButtons.Close(target);
            });
    }

    internal static GrasshopperUiIntent<Unit> Modify(
        string name,
        Option<string> info = default,
        Option<IIcon> icon = default,
        Option<Color> colour = default,
        Option<(PointF Point, bool Immediate)> anchor = default,
        Option<bool> enabled = default) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from found in Optional(canvas.FloatingButtons.FindByName(name: name))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Modify)), detail: $"button '{name}' not found"))
            from validAnchor in anchor.Match(
                Some: a => Op.Of(name: nameof(Modify)).AcceptPoint(value: a.Point, detail: "non-finite anchor").Map(p => Some((Point: p, a.Immediate))),
                None: () => Fin.Succ(Option<(PointF Point, bool Immediate)>.None))
            from _ in Seq(
                ModifyField(value: info, mutate: v => canvas.FloatingButtons.ModifyInfo(name: name, buttonInfo: v), field: "ModifyInfo"),
                ModifyField(value: icon, mutate: v => canvas.FloatingButtons.ModifyIcon(name: name, icon: v), field: "ModifyIcon"),
                ModifyField(value: colour, mutate: v => canvas.FloatingButtons.ModifyColour(name: name, colour: v), field: "ModifyColour"),
                ModifyField(value: validAnchor, mutate: av => canvas.FloatingButtons.ModifyAnchor(name: name, anchor: av.Point, immediate: av.Immediate), field: "ModifyAnchor"),
                ModifyField(value: enabled, mutate: v => found.Enabled = v, field: "Enabled"))
                .TraverseM(static step => step).As()
            select unit);

    // Present fields self-name Op.Attempt so TraverseM faults identify the exact chrome mutation.
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

    private static FloatingButtonInfo InfoOf(NativeFloatingButton button) =>
        new(Name: button.Name, Info: button.Info,
            Position: button.Position, State: button.State,
            Enabled: button.Enabled, HasFocus: button.HasFocus,
            Colour: button.Colour,
            Anchor: button.Position == FloatingPosition.Anchored ? Some(button.Anchor) : Option<PointF>.None,
            // NumericValue is null until MakeNumeric; project host absence through Option.
            NumericValue: Optional(button.NumericValue).Map(static value => value.Value));

    private static GhResponse NoOp(NativeFloatingButton _, MouseEventArgs __) => GhResponse.Ignored;
}

// Canvas leases key by identity hash so the per-lease atom does not retain live canvases.
internal static class CanvasLease {
    internal static readonly CanvasLease<TimeSpan> HoverDelayLease = new(Slot: nameof(GhCanvas.MouseHoverDelay), Get: static c => c.MouseHoverDelay, Set: static (c, v) => c.MouseHoverDelay = v);
    internal static readonly CanvasLease<bool> RedrawLease = new(Slot: nameof(GhCanvas.RedrawOnMouseMove), Get: static c => c.RedrawOnMouseMove, Set: static (c, v) => c.RedrawOnMouseMove = v);
    internal static readonly CanvasLease<NSPressureConfiguration> PressureLease = new(Slot: "PressureConfiguration", Get: static c => (c.ControlObject as NSView)?.PressureConfiguration!, Set: static (c, v) => Optional(c.ControlObject as NSView).Iter(view => view.PressureConfiguration = v));
}

internal sealed class CanvasLease<T>(string Slot, Func<GhCanvas, T> Get, Action<GhCanvas, T> Set) {
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct Entry(Guid Token, T Value);
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct State(T Baseline, Seq<Entry> Entries);

    // Each lease owns a strongly-typed stack keyed by canvas-identity hash — no untyped HashMap<int,object> + State<T> casts.
    private readonly Atom<HashMap<int, State>> stacks = Atom(value: HashMap<int, State>());

    private int KeyOf(GhCanvas canvas) => HashCode.Combine(RuntimeHelpers.GetHashCode(o: canvas), Slot);

    internal Guid Enter(GhCanvas canvas, T value) {
        Guid token = Guid.NewGuid();
        int key = KeyOf(canvas: canvas);
        T baseline = Get(arg: canvas);
        _ = stacks.Swap(map => map.AddOrUpdate(
            key: key,
            Some: state => state with { Entries = state.Entries + new Entry(Token: token, Value: value) },
            None: () => new State(Baseline: baseline, Entries: Seq(new Entry(Token: token, Value: value)))));
        Set(arg1: canvas, arg2: value);
        return token;
    }

    internal bool IsTop(GhCanvas canvas, Guid token, T value) =>
        stacks.Value.Find(key: KeyOf(canvas: canvas))
            .Map(state => !state.Entries.IsEmpty
                && state.Entries.Last().Token == token
                && EqualityComparer<T>.Default.Equals(x: state.Entries.Last().Value, y: value))
            .IfNone(noneValue: false);

    // BOUNDARY ADAPTER — non-LIFO disposal restores the top remaining value, or the captured baseline once empty.
    internal void Exit(GhCanvas canvas, Guid token) {
        int key = KeyOf(canvas: canvas);
        Option<T> restore = Option<T>.None;
        _ = stacks.Swap(map => map.Find(key)
            .Map(state => (State: state, Kept: state.Entries.Filter(entry => entry.Token != token)))
            .Match(
                Some: pair => {
                    restore = Some(pair.Kept.IsEmpty ? pair.State.Baseline : pair.Kept.Last().Value);
                    return pair.Kept.IsEmpty ? map.Remove(key) : map.SetItem(key, pair.State with { Entries = pair.Kept });
                },
                None: () => map));
        _ = restore.Iter(value => Set(arg1: canvas, arg2: value));
    }
}

internal static class Interaction {
    internal static CanvasChromePlan Plan(InteractionOp op) =>
        op.Switch(
            pushCase: static p => CanvasChromePlan.SubscriptionOf(intent: Push(p.Target)),
            registerCase: static r => CanvasChromePlan.SubscriptionOf(intent: Register(r.Responsive, r.System)),
            hoverCase: static h => CanvasChromePlan.SubscriptionOf(intent: Hover(h.Delay, h.Handler)),
            contextMenuCase: static c => CanvasChromePlan.SubscriptionOf(intent: ContextMenu(c.Handler)),
            resizeCase: static r => CanvasChromePlan.SubscriptionOf(intent: Resize(r.Target, r.Snap, r.Decide)),
            gestureCase: static g => CanvasChromePlan.SubscriptionOf(intent: Gesture(g.Recognizer, g.States, g.Handle)),
            pressureCase: static p => CanvasChromePlan.SubscriptionOf(intent: Pressure(p.Behavior)),
            keyboardCase: static k => CanvasChromePlan.SubscriptionOf(intent: Keyboard(k.Handle)),
            statusCase: static _ => CanvasChromePlan.Result(intent: SnapshotNow().Map(static snap => (CanvasChromeResult)new CanvasChromeResult.InteractionStatusCase(Snapshot: snap))));

    // PushInteraction has no public pop-by-reference; interactions self-release on completion.
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

    // Delay None preserves current MouseHoverDelay; explicit delays are leased and restored on detach.
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
            let token = Atom(value: Guid.Empty)
            from sub in Subscription.Bind(
                attach: () => {
                    _ = validDelay.Iter(value => token.Swap(_ => CanvasLease.HoverDelayLease.Enter(canvas: canvas, value: value)));
                    canvas.MouseHover += onHover;
                },
                detach: () => {
                    canvas.MouseHover -= onHover;
                    _ = validDelay.Iter(_unused => CanvasLease.HoverDelayLease.Exit(canvas: canvas, token: token.Value));
                },
                marshalToUi: true)
            select sub);

    internal static GrasshopperUiIntent<Subscription> ContextMenu(Func<ContextMenuSnapshot, Fin<Unit>> handler) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ContextMenu)), detail: "handler is required"))
            let onPopulate = (EventHandler<PopulateContextMenuEventArgs>)((_, e) =>
                _ = GrasshopperUi.Handler(valid: () => valid(arg: new ContextMenuSnapshot(Menu: e.Menu, MouseEvent: e.MouseEvent, HasExistingItems: e.Menu.Items.Count > 0))))
            from sub in Subscription.Bind(
                attach: () => canvas.PopulateContextMenu += onPopulate,
                detach: () => canvas.PopulateContextMenu -= onPopulate,
                marshalToUi: true)
            select sub);

    // SnapCore needs document-owned live IAttributes, not only IResizableAttributes pivot/size.
    internal static GrasshopperUiIntent<Subscription> Resize(IResizableAttributes target, SnapSetting snap, Func<ResizeSession, Fin<DecisionDelta>> decide) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from document in scope.NeedDocument()
            from validTarget in Optional(target).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Resize)), detail: "resize target is required"))
            from owner in Optional(validTarget as IAttributes).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Resize)), detail: "resize target must be a live IAttributes"))
            from validDecide in Optional(decide).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Resize)), detail: "decide is required"))
            from sub in Subscription.Bind(
                attach: () => canvas.PushInteraction(interaction: new ResizeInteraction(target: validTarget, owner: owner, document: document, canvas: canvas, snap: snap, decide: validDecide)),
                detach: static () => { },
                marshalToUi: true)
            select sub);

    // Local monitor observes gesture phases and returns events unchanged so GH2 input routing is not swallowed.
    private const NSEventMask GestureMask =
        NSEventMask.EventGesture | NSEventMask.EventMagnify | NSEventMask.EventSwipe | NSEventMask.EventRotate
        | NSEventMask.EventBeginGesture | NSEventMask.EventEndGesture | NSEventMask.SmartMagnify | NSEventMask.Pressure;

    internal static GrasshopperUiIntent<Subscription> Gesture(NSGestureRecognizer recognizer, Seq<NSGestureRecognizerState> states, Func<GestureSnapshot, Fin<Unit>> handle) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from view in Optional(canvas.ControlObject as NSView).ToFin(Fail: UiFault.MutationRejected(op: Op.Of(name: nameof(Gesture)), detail: "canvas.ControlObject is not an NSView"))
            from validRecognizer in Optional(recognizer).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Gesture)), detail: "recognizer is required"))
            from validHandle in Optional(handle).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Gesture)), detail: "handle is required"))
            let onGesture = (LocalEventHandler)(gestureEvent => {
                _ = states.Exists(state => state == validRecognizer.State)
                    ? GrasshopperUi.Handler(valid: () => validHandle(arg: new GestureSnapshot(Recognizer: validRecognizer, State: validRecognizer.State, Location: GestureLocation(recognizer: validRecognizer, view: view))))
                    : unit;
                return gestureEvent;
            })
            let monitor = Atom(value: Option<NSObject>.None)
            from sub in Subscription.Bind(
                attach: () => {
                    view.AddGestureRecognizer(gestureRecognizer: validRecognizer);
                    _ = monitor.Swap(_ => Optional(NSEvent.AddLocalMonitorForEventsMatchingMask(mask: GestureMask, handler: onGesture)));
                },
                detach: () => {
                    _ = monitor.Value.Iter(NSEvent.RemoveMonitor);
                    view.RemoveGestureRecognizer(gestureRecognizer: validRecognizer);
                },
                marshalToUi: true)
            select sub);

    // Lease PressureConfiguration so detach restores the prior force-touch behavior.
    internal static GrasshopperUiIntent<Subscription> Pressure(NSPressureBehavior behavior) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from view in Optional(canvas.ControlObject as NSView).ToFin(Fail: UiFault.MutationRejected(op: Op.Of(name: nameof(Pressure)), detail: "canvas.ControlObject is not an NSView"))
            let token = Atom(value: Guid.Empty)
            from sub in Subscription.Bind(
                attach: () => token.Swap(_ => CanvasLease.PressureLease.Enter(canvas: canvas, value: new NSPressureConfiguration(pressureBehavior: behavior))),
                detach: () => CanvasLease.PressureLease.Exit(canvas: canvas, token: token.Value),
                marshalToUi: true)
            select sub);

    internal static GrasshopperUiIntent<Subscription> Keyboard(Func<KeyEventArgs, bool, Fin<Unit>> handle) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validHandle in Optional(handle).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Keyboard)), detail: "handle is required"))
            let onKeyDown = (EventHandler<KeyEventArgs>)((_, e) =>
                _ = GrasshopperUi.Handler(valid: () => validHandle(e, true)))
            let onKeyUp = (EventHandler<KeyEventArgs>)((_, e) =>
                _ = GrasshopperUi.Handler(valid: () => validHandle(e, false)))
            from sub in Subscription.Bind(
                attach: () => {
                    canvas.KeyDown += onKeyDown;
                    canvas.KeyUp += onKeyUp;
                },
                detach: () => {
                    canvas.KeyDown -= onKeyDown;
                    canvas.KeyUp -= onKeyUp;
                },
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

    // BOUNDARY ADAPTER — LocationInView returns view-space NFloat coordinates; GestureSnapshot carries Eto PointF.
    private static PointF GestureLocation(NSGestureRecognizer recognizer, NSView view) {
        CGPoint point = recognizer.LocationInView(view: view);
        return new PointF(x: (float)point.X, y: (float)point.Y);
    }
}

// BOUNDARY ADAPTER — IInteraction focus capsule; mouse-up releases focus and clears the snap overlay.
[BoundaryAdapter]
internal sealed class ResizeInteraction : AbstractInteraction {
    private readonly IResizableAttributes target;
    private readonly IAttributes owner;
    private readonly GhDocument document;
    private readonly GhCanvas canvas;
    private readonly SnappingPolicy policy;
    private readonly Func<ResizeSession, Fin<DecisionDelta>> decide;
    private readonly Atom<ResizeSession> session;

    internal ResizeInteraction(IResizableAttributes target, IAttributes owner, GhDocument document, GhCanvas canvas, SnapSetting snap, Func<ResizeSession, Fin<DecisionDelta>> decide)
        : base(nomen: new Nomen(name: "Resize", info: "Rasm resize interaction"), icon: null) {
        this.target = target;
        this.owner = owner;
        this.document = document;
        this.canvas = canvas;
        policy = new SnappingPolicy(Settings: Seq(snap));
        this.decide = decide;
        session = Atom(value: new ResizeSession(Start: owner.Bounds, Current: owner.Bounds, Snap: Option<SnappingSnapshot>.None));
    }

    public override GhResponse RespondToMouseMove(MouseEventArgs e) {
        _ = session.Swap(prev => prev with { MouseContent = Some(canvas.Map(point: e.Location, from: CoordinateSystem.Control, to: CoordinateSystem.Content)) });
        return decide(arg: session.Value)
            .Map(Apply)
            .IfFail(GhResponse.Ignored);
    }

    public override GhResponse RespondToMouseUp(MouseEventArgs e) {
        canvas.SnapXAction = null;
        canvas.SnapYAction = null;
        return GhResponse.Release;
    }

    // Only bounds/size deltas update geometry; cursor/response deltas keep the current rectangle.
    private GhResponse Apply(DecisionDelta delta) {
        RectangleF bounds = delta.Switch(
            state: session.Value.Current,
            boundsCase: static (_, b) => b.Value,
            sizeCase: static (current, s) => new RectangleF(location: current.Location, size: s.Value),
            cursorCase: static (current, _) => current,
            responseCase: static (current, _) => current);
        Option<SnappingSnapshot> snapped = owner.Snappable
            ? Layout.SnapCore(document: document, obj: owner.Owner, policy: policy, visibleLimit: canvas.VisibleFrame, bounds: Some(bounds), wireCandidates: Seq(owner.Owner))
            : Option<SnappingSnapshot>.None;
        RectangleF snappedBounds = snapped.Map(snap => new RectangleF(x: bounds.X + snap.Dx, y: bounds.Y + snap.Dy, width: bounds.Width, height: bounds.Height)).IfNone(bounds);
        target.Size = snappedBounds.Size;
        _ = session.Swap(prev => prev with { Current = snappedBounds, Snap = snapped });
        // BOUNDARY ADAPTER — null is GH2's clear-sentinel for SnapXAction/SnapYAction; None projects to host clear.
#pragma warning disable CS8603, CS8625
        canvas.SnapXAction = snapped.Bind(snap => snap.XLabel.Map(label => AxisAction(dx: snap.Dx, dy: 0f, label: label, lines: snap.Lines))).IfNone(() => null);
        canvas.SnapYAction = snapped.Bind(snap => snap.YLabel.Map(label => AxisAction(dx: 0f, dy: snap.Dy, label: label, lines: snap.Lines))).IfNone(() => null);
#pragma warning restore CS8603, CS8625
        return GhResponse.Handled;
    }

    // Rebuild native per-axis overlay actions from the snapped delta and label.
    private static SnappingAction AxisAction(float dx, float dy, SnapLabel label, Seq<LineF> lines) =>
        new(dx: dx, dy: dy, text: label.Text, point: label.Point, anchor: label.Anchor.IfNone(TextAnchor.LowerMiddle), lines: [.. lines]);
}
