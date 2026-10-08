using System.Numerics;
using System.Runtime.CompilerServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Drawing;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI.Components;

namespace Rasm.Rhino.UI.macOS;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DrawnViews {
    // --- [ATTACH]
    internal static void Attach(DrawableHandler handler) =>
        _ = Mounted((ComponentControl)handler.Widget, handler.Control);

    private static bool Mounted(ComponentControl control, NSView view) =>
        Callbacks.Succeeded(
            from layer in IO.lift(() => {
                view.WantsLayer = true;
                return Missing.Unless(view.Layer, nameof(NSView.Layer));
            })
            from attached in IO.lift(() => {
                layer.ContentsFormat = CAContentsFormat.Rgba16Float;
                control.Paint += Callbacks.Handler<PaintEventArgs>(_ => Folded(control, view), new CallbackSite(control.Sink, typeof(Drawable), nameof(Drawable.Paint)));
            })
            select unit,
            new CallbackSite(control.Sink, typeof(DrawableHandler), nameof(Attach)));

    // --- [ELEMENTS]
    private static readonly object Elements = new();

    private sealed record Shown((RectangleF Frame, PartRole Role, string Label) Placement, NSAccessibilityElement Element);

    private static IO<Unit> Folded(ComponentControl control, NSView view) =>
        from held in IO.lift(() => control.Properties.Get<Seq<Shown>>(Elements))
        let parts = control.Parts
        let placements = parts.Map(static part => (Frame: part.Shape.Region(part.Reach), part.Facet.Role, part.Facet.Label))
        let kept = placements == held.Map(static shown => shown.Placement)
        let next = kept ? held : placements.Map(placement => new Shown(placement, Element(control, view, placement)))
        from updated in parts.Zip(next).TraverseM(pair => Updated(control, view, pair.First.Facet, pair.Second)).As()
        from laid in unless(kept, IO.lift(() => {
            control.Properties.Set(Elements, next);
            view.AccessibilityChildren = [.. next.Map(static shown => shown.Element)];
            NSAccessibility.PostNotification(view, NSView.LayoutChangedNotification);
        })).As()
        select unit;

    private static NSAccessibilityElement Element(ComponentControl control, NSView view, (RectangleF Frame, PartRole Role, string Label) placement) =>
        new() {
            AccessibilityRole = placement.Role.Map(
                image: NSAccessibilityRoles.ImageRole,
                button: NSAccessibilityRoles.ButtonRole,
                cell: NSAccessibilityRoles.CellRole,
                handle: NSAccessibilityRoles.HandleRole,
                slider: NSAccessibilityRoles.SliderRole),
            AccessibilityLabel = placement.Label,
            AccessibilityParent = view,
            AccessibilityCustomActions = [.. Actions(control, placement.Role, placement.Frame.Center)],
        };

    private static Seq<NSAccessibilityCustomAction> Actions(ComponentControl control, PartRole role, PointF centre) =>
        Seq<(bool Taken, NSString Action, Func<bool> Perform)>(
                (role.Activates, NSAccessibilityActions.PressAction, () => control.Activate(centre)),
                (role.Adjusts, NSAccessibilityActions.IncrementAction, () => control.Adjust(centre, 1)),
                (role.Adjusts, NSAccessibilityActions.DecrementAction, () => control.Adjust(centre, -1)))
            .Filter(static row => row.Taken)
            .Choose(static row => Optional(NSAccessibility.GetActionDescription(row.Action)).Map(name => new NSAccessibilityCustomAction { Name = name, Handler = row.Perform }));

    private static IO<Unit> Updated(ComponentControl control, NSView view, PartFacet facet, Shown shown) =>
        from before in IO.lift(() => Optional(shown.Element.AccessibilityValue).Map(static value => value.ToString()))
        let frame = shown.Placement.Frame
        from written in IO.lift(() => {
            shown.Element.AccessibilityFrameInParentSpace = new CGRect(frame.X, (float)view.Bounds.Height - frame.Bottom, frame.Width, frame.Height);
            shown.Element.AccessibilityHelp = facet.Help.ValueUnsafe();
            shown.Element.AccessibilityEnabled = control.Enabled;
            shown.Element.AccessibilityValue = facet.Value.Map(static value => (NSObject)new NSString(value)).ValueUnsafe();
        })
        from posted in unless(before == facet.Value, IO.lift(() => NSAccessibility.PostNotification(shown.Element, NSView.ValueChangedNotification))).As()
        select unit;

    // --- [RASTERS]
    internal static IO<Bitmap> Raster(Control view, PixelFrame frame, Transfer encoding) =>
        IO.lift(() =>
            from host in Missing.Unless(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView))
            from layer in Missing.Unless(host.Layer, nameof(NSView.Layer))
            let samples = Samples(frame, encoding)
            from bitmap in Drawn(samples.Frame, samples.Space, layer)
            select bitmap);

    private static (PixelFrame Frame, NSString Space) Samples(PixelFrame frame, Transfer encoding) =>
        encoding.Switch(
                standard: static transfer => transfer.Curve.Map<Option<NSString>>(
                    linear: CGColorSpaceNames.ExtendedLinearSrgb,
                    srgb: CGColorSpaceNames.ExtendedSrgb,
                    pq: None,
                    hlg: None,
                    acesCct: None,
                    acesCc: None),
                gamma: static _ => None,
                camera: static _ => None)
            .Match(
                Some: space => (frame, space),
                None: () => (encoding.Light(frame), CGColorSpaceNames.ExtendedLinearSrgb));

    private static Fin<Bitmap> Drawn(PixelFrame frame, NSString name, CALayer layer) =>
        Missing.Unless(CGColorSpace.CreateWithName(name), nameof(CGColorSpace.CreateWithName)).Map(space => {
            int pixel = Unsafe.SizeOf<Vector4>();
            using CGColorSpace owned = space;
            using CGDataProvider provider = new(frame.Address, frame.Block.Length * sizeof(float), _ => GC.KeepAlive(frame));
            using CGImage image = new(
                frame.Size.Width, frame.Size.Height, sizeof(float) * 8, pixel * 8, frame.Size.Width * pixel, owned,
                CGBitmapFlags.Last | CGBitmapFlags.FloatComponents | CGBitmapFlags.ByteOrder32Little,
                provider, decode: null, shouldInterpolate: true, CGColorRenderingIntent.Default);
            layer.WantsExtendedDynamicRangeContent = true;
            return new Bitmap(new BitmapHandler(new NSImage(image, new CGSize(frame.Size.Width, frame.Size.Height))));
        });

    internal static IO<float> Headroom(Control view) =>
        IO.lift(() =>
            from host in Missing.Unless(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView))
            from window in Missing.Unless(host.Window, nameof(NSView.Window))
            from screen in Missing.Unless(window.Screen, nameof(NSWindow.Screen))
            select (float)screen.MaximumExtendedDynamicRangeColorComponentValue);
}
