using System.Drawing;
using System.Reflection;
using MathNet.Numerics.RootFinding;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.TopologicalSort;
using Rhino.ApplicationSettings;
using Rhino.PlugIns;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.Theme;
using Wacton.Unicolour;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Zone {
    public static readonly Zone Frame = new(static () => ThemeSettings.Frame);
    public static readonly Zone Content = new(static () => ThemeSettings.Content);

    public Func<ThemeZone> Live { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ThemeKey {
    public Option<bool> Dark { get; init; }

    public sealed record Entry(Zone Zone, string Id) : ThemeKey {
        public bool Within(Func<ThemeZone, ThemeBase> element) =>
            toSeq(element(Zone.Live()).Enumerate()).Exists(entry => string.Equals(entry.Id, Id, StringComparison.Ordinal));
    }

    public sealed record Paint(PaintColor Member) : ThemeKey;

    public sealed record Widget(WidgetColor Member) : ThemeKey;

    public sealed record Appearance(string Member) : ThemeKey;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColorBinding {
    public sealed record Set(Unicolour Value) : ColorBinding;

    public sealed record Inherit(ThemeKey Source, Seq<ColorModifier> Modifiers) : ColorBinding;
}

[SmartEnum]
public sealed partial class ColorChannel {
    public static readonly ColorChannel Lightness = new(static (color, delta) => Oklch(color, (lightness, chroma, hue) => (lightness + delta, chroma, hue)));
    public static readonly ColorChannel Chroma = new(static (color, factor) => Oklch(color, (lightness, chroma, hue) => (lightness, chroma * factor, hue)));
    public static readonly ColorChannel Hue = new(static (color, degrees) => Oklch(color, (lightness, chroma, hue) => (lightness, chroma, (hue + degrees) % 360d)));
    public static readonly ColorChannel Alpha = new(static (color, alpha) => new Unicolour(ColourSpace.Oklch, color.Oklch.L, color.Oklch.C, color.Oklch.H, alpha));

    [UseDelegateFromConstructor]
    public partial Unicolour Apply(Unicolour color, double amount);

    private static Unicolour Oklch(Unicolour color, Func<double, double, double, (double L, double C, double H)> change) =>
        change(color.Oklch.L, color.Oklch.C, color.Oklch.H) switch {
            var (lightness, chroma, hue) => new Unicolour(ColourSpace.Oklch, lightness, chroma, hue, color.Alpha.A),
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColorModifier {
    public sealed record Adjust(ColorChannel Channel, double Amount) : ColorModifier;

    public sealed record Mix(Unicolour Toward, double Amount) : ColorModifier;

    public sealed record Tint(Unicolour Neutral, Unicolour Accent) : ColorModifier;

    public sealed record Legible(ThemeKey Against, double Ratio) : ColorModifier;
}

public sealed record RhinoTheme(string Name) {
    // --- [DECLARATION]
    public Func<ThemeKey, Option<ColorBinding>> Colors { get; init; } = static _ => None;

    public Seq<IO<IO<Unit>>> Overrides { get; init; }

    private static Preference<string> Stored =>
        Preferences.Setting(new SettingsNode(new SettingsRoot.PlugInNode(Session.PlugIn.Id)), nameof(RhinoTheme), fallback: "", hidden: true);

    public static Preference<string> Current { get; } =
        Preferences.Of(IO.lift(static () => Stored).Bind(static stored => stored.Read), static name => IO.lift(static () => Stored).Bind(stored => stored.Write(name)));

    // --- [APPLY]
    public IO<IO<Unit>> Apply =>
        from restore in Preferences.Swap(Seq(Colored.Map(static colored => colored.Restore)) + Overrides + Seq(Current.Swap(Name)))
        from flushed in Flush
        select restore >> Flush;

    public IO<Unit> Hold => Painted(Overrides + Seq(Current.Swap(Name))).Scoped();

    public static IO<Unit> Following(Seq<RhinoTheme> themes) =>
        (from slot in IO.lift(static () => Ref(Option<Owned<string>>.None))
         from loaded in Selected(themes, slot)
         from followed in Current.Changed(_ => Selected(themes, slot).Scoped())
         select unit).Scoped();

    private static IO<Unit> Selected(Seq<RhinoTheme> themes, Ref<Option<Owned<string>>> slot) =>
        from name in Current.Read
        from moved in unless(slot.Value.Exists(held => string.Equals(held.Value, name, StringComparison.Ordinal)),
            Products.Clear(slot) >> themes.Find(theme => string.Equals(theme.Name, name, StringComparison.Ordinal))
                .Traverse(theme => Products.Replace(slot, theme.Painted(theme.Overrides).Map(_ => name))).As().Map(unit)).As()
        select unit;

    private IO<Unit> Painted(Seq<IO<IO<Unit>>> swaps) =>
        from latest in IO.lift(static () => Ref(Option<Owned<(bool Dark, IO<Unit> Restore)>>.None))
        from colored in Products.Replace(latest, Colored)
        from held in Preferences.Swap(swaps).Bind(Products.Released).Catch(error => Restored(latest) >> IO.fail<Unit>(error))
        from released in Products.Released(Restored(latest) >> Flush)
        from flushed in Flush
        from followed in ThemeChanged.Attach(_ => Switched(latest))
        select unit;

    private IO<(bool Dark, IO<Unit> Restore)> Colored =>
        from dark in Running
        from palette in PaletteFor(dark)
        from restore in Preferences.Swap(Swaps(palette))
        select (dark, restore);

    private IO<Unit> Switched(Ref<Option<Owned<(bool Dark, IO<Unit> Restore)>>> latest) =>
        from dark in Running
        from current in Current.Read
        let held = latest.Value
        from switched in when(string.Equals(current, Name, StringComparison.Ordinal) && held.Exists(owned => owned.Value.Dark != dark),
            Restored(latest) >> Products.Replace(latest, Colored).Map(static _ => unit) >> Flush).As()
        select switched;

    private static IO<Unit> Restored(Ref<Option<Owned<(bool Dark, IO<Unit> Restore)>>> latest) =>
        IO.lift(() => latest.Value).Bind(static held => held.Match(Some: static owned => owned.Value.Restore, None: static () => IO.pure(unit)));

    private static IO<Unit> Flush => IO.lift(static () => PlugIn.FlushSettingsSavedQueue());

    private static Seq<IO<IO<Unit>>> Swaps(HashMap<ThemeKey, Color> palette) =>
        toSeq(palette.AsIterable()).Map(static row => row.Key.Switch(
            row.Value,
            entry: static (color, key) => Keyed(key).Swap(Some(color)),
            paint: static (color, key) => Moded(key, Preferences.Property(() => AppearanceSettings.GetPaintColor(key.Member), held => AppearanceSettings.SetPaintColor(key.Member, held, forceUiUpdate: true)).Swap(color)),
            widget: static (color, key) => Moded(key, Preferences.Property(() => AppearanceSettings.GetWidgetColor(key.Member), held => AppearanceSettings.SetWidgetColor(key.Member, held, forceUiUpdate: true)).Swap(color)),
            appearance: static (color, key) => Moded(key, AppearanceColors.Find(member => string.Equals(member.Name, key.Member, StringComparison.Ordinal)).Match(
                Some: member => Preferences.Property(() => (Color)member.GetValue(obj: null)!, held => member.SetValue(obj: null, held)).Swap(color),
                None: () => IO.fail<IO<Unit>>(new RhinoFault(key.Member, RhinoReason.Missing))))));

    private static readonly Seq<PropertyInfo> AppearanceColors =
        toSeq(typeof(AppearanceSettings).GetProperties(BindingFlags.Public | BindingFlags.Static)).Filter(static member => member.PropertyType == typeof(Color) && member is { CanRead: true, CanWrite: true }).Strict();

    private static IO<IO<Unit>> Moded(ThemeKey key, IO<IO<Unit>> swap) =>
        swap.Map(restore => Running.Bind(dark => when(key.Dark.ForAll(mode => mode == dark), restore).As()));

    private static readonly SettingsNode Keys = SettingsNode.Application.Child("UI").Child("ThemeSettings");

    private static IO<bool> Running => IO.lift(static () => HostUtils.RunningInDarkMode);

    private static Preference<Option<Color>> Keyed(ThemeKey.Entry key) =>
        IO.lift(() => Accessors.Combine(owner: null, Accessors.SettingId(key.Zone.Live()), key.Id)) switch {
            var setting => Preferences.Of(
                from name in setting
                from found in Keys.Existing
                select found.Bind(node => Conversions.Present(node.TryGetColor(name, out Color held), held)),
                color =>
                    from name in setting
                    from node in Keys.Created
                    from written in IO.lift(() => color.Match(Some: held => node.SetColor(name, held), None: () => node.DeleteItem(name)))
                    select written,
                Some(ThemeChanged)),
        };

    // --- [PALETTE]
    private const double ContrastAccuracy = 1e-4;
    private const int ContrastIterations = 64;

    public IO<HashMap<ThemeKey, Color>> Palette => Running.Bind(PaletteFor);

    private IO<HashMap<ThemeKey, Color>> PaletteFor(bool dark) =>
        from live in Live(dark)
        from resolved in IO.lift(Resolved(live.Values, live.Hooked))
        select resolved;

    private static IO<(HashMap<ThemeKey, Color> Values, HashSet<PaintColor> Hooked)> Live(bool dark) =>
        from entries in IO.lift(() => (
            from zone in toSeq(Zone.Items)
            from entry in toSeq(zone.Live().Enumerate())
            where entry.Value is Eto.Drawing.Color
            select (Key: (ThemeKey)new ThemeKey.Entry(zone, entry.Id) { Dark = Some(dark) }, Color: Color.FromArgb(((Eto.Drawing.Color)entry.Value).ToArgb()))).Strict())
        from paints in Accessors.Scoped(static () => toSeq(Enum.GetValues<PaintColor>())
            .Map(static member => (Member: member, Hooked: Accessors.GetPaintColorHook(owner: null, member) is not null, Color: AppearanceSettings.GetPaintColor(member))).Strict())
        from widgets in IO.lift(() => toSeq(Enum.GetValues<WidgetColor>())
            .Map(member => (Key: (ThemeKey)new ThemeKey.Widget(member) { Dark = Some(dark) }, Color: AppearanceSettings.GetWidgetColor(member))).Strict())
        from appearance in IO.lift(() => AppearanceColors
            .Map(member => (Key: (ThemeKey)new ThemeKey.Appearance(member.Name) { Dark = Some(dark) }, Color: (Color)member.GetValue(obj: null)!)).Strict())
        select (
            toHashMap(entries + paints.Map(paint => (Key: (ThemeKey)new ThemeKey.Paint(paint.Member) { Dark = Some(dark) }, paint.Color)) + widgets + appearance),
            toHashSet(paints.Filter(static paint => paint.Hooked).Map(static paint => paint.Member)));

    private Fin<HashMap<ThemeKey, Color>> Resolved(HashMap<ThemeKey, Color> live, HashSet<PaintColor> hooked) {
        HashMap<ThemeKey, ColorBinding> bound = toHashMap(toSeq(live.Keys).Choose(key => (Colors(key) || Colors(key with { Dark = None })).Map(binding => (key, binding))));
        HashMap<ThemeKey, Seq<ThemeKey>> dependencies = bound.Map((key, binding) => binding.Switch(
            key,
            set: static (_, _) => Seq<ThemeKey>(),
            inherit: static (at, inherit) => (inherit.Source with { Dark = inherit.Source.Dark || at.Dark }) switch {
                var source => (source == at ? Seq<ThemeKey>() : Seq(source))
                    + inherit.Modifiers.Choose(static modifier => modifier is ColorModifier.Legible legible ? Some(legible.Against) : None)
                        .Map(against => against with { Dark = against.Dark || at.Dark }),
            }).Filter(bound.ContainsKey));

        Func<ThemeKey, Fin<Unicolour>> basis = key =>
            live.Find(key).Map(ToUnicolour).ToFin(new RhinoFault(key, RhinoReason.Missing));

        Func<ThemeKey, HashMap<ThemeKey, Fin<Unicolour>>, Fin<Unicolour>> value = (key, known) =>
            bound.ContainsKey(key) ? known[key] : basis(key);

        Fin<Unicolour> Resolve(ThemeKey key, HashMap<ThemeKey, Fin<Unicolour>> known) =>
            key is ThemeKey.Paint { Member: var member } && hooked.Contains(member)
                ? new RhinoFault(member, RhinoReason.Unsupported)
                : bound[key].Switch(
                    (Key: key, Known: known, Base: basis, Value: value),
                    set: static (_, set) => set.Value,
                    inherit: static (state, inherit) => (inherit.Source with { Dark = inherit.Source.Dark || state.Key.Dark }) switch {
                        var source =>
                            from color in source == state.Key ? state.Base(source) : state.Value(source, state.Known)
                            let read = fun((ThemeKey against) => state.Value(against with { Dark = against.Dark || state.Key.Dark }, state.Known))
                            from modified in inherit.Modifiers.FoldM(color, (current, modifier) => Modified(current, modifier, read)).As()
                            select modified,
                    });

        return Try.lift(() => toSeq(dependencies.Keys)
                .ToBidirectionalGraph(key => dependencies[key].Map(target => new Edge<ThemeKey>(key, target)), allowParallelEdges: false)
                .SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward)).Run()
            .MapFail(error => error.HasException<NonAcyclicGraphException>() ? new RhinoFault(Name, RhinoReason.Repeated, Inner: error) : error)
            .Map(order => toSeq(order).Fold(HashMap<ThemeKey, Fin<Unicolour>>(), (known, key) =>
                known.Add(key, Resolve(key, known).MapFail(error => new RhinoFault(key, RhinoReason.Invalid, Inner: error)))))
            .Bind(static known => toSeq(known.AsIterable()).Traverse(static row => row.Value.Map(color => (row.Key, Color: ToColor(color)))).As())
            .Map(static rows => toHashMap(rows));
    }

    // --- [MODIFIERS]
    private static Fin<Unicolour> Modified(Unicolour color, ColorModifier modifier, Func<ThemeKey, Fin<Unicolour>> value) =>
        modifier.Switch(
            (Color: color, Value: value),
            adjust: static (state, adjust) => adjust.Channel.Apply(state.Color, adjust.Amount),
            mix: static (state, mix) => state.Color.Mix(mix.Toward, ColourSpace.Oklch, mix.Amount),
            tint: static (state, tint) => (Math.Abs(state.Color.Oklch.C - tint.Neutral.Oklch.C) <= Math.Abs(state.Color.Oklch.C - tint.Accent.Oklch.C) ? tint.Neutral : tint.Accent) switch {
                var hue => new Unicolour(ColourSpace.Oklch, state.Color.Oklch.L, hue.Oklch.C, hue.Oklch.H, state.Color.Alpha.A),
            },
            legible: static (state, legible) => state.Value(legible.Against).Map(against => Legible(state.Color, against, legible.Ratio)));

    private static Unicolour Legible(Unicolour color, Unicolour against, double ratio) =>
        color.Contrast(against) >= ratio
            ? color
            : (against.Oklch.L < 0.5d ? 1d : 0d) switch {
                var extreme => Lightened(color,
                    Bisection.TryFindRoot(lightness => Lightened(color, lightness).Contrast(against) - ratio, Math.Min(color.Oklch.L, extreme), Math.Max(color.Oklch.L, extreme), ContrastAccuracy, ContrastIterations, out double root)
                        ? root
                        : extreme),
            };

    private static Unicolour Lightened(Unicolour color, double lightness) =>
        new(ColourSpace.Oklch, lightness, color.Oklch.C, color.Oklch.H, color.Alpha.A);

    private static Unicolour ToUnicolour(Color color) => new(ColourSpace.Rgb255, color.R, color.G, color.B, color.A / (double)byte.MaxValue);

    private static Color ToColor(Unicolour color) =>
        color.MapToRgbGamut().Rgb.Byte255 switch {
            var rgb => Color.FromArgb(color.Alpha.A255, rgb.R, rgb.G, rgb.B),
        };

    // --- [SIGNALS]
    private static readonly HostEvent<Unit> ThemeChanged =
        Subscriptions.On<EventHandler, EventArgs>(static h => ThemeSettings.ThemeChanged += h, static h => ThemeSettings.ThemeChanged -= h, static handler => handler.Invoke).Choose(static _ => Some(unit));

    public static IO<Unit> Changed(Func<bool, IO<Unit>> react) =>
        Subscriptions.Merged(ThemeChanged, Preferences.AppSettingsChanged).Attach(_ => Running.Bind(react));

    // --- [DERIVATION]
    public static Func<ThemeKey, Option<ColorBinding>> Tinted(Unicolour neutral, Unicolour accent) =>
        key => key is ThemeKey.Entry ? Some<ColorBinding>(new ColorBinding.Inherit(key, [new ColorModifier.Tint(neutral, accent)])) : None;
}
