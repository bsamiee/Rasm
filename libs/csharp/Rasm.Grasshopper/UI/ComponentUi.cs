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
public readonly record struct ComponentUi {
    private readonly Seq<ComponentUiOp> ops;

    private ComponentUi(Seq<ComponentUiOp> ops) => this.ops = ops;

    public static ComponentUi Empty => default;

    public static ComponentUi operator +(ComponentUi left, ComponentUi right) =>
        Add(left: left, right: right);

    public static ComponentUi Add(ComponentUi left, ComponentUi right) =>
        new(ops: left.ops + right.ops);

    public static ComponentUi Of(Func<ComponentUiContext, Fin<ComponentUiDecision>> run) =>
        new(ops: Seq(new ComponentUiOp(Run: run)));

    public static ComponentUi When(ComponentUiPhase phase, Func<ComponentUiContext, Fin<ComponentUiDecision>> run) =>
        Of(run: context => (context.Phase == phase) switch {
            true => run(arg: context),
            false => Fin.Succ(value: ComponentUiDecision.Pass),
        });

    internal IAttributes Attributes(GhComponent owner) =>
        ops.IsEmpty ? new ComponentAttributes(owner: owner) : new RasmComponentAttributes(owner: owner, ui: this);

    internal Fin<Unit> Append(GhComponent owner, InputPanelControl panel) =>
        Run(context: ComponentUiContext.ForPanel(owner: owner, panel: panel)).Map(static _ => unit);

    internal Fin<ComponentUiDecision> Run(ComponentUiContext context) {
        Seq<ComponentUiOp> current = ops;
        // BOUNDARY ADAPTER -- GH2 component UI callbacks enter the Rasm rail synchronously and surface exceptions as Fin failures.
        return Try.lift<Fin<ComponentUiDecision>>(f: () => current.Fold(Fin.Succ(value: ComponentUiDecision.Pass), (Fin<ComponentUiDecision> state, ComponentUiOp op) =>
                state.Bind(decision => decision.IsTerminal switch {
                    true => Fin.Succ(value: decision),
                    false => op.Run(arg: context).Map(next => decision + next),
                })))
            .Run()
            .MapFail(_ => Op.Of(name: nameof(Run)).InvalidResult())
            .Bind(static result => result);
    }
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct ComponentUiOp(Func<ComponentUiContext, Fin<ComponentUiDecision>> Run);

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
    Option<Skin> Skin,
    Option<Capsule> Capsule,
    Option<Shade> Shade,
    Option<MouseEventArgs> Mouse,
    Option<KeyEventArgs> Key,
    Option<PointF> Point,
    Option<UiShape> Shape) {
    public static ComponentUiContext Layout(GhComponent owner, UiShape shape) =>
        new(Phase: ComponentUiPhase.Layout, Owner: owner, Panel: None, Menu: None, Context: None, Skin: None, Capsule: None, Shade: None, Mouse: None, Key: None, Point: None, Shape: Optional(shape));

    public static ComponentUiContext Draw(GhComponent owner, UiContext context, Skin skin, Capsule capsule, Shade shade) =>
        new(Phase: ComponentUiPhase.DrawForeground, Owner: owner, Panel: None, Menu: None, Context: Optional(context), Skin: Optional(skin), Capsule: Optional(capsule), Shade: Optional(shade), Mouse: None, Key: None, Point: None, Shape: None);

    public static ComponentUiContext ForPanel(GhComponent owner, InputPanelControl panel) =>
        new(Phase: ComponentUiPhase.InputPanel, Owner: owner, Panel: Optional(panel), Menu: None, Context: None, Skin: None, Capsule: None, Shade: None, Mouse: None, Key: None, Point: None, Shape: None);

    public static ComponentUiContext ForMenu(GhComponent owner, ContextMenu menu) =>
        new(Phase: ComponentUiPhase.ContextMenu, Owner: owner, Panel: None, Menu: Optional(menu), Context: None, Skin: None, Capsule: None, Shade: None, Mouse: None, Key: None, Point: None, Shape: None);

    public static ComponentUiContext Pointer(ComponentUiPhase phase, GhComponent owner, PointF point) =>
        new(Phase: phase, Owner: owner, Panel: None, Menu: None, Context: None, Skin: None, Capsule: None, Shade: None, Mouse: None, Key: None, Point: Optional(point), Shape: None);

    public static ComponentUiContext MouseEvent(ComponentUiPhase phase, GhComponent owner, MouseEventArgs mouse) =>
        new(Phase: phase, Owner: owner, Panel: None, Menu: None, Context: None, Skin: None, Capsule: None, Shade: None, Mouse: Optional(mouse), Key: None, Point: Optional(mouse).Map(static value => value.Location), Shape: None);

    public static ComponentUiContext KeyEvent(ComponentUiPhase phase, GhComponent owner, KeyEventArgs key) =>
        new(Phase: phase, Owner: owner, Panel: None, Menu: None, Context: None, Skin: None, Capsule: None, Shade: None, Mouse: None, Key: Optional(key), Point: None, Shape: None);
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
        _ = ui.Run(context: ComponentUiContext.Layout(owner: Owner, shape: shape)).Map(decision => {
            Bounds = decision.Bounds.IfNone(Bounds);
            return unit;
        });
    }

    protected override void DrawForeground(UiContext context, Skin skin, Capsule capsule, Shade shade) {
        base.DrawForeground(context: context, skin: skin, capsule: capsule, shade: shade);
        _ = ui.Run(context: ComponentUiContext.Draw(owner: Owner, context: context, skin: skin, capsule: capsule, shade: shade));
    }

    void IContextMenuAware.AppendToMenu(ContextMenu menu) =>
        _ = ui.Run(context: ComponentUiContext.ForMenu(owner: Owner, menu: menu));

    Cursor ICursorAwareAttributes.CursorAt(PointF point) =>
        ui.Run(context: ComponentUiContext.Pointer(phase: ComponentUiPhase.Cursor, owner: Owner, point: point))
            .Map(decision => decision.Cursor.IfNone(Cursors.Default))
            .IfFail(_ => Cursors.Default);

    bool IMouseHoverAttributes.RespondToMouseHover(PointF controlPoint, PointF contentPoint) =>
        ui.Run(context: ComponentUiContext.Pointer(phase: ComponentUiPhase.Hover, owner: Owner, point: contentPoint))
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
        ui.Run(context: ComponentUiContext.MouseEvent(phase: phase, owner: Owner, mouse: mouse))
            .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
            .IfFail(_ => UiResponse.Ignored);

    private UiResponse Respond(ComponentUiPhase phase, KeyEventArgs key) =>
        ui.Run(context: ComponentUiContext.KeyEvent(phase: phase, owner: Owner, key: key))
            .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
            .IfFail(_ => UiResponse.Ignored);
}
