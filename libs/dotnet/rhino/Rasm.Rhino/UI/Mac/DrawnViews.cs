using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using ObjCRuntime;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Drawing;

namespace Rasm.Rhino.UI.Mac;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DrawnViews {
    // --- [LAYERS]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint HitTest(nint view, nint selector, CGPoint point);

    private static readonly HitTest PassThrough = static (_, _, _) => 0;
    private static readonly Lazy<Fin<nint>> HostClass = new(static () => NativeControls.Subclass(Class.GetHandle(typeof(NSView)),
        Seq((Selector.GetHandle("hitTest:"), (Delegate)PassThrough))));

    public static IO<Unit> Layers(Control control, CALayer layer, IPlugInSink sink, Action<IDisposable> retain) =>
        NativeControls.Lifetime(control, owned =>
            from parent in IO.lift(() => Refused.UnlessFound(control.GetContainerView(), nameof(MacControlExtensions.GetContainerView)))
            from nativeClass in IO.lift(static () => HostClass.Value)
            let site = new CallbackSite(sink, typeof(DrawnViews), nameof(Layers))
            from view in DisposalOps.Acquire(IO.lift(() => new NSView(parent.Bounds)), static held =>
                Callbacks.Each(Seq(IO.lift(held.RemoveFromSuperview), IO.lift(() => { held.Layer = null; }), IO.lift(held.Dispose))).Map(unit), owned)
            from placed in IO.lift(() => {
                _ = SafeNativeMethods.object_setClass(view.Handle, nativeClass);
                (view.Layer, view.WantsLayer, view.AutoresizingMask) = (layer, true, NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable);
                parent.AddSubview(view);
            })
            from fitted in Fit(parent, view, layer)
            from bounds in DisposalOps.Acquire(IO.lift(() => NSView.Notifications.ObserveBoundsChanged(parent,
                Callbacks.Handler<NSNotificationEventArgs>(_ => Fit(parent, view, layer), site))), owned)
            from backing in DisposalOps.Acquire(IO.lift(() => NSWindow.Notifications.ObserveDidChangeBackingProperties(
                Callbacks.Handler<NSNotificationEventArgs>(args => when(Equals(args.Notification.Object, view.Window), Fit(parent, view, layer)).As(), site))), owned)
            let update = Callbacks.Handler<EventArgs>(_ => Fit(parent, view, layer), site)
            from size in Subscriptions.Attach(
                handler => control.SizeChanged += handler, handler => control.SizeChanged -= handler, update, owned)
            from load in Subscriptions.Attach(
                handler => control.LoadComplete += handler, handler => control.LoadComplete -= handler, update, owned)
            select unit,
            new CallbackSite(sink, typeof(DrawnViews), nameof(Layers)), retain);

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
                return Refused.UnlessFound(view.Layer, nameof(NSView.Layer));
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
            AccessibilityCustomActions = NativeControls.Actions(Seq<(bool Taken, NSString Action, Func<bool> Perform)>(
                    (placement.Role.Activates, NSAccessibilityActions.PressAction, () => control.Activate(placement.Frame.Center)),
                    (placement.Role.Adjusts, NSAccessibilityActions.IncrementAction, () => control.Adjust(placement.Frame.Center, 1)),
                    (placement.Role.Adjusts, NSAccessibilityActions.DecrementAction, () => control.Adjust(placement.Frame.Center, -1)))
                .Filter(static row => row.Taken).Map(static row => (row.Action, row.Perform))),
        };

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
}
