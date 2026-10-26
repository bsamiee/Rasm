using System.Runtime.CompilerServices;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Rhino.UI.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate SKColorType BitmapOrder();

internal delegate Fin<Unit> ExtendedPresent(Control view, Graphics graphics, SKImage frame, RectangleF destination);

internal delegate IO<Unit> SlotSeated(Control slot, Option<string> label);

internal delegate IO<Func<Mount, Output, IO<Unit>>> NativeScene(SceneView view, Action<IDisposable> retain);

internal delegate IO<Unit> OpaqueTarget(SceneView view, Action<IDisposable> retain);

internal delegate IO<OpaqueTarget> OpaqueTargets(IPlugInSink owner, Action<IDisposable> retain);

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SceneTarget {

    public required SKImageInfo Info { get; init; }

    public SKCanvas Canvas => Switch(standard: static standard => standard.Surface.Canvas, extended: static extended => extended.Surface.Canvas);

    public bool Ranged => Map(standard: false, extended: true);

    internal static SKSurfaceProperties Properties { get; } = new(SKPixelGeometry.Unknown);

    internal IO<Unit> Released =>
        Switch(
            standard: static standard => Callbacks.Each(Seq(IO.lift(standard.Surface.Dispose), IO.lift(standard.Data.Dispose), IO.lift(standard.Bitmap.Dispose))).Map(static _ => unit),
            extended: static extended => IO.lift(extended.Surface.Dispose));

    internal static IO<SceneTarget> Allocated(SKSizeI pixels, bool ranged) => ranged ? Floated(pixels) : Bitmapped(pixels);

    private static IO<SceneTarget> Bitmapped(SKSizeI pixels) =>
        from order in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<BitmapOrder>(), nameof(BitmapOrder)).Map(static row => row()()))
        from target in IO.lift(() => {
            Bitmap bitmap = new(pixels.Width, pixels.Height, PixelFormat.Format32bppRgba);
            BitmapData data = bitmap.Lock();
            SKImageInfo info = new(pixels.Width, pixels.Height, order, data.PremultipliedAlpha ? SKAlphaType.Premul : SKAlphaType.Unpremul, SKColorSpace.CreateSrgb());
            return Refused.UnlessFound(SKSurface.Create(info, data.Data, data.ScanWidth, Properties), nameof(SKSurface.Create))
                .Map(surface => (SceneTarget)new Standard(bitmap, data, surface) { Info = info })
                .MapFail(error => {
                    data.Dispose();
                    bitmap.Dispose();
                    return error;
                });
        })
        select target;

    private static IO<SceneTarget> Floated(SKSizeI pixels) =>
        IO.lift(() => new SKImageInfo(pixels.Width, pixels.Height, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgbLinear()) switch {
            var info => Refused.UnlessFound(SKSurface.Create(info, Properties), nameof(SKSurface.Create)).Map(surface => (SceneTarget)new Extended(surface) { Info = info }),
        });

    public sealed record Standard(Bitmap Bitmap, BitmapData Data, SKSurface Surface) : SceneTarget;

    public sealed record Extended(SKSurface Surface) : SceneTarget;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneView : Drawable {
    // --- [STATE]
    private sealed record Seat(Hosted.Slot Slot, PixelLayout Holder, Control Control);

    private sealed record Mounted(Mount Mount, Shipped Shipped, Shipped Stated, TimeProvider Clock, Func<bool, IO<Unit>> Running, Func<Mount, Output, IO<Unit>> Native) {
        public Option<(SceneTarget Target, PaintCache Cache)> Raster { get; init; }
        public HashMap<Seq<Key>, Seat> Seats { get; init; }
        public Option<Rasm.UI.Cursor> Pointer { get; init; }
    }

    private readonly RowScope scope;
    private readonly SKRegion pending = new();
    private readonly PixelLayout seats = new();
    private Node view;
    private Option<IDisposable> listening;
    private Option<Mounted> mounted;
    private (bool Shown, bool Key) native;
    private int marking;

    public SceneView(RowScope scope, Node view) {
        (this.scope, this.view) = (scope, view);
        Content = seats;
        CanFocus = true;
        Style = HandlerStyle;
        Callbacks.Run(ViewOps.WhileLoaded(this, Listened, Site(nameof(ViewOps.WhileLoaded)), lease => listening = Some(lease)), Site());
    }

    public static string HandlerStyle { get; } = $"{typeof(SceneView).Assembly.GetName().Name}.{nameof(SceneView)}";

    public bool Cycles { get; init; }

    public Func<Seq<Key>, IO<Control>> Slots { get; init; } = static path => IO.fail<Control>(new Refused(nameof(Slots), $"no control for slot /{string.Join('/', path.Map(static key => key.ToValue()))}"));

    internal Option<Mount> Current => mounted.Map(static held => held.Mount);

    internal IPlugInSink Sink => scope.Sink;

    public static PlugInSlot<(Shipped Shipped, TimeProvider Clock)> Shipping { get; } = new();

    internal static PlugInSlot<OpaqueTarget> Opaque { get; } = new();

    internal static OpaqueTarget Bitmapped { get; } = static (_, _) => IO.pure(unit);

    // --- [SHOW]
    public IO<Unit> Show(Node next) =>
        IO.lift(() => view = next).Bind(_ => mounted.Traverse(held =>
            from now in Now(held.Clock)
            from moved in IO.lift(() => held.Mount.Show(next, now))
            from applied in Applied(held, moved)
            select unit).As()).Map(static _ => unit);

    // --- [SESSION]
    public static Seq<Func<IPlugInSink, Action<IDisposable>, IO<Unit>>> Opened(ViewCatalog catalog, TimeProvider clock) =>
        catalog.Rows.Bind(ViewCatalog.Tree).Exists(static held => held.Children.Exists(static child => !child.Controls.IsEmpty))
            ? [(owner, retain) => Published(owner, clock, retain)]
            : [];

    private static IO<Unit> Published(IPlugInSink owner, TimeProvider clock, Action<IDisposable> retain) =>
        from shipped in IO.lift(() => Decoded(owner, clock)).Bind(static published => published)
            .Catch(static error => error.IsExceptional, static error => IO.fail<Unit>(new UnheldView(typeof(SceneView), error)))
        from opaque in (from targets in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<OpaqueTargets>(), nameof(OpaqueTargets)))
                        from target in targets()(owner, retain)
                        from published in Opaque.Publish(owner, target)
                        select unit).MapFail(static error => new UnheldView(typeof(SceneView), error))
        select unit;

    private static IO<Unit> Decoded(IPlugInSink owner, TimeProvider clock) => Rasm.UI.Styling.Styles.Embedded.Bind(shipped => Shipping.Publish(owner, (shipped, clock)));

    // --- [LIFETIME]
    private IO<Unit> Listened(Action<IDisposable> retain) =>
        from host in Shipping.Of(scope.Sink)
        from link in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<FrameLink>(), nameof(FrameLink)))
        from running in link()(this, scope.Sink)(new Ticks(this), retain)
        from projection in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<NativeScene>(), nameof(NativeScene)))
        from native in projection()(this, retain)
        from created in Created(host.Shipped, host.Clock, running, native)
        from released in DisposalOps.Acquire(IO.pure(unit), _ => Released, retain)
        from theme in HostTheme.In(this).Inline(_ => Rehosted, scope.Sink, retain)
        from visible in scope.OnVisibility(_ => Rehosted, retain)
        select unit;

    private IO<Unit> Created(Shipped shipped, TimeProvider clock, Func<bool, IO<Unit>> running, Func<Mount, Output, IO<Unit>> native) =>
        from stated in IO.lift(() => HostTheme.Stated(shipped, HostTheme.Compact(this)))
        from host in Hosting
        from made in IO.lift(() => Mount.Create(host, stated, view, []))
        from applied in Applied(new Mounted(made.Mount, shipped, stated, clock, running, native), made)
        select unit;

    private IO<Unit> Rehosted =>
        mounted.Traverse(held =>
            from host in Hosting
            from stated in IO.lift(() => HostTheme.Stated(held.Shipped, HostTheme.Compact(this)))
            from now in Now(held.Clock)
            from moved in stated.Equals(held.Stated)
                ? IO.pure(held.Mount.Rehost(host, now))
                : IO.lift(() => Mount.Create(host, stated, view, held.Mount.Scene.Adjusted))
            from applied in Applied(held with { Stated = stated }, moved)
            select unit).As().Map(static _ => unit);

    private IO<Unit> Released =>
        IO.lift(() => {
            Option<Mounted> held = mounted;
            mounted = None;
            pending.SetEmpty();
            return held;
        }).Bind(held => held.Match(
            Some: last => Callbacks.Each(
                last.Raster.ToSeq().Bind(static raster => Seq(raster.Target.Released, IO.lift(raster.Cache.Dispose)))
                    .Concat(last.Seats.Values.ToSeq().Map(seat => IO.lift(() => Unseated(seat))))
                    .Add(last.Running(false))).Map(static _ => unit),
            None: static () => IO.pure(unit)));

    private IO<Host> Hosting =>
        scope.Visibility.Map(visible => new Host(
            new SKSize(Math.Max(0, Width), Math.Max(0, Height)),
            HostTheme.Projected(HostTheme.Compact(this)),
            !(visible && native.Shown) ? Visibility.Hidden : native.Key ? Visibility.Active : Visibility.Shown,
            Cycles,
            RowText.Culture));

    internal IO<Unit> Natively(bool shown, bool key) => IO.lift(() => native = (shown, key)).Bind(_ => Rehosted);

    internal void Marked(int offset) => marking = offset;

    // --- [OUTPUT]
    private IO<bool> Applied(Mounted held, (Mount Mount, Output Output) moved) =>
        from damaged in IO.lift(() => Damaged(held, moved.Output.Damage))
        from placed in Seated(held.Seats, moved.Mount)
        from cursor in when(held.Pointer != Some(moved.Mount.Cursor), IO.lift(() => HostInput.Cursor(moved.Mount.Cursor)).Map(shown => Cursor = shown).Map(static _ => unit)).As()
        from stored in IO.lift(() => mounted = Some(held with { Mount = moved.Mount, Seats = placed, Pointer = moved.Mount.Cursor }))
        from running in held.Running(moved.Mount.Animating)
        from projected in held.Native(moved.Mount, moved.Output)
        from fitted in IO.lift(() => Fitted(moved.Mount.Scene))
        from answered in moved.Output.Answer.IfNone(IO.pure(unit))
        select moved.Output.Answer.IsSome;

    private Unit Damaged(Mounted held, Seq<SKRect> damage) {
        Option<float> scale = Optional(ParentWindow).Map(static window => window.LogicalPixelSize);
        _ = damage.Iter(rect => {
            _ = scale.Match(
                Some: device => pending.Op(SKRectI.Ceiling(SKRect.Create(rect.Left * device, rect.Top * device, rect.Width * device, rect.Height * device), outwards: true), SKRegionOperation.Union),
                None: () => held.Raster.Exists(raster => pending.SetRect(SKRectI.Create(raster.Target.Info.Size))));
            Invalidate(Rectangle.Ceiling(new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height)));
        });
        return unit;
    }

    private Unit Fitted(Scene scene) {
        float preferred = scene.Preferred(new SKSize(Math.Max(0, Width), float.PositiveInfinity)).Height;
        if (float.IsFinite(preferred) && (int)MathF.Ceiling(preferred) != Height) Height = (int)MathF.Ceiling(preferred);
        return unit;
    }

    // --- [SLOTS]
    private IO<HashMap<Seq<Key>, Seat>> Seated(HashMap<Seq<Key>, Seat> held, Mount mount) =>
        toHashMap(mount.Scene.Hosted.Choose(static hosted => hosted.Switch(slot: static slot => Some(slot), backdrop: static _ => Option<Hosted.Slot>.None)).Map(static slot => (slot.Path, slot))) switch {
            var wanted => toTrackingHashMap(held.Map(static seat => seat.Slot).AsIterable().Map(static entry => (entry.Key, entry.Value))).Snapshot()
                .RemoveRange(held.Keys.Filter(path => !wanted.ContainsKey(path)))
                .AddOrUpdateRange(wanted.AsIterable().Map(static entry => (entry.Key, entry.Value)))
                .Changes.AsIterable().ToSeq()
                .Fold(IO.pure(held), (sequence, change) => sequence.Bind(seated => Changed(seated, change.Key, change.Value, Elements(mount.Tree)))),
        };

    private IO<HashMap<Seq<Key>, Seat>> Changed(HashMap<Seq<Key>, Seat> seated, Seq<Key> path, Change<Hosted.Slot> change, Seq<Element> elements) =>
        change switch {
            EntryAdded<Hosted.Slot> added =>
                from control in Slots(path)
                from seat in IO.lift(() => {
                    PixelLayout holder = new();
                    holder.Add(control, Offset(added.Value));
                    seats.Add(holder, Placed(added.Value));
                    return Resized(new Seat(added.Value, holder, control));
                })
                from sized in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<SlotSeated>(), nameof(SlotSeated)).Map(static row => row()))
                    .Bind(row => row(control, elements.Find(element => element.Id == path).Bind(static element => element.Accessible.Label)))
                select seated.AddOrUpdate(path, seat),
            EntryMappedTo<Hosted.Slot> mapped => IO.lift(() => seated.Find(path).Match(
                Some: seat => {
                    seats.Move(seat.Holder, Placed(mapped.To));
                    seat.Holder.Move(seat.Control, Offset(mapped.To));
                    return seated.AddOrUpdate(path, Resized(seat with { Slot = mapped.To }));
                },
                None: () => seated)),
            EntryRemoved<Hosted.Slot> => IO.lift(() => seated.Find(path).Match(
                Some: seat => {
                    _ = Unseated(seat);
                    return seated.Remove(path);
                },
                None: () => seated)),
            _ => IO.pure(seated),
        };

    private static Eto.Drawing.Point Placed(Hosted.Slot slot) => new((int)MathF.Floor(slot.Clip.Left), (int)MathF.Floor(slot.Clip.Top));

    private static Eto.Drawing.Point Offset(Hosted.Slot slot) => new((int)MathF.Round(slot.Frame.Left - slot.Clip.Left, MidpointRounding.ToEven), (int)MathF.Round(slot.Frame.Top - slot.Clip.Top, MidpointRounding.ToEven));

    private static Seat Resized(Seat seat) {
        seat.Holder.Size = Size.Ceiling(new SizeF(seat.Slot.Clip.Width, seat.Slot.Clip.Height));
        seat.Control.Size = Size.Ceiling(new SizeF(seat.Slot.Frame.Width, seat.Slot.Frame.Height));
        return seat;
    }

    private Unit Unseated(Seat seat) {
        seats.Remove(seat.Holder);
        seat.Holder.Remove(seat.Control);
        seat.Control.Dispose();
        seat.Holder.Dispose();
        return unit;
    }

    // --- [PAINT]
    protected override void OnPaint(PaintEventArgs e) {
        Callbacks.Run(Painted(e.Graphics), Site());
        base.OnPaint(e);
    }

    private IO<Unit> Painted(Graphics graphics) =>
        mounted.Traverse(held =>
            from scale in IO.lift(() => Refused.UnlessFound(ParentWindow, nameof(ParentWindow)).Map(static window => window.LogicalPixelSize))
            let pixels = new SKSizeI((int)MathF.Ceiling(Width * scale), (int)MathF.Ceiling(Height * scale))
            from painted in when(pixels is { Width: > 0, Height: > 0 },
                from raster in Rastered(held, pixels, held.Mount.Scene.Extended)
                from now in Now(held.Clock)
                from drawn in IO.lift(() => Drawn(raster.Target, raster.Cache, held.Mount, scale, now))
                from shown in Blitted(graphics, raster.Target)
                select unit).As()
            select unit).As().Map(static _ => unit);

    private IO<(SceneTarget Target, PaintCache Cache)> Rastered(Mounted held, SKSizeI pixels, bool ranged) =>
        held.Raster.Filter(raster => raster.Target.Info.Size == pixels && raster.Target.Ranged == ranged).Match(
            Some: static raster => IO.pure(raster),
            None: () =>
                from released in held.Raster.Traverse(static raster => raster.Target.Released.Bind(_ => IO.lift(raster.Cache.Dispose))).As()
                from target in SceneTarget.Allocated(pixels, ranged)
                let raster = (Target: target, Cache: new PaintCache(target.Info, SceneTarget.Properties))
                from stored in IO.lift(() => {
                    _ = pending.SetRect(SKRectI.Create(pixels));
                    mounted = mounted.Map(current => current with { Raster = Some(raster) });
                })
                select raster);

    private Unit Drawn(SceneTarget target, PaintCache cache, Mount mount, float scale, Duration now) {
        if (pending.IsEmpty) return unit;
        SKCanvas canvas = target.Canvas;
        canvas.ResetMatrix();
        canvas.Scale(scale);
        Painter.Paint(canvas, cache, mount, pending, now);
        canvas.Flush();
        pending.SetEmpty();
        return unit;
    }

    private IO<Unit> Blitted(Graphics graphics, SceneTarget target) =>
        target.Switch(
            (View: this, Graphics: graphics, Destination: new RectangleF(0f, 0f, Width, Height)),
            standard: static (held, standard) => IO.lift(() => held.Graphics.DrawImage(standard.Bitmap, new RectangleF(0f, 0f, standard.Info.Width, standard.Info.Height), held.Destination)),
            extended: static (held, extended) =>
                from present in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<ExtendedPresent>(), nameof(ExtendedPresent)).Map(static row => row()))
                from shown in IO.lift(() => present(held.View, held.Graphics, extended.Surface.Snapshot(), held.Destination))
                select unit);

    // --- [INPUT]
    internal bool Receive(Input input, Duration now) =>
        Callbacks.Answer(
            mounted.Match(
                Some: held => IO.lift(() => held.Mount.Receive(input, now)).Bind(moved => Applied(held, moved)),
                None: static () => IO.pure(value: false)),
            static () => false,
            Site());

    private bool Received(Fin<Input> input) =>
        Callbacks.Answer(from held in IO.lift(() => input) from now in IO.lift(static () => HostInput.Now) select Receive(held, now), static () => false, Site());

    internal bool Timed(Input input) =>
        Callbacks.Answer(mounted.Match(Some: held => Now(held.Clock).Map(now => Receive(input, now)), None: static () => IO.pure(value: false)), static () => false, Site());

    protected override void OnMouseDown(MouseEventArgs e) {
        e.Handled = Received(HostInput.Clicks.Map(clicks => HostInput.Pressed(e, clicks)));
        if (e.Handled && !HasFocus) Focus();
        if (!e.Handled) base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        e.Handled = Received(HostInput.Clicks.Map(clicks => HostInput.Moved(e, clicks)));
        if (!e.Handled) base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        e.Handled = HostInput.Clicks.Map(clicks => HostInput.Released(e, clicks)).Match(Succ: held => held.Exists(input => Received(input)), Fail: static _ => false);
        if (!e.Handled) base.OnMouseUp(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        _ = Timed(HostInput.Left(e));
        base.OnMouseLeave(e);
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        _ = Timed(new Input.Focused());
    }

    protected override void OnLostFocus(EventArgs e) {
        _ = Timed(new Input.Blurred());
        base.OnLostFocus(e);
    }

    protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        Callbacks.Run(Rehosted, Site());
    }

    // --- [KEYS]
    protected override void OnKeyDown(KeyEventArgs e) {
        e.Handled = mounted.Exists(held => HostInput.Keyed(held.Mount, e).Exists(Timed));
        if (!e.Handled) base.OnKeyDown(e);
    }

    protected override void OnTextInput(TextInputEventArgs e) {
        e.Cancel = Timed(new Input.Perform(new Act.Insert(e.Text), new Target.Focused()));
        base.OnTextInput(e);
    }

    protected override void OnTextComposition(TextCompositionEventArgs e) {
        e.Handled = Timed(new Input.Perform(new Act.Compose(e.IsActive ? e.Text : "", e.IsActive ? marking : 0), new Target.Focused()));
        base.OnTextComposition(e);
    }

    protected override void OnTextInsertionBoundsRequested(TextInsertionBoundsEventArgs e) {
        e.Bounds = Current.Bind(static held => held.Caret).Map<RectangleF?>(static caret => new RectangleF(caret.Left, caret.Top, caret.Width, caret.Height)).ValueUnsafe();
        base.OnTextInsertionBoundsRequested(e);
    }

    // --- [TIME]
    private static IO<Duration> Now(TimeProvider clock) => IO.lift(() => Duration.FromSeconds(clock.GetTimestamp() / (double)clock.TimestampFrequency));

    private sealed record Ticks(SceneView View) : Sink<Duration> {
        public override IO<Unit> Post(Duration value) =>
            View.mounted.Traverse(held => IO.lift(() => held.Mount.Tick(value)).Bind(moved => View.Applied(held, moved))).As().Map(static _ => unit);

        public override IO<Unit> Complete() => IO.pure(unit);

        public override IO<Unit> Fail(Error Error) => IO.pure(unit);

        public override Sink<X> Comap<X>(Func<X, Duration> f) => Sink<X>.Void.Combine((X x) => (x, f(x)), this);
    }

    // --- [ELEMENTS]
    internal static Seq<Element> Elements(Seq<Element> tree) => tree.Bind(static element => element.Cons(Elements(element.Children)));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                _ = listening.Iter(static lease => lease.Dispose());
                pending.Dispose();
                seats.Dispose();
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    private CallbackSite Site([CallerMemberName] string member = "") => new(scope.Sink, typeof(SceneView), member);
}
