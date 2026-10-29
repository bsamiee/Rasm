using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;

namespace Rasm.Grasshopper.Components;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class BodyAttributes : ComponentAttributes, IResizableAttributes, ICursorAwareAttributes {
    // --- [STATE]
    private const string SizeKey = "Attr.Size";
    private const string Host = "Grasshopper2.Doc.Attributes.ModularComponentAttributes, Grasshopper2";
    private readonly ModularComponent component;
    private readonly Action<Context, Skin, Shade, RectangleF> paint;
    private readonly Option<IResponsive> widget;
    private readonly object modular;
    private Option<(ResizingFrame Frame, ResizeAction Undo, PaddingF Margins)> resize;

    public BodyAttributes(ModularComponent owner, SizeF minimum, SizeF maximum, Action<Context, Skin, Shade, RectangleF> paint, Option<IResponsive> widget) : base(owner) {
        (component, Minimum, Maximum, this.paint, this.widget) = (owner, minimum, maximum, paint, widget);
        modular = Create(owner);
        Responder.MouseDownHook += Pressed;
        Responder.MouseDragHook += Dragged;
        Responder.MouseUpHook += Released;
        Responder.KeyDownHook += Keyed;
        Responder.LostFocus += (_, _) => resize.Iter(held => Restored(held.Undo));
        _ = widget.Map(Forwarded);
    }

    // --- [LAYOUT]
    SizeF IResizableAttributes.Size {
        get => SizeF.Max(Minimum, SizeF.Min(Owner.CustomValues.Get(SizeKey, new SizeF(StandardLayout.LargeObjectWidth, StandardLayout.LargeObjectHeight)), Maximum));
        set {
            Owner.CustomValues.Set(SizeKey, SizeF.Max(Minimum, SizeF.Min(value, Maximum)).Round());
            InvalidateLayout();
        }
    }

    private SizeF Minimum => SizeF.Max(field, new SizeF(0f, (StandardLayout.ParameterMinHeight * int.Max(component.ModularInputs.VisibleCount, component.ModularOutputs.VisibleCount)) - (2f * StandardLayout.IconPadding)));

    private SizeF Maximum => SizeF.Max(Minimum, field);

    private PaddingF Margins => new(ContentBox.Left - Bounds.Left, ContentBox.Top - Bounds.Top, Bounds.Right - ContentBox.Right, Bounds.Bottom - ContentBox.Bottom);

    protected override void LayoutCentralBox(Font font) {
        ContentBox = RectangleF.FromCenter(Pivot, ((IResizableAttributes)this).Size);
        CentralBox = RectangleF.FromCenter(Pivot, ContentBox.Size + new SizeF(2f * StandardLayout.IconPadding, 2f * StandardLayout.IconPadding));
        _ = widget.Iter(held => held.Responder.RegionBoundary = ContentBox);
    }

    protected override void LayoutInputParameters(Font font) {
        LayoutInputParameters(font, component.ModularInputs.VisibleParameters, component.ModularInputs.VisibleCount);
        _ = toSeq(component.ModularInputs.HiddenParameters).Iter(hidden => Optional(hidden.Attributes as InputParameterAttributes).Iter(held => held.AssignBounds(new RectangleF(Pivot, SizeF.Empty))));
    }

    protected override void LayoutOutputParameters(Font font) {
        LayoutOutputParameters(font, component.ModularOutputs.VisibleParameters, component.ModularOutputs.VisibleCount);
        _ = toSeq(component.ModularOutputs.HiddenParameters).Iter(hidden => Optional(hidden.Attributes as OutputParameterAttributes).Iter(held => held.AssignBounds(new RectangleF(Pivot, SizeF.Empty))));
    }

    // --- [DRAWING]
    protected override void DrawContent(Context context, Skin skin, Capsule capsule, Shade shade) => Hooks.Run(IO.lift(() => paint(context, skin, shade, ContentBox)), unit);

    protected override void DrawForeground(Context context, Skin skin, Capsule capsule, Shade shade) {
        ColourInputCategories(modular, context, capsule, component.ModularInputs);
        ColourOutputCategories(modular, context, capsule, component.ModularOutputs);
        base.DrawForeground(context, skin, capsule, shade);
    }

    protected override void DrawForegroundDecorations(Context context, Skin skin, Capsule capsule, Shade shade) => DrawForegroundDecorations(modular, context, skin, capsule, shade);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(Host)]
    private static extern object Create(ModularComponent component);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ColourInputCategories))]
    private static extern void ColourInputCategories([UnsafeAccessorType(Host)] object attributes, Context context, Capsule capsule, ModularList input);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ColourOutputCategories))]
    private static extern void ColourOutputCategories([UnsafeAccessorType(Host)] object attributes, Context context, Capsule capsule, ModularList output);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(DrawForegroundDecorations))]
    private static extern void DrawForegroundDecorations([UnsafeAccessorType(Host)] object attributes, Context context, Skin skin, Capsule capsule, Shade shade);

    // --- [RESIZING]
    private Padding Bands() {
        (SizeF low, SizeF high) = (Minimum, Maximum);
        (int horizontal, int vertical) = (low.Width == high.Width ? 0 : ResizableAttributes<Component>.EdgeSize, low.Height == high.Height ? 0 : ResizableAttributes<Component>.EdgeSize);
        return new Padding(horizontal, vertical, horizontal, vertical);
    }

    private Response Pressed(ResponseMouseArgs args) {
        if (args.Buttons is not MouseButtons.Primary)
            return Response.Ignored;
        PaddingF margins = Margins;
        ResizingFrame frame = new(Bounds, Minimum + margins.Size, Maximum + margins.Size, SnappingConstraints.CreateFromDocument(Owner.Document, Owner.InstanceId), SnappingSettings.Current);
        resize = frame.Begin(args.ContentLocation, Bands()) ? Some((frame, new ResizeAction(Owner), margins)) : None;
        return resize.IsSome ? Response.Capture : Response.Ignored;
    }

    private Response Dragged(ResponseMouseArgs args) =>
        resize.Filter(_ => args.Buttons is MouseButtons.Primary).Map(held => {
            held.Frame.Continue(args.ContentLocation);
            return Fitted(held.Frame.Resized.Size - held.Margins.Size, RectangleF.Inset(held.Frame.Resized, held.Margins).Center, Response.Handled);
        }).IfNone(Response.Ignored);

    private Response Released(ResponseMouseArgs args) =>
        resize.Filter(_ => args.Buttons is MouseButtons.Primary).Map(held => {
            _ = Ended();
            _ = Optional(Owner.Document).Iter(document => document.Undo.Do(new VerbNoun("Resize", Owner.Nomen.Name), held.Undo));
            return Response.Release;
        }).IfNone(Response.Ignored);

    private Response Keyed(KeyEventArgs args) =>
        resize.Map(held => (args.Key, args.Modifiers) switch {
            (Keys.Escape, _) => Restored(held.Undo),
            (Keys.S, Keys.None) => Toggled(args),
            _ => Response.Ignored,
        }).IfNone(Response.Ignored);

    private Response Restored(ResizeAction undo) {
        _ = Ended();
        return Fitted(undo.Size, undo.Pivot, Response.Release);
    }

    private Response Fitted(SizeF size, PointF pivot, Response response) {
        ((IResizableAttributes)this).Size = size;
        Pivot = pivot;
        return response;
    }

    private static Response Toggled(KeyEventArgs args) {
        Settings.CanvasSnapToObjects.Value = !Settings.CanvasSnapToObjects.Value;
        args.Handled = true;
        return Response.Handled;
    }

    private Unit Ended() {
        resize = None;
        return Optional(Editor.Instance).Iter(static editor => {
            editor.Canvas.SnapXAction = null;
            editor.Canvas.SnapYAction = null;
        });
    }

    Cursor? ICursorAwareAttributes.CursorAt(PointF point) {
        PaddingF margins = Margins;
        return Bounds.Contains(point, 0.5f) ? new ResizingFrame(Bounds, Minimum + margins.Size, Maximum + margins.Size).CursorAt(point, Bands()) : null;
    }

    // --- [WIDGET]
    private bool Inside(ResponseMouseArgs args) => ContentBox.Contains(args.ContentLocation);

    private Unit Forwarded(IResponsive held) {
        Responses target = held.Responder;
        Responder.MouseDownHook += args => Inside(args) ? target.MouseDown(args) : Response.Ignored;
        Responder.MouseSingleClickHook += args => Inside(args) ? target.MouseSingleClick(args) : Response.Ignored;
        Responder.MouseDoubleClickHook += args => Inside(args) ? target.MouseDoubleClick(args) : Response.Ignored;
        Responder.MouseWheelHook += args => Inside(args) ? target.MouseWheel(args) : Response.Ignored;
        Responder.MouseOverHook += args => {
            if (Inside(args))
                target.MouseOver(args);
        };
        Responder.MouseDragHook += target.MouseDrag;
        Responder.MouseUpHook += target.MouseUp;
        Responder.MouseLeaveHook += target.MouseLeave;
        Responder.KeyDownHook += target.KeyDown;
        Responder.KeyUpHook += target.KeyUp;
        Responder.TextInputHook += target.TextInput;
        Responder.RotationHook += target.Rotation;
        return unit;
    }
}
