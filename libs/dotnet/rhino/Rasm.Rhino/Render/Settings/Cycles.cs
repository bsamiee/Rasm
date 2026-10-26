using Rasm.UI;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.Collections;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TextureBake {
    public static readonly TextureBake Low = new(0);
    public static readonly TextureBake Standard = new(1);
    public static readonly TextureBake High = new(2);
    public static readonly TextureBake Ultra = new(3);
    public static readonly TextureBake Disable = new(4);
}

[SmartEnum<int>]
public sealed partial class CyclesPreset {
    public static readonly CyclesPreset Architecture = new(0, 0.5d, 3d, 16, 1);
    public static readonly CyclesPreset Product = new(1, 0d, 20d, 256, 2);

    public double FilterGlossy { get; }
    public double SampleClampIndirect { get; }
    public int AdaptiveMinSamples { get; }
    public int SampleFactor { get; }

    public int QualitySamples(AntialiasLevel level) =>
        SampleFactor * level switch {
            AntialiasLevel.None => 15,
            AntialiasLevel.Draft => 50,
            AntialiasLevel.Good => 500,
            AntialiasLevel.High => 1500,
        };

    public IO<Unit> Apply(RenderSettingsSource source) =>
        RenderSettingsSources.Dictionary(source).Edit(target =>
            (ArchivableDictionaries.Set(target, CyclesKey.RenderPreset.Key, Key),
             ArchivableDictionaries.Set(target, CyclesKey.FilterGlossy.Key, FilterGlossy),
             ArchivableDictionaries.Set(target, CyclesKey.SampleClampIndirect.Key, SampleClampIndirect),
             ArchivableDictionaries.Set(target, CyclesKey.AdaptiveMinSamples.Key, AdaptiveMinSamples))
                .Apply(static (_, _, _, _) => unit)
                .As());

    public static CyclesPreset Inferred(double filter, double indirect, int minimum) =>
        (filter, indirect, minimum) switch {
            (0d, 0d, _) => Product,
            var signature when signature == (Product.FilterGlossy, Product.SampleClampIndirect, Product.AdaptiveMinSamples) => Product,
            _ => Architecture,
        };

    public static IO<CyclesPreset> Held(RenderSettingsSource source) =>
        RenderSettingsSources.Dictionary(source).Read.Bind(static dictionary => IO.lift(() =>
            (Stored<CyclesPreset, int>(dictionary, CyclesKey.RenderPreset, CyclesPreset.Validated), Stored(dictionary, CyclesKey.FilterGlossy, CyclesKey.Amounts.Create),
             Stored(dictionary, CyclesKey.SampleClampIndirect, CyclesKey.Amounts.Create), Stored(dictionary, CyclesKey.AdaptiveMinSamples, CyclesKey.Counts.Create))
                .Apply(static (preset, filter, indirect, minimum) => preset.IfNone(() =>
                    Inferred(filter.IfNone(Architecture.FilterGlossy), indirect.IfNone(Architecture.SampleClampIndirect), minimum.IfNone(Architecture.AdaptiveMinSamples))))
                .As()));

    private static Fin<Option<T>> Stored<T, TRaw>(ArchivableDictionary dictionary, CyclesKey key, Func<TRaw, Fin<T>> accept) where TRaw : notnull =>
        ArchivableDictionaries.Find<TRaw>(dictionary, key.Key).Bind(raw => raw.Traverse(accept).As());
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class CyclesKey {
    internal static readonly NumericDomain<int, int> Counts = new(Fin.Succ, identity, (0, int.MaxValue), new() { Track = None });
    internal static readonly NumericDomain<double, double> Amounts = new(Fin.Succ, identity, (0d, double.MaxValue), new() { Track = None });

    public static readonly CyclesKey RenderPreset = new Row<CyclesPreset, int>(
        ValueKey.Keyed<CyclesPreset, int>("RenderPreset", new("Presets", "Sampling preset, Architecture or Product"), CyclesPreset.Architecture));
    public static readonly CyclesKey Seed = new Row<int, int>(
        ValueKey.Numeric("Seed", new("Seed", "Random number generator seed"), 128, Counts));
    public static readonly CyclesKey Samples = new Row<int, int>(
        ValueKey.Numeric("Samples", new("Samples", "Samples per pixel"), 1000,
            new NumericDomain<int, int>(Fin.Succ, identity, (1, int.MaxValue), new() { Track = None })));
    public static readonly CyclesKey UseDocumentSamples = new Row<bool, bool>(
        ValueKey.Toggle("UseDocumentSamples", new("Override Production Render Quality", "Off applies Samples to viewport alone"), @default: false));
    public static readonly CyclesKey MaxBounce = new Row<int, int>(
        ValueKey.Numeric("MaxBounce", new("Maximum", "Bounce limit across every ray type"), 32, Counts));
    public static readonly CyclesKey MaxDiffuseBounce = new Row<int, int>(
        ValueKey.Numeric("MaxDiffuseBounce", new("Diffuse", "Bounce limit for diffuse rays"), 4, Counts));
    public static readonly CyclesKey MaxGlossyBounce = new Row<int, int>(
        ValueKey.Numeric("MaxGlossyBounce", new("Glossy", "Bounce limit for glossy rays"), 16, Counts));
    public static readonly CyclesKey MaxTransmissionBounce = new Row<int, int>(
        ValueKey.Numeric("MaxTransmissionBounce", new("Transmission", "Bounce limit for transmission rays"), 32, Counts));
    public static readonly CyclesKey MaxVolumeBounce = new Row<int, int>(
        ValueKey.Numeric("MaxVolumeBounce", new("Volume", "Bounce limit for volume rays"), 32, Counts));
    public static readonly CyclesKey TransparentMaxBounce = new Row<int, int>(
        ValueKey.Numeric("TransparentMaxBounce", new("Transparency", "Bounce limit through transparent surfaces"), 32, Counts));
    public static readonly CyclesKey TextureBakeQuality = new Row<TextureBake, int>(
        ValueKey.Keyed<TextureBake, int>("TextureBakeQuality", new("Texture Bake Quality", "Texture bake resolution"), TextureBake.Low));
    public static readonly CyclesKey UseAdaptiveSampling = new Row<bool, bool>(
        ValueKey.Toggle("UseAdaptiveSampling", new("Adaptive sampling", "Stops sampling pixels that reach noise threshold"), @default: true));
    public static readonly CyclesKey AdaptiveThreshold = new Row<double, double>(
        ValueKey.Numeric("AdaptiveThreshold", new("Adaptive threshold", "Noise level a pixel stops sampling at"), 0.01d,
            new NumericDomain<double, double>(Fin.Succ, identity, (double.BitIncrement(0d), 1d), new() { Track = None })));
    public static readonly CyclesKey AdaptiveMinSamples = new Row<int, int>(
        ValueKey.Numeric("AdaptiveMinSamples", new("Adaptive minimum samples", "Samples before adaptive stopping, 0 derives them from threshold"),
            CyclesPreset.Architecture.AdaptiveMinSamples, Counts));
    public static readonly CyclesKey AoBounces = new Row<int, int>(
        ValueKey.Numeric("AoBounces", new("AO bounces", "Bounces before ambient occlusion replaces global illumination"), 0, Counts));
    public static readonly CyclesKey SampleClampDirect = new Row<double, double>(
        ValueKey.Numeric("SampleClampDirect", new("Direct sample clamp", "Limit on direct light per sample, 0 off"), 3d, Amounts));
    public static readonly CyclesKey SampleClampIndirect = new Row<double, double>(
        ValueKey.Numeric("SampleClampIndirect", new("Indirect sample clamp", "Limit on indirect light per sample, 0 off"), CyclesPreset.Architecture.SampleClampIndirect, Amounts));
    public static readonly CyclesKey FilterGlossy = new Row<double, double>(
        ValueKey.Numeric("FilterGlossy", new("Filter glossy", "Blur of glossy paths that suppresses caustic fireflies, 0 off"), CyclesPreset.Architecture.FilterGlossy, Amounts));
    public static readonly CyclesKey CausticsReflective = new Row<bool, bool>(
        ValueKey.Toggle("CausticsReflective", new("Reflective caustics", "Light paths reflected onto diffuse surfaces"), @default: true));
    public static readonly CyclesKey CausticsRefractive = new Row<bool, bool>(
        ValueKey.Toggle("CausticsRefractive", new("Refractive caustics", "Light paths refracted onto diffuse surfaces"), @default: true));
    public static readonly CyclesKey UseDirectLight = new Row<bool, bool>(
        ValueKey.Toggle("UseDirectLight", new("Sample all lights", "Direct light from every light at each bounce"), @default: true));
    public static readonly CyclesKey UseIndirectLight = new Row<bool, bool>(
        ValueKey.Toggle("UseIndirectLight", new("Sample all lights indirect", "Indirect light from every light at each bounce"), @default: true));
    public static readonly CyclesKey MaxPasses = new Row<bool, bool>(
        ValueKey.Toggle("MaxPasses", new("Show max passes", "Shows pass limit in Raytraced heads-up display"), @default: true));

    public static string Owner => "rhino-render";

    internal abstract (ValueBinding Binding, IO<Unit> Reset) Entry(DictionaryOwner owner);

    private sealed class Row<TValue, TRaw>(ValueKey<TValue> key) : CyclesKey(key.Key) where TValue : notnull where TRaw : notnull, ISpanParsable<TRaw> {
        private readonly ValueKey<TValue> key = key;

        internal override (ValueBinding Binding, IO<Unit> Reset) Entry(DictionaryOwner owner) =>
            ArchivableDictionaries.Store<TValue, TRaw>(owner, key, None) switch {
                var store => (ValueBinding.Of(Owner, key.Codec, store), store.Write(None).Map(unit)),
            };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CyclesSetting {
    public static Guid PlugInId { get; } = new(0x9bc28e9e, 0x7a6c, 0x4b8f, 0xa0, 0xc6, 0x3d, 0x05, 0xe0, 0x2d, 0x1b, 0x97);

    private static SettingsNode Node { get; } = new(PlugInId);

    public static IO<BindingGroup> Bindings {
        get {
            const string threads = "Threads";
            return from node in SettingRoots.TryGetChild(Node)
                   select new BindingGroup(Seq(
                       Bind(ValueKey.Numeric("ThrottleMs", new("Throttle (in ms)", "Pause between Raytraced viewport updates"), 100, CyclesKey.Counts), SettingType.Integer, Applied.Live),
                       Bind(ValueKey.Raw("SelectedDeviceStr", new("Device", "Devices Rhino Render runs on, -1 picks first GPU"), "-1"), SettingType.String, Applied.Live),
                       Bind(ValueKey.Raw("IntermediateSelectedDeviceStr", new("Device selection", "Device choice the device section writes with Device"), "-1"), SettingType.String, Applied.Live),
                       Bind(ValueKey.Numeric("PixelSize", new("Pixel size", "Raytraced pixel size, Sharpness writes 11 minus it"), 1,
                           new NumericDomain<int, int>(Fin.Succ, identity, (1, 10), new() { Track = None })), SettingType.Integer, Applied.Live),
                       Bind(ValueKey.Toggle("StartGpuKernelCompiler", new("Compile GPU kernels", "Starts compiling when Rhino Render loads"), @default: true), SettingType.Bool, Applied.IdleSave),
                       Bind(ValueKey.Toggle("UseLightTree", new("Light tree", "Samples lights through light tree"), @default: true), SettingType.Bool, Applied.Live),
                       Bind(ValueKey.Numeric("SunLightFactor", new("Sun light factor", "Multiplier on sun intensity"), 3.2d, CyclesKey.Amounts), SettingType.Double, Applied.Live),
                       Bind(ValueKey.Numeric("AreaLightFactor", new("Area light factor", "Multiplier on rectangular light intensity"), 17.2d, CyclesKey.Amounts), SettingType.Double, Applied.Live),
                       Bind(ValueKey.Numeric("LinearLightFactor", new("Linear light factor", "Multiplier on linear light intensity"), 10d, CyclesKey.Amounts), SettingType.Double, Applied.Live),
                       Bind(ValueKey.Numeric("PointLightFactor", new("Point light factor", "Multiplier on point light intensity"), 40d, CyclesKey.Amounts), SettingType.Double, Applied.Live),
                       Bind(ValueKey.Numeric("SpotLightFactor", new("Spot light factor", "Multiplier on spot light intensity"), 40d, CyclesKey.Amounts), SettingType.Double, Applied.Live))
                       + node.Bind(static settings => Callbacks.Found(settings.TryGetDefault(threads, out int factory), factory))
                           .Map(static factory => Bind(ValueKey.Raw(threads, new("CPU Threads", "Threads for CPU render sessions"), factory), SettingType.Integer, Applied.Live)).ToSeq());

            static ValueBinding Bind<TValue, TRaw>(ValueKey<TValue> key, SettingType<TRaw> kind, Applied applied)
                where TValue : notnull
                where TRaw : notnull, ISpanParsable<TRaw> =>
                ValueBinding.Of("rhino-render-options", key.Codec, PlugInSettings.Store(Node, new PlugInSetting<TValue, TRaw>(key, kind) { Applied = applied }));
        }
    }
}
