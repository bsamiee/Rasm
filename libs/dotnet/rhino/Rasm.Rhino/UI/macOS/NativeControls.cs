using System.Numerics;
using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using Rasm.Imaging.ColorManagement;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rhino.UI;

namespace Rasm.Rhino.UI.macOS;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class NativeControls {
    // --- [STYLES]
    internal static void Track(SliderHandler handler) => _ = Marked((ParameterSlider)handler.Widget, handler.Control);

    internal static void Field(TextBoxHandler handler) => _ = Scrubbed((NumberField)handler.Widget, handler);

    internal static void Rows(RowGrid grid) =>
        _ = Callbacks.Answer(
            IO.lift(() => grid.Styles.Add<object>(style: null, target =>
                _ = Callbacks.Answer(
                    Optional(target as IMacViewHandler).Map(handler => Sized(grid, handler)),
                    static () => unit,
                    static () => unit,
                    new CallbackSite(grid.Sink, typeof(RowGrid), nameof(Sized))))),
            static () => unit,
            new CallbackSite(grid.Sink, typeof(RowGrid), nameof(Rows)));

    // --- [TRACK]
    private static Unit Marked(ParameterSlider slider, NSSlider native) =>
        Callbacks.Answer(
            from host in IO.lift(() => {
                native.WantsLayer = true;
                return Missing.Unless(native.Layer, nameof(NSView.Layer));
            })
            from marks in IO.lift(() => {
                CALayer layer = new() { MasksToBounds = true, MagnificationFilter = CALayer.FilterNearest, ContentsGravity = CALayer.GravityResize };
                host.AddSublayer(layer);
                native.Continuous = true;
                native.AccessibilityCustomActions = Steps(slider.Adjust);
                return layer;
            })
            let place = Callbacks.Handler<EventArgs>(_ => Placed(native, slider, marks), new CallbackSite(slider.Sink, typeof(ParameterSlider), nameof(Placed)))
            from attached in IO.lift(() => {
                slider.ValueChanged += place;
                slider.SizeChanged += place;
                slider.EnabledChanged += place;
                slider.ThemeChanged += place;
            })
            select unit,
            static () => unit,
            new CallbackSite(slider.Sink, typeof(SliderHandler), nameof(Track)));

    private static IO<Unit> Placed(NSSlider native, ParameterSlider slider, CALayer marks) =>
        from paint in IO.lift(static () => (
                Missing.Unless(NSColor.ControlAccent.UsingColorSpace(NSColorSpace.SRGBColorSpace), nameof(NSColor.UsingColorSpace)),
                Missing.Unless(NSColor.DisabledControlText.UsingColorSpace(NSColorSpace.SRGBColorSpace), nameof(NSColor.UsingColorSpace)),
                Missing.Unless(CGColorSpace.CreateWithName(CGColorSpaceNames.Srgb), nameof(CGColorSpace.CreateWithName)))
            .Apply(static (accent, disabled, space) => (Accent: accent, Disabled: disabled, Space: space)).As())
        from placed in IO.lift(() => {
            NSSliderCell cell = (NSSliderCell)native.Cell;
            CGRect bar = cell.BarRectFlipped(native.IsFlipped);
            CGRect knob = cell.KnobRectFlipped(native.IsFlipped);
            int columns = (int)Math.Round(native.ConvertRectToBacking(bar).Width, MidpointRounding.ToEven);
            using CGColorSpace space = paint.Space;
            using CGImage? image = columns > 0
                ? Image(Columns(slider, bar, knob, columns, new Vector4((float)paint.Accent.RedComponent, (float)paint.Accent.GreenComponent, (float)paint.Accent.BlueComponent, 1f), native.Enabled ? 1f : (float)paint.Disabled.AlphaComponent), space)
                : null;
            marks.Frame = bar;
            marks.CornerRadius = bar.Height / 2;
            marks.Hidden = slider.Mixed;
            marks.Contents = image;
            native.AccessibilityValueDescription = slider.ValueText;
        })
        select unit;

    private static CGImage Image(Vector4[] row, CGColorSpace space) {
        using CGDataProvider pixels = new(MemoryMarshal.AsBytes(row.AsSpan()).ToArray());
        return new CGImage(
            row.Length, 1, 32, 128, row.Length * 16, space,
            CGBitmapFlags.Last | CGBitmapFlags.FloatComponents | CGBitmapFlags.ByteOrder32Little,
            pixels, decode: null, shouldInterpolate: false, CGColorRenderingIntent.Default);
    }

    private static Vector4[] Columns(ParameterSlider slider, CGRect bar, CGRect knob, int columns, Vector4 tint, float alpha) {
        double travel = bar.Width - knob.Width;
        double low = bar.X + (knob.Width / 2);
        Seq<double> xs = toSeq(Range(0, columns)).Map(index => bar.X + ((index + 0.5) * bar.Width / columns));
        Option<(double From, double To)> span = slider.Origin.Map(origin => low + (origin * travel)).Map(start => (double.Min(start, knob.GetMidX()), double.Max(start, knob.GetMidX())));
        Vector4[] row = xs.Traverse(x => slider.Fill(double.Clamp((x - low) / travel, 0d, 1d))).As().Match(
            Some: static samples => {
                Vector4[] light = [.. samples];
                TransferCurve.Srgb.Encode(light, Nits.ReferenceWhite);
                return light;
            },
            None: () => [.. xs.Map(x => span.Exists(band => x >= band.From && x <= band.To) ? tint : Vector4.Zero)]);
        return [.. toSeq(row).Zip(xs, (pixel, x) => x >= knob.X && x <= knob.GetMaxX() ? Vector4.Zero : pixel with { W = pixel.W * alpha })];
    }

    // --- [SCRUB]
    private static Unit Scrubbed(NumberField field, TextBoxHandler handler) =>
        Callbacks.Answer(
            from held in IO.lift(static () => Atom((Held: Option<NSObject>.None, Taken: Option<NSObject>.None)))
            let site = new CallbackSite(field.Sink, typeof(NumberField), nameof(Scrubbed))
            from attached in IO.lift(() => {
                field.Scrub += Callbacks.Handler<bool>(scrubbing => scrubbing ? Held(handler, held, site) : Freed(held), site);
                field.LoadComplete += Callbacks.Handler<EventArgs>(_ => IO.lift(() => field.Font = Themes.Digits(field.Font)), site);
                handler.Control.AccessibilityCustomActions = Steps(field.Adjust);
            })
            select unit,
            static () => unit,
            new CallbackSite(field.Sink, typeof(TextBoxHandler), nameof(Field)));

    private static IO<Unit> Held(TextBoxHandler handler, Atom<(Option<NSObject> Held, Option<NSObject> Taken)> held, CallbackSite site) =>
        from monitor in IO.lift(() => Missing.Unless(
            NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyDown, key => Escaped(handler, key, site)),
            nameof(NSEvent.AddLocalMonitorForEventsMatchingMask)))
        from stored in held.SwapIO(_ => (Some(monitor), Option<NSObject>.None))
        from hidden in IO.lift(NSCursor.Hide)
        from detached in IO.lift(static () => Refused.Unless(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition(connected: false) == 0, nameof(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition)))
        select unit;

    private static IO<Unit> Freed(Atom<(Option<NSObject> Held, Option<NSObject> Taken)> held) =>
        from cleared in held.SwapIO(static state => (Option<NSObject>.None, state.Held))
        from released in cleared.Taken.Match(
            Some: static token =>
                from removed in IO.lift(() => NSEvent.RemoveMonitor(token))
                from shown in IO.lift(NSCursor.Unhide)
                from attached in IO.lift(static () => Refused.Unless(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition(connected: true) == 0, nameof(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition)))
                select unit,
            None: static () => IO.pure(unit))
        select unit;

    private static NSEvent? Escaped(TextBoxHandler handler, NSEvent key, CallbackSite site) =>
        Callbacks.Answer(IO.lift(() => key.ToEtoKeyEventArgs() is { Key: Keys.Escape } escape ? Forwarded(handler, escape, key) : key), () => key, site);

    private static NSEvent? Forwarded(TextBoxHandler handler, KeyEventArgs escape, NSEvent key) {
        handler.OnKeyDown(escape);
        return escape.Handled ? null : key;
    }

    // --- [SIZES]
    private static IO<Unit> Sized(RowGrid grid, IMacViewHandler handler) =>
        IO.lift(() => {
            NSView native = handler.ContainerControl;
            switch (native) {
                case NSControl control:
                    control.ControlSize = SizeOf(handler.Widget);
                    break;
                case NSProgressIndicator progress:
                    progress.ControlSize = SizeOf(handler.Widget);
                    break;
            }
            if (handler.Widget is Slider or ProgressBar)
                handler.Widget.Height = (int)Math.Ceiling(handler.GetAlignmentSizeForSize(native.FittingSize).Height);
            _ = grid.Caption(handler.Widget).Iter(label => native.AccessibilityTitleUIElement = label.GetContainerView());
        });

    private static NSControlSize SizeOf(Control control) =>
        toSeq(control.Parents).Exists(static parent =>
            Optional(parent.Style).Exists(static style => style.Split(' ').Contains(Panels.EtoPanelStyleName, StringComparer.Ordinal)))
            ? NSControlSize.Small
            : NSControlSize.Regular;

    // --- [STEPS]
    private static NSAccessibilityCustomAction[] Steps(Func<int, bool> adjust) =>
        [.. Seq((Action: NSAccessibilityActions.IncrementAction, Steps: 1), (Action: NSAccessibilityActions.DecrementAction, Steps: -1))
            .Choose(row => Optional(NSAccessibility.GetActionDescription(row.Action)).Map(name => new NSAccessibilityCustomAction { Name = name, Handler = () => adjust(row.Steps) }))];
}
