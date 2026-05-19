using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.UI;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using GhComponent = Grasshopper2.Components.Component;
using InputPanelControl = Grasshopper2.UI.InputPanel.InputPanel;
using UiContext = Eto.Drawing.Context;
using UiResponse = Grasshopper2.UI.Flex.Response;
using UiShape = Grasshopper2.UI.Skinning.Shape;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] ---------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ComponentUi(Seq<ComponentUiOp> Ops) {
    public static ComponentUi Empty => default;

    public static ComponentUi operator +(ComponentUi left, ComponentUi right) =>
        Add(left: left, right: right);

    public static ComponentUi Add(ComponentUi left, ComponentUi right) =>
        new(Ops: left.Ops + right.Ops);

    public static ComponentUi Of(Func<ComponentUiContext, Fin<ComponentUiDecision>> run) =>
        new(Ops: Seq(new ComponentUiOp(Run: run)));

    public static ComponentUi When(ComponentUiPhase phase, Func<ComponentUiContext, Fin<ComponentUiDecision>> run) =>
        Of(run: context => context.Phase == phase ? run(arg: context) : Fin.Succ(value: ComponentUiDecision.Pass));

    internal IAttributes Attributes(GhComponent owner) =>
        Ops.IsEmpty ? new ComponentAttributes(owner: owner) : new RasmComponentAttributes(owner: owner, ui: this);

    internal Fin<Unit> Append(GhComponent owner, InputPanelControl panel) =>
        Run(context: ComponentUiContext.ForPanel(owner: owner, panel: panel)).Map(static _ => unit);

    internal Fin<ComponentUiDecision> Run(ComponentUiContext context) =>
        Ops.Fold(Fin.Succ(value: ComponentUiDecision.Pass), (Fin<ComponentUiDecision> state, ComponentUiOp op) =>
            state.Bind(decision => decision.IsTerminal ? Fin.Succ(value: decision) : op.Run(arg: context).Map(next => decision + next)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ComponentUiOp(Func<ComponentUiContext, Fin<ComponentUiDecision>> Run);

public enum ComponentUiPhase {
    Layout,
    DrawForeground,
    InputPanel,
    ContextMenu,
    Cursor,
    Hover,
    MouseDown,
    MouseMove,
    MouseUp,
    MouseSingleClick,
    MouseDoubleClick,
    KeyDown,
    KeyUp,
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ComponentUiContext(
    ComponentUiPhase Phase,
    GhComponent Owner,
    Option<InputPanelControl> Panel,
    Option<ContextMenu> Menu,
    Option<UiContext> Context,
    Option<MouseEventArgs> Mouse,
    Option<KeyEventArgs> Key,
    Option<PointF> Point,
    Option<UiShape> Shape) {
    public static ComponentUiContext Layout(GhComponent owner, UiShape shape) =>
        new(Phase: ComponentUiPhase.Layout, Owner: owner, Panel: None, Menu: None, Context: None, Mouse: None, Key: None, Point: None, Shape: Optional(shape));

    public static ComponentUiContext Draw(GhComponent owner, UiContext context) =>
        new(Phase: ComponentUiPhase.DrawForeground, Owner: owner, Panel: None, Menu: None, Context: Optional(context), Mouse: None, Key: None, Point: None, Shape: None);

    public static ComponentUiContext ForPanel(GhComponent owner, InputPanelControl panel) =>
        new(Phase: ComponentUiPhase.InputPanel, Owner: owner, Panel: Optional(panel), Menu: None, Context: None, Mouse: None, Key: None, Point: None, Shape: None);

    public static ComponentUiContext ForMenu(GhComponent owner, ContextMenu menu) =>
        new(Phase: ComponentUiPhase.ContextMenu, Owner: owner, Panel: None, Menu: Optional(menu), Context: None, Mouse: None, Key: None, Point: None, Shape: None);

    public static ComponentUiContext Pointer(ComponentUiPhase phase, GhComponent owner, PointF point) =>
        new(Phase: phase, Owner: owner, Panel: None, Menu: None, Context: None, Mouse: None, Key: None, Point: Optional(point), Shape: None);

    public static ComponentUiContext MouseEvent(ComponentUiPhase phase, GhComponent owner, MouseEventArgs mouse) =>
        new(Phase: phase, Owner: owner, Panel: None, Menu: None, Context: None, Mouse: Optional(mouse), Key: None, Point: Optional(mouse).Map(static value => value.Location), Shape: None);

    public static ComponentUiContext KeyEvent(ComponentUiPhase phase, GhComponent owner, KeyEventArgs key) =>
        new(Phase: phase, Owner: owner, Panel: None, Menu: None, Context: None, Mouse: None, Key: Optional(key), Point: None, Shape: None);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ComponentUiDecision(
    Option<RectangleF> Bounds,
    Option<UiResponse> Response,
    Option<Cursor> Cursor,
    Option<bool> Hover,
    bool IsTerminal) {
    public static ComponentUiDecision Pass => default;
    public static ComponentUiDecision Handled => new(Bounds: None, Response: Optional(UiResponse.Handled), Cursor: None, Hover: None, IsTerminal: true);
    public static ComponentUiDecision Capture => new(Bounds: None, Response: Optional(UiResponse.Capture), Cursor: None, Hover: None, IsTerminal: true);
    public static ComponentUiDecision Release => new(Bounds: None, Response: Optional(UiResponse.Release), Cursor: None, Hover: None, IsTerminal: true);
    public static ComponentUiDecision WithBounds(RectangleF bounds) => new(Bounds: Optional(bounds), Response: None, Cursor: None, Hover: None, IsTerminal: false);
    public static ComponentUiDecision WithCursor(Cursor cursor) => new(Bounds: None, Response: None, Cursor: Optional(cursor), Hover: None, IsTerminal: false);
    public static ComponentUiDecision WithHover(bool value) => new(Bounds: None, Response: None, Cursor: None, Hover: Optional(value), IsTerminal: false);

    public static ComponentUiDecision operator +(ComponentUiDecision left, ComponentUiDecision right) =>
        Add(left: left, right: right);

    public static ComponentUiDecision Add(ComponentUiDecision left, ComponentUiDecision right) =>
        new(
            Bounds: right.Bounds | left.Bounds,
            Response: left.Response | right.Response,
            Cursor: right.Cursor | left.Cursor,
            Hover: right.Hover | left.Hover,
            IsTerminal: left.IsTerminal || right.IsTerminal);
}

// --- [SERVICES] -------------------------------------------------------------------------
[BoundaryAdapter]
internal sealed class RasmComponentAttributes :
    ComponentAttributes,
    IContextMenuAware,
    ICursorAwareAttributes,
    IMouseHoverAttributes {
    private readonly ComponentUi ui;

    internal RasmComponentAttributes(GhComponent owner, ComponentUi ui) : base(owner: owner) =>
        this.ui = ui;

    protected override void LayoutBounds(UiShape shape) {
        base.LayoutBounds(shape: shape);
        _ = Run(context: ComponentUiContext.Layout(owner: Owner, shape: shape)).Map(decision => {
            Bounds = decision.Bounds.IfNone(Bounds);
            return unit;
        });
    }

    protected override void DrawForeground(UiContext context, Skin skin, Capsule capsule, Shade shade) {
        base.DrawForeground(context: context, skin: skin, capsule: capsule, shade: shade);
        _ = Run(context: ComponentUiContext.Draw(owner: Owner, context: context));
    }

    void IContextMenuAware.AppendToMenu(ContextMenu menu) =>
        _ = Run(context: ComponentUiContext.ForMenu(owner: Owner, menu: menu));

    Cursor ICursorAwareAttributes.CursorAt(PointF point) =>
        Run(context: ComponentUiContext.Pointer(phase: ComponentUiPhase.Cursor, owner: Owner, point: point))
            .Map(decision => decision.Cursor.IfNone(Cursors.Default))
            .IfFail(_ => Cursors.Default);

    bool IMouseHoverAttributes.RespondToMouseHover(PointF controlPoint, PointF contentPoint) =>
        Run(context: ComponentUiContext.Pointer(phase: ComponentUiPhase.Hover, owner: Owner, point: contentPoint))
            .Map(decision => decision.Hover.IfNone(false))
            .IfFail(_ => false);

    protected override UiResponse HandleMouseDown(MouseEventArgs e) => Respond(phase: ComponentUiPhase.MouseDown, mouse: e);
    protected override UiResponse HandleMouseMove(MouseEventArgs e) => Respond(phase: ComponentUiPhase.MouseMove, mouse: e);
    protected override UiResponse HandleMouseUp(MouseEventArgs e) => Respond(phase: ComponentUiPhase.MouseUp, mouse: e);
    protected override UiResponse HandleSingleClick(MouseEventArgs e) => Respond(phase: ComponentUiPhase.MouseSingleClick, mouse: e);
    protected override UiResponse HandleDoubleClick(MouseEventArgs e) => Respond(phase: ComponentUiPhase.MouseDoubleClick, mouse: e);
    protected override UiResponse HandleKeyDown(KeyEventArgs e) => Respond(phase: ComponentUiPhase.KeyDown, key: e);
    protected override UiResponse HandleKeyUp(KeyEventArgs e) => Respond(phase: ComponentUiPhase.KeyUp, key: e);

    private UiResponse Respond(ComponentUiPhase phase, MouseEventArgs mouse) =>
        Run(context: ComponentUiContext.MouseEvent(phase: phase, owner: Owner, mouse: mouse))
            .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
            .IfFail(_ => UiResponse.Ignored);

    private UiResponse Respond(ComponentUiPhase phase, KeyEventArgs key) =>
        Run(context: ComponentUiContext.KeyEvent(phase: phase, owner: Owner, key: key))
            .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
            .IfFail(_ => UiResponse.Ignored);

    private Fin<ComponentUiDecision> Run(ComponentUiContext context) =>
        // BOUNDARY ADAPTER -- GH2 canvas callbacks enter the Rasm UI rail synchronously and surface exceptions as Fin failures.
        Try.lift<Fin<ComponentUiDecision>>(f: () => ui.Run(context: context))
            .Run()
            .MapFail(_ => Op.Of(name: nameof(Run)).InvalidResult())
            .Bind(static result => result);
}
