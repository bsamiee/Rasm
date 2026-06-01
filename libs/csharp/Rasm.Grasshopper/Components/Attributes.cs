using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo.Actions;
using GhComponent = Grasshopper2.Components.Component;
using GhLazyStrings = Grasshopper2.UI.LazyStrings;
using GhResizingFrame = Grasshopper2.UI.ResizingFrame;
using GhSettings = Grasshopper2.Settings;
using UiContext = Eto.Drawing.Context;
using UiResponse = Grasshopper2.UI.Flex.Response;
using UiShape = Grasshopper2.UI.Skinning.Shape;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly partial record struct ComponentUi {
    private readonly Seq<StepOp> ops;

    private ComponentUi(Seq<StepOp> ops) => this.ops = ops;

    public static ComponentUi Empty => default;

    public static ComponentUi operator +(ComponentUi left, ComponentUi right) =>
        Add(left: left, right: right);

    public static ComponentUi Add(ComponentUi left, ComponentUi right) =>
        new(ops: left.ops + right.ops);

    public static ComponentUi Of(Func<Callback, Fin<Decision>> run) =>
        new(ops: Seq(new StepOp(Phase: None, Run: run)));

    public static ComponentUi When(Phase phase, Func<Callback, Fin<Decision>> run) =>
        new(ops: Seq(new StepOp(Phase: Some(phase), Run: context => phase.Matches(context: context) ? run(arg: context) : Fin.Succ(value: Decision.Pass))));

    public static ComponentUi Chrome(
        Func<Callback.Draw, Fin<Decision>> draw,
        Func<Callback.Menu, Fin<Decision>>? menu = null,
        Func<Callback.Tip, Fin<Decision>>? tooltip = null) =>
        When(phase: Phase.DrawForeground, run: context => draw(arg: (Callback.Draw)context))
        + (menu is null ? Empty : When(phase: Phase.ContextMenu, run: context => menu(arg: (Callback.Menu)context)))
        + (tooltip is null ? Empty : When(phase: Phase.Tooltip, run: context => tooltip(arg: (Callback.Tip)context)));

    public static ComponentUi Resizable(Func<Callback.Frame, Fin<Decision>> resize) =>
        When(phase: Phase.Resize, run: context => resize(arg: (Callback.Frame)context));

    public static ComponentUi Editor(Func<Callback.Panel, Fin<Decision>> panel) =>
        When(phase: Phase.InputPanel, run: context => panel(arg: (Callback.Panel)context));

    public enum MouseKind { Down, Move, Up, SingleClick, DoubleClick }

    internal IAttributes Attributes(GhComponent owner) =>
        ops.IsEmpty
            ? new ComponentAttributes(owner: owner)
            : Supports(phase: Phase.Resize)
                ? new RasmResizableAttributes(owner: owner, ui: this)
                : new RasmAttributes(owner: owner, ui: this);

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

    private bool Supports(Phase phase) =>
        ops.Exists(op => op.Phase.Match(Some: candidate => candidate == phase, None: () => false));

    // Each phase self-dispatches via a Matches delegate against the Callback shape; replaces the per-case Kind override.
    [SmartEnum<int>]
    public sealed partial class Phase {
        public static readonly Phase Layout = new(key: 0, matches: static ctx => ctx is Callback.Bounds);
        public static readonly Phase DrawForeground = new(key: 1, matches: static ctx => ctx is Callback.Draw);
        public static readonly Phase InputPanel = new(key: 2, matches: static ctx => ctx is Callback.Panel);
        public static readonly Phase ContextMenu = new(key: 3, matches: static ctx => ctx is Callback.Menu);
        public static readonly Phase Cursor = new(key: 4, matches: static ctx => ctx is Callback.Pointer.Cursor);
        public static readonly Phase Hover = new(key: 5, matches: static ctx => ctx is Callback.Pointer.Hover);
        public static readonly Phase MouseDown = new(key: 6, matches: static ctx => ctx is Callback.Mouse { Kind: MouseKind.Down });
        public static readonly Phase MouseMove = new(key: 7, matches: static ctx => ctx is Callback.Mouse { Kind: MouseKind.Move });
        public static readonly Phase MouseUp = new(key: 8, matches: static ctx => ctx is Callback.Mouse { Kind: MouseKind.Up });
        public static readonly Phase MouseSingleClick = new(key: 9, matches: static ctx => ctx is Callback.Mouse { Kind: MouseKind.SingleClick });
        public static readonly Phase MouseDoubleClick = new(key: 10, matches: static ctx => ctx is Callback.Mouse { Kind: MouseKind.DoubleClick });
        public static readonly Phase KeyDown = new(key: 11, matches: static ctx => ctx is Callback.Key.Down);
        public static readonly Phase KeyUp = new(key: 12, matches: static ctx => ctx is Callback.Key.Up);
        public static readonly Phase Resize = new(key: 13, matches: static ctx => ctx is Callback.Frame);
        public static readonly Phase Tooltip = new(key: 14, matches: static ctx => ctx is Callback.Tip);
        [UseDelegateFromConstructor] internal partial bool Matches(Callback context);
    }

    public abstract record Callback(GhComponent Owner) {
        public sealed record Bounds(GhComponent Owner, UiShape Shape) : Callback(Owner: Owner);

        public sealed record Draw(GhComponent Owner, UiContext Context, Skin Skin, Capsule Capsule, Shade Shade) : Callback(Owner: Owner);

        public sealed record Panel(GhComponent Owner, InputPanel Value) : Callback(Owner: Owner);

        public sealed record Menu(GhComponent Owner, ContextMenu Value) : Callback(Owner: Owner);

        public abstract record Pointer(GhComponent Owner, PointF ContentPoint, Option<PointF> ControlPoint = default) : Callback(Owner: Owner) {
            public sealed record Cursor(GhComponent Owner, PointF ContentPoint) : Pointer(Owner: Owner, ContentPoint: ContentPoint);

            public sealed record Hover(GhComponent Owner, PointF ContentPoint, PointF Control) : Pointer(Owner: Owner, ContentPoint: ContentPoint, ControlPoint: Optional(Control));
        }

        public sealed record Mouse(GhComponent Owner, MouseKind Kind, MouseEventArgs Args) : Callback(Owner: Owner) {
            public PointF Point => Args.Location;
        }

        public abstract record Key(GhComponent Owner, KeyEventArgs Args) : Callback(Owner: Owner) {
            public sealed record Down(GhComponent Owner, KeyEventArgs Args) : Key(Owner: Owner, Args: Args);

            public sealed record Up(GhComponent Owner, KeyEventArgs Args) : Key(Owner: Owner, Args: Args);
        }

        public sealed record Frame(GhComponent Owner, SizeF Current) : Callback(Owner: Owner);

        public sealed record Tip(GhComponent Owner, PointF Point, ObjectSolutionState State) : Callback(Owner: Owner);
    }

    public readonly record struct TooltipDetail(object Native) {
        public static TooltipDetail Of(string value) => new(Native: value);
        public static TooltipDetail Of(GhLazyStrings value) => new(Native: value);
        public static TooltipDetail Of(GhLazyStrings[] value) => new(Native: value);
        public static TooltipDetail Of(Action<UiContext, Rectangle> paint) => new(Native: paint);
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct Decision(
        Option<RectangleF> Bounds,
        Option<UiResponse> Response,
        Option<Cursor> Cursor,
        Option<bool> Hover,
        Option<SizeF> Size,
        Option<TooltipDetail> Tooltip,
        bool IsTerminal) {
        public static Decision Pass => default;
        public static Decision Handled => new(Bounds: None, Response: Optional(UiResponse.Handled), Cursor: None, Hover: None, Size: None, Tooltip: None, IsTerminal: true);
        public static Decision Capture => new(Bounds: None, Response: Optional(UiResponse.Capture), Cursor: None, Hover: None, Size: None, Tooltip: None, IsTerminal: true);
        public static Decision Release => new(Bounds: None, Response: Optional(UiResponse.Release), Cursor: None, Hover: None, Size: None, Tooltip: None, IsTerminal: true);
        public static Decision WithBounds(RectangleF bounds) => new(Bounds: Optional(bounds), Response: None, Cursor: None, Hover: None, Size: None, Tooltip: None, IsTerminal: false);
        public static Decision WithCursor(Cursor cursor) => new(Bounds: None, Response: None, Cursor: Optional(cursor), Hover: None, Size: None, Tooltip: None, IsTerminal: false);
        public static Decision WithHover(bool value) => new(Bounds: None, Response: None, Cursor: None, Hover: Optional(value), Size: None, Tooltip: None, IsTerminal: false);
        public static Decision WithSize(SizeF size) => new(Bounds: None, Response: None, Cursor: None, Hover: None, Size: Optional(size), Tooltip: None, IsTerminal: false);
        public static Decision WithTooltip(TooltipDetail detail) => new(Bounds: None, Response: None, Cursor: None, Hover: None, Size: None, Tooltip: Optional(detail), IsTerminal: false);

        public static Decision operator +(Decision left, Decision right) =>
            Add(left: left, right: right);

        public static Decision Add(Decision left, Decision right) =>
            new(
                Bounds: RightBiased(right: right.Bounds, left: left.Bounds),
                Response: RightBiased(right: right.Response, left: left.Response),
                Cursor: RightBiased(right: right.Cursor, left: left.Cursor),
                Hover: RightBiased(right: right.Hover, left: left.Hover),
                Size: RightBiased(right: right.Size, left: left.Size),
                Tooltip: RightBiased(right: right.Tooltip, left: left.Tooltip),
                IsTerminal: left.IsTerminal || right.IsTerminal);

        private static Option<T> RightBiased<T>(Option<T> right, Option<T> left) =>
            right.Match(Some: Some, None: () => left);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct StepOp(Option<Phase> Phase, Func<Callback, Fin<Decision>> Run);

    // --- [COMPOSITION] ------------------------------------------------------------------------
    [BoundaryAdapter]
    private class RasmAttributes :
        ComponentAttributes,
        IContextMenuAware,
        ICursorAwareAttributes,
        IMouseHoverAttributes {
        protected readonly ComponentUi ui;

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

        Cursor ICursorAwareAttributes.CursorAt(PointF point) => ResolveCursor(point: point);

        protected Cursor ResolveCursor(PointF point) =>
            ui.Run(context: new Callback.Pointer.Cursor(Owner: Owner, ContentPoint: point))
                .Map(decision => decision.Cursor.IfNone(Cursors.Default))
                .IfFail(_ => Cursors.Default);

        bool IMouseHoverAttributes.RespondToMouseHover(PointF controlPoint, PointF contentPoint) =>
            ui.Run(context: new Callback.Pointer.Hover(Owner: Owner, ContentPoint: contentPoint, Control: controlPoint))
                .Map(decision => decision.Hover.IfNone(noneValue: false))
                .IfFail(_ => false);

        protected override object TooltipDetails(PointF point, ObjectSolutionState state) {
            object fallback = base.TooltipDetails(point: point, state: state);
            return ui.Run(context: new Callback.Tip(Owner: Owner, Point: point, State: state))
                .Map(decision => decision.Tooltip.Match(Some: static detail => detail.Native, None: () => fallback))
                .IfFail(_ => fallback);
        }

        protected override UiResponse HandleMouseDown(MouseEventArgs e) => Respond(context: new Callback.Mouse(Owner: Owner, Kind: MouseKind.Down, Args: e));
        protected override UiResponse HandleMouseMove(MouseEventArgs e) => Respond(context: new Callback.Mouse(Owner: Owner, Kind: MouseKind.Move, Args: e));
        protected override UiResponse HandleMouseUp(MouseEventArgs e) => Respond(context: new Callback.Mouse(Owner: Owner, Kind: MouseKind.Up, Args: e));
        protected override UiResponse HandleSingleClick(MouseEventArgs e) => Respond(context: new Callback.Mouse(Owner: Owner, Kind: MouseKind.SingleClick, Args: e));
        protected override UiResponse HandleDoubleClick(MouseEventArgs e) => Respond(context: new Callback.Mouse(Owner: Owner, Kind: MouseKind.DoubleClick, Args: e));
        protected override UiResponse HandleKeyDown(KeyEventArgs e) => Respond(context: new Callback.Key.Down(Owner: Owner, Args: e));
        protected override UiResponse HandleKeyUp(KeyEventArgs e) => Respond(context: new Callback.Key.Up(Owner: Owner, Args: e));

        private UiResponse Respond(Callback context) =>
            ui.Run(context: context)
                .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
                .IfFail(_ => UiResponse.Ignored);
    }

    [BoundaryAdapter]
    private sealed class RasmResizableAttributes :
        RasmAttributes,
        IResizableAttributes,
        ICursorAwareAttributes {
        private const int ResizeEdgeSize = ResizableAttributes<GhComponent>.EdgeSize;
        private readonly ResizePolicy resize = ResizePolicy.Default;
        private SizeF size;
        private GhResizingFrame? resizer;
        private Grasshopper2.Undo.Action? resizeUndo;
        private SnappingAction? priorSnapX;
        private SnappingAction? priorSnapY;
        private bool snapCaptured;

        internal RasmResizableAttributes(GhComponent owner, ComponentUi ui) : base(owner: owner, ui: ui) =>
            size = resize.Clamp(value: owner.CustomValues.Get(key: resize.Key, @default: resize.DefaultSize));

        public SizeF Size {
            get => size;
            set {
                SizeF admitted = AdmitSize(value: value);
                _ = admitted == size
                    ? unit
                    : Op.Side(() => {
                        size = admitted;
                        Owner.CustomValues.Set(key: resize.Key, value: admitted);
                        Bounds = new RectangleF(location: Pivot, size: size);
                        Invalidate();
                    });
            }
        }

        Cursor ICursorAwareAttributes.CursorAt(PointF point) =>
            ResizeCursor(point: point).IfNone(() => ResolveCursor(point: point));

        protected override void PivotMoved(PointF oldPivot, PointF newPivot) {
            Bounds = new RectangleF(location: Pivot, size: size);
            Invalidate();
        }

        protected override void LayoutBounds(UiShape shape) {
            base.LayoutBounds(shape: shape);
            Size = Owner.CustomValues.Get(key: resize.Key, @default: Bounds.Size);
        }

        protected override UiResponse HandleMouseDown(MouseEventArgs e) =>
            BeginResize(args: e).IfNone(() => base.HandleMouseDown(e: e));

        protected override UiResponse HandleMouseMove(MouseEventArgs e) =>
            ContinueResize(args: e).IfNone(() => base.HandleMouseMove(e: e));

        protected override UiResponse HandleMouseUp(MouseEventArgs e) =>
            EndResize(args: e).IfNone(() => base.HandleMouseUp(e: e));

        protected override UiResponse HandleKeyDown(KeyEventArgs e) =>
            ToggleResizeSnap(args: e).IfNone(() => base.HandleKeyDown(e: e));

        private SizeF AdmitSize(SizeF value) {
            SizeF clamped = resize.Clamp(value: value);
            return ui.Run(context: new Callback.Frame(Owner: Owner, Current: clamped))
                .Map(decision => resize.Clamp(value: decision.Size.IfNone(clamped)))
                .IfFail(clamped);
        }

        private Padding ResizeEdges() {
            Padding edges = new(all: ResizeEdgeSize);
            _ = resize.Minimum.Height == resize.Maximum.Height ? Op.Side(() => { edges.Top = 0; edges.Bottom = 0; }) : unit;
            _ = resize.Minimum.Width == resize.Maximum.Width ? Op.Side(() => { edges.Left = 0; edges.Right = 0; }) : unit;
            return edges;
        }

        private Option<Cursor> ResizeCursor(PointF point) =>
            Bounds.Contains(point, 0.5f)
                ? Optional(new GhResizingFrame(Bounds, resize.Minimum, resize.Maximum).CursorAt(mouse: point, edges: ResizeEdges()))
                : Option<Cursor>.None;

        private Option<UiResponse> BeginResize(MouseEventArgs args) =>
            args.Buttons == MouseButtons.Primary
                ? new GhResizingFrame(
                    Bounds,
                    resize.Minimum,
                    resize.Maximum,
                    SnappingConstraints.CreateFromDocument(Owner.Document, Owner.InstanceId),
                    SnappingSettings.Current) switch {
                        GhResizingFrame frame when frame.Begin(mouse: args.Location, edges: ResizeEdges()) =>
                            Op.Side(() => { CaptureSnapState(); resizer = frame; resizeUndo = new ResizeAction(obj: Owner); }) switch { _ => Some(UiResponse.Capture) },
                        _ => Op.Side(() => { resizer = null; resizeUndo = null; ClearSnapState(); }) switch { _ => Option<UiResponse>.None },
                    }
                : Option<UiResponse>.None;

        private Option<UiResponse> ContinueResize(MouseEventArgs args) =>
            (args.Buttons, resizer) switch {
                (MouseButtons.Primary, GhResizingFrame frame) => Some(ContinueResize(frame: frame, point: args.Location)),
                _ => Option<UiResponse>.None,
            };

        private UiResponse ContinueResize(GhResizingFrame frame, PointF point) {
            frame.Continue(mouse: point);
            Pivot = frame.Resized.Location;
            Size = frame.Resized.Size;
            return UiResponse.Handled;
        }

        private Option<UiResponse> EndResize(MouseEventArgs args) =>
            (args.Buttons, resizer) switch {
                (MouseButtons.Primary, GhResizingFrame) => Some(CommitResize()),
                _ => Option<UiResponse>.None,
            };

        private UiResponse CommitResize() {
            // Editor.Instance is null when no editor is running, and Document may be null mid-drag; guard both.
            _ = Optional(Grasshopper2.UI.Editor.Instance).Iter(editor => Optional(editor.Canvas).Iter(canvas => {
                canvas.SnapXAction = snapCaptured ? priorSnapX : null;
                canvas.SnapYAction = snapCaptured ? priorSnapY : null;
            }));
            _ = Optional(Owner.Document).Iter(document => document.Undo.Do((verb: "Resize", noun: Owner.Nomen.Name), resizeUndo));
            resizeUndo = null;
            resizer = null;
            ClearSnapState();
            return UiResponse.Release;
        }

        private void CaptureSnapState() {
            _ = Optional(Grasshopper2.UI.Editor.Instance).Iter(editor => Optional(editor.Canvas).Iter(canvas => {
                priorSnapX = canvas.SnapXAction;
                priorSnapY = canvas.SnapYAction;
                snapCaptured = true;
            }));
        }

        private void ClearSnapState() {
            priorSnapX = null;
            priorSnapY = null;
            snapCaptured = false;
        }

        private Option<UiResponse> ToggleResizeSnap(KeyEventArgs args) =>
            (resizer, args.Modifiers, args.Key) switch {
                (not null, Keys.None, Keys.Space) => Some(FlipResizeSnap(args: args)),
                _ => Option<UiResponse>.None,
            };

        private static UiResponse FlipResizeSnap(KeyEventArgs args) {
            GhSettings.CanvasSnapToObjects.Value = !GhSettings.CanvasSnapToObjects.Value;
            args.Handled = true;
            return UiResponse.Handled;
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly record struct ResizePolicy(string Key, SizeF DefaultSize, SizeF Minimum, SizeF Maximum) {
            internal static ResizePolicy Default { get; } = new(
                Key: "Rasm.Component.Attributes.Size",
                DefaultSize: new SizeF(width: 240f, height: 120f),
                Minimum: new SizeF(width: 64f, height: 40f),
                Maximum: new SizeF(width: 4096f, height: 4096f));
            internal SizeF Clamp(SizeF value) =>
                new(
                    width: Math.Clamp(value: MathF.Round(value.Width, MidpointRounding.ToEven), min: Minimum.Width, max: Maximum.Width),
                    height: Math.Clamp(value: MathF.Round(value.Height, MidpointRounding.ToEven), min: Minimum.Height, max: Maximum.Height));
        }
    }
}
