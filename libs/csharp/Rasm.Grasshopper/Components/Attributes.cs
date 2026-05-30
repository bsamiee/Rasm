using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using GhComponent = Grasshopper2.Components.Component;
using UiContext = Eto.Drawing.Context;
using UiResponse = Grasshopper2.UI.Flex.Response;
using UiShape = Grasshopper2.UI.Skinning.Shape;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ComponentUi {
    private readonly Seq<StepOp> ops;

    private ComponentUi(Seq<StepOp> ops) => this.ops = ops;

    public static ComponentUi Empty => default;

    public static ComponentUi operator +(ComponentUi left, ComponentUi right) =>
        Add(left: left, right: right);

    public static ComponentUi Add(ComponentUi left, ComponentUi right) =>
        new(ops: left.ops + right.ops);

    public static ComponentUi Of(Func<Callback, Fin<Decision>> run) =>
        new(ops: Seq(new StepOp(Run: run)));

    public static ComponentUi When(Phase phase, Func<Callback, Fin<Decision>> run) =>
        Of(run: context => context.Kind == phase ? run(arg: context) : Fin.Succ(value: Decision.Pass));

    internal IAttributes Attributes(GhComponent owner) =>
        ops.IsEmpty ? new ComponentAttributes(owner: owner) : new RasmAttributes(owner: owner, ui: this);

    internal Fin<Unit> Append(GhComponent owner, InputPanel panel) =>
        Run(context: new Callback.Panel(Owner: owner, Value: panel)).Map(static _ => unit);

    internal Fin<Decision> Run(Callback context) {
        Seq<StepOp> current = ops;
        return Try.lift<Fin<Decision>>(f: () => current.Fold(Fin.Succ(value: Decision.Pass), (state, op) =>
                state.Bind(decision => decision.IsTerminal ? Fin.Succ(value: decision) : op.Run(arg: context).Map(next => decision + next))))
            .Run()
            .MapFail(error => Op.Of(name: nameof(Run)).InvalidResult() + error)
            .Bind(static result => result);
    }

    public enum Phase {
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

    public abstract record Callback(GhComponent Owner) {
        public abstract Phase Kind { get; }

        public sealed record Bounds(GhComponent Owner, UiShape Shape) : Callback(Owner: Owner) {
            public override Phase Kind => Phase.Layout;
        }

        public sealed record Draw(GhComponent Owner, UiContext Context, Skin Skin, Capsule Capsule, Shade Shade) : Callback(Owner: Owner) {
            public override Phase Kind => Phase.DrawForeground;
        }

        public sealed record Panel(GhComponent Owner, InputPanel Value) : Callback(Owner: Owner) {
            public override Phase Kind => Phase.InputPanel;
        }

        public sealed record Menu(GhComponent Owner, ContextMenu Value) : Callback(Owner: Owner) {
            public override Phase Kind => Phase.ContextMenu;
        }

        public sealed record Pointer(Phase Requested, GhComponent Owner, PointF ContentPoint, Option<PointF> ControlPoint = default) : Callback(Owner: Owner) {
            public override Phase Kind => Requested;
        }

        public sealed record Mouse(Phase Requested, GhComponent Owner, MouseEventArgs Args) : Callback(Owner: Owner) {
            public override Phase Kind => Requested;
            public PointF Point => Args.Location;
        }

        public sealed record Key(Phase Requested, GhComponent Owner, KeyEventArgs Args) : Callback(Owner: Owner) {
            public override Phase Kind => Requested;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct Decision(
        Option<RectangleF> Bounds,
        Option<UiResponse> Response,
        Option<Cursor> Cursor,
        Option<bool> Hover,
        bool IsTerminal) {
        public static Decision Pass => default;
        public static Decision Handled => new(Bounds: None, Response: Optional(UiResponse.Handled), Cursor: None, Hover: None, IsTerminal: true);
        public static Decision Capture => new(Bounds: None, Response: Optional(UiResponse.Capture), Cursor: None, Hover: None, IsTerminal: true);
        public static Decision Release => new(Bounds: None, Response: Optional(UiResponse.Release), Cursor: None, Hover: None, IsTerminal: true);
        public static Decision WithBounds(RectangleF bounds) => new(Bounds: Optional(bounds), Response: None, Cursor: None, Hover: None, IsTerminal: false);
        public static Decision WithCursor(Cursor cursor) => new(Bounds: None, Response: None, Cursor: Optional(cursor), Hover: None, IsTerminal: false);
        public static Decision WithHover(bool value) => new(Bounds: None, Response: None, Cursor: None, Hover: Optional(value), IsTerminal: false);

        public static Decision operator +(Decision left, Decision right) =>
            Add(left: left, right: right);

        public static Decision Add(Decision left, Decision right) =>
            new(
                Bounds: right.Bounds | left.Bounds,
                Response: right.Response | left.Response,
                Cursor: right.Cursor | left.Cursor,
                Hover: right.Hover | left.Hover,
                IsTerminal: left.IsTerminal || right.IsTerminal);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct StepOp(Func<Callback, Fin<Decision>> Run);

    // --- [COMPOSITION] ------------------------------------------------------------------------
    [BoundaryAdapter]
    private sealed class RasmAttributes :
        ComponentAttributes,
        IContextMenuAware,
        ICursorAwareAttributes,
        IMouseHoverAttributes {
        private readonly ComponentUi ui;

        internal RasmAttributes(GhComponent owner, ComponentUi ui) : base(owner: owner) =>
            this.ui = ui;

        protected override void LayoutBounds(UiShape shape) {
            base.LayoutBounds(shape: shape);
            _ = ui.Run(context: new Callback.Bounds(Owner: Owner, Shape: shape)).Map(decision => {
                Bounds = decision.Bounds.IfNone(Bounds);
                return unit;
            });
        }

        protected override void DrawForegroundDecorations(UiContext context, Skin skin, Capsule capsule, Shade shade) {
            base.DrawForegroundDecorations(context: context, skin: skin, capsule: capsule, shade: shade);
            _ = ui.Run(context: new Callback.Draw(Owner: Owner, Context: context, Skin: skin, Capsule: capsule, Shade: shade));
        }

        void IContextMenuAware.AppendToMenu(ContextMenu menu) =>
            _ = ui.Run(context: new Callback.Menu(Owner: Owner, Value: menu));

        Cursor ICursorAwareAttributes.CursorAt(PointF point) =>
            ui.Run(context: new Callback.Pointer(Requested: Phase.Cursor, Owner: Owner, ContentPoint: point))
                .Map(decision => decision.Cursor.IfNone(Cursors.Default))
                .IfFail(_ => Cursors.Default);

        bool IMouseHoverAttributes.RespondToMouseHover(PointF controlPoint, PointF contentPoint) =>
            ui.Run(context: new Callback.Pointer(Requested: Phase.Hover, Owner: Owner, ContentPoint: contentPoint, ControlPoint: Optional(controlPoint)))
                .Map(decision => decision.Hover.IfNone(noneValue: false))
                .IfFail(_ => false);

        protected override UiResponse HandleMouseDown(MouseEventArgs e) => Respond(phase: Phase.MouseDown, mouse: e);
        protected override UiResponse HandleMouseMove(MouseEventArgs e) => Respond(phase: Phase.MouseMove, mouse: e);
        protected override UiResponse HandleMouseUp(MouseEventArgs e) => Respond(phase: Phase.MouseUp, mouse: e);
        protected override UiResponse HandleSingleClick(MouseEventArgs e) => Respond(phase: Phase.MouseSingleClick, mouse: e);
        protected override UiResponse HandleDoubleClick(MouseEventArgs e) => Respond(phase: Phase.MouseDoubleClick, mouse: e);
        protected override UiResponse HandleKeyDown(KeyEventArgs e) => Respond(phase: Phase.KeyDown, key: e);
        protected override UiResponse HandleKeyUp(KeyEventArgs e) => Respond(phase: Phase.KeyUp, key: e);

        private UiResponse Respond(Phase phase, MouseEventArgs mouse) =>
            ui.Run(context: new Callback.Mouse(Requested: phase, Owner: Owner, Args: mouse))
                .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
                .IfFail(_ => UiResponse.Ignored);

        private UiResponse Respond(Phase phase, KeyEventArgs key) =>
            ui.Run(context: new Callback.Key(Requested: phase, Owner: Owner, Args: key))
                .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
                .IfFail(_ => UiResponse.Ignored);
    }
}
