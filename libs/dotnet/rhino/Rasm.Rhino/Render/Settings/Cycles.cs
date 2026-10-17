using System.Diagnostics;
using Rasm.Imaging.Pixels;
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
            _ => throw new UnreachableException(),
        };

    public IO<Unit> Apply(RenderSettingsSource source) =>
        RenderSettingsSources.Dictionary(source).Edit(target =>
            (ArchivableDictionaries.Set(target, CyclesKey.RenderPreset.Key, Key).ToValidation(),
             ArchivableDictionaries.Set(target, CyclesKey.FilterGlossy.Key, FilterGlossy).ToValidation(),
             ArchivableDictionaries.Set(target, CyclesKey.SampleClampIndirect.Key, SampleClampIndirect).ToValidation(),
             ArchivableDictionaries.Set(target, CyclesKey.AdaptiveMinSamples.Key, AdaptiveMinSamples).ToValidation())
                .Apply(static (_, _, _, _) => unit)
                .As()
                .ToFin());

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
                .As()
                .ToFin()));

    private static Validation<Error, Option<T>> Stored<T, TRaw>(ArchivableDictionary dictionary, CyclesKey key, Func<TRaw, Fin<T>> admit) where TRaw : notnull =>
        ArchivableDictionaries.Find<TRaw>(dictionary, key.Key).Bind(raw => raw.Traverse(admit).As()).ToValidation();
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class CyclesKey {
    internal static readonly NumericDomain<int, int> Counts = new(Fin.Succ, identity, (0, int.MaxValue), new() { Form = NumberForm.Field });
    internal static readonly NumericDomain<double, double> Amounts = new(Fin.Succ, identity, (0d, double.MaxValue), new() { Form = NumberForm.Field });

    public static readonly CyclesKey RenderPreset = new Row<CyclesPreset, int>(
        ValueKey.Of<CyclesPreset, int>("RenderPreset", "Presets", "Setting to control caustics.", CyclesPreset.Architecture));
    public static readonly CyclesKey Seed = new Row<int, int>(
        ValueKey.Numeric("Seed", "Seed", "Set the seed for the random number generator", 128, Counts));
    public static readonly CyclesKey Samples = new Row<int, int>(
        ValueKey.Numeric("Samples", "Samples", "Settings for controlling Cycles in a session", 1000,
            new NumericDomain<int, int>(Fin.Succ, identity, (1, int.MaxValue), new() { Form = NumberForm.Field })));
    public static readonly CyclesKey UseDocumentSamples = new Row<bool, bool>(
        ValueKey.Raw("UseDocumentSamples", "Override Production Render Quality", "If unchecked affects only viewport sample count", @default: false));
    public static readonly CyclesKey MaxBounce = new Row<int, int>(
        ValueKey.Numeric("MaxBounce", "Maximum", "Settings controlling the bounce limits for different types of rays", 32, Counts));
    public static readonly CyclesKey MaxDiffuseBounce = new Row<int, int>(
        ValueKey.Numeric("MaxDiffuseBounce", "Diffuse", "Settings controlling the bounce limits for different types of rays", 4, Counts));
    public static readonly CyclesKey MaxGlossyBounce = new Row<int, int>(
        ValueKey.Numeric("MaxGlossyBounce", "Glossy", "Settings controlling the bounce limits for different types of rays", 16, Counts));
    public static readonly CyclesKey MaxTransmissionBounce = new Row<int, int>(
        ValueKey.Numeric("MaxTransmissionBounce", "Transmission", "Settings controlling the bounce limits for different types of rays", 32, Counts));
    public static readonly CyclesKey MaxVolumeBounce = new Row<int, int>(
        ValueKey.Numeric("MaxVolumeBounce", "Volume", "Settings controlling the bounce limits for different types of rays", 32, Counts));
    public static readonly CyclesKey TransparentMaxBounce = new Row<int, int>(
        ValueKey.Numeric("TransparentMaxBounce", "Transparency", "Settings controlling the bounce limits for different types of rays", 32, Counts));
    public static readonly CyclesKey TextureBakeQuality = new Row<TextureBake, int>(
        ValueKey.Of<TextureBake, int>("TextureBakeQuality", "Texture Bake Quality", "Setting for controlling texture bake resolution", TextureBake.Low));
    public static readonly CyclesKey UseAdaptiveSampling = new Row<bool, bool>(
        ValueKey.Raw("UseAdaptiveSampling", "Adaptive sampling", "Stop sampling pixels that reach the noise threshold", @default: true));
    public static readonly CyclesKey AdaptiveThreshold = new Row<double, double>(
        ValueKey.Numeric("AdaptiveThreshold", "Adaptive threshold", "Noise level a pixel stops sampling at", 0.01d,
            new NumericDomain<double, double>(Fin.Succ, identity, (double.BitIncrement(0d), 1d), new() { Form = NumberForm.Field })));
    public static readonly CyclesKey AdaptiveMinSamples = new Row<int, int>(
        ValueKey.Numeric("AdaptiveMinSamples", "Adaptive minimum samples", "Samples before adaptive stopping, 0 derives them from the threshold",
            CyclesPreset.Architecture.AdaptiveMinSamples, Counts));
    public static readonly CyclesKey AoBounces = new Row<int, int>(
        ValueKey.Numeric("AoBounces", "AO bounces", "Bounces before ambient occlusion replaces global illumination", 0, Counts));
    public static readonly CyclesKey SampleClampDirect = new Row<double, double>(
        ValueKey.Numeric("SampleClampDirect", "Direct sample clamp", "Limit on direct light per sample, 0 off", 3d, Amounts));
    public static readonly CyclesKey SampleClampIndirect = new Row<double, double>(
        ValueKey.Numeric("SampleClampIndirect", "Indirect sample clamp", "Limit on indirect light per sample, 0 off", CyclesPreset.Architecture.SampleClampIndirect, Amounts));
    public static readonly CyclesKey FilterGlossy = new Row<double, double>(
        ValueKey.Numeric("FilterGlossy", "Filter glossy", "Blur of glossy paths that suppresses caustic fireflies, 0 off", CyclesPreset.Architecture.FilterGlossy, Amounts));
    public static readonly CyclesKey CausticsReflective = new Row<bool, bool>(
        ValueKey.Raw("CausticsReflective", "Reflective caustics", "Light paths reflected onto diffuse surfaces", @default: true));
    public static readonly CyclesKey CausticsRefractive = new Row<bool, bool>(
        ValueKey.Raw("CausticsRefractive", "Refractive caustics", "Light paths refracted onto diffuse surfaces", @default: true));
    public static readonly CyclesKey UseDirectLight = new Row<bool, bool>(
        ValueKey.Raw("UseDirectLight", "Sample all lights", "Direct light from every light at each bounce", @default: true));
    public static readonly CyclesKey UseIndirectLight = new Row<bool, bool>(
        ValueKey.Raw("UseIndirectLight", "Sample all lights indirect", "Indirect light from every light at each bounce", @default: true));
    public static readonly CyclesKey MaxPasses = new Row<bool, bool>(
        ValueKey.Raw("MaxPasses", "Show max passes", "Raytraced heads-up display shows the pass limit", @default: true));

    public static string Owner => "rhino-render";

    public abstract TResult Accept<TResult>(IValueKeyVisitor<TResult> visitor);

    internal (ValueBinding Binding, IO<Unit> Reset) Entry(DictionaryOwner owner) => Accept(new Entries(owner));

    private sealed class Entries(DictionaryOwner owner) : IValueKeyVisitor<(ValueBinding Binding, IO<Unit> Reset)> {
        public (ValueBinding Binding, IO<Unit> Reset) Visit<TValue, TRaw>(ValueKey<TValue, TRaw> key) where TValue : notnull where TRaw : notnull =>
            ArchivableDictionaries.Store(owner, key, None) switch {
                var store => (ValueBinding.Key(Owner, key, store), store.Write(None).Map(unit)),
            };
    }

    private sealed class Row<TValue, TRaw>(ValueKey<TValue, TRaw> key) : CyclesKey(key.Name) where TValue : notnull where TRaw : notnull {
        private readonly ValueKey<TValue, TRaw> key = key;

        public override TResult Accept<TResult>(IValueKeyVisitor<TResult> visitor) => visitor.Visit(key);
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
                       Bind(ValueKey.Numeric("ThrottleMs", "Throttle (in ms)", "Pause between Raytraced viewport updates", 100, CyclesKey.Counts), SettingType.Integer, Applied.Live),
                       Bind(ValueKey.Raw("SelectedDeviceStr", "Device", "Devices Rhino Render runs on, -1 picks the first GPU", "-1"), SettingType.String, Applied.Live),
                       Bind(ValueKey.Raw("IntermediateSelectedDeviceStr", "Device selection", "Device choice the device section writes with Device", "-1"), SettingType.String, Applied.Live),
                       Bind(ValueKey.Numeric("PixelSize", "Pixel size", "Raytraced pixel size, Sharpness writes 11 minus it", 1,
                           new NumericDomain<int, int>(Fin.Succ, identity, (1, 10), new() { Form = NumberForm.Field })), SettingType.Integer, Applied.Live),
                       Bind(ValueKey.Raw("StartGpuKernelCompiler", "Compile GPU kernels", "Compile GPU kernels when Rhino Render loads", @default: true), SettingType.Bool, Applied.IdleSave),
                       Bind(ValueKey.Raw("UseLightTree", "Light tree", "Sample lights through the light tree", @default: true), SettingType.Bool, Applied.Live),
                       Bind(ValueKey.Numeric("SunLightFactor", "Sun light factor", "Multiplier Rhino Render applies to the sun's intensity", 3.2d, CyclesKey.Amounts), SettingType.Double, Applied.Live),
                       Bind(ValueKey.Numeric("AreaLightFactor", "Area light factor", "Multiplier on rectangular light intensity", 17.2d, CyclesKey.Amounts), SettingType.Double, Applied.Live),
                       Bind(ValueKey.Numeric("LinearLightFactor", "Linear light factor", "Multiplier on linear light intensity", 10d, CyclesKey.Amounts), SettingType.Double, Applied.Live),
                       Bind(ValueKey.Numeric("PointLightFactor", "Point light factor", "Multiplier on point light intensity", 40d, CyclesKey.Amounts), SettingType.Double, Applied.Live),
                       Bind(ValueKey.Numeric("SpotLightFactor", "Spot light factor", "Multiplier on spot light intensity", 40d, CyclesKey.Amounts), SettingType.Double, Applied.Live))
                       + node.Bind(static settings => Callbacks.Found(settings.TryGetDefault(threads, out int factory), factory))
                           .Map(static factory => Bind(ValueKey.Raw(threads, "CPU Threads", "Threads used by CPU render sessions", factory), SettingType.Integer, Applied.Live)).ToSeq());

            static ValueBinding Bind<TValue, TRaw>(ValueKey<TValue, TRaw> key, SettingType<TRaw> kind, Applied applied)
                where TValue : notnull
                where TRaw : notnull {
                PlugInSetting<TValue, TRaw> row = new(key, kind) { Applied = applied };
                return ValueBinding.Key("rhino-render-options", row.Value, PlugInSettings.Store(Node, row));
            }
        }
    }
}
