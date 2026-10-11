using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Drawing;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using ObjCRuntime;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Components;

namespace Rasm.Rhino.UI.macOS;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DrawnViews {
    // --- [LAYERS]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint HitTest(nint view, nint selector, CGPoint point);

    private static readonly HitTest PassThrough = static (_, _, _) => 0;
    private static readonly Lazy<Fin<nint>> HostClass = new(static () => NativeControls.Subclass(Class.GetHandle(typeof(NSView)),
        Seq((Selector.GetHandle("hitTest:"), (Delegate)PassThrough))));

    public static IO<Unit> Layers(Control control, CALayer layer, IPlugInSink sink, Action<IDisposable> retain) {
        Action<NSView> release = static held => DisposalOps.Release(Seq<IDisposable>(held,
            new Disposal<NSView>(held, static view => view.Layer = null),
            new Disposal<NSView>(held, static view => view.RemoveFromSuperview()))).Run();
        return NativeControls.Lifetime(control, owned =>
            from parent in IO.lift(() => Missing.Unless(control.GetContainerView(), nameof(MacControlExtensions.GetContainerView)))
            from nativeClass in IO.lift(static () => HostClass.Value)
            let site = new CallbackSite(sink, typeof(DrawnViews), nameof(Layers))
            from view in DisposalOps.Acquire(IO.lift(() => new NSView(parent.Bounds)), held => IO.lift(() => release(held)), held => {
                _ = SafeNativeMethods.object_setClass(held.Handle, nativeClass);
                held.Layer = layer;
                held.WantsLayer = true;
                held.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
                parent.AddSubview(held);
                owned(new Disposal<NSView>(held, release));
            })
            from fitted in Fit(parent, view, layer)
            from bounds in DisposalOps.Acquire(IO.lift(() => NSView.Notifications.ObserveBoundsChanged(parent,
                (_, _) => Callbacks.Run(Fit(parent, view, layer), site))), static token => IO.lift(token.Dispose), owned)
            from backing in DisposalOps.Acquire(IO.lift(() => NSWindow.Notifications.ObserveDidChangeBackingProperties((_, args) => {
                if (Equals(args.Notification.Object, view.Window))
                    Callbacks.Run(Fit(parent, view, layer), site);
            })), static token => IO.lift(token.Dispose), owned)
            let update = Callbacks.Handler<EventArgs>(_ => Fit(parent, view, layer), site)
            from size in Subscriptions.Attach(
                handler => control.SizeChanged += handler, handler => control.SizeChanged -= handler, update, owned)
            from load in Subscriptions.Attach(
                handler => control.LoadComplete += handler, handler => control.LoadComplete -= handler, update, owned)
            select unit,
            new CallbackSite(sink, typeof(DrawnViews), nameof(Layers)), retain);
    }

    private static IO<Unit> Fit(NSView parent, NSView view, CALayer layer) =>
        DisposalOps.Within(static () => { CATransaction.Begin(); return unit; },
            _ => IO.lift(() => {
                CATransaction.DisableActions = true;
                view.Frame = parent.Bounds;
                layer.Frame = view.Bounds;
                layer.GeometryFlipped = true;
                _ = Scaled(layer, view.ConvertSizeToBacking(new CGSize(1, 1)).Width);
            }), static _ => IO.lift(CATransaction.Commit));

    private static Unit Scaled(CALayer layer, NFloat scale) {
        layer.ContentsScale = scale;
        _ = Optional(layer.Sublayers).Iter(children => toSeq(children).Iter(child => Scaled(child, scale)));
        _ = Optional(layer.Mask).Iter(mask => Scaled(mask, scale));
        return unit;
    }

    // --- [ATTACH]
    internal static void Attach(DrawableHandler handler) {
        ComponentControl control = (ComponentControl)handler.Widget;
        NSView view = handler.Control;
        Callbacks.Run(
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
    }

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
    internal static IO<Bitmap> Raster(Control view, PixelFrame frame, Transfer encoding, Action<Bitmap> retain) =>
        from host in IO.lift(() => Missing.Unless(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView)))
        from layer in IO.lift(() => Missing.Unless(host.Layer, nameof(NSView.Layer)))
        let samples = Samples(frame, encoding)
        from bitmap in Drawn(samples.Frame, samples.Space, layer, retain)
        select bitmap;

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

    private static IO<Bitmap> Drawn(PixelFrame frame, NSString name, CALayer layer, Action<Bitmap> retain) =>
        DisposalOps.Acquire(owned =>
            DisposalOps.Within<IDisposable, Bitmap>(temporary =>
                from space in DisposalOps.Acquire(IO.lift(() => Missing.Unless(CGColorSpace.CreateWithName(name), nameof(CGColorSpace.CreateWithName))), static held => IO.lift(held.Dispose), temporary)
                from provider in DisposalOps.Acquire(IO.lift(() => new CGDataProvider(frame.Address, frame.Block.Length * sizeof(float), _ => GC.KeepAlive(frame))), static held => IO.lift(held.Dispose), temporary)
                let pixel = Unsafe.SizeOf<Vector4>()
                from image in DisposalOps.Acquire(IO.lift(() => new CGImage(
                    frame.Size.Width, frame.Size.Height, sizeof(float) * 8, pixel * 8, frame.Size.Width * pixel, space,
                    CGBitmapFlags.Last | CGBitmapFlags.FloatComponents | CGBitmapFlags.ByteOrder32Little,
                    provider, decode: null, shouldInterpolate: true, CGColorRenderingIntent.Default)), static held => IO.lift(held.Dispose), temporary)
                from enabled in IO.lift(() => {
                    if (OperatingSystem.IsMacOSVersionAtLeast(26)) {
                        layer.ContentsHeadroom = image.CalculatedContentHeadroom;
                        layer.PreferredDynamicRange = CADynamicRange.High;
                    } else
                        layer.WantsExtendedDynamicRangeContent = true;
                })
                from drawing in DisposalOps.Acquire(IO.lift(() => {
                    NSImage native = new(image, new CGSize(frame.Size.Width, frame.Size.Height));
                    return (Image: native, Ownership: new Disposal<NSImage>(native, static held => held.Dispose()));
                }), static held => IO.lift(held.Ownership.Dispose), held => temporary(held.Ownership))
                from bitmap in DisposalOps.Acquire(IO.lift(() => new Bitmap(new BitmapHandler(drawing.Image))), static held => IO.lift(held.Dispose), held => {
                    drawing.Ownership.Relinquish();
                    owned(held);
                })
                select bitmap, DisposalOps.Release), (bitmap, _) => retain(bitmap));

    internal static IO<float> Headroom(Control view) =>
        IO.lift(() =>
            from host in Missing.Unless(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView))
            from window in Missing.Unless(host.Window, nameof(NSView.Window))
            from screen in Missing.Unless(window.Screen, nameof(NSWindow.Screen))
            select (float)screen.MaximumExtendedDynamicRangeColorComponentValue);
}
