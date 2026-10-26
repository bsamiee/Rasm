using System.Globalization;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SceneTarget<TPixels> where TPixels : IDisposable {
    public required SKImageInfo Info { get; init; }

    public SKCanvas Canvas => Switch(standard: static standard => standard.Surface.Canvas, extended: static extended => extended.Surface.Canvas);

    internal void Release() =>
        Switch(
            standard: static standard => {
                standard.Surface.Dispose();
                standard.Pixels.Dispose();
            },
            extended: static extended => extended.Surface.Dispose());

    public sealed record Standard(TPixels Pixels, SKSurface Surface) : SceneTarget<TPixels>;

    public sealed record Extended(SKSurface Surface) : SceneTarget<TPixels>;
}

public sealed record SceneBackend<TPixels>(
    Func<SKSizeI, Fin<(TPixels Pixels, SKPixmap Pixmap)>> Allocated,
    Func<Seq<SKRect>, IO<Unit>> Invalidated,
    Func<Func<Duration, IO<Unit>>, IO<Func<bool, IO<Unit>>>> Linked) where TPixels : IDisposable {
    public SKSurfaceProperties Properties { get; init; } = SceneRaster.Aligned;
    public Func<Mount, Output, IO<Unit>> Applied { get; init; } = static (_, _) => IO.pure(unit);
}

public sealed record PlacedControls<TControl>(
    Func<Seq<Key>, (SKRectI Clip, SKRectI Frame), Option<Element>, IO<TControl>> Added,
    Func<TControl, (SKRectI Clip, SKRectI Frame), IO<TControl>> Moved,
    Func<TControl, IO<Unit>> Removed) where TControl : notnull {
    public HashMap<Seq<Key>, (Hosted.Slot Slot, TControl Control)> Held { get; private init; }

    public IO<Unit> Released => Held.Values.ToSeq().TraverseM(held => Removed(held.Control)).As().Map(static _ => unit);

    public IO<PlacedControls<TControl>> Reconciled(Mount mount) =>
        toHashMap(mount.Scene.Hosted.Choose(static hosted => hosted.Switch(slot: static slot => Some(slot), backdrop: static _ => Option<Hosted.Slot>.None)).Map(static slot => (slot.Path, slot))) switch {
            var wanted => toTrackingHashMap(Held.Map(static held => held.Slot).AsIterable().Map(static entry => (entry.Key, entry.Value))).Snapshot()
                .RemoveRange(Held.Keys.Filter(path => !wanted.ContainsKey(path)))
                .AddOrUpdateRange(wanted.AsIterable().Map(static entry => (entry.Key, entry.Value)))
                .Changes.AsIterable().ToSeq()
                .Fold(IO.pure(this), (sequence, change) => sequence.Bind(placed => placed.Changed(change.Key, change.Value, mount))),
        };

    private IO<PlacedControls<TControl>> Changed(Seq<Key> path, Change<Hosted.Slot> change, Mount mount) =>
        (change, Held.Find(path).Map(static held => held.Control).Case) switch {
            (EntryAdded<Hosted.Slot> added, _) => Added(path, Placed(added.Value), Access.Flattened(mount.Tree).Find(element => element.Id == path))
                .Map(control => this with { Held = Held.AddOrUpdate(path, (added.Value, control)) }),
            (EntryMappedTo<Hosted.Slot> mapped, TControl control) => Moved(control, Placed(mapped.To)).Map(moved => this with { Held = Held.AddOrUpdate(path, (mapped.To, moved)) }),
            (EntryRemoved<Hosted.Slot>, TControl control) => Removed(control).Map(_ => this with { Held = Held.Remove(path) }),
            _ => IO.pure(this),
        };

    private static (SKRectI Clip, SKRectI Frame) Placed(Hosted.Slot slot) =>
        (SKRectI.Create((int)MathF.Floor(slot.Clip.Left), (int)MathF.Floor(slot.Clip.Top), (int)MathF.Ceiling(slot.Clip.Width), (int)MathF.Ceiling(slot.Clip.Height)),
            SKRectI.Create((int)MathF.Round(slot.Frame.Left - slot.Clip.Left, MidpointRounding.ToEven), (int)MathF.Round(slot.Frame.Top - slot.Clip.Top, MidpointRounding.ToEven), (int)MathF.Ceiling(slot.Frame.Width), (int)MathF.Ceiling(slot.Frame.Height)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneHost<TPixels>(SceneBackend<TPixels> backend, TimeProvider clock) : IDisposable where TPixels : IDisposable {
    // --- [STATE]
    private sealed record Raster(SceneTarget<TPixels> Target, PaintCache Cache, float Scale);

    private readonly SKRegion damage = new();
    private Option<Node> view;
    private Option<(Host Host, DefaultStyles Defaults)> hosting;
    private Option<Raster> raster;
    private Option<Func<bool, IO<Unit>>> running;

    public Option<Mount> Current { get; private set; }

    public Atom<float> Peak { get; } = Atom(1f);

    // --- [DERIVED]
    public IO<Duration> Now => IO.lift(() => clock.GetElapsedTime(0L).ToDuration());

    public Option<SKSize> Preferred(SKSize offered) =>
        Current.Map(held => held.Scene.Preferred(offered) switch {
            var preferred => new SKSize(preferred.Width, float.Min(preferred.Height, offered.Height)),
        });

    // --- [SESSION]
    public IO<Unit> Show(Node next) =>
        IO.lift(() => view).Bind(prior => unless(prior.Exists(held => ReferenceEquals(held, next)),
            from stored in IO.lift(() => view = Some(next))
            from shown in Current.Match(
                Some: held => from now in Now from moved in IO.lift(() => held.Show(next, now)) from applied in Applied(moved) select unit,
                None: () => hosting.Match(Some: placed => Created(placed.Host, placed.Defaults, []), None: static () => IO.pure(unit)))
            select unit).As());

    public IO<Unit> Rehost(Host host, DefaultStyles defaults) =>
        IO.lift(() => hosting).Bind(prior => unless(prior == Some((Host: host, Defaults: defaults)),
            from stored in IO.lift(() => hosting = Some((Host: host, Defaults: defaults)))
            from moved in Current.Match(
                Some: held => prior.Exists(was => was.Defaults.Equals(defaults))
                    ? from now in Now from applied in Applied(held.Rehost(host, now)) select unit
                    : Created(host, defaults, held.Scene.Adjusted),
                None: () => Created(host, defaults, []))
            select unit).As());

    public IO<bool> Receive(Input input, Duration at) =>
        IO.lift(() => Current).Bind(held => held.Match(Some: current => Applied(current.Receive(input, at)), None: static () => IO.pure(value: false)));

    public IO<Unit> Released =>
        IO.lift(() => {
            Option<Func<bool, IO<Unit>>> held = running;
            _ = Cleared();
            return held;
        }).Bind(static held => held.Traverse(static run => run(false)).As()).Map(static _ => unit);

    private IO<Unit> Created(Host host, DefaultStyles defaults, HashMap<Seq<Key>, float> adjusted) =>
        view.Match(
            Some: node => from made in IO.lift(() => Mount.Create(host, defaults, node, adjusted)) from applied in Applied(made) select unit,
            None: static () => IO.pure(unit));

    private IO<Unit> Tick(Duration now) =>
        IO.lift(() => Current).Bind(held => held.Traverse(current => Applied(current.Tick(now))).As()).Map(static _ => unit);

    // --- [OUTPUT]
    private IO<bool> Applied((Mount Mount, Output Output) moved) =>
        from stored in IO.lift(() => {
            _ = Damaged(moved.Output.Damage);
            Current = Some(moved.Mount);
            return unit;
        })
        from invalidated in unless(moved.Output.Damage.IsEmpty, backend.Invalidated(moved.Output.Damage)).As()
        from linked in Linked(moved.Mount.Animating)
        from applied in backend.Applied(moved.Mount, moved.Output)
        from answered in moved.Output.Answer.IfNone(IO.pure(unit))
        select moved.Output.Answer.IsSome;

    private Unit Damaged(Seq<SKRect> rects) =>
        raster.Iter(held => rects.Iter(rect => damage.Op(SKRectI.Ceiling(SKMatrix.CreateScale(held.Scale, held.Scale).MapRect(rect), outwards: true), SKRegionOperation.Union)));

    private IO<Unit> Linked(bool animating) =>
        (running.IsNone && animating ? backend.Linked(Tick).Map(started => running = Some(started)) : IO.pure(running))
        .Bind(held => held.Traverse(run => run(animating)).As()).Map(static _ => unit);

    // --- [PAINT]
    public IO<Option<SceneTarget<TPixels>>> Paint(float scale) =>
        IO.lift(() => Current).Bind(held => held
            .Map(current => (Mount: current, Pixels: new SKSizeI((int)MathF.Ceiling(current.Host.Size.Width * scale), (int)MathF.Ceiling(current.Host.Size.Height * scale))))
            .Filter(static current => current.Pixels is { Width: > 0, Height: > 0 })
            .Match(
                Some: current =>
                    from target in IO.lift(() => Rastered(current.Pixels, scale, current.Mount.Scene.Extended))
                    from now in Now
                    from drawn in IO.lift(() => Drawn(target, current.Mount, now))
                    select Some(target.Target),
                None: static () => IO.pure(Option<SceneTarget<TPixels>>.None)));

    private Fin<Raster> Rastered(SKSizeI pixels, float scale, bool extended) =>
        raster.Filter(held => held.Target.Info.Size == pixels && held.Scale.Equals(scale) && held.Target.Map(standard: false, extended: true) == extended)
            .Match<Fin<Raster>>(Some: static held => held, None: () => Reallocated(pixels, scale, extended));

    private Fin<Raster> Reallocated(SKSizeI pixels, float scale, bool extended) {
        _ = Cleared(raster);
        raster = None;
        return Allocated(pixels, extended).Map(target => {
            Raster made = new(target, new PaintCache(target.Info, backend.Properties), scale);
            raster = Some(made);
            _ = damage.SetRect(SKRectI.Create(pixels));
            return made;
        });
    }

    private Fin<SceneTarget<TPixels>> Allocated(SKSizeI pixels, bool extended) =>
        extended
            ? SceneRaster.Floated(pixels) switch {
                var info => SceneRaster.Created(info, () => SKSurface.Create(info, backend.Properties)).Map(made => (SceneTarget<TPixels>)new SceneTarget<TPixels>.Extended(made) { Info = info }),
            }
            : backend.Allocated(pixels).Bind(held => Wrapped(held.Pixels, held.Pixmap));

    private Fin<SceneTarget<TPixels>> Wrapped(TPixels pixels, SKPixmap pixmap) {
        using (pixmap) {
            return SceneRaster.Created(pixmap.Info, () => SKSurface.Create(pixmap, backend.Properties))
                .Map(made => (SceneTarget<TPixels>)new SceneTarget<TPixels>.Standard(pixels, made) { Info = pixmap.Info })
                .MapFail(error => {
                    pixels.Dispose();
                    return error;
                });
        }
    }

    private Unit Drawn(Raster target, Mount current, Duration now) {
        if (damage.IsEmpty) return unit;
        SKCanvas canvas = target.Target.Canvas;
        canvas.ResetMatrix();
        canvas.Scale(target.Scale);
        Painter.Paint(canvas, target.Cache, current, damage, now);
        damage.SetEmpty();
        return unit;
    }

    // --- [RELEASE]
    private Unit Cleared() {
        _ = Cleared(raster);
        (raster, Current, hosting, running) = (None, None, None, None);
        damage.SetEmpty();
        return unit;
    }

    private static Unit Cleared(Option<Raster> held) =>
        held.Iter(static current => {
            current.Target.Release();
            current.Cache.Dispose();
        });

    public void Dispose() {
        _ = Cleared();
        damage.Dispose();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SceneRaster {
    // --- [TARGETS]
    public static SKSurfaceProperties Aligned { get; } = new(SKPixelGeometry.Unknown);

    public static SKSurfaceProperties Scaled { get; } = new(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.Unknown);

    internal static SKImageInfo Floated(SKSizeI pixels) => new(pixels.Width, pixels.Height, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgbLinear());

    internal static Fin<SKSurface> Created(SKImageInfo info, Func<SKSurface?> created) =>
        created() is { } made ? made : new Invalid(string.Create(CultureInfo.InvariantCulture, $"SKSurface.Create refused {info.Width}x{info.Height} {info.ColorType}"));

    // --- [RASTER]
    public static Fin<SKImage> Of(DefaultStyles defaults, ColorScheme theme, Node node, SKSizeI pixels) =>
        from made in Mount.Create(new Host(new SKSize(pixels.Width, pixels.Height), theme, Visibility.Shown, Cycles: false, CultureInfo.InvariantCulture), defaults, node, [])
        from image in Painted(made.Mount, made.Mount.Scene.Extended ? Floated(pixels) : new SKImageInfo(pixels.Width, pixels.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb()))
        select image;

    private static Fin<SKImage> Painted(Mount mount, SKImageInfo info) {
        using PaintCache cache = new(info, Scaled);
        using SKRegion whole = new(SKRectI.Create(info.Width, info.Height));
        return Created(info, () => SKSurface.Create(info, Scaled)).Map(surface => {
            using (surface) {
                Painter.Paint(surface.Canvas, cache, mount, whole, Duration.Zero);
                return surface.Snapshot();
            }
        });
    }
}
